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
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// Represents a detail line for the history of a project budget forecast.
	/// Contains actual values and change order values for the forecast detail lines
	/// as totals by the budget complex key and financial period. 
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.PMForecastHistory)]
	[PXPrimaryGraph(typeof(ForecastMaint))]
	[Serializable]
	public class PMForecastHistory : IBqlTable, IProjectFilter
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}

		/// <summary>
		/// The <see cref="PMProject">project</see> of the project budget forecast.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get;
			set;
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

		/// <summary>
		/// The <see cref="PMTask">project task</see> of the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion

		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}

		/// <summary>
		/// The <see cref="PMAccountGroup">project account group</see> of the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.GroupID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public virtual Int32? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}

		/// <summary>
		/// The <see cref="InventoryItem">inventory item</see> of the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}

		/// <summary>
		/// The <see cref="PMCostCode">project cost code</see> of the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostCode.CostCodeID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID>
		{
		}

		/// <summary>
		/// The financial period.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="GL.FinPeriods.TableDefinition.FinPeriod.FinPeriodID"/> field.
		/// </value>
		[GL.FinPeriodID(IsKey = true)]
		public virtual String PeriodID
		{
			get;
			set;
		}
		#endregion
				
		#region ActualQty
		public abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty>
		{
		}

		/// <summary>
		/// The total quantity of the lines of the released AR invoices that correspond to the project budget line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Quantity", Enabled = false)]
		public virtual Decimal? ActualQty
		{
			get;
			set;
		}
		#endregion
		#region CuryArAmount
		public abstract class curyArAmount : PX.Data.BQL.BqlDecimal.Field<curyArAmount>
		{
		}

		/// <summary>
		/// The total amount of the lines of the released AR invoices that correspond to the line of project budget history.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount generated by AR Invoices", Enabled = false)]
		public virtual Decimal? CuryArAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryActualAmount
		public abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount>
		{
		}

		/// <summary>
		/// The total amount of the lines of the released AR invoices that correspond to the project budget line.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount", Enabled = false)]
		public virtual Decimal? CuryActualAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualAmount
		public abstract class actualAmount : PX.Data.BQL.BqlDecimal.Field<actualAmount>
		{
		}

		/// <summary>
		/// The <see cref="CuryActualAmount">actual amount</see> in the base currency.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount in Base Currency", Enabled = false)]
		public virtual Decimal? ActualAmount
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
		[PXDBBaseCury]
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
		/// The inclusive tax amount in base currency calculated from the data of <see cref="ARTran.TaxAmt"/>
		/// and from the data of <see cref="ARTax.RetainedTaxAmt"/>.
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
		/// The total quantity of the potential changes to the project budget.
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
		/// The total amount of the potential changes to the project budget.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Potential CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryDraftChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderQty
		public abstract class changeOrderQty : PX.Data.BQL.BqlDecimal.Field<changeOrderQty>
		{
		}

		/// <summary>
		/// The total quantity of the lines of released change orders that are associated with the project budget line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? ChangeOrderQty
		{
			get;
			set;
		}
		#endregion
		#region CuryChangeOrderAmount
		public abstract class curyChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyChangeOrderAmount>
		{
		}

		/// <summary>
		/// The total amount of the lines of released change orders that are associated with the project budget line.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryChangeOrderAmount
		{
			get;
			set;
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
	}

	[PXBreakInheritance]
	[PMForecastHistoryAccum]
	[Serializable]
	[PXHidden]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMForecastHistoryAccum : PMForecastHistory
	{
		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}

		/// <summary>
		/// The <see cref="PMProject">project</see> of the project budget forecast.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public override Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}

		/// <summary>
		/// The <see cref="PMTask">project task</see> of the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public override Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion

		#region AccountGroupID
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}

		/// <summary>
		/// The <see cref="PMAccountGroup">project account group</see> of the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.GroupID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public override Int32? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}

		/// <summary>
		/// The <see cref="InventoryItem">inventory item</see> of the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public override Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}

		/// <summary>
		/// The <see cref="PMCostCode">project cost code</see> of the budget line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostCode.CostCodeID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public override Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region PeriodID
		public new abstract class periodID : PX.Data.BQL.BqlString.Field<periodID>
		{
		}

		/// <summary>
		/// The financial period.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="GL.FinPeriods.TableDefinition.FinPeriod.FinPeriodID"/> field.
		/// </value>
		[GL.FinPeriodID(IsKey = true)]
		public override String PeriodID
		{
			get;
			set;
		}
		#endregion
	}
}
