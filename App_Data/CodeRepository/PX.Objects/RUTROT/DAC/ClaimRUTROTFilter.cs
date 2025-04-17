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

namespace PX.Objects.RUTROT
{
	[Serializable]
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public partial class ClaimRUTROTFilter : IBqlTable
	{
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }

		[ClaimActions.List]
		[PXDBString(1)]
		[PXDefault(ClaimActions.Claim)]
		[PXUIField(DisplayName = "Action", Visible = true)]
		public virtual string Action
		{
			get;
			set;
		}
		#endregion

		#region DeductionType
		public abstract class rUTROTType : PX.Data.BQL.BqlString.Field<rUTROTType> { }

		[RUTROTTypes.List]
        [PXDefault(RUTROTTypes.RUT, typeof(Search<BranchRUTROT.defaultRUTROTType, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Deduction Type", Visible = true)]
		public string RUTROTType
		{
			get;
			set;
		}
		#endregion
	}

	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class ClaimActions
	{
		public class List : PXStringListAttribute
		{
			public List()
				: base(new string[] { Balance, Claim, Export }, new string[] { "Balance", "Claim", "Export" })
			{
			}
		}

        public const string Balance = "B";
        public const string Export = "E";
		public const string Claim = "C";

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class balance : PX.Data.BQL.BqlString.Constant<balance>
		{
            public balance() : base(Balance) { }
        }

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class export : PX.Data.BQL.BqlString.Constant<export>
		{
			public export() : base(Export) { }
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class claim : PX.Data.BQL.BqlString.Constant<claim>
		{
			public claim() : base(Claim) { }
		}
	}
}
