/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.SO;

namespace PX.Objects.CR
{
	public class CRRelationContactSelectorAttribute : PXCustomSelectorAttribute, IPXFieldDefaultingSubscriber
	{
		public CRRelationContactSelectorAttribute()
			: base(typeof(Search<Contact.contactID>))
		{
			Filterable = true;
			DescriptionField = typeof(Contact.displayName);
			DirtyRead = true;
		}

		public IEnumerable GetRecords()
		{
			BqlCommand select = _Select;

			PXCache sender = PXView.CurrentGraph.Caches[typeof(CRRelation)];

			CRRelation row = null;

			if (PXView.Currents != null)
			{
				foreach (var rec in PXView.Currents)
				{
					row = rec as CRRelation;
					if (row != null) break;
				}
			}

			if (row == null)
				row = sender.Current as CRRelation;

			if (row == null)
				return null;

			select = BqlCommand.AppendJoin<LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>>(select);
			select = select.WhereAnd<Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>();

			if (CRRelationTypeListAttribure.GetTargetID<CRRelation.targetType>(sender, row, row.Role)
				.IsIn(new[]
				{
					(int)CRRelationTypeListAttribure.TypeEntityList.All,
					(int)CRRelationTypeListAttribure.TypeEntityList.BAccount 
				}))
			{
				if (row.EntityID != null && row.TargetType != CRTargetEntityType.Employee)
					select = select.WhereAnd<Where<Contact.bAccountID, Equal<Current<CRRelation.entityID>>>>();
			}
			else if (row.EntityID != null)
				select = select.WhereAnd<Where<Contact.bAccountID, Equal<Current<CRRelation.entityID>>, And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>();
			else if (row.Role == CRRoleTypeList.Supervisor || row.Role == CRRoleTypeList.TechnicalExpert || row.Role == CRRoleTypeList.SupportEngineer)
				select = select.WhereAnd<Where<Contact.contactType, Equal<ContactTypesAttribute.employee>>>();
			else
				select = select.WhereAnd<Where<Contact.contactType, Equal<ContactTypesAttribute.person>>>();

			if (select != null)
			{
				PXView view = _Graph.TypedViews.GetView(select, false);

				var startRow = PXView.StartRow;
				var totalRows = 0;
				var res = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
				PXView.StartRow = 0;

				return res;
			}

			return null;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			CRRelation row = e.Row as CRRelation;
			if (row == null)
				return;

			int target = CRRelationTypeListAttribure.GetTargetID<CRRelation.targetType>(sender, row, row.Role);

			bool isDisabled = target == (int)CRRelationTypeListAttribure.TypeEntityList.BAccount
							|| (target == (int)CRRelationTypeListAttribure.TypeEntityList.All
								&& row.TargetType != typeof(AR.Customer).FullName
								&& row.TargetType != typeof(Vendor).FullName
								&& row.TargetType != typeof(BAccount).FullName
								&& row.TargetType != typeof(PO.POOrder).FullName
								&& row.TargetType != typeof(SO.SOOrder).FullName);

			var state = e.ReturnState as PXFieldState;

			if (state != null)
				state.Enabled = !isDisabled;
		}


		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CRRelation row = (CRRelation)e.Row;

			if (row == null)
				return;

			int? val = null;
			object item = null;

			if (row.TargetNoteID != null)
			{
				var helper = new EntityHelper(sender.Graph);
				Type cachetype = System.Web.Compilation.PXBuildManager.GetType(row.TargetType, false);
				item = helper.GetEntityRow(cachetype, row.TargetNoteID, false);
			}

			switch (row.TargetType)
			{
				case CRTargetEntityType.CROpportunity:
				{
					CROpportunity obj = (CROpportunity)item;
					val = obj?.ContactID;
				}
					break;

				case CRTargetEntityType.CRQuote:
				{
					CRQuote obj = (CRQuote)item;
					val = obj?.ContactID;
				}
					break;

				case CRTargetEntityType.BAccount:
				case CRTargetEntityType.Customer:
				case CRTargetEntityType.Vendor:
				case CRTargetEntityType.Employee:
				{
					BAccount tmp = item as BAccount ?? PXSelectorAttribute.Select<CRRelation.entityID>(sender, row) as BAccount;
					val = tmp?.DefContactID;
				}
					break;

				case CRTargetEntityType.Contact:
				{
					Contact obj = (Contact)item;
					val = obj?.ContactID;
				}
					break;

				case CRTargetEntityType.Lead:
				{
					CRLead obj = (CRLead)item;
					val = obj?.RefContactID;
				}
					break;

				case CRTargetEntityType.CRCase:
				{
					CRCase obj = (CRCase)item;
					val = obj?.ContactID;
				}
					break;

				case CRTargetEntityType.SOOrder:
				{
					var obj = (SOOrder)item;
					val = obj?.ContactID;
				}
					break;

				case CRTargetEntityType.APInvoice:
				case CRTargetEntityType.ARInvoice:
				case CRTargetEntityType.CRCampaign:
				case CRTargetEntityType.EPExpenseClaimDetails:
				case CRTargetEntityType.POOrder:
				default:
					break;
			}

			if (val != null)
				e.NewValue = val;
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null || !ValidateValue)
				return;

			if (sender.Keys.Count == 0 || _FieldName != sender.Keys[sender.Keys.Count - 1])
			{
				var view = GetViewWithParameters(sender, e.NewValue);
				object item = null;
				try
				{
					item = view.SelectSingleBound(e.Row);
				}
				catch (FormatException) { } // thrown by SqlServer
				catch (InvalidCastException) { } // thrown by MySql

				if (item == null)
				{
					throwNoItem(hasRestrictedAccess(sender, _PrimarySimpleSelect, e.Row), e.ExternalCall, e.NewValue);
				}
			}
		}

	}
}
