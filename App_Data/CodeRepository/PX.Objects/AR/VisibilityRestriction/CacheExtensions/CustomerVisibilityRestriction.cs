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
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public sealed class CustomerVisibilityRestriction : PXCacheExtension<Customer>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region AcctCD
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CustomerRaw(typeof(Search2<
						Customer.acctCD,
						LeftJoin<Contact, 
							On<Contact.bAccountID, Equal<Customer.bAccountID>, 
							And<Contact.contactID, Equal<Customer.defContactID>>>,
						LeftJoin<Address, 
							On<Address.bAccountID, Equal<Customer.bAccountID>, 
							And<Address.addressID, Equal<Customer.defAddressID>>>>>,
						Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
							And<Match<Current<AccessInfo.userName>>>>>),
					IsKey = true)]
		public string AcctCD { get; set; }
		#endregion

		#region CustomerClassID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [DBField definded in the base DAC]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Search2<ARSetup.dfltCustomerClassID,
			InnerJoin<CustomerClass, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>,
			Where<CustomerClass.orgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>))]
		[PXSelector(typeof(Search<CustomerClass.customerClassID,
			Where<CustomerClass.orgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>, And<MatchUser>>>),
			CacheGlobal = true,
			DescriptionField = typeof(CustomerClass.descr))]
		public string CustomerClassID { get; set; }
		#endregion

		#region COrgBAccountID
		public abstract class cOrgBAccountID : PX.Data.BQL.BqlInt.Field<cOrgBAccountID> { }
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(0, typeof(Search<CustomerClass.orgBAccountID,Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
		public int? COrgBAccountID { get; set; }
		#endregion

		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches(typeof(BAccountR.cOrgBAccountID))]
		[RestrictVendorByUserBranches(typeof(BAccountR.vOrgBAccountID))]
		public int? ParentBAccountID { get; set; }
		#endregion
	}
}