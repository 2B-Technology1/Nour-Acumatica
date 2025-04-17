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
using PX.Objects.CA;
using PX.Objects.CS;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information related to the different bank accounts associated with a paycheck. The information will be displayed on the Paychecks and Adjustments (PR302000) form.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.PRDirectDepositSplit)]
	public class PRDirectDepositSplit : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRDirectDepositSplit>.By<docType, refNbr, lineNbr>
		{
			public static PRDirectDepositSplit Find(PXGraph graph, string docType, string refNbr, int? lineNbr, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, docType, refNbr, lineNbr, options);
		}

		public static class FK
		{
			public class Payment : PRPayment.PK.ForeignKeyOf<PRDirectDepositSplit>.By<docType, refNbr> { }
			public class CashAccountTransaction : CATran.UK.ForeignKeyOf<PRDirectDepositSplit>.By<caTranID> { }
		}
		#endregion

		#region DocType
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Payment Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		[PXParent(typeof(Select<PRPayment,
			Where<PRPayment.docType, Equal<Current<PRDirectDepositSplit.docType>>, 
				And<PRPayment.refNbr, Equal<Current<PRDirectDepositSplit.refNbr>>>>>))]
		public virtual string DocType { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion

		#region RefNbr
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Reference Nbr.")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		public virtual string RefNbr { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion

		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		[PXDefault]
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion

		#region BankAcctNbr
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Nbr.")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollUS>))]
		[PXUIRequiredIfVisible]
		public virtual string BankAcctNbr { get; set; }
		public abstract class bankAcctNbr : PX.Data.BQL.BqlString.Field<bankAcctNbr> { }
		#endregion

		#region BankRoutingNbr
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Routing Nbr.")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollUS>))]
		[PXUIRequiredIfVisible]
		public virtual string BankRoutingNbr { get; set; }
		public abstract class bankRoutingNbr : PX.Data.BQL.BqlString.Field<bankRoutingNbr> { }
		#endregion

		#region BankAcctType
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		[BankAccountType.List]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollUS>))]
		[PXUIRequiredIfVisible]
		public string BankAcctType { get; set; }
		public abstract class bankAcctType : PX.Data.BQL.BqlString.Field<bankAcctType> { }
		#endregion

		#region BankName
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Name")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollUS>))]
		[PXUIRequiredIfVisible]
		public string BankName { get; set; }
		public abstract class bankName : PX.Data.BQL.BqlString.Field<bankName> { }
		#endregion

		#region Amount
		[PRCurrency]
		[PXUIField(DisplayName = "Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? Amount { get; set; }
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		#endregion

		#region CATranID
		[PXDBLong]
		[PRDirectDepositCashTranID]
		public virtual Int64? CATranID { get; set; }
		public abstract class caTranID : PX.Data.BQL.BqlLong.Field<caTranID> { }
		#endregion

		#region Released
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Released { get; set; }
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion

		#region BankTransitNbrCan
		public abstract class bankTransitNbrCan : PX.Data.BQL.BqlString.Field<bankTransitNbrCan> { }
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Transit Number")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollCAN>))]
		[PXUIRequiredIfVisible]
		public string BankTransitNbrCan { get; set; }
		#endregion

		#region FinInstNbrCan
		public abstract class finInstNbrCan : PX.Data.BQL.BqlString.Field<finInstNbrCan> { }
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Financial Institution Number")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollCAN>))]
		[PXUIRequiredIfVisible]
		public string FinInstNbrCan { get; set; }
		#endregion

		#region BankAcctNbrCan
		public abstract class bankAcctNbrCan : PX.Data.BQL.BqlString.Field<bankAcctNbrCan> { }
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Account Number")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollCAN>))]
		[PXUIRequiredIfVisible]
		public string BankAcctNbrCan { get; set; }
		#endregion

		#region BeneficiaryName
		public abstract class beneficiaryName : PX.Data.BQL.BqlString.Field<beneficiaryName> { }
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Beneficiary Name")]
		[PXDefault]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.payrollCAN>))]
		[PXUIRequiredIfVisible]
		public string BeneficiaryName { get; set; }
		#endregion

		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}
}
