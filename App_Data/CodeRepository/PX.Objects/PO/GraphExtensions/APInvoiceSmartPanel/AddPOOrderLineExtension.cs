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
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    /// <summary>
    /// This class implements graph extension to use special dialogs called Smart Panel to perform "ADD PO LINE" (Screen AP301000)
    /// </summary>
    [Serializable]
    public class AddPOOrderLineExtension : PXGraphExtension<LinkLineExtension, APInvoiceEntry>
    {
        #region Data Members

        [PXCopyPasteHiddenView]
        public PXSelect<POLineRS> poorderlineslist;

        public PXFilter<POOrderFilter> orderfilter;

        #endregion

        #region Initialize

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
        }

        public override void Initialize()
        {
            base.Initialize();

            poorderlineslist.Cache.AllowDelete = false;
            poorderlineslist.Cache.AllowInsert = false;
        }

        #endregion

        #region Actions

        public PXAction<APInvoice> addPOOrderLine;
        [PXUIField(DisplayName = Messages.AddPOOrderLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, FieldClass = "DISTR")]
        [PXLookupButton]
        [APMigrationModeDependentActionRestriction(
            restrictInMigrationMode: true,
            restrictForRegularDocumentInMigrationMode: true,
            restrictForUnreleasedMigratedDocumentInNormalMode: true)]
        public virtual IEnumerable AddPOOrderLine(PXAdapter adapter)
        {
            Base.checkTaxCalcMode();
            if (Base.Document.Current != null &&
                Base.Document.Current.DocType.IsIn(APDocType.Invoice, APDocType.Prepayment) &&
                Base.Document.Current.Released == false &&
                Base.Document.Current.Prebooked == false &&
                poorderlineslist.AskExt(
                    (graph, view) =>
                    {
                        orderfilter.Cache.ClearQueryCacheObsolete();
                        orderfilter.View.Clear();
                        orderfilter.Cache.Clear();

                        poorderlineslist.Cache.ClearQueryCacheObsolete();
                        poorderlineslist.View.Clear();
                        poorderlineslist.Cache.Clear();
                    }, true) == WebDialogResult.OK)
            {
                return AddPOOrderLine2(adapter);
            }
            return adapter.Get();
        }
        public PXAction<APInvoice> addPOOrderLine2;
        [PXUIField(DisplayName = Messages.AddPOOrderLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        [APMigrationModeDependentActionRestriction(
            restrictInMigrationMode: true,
            restrictForRegularDocumentInMigrationMode: true,
            restrictForUnreleasedMigratedDocumentInNormalMode: true)]
        public virtual IEnumerable AddPOOrderLine2(PXAdapter adapter)
        {
            Base.updateTaxCalcMode();
            if (ShouldAddPOOrderLine())
            {
                var selectedLines = poorderlineslist.Cache.Updated.RowCast<POLineRS>().Where(t => t.Selected == true).ToList();
				Base.ProcessPOOrderLines(selectedLines);
                poorderlineslist.Cache.Clear();
            }
            return adapter.Get();
        }

        public virtual bool ShouldAddPOOrderLine()
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

			addPOOrderLine.SetVisible(invoiceState.IsDocumentInvoice);

            PXUIFieldAttribute.SetEnabled(this.poorderlineslist.Cache, null, false);

            
            bool allowAddPOOrderLine = invoiceState.IsDocumentEditable &&
                invoiceState.AllowAddPOByProject &&
				!invoiceState.IsDocumentScheduled &&
				Base.vendor.Current != null &&
                !invoiceState.IsRetainageDebAdj;

            addPOOrderLine.SetEnabled(allowAddPOOrderLine);

			PXUIFieldAttribute.SetEnabled<POLineRS.selected>(this.poorderlineslist.Cache, null, allowAddPOOrderLine);
			PXUIFieldAttribute.SetVisible<POOrderFilter.showBilledLines>(orderfilter.Cache, null, invoiceState.IsDocumentInvoice);
			PXUIFieldAttribute.SetVisible<POLineRS.unbilledQty>(poorderlineslist.Cache, null, invoiceState.IsDocumentInvoice);
			PXUIFieldAttribute.SetVisible<POLineRS.curyUnbilledAmt>(poorderlineslist.Cache, null, invoiceState.IsDocumentInvoice);
        }

        #endregion

        #region Selecting override

        public virtual IEnumerable pOOrderLinesList()
        {
            APInvoice doc = Base.Document.Current;
			bool isInvoice = (doc.DocType == APDocType.Invoice),
                isDebitAdj = (doc.DocType == APDocType.DebitAdj), //used only for subcontracts
                isPrepayment = (doc.DocType == APDocType.Prepayment);
            var filter = orderfilter.Current;

            if (doc?.VendorID == null
                || doc.VendorLocationID == null
                || !isInvoice && !isDebitAdj && !isPrepayment)
            {
                return Enumerable.Empty<POLineRS>();
            }

            PXSelectBase<POLineRS> cmd = new PXSelectReadonly<
                POLineRS,
                Where<POLineRS.orderType, NotIn3<POOrderType.blanket, POOrderType.standardBlanket>,
                    And<POLineRS.cancelled, NotEqual<True>,
                    And<POLineRS.curyID, Equal<Current<APInvoice.curyID>>,
                    And<Where<Current<POOrderFilter.orderNbr>, IsNull, Or<POLineRS.orderNbr, Equal<Current<POOrderFilter.orderNbr>>>>>>>>>(Base);

            if (!isDebitAdj)
            {
                cmd.WhereAnd<Where<POLineRS.closed, NotEqual<True>>>();
            }

            if (!isDebitAdj)
            {
                cmd.WhereAnd<Where<POLineRS.status, In3<POOrderStatus.awaitingLink, POOrderStatus.open, POOrderStatus.completed>>>();
            }
            else
            {
                cmd.WhereAnd<Where<POLineRS.status, In3<POOrderStatus.open, POOrderStatus.completed, POOrderStatus.closed>>>();
            }

            if (isInvoice || isDebitAdj)
			{
				cmd.WhereAnd<Where<POLineRS.pOAccrualType, Equal<POAccrualType.order>>>();
			}
			else if (isPrepayment)
			{
				cmd.WhereAnd<Where<POLineRS.curyReqPrepaidAmt, Less<POLineRS.curyExtCost>>>();
				cmd.WhereAnd<Where<POLineRS.taxZoneID, Equal<Current<APInvoice.taxZoneID>>, Or<POLineRS.taxZoneID, IsNull, And<Current<APInvoice.taxZoneID>, IsNull>>>>();
			}

            if (Base.APSetup.Current.RequireSingleProjectPerDocument == true)
            {
				cmd.WhereAnd<Where<POLineRS.projectID, Equal<Current<APInvoice.projectID>>>>();
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
            {
                cmd.WhereAnd<Where<POLineRS.vendorID, Equal<Current<APInvoice.suppliedByVendorID>>,
                    And<POLineRS.vendorLocationID, Equal<Current<APInvoice.suppliedByVendorLocationID>>,
                    And<POLineRS.payToVendorID, Equal<Current<APInvoice.vendorID>>>>>>();
            }
            else
            {
                cmd.WhereAnd<Where<POLineRS.vendorID, Equal<Current<APInvoice.vendorID>>,
                    And<POLineRS.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>>>>();
            }

            if (filter.ShowBilledLines != true)
            {
                cmd.WhereAnd<Where<POLineRS.unbilledQty, Greater<decimal0>,
                    Or<POLineRS.curyUnbilledAmt, Greater<decimal0>>>>();
            }

			var usedPOAccrual = new Lazy<POAccrualSet>(() => Base.GetUsedPOAccrualSet());
			return cmd.View.SelectMultiBound(new object[] { doc, filter })
				.RowCast<POLineRS>().AsEnumerable()
				.Where(t => !usedPOAccrual.Value.Contains(t))
				.ToList();
        }

        #endregion

        #endregion
    }
}
