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
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// Represents a detail line of a revision of a project budget forecast.
	/// The records of this type are created and edited through the Project Budget Forecast (PM209600) form
	/// (which corresponds to the <see cref="ForecastMaint"/> graph).
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.PMForecastDetail)]
	[PXPrimaryGraph(typeof(ForecastMaint))]
	[Serializable]
	public class PMForecastDetail : IBqlTable, IProjectFilter
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
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID>
		{
		}

		/// <summary>
		/// The identifier of the <see cref="PMForecast">project budget forecast revision</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMForecast.RevisionID"/> field.
		/// </value>
		[PXParent(typeof(Select<PMForecast, Where<PMForecast.projectID, Equal<Current<projectID>>, And<PMForecast.revisionID, Equal<Current<revisionID>>>>>))]
		[PXDBString(15, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Revision")]
		public virtual string RevisionID
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
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>))]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<projectTaskID>.IsRelatedTo<PMTask.taskID>>))]
		[BaseProjectTaskAttribute(typeof(projectID), IsKey = true, Enabled = false, AllowCompleted = true, AllowCanceled = true)]
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
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		[PXDefault]
		[AccountGroupAttribute(IsKey = true, Enabled = false)]
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
		[PXUIField(DisplayName = "Inventory ID", Enabled = false)]
		[PMInventorySelector]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>))]
		[PXDefault]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
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
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		[CostCode(null, typeof(projectTaskID), null, typeof(accountGroupID), true, IsKey = true, Enabled = false, Filterable = false, SkipVerification = true)]
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
		[PXDefault()]
		[PXUIField(DisplayName = "Financial Period", Enabled = false)]
		[PXSelector(typeof(MasterFinPeriod.finPeriodID), DescriptionField = typeof(MasterFinPeriod.descr))]
		public virtual String PeriodID
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
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
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
		/// The original budgeted quantity.
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
		#region CuryAmount
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
		{
		}

		/// <summary>
		/// The original budgeted amount.
		/// </summary>
		[PXDBBaseCury]//Stored in Project currency only.
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Amount")]
		public virtual Decimal? CuryAmount
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
		/// The revised budgeted amount.
		/// </summary>
		[PXDBBaseCury]//Stored in Project currency only.
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount")]
		public virtual Decimal? CuryRevisedAmount
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
		[PXNote(DescriptionField = typeof(description))]
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

	/// <summary>
	/// The virtual DAC that represents a detail line of a project budget forecast.
	/// The records of this type are created and edited through the Project Budget Forecast (PM209600) form
	/// (which corresponds to the <see cref="ForecastMaint"/> graph).
	/// </summary>
	[PXHidden]
	[PXPrimaryGraph(typeof(ForecastMaint))]
	public class PMForecastRecord : IBqlTable, IProjectFilter
	{
		public const string SummaryFinPeriod = "000000";
		public const string TotalFinPeriod = "999998";
		public const string DifferenceFinPeriod = "999999";

		//Keys:

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.ProjectID"/>
		[PXUnboundDefault(typeof(PMForecast.projectID))]
		[PXInt(IsKey = true)]
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

		/// <inheritdoc cref="PMForecastDetail.ProjectTaskID"/>
		[PXInt(IsKey = true)]
		[PXDimensionSelector(ProjectTaskAttribute.DimensionName, typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>>>), typeof(PMTask.taskCD))]
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

		/// <inheritdoc cref="PMForecastDetail.AccountGroupID"/>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXInt(IsKey = true)]
		[PXDimensionSelector(AccountGroupAttribute.DimensionName, typeof(Search<PMAccountGroup.groupID>), typeof(PMAccountGroup.groupCD))]
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

		/// <inheritdoc cref="PMForecastDetail.InventoryID"/>
		[PXInt(IsKey = true)]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID>), typeof(InventoryItem.inventoryCD))]
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

		/// <inheritdoc cref="PMForecastDetail.CostCodeID"/>
		[PXInt(IsKey = true)]
		[PXDimensionSelector(CostCodeAttribute.COSTCODE, typeof(Search<PMCostCode.costCodeID>), typeof(PMCostCode.costCodeCD))]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.IBqlField
		{
		}

		/// <inheritdoc cref="PMForecastDetail.PeriodID"/>
		[PXString(6, IsKey = true, IsFixed = true)]
		public virtual String FinPeriodID
		{
			get;
			set;
		}
		#endregion


		//UI Visible Key Properties:

		#region ProjectTask
		public abstract class projectTask : PX.Data.BQL.BqlInt.Field<projectTask>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.ProjectTaskID"/>
		[PXInt]
		[PXUIField(DisplayName = "Project Task")]
		[PXDimensionSelector(ProjectTaskAttribute.DimensionName, typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>>>), typeof(PMTask.taskCD))]
		public virtual int? ProjectTask
		{
			get;
			set;
		}
		#endregion
		#region AccountGroup
		public abstract class accountGroup : PX.Data.BQL.BqlInt.Field<accountGroup>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.AccountGroupID"/>
		[PXInt]
		[PXUIField(DisplayName = "Account Group")]
		[PXDimensionSelector(AccountGroupAttribute.DimensionName, typeof(Search<PMAccountGroup.groupID>), typeof(PMAccountGroup.groupCD))]
		public virtual int? AccountGroup
		{
			get;
			set;
		}
		#endregion
		#region Inventory
		public abstract class inventory : PX.Data.BQL.BqlInt.Field<inventory>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.InventoryID"/>
		[PXInt]
		[PXUIField(DisplayName = "Inventory ID")]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID>), typeof(InventoryItem.inventoryCD))]
		public virtual int? Inventory
		{
			get;
			set;
		}
		#endregion
		#region CostCode
		public abstract class costCode : PX.Data.BQL.BqlInt.Field<costCode>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.CostCodeID"/>
		[PXInt]
		[PXUIField(DisplayName = "Cost Code", FieldClass = CostCodeAttribute.COSTCODE)]
		[PXDimensionSelector(CostCodeAttribute.COSTCODE, typeof(Search<PMCostCode.costCodeID>), typeof(PMCostCode.costCodeCD))]
		public virtual int? CostCode
		{
			get;
			set;
		}
		#endregion
		#region FinPeriod
		public abstract class period : PX.Data.BQL.BqlString.Field<period>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.PeriodID"/>
		[PXString()]
		[PXUIField(DisplayName = "Financial Period")]
		public virtual String Period
		{
			get;
			set;
		}
		#endregion

		#region AccountGroupType
		public abstract class accountGroupType : PX.Data.BQL.BqlString.Field<accountGroupType>
		{
		}

		/// <summary>
		/// The type of the associated account group.
		/// </summary>
		[PXString(1)]
		public virtual string AccountGroupType
		{
			get;
			set;

		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.Description"/>
		[PXString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
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

		/// <inheritdoc cref="PMForecastDetail.Qty"/>
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Original Budgeted Quantity")]
		public virtual Decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region CuryAmount
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.CuryAmount"/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Original Budgeted Amount")]
		public virtual Decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region RevisedQty
		public abstract class revisedQty : PX.Data.BQL.BqlDecimal.Field<revisedQty>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.RevisedQty"/>
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
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

		/// <inheritdoc cref="PMForecastDetail.CuryRevisedAmount"/>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Revised Budgeted Amount")]
		public virtual Decimal? CuryRevisedAmount
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
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Actual Quantity", Enabled = false)]
		public virtual Decimal? ActualQty
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
		[PXDBCurrency(typeof(PMProject.curyInfoID), typeof(PMForecastRecord.actualAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Actual Amount in Base Currency", Enabled = false)]
		public virtual Decimal? ActualAmount
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
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDBBaseCury]//In Project Currency only
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDBBaseCury]//In Project Currency only
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Budgeted CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CuryChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		
		#region CuryVarianceAmount
		public abstract class curyVarianceAmount : PX.Data.BQL.BqlDecimal.Field<curyVarianceAmount>
		{
		}

		/// <summary>
		/// The difference between the <see cref="CuryRevisedAmount">revised budgeted amount</see> and the <see cref="CuryActualAmount">actual amount</see>.
		/// </summary>
		[PXBaseCury]
		[PXUIField(DisplayName = "Revised Amount - Actual Amount", Enabled = false)]
		public virtual Decimal? CuryVarianceAmount
		{
			[PXDependsOnFields(typeof(curyRevisedAmount), typeof(curyActualAmount))]
			get
			{
				if (this.CuryActualAmount == null)
				{
					return null;
				}

				return this.CuryRevisedAmount - this.CuryActualAmount;
			}
		}
		#endregion
		

		#region VarianceQuantity
		public abstract class varianceQuantity : PX.Data.BQL.BqlDecimal.Field<varianceQuantity>
		{
		}

		/// <summary>
		/// The difference between the <see cref="RevisedQty">revised budgeted quantity</see> and the <see cref="ActualQty">actual quantity</see>.
		/// </summary>
		[PXQuantity]
		[PXUIField(DisplayName = "Revised Quantity - Actual Quantity", Enabled = false)]
		public virtual Decimal? VarianceQuantity
		{
			[PXDependsOnFields(typeof(revisedQty), typeof(actualQty))]
			get
			{
				if (this.ActualQty == null)
				{
					return null;
				}

				return this.RevisedQty - this.ActualQty;
			}
		}
		#endregion


		public bool IsSummary
		{
			get
			{
				return FinPeriodID == SummaryFinPeriod;
			}
		}

		public bool IsTotal
		{
			get
			{
				return FinPeriodID == TotalFinPeriod;
			}
		}

		public bool IsDifference
		{
			get
			{
				return FinPeriodID == DifferenceFinPeriod;
			}
		}


		//PMTask:
		#region PlannedStartDate
		public abstract class plannedStartDate : PX.Data.BQL.BqlDateTime.Field<plannedStartDate>
		{
		}

		/// <summary>
		/// The date when the project task is expected to start.
		/// </summary>
		[PXDate]
		[PXUIField(DisplayName = "Planned Start Date", Enabled = false)]
		public virtual DateTime? PlannedStartDate
		{
			get;
			set;
		}
		#endregion
		#region PlannedEndDate
		public abstract class plannedEndDate : PX.Data.BQL.BqlDateTime.Field<plannedEndDate>
		{
		}

		/// <summary>
		///  The date when the project task is expected to end.
		/// </summary>
		[PXDate]
		[PXUIField(DisplayName = "Planned End Date", Enabled = false)]
		public virtual DateTime? PlannedEndDate
		{
			get;
			set;
		}
		#endregion
	}

	/// <summary>
	/// The projection DAC that represents a <see cref="PMBudget">project budget line</see> connected with the <see cref="PMTask">project task</see> and the <see cref="PMAccountGroup">account group</see> associated with it.
	/// </summary>
	[PXHidden]
	[PXProjection(typeof(Select2<PMBudget,
		InnerJoin<PMTask, On<PMBudget.projectID, Equal<PMTask.projectID>, And<PMBudget.projectTaskID, Equal<PMTask.taskID>>>,
		InnerJoin<PMAccountGroup, On<PMBudget.accountGroupID, Equal<PMAccountGroup.groupID>>>>>), new Type[] { typeof(PMBudget) })]
	public class PMBudgetInfo : IBqlTable, IProjectFilter
	{
		//PMBudget:

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}

		/// <inheritdoc cref="PMBudget.ProjectID"/>
		[PXDefault()]
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectID))]
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

		/// <inheritdoc cref="PMBudget.ProjectTaskID"/>
		[PXDimensionSelector(ProjectTaskAttribute.DimensionName, typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>>>), typeof(PMTask.taskCD))]
        [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectTaskID))]
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

		/// <inheritdoc cref="PMBudget.AccountGroupID"/>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXDimensionSelector(AccountGroupAttribute.DimensionName, typeof(Search<PMAccountGroup.groupID>), typeof(PMAccountGroup.groupCD))]
        [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.accountGroupID))]
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

		/// <inheritdoc cref="PMBudget.InventoryID"/>
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID>), typeof(InventoryItem.inventoryCD))]
        [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.inventoryID))]
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

		/// <inheritdoc cref="PMBudget.CostCodeID"/>
		[PXDimensionSelector(CostCodeAttribute.COSTCODE, typeof(Search<PMCostCode.costCodeID>), typeof(PMCostCode.costCodeCD))]
        [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.costCodeID))]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}

		/// <inheritdoc cref="PMBudget.Description"/>
		[PXDBString(256, IsUnicode = true, BqlField = typeof(PMBudget.description))]
		[PXUIField(DisplayName = "Description")]
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

		/// <inheritdoc cref="PMBudget.Qty"/>
		[PXDBQuantity(BqlField = typeof(PMBudget.qty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Budgeted Quantity")]
		public virtual Decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region CuryAmount
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
		{
		}

		/// <inheritdoc cref="PMBudget.CuryAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyAmount))]
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

		/// <inheritdoc cref="PMBudget.Amount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.amount))]
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

		/// <inheritdoc cref="PMBudget.RevisedQty"/>
		[PXDBQuantity(BqlField = typeof(PMBudget.revisedQty))]
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

		/// <inheritdoc cref="PMBudget.CuryRevisedAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyRevisedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount")]
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

		/// <inheritdoc cref="PMBudget.RevisedAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.revisedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Budgeted Amount in Base Currency")]
		public virtual Decimal? RevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualQty
		public abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty>
		{
		}

		/// <inheritdoc cref="PMBudget.ActualQty"/>
		[PXDBQuantity(BqlField = typeof(PMBudget.actualQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Quantity", Enabled = false)]
		public virtual Decimal? ActualQty
		{
			get;
			set;
		}
		#endregion
		#region CuryActualAmount
		public abstract class curyActualAmount : PX.Data.BQL.BqlDecimal.Field<curyActualAmount>
		{
		}

		/// <inheritdoc cref="PMBudget.CuryActualAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyActualAmount))]
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

		/// <inheritdoc cref="PMBudget.ActualAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.actualAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Amount in Base Currency", Enabled = false)]
		public virtual Decimal? ActualAmount
		{
			get;
			set;
		}
		#endregion
		#region DraftChangeOrderQty
		public abstract class draftChangeOrderQty : PX.Data.BQL.BqlDecimal.Field<draftChangeOrderQty>
		{
		}

		/// <inheritdoc cref="PMBudget.DraftChangeOrderQty"/>
		[PXDBQuantity(BqlField = typeof(PMBudget.draftChangeOrderQty))]
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

		/// <inheritdoc cref="PMBudget.CuryDraftChangeOrderAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyDraftChangeOrderAmount))]
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

		/// <inheritdoc cref="PMBudget.DraftChangeOrderAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.draftChangeOrderAmount))]
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

		/// <inheritdoc cref="PMBudget.ChangeOrderQty"/>
		[PXDBQuantity(BqlField = typeof(PMBudget.changeOrderQty))]
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

		/// <inheritdoc cref="PMBudget.CuryChangeOrderAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.curyChangeOrderAmount))]
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

		/// <inheritdoc cref="PMBudget.ChangeOrderAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMBudget.changeOrderAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted CO Amount in Base Currency", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? ChangeOrderAmount
		{
			get;
			set;
		}
		#endregion

		//PMAccountGroup:

		#region AccountGroupType
		public abstract class accountGroupType : PX.Data.BQL.BqlString.Field<accountGroupType>
		{
		}

		/// <inheritdoc cref="PMAccountGroup.Type"/>
		[PXDBString(1, BqlField = typeof(PMAccountGroup.type))]
		public virtual string AccountGroupType
		{
			get;
			set;

		}
		#endregion
		#region IsExpense
		public abstract class isExpense : PX.Data.BQL.BqlBool.Field<isExpense>
		{
		}

		/// <inheritdoc cref="PMAccountGroup.IsExpense"/>
		[PXDBBool(BqlField = typeof(PMAccountGroup.isExpense))]		
		public virtual Boolean? IsExpense
		{
			get;
			set;
		}
		#endregion

		//PMTask:
		#region PlannedStartDate
		public abstract class plannedStartDate : PX.Data.BQL.BqlDateTime.Field<plannedStartDate>
		{
		}

		/// <inheritdoc cref="PMTask.PlannedStartDate"/>
		[PXDBDate(BqlField = typeof(PMTask.plannedStartDate))]
		[PXUIField(DisplayName = "Planned Start Date", Enabled = false)]
		public virtual DateTime? PlannedStartDate
		{
			get;
			set;
		}
		#endregion
		#region PlannedEndDate
		public abstract class plannedEndDate : PX.Data.BQL.BqlDateTime.Field<plannedEndDate>
		{
		}

		/// <inheritdoc cref="PMTask.PlannedEndDate"/>
		[PXDBDate(BqlField = typeof(PMTask.plannedEndDate))]
		[PXUIField(DisplayName = "Planned End Date", Enabled = false)]
		public virtual DateTime? PlannedEndDate
		{
			get;
			set;
		}
		#endregion

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		
		[PXDBLastModifiedDateTime(BqlField = typeof(PMBudget.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime { get; set; }
	}

	/// <summary>
	/// The projection DAC that represents a <see cref="PMForecastDetail">project budget forecast line</see> connected with the associated <see cref="PMAccountGroup">account group</see>.
	/// </summary>
	[PXHidden]
	[PXBreakInheritance]
	[PXProjection(typeof(Select2<PMForecastDetail,
		InnerJoin<PMAccountGroup, On<PMForecastDetail.accountGroupID, Equal<PMAccountGroup.groupID>>>>), new Type[] { typeof(PMForecastDetail) })]
	public class PMForecastDetailInfo : PMForecastDetail
	{
		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.ProjectID"/>
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXDBInt(IsKey = true)]
		public override Int32? ProjectID
		{
			get;
			set;
		}
		#endregion

		#region RevisionID
		public new abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.RevisionID"/>
		[PXParent(typeof(Select<PMForecast, Where<PMForecast.projectID, Equal<Current<PMForecastDetailInfo.projectID>>, And<PMForecast.revisionID, Equal<Current<PMForecastDetailInfo.revisionID>>>>>))]
		[PXDBString(15, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Revision")]
		public override string RevisionID
		{
			get;
			set;
		}
		#endregion

		#region ProjectTaskID
		public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID>
		{
		}

		/// <inheritdoc cref="PMForecastDetail.ProjectTaskID"/>
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>))]
		[BaseProjectTaskAttribute(typeof(projectID), IsKey = true, Enabled = false, AllowCompleted = true, AllowCanceled = true)]
		public override Int32? ProjectTaskID
		{
			get;
			set;
		}
		#endregion

		//PMAccountGroup:

		#region AccountGroupType
		public abstract class accountGroupType : PX.Data.BQL.BqlString.Field<accountGroupType>
		{
		}

		/// <inheritdoc cref="PMAccountGroup.Type"/>
		[PXDBString(1, BqlField = typeof(PMAccountGroup.type))]
		public virtual string AccountGroupType
		{
			get;
			set;

		}
		#endregion

		#region IsExpense
		public abstract class isExpense : PX.Data.BQL.BqlBool.Field<isExpense>
		{
		}

		/// <inheritdoc cref="PMAccountGroup.IsExpense"/>
		[PXDBBool(BqlField = typeof(PMAccountGroup.isExpense))]
		public virtual Boolean? IsExpense
		{
			get;
			set;
		}
		#endregion
	}

	/// <summary>
	/// The projection DAC that represents a <see cref="PMForecastHistory">project budget forecast history</see> connected with associated <see cref="PMAccountGroup">account group</see>.
	/// </summary>
	[PXHidden]
	[PXBreakInheritance]
	[PXProjection(typeof(Select2<PMForecastHistory,
		InnerJoin<PMAccountGroup, On<PMForecastHistory.accountGroupID, Equal<PMAccountGroup.groupID>>>>), Persistent = false)]
	public class PMForecastHistoryInfo : PMForecastHistory
	{		
		//PMAccountGroup:

		#region AccountGroupType
		public abstract class accountGroupType : PX.Data.BQL.BqlString.Field<accountGroupType>
		{
		}

		/// <inheritdoc cref="PMAccountGroup.Type"/>
		[PXDBString(1, BqlField = typeof(PMAccountGroup.type))]
		public virtual string AccountGroupType
		{
			get;
			set;

		}
		#endregion

		#region IsExpense
		public abstract class isExpense : PX.Data.BQL.BqlBool.Field<isExpense>
		{
		}

		/// <inheritdoc cref="PMAccountGroup.IsExpense"/>
		[PXDBBool(BqlField = typeof(PMAccountGroup.isExpense))]
		public virtual Boolean? IsExpense
		{
			get;
			set;
		}
		#endregion
	}
}
