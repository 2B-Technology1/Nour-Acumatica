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
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.CR;

namespace PX.Objects.AM
{
	/// <summary>
	/// Filter for open estimates.
	/// </summary>
	/// <remarks>
	/// This filter is used on the following forms:
	/// <list type="bullet">
	/// <item><description>Sales Orders (SO301000) (corresponding to the <see cref="SOOrderEntry"/> graph)</description></item>
	/// <item><description>Opportunities (CR304000) (corresponding to the <see cref="OpportunityMaint"/> graph)</description></item>
	/// <item><description>Sales Quotes (CR304500) (corresponding to the <see cref="QuoteMaint"/> graph)</description></item>
	/// </list>
	/// </remarks>
	[Serializable]
    [PXCacheName(Messages.EstimateItem)]
    public class AMOrderEstimateItemFilter : AMEstimateItem
    {
        public new abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

        //override to remove is key
        [EstimateIDSelectPrimary(typeof(Search<AMEstimateItem.estimateID, 
            Where<AMEstimateItem.revisionID, Equal<AMEstimateItem.primaryRevisionID>,
                And<AMEstimateItem.quoteSource, Equal<EstimateSource.estimate>,
                And<AMEstimateItem.estimateStatus, NotEqual<EstimateStatus.canceled>,
                    And<AMEstimateItem.estimateStatus, NotEqual<EstimateStatus.closed>,
				And<AMEstimateItem.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>>>>))]
        [EstimateID(Required = true)]
        public override String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }

        //Added Field for getting the Current Estimate in order to select Revision
        public abstract class currentEstimate : PX.Data.BQL.BqlString.Field<currentEstimate> { }

        protected String _CurrentEstimate;
        [PXString]
        [PXUIField(DisplayName = "Current Estimate")]
        public virtual String CurrentEstimate
        {
            get { return this._CurrentEstimate; }
            set { this._CurrentEstimate = value; }
        }

        public new abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        //override to remove is key
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXSelector(typeof(Search<AMEstimateItem.revisionID, Where<AMEstimateItem.estimateID, 
            Equal<Current<AMOrderEstimateItemFilter.currentEstimate>>>>),
            typeof(AMEstimateItem.revisionID),
            typeof(AMEstimateItem.revisionDate),
            typeof(AMEstimateItem.estimateStatus),
            typeof(AMEstimateItem.isPrimary), ValidateValue = false)]
        [PXUIField(DisplayName = "Revision", Required = true)]
        [PXFormula(typeof(Default<AMOrderEstimateItemFilter.estimateID>))]
        public override String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }

        public abstract class addExisting : PX.Data.BQL.BqlBool.Field<addExisting> { }

        protected bool? _AddExisting;
        [PXUnboundDefault(false)]
        [PXBool]
        [PXUIField(DisplayName = "Add Existing")]
        public virtual bool? AddExisting
        {
            get
            {
                return this._AddExisting;
            }
            set
            {
                this._AddExisting = value;
            }
        }

        #region Cury ID

        // remove currency attributes to avoid errors on filter insert
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXUIField(DisplayName = "Currency")]
        public override String CuryID
        {
            get
            {
                return this._CuryID;
            }
            set
            {
                this._CuryID = value;
            }
        }
        #endregion
        #region Cury Info ID
        // remove currency attributes to avoid errors on filter insert
        [PXDBLong]
        public override Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion
    }
}
