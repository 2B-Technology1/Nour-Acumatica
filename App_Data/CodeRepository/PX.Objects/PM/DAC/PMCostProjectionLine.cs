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
using PX.Objects.CM.Extensions;
using PX.Objects.CN.ProjectAccounting;
using PX.Objects.Common;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// Represents a cost projection revision line.
	/// The records of this type are created and edited through the Cost Projection (PM305000) form
	/// (which corresponds to the <see cref="CostProjectionEntry"/> graph).
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.CostProjectionLine)]
	[PXPrimaryGraph(typeof(CostProjectionEntry))]
	[Serializable]
	public class PMCostProjectionLine : PX.Data.IBqlTable
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PMCostProjectionLine>.By<projectID, revisionID, lineNbr>
		{
			public static PMCostProjectionLine Find(PXGraph graph, int? projectID, string revisionID, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, projectID, revisionID, lineNbr, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Cost Projection
			/// </summary>
			public class CostProjection : PMCostProjection.PK.ForeignKeyOf<PMCostProjectionLine>.By<projectID, revisionID> { }

			/// <summary>
			/// Project
			/// </summary>
			public class Project : PMProject.PK.ForeignKeyOf<PMCostProjection>.By<projectID> { }

			/// <summary>
			/// Project Task
			/// </summary>
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PMCostProjection>.By<projectID, taskID> { }

			/// <summary>
			/// Account Group
			/// </summary>
			public class AccountGroup : PMAccountGroup.PK.ForeignKeyOf<PMCostProjection>.By<accountGroupID> { }

			/// <summary>
			/// Cost Code
			/// </summary>
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PMCostProjection>.By<costCodeID> { }

			/// <summary>
			/// Inventory Item
			/// </summary>
			public class Item : IN.InventoryItem.PK.ForeignKeyOf<PMCostProjection>.By<inventoryID> { }
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// The <see cref="PMProject">project</see> for which the cost projection revision line is created.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[PXDefault(typeof(PMCostProjection.projectID))]
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

		/// <summary>
		/// The revision identifier of the cost projection.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostProjection.revisionID"/> field.
		/// </value>
		[PXDBString(30, IsKey = true)]
		[PXDefault(typeof(PMCostProjection.revisionID))]
		[PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string RevisionID
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <summary>
		/// The number of the cost projection line.
		/// </summary>
		[PXParent(typeof(Select<PMCostProjection, Where<PMCostProjection.projectID, Equal<Current<projectID>>, And<PMCostProjection.revisionID, Equal<Current<revisionID>>>>>))]
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PMCostProjection.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion
		
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		/// <summary>
		/// The identifier of the <see cref="PMTask">project task</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID"/> field.
		/// </value>
		[ProjectTask(typeof(projectID),	AlwaysEnabled = true, DisplayName = "Cost Task", AllowNull = true)]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<taskID>.IsRelatedTo<PMTask.taskID>>))]
		public virtual Int32? TaskID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }

		/// <summary>
		/// The identifier of the <see cref="PMAccountGroup">project account group</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.GroupID"/> field.
		/// </value>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[AccountGroup(typeof(Where<PMAccountGroup.isExpense, Equal<True>>))]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		public virtual Int32? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

		/// <summary>
		/// The identifier of the <see cref="PMCostCode">project cost code</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostCode.CostCodeID"/> field.
		/// </value>
		[CostCode(null, typeof(taskID), PX.Objects.GL.AccountType.Expense, typeof(accountGroupID), SkipVerification = true, AllowNullValue = true)]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		/// <summary>
		/// The identifier of the <see cref="InventoryItem">inventory item</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Inventory ID")]
		[PMInventorySelector]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The description of the cost budget line.
		/// </summary>
		[PXDBString(Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

		/// <summary>
		/// The unit of measure of the cost budget line.
		/// </summary>
		[PXDBString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "UOM", Enabled = false)]
		public virtual String UOM
		{
			get;
			set;
		}
		#endregion
		#region BudgetedQuantity
		public abstract class budgetedQuantity : PX.Data.BQL.BqlDecimal.Field<budgetedQuantity> { }

		/// <summary>
		/// The budgeted quantity of the cost budget line specified in the project.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalBudgetedQuantity>), ForceAggregateRecalculation = true)]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted Quantity", Enabled = false)]
		public virtual Decimal? BudgetedQuantity
		{
			get;
			set;
		}
		#endregion
		#region BudgetedAmount
		public abstract class budgetedAmount : PX.Data.BQL.BqlDecimal.Field<budgetedAmount> { }

		/// <summary>
		/// The budgeted cost of the cost budget line specified in the project.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalBudgetedAmount>), ForceAggregateRecalculation = true)]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted Cost", Enabled = false)]
		public virtual Decimal? BudgetedAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualQuantity
		public abstract class actualQuantity : PX.Data.BQL.BqlDecimal.Field<actualQuantity> { }

		/// <summary>
		/// The total quantity of the released project transactions that correspond to the cost budget line.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalActualQuantity>), ForceAggregateRecalculation = true)]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Quantity", Enabled = false, Visible = false)]
		public virtual Decimal? ActualQuantity
		{
			get;
			set;
		}
		#endregion
		#region ActualAmount
		public abstract class actualAmount : PX.Data.BQL.BqlDecimal.Field<actualAmount> { }

		/// <summary>
		/// The total amount of the released project transactions that correspond to the cost budget line.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalActualAmount>), ForceAggregateRecalculation = true)]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Cost", Enabled = false, Visible = false)]
		public virtual Decimal? ActualAmount
		{
			get;
			set;
		}
		#endregion
		#region UnbilledQuantity
		public abstract class unbilledQuantity : PX.Data.BQL.BqlDecimal.Field<unbilledQuantity> { }

		/// <summary>
		/// The committed open quantity of the cost budget line.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalUnbilledQuantity>), ForceAggregateRecalculation = true)]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Quantity", Enabled = false, Visible = false)]
		public virtual Decimal? UnbilledQuantity
		{
			get;
			set;
		}
		#endregion
		#region UnbilledAmount
		public abstract class unbilledAmount : PX.Data.BQL.BqlDecimal.Field<unbilledAmount> { }

		/// <summary>
		/// The committed open cost of the cost budget line.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalUnbilledAmount>), ForceAggregateRecalculation = true)]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Cost", Enabled = false, Visible = false)]
		public virtual Decimal? UnbilledAmount
		{
			get;
			set;
		}
		#endregion

		#region CompletedQuantity
		public abstract class completedQuantity : PX.Data.BQL.BqlDecimal.Field<completedQuantity> { }

		/// <summary>
		/// The sum of the actual quantity and committed open quantity of the cost budget line specified in the project.
		/// </summary>
		[PXQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual + Committed Open Quantity", Enabled = false)]
		public virtual Decimal? CompletedQuantity
		{
			[PXDependsOnFields(typeof(actualQuantity), typeof(unbilledQuantity))]
			get { return ActualQuantity.GetValueOrDefault() + UnbilledQuantity.GetValueOrDefault(); }

		}
		#endregion
		#region CompletedAmount
		public abstract class completedAmount : PX.Data.BQL.BqlDecimal.Field<completedAmount> { }

		/// <summary>
		/// The sum of the actual cost and committed open cost of the cost budget line specified in the project.
		/// </summary>
		[PXBaseCury()]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual + Committed Open Cost", Enabled = false)]
		public virtual Decimal? CompletedAmount
		{
			[PXDependsOnFields(typeof(actualAmount), typeof(unbilledAmount))]
			get { return ActualAmount.GetValueOrDefault() + UnbilledAmount.GetValueOrDefault(); }
		}
		#endregion
		#region QuantityToComplete
		public abstract class quantityToComplete : PX.Data.BQL.BqlDecimal.Field<quantityToComplete> { }

		/// <summary>
		/// The remainder of the budgeted quantity of the cost budget line that is currently available for completion.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalQuantityToComplete>), ForceAggregateRecalculation = true)]
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Quantity to Complete", Enabled = false)]
		public virtual Decimal? QuantityToComplete
		{
			[PXDependsOnFields(typeof(budgetedQuantity), typeof(completedQuantity))]
			get { return Math.Max(0, BudgetedQuantity.GetValueOrDefault() - CompletedQuantity.GetValueOrDefault()); }
		}
		#endregion
		#region AmountToComplete
		public abstract class amountToComplete : PX.Data.BQL.BqlDecimal.Field<amountToComplete> { }

		/// <summary>
		/// The remainder of the budgeted cost of the cost budget line that is currently available for completion.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalAmountToComplete>), ForceAggregateRecalculation = true)]
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cost to Complete", Enabled = false)]
		public virtual Decimal? AmountToComplete
		{
			[PXDependsOnFields(typeof(budgetedAmount), typeof(completedAmount))]
			get { return Math.Max(0, BudgetedAmount.GetValueOrDefault() - CompletedAmount.GetValueOrDefault()); }
		}
		#endregion
		#region Mode
		public abstract class mode : PX.Data.BQL.BqlString.Field<mode> { }

		/// <summary>
		/// The mode that specifies how the amounts and quantities are recalculated for the cost budget line.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ProjectionMode.ListAttribute"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[ProjectionMode.List()]
		[PXDefault(ProjectionMode.Auto)]
		[PXUIField(DisplayName = "Mode")]
		public virtual String Mode
		{
			get;
			set;
		}
		#endregion
		#region Quantity
		public abstract class quantity : PX.Data.BQL.BqlDecimal.Field<quantity> { }

		/// <summary>
		/// The projected remainder of the budgeted quantity for the cost budget line.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalQuantity>), ForceAggregateRecalculation = true)]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Quantity to Complete")]
		public virtual Decimal? Quantity
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

		/// <summary>
		/// The projected remainder of the budgeted cost for the cost budget line.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalAmount>), ForceAggregateRecalculation = true)]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost to Complete")]
		public virtual Decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region ProjectedQuantity
		public abstract class projectedQuantity : PX.Data.BQL.BqlDecimal.Field<projectedQuantity> { }

		/// <summary>
		/// The projected final quantity at the moment of project completion for the cost budget line.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalProjectedQuantity>), ForceAggregateRecalculation = true)]
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Quantity at Completion")]
		public virtual Decimal? ProjectedQuantity
		{
			get;
			set;
		}
		#endregion
		#region ProjectedAmount
		public abstract class projectedAmount : PX.Data.BQL.BqlDecimal.Field<projectedAmount> { }

		/// <summary>
		/// The projected final cost at the moment of project completion for the cost budget line.
		/// </summary>
		[PXFormula(null, typeof(SumCalc<PMCostProjection.totalProjectedAmount>), ForceAggregateRecalculation = true)]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost at Completion")]
		public virtual Decimal? ProjectedAmount
		{
			get;
			set;
		}
		#endregion
		#region VarianceQuantity
		public abstract class varianceQuantity : PX.Data.BQL.BqlDecimal.Field<varianceQuantity> { }

		/// <summary>
		/// The difference between <see cref="ProjectedQuantity"/> and <see cref="BudgetedQuantity"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Variance Quantity")]
		public virtual Decimal? VarianceQuantity
		{
			get;
			set;
		}
		#endregion
		#region VarianceAmount
		public abstract class varianceAmount : PX.Data.BQL.BqlDecimal.Field<varianceAmount> { }

		/// <summary>
		/// The difference between the <see cref="amount">projected cost</see> at completion and the <see cref="budgetedAmount"/>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Variance Cost")]
		public virtual Decimal? VarianceAmount
		{
			get;
			set;
		}
		#endregion
		#region CompletedPct
		public abstract class completedPct : PX.Data.BQL.BqlDecimal.Field<completedPct> { }

		/// <summary>
		/// The projected percentage of completion for the cost budget line.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Completed (%)")]
		public virtual Decimal? CompletedPct
		{
			get;
			set;
		}
		#endregion
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
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
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get; set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get; set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get; set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get; set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get; set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get; set;
		}
		#endregion
		#endregion
	}
}
