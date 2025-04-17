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
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class CostRollHistory : PXGraph<CostRollHistory>
    {
        [PXFilterable]
        public PXSelectJoin<AMBomCostHistory,
            LeftJoin<AMBomItem, On<AMBomCostHistory.bOMID, Equal<AMBomItem.bOMID>,
                And<AMBomCostHistory.revisionID, Equal<AMBomItem.revisionID>>>>,
			Where<AMBomCostHistory.curyID, Equal<Current<AccessInfo.baseCuryID>>>> CostRollHistoryRecords;
        public PXSetup<AMBSetup> ambsetup;
        public PXSelect<AMBomItem> BomItemRecs;

        public CostRollHistory()
        {
            CostRollHistoryRecords.AllowDelete = false;
            CostRollHistoryRecords.AllowInsert = false;
            CostRollHistoryRecords.AllowUpdate = false;
        }

        [PXUIField(DisplayName = "Current Status")]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<AMBomItem.status> e) { }

        [PXUIField(DisplayName = "Date")]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<AMBomCostHistory.createdDateTime> e) { }

        public PXAction<AMBomCostHistory> ViewBOM;
        [PXUIField(DisplayName = "View BOM")]
        [PXButton]
        protected virtual void viewBOM()
        {
            BOMMaint.Redirect(CostRollHistoryRecords?.Current?.BOMID, CostRollHistoryRecords?.Current?.RevisionID);
        }

    }
}
