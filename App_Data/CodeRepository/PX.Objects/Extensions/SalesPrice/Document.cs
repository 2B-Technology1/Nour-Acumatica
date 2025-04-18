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

namespace PX.Objects.Extensions.SalesPrice
{
    /// <summary>A mapped cache extension that represents a document that supports multiple price lists.</summary>
    public class Document : PXMappedCacheExtension
    {
        #region BranchID
        /// <exclude />
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        /// <exclude />
        protected Int32? _BranchID;

        /// <summary>The identifier of the branch associated with the document.</summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.GL.Branch.BranchID" /> field.
        /// </value>
        public virtual Int32? BranchID
        {
            get
            {
                return _BranchID;
            }
            set
            {
                _BranchID = value;
            }
        }
        #endregion
        #region BAccountID
        /// <exclude />
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        /// <exclude />
        protected Int32? _BAccountID;

        /// <summary>The identifier of the business account of this document.</summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.CR.BAccount.BAccountID" /> field.
        /// </value>
        public virtual Int32? BAccountID
        {
            get
            {
                return _BAccountID;
            }
            set
            {
                _BAccountID = value;
            }
        }
        #endregion
        #region CuryID
        /// <exclude />
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        /// <exclude />
        protected String _CuryID;

        /// <summary>The identifier of the currency in the system.</summary>
        public virtual String CuryID
        {
            get
            {
                return _CuryID;
            }
            set
            {
                _CuryID = value;
            }
        }
        #endregion
        #region CuryInfoID
        /// <exclude />
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        /// <exclude />
        protected Int64? _CuryInfoID;

        /// <summary>The identifier of the <see cref="CM.CurrencyInfo">CurrencyInfo</see> object associated with the document.</summary>
        /// <value>
        /// Corresponds to the <see cref="CM.CurrencyInfo.CuryInfoID" /> field.
        /// </value>
        public virtual Int64? CuryInfoID
        {
            get
            {
                return _CuryInfoID;
            }
            set
            {
                _CuryInfoID = value;
            }
        }
        #endregion
        #region DocumentDate
        /// <exclude />
        public abstract class documentDate : PX.Data.BQL.BqlDateTime.Field<documentDate> { }
        /// <exclude />
        protected DateTime? _DocumentDate;
        /// <summary>The date of the document.</summary>
        public virtual DateTime? DocumentDate
        {
            get
            {
                return _DocumentDate;
            }
            set
            {
                _DocumentDate = value;
            }
        }
        #endregion
        #region TaxCalcMode
        /// <exclude />
        public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }        
        /// <summary>
        /// Tax Calculation Mode
        /// </summary>
        public virtual string TaxCalcMode
        {
            get;
            set;
        }
        #endregion
    }
}
