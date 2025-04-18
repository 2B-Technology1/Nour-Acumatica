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
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	[PXCacheName(Messages.CorporateCard)]
	[Serializable]
	public class CACorpCard : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CACorpCard>.By<corpCardID>
		{
			public static CACorpCard Find(PXGraph graph, int? corpCardID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, corpCardID, options);
		}
		public class UK : PrimaryKeyOf<CACorpCard>.By<corpCardCD>
		{
			public static CACorpCard Find(PXGraph graph, string corpCardCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, corpCardCD, options);
		}
		public static class FK
		{
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<CACorpCard>.By<cashAccountID> { }
		}
		[Obsolete("This foreign key is obsolete and is going to be removed in 2021R1. Use PK instead.")]
		public class PKID : PK { }
		[Obsolete("This foreign key is obsolete and is going to be removed in 2021R1. Use UK instead.")]
		public class PKCD : UK { }
		#endregion

		#region CorporateCreditCardID
		[PXDBIdentity]
		[PXDefault]
		[PXUIField(DisplayName = "Corporate Card ID")]
		public virtual int? CorpCardID { get; set; }
		public abstract class corpCardID : BqlInt.Field<corpCardID> { }
		#endregion
		#region CorpCardCD
		[PXDBString(30, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Corporate Card ID", Required = true)]
		[PXSelector(typeof(Search<corpCardCD>),
			typeof(corpCardCD), typeof(name), typeof(cardNumber), typeof(cashAccountID))]
		[AutoNumber(typeof(Search<CASetup.corpCardNumberingID>), typeof(AccessInfo.businessDate))]
		public virtual string CorpCardCD { get; set; }
		public abstract class corpCardCD : BqlString.Field<corpCardCD> { }
		#endregion
		#region Name
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Name")]
		public virtual string Name { get; set; }
		public abstract class name : BqlString.Field<name> { }
		#endregion
		#region CardNumber
		[PXDBString(20, IsUnicode = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Card Number")]
		public virtual string CardNumber { get; set; }
		public abstract class cardNumber : BqlString.Field<cardNumber> { }
		#endregion
		#region CashAccountID
		[CashAccount(
			null,
			typeof(Search<CashAccount.cashAccountID, Where<CashAccount.useForCorpCard, Equal<True>, And<CashAccount.branchID, Equal<Current<branchID>>>>>),
			DisplayName = "Cash Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
		[PXDefault]
		public virtual int? CashAccountID { get; set; }
		public abstract class cashAccountID : BqlInt.Field<cashAccountID> { }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region IsActive
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : BqlBool.Field<isActive> { }
		#endregion
		#region Tstamp
		[PXDBTimestamp]
		public virtual byte[] Tstamp { get; set; }
		public abstract class tstamp : BqlByteArray.Field<tstamp> { }
		#endregion
		#region CreatedByID
        [PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
        [PXDBCreatedDateTime]
		[PXUIField(DisplayName = "Created On")]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
        [PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = "Last Modified On")]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#region Noteid
		[PXNote]
		public virtual Guid? Noteid { get; set; }
		public abstract class noteid : BqlGuid.Field<noteid> { }
		#endregion
	}
}
