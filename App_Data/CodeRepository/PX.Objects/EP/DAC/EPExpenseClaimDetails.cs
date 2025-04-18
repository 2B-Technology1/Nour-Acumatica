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
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.SM;
using PX.Objects.Common;
using PX.Objects.EP.DAC;

namespace PX.Objects.EP
{
	/// <summary>
	/// The current status of the expense receipt, which is set by the system.
	/// The fields that determine the status of a document are: 
	/// <see cref="EPExpenseClaimDetails.Hold"/>, <see cref="EPExpenseClaimDetails.Released"/>, 
	/// <see cref="EPExpenseClaimDetails.Approved"/>, <see cref="EPExpenseClaimDetails.Rejected"/>.
	/// </summary>
	/// <value>
	/// The field can have one of the following values:
	/// <c>"H"</c>: The receipt is new and has not been submitted for approval yet, or the receipt has been rejected and then put on hold while a user is adjusting it.
	/// <c>"A"</c>: The receipt is ready to be added to a claim after it has been approved (if approval is required for the receipt) 
	/// or after it has been submitted for further processing (if approval is not required).
	/// <c>"O"</c>: The receipt is pending approval.
	/// <c>"C"</c>: The receipt has been rejected.
	/// <c>"R"</c>: The expense claim associated with the receipt has been released.
	/// </value>
	public class EPExpenseClaimDetailsStatus : ILabelProvider
	{
		public const string ApprovedStatus = "A";
		public const string HoldStatus = "H";
		public const string ReleasedStatus = "R";
		public const string OpenStatus = "O";
		public const string RejectedStatus = "C";

		private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ HoldStatus, Messages.HoldStatus },
			{ ApprovedStatus, Messages.ApprovedStatus },
			{ OpenStatus, Messages.OpenStatus },
			{ ReleasedStatus, Messages.ReleasedStatus },
			{ RejectedStatus, Messages.RejectedStatus  },
		};
		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
		public class ListAttribute : LabelListAttribute
		{
			public ListAttribute() : base(_valueLabelPairs)
			{ }
		}

		public class holdStatus : PX.Data.BQL.BqlString.Constant<holdStatus>
		{
			public holdStatus() : base(HoldStatus) {; }
		}

		public class approvedStatus : PX.Data.BQL.BqlString.Constant<approvedStatus>
		{
			public approvedStatus() : base(ApprovedStatus) {; }
		}

		public class openStatus : PX.Data.BQL.BqlString.Constant<openStatus>
		{
			public openStatus() : base(OpenStatus) {; }
		}

		public class releasedStatus : PX.Data.BQL.BqlString.Constant<releasedStatus>
		{
			public releasedStatus() : base(ReleasedStatus) {; }
		}

		public class rejectedStatus : PX.Data.BQL.BqlString.Constant<rejectedStatus>
		{
			public rejectedStatus() : base(RejectedStatus) {; }
		}
	}

	/// <summary>
	/// Contains the main properties of the expense receipt document, which is a record reflecting that an employee performed 
	/// a transaction while working for your organization, thus incurring certain expenses.
	/// An expense receipt can be edited on the Expense Receipt (EP301020) form (which corresponds to the <see cref="ExpenseClaimDetailEntry"/> graph).
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(ExpenseClaimDetailEntry))]
	[PXCacheName(Messages.ExpenseReceipt)]
	public partial class EPExpenseClaimDetails : IBqlTable, PX.Data.EP.IAssign, INotable
	{
	    public const string DocType = "ECD";

	    public class docType : PX.Data.BQL.BqlString.Constant<docType>
        {
	        public docType() : base(DocType)
	        {

	        }
		}

		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<EPExpenseClaimDetails>.By<claimDetailCD>
		{
			public static EPExpenseClaimDetails Find(PXGraph graph, string claimDetailCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, claimDetailCD, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Claim
			/// </summary>
			public class Claim : EPExpenseClaim.PK.ForeignKeyOf<EPExpenseClaim>.By<refNbr> { }

			/// <summary>
			/// Project/Contract
			/// </summary>
			public class Project : PMProject.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<contractID> { }

			/// <summary>
			/// Project Task
			/// </summary>
			public class ProjectTask : PMTask.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<contractID, taskID> { }

			/// <summary>
			/// Cost Code
			/// </summary>
			public class CostCode : PMCostCode.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<costCodeID> { }

			/// <summary>
			/// Item
			/// </summary>
			public class Item : InventoryItem.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<inventoryID> { }

			/// <summary>
			/// Branch
			/// </summary>
			public class Branch : GL.Branch.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<branchID> { }

			/// <summary>
			/// Claimed By 
			/// </summary>
			public class Employee : EPEmployee.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<employeeID> { }

			/// <summary>
			/// Owner
			/// </summary>
			public class OwnerContact : Contact.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<ownerID> { }

			/// <summary>
			/// Tax Zone
			/// </summary>
			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<taxZoneID> { }

			/// <summary>
			/// Tax Category
			/// </summary>
			public class TaxCategory : TX.TaxCategory.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<taxCategoryID> { }

			/// <summary>
			/// Tip Tax Category
			/// </summary>
			public class TipTaxCategory : TX.TaxCategory.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<taxTipCategoryID> { }

			/// <summary>
			/// Customer
			/// </summary>
			public class Customer : AR.Customer.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<customerID> { }

			/// <summary>
			/// Customer Location
			/// </summary>
			public class CustomerLocation : CR.Location.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<customerID, customerLocationID> { }

			/// <summary>
			/// Expense Account
			/// </summary>
			public class ExpenseAccount : Account.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<expenseAccountID> { }

			/// <summary>
			/// Expense Subaccount
			/// </summary>
			public class ExpenseSubaccount : Sub.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<expenseSubID> { }

			/// <summary>
			/// Sales Account
			/// </summary>
			public class SalesAccount : Account.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<salesAccountID> { }

			/// <summary>
			/// Sales Subaccount
			/// </summary>
			public class SalesSubaccount : Sub.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<salesSubID> { }

			/// <summary>
			/// AR Invoice
			/// </summary>
			public class Invoice : ARInvoice.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<aRDocType, aRRefNbr> { }

			/// <summary>
			/// AP Bill
			/// </summary>
			public class Bill : APInvoice.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<aPDocType, aPRefNbr> { }

			/// <summary>
			/// AP Bill Line
			/// </summary>
			public class BillLine : APTran.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<aPDocType, aPRefNbr, aPLineNbr> { }

			/// <summary>
			/// Corporate CC
			/// </summary>
			public class CorporateCC : CACorpCard.PK.ForeignKeyOf<EPExpenseClaimDetails>.By<corpCardID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get;
			set;
		}
		#endregion

		#region ClaimDetailID
		public abstract class claimDetailID : PX.Data.BQL.BqlInt.Field<claimDetailID> { }
		/// <summary>
		/// The identifier of the receipt record in the system, which the system assigns automatically when you save a newly entered receipt.
		/// This field is the key field.
		/// </summary>
		[PXDBIdentity]
		[PXUIField(DisplayName = "Receipt Number", Visibility = PXUIVisibility.Visible)]
		public virtual int? ClaimDetailID
		{
			get;
			set;
		}
		#endregion

		#region ClaimDetailCD
		public abstract class claimDetailCD : PX.Data.BQL.BqlString.Field<claimDetailCD>
		{
			public class NumberingAttribute : AutoNumberAttribute
			{
				public NumberingAttribute(Type setupField, Type dateField) : base(setupField, dateField) { }

				public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
				{
					var row = (EPExpenseClaimDetails)e.Row;
					if (sender.GetStatus(row) == PXEntryStatus.Inserted)
					{
						base.RowPersisting(sender, e);
						string newValue = (string)sender.GetValue(e.Row, _FieldOrdinal);

						if (!string.IsNullOrEmpty(newValue))
						{
							// Acuminator disable once PX1043 SavingChangesInEventHandlers [ReceiptNumber need to be update if it duplicated, when persisting]
							CheckDuplicateAndAssignNextNumber(sender, row, newValue);
						}
					}
				}

				private void CheckDuplicateAndAssignNextNumber(PXCache sender, EPExpenseClaimDetails row, string newValue)
						{
							EPExpenseClaimDetails duplicate = PXSelectReadonly<EPExpenseClaimDetails, Where<EPExpenseClaimDetails.claimDetailCD, Equal<Required<EPExpenseClaimDetails.claimDetailCD>>>>.Select(sender.Graph, newValue);
							if (duplicate != null)
							{
								EPSetup setup = PXSelectReadonly<EPSetup>.Select(sender.Graph);
						string nextNumber = GetNextNumber(sender, row, setup.ReceiptNumberingID, row.ExpenseDate);
						CheckDuplicateAndAssignNextNumber(sender, row, nextNumber);
						}
					else
					{
						sender.SetValue(row, _FieldName, newValue);
					}
				}
			}
		}
		/// <summary>
		/// The user-friendly unique identifier of the receipt.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Receipt Number", Visibility = PXUIVisibility.Visible)]
		[claimDetailCD.Numbering(typeof(EPSetup.receiptNumberingID), typeof(EPExpenseClaimDetails.expenseDate))]
		[PXDefault]
		[EPExpenceReceiptSelector]
		public virtual string ClaimDetailCD
		{
			get;
			set;
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// The company branch that will incur the expenses. If multiple expense receipts associated with different branches are added to one expense claim, 
		/// the branch specified for the claim on the Financial Details tab of the Expense Claim (EP301000) form (which corresponds to the <see cref="ExpenseClaimEntry"/> graph)
		/// will reimburse the expenses and the branches specified in this box for the receipts will incur the expenses.
		/// </summary>
		[Branch(typeof(EPExpenseClaim.branchID))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		/// <summary>
		/// The reference number, which usually matches the number of the original receipt.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXDBDefault(typeof(EPExpenseClaim.refNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<EPExpenseClaim, Where<EPExpenseClaim.refNbr, Equal<Current2<refNbr>>>>))]
		[PXUIField(DisplayName = "Expense Claim Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<EPExpenseClaim.refNbr>), DescriptionField = typeof(EPExpenseClaim.docDesc), ValidateValue = false, DirtyRead = true)]
		[PXFieldDescription]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion

		#region RefNbrNotFiltered
		public abstract class refNbrNotFiltered : PX.Data.BQL.BqlString.Field<refNbrNotFiltered> { }
		/// <summary>
		/// The service field that is used in PXFormula for the <see cref="HoldClaim"/> and <see cref="StatusClaim"/> fields, which don't need restrictions of original RefNbr in a selector.
		/// </summary>
		[PXString(15, IsUnicode = true)]
		[PXFormula(typeof(Current<EPExpenseClaimDetails.refNbr>))]
		[PXVirtualSelector(typeof(Search<EPExpenseClaim.refNbr>), ValidateValue = false)]
		public virtual String RefNbrNotFiltered
		{
			get
			{
				return RefNbr;
			}

			set
			{

			}
		}
		#endregion

		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		/// <summary>
		/// The identifier of the <see cref="EPEmployee">employee</see> who is claiming the expenses.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="EPEmployee.bAccountID"/> field.
		/// </value>
		[PXDBInt]
		[PXDefault(typeof(EPExpenseClaim.employeeID))]
		[PXSubordinateAndWingmenSelector]
		[PXUIField(DisplayName = "Claimed by", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXForeignReference(typeof(Field<employeeID>.IsRelatedTo<BAccount.bAccountID>))]
		public virtual int? EmployeeID
		{
			get;
			set;
		}
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		/// <summary>
		/// The <see cref="Contact">Contact</see> responsible 
		/// for the document approval process.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Contact.ContactID"/> field.
		/// </value>
		[PX.TM.Owner]
		public virtual int? OwnerID
		{
			get;
			set;
		}
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		/// <summary>
		/// The workgroup that is responsible for the document approval process.
		/// </summary>
		[PXDBInt]
		[PX.TM.PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup")]
		public virtual int? WorkgroupID
		{
			get;
			set;
		}
		#endregion

		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the expense receipt has the On Hold <see cref="EPExpenseClaimDetails.Status">status</see>,
		/// which means that the receipt can be edited but cannot be added to a claim and released.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(Visible = false)]
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion

		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the expense receipt has been approved by a responsible person
		/// and has the Approved <see cref="EPExpenseClaimDetails.Status">status</see>.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Approved
		{
            get;
            set;
		}
		#endregion

		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the expense receipt has been rejected by a responsible person.
		/// When the receipt is rejected, its <see cref="EPExpenseClaimDetails.Status">status</see> changes to Rejected.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public bool? Rejected
		{
			get;
			set;
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		/// <summary>
		/// The identifier of the <see cref="PX.Objects.CM.CurrencyInfo">CurrencyInfo</see> object associated with the document.
		/// </summary>
		/// <value>
		/// Is generated automatically and corresponds to the <see cref="PX.Objects.CM.CurrencyInfo.CurrencyInfoID"/> field.
		/// </value>
		[PXDBLong]
		[CurrencyInfo(CuryIDField = "CuryID", CuryDisplayName = "Currency")]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

        /// <summary>
        /// The code of the <see cref="PX.Objects.CM.Currency">currency</see> of the document.
        /// By default, the receipt currency is the currency specified as the default for the employee.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="Company.BaseCuryID">company's base currency</see>.
        /// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXDefault(typeof(Current<AccessInfo.baseCuryID>))]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isActive, Equal<True>>>))]
		[PXUIField(DisplayName = "Currency")]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion

		#region CardCuryInfoID
		public abstract class cardCuryInfoID : PX.Data.BQL.BqlLong.Field<cardCuryInfoID> { }

		/// <summary>
		/// The identifier of the <see cref="CurrencyInfo">CurrencyInfo</see> record that stores
		/// exchange rate from the <see cref="CardCuryID">corporate card currency</see> to the base currency.
		/// </summary>
		[PXDBLong]
		[CurrencyInfo(CuryIDField = nameof(CardCuryID))]
		public virtual long? CardCuryInfoID { get; set; }
		#endregion

		#region CardCuryID
		public abstract class cardCuryID : PX.Data.BQL.BqlString.Field<cardCuryID> { }

		/// <summary>
		/// The currency of the corporate card.
		/// </summary>
		[PXUIField(Enabled = false)]
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		public virtual string CardCuryID { get; set; }
		#endregion

		#region ClaimCuryInfoID
		public abstract class claimCuryInfoID : PX.Data.BQL.BqlLong.Field<claimCuryInfoID> { }
		/// <summary>
		/// The code of the <see cref="PX.Objects.CM.Currency">currency</see> of the claim in which the current receipt is included.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">company's base currency</see>.
		/// </value>
		[PXDBLong]
		[CurrencyInfo(typeof(EPExpenseClaim.curyInfoID), CuryIDField = "ClaimCuryID", CuryDisplayName = "Claim Currency")]
		public virtual long? ClaimCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region ExpenseDate
		public abstract class expenseDate : PX.Data.BQL.BqlDateTime.Field<expenseDate> { }
		/// <summary>
		/// The date of the receipt. By default, the current business date is used when a new receipt is created.
		/// </summary>
		[PXDBDate]
		[PXDefault(typeof(Search<EPExpenseClaim.docDate, Where<EPExpenseClaim.refNbr, Equal<Current<EPExpenseClaimDetails.refNbr>>>>))]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? ExpenseDate
		{
			get;
			set;
		}
		#endregion

		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		/// <summary>
		/// The total amount of taxes associated with the document in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(taxTotal))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		/// <summary>
		/// The total amount of taxes associated with the document 
		/// in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxTipTotal
		public abstract class curyTaxTipTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTipTotal> { }
		/// <summary>
		/// A fake field for correct working a Tax attribute. Always must be zero because a tip is tax exempt.
		/// The total amount of tips taxes associated with the document in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXCurrency(typeof(curyInfoID), typeof(taxTipTotal))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryTaxTipTotal
		{
			get;
			set;
		}
		#endregion
		#region TaxTipTotal
		public abstract class taxTipTotal : PX.Data.BQL.BqlDecimal.Field<taxTipTotal> { }
		/// <summary>
		/// A fake field for correct working a Tax attribute. Always must be zero because a tip is tax exempt. 
		/// The total amount of tips taxes associated with the document 
		/// in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? TaxTipTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxRoundDiff
		public abstract class curyTaxRoundDiff : PX.Data.BQL.BqlDecimal.Field<curyTaxRoundDiff> { }

		/// <summary>
		/// The difference between the original document amount and the rounded amount in the selected currency.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(taxRoundDiff), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Discrepancy", Enabled = false)]
		public decimal? CuryTaxRoundDiff
		{
			get;
			set;
		}
		#endregion
		#region TaxRoundDiff
		public abstract class taxRoundDiff : PX.Data.BQL.BqlDecimal.Field<taxRoundDiff> { }

		/// <summary>
		/// The difference between the original document amount and the rounded amount in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? TaxRoundDiff
		{
			get;
			set;
		}
		#endregion

		#region ExpenseRefNbr
		public abstract class expenseRefNbr : PX.Data.BQL.BqlString.Field<expenseRefNbr> { }
		/// <summary>
		/// The reference number, which usually matches the number of the original receipt.
		/// </summary>
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Ref. Nbr.")]
		public virtual string ExpenseRefNbr
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		/// <summary>
		/// The <see cref="InventoryItem">non-stock item</see>  of the expense, which determines the financial accounts, 
		/// the default tax category, and the unit of measure used for the receipt.
		/// </summary>
		[PXDefault]
		[Inventory(DisplayName = "Expense Item")]
		[PXRestrictor(typeof(Where<InventoryItem.itemType, Equal<INItemTypes.expenseItem>>), Messages.InventoryItemIsNotAnExpenseType)]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion

		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

		/// <summary>
		/// The identifier of the <see cref="TaxZone">tax zone</see> associated with the receipt.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone", Required = false)]
		[PXDefault(typeof(Coalesce<Search<EPEmployee.receiptAndClaimTaxZoneID,
										Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaimDetails.employeeID>>>>,
									Search2<Location.vTaxZoneID,
										RightJoin<EPEmployee, On<EPEmployee.defLocationID, Equal<Location.locationID>>>,
										Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaimDetails.employeeID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		public virtual String TaxZoneID
		{
			get;
			set;
		}
		#endregion
		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }

		/// <summary>
		/// The tax calculation mode, which defines which amounts (tax-inclusive or tax-exclusive) 
		/// should be entered in the detail lines of a document. 
		/// This field is displayed only if the <see cref="FeaturesSet.NetGrossEntryMode"/> field is set to <c>true</c>.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"T"</c> (Tax Settings): The tax amount for the document is calculated according to the settings of the applicable tax or taxes.
		/// <c>"G"</c> (Gross): The amount in the document detail line includes a tax or taxes.
		/// <c>"N"</c> (Net): The amount in the document detail line does not include taxes.
		/// </value>
		[PXDBString(1, IsFixed = true)]
        [PXDefault(TaxCalculationMode.TaxSetting)]
        [TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode
		{
			get;
			set;
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		/// <summary>
		/// The tax category associated with the expense item.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="TaxCategory.TaxCategoryID"/> field.
		/// </value>
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.taxCategoryID>))]
		public virtual string TaxCategoryID
		{
			get;
			set;
		}
        #endregion
        #region HasWithHoldTax
        public abstract class hasWithHoldTax : PX.Data.BQL.BqlBool.Field<hasWithHoldTax> { }

		/// <summary>
		/// A Boolean value that indicates whether withholding taxes are applied to the document.
		/// This is a technical field, which is calculated on the fly and is used to restrict the values of the <see cref="TaxCalcMode"/> field.
		/// </summary>
        [PXBool]
        [RestrictWithholdingTaxCalcMode(typeof(taxZoneID), typeof(taxCalcMode))]
        public virtual bool? HasWithHoldTax
        {
            get;
            set;
        }
        #endregion
        #region HasUseTax
        public abstract class hasUseTax : PX.Data.BQL.BqlBool.Field<hasUseTax> { }

		/// <summary>
		/// A Boolean value that indicates whether use taxes are applied to the document.
		/// This is a technical field, which is calculated on the fly and is used to restrict the values of the <see cref="TaxCalcMode"/> field.
		/// </summary>
        [PXBool]
        [RestrictUseTaxCalcMode(typeof(taxZoneID), typeof(taxCalcMode))]
        public virtual bool? HasUseTax
        {
            get;
            set;
        }
        #endregion

        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		/// <summary>
		/// The <see cref="INUnit">unit of measure</see> of the expense item.
		/// </summary>
		[PXDefault]
		[INUnit(typeof(EPExpenseClaimDetails.inventoryID), DisplayName = "UOM")]
		[PXUIEnabled(typeof(Where<inventoryID, IsNotNull, And<FeatureInstalled<FeaturesSet.multipleUnitMeasure>>>))]
		[PXFormula(typeof(Switch<Case<Where<inventoryID, IsNull>, Null>, Selector<inventoryID, InventoryItem.purchaseUnit>>))]
		public virtual string UOM
		{
			get;
			set;
		}
		#endregion
		#region CuryTipAmt
		public abstract class curyTipAmt : PX.Data.BQL.BqlDecimal.Field<curyTipAmt> { }

		/// <summary>
		/// The amount of non-taxable tips in the document currency that will not be included in the tax base of the receipt.
		/// </summary>
		[PXDBCurrency(typeof(EPExpenseClaimDetails.curyInfoID), typeof(EPExpenseClaimDetails.tipAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tip Amount")]
		[EPTaxTip]
		public virtual decimal? CuryTipAmt
		{
			get;
			set;
		}
		#endregion
		#region TipAmt
		public abstract class tipAmt : PX.Data.BQL.BqlDecimal.Field<tipAmt> { }

		/// <summary>
		/// The amount of non-taxable tips in the base currency of the tenant that will not be included in the tax base of the receipt.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Tip Amount")]
		public virtual decimal? TipAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxTipCategoryID
		public abstract class taxTipCategoryID : PX.Data.BQL.BqlString.Field<taxTipCategoryID> { }
		/// <summary>
		/// The tax category associated with the tip item.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="TaxCategory.TaxCategoryID"/> field.
		/// </value>
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		public virtual string TaxTipCategoryID
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		/// <summary>
		/// The quantity of the expense item that the employee purchased according to the receipt. 
		/// The quantity is expressed in the <see cref="INUnit">unit of measure</see> specified 
		/// for the selected expense <see cref="InventoryItem">non-stock item</see>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "1.0")]
		[PXUIField(DisplayName = "Quantity", Visibility = PXUIVisibility.Visible)]
		[PXUIVerify(typeof(Where<qty, NotEqual<decimal0>, Or<Selector<contractID, Contract.nonProject>, Equal<True>>>), PXErrorLevel.Error, Messages.ValueShouldBeNonZero)]
		public virtual decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitCost
		public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }
		/// <summary>
		/// The cost of one unit of the expense item in the <see cref="CuryID">currency of the document</see>. 
		/// If a standard cost is specified for the expense <see cref="InventoryItem">non-stock item</see>, the standard cost is used as the default unit cost.
		/// </summary>
		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(EPExpenseClaimDetails.curyInfoID), typeof(EPExpenseClaimDetails.unitCost))]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryUnitCost
		{
			get;
			set;
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		/// <summary>
		/// The cost of one unit of the expense item in the <see cref = "Company.BaseCuryID" />base currency of the company</see>. 
		/// If a standard cost is specified for the expense <see cref="InventoryItem">non-stock item</see>, the standard cost is used as the default unit cost.
		/// </summary>
		[PXDBPriceCost]
		[PXDefault(typeof(Search<INItemCost.lastCost, Where<INItemCost.inventoryID, Equal<Current<EPExpenseClaimDetails.inventoryID>>>>))]
		public virtual decimal? UnitCost
		{
			get;
			set;
		}
		#endregion
		#region CuryEmployeePart
		public abstract class curyEmployeePart : PX.Data.BQL.BqlDecimal.Field<curyEmployeePart> { }
		/// <summary>
		/// The part of the total amount that will not be paid back to the employee in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(EPExpenseClaimDetails.curyInfoID), typeof(EPExpenseClaimDetails.employeePart), MinValue = 0)]
		[PXUIField(DisplayName = "Employee Part")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIEnabled(typeof(Where<curyExtCost, GreaterEqual<decimal0>>))]
		[PXFormula(typeof(Switch<Case<Where<curyExtCost, Less<decimal0>>, decimal0>, curyEmployeePart>))]
		[PXUIVerify(
			typeof(Where<curyEmployeePart, Equal<decimal0>,
						Or<curyEmployeePart, Less<decimal0>,
						And<curyEmployeePart, GreaterEqual<curyExtCost>,
						Or<curyEmployeePart, Greater<decimal0>,
						And<curyEmployeePart, LessEqual<curyExtCost>>>>>>), PXErrorLevel.Error, Messages.EmployeePartExceed)]
		[PXUIVerify(
			typeof(Where<curyEmployeePart, Equal<decimal0>,
						Or<curyEmployeePart, Greater<decimal0>,
						And<curyExtCost, Greater<decimal0>,
						Or<curyEmployeePart, Less<decimal0>,
						And<curyExtCost, Less<decimal0>>>>>>), PXErrorLevel.Error, Messages.EmployeePartSign)]
		public virtual decimal? CuryEmployeePart
		{
			get;
			set;
		}
		#endregion
		#region EmployeePart
		public abstract class employeePart : PX.Data.BQL.BqlDecimal.Field<employeePart> { }
		/// <summary>
		/// The part of the total amount that will not be paid back to the employee in the <see cref = "Company.BaseCuryID" />base currency of the company</see>. 
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Employee Part")]
		public virtual decimal? EmployeePart
		{
			get;
			set;
		}
		#endregion
		#region CuryExtCost
		public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }
		/// <summary>
		/// The total amount of the receipt in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(extCost))]
		[PXUIField(DisplayName = "Amount")]
		[PXFormula(typeof(Mult<qty, curyUnitCost>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryExtCost
		{
			get;
			set;
		}
		#endregion
		#region ExtCost
		public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }
		/// <summary>
		/// The total amount of the receipt in the <see cref = "Company.BaseCuryID" />base currency of the company</see>. 
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Total Amount")]
		public virtual decimal? ExtCost
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }

		/// <summary>
		/// The tax amount to be paid for the document in the document currency.
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
		/// The tax amount to be paid for the document in the base currency of the tenant.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxableAmtFromTax
		public abstract class curyTaxableAmtFromTax : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmtFromTax> { }

		/// <summary>
		/// The taxable amount in the currency of the document.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(taxableAmtFromTax))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTaxableAmtFromTax
		{
			get;
			set;
		}
		#endregion
		#region TaxableAmtFromTax
		public abstract class taxableAmtFromTax : PX.Data.BQL.BqlDecimal.Field<taxableAmtFromTax> { }

		/// <summary>
		/// The taxable amount in the base currency of the tenant.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxableAmtFromTax
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }

		/// <summary>
		/// The taxable amount in the currency of the document.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(taxableAmt))]
		[PXFormula(typeof(Sub<curyExtCost, curyEmployeePart>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }

		/// <summary>
		/// The taxable amount in the base currency of the tenant.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

        /// <summary>
        /// The amount to be reimbursed to the employee, which is calculated as the difference between the total amount 
        /// and the employee part in the <see cref="CuryID">currency of the document</see>.
        /// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(tranAmt))]
		[PXFormula(typeof(Add<curyTaxableAmt, curyTipAmt>))]
		[PXUIField(DisplayName = "Claim Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTranAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmtWithTaxes
		public abstract class curyTranAmtWithTaxes : PX.Data.BQL.BqlDecimal.Field<curyTranAmtWithTaxes> { }

		/// <summary>
		/// The amount to be reimbursed to the employee in the currency of the document.
		/// </summary>
		[PXDBCurrency(typeof(EPExpenseClaimDetails.curyInfoID), typeof(EPExpenseClaimDetails.tranAmtWithTaxes))]
		[PXFormula(typeof(Add<curyAmountWithTaxes, curyTipAmt>))]
		[PXUIField(DisplayName = "Claim Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTranAmtWithTaxes
		{
			get;
			set;
		}
		#endregion
		#region CuryAmountWithTaxes
		public abstract class curyAmountWithTaxes : PX.Data.BQL.BqlDecimal.Field<curyAmountWithTaxes> { }

		/// <summary>
		/// The amount of the expense receipt with taxes in the currency of the document.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryAmountWithTaxes
		{
			get;
			set;
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		/// <summary>
		/// The amount to be reimbursed to the employee, which is calculated as the difference between the total amount 
		/// and the employee part in the <see cref = "Company.BaseCuryID" >base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Claim Amount")]
		public virtual decimal? TranAmt
		{
			get;
			set;
		}
		#endregion
		#region TranAmtWithTaxes
		public abstract class tranAmtWithTaxes : PX.Data.BQL.BqlDecimal.Field<tranAmtWithTaxes> { }

		/// <summary>
		/// The amount to be reimbursed to the employee in the base currency of the tenant.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Claim Amount with Taxes")]
		public virtual decimal? TranAmtWithTaxes
		{
			get;
			set;
		}
		#endregion
		#region ClaimCuryTranAmt
		public abstract class claimCuryTranAmt : PX.Data.BQL.BqlDecimal.Field<claimCuryTranAmt> { }
        /// <summary>
        /// The amount to be reimbursed to the employee, which is calculated as the difference between the total amount 
        /// and the employee part in the <see cref = "Company.BaseCuryID">currency of the claim</see> in which the current receipt is included.
        /// </summary>
		[PXDBCurrency(typeof(claimCuryInfoID), typeof(claimTranAmt))]
		[PXUIField(DisplayName = "Amount in Claim Curr.", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ClaimCuryTranAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryNetAmount
		public abstract class curyNetAmount : PX.Data.BQL.BqlDecimal.Field<curyNetAmount> { }

		/// <summary>
		/// The net amount in the currency of the document.
		/// </summary>
		[PXCurrency(typeof(curyInfoID), typeof(netAmount))]
		[PXFormula(typeof(Sub<curyTranAmtWithTaxes, curyTaxTotal>))]
		[PXUIField(DisplayName = "Net Amount", Enabled = false, Visible = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryNetAmount
		{
			get;
			set;
		}
		#endregion
		#region NetAmount
		public abstract class netAmount : PX.Data.BQL.BqlDecimal.Field<netAmount> { }

		/// <summary>
		/// The net amount in the base currency of the tenant.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? NetAmount
		{
			get;
			set;
		}
		#endregion

		#region ClaimTranAmt
		public abstract class claimTranAmt : PX.Data.BQL.BqlDecimal.Field<claimTranAmt> { }
		/// <summary>
		/// The amount to be reimbursed to the employee, which is calculated as the difference between the total amount 
		/// and the employee part in the <see cref = "Company.BaseCuryID">base currency of the company of the claim</see> in which the current receipt is included.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount in Claim Original")]
		public virtual decimal? ClaimTranAmt
		{
			get;
			set;
		}
		#endregion

		#region ClaimCuryTranAmtWithTaxes
		public abstract class claimCuryTranAmtWithTaxes : PX.Data.BQL.BqlDecimal.Field<claimCuryTranAmtWithTaxes> { }

		/// <summary>
		/// The amount claimed by the employee, which is expressed in the currency of the expense claim.
		/// </summary>
		[PXDBCurrency(typeof(claimCuryInfoID), typeof(claimTranAmtWithTaxes))]
		[PXUIField(DisplayName = "Amount in Claim Curr.", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ClaimCuryTranAmtWithTaxes
		{
			get;
			set;
		}
		#endregion
		#region ClaimTranAmtWithTaxes
		public abstract class claimTranAmtWithTaxes : PX.Data.BQL.BqlDecimal.Field<claimTranAmtWithTaxes> { }

		/// <summary>
		/// The amount (in the base currency of the tenant) that the employee has specified
		/// in the claim in which the current receipt is included.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount in Claim Original")]
		public virtual decimal? ClaimTranAmtWithTaxes
		{
			get;
			set;
		}
		#endregion

		#region ClaimCuryTaxTotal
		public abstract class claimCuryTaxTotal : PX.Data.BQL.BqlDecimal.Field<claimCuryTaxTotal> { }

		/// <summary>
		/// The total amount (in the currency of the expense claim) of taxes that are associated with the document.
		/// </summary>
		[PXDBCurrency(typeof(claimCuryInfoID), typeof(taxTotal), BaseCalc = false)]
		[PXUIField(DisplayName = "Amount in Claim Curr.", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ClaimCuryTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region ClaimTaxTotal
		public abstract class claimTaxTotal : PX.Data.BQL.BqlDecimal.Field<claimTaxTotal> { }

		/// <summary>
		/// The total amount of taxes (in the base currency of the tenant)
		/// for the claim in which the current receipt is included.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount in Claim Original")]
		public virtual decimal? ClaimTaxTotal
		{
			get;
			set;
		}
		#endregion

		#region ClaimCuryTaxRoundDiff
		public abstract class claimCuryTaxRoundDiff : PX.Data.BQL.BqlDecimal.Field<claimCuryTaxRoundDiff> { }

		/// <summary>
		/// The difference between the original document amount and the rounded amount in the expense claim currency.
		/// </summary>
		[PXDBCurrency(typeof(claimCuryInfoID), typeof(claimTaxRoundDiff))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ClaimCuryTaxRoundDiff
		{
			get;
			set;
		}
		#endregion
		#region ClaimTaxRoundDiff
		public abstract class claimTaxRoundDiff : PX.Data.BQL.BqlDecimal.Field<claimTaxRoundDiff> { }

		/// <summary>
		/// The difference between the original document amount and the rounded amount in the
		/// base currency of the tenant for the claim in which the current receipt is included.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ClaimTaxRoundDiff
		{
			get;
			set;
		}
		#endregion

		#region ClaimCuryVatExemptTotal
		public abstract class claimCuryVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<claimCuryVatExemptTotal> { }

		/// <summary>
		/// The document total that is exempt from VAT in the expense claim currency. 
		/// This total is calculated as the taxable amount for the tax with the <see cref="Tax.ExemptTax"/> field set to <see langword="true" />
		/// (that is, with the Include in VAT Exempt Total check box selected on the Taxes (TX205000) form).
		/// </summary>
		[PXDBCurrency(typeof(claimCuryInfoID), typeof(claimVatExemptTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ClaimCuryVatExemptTotal
		{
			get;
			set;
		}
		#endregion
		#region ClaimVatExemptTaxTotal
		public abstract class claimVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<claimVatExemptTotal> { }

		/// <summary>
		/// The document total that is exempt from VAT in the base currency
		/// of the tenant for the claim in which the current receipt is included.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ClaimVatExemptTotal
		{
			get;
			set;
		}
		#endregion
		#region ClaimCuryVatTaxableTotal
		public abstract class claimCuryVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<claimCuryVatTaxableTotal> { }

		/// <summary>
		/// The document total that is subjected to VAT in the expense claim currency.
		/// The field is displayed only if the <see cref="Tax.IncludeInTaxable"/> field is set to <see langword="true" />
		/// (that is, the Include in VAT Exempt Total check box is selected on the Taxes (TX205000) form).
		/// </summary>
		[PXDBCurrency(typeof(claimCuryInfoID), typeof(claimVatTaxableTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ClaimCuryVatTaxableTotal
		{
			get;
			set;
		}
		#endregion
		#region ClaimVatTaxableTotal
		public abstract class claimVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<claimVatTaxableTotal> { }

		/// <summary>
		/// The document total that is subjected to VAT in the base currency
		/// of the tenant for the claim in which the current receipt is included.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ClaimVatTaxableTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
		
		/// <summary>
		/// The document total that is exempt from VAT in the selected currency. 
		/// This total is calculated as the taxable amount for the tax 
		/// with the <see cref="Tax.ExemptTax"/> field set to <see langword="true" /> (that is, with the Include in VAT Exempt Total check box selected on the Taxes (TX205000) form).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(vatExemptTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryVatExemptTotal
		{
			get;
			set;
		}
		#endregion
		#region VatExemptTaxTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }
		
		/// <summary>
		/// The document total that is exempt from VAT in the base currency of the tenant.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? VatExemptTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }
		
		/// <summary>
		/// The document total that is subjected to VAT in the selected currency.
		/// The field is displayed only if 
		/// the <see cref="Tax.IncludeInTaxable"/> field is set to <see langword="true" /> (that is, the Include in VAT Exempt Total check box is selected on the Taxes (TX205000) form).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(vatTaxableTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryVatTaxableTotal
		{
			get;
			set;
		}
		#endregion
		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }
		
		/// <summary>
		/// The document total that is subjected to VAT in the base currency of the tenant.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? VatTaxableTotal
		{
			get;
			set;
		}
		#endregion

		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		/// <summary>
		/// The description of the expense.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		public virtual string TranDesc
		{
			get;
			set;
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		/// <summary>
		/// The <see cref = "Customer.bAccountID" />customer</see>, which should be specified if the employee incurred the expenses while working for a particular customer. 
		/// If a contract or project is selected, the customer associated with this contract or project is automatically filled in and the box becomes read-only.
		/// </summary>
		[PXDefault(typeof(EPExpenseClaim.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[CustomerActive(DescriptionField = typeof(Customer.acctName))]
		[PXUIRequired(typeof(billable))]
		[PXUIEnabled(typeof(Where<contractID, IsNull, Or<Selector<contractID, Contract.nonProject>, Equal<True>, Or<Selector<contractID, Contract.customerID>, IsNull>>>))]
		[PXUIVerify(typeof(Where<refNbr, IsNull,
								Or<Current2<customerID>, IsNull,
								Or<Selector<refNbr, EPExpenseClaim.customerID>, IsNull,
								Or<Current2<customerID>, Equal<Selector<refNbr, EPExpenseClaim.customerID>>,
								Or<billable, NotEqual<True>,
								Or<Selector<contractID, Contract.nonProject>, Equal<False>>>>>>>),
					PXErrorLevel.Warning, Messages.CustomerDoesNotMatch)]
		[PXFormula(typeof(Switch<Case<Where<Selector<contractID, Contract.nonProject>, Equal<False>>, Selector<contractID, Contract.customerID>>, Null>))]
		public virtual int? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        /// <summary>
        /// The location of the customer related to the expenses.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="Location.LocationID"/> field.
        /// </value>
		[PXDefault(typeof(Search<Customer.defLocationID, 
			Where<Customer.bAccountID, Equal<Current<EPExpenseClaimDetails.customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(billable))]
		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current2<customerID>>>), DescriptionField = typeof(Location.descr))]
		[PXUIEnabled(typeof(Where<Current2<customerID>, IsNotNull, And<Where<contractID, IsNull, 
			Or<Selector<contractID, Contract.nonProject>, Equal<True>, Or<Selector<contractID, Contract.customerID>, IsNull>>>>>))]
		[PXFormula(typeof(Switch<Case<Where<Current2<customerID>, IsNull>, Null>, Selector<customerID, Customer.defLocationID>>))]
		public virtual int? CustomerLocationID
		{
			get;
			set;
		}
		#endregion
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		/// <summary>
		/// The <see cref = "Contract.ContractID">project or contract</see>, which should be specified if the 
		/// employee incurred the expenses while working on a particular project or contract. 
		/// The value of this field can be specified only if the Project Accounting or Contract Management feature, 
		/// respectively, is enabled on the Enable/Disable Features (CS100000) form.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Project/Contract")]
		[PXDimensionSelector(ContractAttribute.DimensionName,
							typeof(Search2<Contract.contractID,
										   LeftJoin<EPEmployeeContract,
													On<EPEmployeeContract.contractID, Equal<Contract.contractID>,
													And<EPEmployeeContract.employeeID, Equal<Current2<employeeID>>>>>,
										   Where<Contract.isActive, Equal<True>,
												 And<Contract.isCompleted, Equal<False>,
												 And<Where<Contract.nonProject, Equal<True>,
														   Or2<Where<Contract.baseType, Equal<CTPRType.contract>,
														   And<FeatureInstalled<FeaturesSet.contractManagement>>>,
														   Or<Contract.baseType, Equal<CTPRType.project>,
                                                           And2<Where<Contract.visibleInEA, Equal<True>>, 
														   And2<FeatureInstalled<FeaturesSet.projectModule>,
														   And2<Match<Current<AccessInfo.userName>>,
														   And<Where<Contract.restrictToEmployeeList, Equal<False>,
														   Or<EPEmployeeContract.employeeID, IsNotNull>>>>>>>>>>>>,
										   OrderBy<Desc<Contract.contractCD>>>),
							 typeof(Contract.contractCD),
							 typeof(Contract.contractCD),
							 typeof(Contract.description),
							 typeof(Contract.customerID),
							 typeof(Contract.status),
							 Filterable = true,
							 ValidComboRequired = true,
							 CacheGlobal = true,
							 DescriptionField = typeof(Contract.description))]
		[ProjectDefault(BatchModule.EA, AccountType = typeof(expenseAccountID))]

		public virtual int? ContractID
		{
			get;
			set;
		}
		#endregion


		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		/// <summary>
		/// The <see cref="PMTask">project task</see> to which the expenses are related. 
		/// This box is available only if the Project Management feature is enabled on the Enable/Disable Features (CS100000) form.
		/// </summary>
		///
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<contractID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[EPExpenseAllowProjectTaskAttribute(typeof(EPExpenseClaimDetails.contractID), BatchModule.EA, DisplayName = "Project Task")]
		[PXUIEnabled(typeof(Where<contractID, IsNotNull, And<Selector<contractID, Contract.baseType>, Equal<CTPRType.project>>>))]
		[PXFormula(typeof(Switch<Case<Where<contractID, IsNull, Or<Selector<contractID, Contract.baseType>, NotEqual<CTPRType.project>>>, Null>, taskID>))]
		[PXForeignReference(typeof(CompositeKey<Field<contractID>.IsRelatedTo<PMTask.projectID>, Field<taskID>.IsRelatedTo<PMTask.taskID>>))]
		public virtual int? TaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;

		/// <summary>
		/// The identifier of the <see cref="PMCostCode">cost code</see> associated with the record.
		/// </summary>
		[CostCode(typeof(expenseAccountID), typeof(taskID), GL.AccountType.Expense, ReleasedField = typeof(released), DescriptionField = typeof(PMCostCode.description))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
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
		#region Billable
		public abstract class billable : PX.Data.BQL.BqlBool.Field<billable> { }
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the customer should be billed for the claim amount. 
		/// You can use the Bill Expense Claims (EP502000) form to bill the customer if no project is specified.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Billable")]
		public virtual bool? Billable
		{
			get;
			set;
		}
		#endregion
		#region Billed
		public abstract class billed : PX.Data.BQL.BqlBool.Field<billed> { }
		/// <summary>
		/// Indicates (if set to <c>true</c>) that current receipt was billed.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Billed")]
		public virtual bool? Billed
		{
			get;
			set;
		}
		#endregion

		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the expense receipt was released 
		/// and has the Released <see cref="EPExpenseClaimDetails.Status">status</see>.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Visible = false)]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion
		#region ExpenseAccountID
		public abstract class expenseAccountID : PX.Data.BQL.BqlInt.Field<expenseAccountID> { }
		/// <summary>
		/// The <see cref="Account">expense account</see> to which the system records the part of the expense to be paid back to the employee.
		/// </summary>
		[PXDefault]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.cOGSAcctID>))]
		[Account(DisplayName = "Expense Account", Visibility = PXUIVisibility.Visible, AvoidControlAccounts = true)]
		[PXUIVerify(typeof(Where<Current2<contractID>, IsNull,
								Or<Current2<expenseAccountID>, IsNull,
								Or<Selector<contractID, Contract.nonProject>, Equal<True>,
								Or<Selector<contractID, Contract.baseType>, Equal<CTPRType.contract>,
								Or<Selector<expenseAccountID, Account.accountGroupID>, IsNotNull>>>>>),
					PXErrorLevel.Error, Messages.AccountGroupIsNotAssignedForAccount, typeof(expenseAccountID))]//, account.AccountCD.Trim())]
		public virtual int? ExpenseAccountID
		{
			get;
			set;
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		/// <summary>
		/// The corresponding <see cref="Sub">subaccount</see> the system uses to record the part of the expense to be paid back to the employee. 
		/// The segments of the expense subaccount are combined according to the settings specified on the Time and Expenses Preferences (EP101000) form.
		/// </summary>
		[PXDefault]
		[PXFormula(typeof(Default<inventoryID, contractID, taskID, customerLocationID>))]
		[PXFormula(typeof(Default<employeeID>))]
		[SubAccount(typeof(EPExpenseClaimDetails.expenseAccountID), typeof(EPExpenseClaimDetails.branchID), DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible)]
		public virtual int? ExpenseSubID
		{
			get;
			set;
		}
		#endregion
		#region SalesAccountID
		public abstract class salesAccountID : PX.Data.BQL.BqlInt.Field<salesAccountID> { }
		/// <summary>
		/// The <see cref="Account">sales account</see> to which the system records the part of the amount to charge the customer for. 
		/// If the <see cref="Billable">Billable</see> check box is selected, the sales account specified for the expense non-stock item is filled in by default.
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(ExpenseClaimDetailSalesAccountID<billable, inventoryID, customerID, customerLocationID>))]
		[PXUIRequired(typeof(billable))]
		[PXUIEnabled(typeof(billable))]
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, AvoidControlAccounts = true)]

		public virtual int? SalesAccountID
		{
			get;
			set;
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		/// <summary>
		/// The corresponding <see cref="Sub">subaccount</see> the system uses to record the amount to charge the customer for. 
		/// If the <see cref="Billable">Billable</see> check box is selected, the sales subaccount specified for the expense non-stock item is filled in by default. 
		/// The segments of the sales subaccount are combined according to the settings specified on the Time and Expenses Preferences (EP101000) form.
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<billable, inventoryID, contractID, taskID>))]
		[PXFormula(typeof(Default<employeeID, customerLocationID>))]
		[SubAccount(typeof(EPExpenseClaimDetails.salesAccountID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible)]
		[PXUIRequired(typeof(billable))]
		[PXUIEnabled(typeof(billable))]
		public virtual int? SalesSubID
		{
			get;
			set;
		}
		#endregion
		#region ARDocType
		public abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
		/// <summary>
		/// The type of AR document created as a result of billing a claim.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ARDocType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[ARDocType.List]
		[PXUIField(DisplayName = "AR Doument Type", Visibility = PXUIVisibility.Visible, Enabled = false, TabOrder = -1)]
		public virtual string ARDocType
		{
			get;
			set;
		}
		#endregion
		#region ARRefNbr
		public abstract class aRRefNbr : PX.Data.BQL.BqlString.Field<aRRefNbr> { }
		/// <summary>
		/// The reference number of the AR document created as a result of billing a claim.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "AR Reference Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<ARInvoice.refNbr, Where<ARInvoice.docType, Equal<Optional<EPExpenseClaimDetails.aRDocType>>>>))]
		public virtual string ARRefNbr
		{
			get;
			set;
		}
		#endregion
		#region APDocType
		public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }
		/// <summary>
		/// The type of ARPdocument created as a result of releasing a claim.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="APDocType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[APDocType.List]
		[PXUIField(DisplayName = "AP Document Type", Visibility = PXUIVisibility.Visible, Enabled = false, TabOrder = -1)]
		public virtual string APDocType
		{
			get;
			set;
		}
		#endregion
		#region APRefNbr
		public abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }
		/// <summary>
		/// The reference number of the AR document created as a result of releasing a claim.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "AP Reference Nbr.", Enabled = false, Visible = false)]
		[PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<Optional<EPExpenseClaimDetails.aPDocType>>>>))]
		public virtual string APRefNbr
		{
			get;
			set;
		}
		#endregion
		#region APLineNbr
		public abstract class aPLineNbr : PX.Data.BQL.BqlInt.Field<aPLineNbr> { }

		/// <summary>
		/// The number of the AP document line created as a result of releasing an expense claim.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "AP Document Line Nbr.")]
		public virtual Int32? APLineNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		/// <summary>
		/// The status of the expense receipt.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="EPExpenseClaimDetailsStatus.ListAttribute"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(EPExpenseClaimDetailsStatus.HoldStatus)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[EPExpenseClaimDetailsStatus.List()]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion
		#region Status Claim
		public abstract class statusClaim : PX.Data.BQL.BqlString.Field<statusClaim> { }
		/// <summary>
		/// The status of the Expense Claim (EP301000) form (which corresponds to the <see cref="ExpenseClaimEntry"/> graph).
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="EPExpenseClaimStatus.ListAttribute"/>.
		/// </value>
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Expense Claim Status", Enabled = false)]
		[EPExpenseClaimStatus.List()]
		[PXFormula(typeof(Switch<Case<Where<EPExpenseClaimDetails.refNbr, IsNotNull>,
							Selector<EPExpenseClaimDetails.refNbrNotFiltered, EPExpenseClaim.status>>,
							Null>))]
		public virtual String StatusClaim
		{
			get;
			set;
		}
		#endregion
		#region Hold Claim
		public abstract class holdClaim : PX.Data.BQL.BqlBool.Field<holdClaim> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the Expense Claim (EP301000) (which corresponds to the <see cref="ExpenseClaimEntry"/> graph) 
		/// has the On Hold <see cref="EPExpenseClaim.Status">status</see>,
		/// which means that user can pick another claim, otherwise user cannot change claim.
		/// </summary>
		[PXBool()]
		[PXFormula(typeof(Switch<Case<Where<EPExpenseClaimDetails.refNbr, IsNotNull>,
									Selector<EPExpenseClaimDetails.refNbrNotFiltered, EPExpenseClaim.hold>>,
									True>))]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual Boolean? HoldClaim
		{
			get;
			set;
		}

		#endregion
		#region CreatedFromClaim
		public abstract class createdFromClaim : PX.Data.BQL.BqlBool.Field<createdFromClaim> { }

		/// <summary>
		/// A Boolean value that indicates whether an expense receipt was created from an expense claim.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Created from Claim", Visible = false)]
		public virtual bool? CreatedFromClaim
		{
			get;
			set;
		}
		#endregion

		#region SubmitedDate
		public abstract class submitedDate : PX.Data.BQL.BqlDateTime.Field<submitedDate> { }

		/// <summary>
		/// The date when the expense receipt was included in the expense claim.
		/// </summary>
		[PXDBDateAndTime]
		public DateTime? SubmitedDate
		{
			get;
			set;
		}
        #endregion

        #region LegacyReceipt
        public abstract class legacyReceipt : PX.Data.BQL.BqlBool.Field<legacyReceipt> { }

		/// <summary>
		/// A Boolean value that indicates whether the expense receipt was created in a previous version of Acumatica ERP.
		/// Taxes for this receipt will be calculated on update of the Tax Zone, Tax Category, Tax Calculation Mode, or Amount fields.
		/// </summary>
        [PXDBBool]
        [PXDefault(false)]
        public bool? LegacyReceipt
        {
            get;
            set;
        }
        #endregion

        #region CorpCardID

		/// <summary>
		/// The identifier of the <see cref="CACorpCard">corporate card</see> that is used to pay the expense receipt.
		/// </summary>
        [PXDBInt]
	    [PXUIField(DisplayName = "Corporate Card")]
        [PXRestrictor(typeof(Where<CACorpCard.isActive, Equal<True>>), CA.Messages.CorpCardIsInactive)]
	    [PXSelector(typeof(Search<CACorpCard.corpCardID>),
			typeof(CACorpCard.corpCardCD), typeof(CACorpCard.name), typeof(CACorpCard.cardNumber), typeof(Account.curyID),
			SubstituteKey = typeof(CACorpCard.corpCardCD),
			DescriptionField = typeof(CACorpCard.name))]
        public int? CorpCardID { get; set; }
        public abstract class corpCardID : BqlInt.Field<corpCardID>{}
        #endregion

        #region PaidWith

		/// <summary>
		/// The way the expense receipt has been paid.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <list>
		/// <item><description><c>PersAcc</c>: Personal Account</description></item>,
		/// <item><description><c>CardComp</c>: Corporate Card, Company Expense</description></item>,
		/// <item><description><c>CardPers</c>: Corporate Card, Personal Expense</description></item>
		/// </list>
		/// </value>
        [PXUIField(DisplayName = "Paid With")]
        [PXDefault(paidWith.PersonalAccount)]
        [PXDBString(8)]
        [LabelList(typeof(paidWith.Labels))]
        public virtual string PaidWith { get; set; }

        public abstract class paidWith : BqlInt.Field<paidWith>
        {
            public const string PersonalAccount = "PersAcc";
            public const string CardCompanyExpense = "CardComp";
            public const string CardPersonalExpense = "CardPers";

            public class cash : PX.Data.BQL.BqlString.Constant<cash>
            {
                public cash() : base(PersonalAccount) {; }
            }

            public class cardCompanyExpense : PX.Data.BQL.BqlString.Constant<cardCompanyExpense>
            {
                public cardCompanyExpense() : base(CardCompanyExpense) {; }
            }

            public class cardPersonalExpense : PX.Data.BQL.BqlString.Constant<cardPersonalExpense>
            {
                public cardPersonalExpense() : base(CardPersonalExpense) {; }
            }

            public class Labels : ILabelProvider
            {
                private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
                {
                    { PersonalAccount, Messages.PersonalAccount },
                    { CardCompanyExpense, Messages.CorpCardCompanyExpense },
                    { CardPersonalExpense, Messages.CorpCardPersonalExpense },
                };

                public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
            }
        }
		#endregion

		public virtual bool IsPaidWithCard => PaidWith == paidWith.CardCompanyExpense || PaidWith == paidWith.CardPersonalExpense;

		#region BankTranDate
		public abstract class bankTranDate : PX.Data.BQL.BqlDateTime.Field<bankTranDate> { }

		/// <summary>
		/// The CA bank transaction date.
		/// </summary>
		[PXDBDate]
		public DateTime? BankTranDate { get; set; }
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.TM, Messages.SearchableTitleExpenseReceipt, new Type[] { typeof(EPExpenseClaimDetails.refNbr), typeof(EPExpenseClaimDetails.employeeID), typeof(EPEmployee.acctName) },
			new Type[] { typeof(EPExpenseClaimDetails.tranDesc) },
			NumberFields = new Type[] { typeof(EPExpenseClaimDetails.refNbr) },
			Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(EPExpenseClaimDetails.expenseDate), typeof(EPExpenseClaimDetails.status), typeof(EPExpenseClaimDetails.refNbr) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(EPExpenseClaimDetails.tranDesc) },
			SelectForFastIndexing = typeof(Select2<EPExpenseClaimDetails, InnerJoin<EPEmployee, On<EPExpenseClaimDetails.employeeID, Equal<EPEmployee.bAccountID>>>>),
			SelectDocumentUser = typeof(Select2<Users,
			InnerJoin<EPEmployee, On<Users.pKID, Equal<EPEmployee.userID>>>,
			Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaimDetails.employeeID>>>>)
		)]
		[PXNote(
		DescriptionField = typeof(EPExpenseClaimDetails.claimDetailID),
		Selector = typeof(EPExpenseClaimDetails.claimDetailID),
		ShowInReferenceSelector = true)]
		public virtual Guid? NoteID
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
