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
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AM
{
	/// <summary>
	/// Production attributes that are recorded during a transaction.
	/// Parent: <see cref="AMMTran"/>
	/// </summary>
	/// <remarks>
	/// The records of this type are created and edited on the following forms:
	/// <list type="bullet">
	/// <item><description>Move (AM302000) (corresponding to the <see cref="MoveEntry"/> graph)</description></item>
	/// <item><description>Labor (AM301000) (corresponding to the <see cref="LaborEntry"/> graph)</description></item>
	/// </list>
	/// </remarks>
	[Serializable]
    [PXCacheName(Messages.AMMTranAttribute)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMMTranAttribute : IBqlTable, IAMBatch
    {
        internal string DebuggerDisplay => $"DocType = {DocType}, BatNbr = {BatNbr}, TranLineNbr = {TranLineNbr}, LineNbr = {LineNbr}, Label = {Label}, Value = {Value}";

        #region Keys
        public class PK : PrimaryKeyOf<AMMTranAttribute>.By<docType, batNbr, tranLineNbr, lineNbr, prodAttributeLineNbr>
        {
            public static AMMTranAttribute Find(PXGraph graph, string docType, string batNbr, int? tranLineNbr, int? lineNbr, int? prodAttributeLineNbr, PKFindOptions options = PKFindOptions.None) =>
                FindBy(graph, docType, batNbr, tranLineNbr, lineNbr, prodAttributeLineNbr, options);
        }

        public static class FK
        {
            public class Batch : AMBatch.PK.ForeignKeyOf<AMMTranAttribute>.By<docType, batNbr> { }
            public class Tran : AMMTran.PK.ForeignKeyOf<AMMTranAttribute>.By<docType, batNbr, tranLineNbr> { }
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMMTranAttribute>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMMTranAttribute>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMMTranAttribute>.By<orderType, prodOrdID, operationID> { }
            public class ProductionAttribute : AMProdAttribute.PK.ForeignKeyOf<AMMTranAttribute>.By<orderType, prodOrdID, prodAttributeLineNbr> { }
            public class Attribute : PX.Objects.CS.CSAttribute.PK.ForeignKeyOf<AMMTranAttribute>.By<attributeID> { }
        }
        #endregion

        #region DocType (key)
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected String _DocType;
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXUIField(DisplayName = "Doc Type", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMMTran.docType))]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region BatNbr (key)
        public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected String _BatNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMMTran.batNbr))]
        public virtual String BatNbr
        {
            get
            {
                return this._BatNbr;
            }
            set
            {
                this._BatNbr = value;
            }
        }
        #endregion
        #region TranLineNbr (key)
        public abstract class tranLineNbr : PX.Data.BQL.BqlInt.Field<tranLineNbr> { }

        protected Int32? _TranLineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Tran Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMMTran.lineNbr))]
        [PXParent(typeof(Select<AMMTran, Where<AMMTran.docType, Equal<Current<AMMTranAttribute.docType>>, And<AMMTran.batNbr, 
            Equal<Current<AMMTranAttribute.batNbr>>, And<AMMTran.lineNbr, Equal<Current<AMMTranAttribute.tranLineNbr>>>>>>))]
        public virtual Int32? TranLineNbr
        {
            get
            {
                return this._TranLineNbr;
            }
            set
            {
                this._TranLineNbr = value;
            }
        }
        #endregion
        #region LineNbr (key)
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMMTran.lineCntrAttribute))]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(Visible = false, Enabled = false)]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected string _ProdOrdID;
        [ProductionNbr(Visible = false, Enabled = false)]
        public virtual string ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
        #region ProdAttributeLineNbr (key)
        public abstract class prodAttributeLineNbr : PX.Data.BQL.BqlInt.Field<prodAttributeLineNbr> { }

        protected Int32? _ProdAttributeLineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Production Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        public virtual Int32? ProdAttributeLineNbr
        {
            get
            {
                return this._ProdAttributeLineNbr;
            }
            set
            {
                this._ProdAttributeLineNbr = value;
            }
        }
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField(Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Current<AMMTranAttribute.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<AMMTranAttribute.prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD))]
        public virtual int? OperationID
        {
            get
            {
                return this._OperationID;
            }
            set
            {
                this._OperationID = value;
            }
        }
        #endregion
        #region AttributeID
        public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }

        protected string _AttributeID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Attribute ID", Required = true, Visible = false, Enabled = false)]
        public virtual string AttributeID
        {
            get
            {
                return this._AttributeID;
            }
            set
            {
                this._AttributeID = value;
            }
        }
        #endregion
        #region Label
        public abstract class label : PX.Data.BQL.BqlString.Field<label> { }

        protected string _Label;
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Attribute", Enabled = false)]
        public virtual string Label
        {
            get
            {
                return this._Label;
            }
            set
            {
                this._Label = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected string _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public virtual string Descr
        {
            get
            {
                return this._Descr;
            }
            set
            {
                this._Descr = value;
            }
        }
        #endregion
        #region TransactionRequired
        public abstract class transactionRequired : PX.Data.BQL.BqlBool.Field<transactionRequired> { }

        protected bool? _TransactionRequired;
        [PXDBBool]
        [PXUIField(DisplayName = "Required", Enabled = false)]
        public virtual bool? TransactionRequired
        {
            get
            {
                return this._TransactionRequired;
            }
            set
            {
                this._TransactionRequired = value;
            }
        }
        #endregion
        #region Value
        public abstract class value : PX.Data.BQL.BqlString.Field<value> { }

        protected string _Value;
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Value")]
        [AMAttributeValue(typeof(AMMTranAttribute.attributeID))]
        [DynamicValueValidation(typeof(Search<CSAttribute.regExp, Where<CSAttribute.attributeID, Equal<Current<AMMTranAttribute.attributeID>>>>))]
		[PXDependsOnFields(typeof(AMMTranAttribute.attributeID))]
        public virtual string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
            }
        }
        #endregion
        #region System Fields
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected string _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected string _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected byte[] _tstamp;
        [PXDBTimestamp]
        public virtual byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #endregion
    }
}
