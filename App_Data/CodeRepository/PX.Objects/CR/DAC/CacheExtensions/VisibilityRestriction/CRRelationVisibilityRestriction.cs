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

namespace PX.Objects.CR
{
	public sealed class CRRelationVisibilityRestriction : PXCacheExtension<CRRelation>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region EntityID

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search<
			BAccount.bAccountID,
			Where2<
				Where<BAccount.type, Equal<BAccountType.prospectType>,
					Or<BAccount.type, Equal<BAccountType.customerType>,
					Or<BAccount.type, Equal<BAccountType.combinedType>,
					Or2<
						Where<BAccount.type, Equal<BAccountType.vendorType>>, 
						And<BAccount.vOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>>>>,
				And<Match<Current<AccessInfo.userName>>>>>),
			fieldList: new[]
			{
				typeof(BAccount.acctCD),
				typeof(BAccount.acctName),
				typeof(BAccount.classID),
				typeof(BAccount.type),
				typeof(BAccount.parentBAccountID),
				typeof(BAccount.acctReferenceNbr)
			},
			SubstituteKey = typeof(BAccount.acctCD),
			DescriptionField = typeof(BAccount.acctName),
			Filterable = true,
			DirtyRead = true)]
		public int? EntityID { get; set; }

		#endregion
	}
}
