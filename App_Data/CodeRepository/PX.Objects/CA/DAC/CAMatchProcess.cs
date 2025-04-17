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

namespace PX.Objects.CA
{
	[PXHidden]
	public class CAMatchProcess : IBqlTable
	{
		#region Keys
		public class PK : Data.ReferentialIntegrity.Attributes.PrimaryKeyOf<CAMatchProcess>.By<cashAccountID>
		{
			public static CAMatchProcess Find(PXGraph graph, int? cashAccount, PKFindOptions options = PKFindOptions.None) => FindBy(graph, cashAccount, options);
		}

		public static class FK
		{
			public class CashAcccount : CA.CashAccount.PK.ForeignKeyOf<CABankTran>.By<cashAccountID> { }
		}

		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region ProcessUID
		public abstract class processUID : PX.Data.BQL.BqlGuid.Field<processUID> { }

		[PXDBGuid]
		[PXDefault]
		public virtual Guid? ProcessUID
		{
			get;
			set;
		}
		#endregion
		#region OperationStartDate
		public abstract class operationStartDate : PX.Data.BQL.BqlDateTime.Field<operationStartDate> { }

		[PXDBDate(PreserveTime = true)]
		public virtual DateTime? OperationStartDate
		{
			get;
			set;
		}
		#endregion
		#region StartedByID
		public abstract class startedByID : PX.Data.BQL.BqlGuid.Field<startedByID> { }

		[PXDBGuid]
		public virtual Guid? StartedByID
		{
			get;
			set;
		}
		#endregion
	}
}
