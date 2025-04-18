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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using System;

namespace PX.Objects.AR.Standalone
{
	/// <exclude/>
	[PXHidden]
	[Serializable]
	public partial class ARRegister : AR.ARRegister
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ARRegister>.By<docType, refNbr>
		{
			public static ARRegister Find(PXGraph graph, string docType, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, docType, refNbr, options);
		}
		public new static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<ARRegister>.By<branchID> { }
			public class Customer : AR.Customer.PK.ForeignKeyOf<ARRegister>.By<customerID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<ARRegister>.By<curyInfoID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<ARRegister>.By<curyID> { }
			public class ARAccount : GL.Account.PK.ForeignKeyOf<ARRegister>.By<aRAccountID> { }
			public class ARSubaccount : GL.Sub.PK.ForeignKeyOf<ARRegister>.By<aRSubID> { }
		}
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		[PXDBLong]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region ClosedFinPeriodID
		public new abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }
		#endregion

		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		public new abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }

		#region ARAccountID
		public new abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }

		[PXDefault]
		[Account(typeof(ARRegister.branchID), typeof(Search<Account.accountID,
			Where2<Match<Current<AccessInfo.userName>>,
				And<Account.active, Equal<True>,
					And<Account.isCashAccount, Equal<False>,
						And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
							Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AR Account")]
		public override Int32? ARAccountID
		{
			get;
			set;
		}
		#endregion
		#region ARSubID
		public new abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }

		[PXDefault]
		[SubAccount(typeof(ARRegister.aRAccountID), DescriptionField = typeof(Sub.description), DisplayName = "AR Subaccount", Visibility = PXUIVisibility.Visible)]
		public override Int32? ARSubID
		{
			get;
			set;
		}
		#endregion
	}
}
