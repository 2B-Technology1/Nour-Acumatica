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

namespace PX.Objects.AM
{
	/// <summary>
	/// Disassembly transaction attributes based on the <see cref="AMMTranAttribute"/> class. Values are entered on the Attributes tab of the Disassembly (AM301500) form (corresponding to the <see cref="DisassemblyEntry"/> graph).
	/// </summary>
	[Serializable]
    [PXPrimaryGraph(typeof(DisassemblyEntry))]
    [PXCacheName(Messages.AMDisassembleTranAttribute)]
    [System.Diagnostics.DebuggerDisplay("DocType = {DocType}, BatNbr = {BatNbr}, TranLineNbr = {TranLineNbr}, LineNbr = {LineNbr}, Label = {Label}, Value = {Value}")]
    public class AMDisassembleBatchAttribute : AMMTranAttribute
    {
        #region DocType
        public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected new String _DocType;
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXUIField(DisplayName = "Doc Type", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMDisassembleBatch.docType))]
        public override String DocType
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
        #region BatNbr
        public new abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected new String _BatNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Bat Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMDisassembleBatch.batchNbr))]
        public override String BatNbr
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
        #region TranLineNbr
        public new abstract class tranLineNbr : PX.Data.BQL.BqlInt.Field<tranLineNbr> { }

        protected new Int32? _TranLineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Tran Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMDisassembleBatch.lineNbr))]
        [PXParent(typeof(Select<AMDisassembleBatch, 
            Where<AMDisassembleBatch.docType, Equal<Current<AMDisassembleBatchAttribute.docType>>, 
            And<AMDisassembleBatch.batchNbr, Equal<Current<AMDisassembleBatchAttribute.batNbr>>, 
            And<AMDisassembleBatch.lineNbr, Equal<Current<AMDisassembleBatchAttribute.tranLineNbr>>>>>>))]
        public override Int32? TranLineNbr
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
        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected new Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMDisassembleBatch.lineCntrAttribute))]
        public override Int32? LineNbr
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

        public new abstract class value : PX.Data.BQL.BqlString.Field<value> { }

        public new abstract class label : PX.Data.BQL.BqlString.Field<label> { }
    }
}
