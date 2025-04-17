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
using PX.Objects.CS;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class MoveEntry : MoveEntryBase<Where<AMBatch.docType, Equal<AMDocType.move>>>
    {
        #region LineSplitting
        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class LineSplittingExtension : AMMoveLineSplittingExtension<MoveEntry> { }
        public LineSplittingExtension LineSplittingExt => FindImplementation<LineSplittingExtension>();
        public override PXSelectBase<AMMTran> LSSelectDataMember => LineSplittingExt.lsselect; 

        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class ItemAvailabilityExtension : AMBatchItemAvailabilityExtension<MoveEntry> { }
        #endregion

        public MoveEntry()
        {
            GL.OpenPeriodAttribute.SetValidatePeriod<AMBatch.finPeriodID>(batch.Cache, null, GL.PeriodValidation.DefaultSelectUpdate);
            PXVerifySelectorAttribute.SetVerifyField<AMMTran.receiptNbr>(transactions.Cache, null, true);
            PXUIFieldAttribute.SetVisible<AMMTran.receiptNbr>(transactions.Cache, null, true);
        }

        protected override void AMMTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null || sender.GetStatus(row) == PXEntryStatus.InsertedDeleted)
            {
                return;
            }

            //Only prompt when a non referenced batch
            if (batch.Current != null
                && string.IsNullOrWhiteSpace(batch.Current.OrigBatNbr)
                && row.DocType == batch.Current.DocType && row.BatNbr == batch.Current.BatNbr
                && !_skipReleasedReferenceDocsCheck
                && ReferenceDeleteGraph.HasReleasedReferenceDocs(this, row, true))
            {
                throw new PXException(Messages.ReleasedTransactionExist);
            }
        }


        [PXRestrictor(typeof(Where<AMOrderType.function, NotEqual<OrderTypeFunction.disassemble>>), Messages.IncorrectOrderTypeFunction)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMMTran.orderType> e) { }

        protected override void AMMTran_OperationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            base.AMMTran_OperationID_FieldUpdated(sender, e);

            var row = (AMMTran)e.Row;
            if (row?.OperationID == null || IsImport || IsContractBasedAPI || row.Qty.GetValueOrDefault() < 0)
            {
                return;
            }

            var amOrderType = (AMOrderType)PXSelectorAttribute.Select<AMMTran.orderType>(sender, row);
            if (amOrderType == null || !amOrderType.DefaultOperationMoveQty.GetValueOrDefault())
            {
                return;
            }

            var amProdOper = (AMProdOper)PXSelectorAttribute.Select<AMMTran.operationID>(sender, row);
            if (amProdOper != null)
            {
                sender.SetValueExt<AMMTran.qty>(row, amProdOper.QtyRemaining.GetValueOrDefault());
            }
        }

		protected virtual void _(Events.RowPersisting<AMBatch> e, PXRowPersisting baseMethod)
		{
			if (baseMethod != null)
			{
				baseMethod.Invoke(e.Cache, e.Args);
			}
			CheckForUnreleasing(e);
		}

		protected virtual void CheckForUnreleasing(Events.RowPersisting<AMBatch> e)
		{
			ProductionTransactionHelper.CheckForUnreleasedBatches(e);
		}

    }
}
