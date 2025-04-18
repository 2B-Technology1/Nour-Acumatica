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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM.Extensions;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;

namespace PX.Objects.CA
{
	/// <summary>
	/// A CA transaction detail for the bank transaction for which a CA document will be created.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.CABankTranDetail)]
	public partial class CABankTranDetail : IBqlTable, ICATranDetail
	{
		#region Keys
		public class PK : PrimaryKeyOf<CABankTranDetail>.By<bankTranID, bankTranType, lineNbr>
		{
			public static CABankTranDetail Find(PXGraph graph, int? bankTranID, string bankTranType, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bankTranID, bankTranType, lineNbr, options);
		}

		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<CABankTranDetail>.By<branchID> { }
			public class BankTransaction : CA.CABankTran.PK.ForeignKeyOf<CABankTranDetail>.By<bankTranID> { }
			public class OffsetAccount : GL.Account.PK.ForeignKeyOf<CABankTranDetail>.By<accountID> { }
			public class Subaccount : GL.Account.PK.ForeignKeyOf<CABankTranDetail>.By<subID> { }
			public class OffsetCashAccount : CA.CashAccount.PK.ForeignKeyOf<CABankTranDetail>.By<cashAccountID> { }
			public class TaxCategory : TX.TaxCategory.PK.ForeignKeyOf<CABankTranDetail>.By<taxCategoryID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<CABankTranDetail>.By<curyInfoID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<CABankTranDetail>.By<inventoryID> { }
			public class Project : PMProject.PK.ForeignKeyOf<CABankTranDetail>.By<projectID> { }
			public class Task : PMTask.PK.ForeignKeyOf<CABankTranDetail>.By<projectID, taskID> { }
			public class CostCode : PMCostCode.PK.ForeignKeyOf<CABankTranDetail>.By<costCodeID> { }
			public class BusinessAccount : CR.BAccount.PK.ForeignKeyOf<CABankTranDetail>.By<referenceID> { }
		}

		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch]//we can take this field from cashAcct
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region BankTranID
		public abstract class bankTranID : PX.Data.BQL.BqlInt.Field<bankTranID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(CABankTran.tranID))]
		[PXParent(typeof(Select<CABankTran, Where<CABankTran.tranType, Equal<Current<CABankTranDetail.bankTranType>>, And<CABankTran.tranID, Equal<Current<CABankTranDetail.bankTranID>>>>>))]
		[PXUIField(Visible = false)]
		public virtual int? BankTranID
		{
			get;
			set;
		}
		#endregion
		#region BankTranType
		public abstract class bankTranType : PX.Data.BQL.BqlString.Field<bankTranType> { }
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(CABankTran.tranType))]
		[PXUIField(DisplayName = "Type", Visible = false)]
		public virtual string BankTranType
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(CABankTran.lineCntrCA))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(
			typeof(CABankTranDetail.branchID),
			typeof(Search<Account.accountID,
				Where2<Match<Current<AccessInfo.userName>>,
					And<Where<Account.curyID, Equal<Current<CABankTran.curyID>>, Or<Account.curyID, IsNull>>>>>),
			DisplayName = "Offset Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description),
			CacheGlobal = false, Required = true,
			PersistingCheck = PXPersistingCheck.Nothing,
			AvoidControlAccounts = true)]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[SubAccount(typeof(CABankTranDetail.accountID), typeof(CABankTranDetail.branchID), DisplayName = "Offset Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXRestrictor(typeof(Where<CashAccount.branchID, Equal<Current<CABankTranDetail.branchID>>>), Messages.CashAccountNotMatchBranch)]
		[PXRestrictor(typeof(Where<CashAccount.curyID, Equal<Current<CABankTran.curyID>>>), Messages.OffsetAccountForThisEntryTypeMustBeInSameCurrency)]
		[CashAccountScalar(DisplayName = "Offset Cash Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
		[PXDBScalar(typeof(Search<CashAccount.cashAccountID, Where<CashAccount.accountID, Equal<CABankTranDetail.accountID>,
								 And<CashAccount.subID, Equal<CABankTranDetail.subID>, And<CashAccount.branchID, Equal<CABankTranDetail.branchID>>>>>))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
						   Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string TaxCategoryID
		{
			get;
			set;
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }
		[PXDBInt]
		public virtual int? ReferenceID
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		[PXDBString(Common.Constants.TranDescLength512, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string TranDesc
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		[CurrencyInfo(typeof(CABankTran.curyInfoID), PopulateParentCuryInfoID = true)]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }
		[PXDBCurrency(typeof(CABankTranDetail.curyInfoID), typeof(CABankTranDetail.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? CuryTranAmt
		{
			get;
			set;
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran. Amount")]
		public virtual decimal? TranAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		[PXCurrency(typeof(CABankTranDetail.curyInfoID), typeof(CABankTranDetail.taxableAmt), BaseCalc = false)]
		[PXDBScalar(typeof(Search2<CATax.curyTaxableAmt, InnerJoin<Tax, On<Tax.taxID, Equal<CATax.taxID>>>,
		 Where<CATax.adjTranType, Equal<CABankTranDetail.bankTranType>,
			And<CATax.adjRefNbr, Equal<CABankTranDetail.bankTranID>,
			And<CATax.lineNbr, Equal<CABankTranDetail.lineNbr>,
			And<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
			And<Tax.taxType, NotEqual<CSTaxType.withholding>>>>>>, OrderBy<Asc<CATax.taxID>>>))]
		public virtual decimal? CuryTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		[PXBaseCury]
		[PXDBScalar(typeof(Search2<CATax.taxableAmt, InnerJoin<Tax, On<Tax.taxID, Equal<CATax.taxID>>>,
		 Where<CATax.adjTranType, Equal<CABankTranDetail.bankTranType>,
			And<CATax.adjRefNbr, Equal<CABankTranDetail.bankTranID>,
			And<CATax.lineNbr, Equal<CABankTranDetail.lineNbr>,
			And<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
			And<Tax.taxType, NotEqual<CSTaxType.withholding>>>>>>, OrderBy<Asc<CATax.taxID>>>))]
		public virtual decimal? TaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }

		/// <summary>
		/// The amount of tax (VAT) associated with the line in the selected currency.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTaxAmt
		{
			get;
			set;
		}
		#endregion

		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }

		/// <summary>
		/// The amount of tax (VAT) associated with the line in the base currency.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxAmt
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[NonStockItem]
		[PXUIField(DisplayName = "Item ID", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "1.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual decimal? Qty
		{
			get;
			set;
		}
		#endregion

		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? UnitPrice
		{
			get;
			set;
		}
		#endregion

		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

		[PXDBCurrencyPriceCost(typeof(CABankTranDetail.curyInfoID), typeof(CASplit.unitPrice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Price")]
		public virtual decimal? CuryUnitPrice
		{
			get;
			set;
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[ProjectDefault(BatchModule.CA)]
		[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInCA, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBase(Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		[ActiveProjectTask(typeof(CABankTranDetail.projectID), BatchModule.CA, DisplayName = "Project Task", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? TaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(null, typeof(taskID), null, DisplayName = "Cost Code", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region NonBillable
		public abstract class nonBillable : PX.Data.BQL.BqlBool.Field<nonBillable> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Non Billable", Visible = false, Visibility = PXUIVisibility.SelectorVisible, FieldClass = ProjectAttribute.DimensionName)]
		public virtual bool? NonBillable
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
