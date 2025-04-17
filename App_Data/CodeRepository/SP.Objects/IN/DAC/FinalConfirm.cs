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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.SO;

namespace SP.Objects.IN.DAC
{
    [Serializable]
    public class FinalConfirm : IBqlTable
    {
        #region Comment
        public abstract class comment : PX.Data.IBqlField
        {
        }
        protected String _Comment;
        [PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Comment")]
        public virtual String Comment
        {
            get
            {
                return this._Comment;
            }
            set
            {
                this._Comment = value;
            }
        }
        #endregion
        
        #region CuryLineTotal
        public abstract class curyLineTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryLineTotal;
        [PXBaseCury()]
        [PXUIField(DisplayName = "Cart Total", Enabled = false)]
        public virtual Decimal? CuryLineTotal
        {
            get
            {
                return this._CuryLineTotal;
            }
            set
            {
                this._CuryLineTotal = value;
            }
        }
        #endregion

        #region CuryFreightAmt
        public abstract class curyFreightAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryFreightAmt;
        [PXBaseCury()]
        [PXUIField(DisplayName = "Freight", Enabled = false)]
        public virtual Decimal? CuryFreightAmt
        {
            get
            {
                return this._CuryFreightAmt;
            }
            set
            {
                this._CuryFreightAmt = value;
            }
        }
        #endregion

        #region CuryTaxTotal
        public abstract class curyTaxTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryTaxTotal;
        [PXBaseCury()]
        [PXUIField(DisplayName = "Tax Total", Enabled = false)]
        public virtual Decimal? CuryTaxTotal
        {
            get
            {
                return this._CuryTaxTotal;
            }
            set
            {
                this._CuryTaxTotal = value;
            }
        }
        #endregion

        #region CuryDiscTot
        public abstract class curyDiscTot : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDiscTot;
        [PXBaseCury()]
        [PXUIField(DisplayName = "Discount Total", Enabled = false)]
        public virtual Decimal? CuryDiscTot
        {
            get
            {
                return this._CuryDiscTot;
            }
            set
            {
                this._CuryDiscTot = value;
            }
        }
        #endregion

        #region CuryOrderTotal
        public abstract class curyOrderTotal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryOrderTotal;
        [PXBaseCury()]
        [PXUIField(DisplayName = "Total", Enabled = false)]
        public virtual Decimal? CuryOrderTotal
        {
            get
            {
                return this._CuryOrderTotal;
            }
            set
            {
                this._CuryOrderTotal = value;
            }
        }
        #endregion



        #region CurrencyStatus
        public abstract class currencyStatus : PX.Data.IBqlField
        {
        }
        protected String _CurrencyStatus;
        [PXString(IsUnicode = true)]
        [PXUIField()]
        [PXDefault("USD", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String CurrencyStatus
        {
            get
            {
                return this._CurrencyStatus;
            }
            set
            {
                this._CurrencyStatus = value;
            }
        }
        #endregion

        #region Shipping Information
        public abstract class shippingInformation : PX.Data.IBqlField
        {
        }
        protected string _ShippingInformation;
        [PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Shipping Information", Enabled = false)]
        public virtual string ShippingInformation
        {
            get
            {
                return this._ShippingInformation;
            }
            set
            {
                this._ShippingInformation = value;
            }
        }
        #endregion
    }
}
