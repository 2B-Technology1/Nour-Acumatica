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

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR.Extensions.CRCreateReturnOrder;
using PX.Objects.CR.Workflows;
using PX.Objects.SO;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using System.Linq;

namespace PX.Objects.CR.CRCaseMaint_Extensions
{
	public class CRCaseMaint_CRCreateReturnOrder : CRCreateReturnOrder<CRCaseMaint, CRCase>
	{
		public static bool IsActive() => IsExtensionActive();

		#region Events

		public virtual void _(Events.RowSelected<CRCase> e)
		{
			if (e.Row == null)
				return;
			
			bool canCreateReturnOrder = !(e.Row.Status == CaseWorkflow.States.Closed || e.Row.Status == CaseWorkflow.States.Released || e.Row.IsActive.GetValueOrDefault() == false);

			CreateReturnOrder.SetEnabled(canCreateReturnOrder);

			Customer customerRma = SelectFrom<Customer>
				.InnerJoin<SOOrder>
					.On<SOOrder.FK.Customer>
				.Where<
					Customer.bAccountID.IsEqual<@P.AsInt>
					.And<SOOrder.behavior.IsEqual<SOBehavior.rM>
						.And<SOOrder.status.IsNotEqual<SOOrderStatus.completed>>
						.And<SOOrder.status.IsNotEqual<SOOrderStatus.cancelled>>
						.And<SOOrder.status.IsNotEqual<SOOrderStatus.voided>>>>
				.View
				.ReadOnly
				.SelectSingleBound(Base, currents: null, new object[] { e.Row.CustomerID });

			if (customerRma?.AcctCD != null)
			{
				e.Cache.RaiseExceptionHandling<CRCase.customerID>(
					e.Row,
					null,
					new PXSetPropertyException(MessagesNoPrefix.CustomerHasReturnOrderWarning, PXErrorLevel.Warning));
			}
		}

		#endregion

		#region Overrides

		public override SOOrder FillSalesOrder(SOOrderEntry soGraph, CRCase document, SOOrder salesOrder)
		{
			var customer = Customer.PK.Find(Base, document.CustomerID);

			salesOrder.CuryID = customer.CuryID;

			salesOrder.OrderDate = Base.Accessinfo.BusinessDate;
			salesOrder.OrderDesc = document.Subject;
			salesOrder.TermsID = customer.TermsID;
			salesOrder.CustomerID = document.CustomerID;
			salesOrder.CustomerLocationID = document.LocationID ?? customer.DefLocationID;
			var contactID = document.ContactID;
			if (contactID == null)
			{
				if (customer.PrimaryContactID != null)
					contactID = customer.PrimaryContactID;
				else
				{
					var contactsResultSet = PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<CRCase.customerID>>, And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>.Select(Base).Select(res => res[0]);
					var contacts = contactsResultSet.ToList();
					if (contacts.Count == 1)
						contactID = ((Contact)contacts[0]).ContactID;
				}
			}
			salesOrder.ContactID = contactID;

			salesOrder =  soGraph.Document.Update(salesOrder);

			soGraph.customer.Current.CreditRule = customer.CreditRule;

			return salesOrder;
		}

		public override CRRelation FillRelations(SOOrderEntry soGraph, CRCase document, SOOrder salesOrder)
		{
			var relationExt = soGraph.GetExtension<SOOrderEntry_CRRelationDetailsExt>();

			var relation = relationExt.Relations.Insert();

			relation.RefNoteID = salesOrder.NoteID;
			relation.RefEntityType = salesOrder.GetType().FullName;
			relation.Role = CRRoleTypeList.Source;
			relation.TargetType = CRTargetEntityType.CRCase;
			relation.TargetNoteID = document.NoteID;
			relation.DocNoteID = document.NoteID;
			relation.EntityID = document.CustomerID;
			relation.ContactID = document.ContactID;

			return relationExt.Relations.Update(relation);
		}

		public override bool CheckBAccountStateBeforeConvert()
		{
			var bAccount = BAccount.PK.Find(Base, Base.CaseCurrent.Current.CustomerID);

			if (bAccount == null || bAccount.Type == BAccountType.ProspectType)
				return false;

			return true;
		}

		#endregion
	}
}
