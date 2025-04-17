using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects;
using PX.Objects.CA;
using PX.Objects.TX;
using PX.Objects.AR;


namespace Maintenance
{

    public class CATranEntry_Extension : PXGraphExtension<CATranEntry>
    {

        protected virtual void CAAdj_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CAAdj trn = e.Row as CAAdj;

            //if (trn.Status != "R")
            //{
            //    Reverse.SetEnabled(false);
            //}

            PXUIFieldAttribute.SetEnabled<CAAdjExt.usrRefNbr>(sender, trn, true);
            PXUIFieldAttribute.SetEnabled<CAAdjExt.usrDueDate>(sender, trn, true);
            PXUIFieldAttribute.SetEnabled<CAAdjExt.usrCheckNbr>(sender, trn, true);
            PXUIFieldAttribute.SetEnabled<CAAdjExt.usrNRCheckNo>(sender, trn, true);
            PXUIFieldAttribute.SetEnabled<CAAdjExt.usrBranchName>(sender, trn, true);
            PXUIFieldAttribute.SetEnabled<CAAdjExt.usrBankName>(sender, trn, true);

        }

        protected void CASplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            try
            {
                CATranEntry ct = PXGraph.CreateInstance<CATranEntry>();
                var row = (CASplit)e.Row;
                Sub subAccount = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(ct, row.SubID);
                if (subAccount != null)
                {
                    string segmentOne = subAccount.SubCD.Substring(0, 2);
                    SegmentValue segmentValue = PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>,
                                                                      And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>,
                                                                      And<SegmentValue.value, Equal<Required<SegmentValue.value>>>>>>.Select(ct, "SUBACCOUNT", 1, segmentOne);
                    if (segmentValue != null)
                        row.GetExtension<CASplitEx>().UsrSubDescr = segmentValue.Descr;
                    decimal y = 0;
                    int x = decimal.ToInt32(y);

                    string segmentTwo = subAccount.SubCD.Substring(2, 4);
                    segmentValue = PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>,
                                                                      And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>,
                                                                      And<SegmentValue.value, Equal<Required<SegmentValue.value>>>>>>.Select(ct, "SUBACCOUNT", 2, segmentTwo);
                    if (segmentValue != null)
                        row.GetExtension<CASplitEx>().UsrSubDescr += "-" + segmentValue.Descr;

                    string segmentThree = subAccount.SubCD.Substring(6, 2);
                    segmentValue = PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>,
                                                                      And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>,
                                                                      And<SegmentValue.value, Equal<Required<SegmentValue.value>>>>>>.Select(ct, "SUBACCOUNT", 3, segmentThree);
                    if (segmentValue != null)
                        row.GetExtension<CASplitEx>().UsrSubDescr += "-" + segmentValue.Descr;

                    string segmentFour = subAccount.SubCD.Substring(8, 4);
                    segmentValue = PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>,
                                                                      And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>,
                                                                      And<SegmentValue.value, Equal<Required<SegmentValue.value>>>>>>.Select(ct, "SUBACCOUNT", 4, segmentFour);
                    if (segmentValue != null)
                        row.GetExtension<CASplitEx>().UsrSubDescr += "-" + segmentValue.Descr;

                    string segmentFive = subAccount.SubCD.Substring(12, 6);
                    segmentValue = PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>,
                                                                      And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>,
                                                                      And<SegmentValue.value, Equal<Required<SegmentValue.value>>>>>>.Select(ct, "SUBACCOUNT", 5, segmentFive);
                    if (segmentValue != null)
                        row.GetExtension<CASplitEx>().UsrSubDescr += "-" + segmentValue.Descr;

                }
            }
            catch { }
        }

        protected virtual void CASplit_UsrVendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            try
            {
                CASplit split = e.Row as CASplit;



                CATranEntry catran = PXGraph.CreateInstance<CATranEntry>();



                if (split != null && split.InventoryID != null)
                {

                    Vendor vndr = PXSelect<Vendor,

                        Where<Vendor.bAccountID, Equal<Required<CASplitEx.usrVendorID>>>>.

                        Select(catran, split.GetExtension<CASplitEx>().UsrVendorID);



                    if (vndr != null)
                    {

                        //split.GetExtension<CASplitEx>().UsrVendorName = vndr.AcctName;

                        //split.GetExtension<CASplitEx>().UsrType = vndr.Type;

                        split.GetExtension<CASplitEx>().UsrCommercialRecord = vndr.GetExtension<BAccountExt>().UsrVCommercialRecord;

                        //split.GetExtension<CASplitEx>().UsrTaxType = vndr.GetExtension<BAccountExt>().usrTaxType;

                        split.GetExtension<CASplitEx>().UsrRegistrationNo = vndr.GetExtension<BAccountExt>().UsrVRegistrationNo;

                        split.GetExtension<CASplitEx>().UsrTaxesID = vndr.GetExtension<BAccountExt>().UsrTaxesID;

                        split.GetExtension<CASplitEx>().UsrTaxFile = vndr.GetExtension<BAccountExt>().UsrVTaxFile;

                        split.GetExtension<CASplitEx>().UsrTaxNo = vndr.GetExtension<BAccountExt>().UsrVTaxNo;

                    }



                    sender.SetDefaultExt<CASplit.taxCategoryID>(split);

                    sender.SetDefaultExt<CASplit.uOM>(split);

                }
            }
            catch { }

        }
        #region Selects
        [PXViewName(PX.Objects.CA.Messages.CashTransactions)]
        public PXSelect<CAAdj, Where<CAAdj.draft, Equal<False>>> CAAdjRecords;
        public PXSelectJoin<CATaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<CATaxTran.taxID>>>,
            Where<CATaxTran.module, Equal<BatchModule.moduleCA>, And<CATaxTran.tranType, Equal<Current<CAAdj.adjTranType>>,
                And<CATaxTran.refNbr, Equal<Current<CAAdj.adjRefNbr>>>>>> Taxes;
        [PXImport(typeof(CAAdj))]
        public PXSelect<CASplit, Where<CASplit.adjRefNbr, Equal<Current<CAAdj.adjRefNbr>>,
                                                             And<CASplit.adjTranType, Equal<Current<CAAdj.adjTranType>>>>> CASplitRecords;

        #endregion

        #region Event Handlers

        protected bool reversingContext;

        //[PXOverride]
        //public PXAction<CAAdj> Reverse;
        [PXUIField(DisplayName = "Reverse Transaction", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXButton]
        protected virtual IEnumerable reverse(PXAdapter adapter)
        {
            CATranEntry caadjGraph = PXGraph.CreateInstance<CATranEntry>();
            CAAdj current = CAAdjRecords.Current;
            current.GetExtension<CAAdjExt>().UsrReversed = true;
            caadjGraph.Actions.PressSave();

            if (current.Released != true)
                return adapter.Get();

            CAAdj adj = (CAAdj)CAAdjRecords.Cache.CreateCopy(CAAdjRecords.Current);
            adj.GetExtension<CAAdjExt>().UsrCheckNbr = null; //needs CAAdjExt to be implemented only in the DLL
            adj.AdjRefNbr = null;
            adj.Status = null;
            adj.Approved = null;
            adj.Hold = null;
            adj.Released = null;
            adj.ClearDate = null;
            adj.Cleared = null;
            adj.TranID = null;
            adj.NoteID = null;
            adj.CurySplitTotal = null;
            adj.CuryVatExemptTotal = null;
            adj.CuryVatTaxableTotal = null;
            adj.SplitTotal = null;
            adj.VatExemptTotal = null;
            adj.VatTaxableTotal = null;
            adj.CuryTaxRoundDiff *= -1;
            adj.CuryControlAmt *= -1;
            adj.CuryTaxAmt *= -1;
            adj.CuryTaxTotal *= -1;
            adj.CuryTranAmt *= -1;
            adj.TaxRoundDiff *= -1;
            adj.TaxAmt *= -1;
            adj.ControlAmt *= -1;
            adj.TaxTotal *= -1;
            adj.TranAmt *= -1;
            adj.EmployeeID = null;
            adj.GetExtension<CAAdjExt>().UsrReversed = true;

            List<Tuple<CASplit, CASplit>> splits = new List<Tuple<CASplit, CASplit>>();
            foreach (CASplit split in CASplitRecords.Select())
            {
                CASplit newSplit = (CASplit)CASplitRecords.Cache.CreateCopy(split);
                newSplit.AdjRefNbr = null;
                newSplit.NoteID = null;
                newSplit.CuryTranAmt *= -1;
                newSplit.CuryUnitPrice *= -1;
                newSplit.CuryTaxAmt *= -1;
                newSplit.CuryTaxableAmt *= -1;
                newSplit.TranAmt *= -1;
                newSplit.UnitPrice *= -1;
                newSplit.TaxAmt *= -1;
                newSplit.TaxableAmt *= -1;
                splits.Add(new Tuple<CASplit, CASplit>(split, newSplit));
            }
            List<CATaxTran> taxes = new List<CATaxTran>();
            foreach (CATaxTran taxTran in Taxes.Select())
            {
                CATaxTran newTaxTran = new CATaxTran();
                newTaxTran.AccountID = taxTran.AccountID;
                newTaxTran.BranchID = taxTran.BranchID;
                newTaxTran.FinPeriodID = taxTran.FinPeriodID;
                newTaxTran.SubID = taxTran.SubID;
                newTaxTran.TaxBucketID = taxTran.TaxBucketID;
                newTaxTran.TaxID = taxTran.TaxID;
                newTaxTran.TaxType = taxTran.TaxType;
                newTaxTran.TaxZoneID = taxTran.TaxZoneID;
                newTaxTran.TranDate = taxTran.TranDate;
                newTaxTran.VendorID = taxTran.VendorID;
                newTaxTran.CuryID = taxTran.CuryID;
                newTaxTran.Description = taxTran.Description;
                newTaxTran.NonDeductibleTaxRate = taxTran.NonDeductibleTaxRate;
                newTaxTran.TaxRate = taxTran.TaxRate;
                newTaxTran.CuryTaxableAmt = -taxTran.CuryTaxableAmt;
                newTaxTran.CuryTaxAmt = -taxTran.CuryTaxAmt;
                newTaxTran.CuryExpenseAmt = -taxTran.CuryExpenseAmt;
                newTaxTran.TaxableAmt = -taxTran.TaxableAmt;
                newTaxTran.TaxAmt = -taxTran.TaxAmt;
                newTaxTran.ExpenseAmt = -taxTran.ExpenseAmt;

                taxes.Add(newTaxTran);
            }



            caadjGraph.Clear();
            reversingContext = true;
            CAAdj insertedAdj = CAAdjRecords.Insert(adj);
            PXNoteAttribute.CopyNoteAndFiles(CAAdjRecords.Cache, current, CAAdjRecords.Cache, insertedAdj);
            foreach (Tuple<CASplit, CASplit> pair in splits)
            {
                CASplit newSplit = pair.Item2;
                newSplit = CASplitRecords.Insert(newSplit);
                PXNoteAttribute.CopyNoteAndFiles(CASplitRecords.Cache, pair.Item1, CASplitRecords.Cache, newSplit);
            }
            reversingContext = false;
            foreach (CATaxTran newTaxTran in taxes)
            {
                Taxes.Insert(newTaxTran);
            }
            //We should reenter totals depending on taxes as TaxAttribute does not recalculate them if externalCall==false
            CAAdjRecords.Cache.SetValue<CAAdj.taxRoundDiff>(insertedAdj, adj.TaxRoundDiff);
            CAAdjRecords.Cache.SetValue<CAAdj.curyTaxRoundDiff>(insertedAdj, adj.CuryTaxRoundDiff);
            CAAdjRecords.Cache.SetValue<CAAdj.taxTotal>(insertedAdj, adj.TaxAmt);
            CAAdjRecords.Cache.SetValue<CAAdj.curyTaxTotal>(insertedAdj, adj.CuryTaxAmt);
            CAAdjRecords.Cache.SetValue<CAAdj.tranAmt>(insertedAdj, adj.TranAmt);
            CAAdjRecords.Cache.SetValue<CAAdj.curyTranAmt>(insertedAdj, adj.CuryTranAmt);
            insertedAdj = CAAdjRecords.Update(insertedAdj);
            List<CAAdj> ret = new List<CAAdj> { insertedAdj };
            return ret;

        }

        #endregion
    }

}