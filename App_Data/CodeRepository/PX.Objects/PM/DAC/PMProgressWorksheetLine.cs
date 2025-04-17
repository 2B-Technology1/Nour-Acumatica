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
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.TM;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// Contains the main properties of a progress worksheet line. The records of this type are created and edited through the <b>Details</b>
	/// tab of the Progress Worksheets (PM303000) form (which corresponds to the <see cref="ProgressWorksheetEntry" /> graph).
	/// </summary>
	[PXCacheName(Messages.ProgressWorksheetLine)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMProgressWorksheetLine : IBqlTable, IProjectFilter, IQuantify
	{
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <summary>
		/// The original sequence number of the line among all the progress worksheet lines.
		/// </summary>
		/// <remarks>The sequence of line numbers of the progress worksheet lines that belong to a single document can include gaps.</remarks>
		[PXUIField(DisplayName = "Line Number", Visible = false)]
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(PMProgressWorksheet.lineCntr))]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
			public const int Length = 15;
		}

		/// <summary>
		/// The reference number of the parent <see cref="PMProgressWorksheet">progress worksheet</see>.
		/// </summary>
		[PXDBString(refNbr.Length, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXSelector(typeof(Search<PMProgressWorksheet.refNbr>))]
		[PXUIField(DisplayName = "Worksheet Nbr.", Enabled = false)]
		[PXDBDefault(typeof(PMProgressWorksheet.refNbr))]
		[PXParent(typeof(Select<PMProgressWorksheet, Where<PMProgressWorksheet.refNbr, Equal<Current<refNbr>>>>))]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see> associated with the progress worksheet line.
		/// </summary>
		/// <value>
		/// By default, the value of this field is set to the <see cref="PMProgressWorksheet.ProjectID">project ID</see> of the parent progress worksheet.
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDefault(typeof(PMProgressWorksheet.projectID))]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		/// <summary>
		/// The identifier of the <see cref="PMTask">task</see> associated with the progress worksheet line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.TaskID"/> field.
		/// </value>
		[PXRestrictor(typeof(Where<PMTask.status, Equal<ProjectTaskStatus.active>, Or<PMTask.status, Equal<ProjectTaskStatus.planned>>>), PM.Messages.PWProjectTaskNotActive, typeof(PMTask.taskCD), typeof(PMTask.status))]
		[PXDefault(typeof(Search<PMTask.taskID,
			Where<PMTask.projectID, Equal<Current<projectID>>,
				And<PMTask.isDefault, Equal<True>,
				And<PMTask.status, NotEqual<ProjectTaskStatus.canceled>,
				And<PMTask.status, NotEqual<ProjectTaskStatus.completed>>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveOrInPlanningProjectTask(typeof(PMProgressWorksheetLine.projectID), DisplayName = "Project Task")]
		[PXForeignReference(typeof(Field<taskID>.IsRelatedTo<PMTask.taskID>))]
		public virtual Int32? TaskID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		/// <summary>
		/// The identifier of the <see cref="InventoryItem">inventory item</see> associated with the progress worksheet line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		[PXDefault]
		[PXUIField(DisplayName = "Inventory ID")]
		[PXDBInt()]
		[PXSelector(typeof(SelectFrom<InventoryItem>
			.InnerJoin<PMCostBudget>.On<InventoryItem.inventoryID.IsEqual<PMCostBudget.inventoryID>>
			.Where<PMCostBudget.projectID.IsEqual<PMProject.contractID.FromCurrent>.And<PMCostBudget.type.IsEqual<GL.AccountType.expense>>.And<PMCostBudget.inventoryID.IsNotNull>>
			.AggregateTo<GroupBy<PMCostBudget.inventoryID>>
			.SearchFor<InventoryItem.inventoryID>),
			SubstituteKey = typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

		/// <summary>
		/// The identifier of the <see cref="PMCostCode">cost code</see> associated with the cost part of the estimation line.
		/// </summary>
		[PXDefault]
		[PXUIField(DisplayName = "Cost Code", FieldClass = CostCodeAttribute.COSTCODE)]
		[PXDBInt()]
		[PXRestrictor(typeof(Where<PMCostCode.isActive, Equal<True>>), Objects.PM.Messages.CostCodeInactiveWithFormat, typeof(PMCostCode.costCodeCD))]
		[PXDimensionSelector(CostCodeAttribute.COSTCODE, typeof(SelectFrom<PMCostCode>
			.InnerJoin<PMCostBudget>.On<PMCostCode.costCodeID.IsEqual<PMCostBudget.costCodeID>>
			.Where<PMCostBudget.projectID.IsEqual<PMProject.contractID.FromCurrent>.And<PMCostBudget.type.IsEqual<GL.AccountType.expense>>.And<PMCostBudget.costCodeID.IsNotNull>>
			.AggregateTo<GroupBy<PMCostBudget.costCodeID>>
			.SearchFor<PMCostCode.costCodeID>), typeof(PMCostCode.costCodeCD))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }

		/// <summary>The identifier of the <see cref="PMAccountGroup">account group</see> associated with the progress worksheet line.</summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.GroupID" /> field.
		/// </value>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXDefault]
		[PXUIField(DisplayName = "Account Group")]
		[PXDBInt()]
		[PXSelector(typeof(SelectFrom<PMAccountGroup>
			.InnerJoin<PMCostBudget>.On<PMAccountGroup.groupID.IsEqual<PMCostBudget.accountGroupID>>
			.Where<PMCostBudget.projectID.IsEqual<PMProject.contractID.FromCurrent>.And<PMCostBudget.type.IsEqual<GL.AccountType.expense>>.And<PMCostBudget.accountGroupID.IsNotNull>
				.And<PMAccountGroup.isExpense.IsEqual<True>>>
			.AggregateTo<GroupBy<PMCostBudget.accountGroupID>>
			.SearchFor<PMAccountGroup.groupID>),
			SubstituteKey = typeof(PMAccountGroup.groupCD))]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		public virtual Int32? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The description of the progress worksheet line.
		/// </summary>
		[PXString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

		/// <summary>
		/// The UOM of the progress worksheet line.
		/// </summary>
		[PXString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "UOM", Enabled = false)]
		public virtual String UOM
		{
			get;
			set;
		}
		#endregion

		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

		/// <summary>
		/// The completed quantity for the units that are produced or installed from the previous progress worksheet date.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Completed Quantity")]
		public virtual Decimal? Qty
		{
			get;
			set;
		}
		#endregion

		#region Previously Completed Quantity
		public abstract class previouslyCompletedQuantity : PX.Data.BQL.BqlDecimal.Field<previouslyCompletedQuantity> { }

		/// <summary>
		/// /// The sum of the values of the <see cref="PMProgressWorksheetLine.Qty">Completed Quantity</see> fields (for the lines with the same project key) of the progress worksheets that are released up to and including the current progress worksheet date. (The values of the current document are not included in the sum.)
		/// </summary>
		[PXQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Previously Completed Quantity", Enabled = false)]
		public virtual Decimal? PreviouslyCompletedQuantity
		{
			get;
			set;
		}
		#endregion
		#region Prior Period Quantity
		public abstract class priorPeriodQuantity : PX.Data.BQL.BqlDecimal.Field<priorPeriodQuantity> { }

		/// <summary>
		/// The sum of the values of the <see cref="PMProgressWorksheetLine.Qty">Completed Quantity</see> fields for the released progress worksheets with dates within the previous financial period.
		/// </summary>
		[PXQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prior Period Quantity", Enabled = false)]
		public virtual Decimal? PriorPeriodQuantity
		{
			get;
			set;
		}
		#endregion
		#region Current Period Quantity
		public abstract class currentPeriodQuantity : PX.Data.BQL.BqlDecimal.Field<currentPeriodQuantity> { }

		/// <summary>
		/// The sum of the values of the <see cref="PMProgressWorksheetLine.Qty">Completed Quantity</see> fields for the released progress worksheets with the dates that start from the first day of the current financial period and that end with the current date of the document. (The date that is specified in a progress worksheet should be a part of the current period.)
		/// </summary>
		[PXQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Current Period Quantity", Enabled = false)]
		public virtual Decimal? CurrentPeriodQuantity
		{
			get;
			set;
		}
		#endregion
		#region Total Completed Quantity
		public abstract class totalCompletedQuantity : PX.Data.BQL.BqlDecimal.Field<totalCompletedQuantity> { }

		/// <summary>
		/// The sum of <see cref="PMProgressWorksheetLine.PreviouslyCompletedQuantity"/> and <see cref="PMProgressWorksheetLine.Qty"/>.
		/// </summary>
		[PXQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Completed Quantity", Enabled = false)]
		public virtual Decimal? TotalCompletedQuantity
		{
			get;
			set;
		}
		#endregion
		#region Completed (%), Total
		public abstract class completedPercentTotalQuantity : PX.Data.BQL.BqlDecimal.Field<completedPercentTotalQuantity> { }

		/// <summary>
		/// The total dompleted quantity devided by the total budgeted quantity.
		/// </summary>
		[PXDecimal(2)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Completed (%), Total", Enabled = false)]
		public virtual Decimal? CompletedPercentTotalQuantity
		{
			get;
			set;
		}
		#endregion
		#region Total Budgeted Quantity
		public abstract class totalBudgetedQuantity : PX.Data.BQL.BqlDecimal.Field<totalBudgetedQuantity>
		{
		}

		/// /// <summary>
		/// The total budgeted quantity.
		/// </summary>
		///<value>
		///The value of this field is equal to the revised budgeted quantity, which is specified on the Cost Budget tab of the Projects (PM301000) form for the project specified for this progress worksheet.
		///</value>
		[PXQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Budgeted Quantity", Enabled = false)]
		public virtual Decimal? TotalBudgetedQuantity
		{
			get;
			set;
		}
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		/// <summary>
		/// The employee associated with the progress worksheet line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds the value of the <see cref="EPEmployee.PK"/> field.
		/// </value>
		[PXUIField(DisplayName = "Employee", Visible = false)]
		[PXChildUpdatable(AutoRefresh = true)]
		[SubordinateOwnerEmployee(DisplayName = "Employee",Visible = false)]
		public virtual int? OwnerID { get; set; }
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		/// <summary>
		/// The workgroup associated with the progress worksheet line.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PX.TM.EPCompanyTree.WorkGroupID">EPCompanyTree.WorkGroupID</see> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup", Visible = false)]
		[PXWorkgroupSelector]
		public virtual Int32? WorkgroupID
		{
			get;
			set;
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(PMProgressWorksheetLine.refNbr))]
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
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
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
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
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
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
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
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
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
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
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
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
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
}
