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
using PX.Data.BQL;
using PX.TM;

namespace PX.Objects.CR.CRMarketingListMaint_Extensions
{
	[PXHidden]
	public class AddMembersToNewListFilter : IBqlTable
	{
		#region MailListCode

		// Acuminator disable once PX1030 PXDefaultIncorrectUse [required filter]]
		[PXString(30, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXDimension(CRMarketingList.mailListCode.DimensionName)]
		[PXUIField(DisplayName = "Marketing List ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string MailListCode { get; set; }
		public abstract class mailListCode : BqlString.Field<mailListCode> { }

		#endregion

		#region Name

		// Acuminator disable once PX1030 PXDefaultIncorrectUse [required filter]]
		[PXString(50, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "List Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Name { get; set; }
		public abstract class name : BqlString.Field<name> { }

		#endregion

		#region OwnerID

		[Owner]
		public virtual int? OwnerID { get; set; }
		public abstract class ownerID : BqlInt.Field<ownerID> { }

		#endregion
	}
}
