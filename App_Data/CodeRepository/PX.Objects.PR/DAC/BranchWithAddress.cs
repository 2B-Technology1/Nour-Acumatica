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
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	[PXHidden]
	public class BranchAddress : Address
	{
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		public new abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
	}

	[Serializable]
	[PXHidden]
	public class BranchAddressBAccount : BAccount
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public new abstract class acctName : PX.Data.BQL.BqlInt.Field<acctName> { }
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
	}

	[PXHidden]
	[PXProjection(typeof(SelectFrom<BranchWithAddress>
				.InnerJoin<BranchAddressBAccount>.On<BranchAddressBAccount.bAccountID.IsEqual<BranchWithAddress.bAccountID>>
				.InnerJoin<BranchAddress>.On<BranchAddress.addressID.IsEqual<BranchAddressBAccount.defAddressID>>))]
	[PXBreakInheritance]
	public class BranchWithAddress : Branch
	{
		public new class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		#region AcctName
		public new class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

		[PXDBString(BqlField = typeof(BranchAddressBAccount.acctName))]
		[PXUIField(DisplayName = "Branch Name", Visibility = PXUIVisibility.SelectorVisible)]
		public override String AcctName { get; set; }
		#endregion

		#region AddressCountryID
		public abstract class addressCountryID : PX.Data.BQL.BqlString.Field<addressCountryID> { }
		[PXDBString(100, BqlField = typeof(BranchAddress.countryID))]
		[PXUIField(DisplayName = "Address Country")]
		public virtual string AddressCountryID { get; set; }
		#endregion
	}
}
