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

namespace PX.Objects.Extensions.PayLink
{
	/// <summary>
	/// Represents a mapped cache extension for SOOrder/ARInvoice to store data for the Payment Link processing.
	/// </summary>
	public class PayLinkDocument : PXMappedCacheExtension
	{
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		/// <exclude/>
		public string OrderType { get; set; }
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		/// <exclude/>
		public string OrderNbr { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		/// <exclude/>
		public string DocType { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		/// <exclude/>
		public string RefNbr { get; set; }
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <exclude/>
		public int? BranchID { get; set; }
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		/// <exclude/>
		public string CuryID { get; set; }
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
		/// <exclude/>
		public string ProcessingCenterID { get; set; }
		public abstract class deliveryMethod : PX.Data.BQL.BqlString.Field<deliveryMethod> { }
		/// <summary>
		/// Payment Link delivery method (N - none, E - email).
		/// </summary>
		public string DeliveryMethod { get; set; }
		public abstract class payLinkID : PX.Data.BQL.BqlInt.Field<payLinkID> { }
		/// <summary>
		/// Acumatica specific Payment Link Id.
		/// </summary>
		public virtual int? PayLinkID { get; set; }
	}
}
