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

namespace PX.Objects.CR.CRMarketingListMaint_Extensions
{
	[PXHidden]
	public class CopyMembersFilter : IBqlTable
	{
		#region AddMembersOption

		[PXInt]
		[addMembersOption.List]
		[PXUnboundDefault(addMembersOption.AddToNew)]
		[PXUIField]
		public int? AddMembersOption { get; set; }

		public abstract class addMembersOption : BqlInt.Field<addMembersOption>
		{
			public const int AddToNew = 0;
			public const int AddToExisting = 1;

			public class addToNew : BqlInt.Constant<addToNew>
			{
				public addToNew() : base(AddToNew) {}
			}

			public class addToExisting : BqlInt.Constant<addToExisting>
			{
				public addToExisting() : base(AddToExisting) { }
			}

			public class ListAttribute : PXIntListAttribute
			{
				public ListAttribute() : base(
					(AddToNew, "Add All Members to a New Static List"),
					(AddToExisting, "Add All Members to Existing Static Lists")
				) {}
			} 
		}

		#endregion
	}
}
