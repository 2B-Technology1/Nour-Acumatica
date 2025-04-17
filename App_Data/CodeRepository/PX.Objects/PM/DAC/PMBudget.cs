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
using PX.Objects.IN;
using PX.Objects.CM.Extensions;
using PX.Objects.TX;
using System.Collections;
using PX.Objects.CS;
using System.Collections.Generic;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;

namespace PX.Objects.PM
{
	/// <summary>
	/// The base class for the project budget line.
	/// The records of this type are created and edited through the Project Budget (PM309000) form
	/// (which corresponds to the <see cref="ProjectBalanceMaint"/> graph).
	/// </summary>
	[PXCacheName(Messages.PMBudget)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMBudget : IBqlTable, IProjectFilter, IQuantify
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		/// <exclude />
		public class PK : PrimaryKeyOf<PMBudget>.By<PMBudget.projectID, PMBudget.projectTaskID, PMBudget.accountGroupID, PMBudget.costCodeID, PMBudget.inventoryID>
		{
			public static PMBudget Find(PXGraph graph, int? projectID, int? projectTaskID, int? accountGroupID, int? costCodeID, int? inventoryID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, projectID, projectTaskID, accountGroupID, costCodeID, inventoryID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		/// <exclude />
		public static class FK
		{
			/// <summary>
			/// Project
			/// </summary>
			/// <exclude />
			public class Project : PMProject.PK.ForeignKeyOf<PMBudget>.By<projectID> { }

			/// <summary>
			/// Project Task
			/// </summary>
			/// <exclude />
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PMBudget>.By<projectID, projectTaskID> { }

			/// <summary>
			/// Account Group
			/// </summary>
			/// <exclude />
			public class AccountGroup : PMAccountGroup.PK.ForeignKeyOf<PMBudget>.By<accountGroupID> { }

			/// <summary>
			/// Cost Code
			/// </summary>
			/// <exclude />
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PMBudget>.By<costCodeID> { }

			/// <summary>
			/// Inventory Item
			/// </summary>
			/// <exclude />
			public class Item : IN.InventoryItem.PK.ForeignKeyOf<PMBudget>.By<inventoryID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		protected Int32? _ProjectID;

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see> associated with the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID" /> field.
		/// </value>
		[PXParent(typeof(Select<
			PMProject,
			Where<PMProject.contractID, Equal<Current<projectID>>,
				And<PMBudget.type, Equal<Current<PMBudget.type>>>>>))]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}
		/// <summary>
		/// The identifier of the <see cref="PMTask">project task</see> associated with the budget line.
		/// </summary>
		public int? TaskID => ProjectTaskID;

		/// <summary>
		/// The identifier of the <see cref="PMTask">project task</see> associated with the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.TaskID" /> field.
		/// </value>
		[PMTaskCompleted]
		[PXDefault(typeof(Search<
			PMTask.taskID,
			Where<PMTask.projectID, Equal<Current<projectID>>,
				And<PMTask.isDefault, Equal<True>>>>))]
		[PXParent(typeof(Select<
			PMTask,
			Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.taskID, Equal<Current<projectTaskID>>,
				And<PMBudget.type, Equal<Current<PMBudget.type>>>>>>))]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<projectTaskID>.IsRelatedTo<PMTask.taskID>>))]
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		protected Int32? _CostCodeID;

		/// <summary>
		/// The identifier of the <see cref="PMCostCode">Cost Code</see> associated with the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostCode.costCodeID"/> field.
		/// </value>
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		[CostCode(null, typeof(projectTaskID), null, typeof(accountGroupID), true, IsKey = true, Filterable = false, SkipVerification = true)]
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
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}
		protected Int32? _AccountGroupID;

		/// <summary>
		/// The identifier of the <see cref="PMAccountGroup">Account Group</see> associated with the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.GroupID"/> field.
		/// </value>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		[PXDefault]
		[AccountGroupAttribute(IsKey = true)]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;

		/// <summary>
		/// The identifier of the <see cref="InventoryItem">inventory item</see> associated with the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
		[PMInventorySelector]
		[PXParent(typeof(Select<
			InventoryItem,
			Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>))]
		[PXDefault]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}
		protected string _Type;

		/// <summary>
		/// The type of the budget line.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"A"</c>: Asset,
		/// <c>"L"</c>: Liability,
		/// <c>"I"</c>: Income,
		/// <c>"E"</c>: Expense,
		/// <c>"O"</c>: Off-Balance
		/// </value>
		[PXDBString(1)]
		[PXDefault]
		[PMAccountType.List]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region RevenueTaskID
		public abstract class revenueTaskID : PX.Data.BQL.BqlInt.Field<revenueTaskID>
		{
		}
		protected Int32? _RevenueTaskID;

		/// <summary>
		/// The reference to a revenue budget line by task.
		/// </summary>
		[PXDBInt]
		public virtual Int32? RevenueTaskID
		{
			get
			{
				return this._RevenueTaskID;
			}
			set
			{
				this._RevenueTaskID = value;
			}
		}
		#endregion
		#region RevenueInventoryID
		public abstract class revenueInventoryID : PX.Data.BQL.BqlInt.Field<revenueInventoryID>
		{
		}
		protected Int32? _RevenueInventoryID;

		/// <summary>
		/// The reference to a revenue budget line by inventory item.
		/// </summary>
		[PXDBInt]
		public virtual Int32? RevenueInventoryID
		{
			get
			{
				return this._RevenueInventoryID;
			}
			set
			{
				this._RevenueInventoryID = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID>
		{
		}
		protected String _TaxCategoryID;

		/// <summary>
		/// The identifier of the <see cref="TaxCategory">tax category</see> associated with the budget line.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="TaxCategory.TaxCategoryID"/> field.
		/// </value>
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(
			Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		/// <summary>
		/// The identifier of the <see cref="CurrencyInfo">currency info</see> object associated with the budget line.
		/// </summary>
		[PXDBLong]
		[CurrencyInfo(typeof(PMProject.curyInfoID))]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}

		/// <summary>
		/// The description of the budget line.
		/// </summary>
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty>
		{
		}

		/// <summary>
		/// The budgeted quantity of the budget line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Quantity")]
		public virtual Decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}

		/// <summary>
		/// The <see cref="INUnit">unit of measure</see> of the budget line.
		/// </summary>
		[PXDefault(typeof(Search<
			InventoryItem.baseUnit,
			Where<InventoryItem.inventoryID, Equal<Current<PMBudget.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(inventoryID))]
		public virtual String UOM
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitRate
		public abstract class curyUnitRate : PX.Data.BQL.BqlDecimal.Field<curyUnitRate> { }

		/// <summary>
		/// The price or cost of the specified unit of the budget line in the project currency.
		/// </summary>
		[PXDBCurrencyPriceCost(typeof(PMBudget.curyInfoID), typeof(PMBudget.rate))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Rate")]
		public virtual decimal? CuryUnitRate
		{
			get;
			set;
		}
		#endregion
		#region Rate
		public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }

		/// <summary>
		/// The price or cost of the specified unit of the budget line in the base currency of the tenant.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBPriceCost]
		public virtual decimal? Rate
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice>
		{
		}

		/// <summary>
		/// The price of the specified unit of the budget line in the project currency.
		/// </summary>
		[PXDBCurrencyPriceCost(typeof(PMBudget.curyInfoID), typeof(PMBudget.unitPrice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price")]
		public virtual Decimal? CuryUnitPrice
		{
			get;
			set;
		}
		#endregion
		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice>
		{
		}

		/// <summary>
		/// The price of the specified unit of the budget line in the base currency of the tenant.
		/// </summary>
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price in Base Currency")]
		public virtual Decimal? UnitPrice
		{
			get;
			set;
		}
		#endregion
		#region CuryAmount
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
		{
		}

		/// <summary>
		/// The budgeted amount of the budget line in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.amount))]
		[PXFormula(typeof(Mult<qty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount")]
		public virtual Decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount>
		{
		}

		/// <summary>
		/// The budgeted amount of the budget line in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount in Base Currency")]
		public virtual Decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region RevisedQty
		public abstract class revisedQty : PX.Data.BQL.BqlDecimal.Field<revisedQty>
		{
		}

		/// <summary>
		/// The revised budgeted quantity.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Quantity")]
		public virtual Decimal? RevisedQty
		{
			get;
			set;
		}
		#endregion
		#region CuryRevisedAmount
		public abstract class curyRevisedAmount : PX.Data.BQL.BqlDecimal.Field<curyRevisedAmount>
		{
		}

		/// <summary>
		/// The revised budgeted amount in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.revisedAmount))]
		[PXFormula(typeof(Mult<revisedQty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.RevisedBudgetedAmount)]
		public virtual Decimal? CuryRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region RevisedAmount
		public abstract class revisedAmount : PX.Data.BQL.BqlDecimal.Field<revisedAmount>
		{
		}

		/// <summary>
		/// The revised budgeted amount in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount in Base Currency")]
		public virtual Decimal? RevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region InvoicedQty
		public abstract class invoicedQty : PX.Data.BQL.BqlDecimal.Field<invoicedQty> { }

		/// <summary>
		/// The total quantity of unreleased invoices that correspond to the revenue budget line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.DraftInvoiceQuantity, Enabled = false)]
		public virtual Decimal? InvoicedQty { get; set; }
		#endregion
		#region CuryInvoicedAmount
		public abstract class curyInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<curyInvoicedAmount>
		{
		}

		/// <summary>
		/// The total amount of unreleased invoices that correspond to the revenue budget line. The amount is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.invoicedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.DraftInvoiceAmount, Enabled = false)]
		public virtual Decimal? CuryInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region InvoicedAmount
		public abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount>
		{
		}

		/// <summary>
		/// The total amount of unreleased invoices that correspond to the revenue budget line in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Invoices Amount in Base Currency", Enabled = false)]
		public virtual Decimal? InvoicedAmount
		{
			get;
			set;
		}
		#endregion


		#region ActualQty
		public abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty>
		{
		}
		protected Decimal? _ActualQty;

		/// <summary>
		/// The total quantity of the lines of the released accounts receivable invoices that correspond to the budget line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Quantity", Enabled = false)]
		public virtual Decimal? ActualQty
		{
			get
			{
				return this._ActualQty;
			}
			set
			{
				this._ActualQty = value;
			}
		}
		#endregion

		#region CuryActualAmount
		public abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount> { }

		/// <summary>
		/// The total amount of the lines of the released accounts receivable invoices that correspond to the budget line.
		/// The amount is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.baseActualAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.ActualAmount, Enabled = false)]
		public virtual decimal? CuryActualAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualAmount
		public abstract class actualAmount : PX.Data.BQL.BqlDecimal.Field<actualAmount> { }

		/// <summary>
		/// The actual amount in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Hist. Actual Amount in Base Currency", Enabled = false, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		public virtual decimal? ActualAmount
		{
			get;
			set;
		}
		#endregion
		#region BaseActualAmount
		public abstract class baseActualAmount : PX.Data.BQL.BqlDecimal.Field<baseActualAmount> { }

		/// <summary>
		/// The total amount of the lines of the released accounts receivable invoices that correspond to the budget line in the base currency of the tenant.
		/// </summary>
		[PXBaseCury]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount in Base Currency", Enabled = false, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		public virtual decimal? BaseActualAmount
		{
			get;
			set;
		}
		#endregion


		#region CuryInclTaxAmount
		public new abstract class curyInclTaxAmount : PX.Data.BQL.BqlDecimal.Field<curyInclTaxAmount>
		{
		}

		/// <summary>
		/// The inclusive tax amount in project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.inclTaxAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Inclusive Tax Amount", Visible = false, Enabled = false)]
		public virtual Decimal? CuryInclTaxAmount
		{
			get;
			set;
		}
		#endregion
		#region InclTaxAmount
		public new abstract class inclTaxAmount : PX.Data.BQL.BqlDecimal.Field<inclTaxAmount>
		{
		}

		/// <summary>
		/// The inclusive tax amount in the base currency.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Inclusive Tax Amount in Base Currency", Visible = false, Enabled = false)]
		public virtual decimal? InclTaxAmount
		{
			get;
			set;
		}
		#endregion

		#region DraftChangeOrderQty
		public abstract class draftChangeOrderQty : PX.Data.BQL.BqlDecimal.Field<draftChangeOrderQty>
		{
		}

		/// <summary>
		/// The total quantity of the estimation lines of open change requests plus
		/// the total quantity of the budget lines of non-closed change orders.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Potential CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? DraftChangeOrderQty
		{
			get;
			set;
		}
		#endregion
		#region CuryDraftChangeOrderAmount
		public abstract class curyDraftChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyDraftChangeOrderAmount>
		{
		}

		/// <summary>
		/// The total amount of the estimation lines of open change requests plus
		/// the total amount of the budget lines of non-closed change orders.
		/// The amount is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.draftChangeOrderAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Potential CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryDraftChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region DraftChangeOrderAmount
		public abstract class draftChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<draftChangeOrderAmount>
		{
		}

		/// <summary>
		/// The total amount of the estimation lines of open change requests plus the total amount
		/// of the budget lines of non-closed change orders in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Potential CO Amount in Base Currency", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? DraftChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderQty
		public abstract class changeOrderQty : PX.Data.BQL.BqlDecimal.Field<changeOrderQty>
		{
		}
		protected Decimal? _ChangeOrderQty;

		/// <summary>
		/// The total quantity of the lines of released change orders.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? ChangeOrderQty
		{
			get
			{
				return this._ChangeOrderQty;
			}
			set
			{
				this._ChangeOrderQty = value;
			}
		}
		#endregion
		#region CuryChangeOrderAmount
		public abstract class curyChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyChangeOrderAmount>
		{
		}

		/// <summary>
		/// The total amount of the lines of released change orders.
		/// The amount is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.changeOrderAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderAmount
		public abstract class changeOrderAmount : PX.Data.BQL.BqlDecimal.Field<changeOrderAmount>
		{
		}

		/// <summary>
		/// The total amount of the lines of released change orders in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Amount in Base Currency", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? ChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedQty
		public abstract class committedQty : PX.Data.BQL.BqlDecimal.Field<committedQty>
		{
		}
		protected Decimal? _CommittedQty;

		/// <summary>
		/// The total quantity of the commitments.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Quantity", Enabled = false)]
		public virtual Decimal? CommittedQty
		{
			get
			{
				return this._CommittedQty;
			}
			set
			{
				this._CommittedQty = value;
			}
		}
		#endregion
		#region CuryCommittedAmount
		public abstract class curyCommittedAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedAmount>
		{
		}

		/// <summary>
		/// The total amount of the commitments in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Amount", Enabled = false)]
		public virtual Decimal? CuryCommittedAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedAmount
		public abstract class committedAmount : PX.Data.BQL.BqlDecimal.Field<committedAmount>
		{
		}

		/// <summary>
		/// The total amount of the commitments in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Amount in Base Currency", Enabled = false)]
		public virtual Decimal? CommittedAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedCOQty
		public abstract class committedCOQty : PX.Data.BQL.BqlDecimal.Field<committedCOQty>
		{
		}

		/// <summary>
		/// The total quantity of the commitment lines of released change orders.
		/// </summary>
		/// <value>
		/// The quantity is the difference between the <see cref="CommittedQty">Revised Committed Quantity</see>
		/// and the <see cref="CommittedOrigQty">Original Committed Quantity</see>.
		/// </value>
		[PXQuantity]

		[PXUIField(DisplayName = "Committed CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedCOQty
		{
			[PXDependsOnFields(typeof(committedQty), typeof(committedOrigQty))]
			get
			{
				return this.CommittedQty.GetValueOrDefault() - this.CommittedOrigQty.GetValueOrDefault();
			}
		}
		#endregion
		#region CuryCommittedCOAmount
		public abstract class curyCommittedCOAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedCOAmount>
		{
		}

		/// <summary>
		/// The total amount of the commitment lines of released change orders.
		/// The amount is shown in the project currency.
		/// </summary>
		/// <value>
		/// The amount is the difference between the <see cref="CuryCommittedAmount">Revised Committed Amount</see>
		/// and the <see cref="CuryCommittedOrigAmount">Original Committed Amount</see>.
		/// </value>
		[PXCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedCOAmount))]
		[PXUIField(DisplayName = "Committed CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryCommittedCOAmount
		{
			[PXDependsOnFields(typeof(curyCommittedAmount), typeof(curyCommittedOrigAmount))]
			get
			{
				return this.CuryCommittedAmount.GetValueOrDefault() - this.CuryCommittedOrigAmount.GetValueOrDefault();
			}
		}
		#endregion
		#region CommittedCOAmount
		public abstract class committedCOAmount : PX.Data.BQL.BqlDecimal.Field<committedCOAmount>
		{
		}

		/// <summary>
		/// The total amount of the commitment lines of released change orders in the base currency of the tenant.
		/// </summary>
		[PXBaseCury]
		[PXUIField(DisplayName = "Committed CO Amount in Base Currency", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedCOAmount
		{
			[PXDependsOnFields(typeof(committedAmount), typeof(committedOrigAmount))]
			get
			{
				return this.CommittedAmount.GetValueOrDefault() - this.CommittedOrigAmount.GetValueOrDefault();
			}
		}
		#endregion
		#region CommittedOrigQty
		public abstract class committedOrigQty : PX.Data.BQL.BqlDecimal.Field<committedOrigQty>
		{
		}

		/// <summary>
		/// The total original quantity of the commitments.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Committed Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedOrigQty
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedOrigAmount
		public abstract class curyCommittedOrigAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedOrigAmount>
		{
		}

		/// <summary>
		/// The total original amount of the commitments in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedOrigAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Committed Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryCommittedOrigAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedOrigAmount
		public abstract class committedOrigAmount : PX.Data.BQL.BqlDecimal.Field<committedOrigAmount>
		{
		}

		/// <summary>
		/// The total original amount of the commitments in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Committed Amount in Base Currency", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedOrigAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedOpenQty
		public abstract class committedOpenQty : PX.Data.BQL.BqlDecimal.Field<committedOpenQty>
		{
		}
		protected Decimal? _CommittedOpenQty;

		/// <summary>
		/// The total open quantity of the commitments.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Quantity", Enabled = false)]
		public virtual Decimal? CommittedOpenQty
		{
			get
			{
				return this._CommittedOpenQty;
			}
			set
			{
				this._CommittedOpenQty = value;
			}
		}
		#endregion
		#region CuryCommittedOpenAmount
		public abstract class curyCommittedOpenAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedOpenAmount>
		{
		}

		/// <summary>
		/// The total open amount of the commitments in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedOpenAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Amount", Enabled = false)]
		public virtual Decimal? CuryCommittedOpenAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedOpenAmount
		public abstract class committedOpenAmount : PX.Data.BQL.BqlDecimal.Field<committedOpenAmount>
		{
		}

		/// <summary>
		/// The total open amount of the commitments in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Amount in Base Currency", Enabled = false)]
		public virtual Decimal? CommittedOpenAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedReceivedQty
		public abstract class committedReceivedQty : PX.Data.BQL.BqlDecimal.Field<committedReceivedQty>
		{
		}
		protected Decimal? _CommittedReceivedQty;

		/// <summary>
		/// The total received quantity of the commitments.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Received Quantity", Enabled = false)]
		public virtual Decimal? CommittedReceivedQty
		{
			get
			{
				return this._CommittedReceivedQty;
			}
			set
			{
				this._CommittedReceivedQty = value;
			}
		}
		#endregion
		#region CommittedInvoicedQty
		public abstract class committedInvoicedQty : PX.Data.BQL.BqlDecimal.Field<committedInvoicedQty>
		{
		}
		protected Decimal? _CommittedInvoicedQty;

		/// <summary>
		/// The total invoiced quantity of the commitments.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Quantity", Enabled = false)]
		public virtual Decimal? CommittedInvoicedQty
		{
			get
			{
				return this._CommittedInvoicedQty;
			}
			set
			{
				this._CommittedInvoicedQty = value;
			}
		}
		#endregion
		#region CuryCommittedInvoicedAmount
		public abstract class curyCommittedInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedInvoicedAmount>
		{
		}

		/// <summary>
		/// The total invoiced amount of the commitments in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.committedInvoicedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Amount", Enabled = false)]
		public virtual Decimal? CuryCommittedInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region CommittedInvoicedAmount
		public abstract class committedInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<committedInvoicedAmount>
		{
		}

		/// <summary>
		/// The total invoiced amount of the commitments in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Amount in Base Currency", Enabled = false)]
		public virtual Decimal? CommittedInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryActualPlusOpenCommittedAmount
		public abstract class curyActualPlusOpenCommittedAmount : PX.Data.BQL.BqlDecimal.Field<curyActualPlusOpenCommittedAmount>
		{
		}

		/// <summary>
		/// The sum of the <see cref="CuryActualAmount">Actual Amount</see> and <see cref="CuryCommittedOpenAmount">Committed Open Amount</see> values.
		/// The amount is shown in the project currency.
		/// </summary>
		[PXCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.actualPlusOpenCommittedAmount))]
		[PXUIField(DisplayName = "Actual + Open Committed Amount", Enabled = false)]
		public virtual Decimal? CuryActualPlusOpenCommittedAmount
		{
			[PXDependsOnFields(typeof(curyActualAmount), typeof(curyCommittedOpenAmount))]
			get
			{
				return this.CuryActualAmount + this.CuryCommittedOpenAmount;
			}
		}
		#endregion
		#region ActualPlusOpenCommittedAmount
		public abstract class actualPlusOpenCommittedAmount : PX.Data.BQL.BqlDecimal.Field<actualPlusOpenCommittedAmount>
		{
		}

		/// <summary>
		/// The sum of the <see cref="ActualAmount" /> and <see cref="CommittedOpenAmount" /> values.
		/// </summary>
		[PXBaseCury]
		[PXUIField(DisplayName = "Actual + Open Committed Amount in Base Currency", Enabled = false)]
		public virtual Decimal? ActualPlusOpenCommittedAmount
		{
			[PXDependsOnFields(typeof(actualAmount), typeof(committedOpenAmount))]
			get
			{
				return this.ActualAmount + this.CommittedOpenAmount;
			}
		}
		#endregion
		#region CuryVarianceAmount
		public abstract class curyVarianceAmount : PX.Data.BQL.BqlDecimal.Field<curyVarianceAmount>
		{
		}

		/// <summary>
		/// The difference between the <see cref="CuryRevisedAmount">Revised Budgeted Amount</see> and
		/// <see cref="CuryActualPlusOpenCommittedAmount">Actual + Open Committed Amount</see> values.
		/// The amount is shown in the project currency.
		/// </summary>
		[PXCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.varianceAmount))]
		[PXUIField(DisplayName = "Variance Amount", Enabled = false)]
		public virtual Decimal? CuryVarianceAmount
		{
			[PXDependsOnFields(typeof(curyRevisedAmount), typeof(curyActualPlusOpenCommittedAmount))]
			get
			{
				return this.CuryRevisedAmount - this.CuryActualPlusOpenCommittedAmount;
			}
		}
		#endregion
		#region VarianceAmount
		public abstract class varianceAmount : PX.Data.BQL.BqlDecimal.Field<varianceAmount>
		{
		}

		/// <summary>
		/// The difference between the <see cref="RevisedAmount" /> and <see cref="ActualPlusOpenCommittedAmount" /> values.
		/// </summary>
		[PXBaseCury]
		[PXUIField(DisplayName = "Variance Amount in Base Currency", Enabled = false)]
		public virtual Decimal? VarianceAmount
		{
			[PXDependsOnFields(typeof(revisedAmount), typeof(actualPlusOpenCommittedAmount))]
			get
			{
				return this.RevisedAmount - this.ActualPlusOpenCommittedAmount;
			}
		}
		#endregion
		#region Performance
		public abstract class performance : PX.Data.BQL.BqlDecimal.Field<performance>
		{
		}

		/// <summary>
		/// The task performance measure.
		/// </summary>
		[PXDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Performance (%)", Enabled = false)]
		public virtual Decimal? Performance
		{
			get
			{
				if(ProgressBillingBase == PM.ProgressBillingBase.Amount || string.IsNullOrEmpty(ProgressBillingBase))
				{
					return CuryRevisedAmount.HasValue && CuryRevisedAmount != 0.0m ? (CuryActualAmount.GetValueOrDefault() + CuryInclTaxAmount.GetValueOrDefault()) / CuryRevisedAmount.GetValueOrDefault() * 100.0m : 0.0m;
				}
				else if(ProgressBillingBase == PM.ProgressBillingBase.Quantity)
				{
					return RevisedQty.HasValue && RevisedQty != 0.0m ? ActualQty.GetValueOrDefault() / RevisedQty.GetValueOrDefault() * 100.0m : 0.0m;
				}
				return 0.0m;
			}
		}
		#endregion
		#region IsProduction
		public abstract class isProduction : PX.Data.BQL.BqlBool.Field<isProduction>
		{
		}
		protected Boolean? _IsProduction;

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the <see cref="PMTask.CompletedPercent">Completed (%)</see>
		/// of the corresponding task is calculated automatically, based on the completion method of the task.
		/// </summary>
		[PXDefault(false)]
		[PXDBBool()]
		[PXUIField(DisplayName = "Auto Completed (%)")]
		public virtual Boolean? IsProduction
		{
			get
			{
				return this._IsProduction;
			}
			set
			{
				this._IsProduction = value;
			}
		}
		#endregion
		#region CompletedPct
		public abstract class completedPct : PX.Data.BQL.BqlDecimal.Field<completedPct>
		{
		}

		/// <summary>
		/// The percentage of the work that has been completed on the revenue budget line.
		/// </summary>
		[PXDBDecimal(2, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Completed (%)")]
		public virtual decimal? CompletedPct
		{
			get;
			set;
		}
		#endregion
		#region QtyToInvoice
		public abstract class qtyToInvoice : PX.Data.BQL.BqlDecimal.Field<qtyToInvoice> { }

		/// <summary>
		/// The quantity for which the customer will be billed during the next billing.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.PendingInvoiceQuantity)]
		public virtual Decimal? QtyToInvoice { get; set; }
		#endregion
		#region CuryAmountToInvoice
		public abstract class curyAmountToInvoice : PX.Data.BQL.BqlDecimal.Field<curyAmountToInvoice>
		{
		}

		/// <summary>
		/// The amount for which the customer will be billed during the next billing.
		/// The amount is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.amountToInvoice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Invoice Amount")]
		public virtual Decimal? CuryAmountToInvoice
		{
			get;
			set;
		}
		#endregion
		#region AmountToInvoice
		public abstract class amountToInvoice : PX.Data.BQL.BqlDecimal.Field<amountToInvoice>
		{
		}

		/// <summary>
		/// The amount for which the customer will be billed during the next billing in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Invoice Amount in Base Currency")]
		public virtual Decimal? AmountToInvoice
		{
			get;
			set;
		}
		#endregion
		#region PrepaymentPct
		public abstract class prepaymentPct : PX.Data.BQL.BqlDecimal.Field<prepaymentPct>
		{
		}

		/// <summary>
		/// The field is reserved for a feature that is currently not supported.
		/// </summary>
		/// <exclude />
		[PXDecimal(2, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Prepaid (%)")]
		public virtual decimal? PrepaymentPct
		{
			get;
			set;
		}
		#endregion
		#region CuryPrepaymentAmount
		public abstract class curyPrepaymentAmount : PX.Data.BQL.BqlDecimal.Field<curyPrepaymentAmount>
		{
		}

		/// <summary>
		/// The field is reserved for a feature that is currently not supported.
		/// </summary>
		/// <exclude />
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.prepaymentAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Amount")]
		public virtual Decimal? CuryPrepaymentAmount
		{
			get;
			set;
		}
		#endregion
		#region PrepaymentAmount
		public abstract class prepaymentAmount : PX.Data.BQL.BqlDecimal.Field<prepaymentAmount>
		{
		}

		/// <summary>
		/// The field is reserved for a feature that is currently not supported.
		/// </summary>
		/// <exclude />
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Amount in Base Currency")]
		public virtual Decimal? PrepaymentAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryPrepaymentAvailable
		public abstract class curyPrepaymentAvailable : PX.Data.BQL.BqlDecimal.Field<curyPrepaymentAvailable>
		{
		}

		/// <summary>
		/// The field is reserved for a feature that is currently not supported.
		/// </summary>
		/// <exclude />
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.prepaymentAvailable))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Available", Enabled = false)]
		public virtual Decimal? CuryPrepaymentAvailable
		{
			get;
			set;
		}
		#endregion
		#region PrepaymentAvailable
		public abstract class prepaymentAvailable : PX.Data.BQL.BqlDecimal.Field<prepaymentAvailable>
		{
		}

		/// <summary>
		/// The field is reserved for a feature that is currently not supported.
		/// </summary>
		/// <exclude />
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Available in Base Currency", Enabled = false)]
		public virtual Decimal? PrepaymentAvailable
		{
			get;
			set;
		}
		#endregion
		#region CuryPrepaymentInvoiced
		public abstract class curyPrepaymentInvoiced : PX.Data.BQL.BqlDecimal.Field<curyPrepaymentInvoiced>
		{
		}

		/// <summary>
		/// The field is reserved for a feature that is currently not supported.
		/// </summary>
		/// <exclude />
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.prepaymentInvoiced))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Invoiced", Enabled = false)]
		public virtual Decimal? CuryPrepaymentInvoiced
		{
			get;
			set;
		}
		#endregion
		#region PrepaymentInvoiced
		public abstract class prepaymentInvoiced : PX.Data.BQL.BqlDecimal.Field<prepaymentInvoiced>
		{
		}

		/// <summary>
		/// The field is reserved for a feature that is currently not supported.
		/// </summary>
		/// <exclude />
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Invoiced in Base Currency", Enabled = false)]
		public virtual Decimal? PrepaymentInvoiced
		{
			get;
			set;
		}
		#endregion
		#region LimitQty
		public abstract class limitQty : PX.Data.BQL.BqlBool.Field<limitQty>
		{
		}

		/// <summary>
		/// A Boolean value that indicates whether the system controls the quantity
		/// available to bill the customer based on the billing limit quantity.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Limit Quantity")]
		public virtual Boolean? LimitQty
		{
			get;
			set;
		}
		#endregion
		#region LimitAmount
		public abstract class limitAmount : PX.Data.BQL.BqlBool.Field<limitAmount>
		{
		}

		/// <summary>
		/// Indicates (if set to <see langword="true" />) that the system controls the amount
		/// available to bill the customer based on the billing limit amount.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Limit Amount")]
		public virtual Boolean? LimitAmount
		{
			get;
			set;
		}
		#endregion
		#region MaxQty
		public abstract class maxQty : PX.Data.BQL.BqlDecimal.Field<maxQty>
		{
		}

		/// <summary>
		/// The maximum billable quantity for the revenue budget line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Maximum Quantity")]
		public virtual Decimal? MaxQty
		{
			get;
			set;
		}
		#endregion
		#region CuryMaxAmount
		public abstract class curyMaxAmount : PX.Data.BQL.BqlDecimal.Field<curyMaxAmount>
		{
		}

		/// <summary>
		/// The maximum billable amount for the revenue budget line in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.maxAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Maximum Amount")]
		public virtual Decimal? CuryMaxAmount
		{
			get;
			set;
		}
		#endregion
		#region MaxAmount
		public abstract class maxAmount : PX.Data.BQL.BqlDecimal.Field<maxAmount>
		{
		}

		/// <summary>
		/// The maximum billable amount for the revenue budget line in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Maximum Amount in Base Currency")]
		public virtual Decimal? MaxAmount
		{
			get;
			set;
		}
		#endregion
		#region RetainagePct
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct>
		{
		}

		/// <summary>
		/// The percent of the invoice line amount to be retained by the customer. 
		/// </summary>
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage (%)", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainedAmount
		/// <exclude/>
		public abstract class curyRetainedAmount : PX.Data.BQL.BqlDecimal.Field<curyRetainedAmount> { }
		/// <summary>
		/// The retained amount in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.retainedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retained Amount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? CuryRetainedAmount
		{
			get;
			set;
		}
		#endregion
		#region RetainedAmount
		/// <exclude/>
		public abstract class retainedAmount : PX.Data.BQL.BqlDecimal.Field<retainedAmount> { }
		/// <summary>
		/// Retained Amount (In Base Currency)
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retained Amount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? RetainedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryDraftRetainedAmount
		/// <exclude/>
		public abstract class curyDraftRetainedAmount : PX.Data.BQL.BqlDecimal.Field<curyDraftRetainedAmount> { }
		/// <summary>
		/// The draft retained amount in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.draftRetainedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Retained Amount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? CuryDraftRetainedAmount
		{
			get;
			set;
		}
		#endregion
		#region DraftRetainedAmount
		/// <exclude/>
		public abstract class draftRetainedAmount : PX.Data.BQL.BqlDecimal.Field<draftRetainedAmount> { }
		/// <summary>
		/// Draft Retained Amount (in Base Currency)
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Retained Amount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? DraftRetainedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryTotalRetainedAmount
		/// <exclude/>
		public abstract class curyTotalRetainedAmount : PX.Data.BQL.BqlDecimal.Field<curyTotalRetainedAmount>
		{
		}

		/// <summary>
		/// The total retained amount in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.totalRetainedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Retained Amount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? CuryTotalRetainedAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalRetainedAmount
		/// <exclude/>
		public abstract class totalRetainedAmount : PX.Data.BQL.BqlDecimal.Field<totalRetainedAmount>
		{
		}

		/// <summary>
		/// Total Retained Amount in Base Currency
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Retained Amount in Base Currency", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? TotalRetainedAmount
		{
			get;
			set;
		}
		#endregion
		#region RetainageMaxPct
		/// <exclude/>
		public abstract class retainageMaxPct : PX.Data.BQL.BqlDecimal.Field<retainageMaxPct>
		{
		}
		/// <summary>
		/// Retainage Cap %
		/// </summary>
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cap (%)", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? RetainageMaxPct
		{
			get;
			set;
		}
		#endregion

		#region CuryLastCostToComplete
		public abstract class curyLastCostToComplete : PX.Data.BQL.BqlDecimal.Field<curyLastCostToComplete>
		{
		}

		/// <summary>
		/// The value of the Cost to Complete column before the most recent change was made to it.
		/// The value is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.lastCostToComplete))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost to Complete", Enabled = false)]
		public virtual Decimal? CuryLastCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region LastCostToComplete
		public abstract class lastCostToComplete : PX.Data.BQL.BqlDecimal.Field<lastCostToComplete>
		{
		}

		/// <summary>
		/// The value of the Cost to Complete column before the most recent change was made to it in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost to Complete in Base Currency", Enabled = false)]
		public virtual Decimal? LastCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CuryCostToComplete
		public abstract class curyCostToComplete : PX.Data.BQL.BqlDecimal.Field<curyCostToComplete>
		{
		}

		/// <summary>
		/// The current projected amount that is required to complete the cost budget line.
		/// The amount is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.costToComplete))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost to Complete")]
		public virtual Decimal? CuryCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CostToComplete
		public abstract class costToComplete : PX.Data.BQL.BqlDecimal.Field<costToComplete>
		{
		}

		/// <summary>
		/// The current projected amount that is required to complete the cost budget line in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost To Complete in Base Currency")]
		public virtual Decimal? CostToComplete
		{
			get;
			set;
		}
		#endregion
		#region LastPercentCompleted
		public abstract class lastPercentCompleted : PX.Data.BQL.BqlDecimal.Field<lastPercentCompleted>
		{
		}

		/// <summary>
		/// The value of the Percentage of Completion column before the most recent change was made to it.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Percentage of Completion", Enabled = false)]
		public virtual Decimal? LastPercentCompleted
		{
			get;
			set;
		}
		#endregion
		#region PercentCompleted
		public abstract class percentCompleted : PX.Data.BQL.BqlDecimal.Field<percentCompleted>
		{
		}

		/// <summary>
		/// The current approximate percentage of project completion that corresponds to the cost budget line.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Percentage of Completion")]
		public virtual Decimal? PercentCompleted
		{
			get;
			set;
		}
		#endregion
		#region CuryLastCostAtCompletion
		public abstract class curyLastCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<curyLastCostAtCompletion>
		{
		}

		/// <summary>
		/// The value of the Cost at Completion column before the most recent change was made to it.
		/// The value is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.lastCostAtCompletion))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost at Completion", Enabled = false)]
		public virtual Decimal? CuryLastCostAtCompletion
		{
			get;
			set;
		}
		#endregion
		#region LastCostAtCompletion
		public abstract class lastCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<lastCostAtCompletion>
		{
		}

		/// <summary>
		/// The value of the Cost at Completion column before the most recent change was made to it in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost at Completion in Base Currency", Enabled = false)]
		public virtual Decimal? LastCostAtCompletion
		{
			get;
			set;
		}
		#endregion
		#region CuryCostAtCompletion
		public abstract class curyCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<curyCostAtCompletion>
		{
		}

		/// <summary>
		/// The current projected total cost amount of the cost budget line in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(PMBudget.curyInfoID), typeof(PMBudget.costAtCompletion))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost at Completion")]
		public virtual Decimal? CuryCostAtCompletion
		{
			get;
			set;
		}
		#endregion
		#region CostAtCompletion
		public abstract class costAtCompletion : PX.Data.BQL.BqlDecimal.Field<costAtCompletion>
		{
		}

		/// <summary>
		/// The current projected total cost amount of the cost budget line in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost at Completion")]
		public virtual Decimal? CostAtCompletion
		{
			get;
			set;
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr>
		{
		}

		/// <summary>
		/// The counter of budget lines.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? LineCntr { get; set; }
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder>
		{
		}
		protected int _SortOrder = 0;

		/// <summary>
		/// An internal field that is used to arrange the records on the Revenue Budget and Cost Budget tabs of the Projects (PM301000) form.
		/// </summary>
		[PXInt()]
		public virtual int? SortOrder
		{
			get
			{
				return _SortOrder;
			}
			set
			{
				_SortOrder = value.GetValueOrDefault();
			}
		}
		#endregion

		#region CostProjectionCompletedPct
		public abstract class costProjectionCompletedPct : PX.Data.BQL.BqlDecimal.Field<costProjectionCompletedPct> { }

		/// <summary>
		/// The projected percentage of completion for the cost budget line in the last
		/// released revision of the cost projection for this line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Completed (%)", Enabled = false, FieldClass = nameof(FeaturesSet.Construction))]
		public virtual decimal? CostProjectionCompletedPct
		{
			get;
			set;
		}
		#endregion

		#region CostProjectionQtyToComplete
		public abstract class costProjectionQtyToComplete : PX.Data.BQL.BqlDecimal.Field<costProjectionQtyToComplete> { }

		/// <summary>
		/// The remainder of the budgeted quantity for the cost budget line in the last
		/// released revision of the cost projection for this line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Quantity to Complete", Enabled = false, FieldClass = nameof(FeaturesSet.Construction))]
		public virtual decimal? CostProjectionQtyToComplete
		{
			get;
			set;
		}
		#endregion
		#region CostProjectionQtyAtCompletion
		public abstract class costProjectionQtyAtCompletion : PX.Data.BQL.BqlDecimal.Field<costProjectionQtyAtCompletion> { }

		/// <summary>
		/// The projected final quantity at project completion for the cost budget line
		/// in the last released revision of the cost projection for this line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Quantity at Completion", Enabled = false, FieldClass = nameof(FeaturesSet.Construction))]
		public virtual decimal? CostProjectionQtyAtCompletion
		{
			get;
			set;
		}
		#endregion

		#region CuryCostProjectionCostToComplete
		public abstract class curyCostProjectionCostToComplete : PX.Data.BQL.BqlDecimal.Field<curyCostProjectionCostToComplete>
		{
		}

		/// <summary>
		/// The remainder of the budgeted cost for the cost budget line in the last
		/// released revision of the cost projection for this line.
		/// The value is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(costProjectionCostToComplete))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost to Complete", Enabled = false, FieldClass = nameof(FeaturesSet.Construction))]
		public virtual decimal? CuryCostProjectionCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CostProjectionCostToComplete
		public abstract class costProjectionCostToComplete : PX.Data.BQL.BqlDecimal.Field<costProjectionCostToComplete>
		{
		}

		/// <summary>
		/// The remainder of the budgeted cost for the cost budget line in the last released 
		/// revision of the cost projection for this line in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost To Complete in Base Currency", Enabled = false, FieldClass = nameof(FeaturesSet.Construction))]
		public virtual decimal? CostProjectionCostToComplete
		{
			get;
			set;
		}
		#endregion

		#region CuryCostProjectionCostAtCompletion
		public abstract class curyCostProjectionCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<curyCostProjectionCostAtCompletion>
		{
		}

		/// <summary>
		/// The projected final cost at project completion for the cost budget line
		/// in the last released revision of the cost projection for this line.
		/// The value is shown in the project currency.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(costProjectionCostAtCompletion))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost at Completion", Enabled = false, FieldClass = nameof(FeaturesSet.Construction))]
		public virtual decimal? CuryCostProjectionCostAtCompletion
		{
			get;
			set;
		}
		#endregion
		#region CostProjectionCostAtCompletion
		public abstract class costProjectionCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<costProjectionCostAtCompletion>
		{
		}

		/// <summary>
		/// The projected final cost at project completion for the cost budget line in the last released  
		/// revision of the cost projection for this line in the base currency of the tenant.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost at Completion in Base Currency", Enabled = false, FieldClass = nameof(FeaturesSet.Construction))]
		public virtual decimal? CostProjectionCostAtCompletion
		{
			get;
			set;
		}
		#endregion

		#region ProgressBillingBase
		public abstract class progressBillingBase : Data.BQL.BqlString.Field<progressBillingBase> { }

		/// <summary>
		/// The value that the system uses as the basis for progress billing of the project task.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"A"</c>: Amount,
		/// <c>"Q"</c>: Quantity
		/// </value>
		[PXDBString]
		[PXDefault(PM.ProgressBillingBase.Amount, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = Messages.ProgressBillingBase)]
		[ProgressBillingBase.List]
		public virtual string ProgressBillingBase { get; set; }
		#endregion

		#region ProductivityTracking
		public abstract class productivityTracking : PX.Data.BQL.BqlString.Field<productivityTracking>
		{
		}

		/// <summary>
		/// The type of use of the budget line in the <see cref="PMProgressWorksheet">progress worksheet document</see>.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"N"</c>: Not Allowed,
		/// <c>"D"</c>: On Demand,
		/// <c>"T"</c>: Template
		/// </value>
		[PXDBString(1)]
		[PXDefault(PMProductivityTrackingType.NotAllowed)]
		[PMProductivityTrackingType.List]
		[PXUIField(DisplayName = "Productivity Tracking", FieldClass = nameof(CS.FeaturesSet.Construction))]
		public virtual string ProductivityTracking
		{
			get;
			set;
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
		{
		}
		protected Guid? _NoteID;
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID>
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID>
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime>
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMProductivityTrackingType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Template, Messages.ProductivityTypeTemplate),
					Pair(NotAllowed, Messages.ProductivityTypeNotAllowed),
					Pair(OnDemand, Messages.ProductivityTypeOnDemand),
				})
			{ }
		}

		public const string NotAllowed = "N";
		public class notAllowed : PX.Data.BQL.BqlString.Constant<notAllowed>
		{
			public notAllowed() : base(NotAllowed) {; }
		}

		public const string OnDemand = "D";
		public class onDemand : PX.Data.BQL.BqlString.Constant<onDemand>
		{
			public onDemand() : base(OnDemand) {; }
		}

		public const string Template = "T";
		public class template : PX.Data.BQL.BqlString.Constant<template>
		{
			public template() : base(Template) {; }
		}
	}

	[PXBreakInheritance]
	[PMBudgetAccum]
	[Serializable]
	[PXHidden]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMBudgetAccum : PMBudget
	{
		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		[PXDefault]
		[PXDBInt(IsKey = true)]
		public override Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region ProjectTaskID
		public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}
		[PMTaskCompleted]
		[PXDefault]
		[PXDBInt(IsKey = true)]
		public override Int32? ProjectTaskID
		{
			get; set;
		}
		#endregion
		#region AccountGroupID
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}
		[PXDefault]
		[PXDBInt(IsKey = true)]
		public override Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		[PXDefault]
		[PXDBInt(IsKey = true)]
		public override Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region CostCodeID
		public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}

		[PXDefault]
		[PXDBInt(IsKey = true)]
		public override Int32? CostCodeID
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

		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		[PMUnit(typeof(inventoryID))]
		public override String UOM
		{
			get;
			set;
		}
		#endregion
		#region ActualQty
		public new abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty>
		{
		}
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? ActualQty
		{
			get
			{
				return this._ActualQty;
			}
			set
			{
				this._ActualQty = value;
			}
		}
		#endregion
		#region InvoicedQty
		public new abstract class invoicedQty : PX.Data.BQL.BqlDecimal.Field<invoicedQty> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? InvoicedQty { get; set; }
		#endregion
	}

	/// <summary>Represents a project budget line with the <see cref="GL.AccountType.Income">Income</see> type. The records of this type are created and edited through the <strong>Revenue
	/// Budget</strong> tab of the Projects (PM301000) form (which corresponds to the <see cref="ProjectEntry" /> graph) and through the <strong>Revenue Budget</strong> tab of the
	/// Projects Templates (PM208000) form (which corresponds to the <see cref="TemplateMaint" /> graph). The DAC is based on the <see cref="PMBudget" /> DAC and extends it with the fields
	/// relevant to the lines of this type.</summary>
	[PXCacheName(Messages.PMRevenueBudget)]
	[PXBreakInheritance]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMRevenueBudget : PMBudget
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		/// <exclude />
		public new class PK : PrimaryKeyOf<PMRevenueBudget>.By<PMRevenueBudget.projectID, PMRevenueBudget.projectTaskID, PMRevenueBudget.accountGroupID, PMRevenueBudget.costCodeID, PMRevenueBudget.inventoryID>
		{
			public static PMRevenueBudget Find(PXGraph graph, int? projectID, int? projectTaskID, int? accountGroupID, int? costCodeID, int? inventoryID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, projectID, projectTaskID, accountGroupID, costCodeID, inventoryID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		/// <exclude />
		public new static class FK
		{
			/// <summary>
			/// Project
			/// </summary>
			/// <exclude />
			public class Project : PMProject.PK.ForeignKeyOf<PMRevenueBudget>.By<projectID> { }

			/// <summary>
			/// Project Task
			/// </summary>
			/// <exclude />
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PMRevenueBudget>.By<projectID, projectTaskID> { }

			/// <summary>
			/// Account Group
			/// </summary>
			/// <exclude />
			public class AccountGroup : PMAccountGroup.PK.ForeignKeyOf<PMRevenueBudget>.By<accountGroupID> { }

			/// <summary>
			/// Cost Code
			/// </summary>
			/// <exclude />
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PMRevenueBudget>.By<costCodeID> { }

			/// <summary>
			/// Inventory Item
			/// </summary>
			/// <exclude />
			public class Item : IN.InventoryItem.PK.ForeignKeyOf<PMRevenueBudget>.By<inventoryID> { }
		}
		#endregion

		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}

		/// <inheritdoc/>
		[PXParent(typeof(Select<
			PMProject,
			Where<PMProject.contractID, Equal<Current<projectID>>,
				And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>))]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[PXDBInt(IsKey = true)]
		public override Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region ProjectTaskID
		public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}

		/// <inheritdoc/>
		[PXDefault(typeof(Search<
			PMTask.taskID,
			Where<PMTask.projectID, Equal<Current<PMRevenueBudget.projectID>>,
				And<PMTask.isDefault, Equal<True>>>>))]
		[PXParent(typeof(Select<
			PMTask,
			Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.taskID, Equal<Current<projectTaskID>>,
				And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>>))]
		[ProjectTask(typeof(projectID), IsKey = true, AlwaysEnabled = true, DirtyRead = true)]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<projectTaskID>.IsRelatedTo<PMTask.taskID>>))]
		public override Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}

		/// <inheritdoc/>
		[CostCode(null, typeof(projectTaskID), GL.AccountType.Income, typeof(accountGroupID), true, IsKey = true, Filterable = false, SkipVerification = true)]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public override Int32? CostCodeID
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
		#region AccountGroupID
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}

		/// <inheritdoc/>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXDefault]
		[AccountGroup(typeof(
			Where<PMAccountGroup.type, Equal<GL.AccountType.income>>), IsKey = true)]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		public override Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}

		/// <inheritdoc/>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Inventory ID")]
		[PXDefault]
		[PMInventorySelector]
		public override Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}

		/// <summary>
		/// The type of the budget line.
		/// </summary>
		/// <value>
		/// By default, its value is <see cref="GL.AccountType.Income">Income</see>.
		/// </value>
		[PXDBString(1)]
		[PXDefault(GL.AccountType.Income)]
		[PMAccountType.List()]
		[PXUIField(DisplayName = "Budget Type", Visible = false, Enabled = false)]
		public override string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		public new abstract class revenueTaskID : PX.Data.BQL.BqlInt.Field<revenueTaskID> { }
		public new abstract class revenueInventoryID : PX.Data.BQL.BqlInt.Field<revenueInventoryID> { }
		#region TaxCategoryID
		public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

		/// <inheritdoc/>
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(
			Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public override String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region Description
		public new abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}

		/// <inheritdoc/>
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public override String Description
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty>
		{
		}

		/// <inheritdoc/>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Quantity")]
		public override Decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}

		/// <inheritdoc/>
		[PXDefault(typeof(Search<
			InventoryItem.baseUnit,
			Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(inventoryID))]
		public override String UOM
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitRate
		public new abstract class curyUnitRate : PX.Data.BQL.BqlDecimal.Field<curyUnitRate> { }

		/// <summary>
		/// The price of the specified unit of the revenue budget line in the project currency.
		/// </summary>
		[PXDBCurrencyPriceCost(typeof(curyInfoID), typeof(rate))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Rate")]
		public override decimal? CuryUnitRate
		{
			get;
			set;
		}
		#endregion
		#region Rate
		public new abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }

		/// <summary>
		/// The price of the specified unit of the revenue budget line in the base currency of the tenant.
		/// </summary>
		[PXDBPriceCost]
		public override decimal? Rate
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitPrice
		public new abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrencyPriceCost(typeof(curyInfoID), typeof(unitPrice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price")]
		public override Decimal? CuryUnitPrice
		{
			get;
			set;
		}
		#endregion
		#region UnitPrice
		public new abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice>
		{
		}

		/// <inheritdoc/>
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price in Base Currency")]
		public override Decimal? UnitPrice
		{
			get;
			set;
		}
		#endregion

		#region CuryAmount
		public new abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(curyInfoID), typeof(amount))]
		[PXFormula(typeof(Mult<qty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount")]
		public override Decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public new abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount>
		{
		}

		/// <inheritdoc/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount in Base Currency")]
		public override Decimal? Amount
		{
			get;
			set;
		}
		#endregion


		#region CuryInclTaxAmount
		public new abstract class curyInclTaxAmount : PX.Data.BQL.BqlDecimal.Field<curyInclTaxAmount>
		{
		}

		/// <summary>
		/// The inclusive tax amount in project currency calculated from the data of <see cref="ARTran.CuryTaxAmt"/>
		/// and from the data of <see cref="ARTax.CuryRetainedTaxAmt"/>.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(inclTaxAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Inclusive Tax Amount", Visible = false, Enabled = false)]
		public override Decimal? CuryInclTaxAmount
		{
			get;
			set;
		}
		#endregion
		#region InclTaxAmount
		public new abstract class inclTaxAmount : PX.Data.BQL.BqlDecimal.Field<inclTaxAmount>
		{
		}

		/// <summary>
		/// The inclusive tax amount in base currency calculated from the data of <see cref="ARTran.TaxAmt"/>
		/// and from the data of <see cref="ARTax.RetainedTaxAmt"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Inclusive Tax Amount in Base Currency", Visible = false, Enabled = false)]
		public override decimal? InclTaxAmount
		{
			get;
			set;
		}
		#endregion

		#region RevisedQty
		public new abstract class revisedQty : PX.Data.BQL.BqlDecimal.Field<revisedQty>
		{
		}

		/// <inheritdoc/>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Quantity")]
		public override Decimal? RevisedQty
		{
			get;
			set;
		}
		#endregion
		#region CuryRevisedAmount
		public new abstract class curyRevisedAmount : PX.Data.BQL.BqlDecimal.Field<curyRevisedAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(curyInfoID), typeof(revisedAmount))]
		[PXFormula(typeof(Mult<revisedQty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.RevisedBudgetedAmount)]
		public override Decimal? CuryRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region RevisedAmount
		public new abstract class revisedAmount : PX.Data.BQL.BqlDecimal.Field<revisedAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount in Base Currency")]
		public override Decimal? RevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryInvoicedAmount
		public new abstract class curyInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<curyInvoicedAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(curyInfoID), typeof(invoicedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Invoice Amount", Enabled = false)]
		public override Decimal? CuryInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region InvoicedAmount
		public new abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Invoices Amount in Base Currency", Enabled = false)]
		public override Decimal? InvoicedAmount
		{
			get;
			set;
		}
		#endregion
		public new abstract class draftChangeOrderQty : PX.Data.BQL.BqlDecimal.Field<draftChangeOrderQty> { }
		public new abstract class curyDraftChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyDraftChangeOrderAmount> { }
		public new abstract class changeOrderQty : PX.Data.BQL.BqlDecimal.Field<changeOrderQty> { }
		public new abstract class curyChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyChangeOrderAmount> { }
		public new abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty> { }
		public new abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount> { }
		public new abstract class committedQty : PX.Data.BQL.BqlDecimal.Field<committedQty> { }
		public new abstract class committedAmount : PX.Data.BQL.BqlDecimal.Field<committedAmount> { }
		public new abstract class committedOpenQty : PX.Data.BQL.BqlDecimal.Field<committedOpenQty> { }
		public new abstract class committedOpenAmount : PX.Data.BQL.BqlDecimal.Field<committedOpenAmount> { }
		public new abstract class committedReceivedQty : PX.Data.BQL.BqlDecimal.Field<committedReceivedQty> { }
		public new abstract class committedInvoicedQty : PX.Data.BQL.BqlDecimal.Field<committedInvoicedQty> { }
		public new abstract class committedInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<committedInvoicedAmount> { }
		public new abstract class curyActualPlusOpenCommittedAmount : PX.Data.BQL.BqlDecimal.Field<curyActualPlusOpenCommittedAmount> { }
		public new abstract class curyVarianceAmount : PX.Data.BQL.BqlDecimal.Field<curyVarianceAmount> { }
		public new abstract class performance : PX.Data.BQL.BqlDecimal.Field<performance> { }
		public new abstract class qtyToInvoice : PX.Data.BQL.BqlDecimal.Field<qtyToInvoice> { }
		public new abstract class invoicedQty : PX.Data.BQL.BqlDecimal.Field<invoicedQty> { }

		#region ProgressBillingBase
		public new abstract class progressBillingBase : PX.Data.BQL.BqlString.Field<progressBillingBase> { }
		[PXDBString]
		[PXDefault(PM.ProgressBillingBase.Amount, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = Messages.ProgressBillingBase)]
		[ProgressBillingBase.List]
		public override string ProgressBillingBase { get; set; }
		#endregion

		#region CompletedPct
		public new abstract class completedPct : PX.Data.BQL.BqlDecimal.Field<completedPct>
		{
			public const int Precision = 2;
		}

		/// <inheritdoc/>
		[PXDBDecimal(completedPct.Precision, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Completed (%)")]
		public override decimal? CompletedPct
		{
			get;
			set;
		}
		#endregion
		#region CuryAmountToInvoice
		public new abstract class curyAmountToInvoice : PX.Data.BQL.BqlDecimal.Field<curyAmountToInvoice>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(PMRevenueBudget.curyInfoID), typeof(PMRevenueBudget.amountToInvoice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Invoice Amount")]
		public override Decimal? CuryAmountToInvoice
		{
			get;
			set;
		}
		#endregion
		#region PrepaymentPct
		public new abstract class prepaymentPct : PX.Data.BQL.BqlDecimal.Field<prepaymentPct>
		{
		}

		/// <inheritdoc/>
		[PXDecimal(PMRevenueBudget.completedPct.Precision, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Prepaid (%)")]
		public override decimal? PrepaymentPct
		{
			get;
			set;
		}
		#endregion
		#region CuryPrepaymentAmount
		public new abstract class curyPrepaymentAmount : PX.Data.BQL.BqlDecimal.Field<curyPrepaymentAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(PMRevenueBudget.curyInfoID), typeof(PMRevenueBudget.prepaymentAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Amount")]
		public override Decimal? CuryPrepaymentAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryPrepaymentAvailable
		public new abstract class curyPrepaymentAvailable : PX.Data.BQL.BqlDecimal.Field<curyPrepaymentAvailable>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(PMRevenueBudget.curyInfoID), typeof(PMRevenueBudget.prepaymentAvailable))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Available", Enabled = false)]
		public override Decimal? CuryPrepaymentAvailable
		{
			get;
			set;
		}
		#endregion
		#region CuryPrepaymentInvoiced
		public new abstract class curyPrepaymentInvoiced : PX.Data.BQL.BqlDecimal.Field<curyPrepaymentInvoiced>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(PMRevenueBudget.curyInfoID), typeof(PMRevenueBudget.prepaymentInvoiced))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Invoiced", Enabled = false)]
		public override Decimal? CuryPrepaymentInvoiced
		{
			get;
			set;
		}
		#endregion
		#region LimitQty
		public new abstract class limitQty : PX.Data.BQL.BqlBool.Field<limitQty>
		{
		}

		/// <inheritdoc/>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Limit Quantity")]
		public override Boolean? LimitQty
		{
			get;
			set;
		}
		#endregion
		#region LimitAmount
		public new abstract class limitAmount : PX.Data.BQL.BqlBool.Field<limitAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Limit Amount")]
		public override Boolean? LimitAmount
		{
			get;
			set;
		}
		#endregion
		#region MaxQty
		public new abstract class maxQty : PX.Data.BQL.BqlDecimal.Field<maxQty>
		{
		}

		/// <inheritdoc/>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Maximum Quantity")]
		public override Decimal? MaxQty
		{
			get;
			set;
		}
		#endregion
		#region CuryMaxAmount
		public new abstract class curyMaxAmount : PX.Data.BQL.BqlDecimal.Field<curyMaxAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(PMRevenueBudget.curyInfoID), typeof(PMRevenueBudget.maxAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Maximum Amount")]
		public override Decimal? CuryMaxAmount
		{
			get;
			set;
		}
		#endregion

		#region RetainagePct
		public new abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct>
		{
		}

		/// <inheritdoc/>
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXDefault(typeof(Search<
			PMProject.retainagePct,
			Where<PMProject.contractID, Equal<Current<projectID>>>>))]
		[PXUIField(DisplayName = "Retainage (%)", FieldClass = nameof(FeaturesSet.Retainage))]
		public override decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion

		#region CuryCapAmount
		/// <exclude/>
		public abstract class curyCapAmount : PX.Data.BQL.BqlDecimal.Field<curyCapAmount> { }
		/// <summary>
		/// Retainage Cap Amount in the project currency.
		/// </summary>
		/// 
		[PXFormula(typeof(Switch<Case<Where<Current<PMProject.includeCO>, Equal<True>>,
				Mult<curyRevisedAmount, Mult<Div<retainagePct, decimal100>, Div<retainageMaxPct, decimal100>>>>,
			Case<Where<Current<PMProject.includeCO>, NotEqual<True>>,
				Mult<curyAmount, Mult<Div<retainagePct, decimal100>, Div<retainageMaxPct, decimal100>>>>>))]
		[PXCurrency(typeof(PMRevenueBudget.curyInfoID), typeof(PMRevenueBudget.capAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Retainage Cap Amount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? CuryCapAmount
		{
			get;
			set;
		}
		#endregion

		#region CapAmount
		/// <exclude/>
		public abstract class capAmount : PX.Data.BQL.BqlDecimal.Field<capAmount> { }
		/// <summary>
		/// Retainage Cap Amount (in Base Currency)
		/// </summary>
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CapAmount
		{
			get;
			set;
		}
		#endregion

		#region CuryLastCostToComplete
		public new abstract class curyLastCostToComplete : PX.Data.BQL.BqlDecimal.Field<curyLastCostToComplete>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(PMRevenueBudget.curyInfoID), typeof(PMRevenueBudget.lastCostToComplete))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost To Complete", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public override Decimal? CuryLastCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CuryCostToComplete
		public new abstract class curyCostToComplete : PX.Data.BQL.BqlDecimal.Field<curyCostToComplete>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(PMRevenueBudget.curyInfoID), typeof(PMRevenueBudget.costToComplete))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost To Complete", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public override Decimal? CuryCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region LastPercentCompleted
		public new abstract class lastPercentCompleted : PX.Data.BQL.BqlDecimal.Field<lastPercentCompleted>
		{
		}

		/// <inheritdoc/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last % Completed", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public override Decimal? LastPercentCompleted
		{
			get;
			set;
		}
		#endregion
		#region PercentCompleted
		public new abstract class percentCompleted : PX.Data.BQL.BqlDecimal.Field<percentCompleted>
		{
		}

		/// <inheritdoc/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "% Completed", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public override Decimal? PercentCompleted
		{
			get;
			set;
		}
		#endregion
		#region CuryLastCostAtCompletion
		public new abstract class curyLastCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<curyLastCostAtCompletion>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(PMRevenueBudget.curyInfoID), typeof(PMRevenueBudget.lastCostAtCompletion))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost At Completion", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public override Decimal? CuryLastCostAtCompletion
		{
			get;
			set;
		}
		#endregion
		#region CostAtCompletion
		public new abstract class curyCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<curyCostAtCompletion>
		{
		}

		/// <inheritdoc/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost At Completion", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public override Decimal? CuryCostAtCompletion
		{
			get;
			set;
		}
		#endregion

		public new abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr>
		{
		}

		#region IsProduction
		public new abstract class isProduction : PX.Data.BQL.BqlBool.Field<isProduction>
		{
		}

		/// <inheritdoc/>
		[PXDefault(false)]
		[PXDBBool()]
		public override Boolean? IsProduction
		{
			get
			{
				return this._IsProduction;
			}
			set
			{
				this._IsProduction = value;
			}
		}
		#endregion
	}

	/// <summary>Represents a project budget line with the <see cref="GL.AccountType.Expense">Expense</see> type. The records of this type are created and edited through the <b>Cost Budget</b>
	/// tab of the Projects (PM301000) form (which corresponds to the <see cref="ProjectEntry" /> graph) and through the <b>Cost Budget</b> tab of the Projects Templates (PM208000)
	/// form (which corresponds to the <see cref="TemplateMaint" /> graph). The DAC is based on the <see cref="PMBudget" /> DAC.</summary>
	[PXCacheName(Messages.PMCostBudget)]
	[PXBreakInheritance]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMCostBudget : PMBudget
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		/// <exclude />
		public new class PK : PrimaryKeyOf<PMCostBudget>.By<PMCostBudget.projectID, PMCostBudget.projectTaskID, PMCostBudget.accountGroupID, PMCostBudget.costCodeID, PMCostBudget.inventoryID>
		{
			public static PMCostBudget Find(PXGraph graph, int? projectID, int? projectTaskID, int? accountGroupID, int? costCodeID, int? inventoryID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, projectID, projectTaskID, accountGroupID, costCodeID, inventoryID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		/// <exclude />
		public new static class FK
		{
			/// <summary>
			/// Project
			/// </summary>
			/// <exclude />
			public class Project : PMProject.PK.ForeignKeyOf<PMCostBudget>.By<projectID> { }

			/// <summary>
			/// Project Task
			/// </summary>
			/// <exclude />
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PMCostBudget>.By<projectID, projectTaskID> { }

			/// <summary>
			/// Account Group
			/// </summary>
			/// <exclude />
			public class AccountGroup : PMAccountGroup.PK.ForeignKeyOf<PMCostBudget>.By<accountGroupID> { }

			/// <summary>
			/// Cost Code
			/// </summary>
			/// <exclude />
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PMCostBudget>.By<costCodeID> { }

			/// <summary>
			/// Inventory Item
			/// </summary>
			/// <exclude />
			public class Item : IN.InventoryItem.PK.ForeignKeyOf<PMCostBudget>.By<inventoryID> { }
		}
		#endregion

		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}

		/// <inheritdoc/>
		[PXParent(typeof(Select<
			PMProject,
			Where<PMProject.contractID, Equal<Current<projectID>>,
				And<PMCostBudget.type, Equal<GL.AccountType.expense>>>>))]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[PXDBInt(IsKey = true)]
		public override Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region ProjectTaskID
		public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}

		/// <inheritdoc/>
		[PMTaskCompleted]
		[PXDefault(typeof(Search<
			PMTask.taskID,
			Where<PMTask.projectID, Equal<Current<PMCostBudget.projectID>>,
				And<PMTask.isDefault, Equal<True>>>>))]
		[PXParent(typeof(Select<
			PMTask,
			Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.taskID, Equal<Current<projectTaskID>>,
				And<PMCostBudget.type, Equal<GL.AccountType.expense>>>>>))]
		[ProjectTask(typeof(projectID), IsKey = true, AlwaysEnabled = true, DirtyRead = true)]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<projectTaskID>.IsRelatedTo<PMTask.taskID>>))]
		public override Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}

		/// <inheritdoc/>
		[CostCode(null, typeof(projectTaskID), GL.AccountType.Expense, typeof(accountGroupID), true, IsKey = true, Filterable = false, SkipVerification = true)]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public override Int32? CostCodeID
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
		#region AccountGroupID
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}

		/// <inheritdoc/>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXDefault]
		[AccountGroup(typeof(
			Where<PMAccountGroup.isExpense, Equal<True>>), IsKey = true)]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		public override Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}

		/// <inheritdoc/>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Inventory ID")]
		[PXDefault]
		[PMInventorySelector]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public override Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}

		/// <summary>
		/// The type of the budget line.
		/// </summary>
		/// <value>
		/// By default, its value is <see cref="GL.AccountType.Expense">Expense</see>.
		/// </value>
		[PXDBString(1)]
		[PXDefault(GL.AccountType.Expense)]
		[PMAccountType.List()]
		[PXUIField(DisplayName = "Budget Type", Visible = false, Enabled = false)]
		public override string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region RevenueTaskID
		public new abstract class revenueTaskID : PX.Data.BQL.BqlInt.Field<revenueTaskID>
		{
		}

		/// <inheritdoc/>
		[PXUIField(DisplayName = "Revenue Task")]
		[PXDBInt]
		//[PXSelector(typeof(Search5<PMTask.taskID, 
		//	InnerJoin<PMRevenueBudget, On<PMTask.projectID, Equal<PMRevenueBudget.projectID>, And<PMTask.taskID, Equal<PMRevenueBudget.projectTaskID>>>>,
		//	Where<PMRevenueBudget.projectID, Equal<Current<PMCostBudget.projectID>>, And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>,
		//	Aggregate<GroupBy<PMTask.taskID>>>), 
		//	typeof(PMTask.description),	SubstituteKey =typeof(PMTask.taskCD))]
		[PMRevenueBudgetLineTaskSelector]
		public override Int32? RevenueTaskID
		{
			get
			{
				return this._RevenueTaskID;
			}
			set
			{
				this._RevenueTaskID = value;
			}
		}
		#endregion
		#region RevenueInventoryID
		public new abstract class revenueInventoryID : PX.Data.BQL.BqlInt.Field<revenueInventoryID>
		{
		}

		/// <inheritdoc/>
		[PXUIField(DisplayName = "Revenue Item")]
		[PXDBInt]
		//[PMInventorySelector(typeof(Search2<InventoryItem.inventoryID,
		//	InnerJoin<PMRevenueBudget, On<PMRevenueBudget.inventoryID, Equal<InventoryItem.inventoryID>>>,
		//	Where<PMRevenueBudget.projectID, Equal<Current<PMCostBudget.projectID>>, 
		//	And<PMRevenueBudget.projectTaskID, Equal<Current<PMCostBudget.revenueTaskID>>>>>), typeof(PMRevenueBudget.description), SubstituteKey = typeof(InventoryItem.inventoryCD))]
		[PMRevenueBudgetLineSelector]
		public override Int32? RevenueInventoryID
		{
			get
			{
				return this._RevenueInventoryID;
			}
			set
			{
				this._RevenueInventoryID = value;
			}
		}
		#endregion

		#region Description
		public new abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}

		/// <inheritdoc/>
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public override String Description
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty>
		{
		}

		/// <inheritdoc/>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Quantity")]
		public override Decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}

		/// <inheritdoc/>
		[PXDefault(typeof(Search<
			InventoryItem.baseUnit,
			Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(inventoryID))]
		public override String UOM
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitRate
		public new abstract class curyUnitRate : PX.Data.BQL.BqlDecimal.Field<curyUnitRate> { }
		/// <summary>
		/// The cost of the specified unit of the cost budget line in the project currency.
		/// </summary>
		[PXDBCurrencyPriceCost(typeof(curyInfoID), typeof(rate))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Rate")]
		public override decimal? CuryUnitRate
		{
			get;
			set;
		}
		#endregion
		#region Rate
		public new abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }

		/// <summary>
		/// The cost of the specified unit of the cost budget line in the base currency of the tenant.
		/// </summary>
		[PXDBPriceCost]
		public override decimal? Rate
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitPrice
		public new abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrencyPriceCost(typeof(curyInfoID), typeof(unitPrice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price")]
		public override Decimal? CuryUnitPrice
		{
			get;
			set;
		}
		#endregion
		#region UnitPrice
		public new abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice>
		{
		}

		/// <inheritdoc/>
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price in Base Currency")]
		public override Decimal? UnitPrice
		{
			get;
			set;
		}
		#endregion
		#region CuryAmount
		public new abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(curyInfoID), typeof(amount))]
		[PXFormula(typeof(Mult<qty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount")]
		public override Decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public new abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount>
		{
		}

		/// <inheritdoc/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount in Base Currency")]
		public override Decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region RevisedQty
		public new abstract class revisedQty : PX.Data.BQL.BqlDecimal.Field<revisedQty>
		{
		}

		/// <inheritdoc/>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Quantity")]
		public override Decimal? RevisedQty
		{
			get;
			set;
		}
		#endregion
		#region CuryRevisedAmount
		public new abstract class curyRevisedAmount : PX.Data.BQL.BqlDecimal.Field<curyRevisedAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(curyInfoID), typeof(revisedAmount))]
		[PXFormula(typeof(Mult<revisedQty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount")]
		public override Decimal? CuryRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region RevisedAmount
		public new abstract class revisedAmount : PX.Data.BQL.BqlDecimal.Field<revisedAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount in Base Currency")]
		public override Decimal? RevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryInvoicedAmount
		public new abstract class curyInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<curyInvoicedAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBCurrency(typeof(curyInfoID), typeof(invoicedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Invoices Amount", Enabled = false)]
		public override Decimal? CuryInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region InvoicedAmount
		public new abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount>
		{
		}

		/// <inheritdoc/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Invoices Amount in Base Currency", Enabled = false)]
		public override Decimal? InvoicedAmount
		{
			get;
			set;
		}
		#endregion

		public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		public new abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty> { }
		public new abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount> { }
		public new abstract class draftChangeOrderQty : PX.Data.BQL.BqlDecimal.Field<draftChangeOrderQty> { }
		public new abstract class curyDraftChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyDraftChangeOrderAmount> { }
		public new abstract class changeOrderQty : PX.Data.BQL.BqlDecimal.Field<changeOrderQty> { }
		public new abstract class curyChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyChangeOrderAmount> { }
		public new abstract class committedQty : PX.Data.BQL.BqlDecimal.Field<committedQty> { }
		public new abstract class committedAmount : PX.Data.BQL.BqlDecimal.Field<committedAmount> { }
		public new abstract class committedOpenQty : PX.Data.BQL.BqlDecimal.Field<committedOpenQty> { }
		public new abstract class committedOpenAmount : PX.Data.BQL.BqlDecimal.Field<committedOpenAmount> { }
		public new abstract class committedReceivedQty : PX.Data.BQL.BqlDecimal.Field<committedReceivedQty> { }
		public new abstract class committedInvoicedQty : PX.Data.BQL.BqlDecimal.Field<committedInvoicedQty> { }
		public new abstract class committedInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<committedInvoicedAmount> { }
		public new abstract class curyActualPlusOpenCommittedAmount : PX.Data.BQL.BqlDecimal.Field<curyActualPlusOpenCommittedAmount> { }
		public new abstract class curyVarianceAmount : PX.Data.BQL.BqlDecimal.Field<curyVarianceAmount> { }
		public new abstract class performance : PX.Data.BQL.BqlDecimal.Field<performance> { }
		public new abstract class isProduction : PX.Data.BQL.BqlBool.Field<isProduction> { }
		public new abstract class productivityTracking : PX.Data.BQL.BqlString.Field<productivityTracking> { }

		public new abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr>
		{
		}

		#region RetainagePct
		public new abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct>
		{
		}

		/// <inheritdoc/>
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXDefault(typeof(Search<
			PMProject.retainagePct,
			Where<PMProject.contractID, Equal<Current<projectID>>>>))]
		[PXUIField(DisplayName = "Retainage (%)", FieldClass = nameof(FeaturesSet.Retainage))]
		public override decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(Messages.Budget)]
	[PXBreakInheritance]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMOtherBudget : PMBudget
	{
		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		[PXParent(typeof(Select<
			PMProject, 
			Where<PMProject.contractID, Equal<Current<projectID>>, 
				And<PMOtherBudget.type, NotEqual<GL.AccountType.income>, 
				And<PMOtherBudget.type, NotEqual<GL.AccountType.expense>>>>>))]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[PXDBInt(IsKey = true)]
		public override Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region ProjectTaskID
		public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}
		[PXDefault(typeof(Search<
			PMTask.taskID, 
			Where<PMTask.projectID, Equal<Current<projectID>>, 
				And<PMTask.isDefault, Equal<True>>>>))]
		[PXParent(typeof(Select<
			PMTask,
			Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.taskID, Equal<Current<projectTaskID>>,
				And<PMOtherBudget.type, NotEqual<GL.AccountType.income>, 
				And<PMOtherBudget.type, NotEqual<GL.AccountType.expense>>>>>>))]
		[ProjectTask(typeof(PMProject.contractID), IsKey = true, AlwaysEnabled = true, DirtyRead = true)]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<projectTaskID>.IsRelatedTo<PMTask.taskID>>))]
		public override Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		[CostCode(null, typeof(projectTaskID), IsKey = true, SkipVerification = true)]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public override Int32? CostCodeID
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
		#region AccountGroupID
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXDefault]
		[AccountGroup(typeof(
			Where<PMAccountGroup.isExpense, Equal<False>, 
				And<PMAccountGroup.type, NotEqual<GL.AccountType.income>>>), IsKey = true)]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		public override Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Inventory ID")]
		[PXDefault]
		[PMInventorySelector]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public override Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}
		[PXDBString(1)]
		[PXDefault(GL.AccountType.Asset)]
		public override string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion

		public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		#region Description
		public new abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public override String Description
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty>
		{
		}
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Quantity")]
		public override Decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		[PXDefault(typeof(Search<
			InventoryItem.baseUnit,
			Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(inventoryID))]
		public override String UOM
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitRate
		public new abstract class curyUnitRate : PX.Data.BQL.BqlDecimal.Field<curyUnitRate> { }
		[PXDBCurrencyPriceCost(typeof(curyInfoID), typeof(rate))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Rate")]
		public override decimal? CuryUnitRate
		{
			get;
			set;
		}
		#endregion
		#region Rate
		public new abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
		[PXDBPriceCost]
		public override decimal? Rate
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitPrice
		public new abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice>
		{
		}
		[PXDBCurrencyPriceCost(typeof(curyInfoID), typeof(unitPrice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price")]
		public override Decimal? CuryUnitPrice
		{
			get;
			set;
		}
		#endregion
		#region UnitPrice
		public new abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice>
		{
		}
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price in Base Currency")]
		public override Decimal? UnitPrice
		{
			get;
			set;
		}
		#endregion
		#region CuryAmount
		public new abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(amount))]
		[PXFormula(typeof(Mult<qty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount")]
		public override Decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public new abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount in Base Currency")]
		public override Decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region RevisedQty
		public new abstract class revisedQty : PX.Data.BQL.BqlDecimal.Field<revisedQty>
		{
		}
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Quantity")]
		public override Decimal? RevisedQty
		{
			get;
			set;
		}
		#endregion
		#region CuryRevisedAmount
		public new abstract class curyRevisedAmount : PX.Data.BQL.BqlDecimal.Field<curyRevisedAmount>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(revisedAmount))]
		[PXFormula(typeof(Mult<revisedQty, curyUnitRate>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount")]
		public override Decimal? CuryRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region RevisedAmount
		public new abstract class revisedAmount : PX.Data.BQL.BqlDecimal.Field<revisedAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount in Base Currency")]
		public override Decimal? RevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryInvoicedAmount
		public new abstract class curyInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<curyInvoicedAmount>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(invoicedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Invoices Amount", Enabled = false)]
		public override Decimal? CuryInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region InvoicedAmount
		public new abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Draft Invoices Amount in Base Currency", Enabled = false)]
		public override Decimal? InvoicedAmount
		{
			get;
			set;
		}
		#endregion
		public new abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty> { }
		public new abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount> { }
		public new abstract class changeOrderQty : PX.Data.BQL.BqlDecimal.Field<changeOrderQty> { }
		public new abstract class curyChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyChangeOrderAmount> { }
		public new abstract class committedQty : PX.Data.BQL.BqlDecimal.Field<committedQty> { }
		public new abstract class committedAmount : PX.Data.BQL.BqlDecimal.Field<committedAmount> { }
		public new abstract class committedOpenQty : PX.Data.BQL.BqlDecimal.Field<committedOpenQty> { }
		public new abstract class committedOpenAmount : PX.Data.BQL.BqlDecimal.Field<committedOpenAmount> { }
		public new abstract class committedReceivedQty : PX.Data.BQL.BqlDecimal.Field<committedReceivedQty> { }
		public new abstract class committedInvoicedQty : PX.Data.BQL.BqlDecimal.Field<committedInvoicedQty> { }
		public new abstract class committedInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<committedInvoicedAmount> { }
		public new abstract class curyActualPlusOpenCommittedAmount : PX.Data.BQL.BqlDecimal.Field<curyActualPlusOpenCommittedAmount> { }
		public new abstract class curyVarianceAmount : PX.Data.BQL.BqlDecimal.Field<curyVarianceAmount> { }
		public new abstract class performance : PX.Data.BQL.BqlDecimal.Field<performance> { }
		public new abstract class isProduction : PX.Data.BQL.BqlBool.Field<isProduction> { }

		public new abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr>
		{
		}

	}

	[Serializable]
	public class PMRevenueBudgetLineTaskSelectorAttribute : PXCustomSelectorAttribute
	{

		public PMRevenueBudgetLineTaskSelectorAttribute() : base(typeof(PMTask.taskID), typeof(PMTask.taskCD), typeof(PMTask.description))
		{
			SubstituteKey = typeof(PMTask.taskCD);
			DescriptionField = typeof(PMTask.description);

		}

		protected virtual IEnumerable GetRecords()
		{
			var selectRevenueBudget = new PXSelect<PMRevenueBudget,
					Where<PMRevenueBudget.projectID, Equal<Current<PMProject.contractID>>,
						And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(_Graph);

			HashSet<int> budgetedTasks = new HashSet<int>();
			foreach (PMRevenueBudget budget in selectRevenueBudget.Select())
			{
				budgetedTasks.Add(budget.TaskID.Value);
			}


			var select = new PXSelect<PMTask, 
				Where<PMTask.projectID, Equal<Current<PMProject.contractID>>>>(this._Graph);

			foreach (PMTask task in select.Select())
			{
				if (budgetedTasks.Contains(task.TaskID.Value))
				{
					yield return task;
				}
			}
		}
	}

	[Serializable]
	public class PMRevenueBudgetLineSelectorAttribute : PXCustomSelectorAttribute
	{
		public PMRevenueBudgetLineSelectorAttribute() : base(typeof(InventoryItem.inventoryID), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
			SubstituteKey = typeof(InventoryItem.inventoryCD);
			DescriptionField = typeof(InventoryItem.descr);
		}

		protected virtual IEnumerable GetRecords()
		{
			object current = null;
			if (PXView.Currents != null && PXView.Currents.Length > 0)
			{
				current = PXView.Currents[0];
			}
			else
			{
				current = _Graph.Caches[_CacheType].Current;
			}

			var select = new PXSelectJoin<PMRevenueBudget,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<PMRevenueBudget.inventoryID>>>,
					Where<PMRevenueBudget.projectID, Equal<Current<PMCostBudget.projectID>>,
						And<PMRevenueBudget.projectTaskID, Equal<Current<PMCostBudget.revenueTaskID>>,
						And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>>(this._Graph);

			List<InventoryItem> list = new List<InventoryItem>();
			foreach (PXResult<PMRevenueBudget, InventoryItem> res in select.View.SelectMultiBound(new object[] { current, current }))
			{
				InventoryItem item = (InventoryItem)res;
				PMRevenueBudget budget = (PMRevenueBudget)res;

				item.Descr = budget.Description;

				list.Add(item);
			}

			return list;
		}
	}
}

namespace PX.Objects.PM.Lite
{
	[PXCacheName(Messages.Budget)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMBudget : IBqlTable, IProjectFilter, IQuantify
	{		
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		protected Int32? _ProjectID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}
		/// <summary>
		/// Get or set Project TaskID
		/// </summary>
		public int? TaskID => ProjectTaskID;
		[PMTaskCompleted]
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		protected Int32? _CostCodeID;
		[PXDBInt(IsKey = true)]
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
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}
		protected Int32? _AccountGroupID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
				
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty>
		{
		}
		
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		public virtual String UOM
		{
			get;
			set;
		}
		#endregion
		#region CuryRevisedAmount
		public abstract class curyRevisedAmount : PX.Data.BQL.BqlDecimal.Field<curyRevisedAmount>
		{
		}
		[PXDBDecimal]
		public virtual decimal? CuryRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryActualAmount
		public abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount> { }
		[PXDBDecimal]
		public virtual decimal? CuryActualAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedOpenAmount
		public abstract class curyCommittedOpenAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedOpenAmount>
		{
		}
		[PXDBDecimal]
		public virtual decimal? CuryCommittedOpenAmount
		{
			get;
			set;
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}
		
		[PXDBString(1)]
		[PXDefault]
		public virtual string Type
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}
		protected String _Description;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region IsProduction
		public abstract class isProduction : PX.Data.BQL.BqlBool.Field<isProduction>
		{
		}

		[PXDefault(false)]
		[PXDBBool()]
		public virtual Boolean? IsProduction
		{
			get;
			set;
		}
		#endregion
		#region ProgressBillingBase
		public abstract class progressBillingBase : Data.BQL.BqlString.Field<progressBillingBase> { }

		[PXDBString]
		[PXDefault]
		public string ProgressBillingBase { get; set; }
		#endregion
	}
}
