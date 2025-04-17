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

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class AccountsFilter : IBqlTable, IClassIdFilter
	{
		#region BAccountID

		public abstract class bAccountID : PX.Data.BQL.BqlString.Field<bAccountID> { }

		[PXDefault]
		[PXDBString]
		[PXDimension(BAccountAttribute.DimensionName)]
		[PXUIField(DisplayName = "Business Account ID", Required = true)]
		public virtual string BAccountID { get; set; }

		#endregion

		#region AccountName

		public abstract class accountName : PX.Data.BQL.BqlString.Field<accountName> { }

		[PXDefault]
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Business Account Name", Required = true)]
		public virtual string AccountName { get; set; }

		#endregion

		#region AccountClass

		public abstract class accountClass : PX.Data.BQL.BqlString.Field<accountClass> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Business Account Class")]
		[PXSelector(typeof(CRCustomerClass.cRCustomerClassID), DescriptionField = typeof(CRCustomerClass.description))]
		public virtual string AccountClass { get; set; }

		string IClassIdFilter.ClassID => AccountClass;

		#endregion

		#region LinkContactToAccount

		public abstract class linkContactToAccount : PX.Data.BQL.BqlBool.Field<linkContactToAccount> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Link Contact to Account")]
		public virtual bool? LinkContactToAccount { get; set; }

		#endregion
	}
}