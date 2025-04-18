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
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PR
{
	/// <summary>
	/// Payroll Module's extension of the InventoryItem DAC.
	/// </summary>
	public sealed class PRxInventoryItem : PXCacheExtension<InventoryItem>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region Keys
		public static class FK
		{
			public class EarningsAccount : Account.PK.ForeignKeyOf<InventoryItem>.By<earningsAcctID> { }
			public class EarningsSubaccount : Sub.PK.ForeignKeyOf<InventoryItem>.By<earningsSubID> { }
			public class BenefitExpenseAccount : Account.PK.ForeignKeyOf<InventoryItem>.By<benefitExpenseAcctID> { }
			public class BenefitExpenseSubaccount : Sub.PK.ForeignKeyOf<InventoryItem>.By<benefitExpenseSubID> { }
			public class TaxExpenseAccount : Account.PK.ForeignKeyOf<InventoryItem>.By<taxExpenseAcctID> { }
			public class TaxExpenseSubaccount : Sub.PK.ForeignKeyOf<InventoryItem>.By<taxExpenseSubID> { }
			public class PTOExpenseAccount : Account.PK.ForeignKeyOf<InventoryItem>.By<ptoExpenseAcctID> { }
			public class PTOExpenseSubaccount : Sub.PK.ForeignKeyOf<InventoryItem>.By<ptoExpenseSubID> { }
		}
		#endregion

		#region EarningsAcctID
		public abstract class earningsAcctID : PX.Data.BQL.BqlInt.Field<earningsAcctID> { }
		[Account(DisplayName = "Earnings Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<PRxINPostClass.earningsAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.earningsAcctID>.IsRelatedTo<Account.accountID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PREarningAccountRequired(GLAccountSubSource.LaborItem)]
		public int? EarningsAcctID { get; set; }
		#endregion

		#region EarningsSubID
		public abstract class earningsSubID : PX.Data.BQL.BqlInt.Field<earningsSubID> { }
		[SubAccount(typeof(PRxInventoryItem.earningsAcctID), DisplayName = "Earnings Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<PRxINPostClass.earningsSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.earningsSubID>.IsRelatedTo<Sub.subID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PREarningSubRequired(GLAccountSubSource.LaborItem)]
		public int? EarningsSubID { get; set; }
		#endregion

		#region BenefitExpenseAcctID
		public abstract class benefitExpenseAcctID : PX.Data.BQL.BqlInt.Field<benefitExpenseAcctID> { }
		[Account(DisplayName = "Benefit Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<PRxINPostClass.benefitExpenseAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.benefitExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRBenExpenseAccountRequired(GLAccountSubSource.LaborItem)]
		public int? BenefitExpenseAcctID { get; set; }
		#endregion

		#region BenefitExpenseSubID
		public abstract class benefitExpenseSubID : PX.Data.BQL.BqlInt.Field<benefitExpenseSubID> { }
		[SubAccount(typeof(PRxInventoryItem.benefitExpenseAcctID), DisplayName = "Benefit Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<PRxINPostClass.benefitExpenseSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.benefitExpenseSubID>.IsRelatedTo<Sub.subID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRBenExpenseSubRequired(GLAccountSubSource.LaborItem)]
		public int? BenefitExpenseSubID { get; set; }
		#endregion

		#region TaxExpenseAcctID
		public abstract class taxExpenseAcctID : PX.Data.BQL.BqlInt.Field<taxExpenseAcctID> { }
		[Account(DisplayName = "Tax Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<PRxINPostClass.taxExpenseAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.taxExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRTaxExpenseAccountRequired(GLAccountSubSource.LaborItem)]
		public int? TaxExpenseAcctID { get; set; }
		#endregion

		#region TaxExpenseSubID
		public abstract class taxExpenseSubID : PX.Data.BQL.BqlInt.Field<taxExpenseSubID> { }
		[SubAccount(typeof(PRxInventoryItem.taxExpenseAcctID), DisplayName = "Tax Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<PRxINPostClass.taxExpenseSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.taxExpenseSubID>.IsRelatedTo<Sub.subID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRTaxExpenseSubRequired(GLAccountSubSource.LaborItem)]
		public int? TaxExpenseSubID { get; set; }
		#endregion

		#region PTOExpenseAcctID
		public abstract class ptoExpenseAcctID : PX.Data.BQL.BqlInt.Field<ptoExpenseAcctID> { }
		[Account(DisplayName = "PTO Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXDefault(typeof(Search<PRxINPostClass.ptoExpenseAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(FK.PTOExpenseAccount))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRPTOExpenseAccountRequired(GLAccountSubSource.LaborItem, typeof(Where<FeatureInstalled<FeaturesSet.payrollCAN>>))]
		public int? PTOExpenseAcctID { get; set; }
		#endregion

		#region PTOExpenseSubID
		public abstract class ptoExpenseSubID : PX.Data.BQL.BqlInt.Field<ptoExpenseSubID> { }
		[SubAccount(typeof(ptoExpenseAcctID), DisplayName = "PTO Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXDefault(typeof(Search<PRxINPostClass.ptoExpenseSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(FK.PTOExpenseSubaccount))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRPTOExpenseSubRequired(GLAccountSubSource.LaborItem, typeof(Where<FeatureInstalled<FeaturesSet.payrollCAN>>))]
		public int? PTOExpenseSubID { get; set; }
		#endregion
	}
}
