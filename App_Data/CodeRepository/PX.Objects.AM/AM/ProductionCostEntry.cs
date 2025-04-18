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
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.AM.Attributes;
using System.Collections;
using System.Collections.Generic;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Entry for production costs. If an entry page exists it would be read only. These transactions are built automatically from other entry pages (Move/Labor)
    /// </summary>
    public class ProductionCostEntry : AMBatchSimpleEntryBase
    {
        public PXSelectJoin<AMBatch,
			InnerJoinSingleTable<Branch, On<AMBatch.branchID, Equal<Branch.branchID>>>,
			Where<AMBatch.docType, Equal<AMDocType.prodCost>,
				And<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>>> batch;
        public PXSelect<AMMTran, Where<AMMTran.docType, Equal<Current<AMBatch.docType>>, And<AMMTran.batNbr, Equal<Current<AMBatch.batNbr>>>>> transactions;

        public ProductionCostEntry()
        {
            batch.AllowUpdate = false;
            batch.AllowDelete = false;

            transactions.AllowUpdate = false;
            transactions.AllowInsert = false;
            transactions.AllowDelete = false;

            PXUIFieldAttribute.SetVisible<AMMTran.tranType>(transactions.Cache, null, true);
        }

        #region Cache Attached

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Tran Description", Visible = true)]
        protected virtual void _(Events.CacheAttached<AMMTran.tranDesc> e) { }

        [OperationIDField]
        [PXSelector(typeof(Search<AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Current<AMMTran.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD), ValidateValue = false)]
		protected virtual void _(Events.CacheAttached<AMMTran.operationID> e) { }

        [AMOrderTypeSelector(ValidateValue = false)]
        [AMOrderTypeField]
		protected virtual void _(Events.CacheAttached<AMMTran.orderType> e) { }

        [Inventory]
		protected virtual void _(Events.CacheAttached<AMMTran.inventoryID> e) { }

        [PXDBInt]
		protected virtual void _(Events.CacheAttached<AMMTran.locationID> e) { }

        [PXDBInt]
		protected virtual void _(Events.CacheAttached<AMMTran.siteID> e) { }

        [INUnit(typeof(AMMTran.inventoryID))]
		protected virtual void _(Events.CacheAttached<AMMTran.uOM> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Quantity", Visible = false, Enabled = false)]
		protected virtual void _(Events.CacheAttached<AMMTran.qty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXLineNbr(typeof(AMBatch.lineCntr), DecrementOnDelete = false, ReuseGaps = false)]
		protected virtual void _(Events.CacheAttached<AMMTran.lineNbr> sender)
		{

		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(Enabled = false)]
		protected virtual void _(Events.CacheAttached<AMMTran.branchID> e) { }

		#endregion

		protected virtual void AMBatch_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.ProdCost;
        }

        protected virtual void AMMTran_DocType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMDocType.ProdCost;
        }

        protected virtual void AMBatch_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (ampsetup.Current.RequireControlTotal == false)
            {
                if (PXCurrencyAttribute.IsNullOrEmpty(((AMBatch)e.Row).TotalAmount) == false)
                {
                    sender.SetValue<AMBatch.controlAmount>(e.Row, ((AMBatch)e.Row).TotalAmount);
                }
                else
                {
                    sender.SetValue<AMBatch.controlAmount>(e.Row, 0m);
                }
            }
        }

        protected virtual void AMBatch_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var batch = (AMBatch) e.Row;
            if (batch == null)
            {
                return;
            }

            var editablebatch = batch.EditableBatch == true;
            var balancedBatch = batch.DeletableBatch == true;

            sender.AllowInsert = true;
            sender.AllowUpdate = editablebatch;
            Delete.SetEnabled(balancedBatch);
            release.SetVisible(balancedBatch);
            release.SetEnabled(balancedBatch);

            PXUIFieldAttribute.SetVisible<AMBatch.controlQty>(sender, batch, false);
            PXUIFieldAttribute.SetVisible<AMBatch.controlAmount>(sender, batch, false);
            PXUIFieldAttribute.SetEnabled<AMBatch.status>(sender, batch, false);
            PXUIFieldAttribute.SetEnabled<AMBatch.hold>(sender, batch, editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.finPeriodID>(sender, batch, editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.controlQty>(sender, batch, editablebatch);
            PXUIFieldAttribute.SetEnabled<AMBatch.controlQty>(sender, batch, editablebatch);
        }



        #region Buttons

        public PXAction<AMBatch> release;
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
        [PXProcessButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            PXCache cache = batch.Cache;
            List<AMBatch> list = new List<AMBatch>();
            foreach (AMBatch amdoc in adapter.Get<AMBatch>())
            {
                if (amdoc.Hold == false && amdoc.Released == false)
                {
                    cache.Update(amdoc);
                    list.Add(amdoc);
                }
            }
            if (list.Count == 0)
            {
                throw new PXException(PX.Objects.IN.Messages.Document_Status_Invalid);
            }
            Save.Press();

            PXLongOperation.StartOperation(this,
                delegate ()
                {
                    AMDocumentRelease.ReleaseDoc(list);
                });

            return list;
        }

        [PXDeleteButton]
        [PXUIField(DisplayName = "")]
        protected virtual IEnumerable delete(PXAdapter a)
        {
            var row = batch.Current;
            if (row == null)
                return a.Get();

            if (ReferenceDeleteGraph.HasReleasedReferenceDocs(this, row, true))
            {
                throw new PXException(Messages.ReleasedBatchExist);
            }

            //make sure the original move/labor batch is not balanced
            AMBatch result = SelectFrom<AMBatch>.Where<AMBatch.docType.IsEqual<@P.AsString>
                .And<AMBatch.batNbr.IsEqual<@P.AsString>>
                .And<AMBatch.released.IsEqual<False>>>.View.Select(this, row.OrigDocType, row.OrigBatNbr);
            if(result != null)
            {
                throw new PXException(Messages.UnreleasedBatchExist);
            }

            batch.Delete(row);
            this.Save.Press();

            return a.Get();
        }

        #endregion

        #region AMBatchSimpleEntryBase members

        public override PXSelectBase<AMBatch> AMBatchDataMember => batch;
        public override PXSelectBase<AMMTran> AMMTranDataMember => transactions;

        #endregion
    }
}
