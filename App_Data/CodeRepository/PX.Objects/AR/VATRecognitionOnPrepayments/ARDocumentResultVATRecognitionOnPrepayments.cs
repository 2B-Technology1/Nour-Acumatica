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
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.AR
{
	/// <exclude/>
	public sealed class ARDocumentResultVATRecognitionOnPrepayments : PXCacheExtension<ARDocumentEnq.ARDocumentResult>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.vATRecognitionOnPrepayments>();
		}

		#region ARAccountID
		/// <inheritdoc cref="ARRegister.ARAccountID"/>
		public abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }

		// Acuminator disable once PX1095 NoUnboundTypeAttributeWithPXDBCalced [Type field define with Account attribue]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[Account(typeof(ARRegister.branchID), IsDBField = false, DisplayName = "AR Account",
			BqlTable = typeof(ARRegister))]
		[PXDBCalced(typeof(
			Switch<Case<Where<ARRegister.docType.IsEqual<ARDocType.prepaymentInvoice>>,
					ARRegister.prepaymentAccountID>,
				ARRegister.aRAccountID>), typeof(int?))]
		public int? ARAccountID { get; set; }
		#endregion
		#region ARSubID
		/// <inheritdoc cref="ARRegister.ARSubID"/>
		public abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }

		// Acuminator disable once PX1095 NoUnboundTypeAttributeWithPXDBCalced [Type field define with SubAccount attribue]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[SubAccount(typeof(aRAccountID), IsDBField = false, DescriptionField = typeof(Sub.description), DisplayName = "AR Subaccount",
			BqlTable = typeof(ARRegister))]
		[PXDBCalced(typeof(
			Switch<Case<Where<ARRegister.docType.IsEqual<ARDocType.prepaymentInvoice>>,
					ARRegister.prepaymentSubID>,
				ARRegister.aRSubID>), typeof(int?))]
		public int? ARSubID { get; set; }
		#endregion
	}
}
