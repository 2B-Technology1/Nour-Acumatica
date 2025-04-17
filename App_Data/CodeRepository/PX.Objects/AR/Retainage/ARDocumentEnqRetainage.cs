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
using PX.Objects.CS;

namespace PX.Objects.AR
{
	[Serializable]
	public class ARDocumentEnqRetainage : PXGraphExtension<ARDocumentEnq>
	{
		[Serializable]
		[PXHidden]
		public class ARRegisterOrig : ARRegister
		{
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : IBqlField { }
			#endregion
			#region BranchID
			public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			#endregion
			#region Released
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			#endregion
			#region OrigDocType
			public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
			#endregion
			#region OrigRefNbr
			public new abstract class origRefNbr : IBqlField { }
			#endregion
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.retainage>();
		}

		#region Cache Attached Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency Original Retainage")]
		protected virtual void ARDocumentResult_CuryRetainageTotal_CacheAttached(PXCache sender) { }
		
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency Unreleased Retainage")]
		protected virtual void ARDocumentResult_CuryRetainageUnreleasedAmt_CacheAttached(PXCache sender) { }
		
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency Total Amount")]
		protected virtual void ARDocumentResult_CuryOrigDocAmtWithRetainageTotal_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Retainage", Visible = false)]
		[PXDBCalced(typeof(
				Switch<Case<Where<
						Exists<Select<
							ARRegisterOrig,
							Where<ARRegisterOrig.origDocType, Equal<ARDocumentEnq.ARDocumentResult.docType>,
							And<ARRegisterOrig.origRefNbr, Equal<ARDocumentEnq.ARDocumentResult.refNbr>,
							And<ARRegisterOrig.branchID, Equal<ARDocumentEnq.ARDocumentResult.branchID>,
							And<ARRegisterOrig.released, Equal<True>>>>>>>>,
					True>, False>),
			typeof(bool))]
		protected virtual void ARDocumentResult_Retainage_CacheAttached(PXCache sender) { }
		#endregion
	}
}