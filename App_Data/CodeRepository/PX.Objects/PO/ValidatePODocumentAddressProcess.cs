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
using PX.Objects.CR;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.PO
{
	public class ValidatePODocumentAddressProcess : ValidateDocumentAddressGraph<ValidatePODocumentAddressProcess>
	{
		#region Event Handlers
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[DocumentTypeField.List(DocumentTypeField.SetOfValues.PORelatedDocumentTypes)]
		public virtual void _(Events.CacheAttached<ValidateDocumentAddressFilter.documentType> e) { }
		#endregion

		#region Method Overrides
		protected override IEnumerable GetAddressRecords(ValidateDocumentAddressFilter filter)
		{
			IEnumerable result = new List<UnvalidatedAddress>();
			if (filter.DocumentType.Trim().Equals(DocumentTypeField.PurchaseOrder))
			{
				result = AddPurchaseOrderAddresses(filter);
			}
			return result;
		}
		#endregion

		#region Protected Methods
		protected virtual IEnumerable AddPurchaseOrderAddresses(ValidateDocumentAddressFilter filter)
		{
			List<Object> poRemitAddresses = new List<Object>();
			List<Object> poShipAddresses = new List<Object>();

			// For Purchase Order, Validate Address is available in the workflow differently depending upon the order type.
			object[] regularOrderStatusList = new string[] { POOrderStatus.Hold, POOrderStatus.PendingPrint, POOrderStatus.PendingEmail, POOrderStatus.PendingApproval,
				POOrderStatus.Rejected, POOrderStatus .Open, POOrderStatus .Completed, POOrderStatus.Cancelled, POOrderStatus.Closed };
			object[] dropShipStatusList = new string[] { POOrderStatus.Hold, POOrderStatus.PendingPrint, POOrderStatus .PendingEmail, POOrderStatus.PendingApproval,
				POOrderStatus.Rejected, POOrderStatus .AwaitingLink, POOrderStatus .Open, POOrderStatus.Cancelled, POOrderStatus.Closed };
			object[] projDropShpStatusList = new string[] { POOrderStatus.Hold, POOrderStatus .PendingPrint, POOrderStatus .PendingEmail, POOrderStatus.PendingApproval,
				POOrderStatus.Rejected, POOrderStatus.Open, POOrderStatus .Completed, POOrderStatus.Cancelled, POOrderStatus.Closed };
			object[] standardBlanketStatusList = new string[] { POOrderStatus.Hold, POOrderStatus.PendingApproval, POOrderStatus.Rejected, POOrderStatus.Open, POOrderStatus.Cancelled };
			object[] blanketStatusList = new string[] { POOrderStatus.Hold, POOrderStatus.PendingApproval, POOrderStatus.Rejected, POOrderStatus.Completed,
				POOrderStatus.Cancelled, POOrderStatus.Closed };

			Dictionary<string, object[]> statusForOrderTypes = new Dictionary<string, object[]>
			{
				{ POOrderType.RegularOrder, regularOrderStatusList },
				{ POOrderType.DropShip, dropShipStatusList },
				{ POOrderType.ProjectDropShip, projDropShpStatusList },
				{ POOrderType.StandardBlanket, standardBlanketStatusList },
				{ POOrderType.Blanket, blanketStatusList }
			};

			PXSelectBase<POOrder> RemitAddrCmd = new PXSelectJoin<POOrder,
					InnerJoin<PORemitAddress, On<POOrder.remitAddressID, Equal<PORemitAddress.addressID>>>,
					Where<PORemitAddress.isDefaultAddress, Equal<False>,
					And<PORemitAddress.isValidated, Equal<False>,
					And<POOrder.cancelled, Equal<False>,
					And<POOrder.orderType, Equal<Required<POOrder.orderType>>,
					And<POOrder.status, In<Required<POOrder.status>>>>>>>>(this);

			PXSelectBase<POOrder> shipAddrCmd = new PXSelectJoin<POOrder,
				InnerJoin<POShipAddress, On<POOrder.shipAddressID, Equal<POShipAddress.addressID>>>,
				Where<POShipAddress.isDefaultAddress, Equal<False>,
				And<POShipAddress.isValidated, Equal<False>,
				And<POOrder.cancelled, Equal<False>,
				And<POOrder.orderType, Equal<Required<POOrder.orderType>>,
				And<POOrder.status, In<Required<POOrder.status>>>>>>>>(this);

			if (!string.IsNullOrEmpty(filter?.Country))
			{
				RemitAddrCmd.WhereAnd<Where<PORemitAddress.countryID, Equal<Required<PORemitAddress.countryID>>>>();
				shipAddrCmd.WhereAnd<Where<POShipAddress.countryID, Equal<Required<POShipAddress.countryID>>>>();
			}

			foreach (var val in statusForOrderTypes)
			{
				object[] parms = string.IsNullOrEmpty(filter?.Country) ? new object[] { val.Key, (object)val.Value } : new object[] { val.Key, (object)val.Value, filter.Country };
				poRemitAddresses.AddRange(RemitAddrCmd.View.SelectMulti(parms));
				poShipAddresses.AddRange(shipAddrCmd.View.SelectMulti(parms));
			}

			foreach (PXResult<POOrder, PORemitAddress> item in poRemitAddresses)
			{
				var poRemitAddr = (PORemitAddress)item;
				var poOrder = (POOrder)item;
				string orderType = new POOrderType.ListAttribute().ValueLabelDic[poOrder.OrderType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(poRemitAddr, poOrder,
					documentNbr: string.IsNullOrEmpty(orderType) ? poOrder.OrderNbr : string.Format("{0}, {1}", orderType, poOrder.OrderNbr),
					documentType: CR.MessagesNoPrefix.PurchaseOrderDesc,
					status: new POOrderStatus.ListAttribute().ValueLabelDic[poOrder.Status]);

				yield return DocumentAddresses.Insert(address);
			}

			foreach (PXResult<POOrder, POShipAddress> item in poShipAddresses)
			{
				var poShipAddr = (POShipAddress)item;
				var poOrder = (POOrder)item;
				string orderType = new POOrderType.ListAttribute().ValueLabelDic[poOrder.OrderType];

				UnvalidatedAddress address = ConvertToUnvalidatedAddress(poShipAddr, poOrder,
					documentNbr: string.IsNullOrEmpty(orderType) ? poOrder.OrderNbr : string.Format("{0}, {1}", orderType, poOrder.OrderNbr),
					documentType: CR.MessagesNoPrefix.PurchaseOrderDesc,
					status: new POOrderStatus.ListAttribute().ValueLabelDic[poOrder.Status]);

				yield return DocumentAddresses.Insert(address);
			}
		}
		#endregion
	}
}
