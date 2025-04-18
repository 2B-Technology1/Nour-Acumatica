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
using PX.Objects.IN;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
	/// <summary>
	/// Maintenance table for the tools on an estimate operation on the Estimate Operations (AM304000) form (corresponding to the <see cref="EstimateOperMaint"/> graph).
	/// Parent: <see cref="AMEstimateItem"/>, <see cref="AMEstimateOper"/>
	/// </summary>
	[Serializable]
    [PXCacheName(Messages.EstimateTool)]
    public class AMEstimateTool : IBqlTable, IEstimateOper, INotable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMEstimateTool>.By<estimateID, revisionID, operationID, lineID>
        {
            public static AMEstimateTool Find(PXGraph graph, string estimateID, string revisionID, int? operationID, int? lineID, PKFindOptions options = PKFindOptions.None)
                => FindBy(graph, estimateID, revisionID, operationID, lineID, options);
            public static AMEstimateTool FindDirty(PXGraph graph, string estimateID, string revisionID, int? operationID, int? lineID)
                => PXSelect<AMEstimateTool,
                    Where<estimateID, Equal<Required<estimateID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>>>>>>
                    .SelectWindowed(graph, 0, 1, estimateID, revisionID, operationID, lineID);
        }

        public static class FK
        {
            public class Estimate : AMEstimateItem.PK.ForeignKeyOf<AMEstimateTool>.By<estimateID, revisionID> { }
            public class Operation : AMEstimateOper.PK.ForeignKeyOf<AMEstimateTool>.By<estimateID, revisionID, operationID> { }
            public class Tool : AMToolMst.PK.ForeignKeyOf<AMEstimateTool>.By<toolID> { }
        }

        #endregion

        #region Estimate ID
        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

        protected String _EstimateID;

        [PXDBDefault(typeof (AMEstimateOper.estimateID))]
        [EstimateID(IsKey = true, Enabled = false, Visible = false)]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }

        #endregion
        #region Revision ID

        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;

        [PXDBDefault(typeof(AMEstimateOper.revisionID))]
        [PXDBString(10, IsUnicode = true, InputMask = ">AAAAAAAAAA", IsKey = true)]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }

        #endregion
        #region Operation ID

        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected Int32? _OperationID;

        [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDefault(typeof (AMEstimateOper.operationID))]
        [PXParent(
            typeof (Select<AMEstimateOper, Where<AMEstimateOper.estimateID, Equal<Current<AMEstimateTool.estimateID>>,
                And<AMEstimateOper.revisionID, Equal<Current<AMEstimateTool.revisionID>>,
                    And<AMEstimateOper.operationID, Equal<Current<AMEstimateTool.operationID>>>>>>))]
        public virtual Int32? OperationID
        {
            get { return this._OperationID; }
            set { this._OperationID = value; }
        }

        #endregion
        #region Line ID

        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

        protected Int32? _LineID;

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        [PXLineNbr(typeof (AMEstimateOper.lineCntrTool))]
        public virtual Int32? LineID
        {
            get { return this._LineID; }
            set { this._LineID = value; }
        }

        #endregion
        #region Qty Req

        public abstract class qtyReq : PX.Data.BQL.BqlDecimal.Field<qtyReq> { }

        protected Decimal? _QtyReq;

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Qty Req")]
        public virtual Decimal? QtyReq
        {
            get { return this._QtyReq; }
            set { this._QtyReq = value; }
        }

        #endregion
        #region ToolID

        public abstract class toolID : PX.Data.BQL.BqlString.Field<toolID> { }

        protected String _ToolID;

        [ToolIDField]
        [PXDefault]
        [PXSelector(typeof (Search<AMToolMst.toolID, Where<AMToolMst.active, Equal<True>>>))]
        public virtual String ToolID
        {
            get { return this._ToolID; }
            set { this._ToolID = value; }
        }

        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDefault(typeof (Search<AMToolMst.descr, Where<AMToolMst.toolID, Equal<Current<AMEstimateTool.toolID>>>>))]
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public virtual String Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        protected Decimal? _UnitCost;
        [PXDBDecimal(6)]
		[PXDefault(typeof(Search<AMToolMstCurySettings.unitCost, Where<AMToolMstCurySettings.toolID, Equal<Current<toolID>>,
			And<AMToolMstCurySettings.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>))]
		[PXUIField(DisplayName = "Unit Cost", Enabled = false)]
        public virtual Decimal? UnitCost
        {
            get { return this._UnitCost; }
            set { this._UnitCost = value; }
        }

        #endregion
        #region Tool Oper Cost

        public abstract class toolOperCost : PX.Data.BQL.BqlDecimal.Field<toolOperCost> { }

        protected Decimal? _ToolOperCost;

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXFormula(typeof (Mult<AMEstimateTool.unitCost, AMEstimateTool.qtyReq>),
            (typeof (SumCalc<AMEstimateOper.toolUnitCost>)))]
        public virtual Decimal? ToolOperCost
        {
            get { return this._ToolOperCost; }
            set { this._ToolOperCost = value; }
        }

        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }


        protected Guid? _CreatedByID;

        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get { return this._CreatedByID; }
            set { this._CreatedByID = value; }
        }

        #endregion
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }


        protected String _CreatedByScreenID;

        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get { return this._CreatedByScreenID; }
            set { this._CreatedByScreenID = value; }
        }

        #endregion
        #region CreatedDateTime

        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }


        protected DateTime? _CreatedDateTime;

        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get { return this._CreatedDateTime; }
            set { this._CreatedDateTime = value; }
        }

        #endregion
        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }


        protected Guid? _LastModifiedByID;

        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get { return this._LastModifiedByID; }
            set { this._LastModifiedByID = value; }
        }

        #endregion
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }


        protected String _LastModifiedByScreenID;

        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get { return this._LastModifiedByScreenID; }
            set { this._LastModifiedByScreenID = value; }
        }

        #endregion
        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }


        protected DateTime? _LastModifiedDateTime;

        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get { return this._LastModifiedDateTime; }
            set { this._LastModifiedDateTime = value; }
        }

        #endregion
        #region tstamp

        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }


        protected Byte[] _tstamp;

        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }

        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
    }
}
