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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Estimate PX Selector Attribute for all Estimates/Revisions
    /// </summary>
    public class EstimateIDSelectPrimaryAttribute : PXSelectorAttribute
    {
        public EstimateIDSelectPrimaryAttribute()
            : base(typeof(Search<AMEstimateItem.estimateID, Where<AMEstimateItem.revisionID, Equal<AMEstimateItem.primaryRevisionID>,
				And<AMEstimateItem.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>), ColumnList)
        { }

        public EstimateIDSelectPrimaryAttribute(params Type[] colList)
            : base(typeof(Search<AMEstimateItem.estimateID, Where<AMEstimateItem.revisionID, Equal<AMEstimateItem.primaryRevisionID>,
				And<AMEstimateItem.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>), colList) { }

        public EstimateIDSelectPrimaryAttribute(Type searchType)
            : base(searchType, ColumnList)
        { }

        /// <summary>
        /// Column list for EstimateID selector
        /// </summary>
        public static Type[] ColumnList => new [] {typeof(AMEstimateItem.estimateID),
            typeof(AMEstimateItem.revisionID),
            typeof(AMEstimateItem.inventoryCD),
            typeof(AMEstimateItem.itemDesc),
            typeof(AMEstimateItem.estimateClassID),
            typeof(AMEstimateItem.estimateStatus),
            typeof(AMEstimateItem.revisionDate)
        };
    }
}
