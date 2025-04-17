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
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	public class CRRelationAccount : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXFieldDefaultingSubscriber
	{
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CRRelation row = e.Row as CRRelation;
			if (row == null)
				return;

			((PXFieldState) e.ReturnState).Enabled = 
				row.Role != CRRoleTypeList.RelatedEntity 
				&& row.Role != CRRoleTypeList.Source
				&& row.Role != CRRoleTypeList.Derivative
				&& row.Role != CRRoleTypeList.Child
				&& row.Role != CRRoleTypeList.Parent;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			int? val = null;
			CRRelation row = (CRRelation)e.Row;

			if (row == null 
				|| row.Role != CRRoleTypeList.RelatedEntity 
				&& (row.Role != CRRoleTypeList.Source
					&& row.Role != CRRoleTypeList.Derivative
					&& row.Role != CRRoleTypeList.Child
					&& row.Role != CRRoleTypeList.Parent)
				|| row.TargetNoteID == null) // only "RE"
				return;

			var helper = new EntityHelper(sender.Graph);
			Type cachetype = System.Web.Compilation.PXBuildManager.GetType(row.TargetType, false);
			var obj = helper.GetEntityRow(cachetype, row.TargetNoteID, false);

			if (obj == null)
				return;

			switch (row.TargetType)
			{
				case CRTargetEntityType.APInvoice:
				{
					var tmp = (PX.Objects.AP.APInvoice)obj;
					val = tmp.VendorID;
				}
					break;

				case CRTargetEntityType.ARInvoice:
				{
					var tmp = (PX.Objects.AR.ARInvoice)obj;
					val = tmp.CustomerID;
				}
					break;

				case CRTargetEntityType.CROpportunity:
				{
					var tmp = (PX.Objects.CR.CROpportunity)obj;
					val = tmp.BAccountID;
				}
					break;

				case CRTargetEntityType.CRQuote:
				{
					var tmp = (PX.Objects.CR.CRQuote)obj;
					val = tmp.BAccountID;
				}
					break;

				case CRTargetEntityType.POOrder:
				{
					var tmp = (PX.Objects.PO.POOrder)obj;
					val = tmp.VendorID;
				}
					break;

				case CRTargetEntityType.SOOrder:
				{
					var tmp = (PX.Objects.SO.SOOrder)obj;
					val = tmp.CustomerID;
				}
					break;

				case CRTargetEntityType.Customer:
				case CRTargetEntityType.Vendor:
				case CRTargetEntityType.BAccount: //Prospect
				{
					BAccount tmp = (BAccount)obj;
					val = tmp.BAccountID;
				}
					break;

				case CRTargetEntityType.EPExpenseClaimDetails:
				{
					EPExpenseClaimDetails tmp = (PX.Objects.EP.EPExpenseClaimDetails)obj;
					val = tmp.CustomerID;
				}
					break;

				case CRTargetEntityType.CRCase:
				{
					CRCase tmp = (PX.Objects.CR.CRCase)obj;
					val = tmp.CustomerID;
				}
					break;

				case CRTargetEntityType.Lead:
				case CRTargetEntityType.Contact:
				{
					Contact tmp = (PX.Objects.CR.Contact)obj;
					val = tmp.BAccountID;
				}
					break;

				case CRTargetEntityType.Employee:
				case CRTargetEntityType.CRCampaign:
				default:
					break;
			}

			e.NewValue = val;
		}

	}
}
