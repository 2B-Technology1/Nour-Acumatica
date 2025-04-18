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

using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    /// <summary>
    /// This class implements graph extension to use special dialogs called Smart Panel to perform "ADD PO" (Screen AP301000)
    /// </summary>
    [Serializable]
	public class AddPOOrderExtension : PXGraphExtension<LinkLineExtension, APInvoiceEntry>
	{
		#region Data Members

        [PXCopyPasteHiddenView]
        public PXSelect<POOrderRS> poorderslist;

        #endregion

        #region Initialize

        public static bool IsActive()
        {
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		public override void Initialize()
		{
			base.Initialize();

			poorderslist.Cache.AllowDelete = false;
            poorderslist.Cache.AllowInsert = false;
		}

        #endregion

        #region Actions

        public PXAction<APInvoice> addPOOrder;
        public PXAction<APInvoice> addPOOrder2;

        [PXUIField(DisplayName = Messages.AddPOOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, FieldClass = "DISTR")]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddPOOrder(PXAdapter adapter)
		{
			Base.checkTaxCalcMode();
			if (Base.Document.Current != null &&
				Base.Document.Current.DocType.IsIn(APDocType.Invoice, APDocType.Prepayment) &&
				Base.Document.Current.Released == false &&
				Base.Document.Current.Prebooked == false &&
                poorderslist.AskExt(
					(graph, view) =>
					{
                        Base1.filter.Cache.ClearQueryCacheObsolete();
                        Base1.filter.View.Clear();
                        Base1.filter.Cache.Clear();

                        poorderslist.Cache.ClearQueryCacheObsolete();
                        poorderslist.View.Clear();
                        poorderslist.Cache.Clear();
					}, true) == WebDialogResult.OK)
			{
				Base.updateTaxCalcMode();
				return AddPOOrder2(adapter);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.AddPOOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddPOOrder2(PXAdapter adapter)
		{
			if (ShouldAddPOOrder())
			{
				using (new APInvoiceEntry.SkipUpdAdjustments(Base.Document.Current.DocType + Base.Document.Current.RefNbr))
				{
				List<POOrder> orders = poorderslist.Cache.Updated.RowCast<POOrder>().Where(rc => rc.Selected == true).ToList();
				foreach (POOrder rc in orders)
				{
					Base.InvoicePOOrder(rc, false);
				}
				Base.AttachPrepayment(orders);
			}
			}
			return adapter.Get();
		}

		public virtual bool ShouldAddPOOrder()
		{
			bool result = Base.Document.Current != null &&
				Base.Document.Current.DocType == APDocType.Invoice &&
				Base.Document.Current.Released == false &&
				Base.Document.Current.Prebooked == false;
			return result;
		}

        #endregion

        #region Events

        #region APInvoice Events

        protected virtual void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            APInvoice document = e.Row as APInvoice;
            if (document == null) return;

            var invoiceState = Base.GetDocumentState(cache, document);

            addPOOrder.SetVisible(invoiceState.IsDocumentInvoice);

            PXUIFieldAttribute.SetEnabled(poorderslist.Cache, null, false);

            bool allowAddPOOrder = invoiceState.IsDocumentEditable &&
                invoiceState.AllowAddPOByProject &&
				!invoiceState.IsDocumentScheduled &&
                Base.vendor.Current != null &&
                !invoiceState.IsRetainageDebAdj;

            addPOOrder.SetEnabled(allowAddPOOrder);
			PXUIFieldAttribute.SetEnabled<POOrderRS.selected>(poorderslist.Cache, null, allowAddPOOrder);
			PXUIFieldAttribute.SetVisible<POOrderRS.unbilledOrderQty>(poorderslist.Cache, null, invoiceState.IsDocumentInvoice);
			PXUIFieldAttribute.SetVisible<POOrderRS.curyUnbilledOrderTotal>(poorderslist.Cache, null, invoiceState.IsDocumentInvoice);
        }

        #endregion

        #region Selecting override

        public virtual IEnumerable pOOrderslist()
		{
			foreach(POOrderRS order in GetPOOrderList())
			{
				yield return order;
			}
		}

		public virtual IEnumerable<PXResult<POOrderRS, POLine>> GetPOOrderList()
		{
			APInvoice doc = Base.Document.Current;
			bool isInvoice = doc.DocType == APDocType.Invoice;
			bool isDebitAdj = doc.DocType == APDocType.DebitAdj; //used only for subcontracts
			bool isPrepayment = doc.DocType == APDocType.Prepayment;

			if (doc?.VendorID == null
				|| doc.VendorLocationID == null
				|| !isInvoice && !isDebitAdj && !isPrepayment)
			{
				yield break;
			}

			var usedOrderLines = new Dictionary<APTran, int>(new POOrderComparer());
			foreach (APTran aPTran in Base.Transactions.Select().RowCast<APTran>().AsEnumerable()
				.Where(t => !string.IsNullOrEmpty(t.PONbr) && (isPrepayment || t.POAccrualType == POAccrualType.Order)))
			{
				usedOrderLines.TryGetValue(aPTran, out int count);
				usedOrderLines[aPTran] = count + 1;
			}

			PXSelectBase<POOrderRS> cmd = new PXSelectJoinGroupBy<
				POOrderRS,
				InnerJoin<POLine, On<POLine.orderType, Equal<POOrderRS.orderType>,
					And<POLine.orderNbr, Equal<POOrderRS.orderNbr>>>>,
				Where<POOrderRS.orderType, NotIn3<POOrderType.blanket, POOrderType.standardBlanket>,
					And<POOrderRS.curyID, Equal<Current<APInvoice.curyID>>,
					And<POLine.cancelled, NotEqual<True>>>>,
				Aggregate
					<GroupBy<POOrderRS.orderType,
					GroupBy<POOrderRS.orderNbr,
					GroupBy<POOrderRS.orderDate,
					GroupBy<POOrderRS.curyID,
					GroupBy<POOrderRS.curyOrderTotal,
					GroupBy<POOrderRS.hold,
					GroupBy<POOrderRS.cancelled,
					Sum<POLine.orderQty,
					Sum<POLine.curyExtCost,
					Sum<POLine.extCost,
					Count<POLine.lineNbr>>>>>>>>>>>>>(Base);

			if (!isDebitAdj)
			{
				cmd.WhereAnd<Where<POLine.closed, NotEqual<True>>>();
			}

			if (!isDebitAdj)
			{
				cmd.WhereAnd<Where<POOrderRS.status, In3<POOrderStatus.awaitingLink, POOrderStatus.open, POOrderStatus.completed>>>();
			}
			else
			{
				cmd.WhereAnd<Where<POOrderRS.status, In3<POOrderStatus.open, POOrderStatus.completed, POOrderStatus.closed>>>();
			}

			if (isInvoice || isDebitAdj)
			{
				cmd.WhereAnd<Where<POLine.pOAccrualType, Equal<POAccrualType.order>>>();
			}
			else if (isPrepayment)
			{
				cmd.WhereAnd<Where<POOrderRS.taxZoneID, Equal<Current<APInvoice.taxZoneID>>, Or<POOrderRS.taxZoneID, IsNull, And<Current<APInvoice.taxZoneID>, IsNull>>>>();
			}

			if (Base.APSetup.Current.RequireSingleProjectPerDocument == true)
			{
				cmd.WhereAnd<Where<POOrderRS.projectID, Equal<Current<APInvoice.projectID>>>>();
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
			{
				cmd.WhereAnd<Where<POOrderRS.vendorID, Equal<Current<APInvoice.suppliedByVendorID>>,
					And<POOrderRS.vendorLocationID, Equal<Current<APInvoice.suppliedByVendorLocationID>>,
					And<POOrderRS.payToVendorID, Equal<Current<APInvoice.vendorID>>>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<POOrderRS.vendorID, Equal<Current<APInvoice.vendorID>>,
					And<POOrderRS.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>>>>();
			}

			foreach (PXResult<POOrderRS, POLine> result in cmd.View.SelectMultiBound(new object[] { doc }))
			{
				POOrderRS order = result;
				APTran aPTran = new APTran
				{
					PONbr = order.OrderNbr,
					POOrderType = order.OrderType
				};
				usedOrderLines.TryGetValue(aPTran, out int count);
				if (count < result.RowCount)
				{
					yield return result;
				}
			}
		}

		#endregion

		#region POOrderRS

		public virtual void POOrderRS_CuryID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			POOrderRS row = (POOrderRS)e.Row;
			APInvoice doc = Base.Document.Current;
			if (row != null && doc != null)
			{
				if (row.CuryID != doc.CuryID)
				{
					string fieldName = typeof(POOrderRS.curyID).Name;
					PXErrorLevel msgLevel = PXErrorLevel.RowWarning;
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
					 null, null, AP.Messages.APDocumentCurrencyDiffersFromSourceDocument, msgLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
					e.IsAltered = true;
				}
			}
		}

		#endregion

		#region POLineS Events

		public virtual void POLineS_Selected_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POLineS row = (POLineS)e.Row;
			if (row != null && !(bool)e.OldValue && (bool)row.Selected)
			{
				foreach (POLineS item in sender.Updated)
				{
					if (item.Selected == true && item != row)
					{
						sender.SetValue<POLineS.selected>(item, false);

                        Base1.linkLineOrderTran.View.RequestRefresh();
					}

				}

				foreach (POReceiptLineS item in Base1.linkLineReceiptTran.Cache.Updated)
				{
					if (item.Selected == true)
					{
						Base1.linkLineReceiptTran.Cache.SetValue<POReceiptLineS.selected>(item, false);
                        Base1.linkLineReceiptTran.View.RequestRefresh();
					}
				}

			}
		}

		public virtual void POLineS_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.IsDirty = false;
		}

		public virtual void POLineS_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		#endregion

	}
}
