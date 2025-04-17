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
using PX.Objects.CS;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the bank information of a specific employee. The information will be displayed on the Employee Payroll Settings (PR203000) form.
	/// </summary>
	[PXCacheName(Messages.PREmployeeDirectDeposit)]
	[Serializable]
	public class PREmployeeDirectDeposit : IBqlTable, ISortOrder
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployeeDirectDeposit>.By<bAccountID, lineNbr>
		{
			public static PREmployeeDirectDeposit Find(PXGraph graph, int? bAccountID, int? lineNbr, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, bAccountID, lineNbr, options);
		}

		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PREmployeeDirectDeposit>.By<bAccountID> { }
		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeeDirectDeposit.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		#endregion
		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		[PXDefault]
		[PXLineNbr(typeof(PREmployee))]
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion
		#region BankAcctNbr
		public abstract class bankAcctNbr : PX.Data.BQL.BqlString.Field<bankAcctNbr> { }
		[PXDBString(30, IsUnicode = true, InputMask = "AAAAAAAAAAAAAAAAAA")]
		[PXDefault]
		[PXUIField(DisplayName = "Account Number")]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollUS>))]
		[PXUIRequiredIfVisible]
		public string BankAcctNbr { get; set; }
		#endregion
		#region BankRoutingNbr
		public abstract class bankRoutingNbr : PX.Data.BQL.BqlString.Field<bankRoutingNbr> { }
		[PXDBString(30, IsUnicode = true, InputMask = "000000000")]
		[PXUIField(DisplayName = "Bank Routing Number")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollUS>))]
		[PXUIRequiredIfVisible]
		public string BankRoutingNbr { get; set; }
		#endregion
		#region BankAcctType
		public abstract class bankAcctType : PX.Data.BQL.BqlString.Field<bankAcctType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(BankAccountType.Checking)]
		[PXUIField(DisplayName = "Type")]
		[BankAccountType.List]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollUS>))]
		[PXUIRequiredIfVisible]
		public string BankAcctType { get; set; }
		#endregion
		#region BankName
		public abstract class bankName : PX.Data.BQL.BqlString.Field<bankName> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Name")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollUS>))]
		[PXUIRequiredIfVisible]
		public string BankName { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Amount")]
		public decimal? Amount { get; set; }
		#endregion
		#region Percent
		public abstract class percent : PX.Data.BQL.BqlDecimal.Field<percent> { }
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Percent")]
		public decimal? Percent { get; set; }
		#endregion
		#region GetsRemainder
		public abstract class getsRemainder : PX.Data.BQL.BqlBool.Field<getsRemainder> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Gets Remainder")]
		public bool? GetsRemainder { get; set; }
		#endregion
		#region SortOrder
		[PXDBInt(MinValue = 1)]
		[PXUIField(DisplayName = "Sequence")]
		[PXDefault]
		[PXCheckUnique(typeof(PREmployeeDirectDeposit.bAccountID))]
		public virtual int? SortOrder { get; set; }
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		#endregion
		#region BankTransitNbrCan
		public abstract class bankTransitNbrCan : PX.Data.BQL.BqlString.Field<bankTransitNbrCan> { }
		[PXDBString(30, IsUnicode = true, InputMask = "00000")]
		[PXUIField(DisplayName = "Bank Transit Number")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollCAN>))]
		[PXUIRequiredIfVisible]
		public string BankTransitNbrCan { get; set; }
		#endregion
		#region FinInstNbrCan
		public abstract class finInstNbrCan : PX.Data.BQL.BqlString.Field<finInstNbrCan> { }
		[PXDBString(30, IsUnicode = true, InputMask = "000")]
		[PXUIField(DisplayName = "Financial Institution Number")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollCAN>))]
		[PXUIRequiredIfVisible]
		public string FinInstNbrCan { get; set; }
		#endregion
		#region BankAcctNbrCan
		public abstract class bankAcctNbrCan : PX.Data.BQL.BqlString.Field<bankAcctNbrCan> { }
		[PXDBString(30, IsUnicode = true, InputMask = "AAAAAAAAAAAA")]
		[PXUIField(DisplayName = "Bank Account Number")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollCAN>))]
		[PXUIRequiredIfVisible]
		public string BankAcctNbrCan { get; set; }
		#endregion
		#region BeneficiaryName
		public abstract class beneficiaryName : PX.Data.BQL.BqlString.Field<beneficiaryName> { }
		[PXDBString(30, IsUnicode = true, InputMask = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
		[PXUIField(DisplayName = "Beneficiary Name")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollCAN>))]
		[PXUIRequiredIfVisible]
		public string BeneficiaryName { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
