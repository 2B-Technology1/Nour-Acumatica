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
using PX.Objects.CM.Extensions;

namespace PX.Objects.Extensions.MultiCurrency
{
    /// <summary>A mapped cache extension that represents a document that supports multiple currencies.</summary>
    public class Document : PXMappedCacheExtension
    {
        #region BAccountID
        /// <exclude />
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        /// <summary>The identifier of the business account of the document.</summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.CR.BAccount.BAccountID" /> field.
        /// </value>
        public virtual Int32? BAccountID
        {
			get;
			set;
        }
        #endregion
        #region BranchID
        /// <exclude />
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        /// <summary>The identifier of the branch of the document.</summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.GL.Branch.BranchID" /> field.
        /// </value>
        public virtual Int32? BranchID
        {
	        get;
	        set;
        }
        #endregion
        #region CuryID
        /// <exclude />
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        /// <summary>
        /// The code of the <see cref="Currency"/> of the document.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="PX.Objects.GL.Company.BaseCuryID">base currency of the company</see>.
        /// Corresponds to the <see cref="PX.Objects.CM.Currency.CuryID"/> field.
        /// </value>
        public virtual String CuryID
        {
			get;
			set;
        }
		#endregion
		#region CurrencyRate
		public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		[PXDecimal]
		public virtual decimal? CuryRate
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CurrencyInfo(typeof(CurrencyInfo.curyInfoID))]
		public virtual Int64? CuryInfoID { get; set; }
		#endregion
		#region DocumentDate
		/// <exclude />
		public abstract class documentDate : PX.Data.BQL.BqlDateTime.Field<documentDate> { }
        /// <summary>The date of the document.</summary>
        public virtual DateTime? DocumentDate
        {
			get;
			set;
        }
		#endregion

		#region CuryViewState
		/// <exclude />
		public abstract class curyViewState : PX.Data.BQL.BqlDateTime.Field<curyViewState> { }
		/// <summary>
		/// Required for PXCurrencyRate to be operational
		/// </summary>
		public virtual bool? CuryViewState { get; set; }
		#endregion
	}
}
