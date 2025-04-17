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
    public class EstimateIDSelectAllAttribute : PXSelectorAttribute
    {
        #region No arguments
        /// <summary>
        /// Manufacturing Estimate Select All PX Selector Attribute.
        /// Fields displayed in selector:
        /// AMEstimateItem.estimateID; AMEstimateItem.revisionID; AMEstimateItem.status; 
        /// </summary>
        public EstimateIDSelectAllAttribute()
            : base(typeof(Search<AMEstimateItem.estimateID, Where<AMEstimateItem.curyID, Equal<Current<AccessInfo.baseCuryID>>>>), ColumnList)
        { }
        #endregion

        #region Column Parms Argument Only
        /// <summary>
        /// Manufacturing Estimate Select All PX Selector Attribute.
        /// Search is defined, pass the columns to display in the selector.
        /// Available table AMEstimateItem
        /// </summary>
        /// <param name="colList">Columns to display in selector</param>
        public EstimateIDSelectAllAttribute(params Type[] colList)
            : base(typeof(Search<AMEstimateItem.estimateID, Where<AMEstimateItem.curyID, Equal<AccessInfo.baseCuryID>>>), colList) { }
        #endregion

        /// <summary>
        /// EstimateID SelectAll attribute with a search type and predefined column list
        /// </summary>
        /// <param name="searchType"></param>
        public EstimateIDSelectAllAttribute(Type searchType)
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
            typeof(AMEstimateItem.revisionDate),
            typeof(AMEstimateItem.isPrimary)
        };
    }
}
