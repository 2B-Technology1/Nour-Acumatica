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
using System;

namespace PX.Objects.AR.Light
{
	[PXHidden]
	public partial class BAccount : IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBIdentity]
		public virtual int? BAccountID { get; set; }
		#endregion
		#region AcctName
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

		[PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(60, IsUnicode = true)]
		public virtual string AcctName { get; set; }
		#endregion
		#region ConsolidatingBAccountID
		public abstract class consolidatingBAccountID : PX.Data.BQL.BqlInt.Field<consolidatingBAccountID> { }

		[PXDBInt]
		public virtual int? ConsolidatingBAccountID { get; set; }
		#endregion
		#region AcctCD
		public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string AcctCD { get; set; }
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXDBString(5, IsUnicode = true)]
		public virtual string CuryID { get; set; }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(1, IsFixed = true)]
		[CustomerStatus.List]
		public virtual string Status { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXDBGuid]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Parent Account", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? ParentBAccountID { get; set; }
		#endregion
	}
	[Serializable]
	[PXTable(typeof(BAccount.bAccountID))]
	[PXCacheName("Light version of Customer DAC for Statements Printing")]
	public class Customer : BAccount
	{
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual string CustomerClassID { get; set; }
		#endregion
		#region StatementCycleId
		public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual string StatementCycleId { get; set; }
		#endregion
		#region ConsolidatingBAccountID
		public new abstract class consolidatingBAccountID : PX.Data.BQL.BqlInt.Field<consolidatingBAccountID> { }
		#endregion
		#region PrintCuryStatements
		public abstract class printCuryStatements : PX.Data.BQL.BqlBool.Field<printCuryStatements> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Multi-Currency Statements")]
		public virtual bool? PrintCuryStatements { get; set; }
		#endregion
		//AcctCD - in base
		//NoteID - in base
		#region SendStatementByEmail

		public abstract class sendStatementByEmail : PX.Data.BQL.BqlBool.Field<sendStatementByEmail> { }
		[PXDBBool]

		[PXUIField(DisplayName = "Send Statements by Email")]
		public virtual bool? SendStatementByEmail { get; set; }
		#endregion
		#region PrintStatements
		public abstract class printStatements : PX.Data.BQL.BqlBool.Field<printStatements> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Print Statements")]
		public virtual bool? PrintStatements { get; set; }
		#endregion
		//BAccountID - in base
		//AcctName - in base
		#region DefBillContactID
		public abstract class defBillContactID : PX.Data.BQL.BqlInt.Field<defBillContactID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Default Contact", Visibility = PXUIVisibility.Invisible)]
		public virtual int? DefBillContactID { get; set; }
		#endregion
		#region DefBillAddressID
		public abstract class defBillAddressID : PX.Data.BQL.BqlInt.Field<defBillAddressID> { }

		[PXDBInt]
		public virtual int? DefBillAddressID { get; set; }
		#endregion

		#region LocaleName
		public abstract class localeName : PX.Data.BQL.BqlString.Field<localeName> { }
		/// <summary>
		/// The name of the customer's locale.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Locale")]
		public virtual string LocaleName { get; set; }
		#endregion

		#region StatementType
		public abstract class statementType : PX.Data.BQL.BqlString.Field<statementType> { }
		/// <summary>
		/// The type of customer statements generated for the customer.
		/// The list of possible values of the field is determined by 
		/// <see cref="StatementTypeAttribute"/>.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Statement Type")]
		public virtual string StatementType { get; set; }
		#endregion
	}
}
