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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.IN.InventoryRelease;
using PX.Objects.IN.InventoryRelease.Accumulators.Documents;
using PX.Objects.PO;
using System;

namespace PX.Objects.IN.GraphExtensions.INReleaseProcessExt
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class UpdatePOAccrual: PXGraphExtension<INReleaseProcess>
    {
        public PXSelect<POAccrualStatus> poAccrualStatuses;
        public PXSelect<POAccrualSplit> poAccrualSplits;
        public PXSelect<POAccrualDetailPostedUpdate> poAccrualDetailPostedUpdate;
        public PXSelect<POAccrualDetailPPVAdjPostedUpdate> poAccrualDetailPPVAdjPostedUpdate;
        public PXSelect<POAccrualDetailTaxAdjPostedUpdate> poAccrualDetailTaxAdjPostedUpdate;

        protected INRegister ProcesssingDocument;
        protected DuplicatesSearchEngine<POReceiptLine> ProcessedStatuses;

        protected virtual void InitializeProcessedCaches(INRegister register)
        {
            ProcesssingDocument = register;
            ProcessedStatuses = new DuplicatesSearchEngine<POReceiptLine>(
                Base.Caches<POReceiptLine>(),
                new[] { typeof(POReceiptLine.pOAccrualRefNoteID), typeof(POReceiptLine.pOAccrualLineNbr), typeof(POReceiptLine.pOAccrualType) },
                Array<POReceiptLine>.Empty);
        }

        protected virtual void MarkProcessed(INTran tran, POReceiptLine receiptLine)
        {
            ProcessedStatuses.AddItem(receiptLine);
        }

        protected virtual bool IsProcessed(POReceiptLine receiptLine) => ProcessedStatuses.Find(receiptLine) != null;

        protected virtual POReceiptLine FindReceiptLine(INTran tran) => POReceiptLine.PK.Find(Base, tran.POReceiptType, tran.POReceiptNbr, tran.POReceiptLineNbr);

        protected virtual POAccrualStatus FindAccrualStatus(POReceiptLine receiptLine)
        {
            return SelectFrom<POAccrualStatus>
               .Where<POAccrualStatus.refNoteID.IsEqual<@P.AsGuid>
               .And<POAccrualStatus.lineNbr.IsEqual<@P.AsInt>>
               .And<POAccrualStatus.type.IsEqual<@P.AsString.ASCII>>>
               .View.SelectWindowed(Base, 0, 1,
                   receiptLine.POAccrualRefNoteID,
                   receiptLine.POAccrualLineNbr,
                   receiptLine.POAccrualType);
        }

        protected virtual bool AffectsPOAccrual(INTran tran)
        {
            if (string.IsNullOrEmpty(tran.POReceiptNbr) 
                || tran.POReceiptLineNbr == null
                || tran.POReceiptType == POReceiptType.TransferReceipt)
                return false;

            var register = Base.inregister.Current;

            return register.IsPPVTran == true
                || register.IsTaxAdjustmentTran == true
                || register.DocType == INDocType.Receipt
                || register.DocType == INDocType.Issue && register.SOShipmentType == SO.SOShipmentType.DropShip
				|| register.DocType == INDocType.Issue && tran.POReceiptType == POReceiptType.POReturn;
        }

        /// <summary>
        /// Overrides <see cref="INReleaseProcess.OnTranReleased"/>
        /// </summary>
        [PXOverride]
        public virtual void OnTranReleased(INTran tran, Action<INTran> baseImpl)
        {
            baseImpl(tran);

            if (!AffectsPOAccrual(tran))
                return;

            var register = Base.inregister.Current;

            POReceiptLine receiptLine;
            if (ProcesssingDocument?.DocType != register.DocType
                || ProcesssingDocument?.RefNbr != register.RefNbr)
            {
                InitializeProcessedCaches(register);

                receiptLine = FindReceiptLine(tran);
            }
            else
            {
                receiptLine = FindReceiptLine(tran);
                if (IsProcessed(receiptLine))
                    return;
            }

            POAccrualStatus status = FindAccrualStatus(receiptLine);

            if(register.IsPPVTran == true)
            {
                UpdatePPVAdjPosted(tran, status);
                status.UnreleasedPPVAdjCntr--;
            }
            else if (register.IsTaxAdjustmentTran == true)
            {
                UpdateTaxAdjPosted(tran, status);
                status.UnreleasedTaxAdjCntr--;
            }
            else
            {
                var update = UpdateINReceiptPosted(tran);
                status.UnreleasedReceiptCntr--;

                if(register.DocType == INDocType.Issue && register.SOShipmentType == SO.SOShipmentType.DropShip)
                {
                    update.FinPeriodID = register.FinPeriodID;
                    poAccrualDetailPostedUpdate.Update(update);

                    if (register.FinPeriodID.CompareTo(status.MaxFinPeriodID) > 0)
                        status.MaxFinPeriodID = register.FinPeriodID;

                    var laterSplits = new SelectFrom<POAccrualSplit>
                        .Where<
						POAccrualSplit.pOReceiptType.IsEqual<@P.AsString.ASCII>
						.And<POAccrualSplit.pOReceiptNbr.IsEqual<@P.AsString>>
                        .And<POAccrualSplit.pOReceiptLineNbr.IsEqual<@P.AsInt>>
                        .And<POAccrualSplit.finPeriodID.IsLess<@P.AsString.ASCII>>>
                        .View(Base)
                        .SelectMain(receiptLine.ReceiptType, receiptLine.ReceiptNbr, receiptLine.LineNbr, register.FinPeriodID);
                    foreach(var split in laterSplits)
                    {
                        split.FinPeriodID = register.FinPeriodID;
                        poAccrualSplits.Update(split);
                    }
                }
            }

            poAccrualStatuses.Update(status);

            MarkProcessed(tran, receiptLine);
        }

		protected virtual POAccrualDetailPostedUpdate UpdateAccrualDetailPosted(INTran tran)
        {
            var detail = new POAccrualDetailPostedUpdate
            {
				POReceiptType = tran.POReceiptType,
				POReceiptNbr = tran.POReceiptNbr,
                LineNbr = tran.POReceiptLineNbr
            };

            detail = poAccrualDetailPostedUpdate.Insert(detail);

            detail.Posted = true;

            return poAccrualDetailPostedUpdate.Update(detail);
        }

		// TODO: Replace UpdateINReceiptPosted usages with UpdateAccrualDetailPosted.
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		protected virtual POAccrualDetailPostedUpdate UpdateINReceiptPosted(INTran tran) => UpdateAccrualDetailPosted(tran);

        protected virtual POAccrualDetailPPVAdjPostedUpdate UpdatePPVAdjPosted(INTran tran, POAccrualStatus status)
        {
            var detail = new POAccrualDetailPPVAdjPostedUpdate
            {
                POAccrualRefNoteID = status.RefNoteID,
                POAccrualLineNbr = status.LineNbr,
                POAccrualType = status.Type,
                PPVAdjRefNbr = tran.RefNbr
            };

            detail = poAccrualDetailPPVAdjPostedUpdate.Insert(detail);

            detail.PPVAdjPosted = true;

            return poAccrualDetailPPVAdjPostedUpdate.Update(detail);
        }

        protected virtual POAccrualDetailTaxAdjPostedUpdate UpdateTaxAdjPosted(INTran tran, POAccrualStatus status)
        {
            var detail = new POAccrualDetailTaxAdjPostedUpdate
            {
                POAccrualRefNoteID = status.RefNoteID,
                POAccrualLineNbr = status.LineNbr,
                POAccrualType = status.Type,
                TaxAdjRefNbr = tran.RefNbr
            };

            detail = poAccrualDetailTaxAdjPostedUpdate.Insert(detail);

            detail.TaxAdjPosted = true;

            return poAccrualDetailTaxAdjPostedUpdate.Update(detail);
        }

        /// <summary>
        /// Overrides <see cref="INReleaseProcess.UpdatePOReceiptLineCost(INTran, INTranCost, InventoryItem)"/>
        /// </summary>
        [PXOverride]
        public virtual POReceiptLineUpdate UpdatePOReceiptLineCost(INTran tran, INTranCost tranCost, InventoryItem item, Func<INTran, INTranCost, InventoryItem, POReceiptLineUpdate> baseImpl)
        {
            var receiptLineUpdate = baseImpl(tran, tranCost, item);
            
            if (receiptLineUpdate != null)
                UpdateAccruedCost(tran, receiptLineUpdate);

            return receiptLineUpdate;
        }

        protected virtual void UpdateAccruedCost(INTran tran, POReceiptLineUpdate receiptLineUpdate)
        {
            var detail = new POAccrualDetailPostedUpdate
            {
				POReceiptType = tran.POReceiptType,
				POReceiptNbr = tran.POReceiptNbr,
                LineNbr = tran.POReceiptLineNbr
            };

            detail = poAccrualDetailPostedUpdate.Insert(detail);
            detail.PreviousCost = detail.AccruedCost;
            detail.AccruedCost = -receiptLineUpdate.TranCostFinal;
            detail = poAccrualDetailPostedUpdate.Update(detail);

            var receiptLine = FindReceiptLine(tran);

            POAccrualStatus status = FindAccrualStatus(receiptLine);
            
			status.ReceivedCost = -receiptLineUpdate.TranCostFinal;
            poAccrualStatuses.Update(status);
        }
    }
}
