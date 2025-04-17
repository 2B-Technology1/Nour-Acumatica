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
using PX.Objects.EP;
using PX.Objects.CM.Extensions;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PM;
using System;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CN
{
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select5<PMForecastHistory,
        CrossJoin<FinPeriod>,
        Where<FinPeriod.finPeriodID, GreaterEqual<minPeriod>>,
        Aggregate<
            GroupBy<PMForecastHistory.projectID,
            GroupBy<PMForecastHistory.projectTaskID,
            GroupBy<PMForecastHistory.costCodeID,
            GroupBy<PMForecastHistory.accountGroupID,
            GroupBy<FinPeriod.finPeriodID>>>>>>>))]
    public class PMForecastHistoryByPeriods : IBqlTable
    {
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistory.projectID))]
        public virtual int? ProjectID
        {
            get;
            set;
        }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistory.projectTaskID))]
        public virtual int? ProjectTaskID
        {
            get;
            set;
        }
        #endregion
        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistory.costCodeID))]
        public virtual int? CostCodeID
        {
            get;
            set;
        }
        #endregion
        #region AccountGroupID
        public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistory.accountGroupID))]
        public virtual int? AccountGroupID
        {
            get;
            set;
        }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        [FinPeriodID(IsKey = true, BqlField = typeof(FinPeriod.finPeriodID))]
        public virtual string FinPeriodID
        {
            get;
            set;
        }
        #endregion

        public sealed class minPeriod : PX.Data.BQL.BqlString.Constant<minPeriod>
        {
            public minPeriod() : base("201201") { }
        }
    }

    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select5<PMForecastHistoryByPeriods,
        LeftJoin<PMForecastHistory, On<PMForecastHistory.projectID, Equal<PMForecastHistoryByPeriods.projectID>,
            And<PMForecastHistory.projectTaskID, Equal<PMForecastHistoryByPeriods.projectTaskID>,
            And<PMForecastHistory.costCodeID, Equal<PMForecastHistoryByPeriods.costCodeID>,
            And<PMForecastHistory.accountGroupID, Equal<PMForecastHistoryByPeriods.accountGroupID>,
            And<PMForecastHistory.periodID, LessEqual<PMForecastHistoryByPeriods.finPeriodID>>>>>>>,
        Aggregate<
            GroupBy<PMForecastHistoryByPeriods.projectID,
            GroupBy<PMForecastHistoryByPeriods.projectTaskID,
            GroupBy<PMForecastHistoryByPeriods.costCodeID,
            GroupBy<PMForecastHistoryByPeriods.accountGroupID,
            GroupBy<PMForecastHistoryByPeriods.finPeriodID,
            Sum<PMForecastHistory.curyActualAmount>>>>>>>>))]
    public class PMPTDData : IBqlTable
    {
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistoryByPeriods.projectID))]
        public virtual int? ProjectID
        {
            get;
            set;
        }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistoryByPeriods.projectTaskID))]
        public virtual int? ProjectTaskID
        {
            get;
            set;
        }
        #endregion
        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistoryByPeriods.costCodeID))]
        public virtual int? CostCodeID
        {
            get;
            set;
        }
        #endregion
        #region AccountGroupID
        public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistoryByPeriods.accountGroupID))]
        public virtual int? AccountGroupID
        {
            get;
            set;
        }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        [FinPeriodID(IsKey = true, BqlField = typeof(PMForecastHistoryByPeriods.finPeriodID))]
        public virtual string FinPeriodID
        {
            get;
            set;
        }
        #endregion
        #region FinPTDAmount
        public abstract class finPTDAmount : PX.Data.BQL.BqlDecimal.Field<finPTDAmount> { }
        [PXDBBaseCury(BqlField = typeof(PMForecastHistory.curyActualAmount))]
        public virtual decimal? FinPTDAmount
        {
            get;
            set;
        }
        #endregion
    }

    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select2<PMBudget, CrossJoin<EPEarningType>,
    Where<PMBudget.type, Equal<AccountType.income>,
        And<Where<EPEarningType.typeCD, Equal<EPSetup.EarningTypeRG>, Or<EPEarningType.typeCD, Equal<EPSetup.EarningTypeHL>>>>>>))]
    public class PMRevisedCOAmount : IBqlTable
    {
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get;
            set;
        }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectID))]
        public virtual int? ProjectID
        {
            get;
            set;
        }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectTaskID))]
        public virtual int? ProjectTaskID
        {
            get;
            set;
        }
        #endregion
        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.costCodeID))]
        public virtual int? CostCodeID
        {
            get;
            set;
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.inventoryID))]
        public virtual int? InventoryID
        {
            get;
            set;
        }
        #endregion
        #region AccountGroupID
        public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
        [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.accountGroupID))]
        public virtual int? AccountGroupID
        {
            get;
            set;
        }
        #endregion

        #region Type
        public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
        [PXString(255, IsKey = true)]
        [PXUIField(DisplayName = "Type")]
        [PXDBCalced(typeof(Switch<Case<Where<EPEarningType.typeCD, Equal<EPSetup.EarningTypeRG>>, approvedChangeOrder>, revisedContract>), typeof(string))]
        public virtual string Type
        {
            get;
            set;
        }
        #endregion
        #region RevContractAndCOAmt
        public abstract class revContractAndCOAmt : PX.Data.BQL.BqlDecimal.Field<revContractAndCOAmt> { }
        [PXDecimal]
        [PXUIField(DisplayName = "Rev Contract And COAmt")]
        [PXDBCalced(typeof(Switch<Case<Where<EPEarningType.typeCD, Equal<EPSetup.EarningTypeRG>>, PMBudget.changeOrderAmount>, Add<PMBudget.amount, PMBudget.changeOrderAmount>>), typeof(decimal))]
        public virtual decimal? RevContractAndCOAmt
        {
            get;
            set;
        }

        public sealed class approvedChangeOrder : PX.Data.BQL.BqlString.Constant<approvedChangeOrder>
        {
            public approvedChangeOrder() : base("Approved Change Order") { }
        }

        public sealed class revisedContract : PX.Data.BQL.BqlString.Constant<revisedContract>
        {
            public revisedContract() : base("Revised Contract") { }
        }
    }
	#endregion

	[PXCacheName(PM.Messages.PMReportProject)]
	[Serializable]
	[PXProjection(typeof(Select<PMProject>))]
	[PXPrimaryGraph(new Type[] { typeof(ProjectEntry) },
		new Type[] {
		typeof(Select<PMProject,
			Where<PMProject.contractID, Equal<Current<contractID>>,
			And<PMProject.baseType, Equal<CTPRType.project>,
			And<PMProject.nonProject, Equal<False>>>>>)
		})]
	public class PMReportProject : IBqlTable
	{
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		[PXDBInt(BqlField = typeof(PMProject.contractID))]
		[PXSelector(typeof(Search<contractID, Where<baseType, Equal<CTPRType.project>>>), SubstituteKey = typeof(contractCD))]
		public virtual int? ContractID
		{
			get;
			set;
		}
		#endregion
		#region BaseType
		public abstract class baseType : PX.Data.BQL.BqlString.Field<baseType> { }
		[PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(PMProject.baseType))]
		public virtual string BaseType
		{
			get;
			set;
		}
		#endregion
		#region ContractCD
		public abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }
		[PXDimensionSelector(ProjectAttribute.DimensionName,
			typeof(Search<contractCD,
				Where<baseType, Equal<CTPRType.project>,
					And<nonProject, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>))]
		[PXDBString(IsKey = true, InputMask = "", BqlField = typeof(PMProject.contractCD))]
		public virtual string ContractCD
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(60, BqlField = typeof(PMProject.description))]
		public virtual string Description
		{
			get;
			set;
		}
		#endregion
		#region DefaultBranchID
		public abstract class defaultBranchID : PX.Data.BQL.BqlInt.Field<defaultBranchID> { }
		[Branch(IsDetail = true, BqlField = typeof(PMProject.defaultBranchID))]
		public virtual int? DefaultBranchID
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(1, BqlField = typeof(PMProject.status))]
		[ProjectStatus.List()]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		[PXDBDate(BqlField = typeof(PMProject.startDate))]
		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		[PXDBDate(BqlField = typeof(PMProject.expireDate))]
		public virtual DateTime? ExpireDate
		{
			get;
			set;
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool(BqlField = typeof(PMProject.isActive))]
		public virtual bool? IsActive
		{
			get;
			set;
		}
		#endregion
		#region IsCompleted
		public abstract class isCompleted : PX.Data.BQL.BqlBool.Field<isCompleted> { }
		[PXDBBool(BqlField = typeof(PMProject.isCompleted))]
		public virtual bool? IsCompleted
		{
			get;
			set;
		}
		#endregion
		#region IsCancelled
		public abstract class isCancelled : PX.Data.BQL.BqlBool.Field<isCancelled> { }
		[PXDBBool(BqlField = typeof(PMProject.isCancelled))]
		public virtual bool? IsCancelled
		{
			get;
			set;
		}
		#endregion
		#region NonProject
		public abstract class nonProject : PX.Data.BQL.BqlBool.Field<nonProject> { }
		[PXDBBool(BqlField = typeof(PMProject.nonProject))]
		public virtual bool? NonProject
		{
			get;
			set;
		}
		#endregion
		#region Bas
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXDBString(BqlField = typeof(PMProject.curyID))]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(PM.Messages.PMWipBudget)]
	[Serializable]
	[PXProjection(typeof(SelectFrom<PMBudget>))]
	public class PMWipBudget : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectTaskID))]
		public virtual int? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.costCodeID))]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.accountGroupID))]
		public virtual int? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.inventoryID))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(1, BqlField = typeof(PMBudget.type))]
		public virtual string Type
		{
			get;
			set;
		}
		#endregion

		#region CuryAmount
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount> { }
		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyAmount))]
		public virtual decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region OriginalContractAmount
		public abstract class originalContractAmount : PX.Data.BQL.BqlDecimal.Field<originalContractAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.income>>, curyAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? OriginalContractAmount
		{
			get;
			set;
		}
		#endregion
		#region OriginalCostAmount
		public abstract class originalCostAmount : PX.Data.BQL.BqlDecimal.Field<originalCostAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>>, curyAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? OriginalCostAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryChangeOrderAmount
		public abstract class curyChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyChangeOrderAmount> { }
		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyChangeOrderAmount))]
		public virtual decimal? CuryChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderContractAmount
		public abstract class changeOrderContractAmount : PX.Data.BQL.BqlDecimal.Field<changeOrderContractAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.income>>, curyChangeOrderAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? ChangeOrderContractAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderCostAmount
		public abstract class changeOrderCostAmount : PX.Data.BQL.BqlDecimal.Field<changeOrderCostAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>>, curyChangeOrderAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? ChangeOrderCostAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryCostToComplete
		public abstract class curyCostToComplete : PX.Data.BQL.BqlDecimal.Field<curyCostToComplete> { }
		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyCostToComplete))]
		public virtual decimal? CuryCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CostToComplete
		public abstract class costToComplete : PX.Data.BQL.BqlDecimal.Field<costToComplete> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>>, curyCostToComplete>, decimal0>), typeof(decimal))]
		public virtual decimal? CostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CostProjectionCostAtCompletion
		public abstract class costProjectionCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<costProjectionCostAtCompletion> { }
		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyCostProjectionCostAtCompletion))]
		public virtual decimal? CostProjectionCostAtCompletion
		{
			get;
			set;
		}
		#endregion
		#region CostProjectionCostToComplete
		public abstract class costProjectionCostToComplete : PX.Data.BQL.BqlDecimal.Field<costProjectionCostToComplete> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>>, PMBudget.curyCostProjectionCostToComplete>, decimal0>), typeof(decimal))]
		public virtual decimal? CostProjectionCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedOrigAmount
		public abstract class curyCommittedOrigAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedOrigAmount> { }
		
		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyCommittedOrigAmount))]
		public virtual decimal? CuryCommittedOrigAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedCOAmount
		public abstract class curyCommittedCOAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedCOAmount> { }

		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyCommittedCOAmount))]
		public virtual decimal? CuryCommittedCOAmount
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(PM.Messages.PMWipCostProjection)]
	[Serializable]
	[PXProjection(typeof(
		SelectFrom<PMCostProjectionLine>
			.InnerJoin<PMCostProjection>
				.On<PMCostProjectionLine.projectID.IsEqual<PMCostProjection.projectID>>
			.Where<PMCostProjection.released.IsEqual<True>>
			.AggregateTo<GroupBy<PMCostProjectionLine.projectID>,
				GroupBy<PMCostProjectionLine.costCodeID>,
				GroupBy<PMCostProjectionLine.accountGroupID>,
				GroupBy<PMCostProjectionLine.taskID>,
				GroupBy<PMCostProjectionLine.inventoryID>>))]
	public class PMWipCostProjection : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMCostProjectionLine.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		[PXDBInt(BqlField = typeof(PMCostProjectionLine.taskID))]
		public virtual int? TaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[PXDBInt(BqlField = typeof(PMCostProjectionLine.costCodeID))]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		[PXDBInt(BqlField = typeof(PMCostProjectionLine.accountGroupID))]
		public virtual int? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(BqlField = typeof(PMCostProjectionLine.inventoryID))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(PM.Messages.PMWipCostProjectionBudget)]
	[Serializable]
	[PXProjection(typeof(
		SelectFrom<PMBudget>
			.LeftJoin<PMWipCostProjection>
				.On<PMBudget.projectID.IsEqual<PMWipCostProjection.projectID>
					.And<PMBudget.costCodeID.IsEqual<PMWipCostProjection.costCodeID>
					.And<PMBudget.accountGroupID.IsEqual<PMWipCostProjection.accountGroupID>
					.And<PMBudget.projectTaskID.IsEqual<PMWipCostProjection.taskID>
					.And<PMBudget.inventoryID.IsEqual<PMWipCostProjection.inventoryID>>>>>>))]
	public class PMWipCostProjectionBudget : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectTaskID))]
		public virtual int? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.costCodeID))]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.accountGroupID))]
		public virtual int? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMBudget.inventoryID))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(1, BqlField = typeof(PMBudget.type))]
		public virtual string Type
		{
			get;
			set;
		}
		#endregion

		#region CuryAmount
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount> { }
		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyAmount))]
		public virtual decimal? CuryAmount
		{
			get;
			set;
		}
		#endregion
		#region OriginalContractAmount
		public abstract class originalContractAmount : PX.Data.BQL.BqlDecimal.Field<originalContractAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.income>>, curyAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? OriginalContractAmount
		{
			get;
			set;
		}
		#endregion
		#region OriginalCostAmount
		public abstract class originalCostAmount : PX.Data.BQL.BqlDecimal.Field<originalCostAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>>, curyAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? OriginalCostAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryChangeOrderAmount
		public abstract class curyChangeOrderAmount : PX.Data.BQL.BqlDecimal.Field<curyChangeOrderAmount> { }
		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyChangeOrderAmount))]
		public virtual decimal? CuryChangeOrderAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderContractAmount
		public abstract class changeOrderContractAmount : PX.Data.BQL.BqlDecimal.Field<changeOrderContractAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.income>>, curyChangeOrderAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? ChangeOrderContractAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderCostAmount
		public abstract class changeOrderCostAmount : PX.Data.BQL.BqlDecimal.Field<changeOrderCostAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>>, curyChangeOrderAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? ChangeOrderCostAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryCostToComplete
		public abstract class curyCostToComplete : PX.Data.BQL.BqlDecimal.Field<curyCostToComplete> { }
		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyCostToComplete))]
		public virtual decimal? CuryCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CostToComplete
		public abstract class costToComplete : PX.Data.BQL.BqlDecimal.Field<costToComplete> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>>, curyCostToComplete>, decimal0>), typeof(decimal))]
		public virtual decimal? CostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CostProjectionCostAtCompletion
		public abstract class costProjectionCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<costProjectionCostAtCompletion> { }
		[PXDBDecimal(1, BqlField = typeof(PMBudget.curyCostProjectionCostAtCompletion))]
		public virtual decimal? CostProjectionCostAtCompletion
		{
			get;
			set;
		}
		#endregion
		#region CostProjectionCostToComplete
		public abstract class costProjectionCostToComplete : PX.Data.BQL.BqlDecimal.Field<costProjectionCostToComplete> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>>, PMBudget.curyCostProjectionCostToComplete>, decimal0>), typeof(decimal))]
		public virtual decimal? CostProjectionCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedOrigAmount
		public abstract class curyCommittedOrigAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedOrigAmount> { }

		[PXDBDecimal(2, BqlField = typeof(PMBudget.curyCommittedOrigAmount))]
		public virtual decimal? CuryCommittedOrigAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedCOAmount
		public abstract class curyCommittedCOAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedCOAmount> { }

		[PXDecimal(2)]
		[PXDBCalced(typeof(Sub<PMBudget.curyCommittedAmount, PMBudget.curyCommittedOrigAmount>), typeof(decimal))]
		public virtual decimal? CuryCommittedCOAmount
		{
			get;
			set;
		}
		#endregion

		#region ProjectedCostAtCompletion
		public abstract class projectedCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<projectedCostAtCompletion> { }
		[PXDecimal]
		[PXDBCalced(typeof(
			Switch<
				Case<Where<type, Equal<AccountType.expense>, And<PMWipCostProjection.projectID, IsNotNull>>, PMBudget.curyCostProjectionCostAtCompletion,
				Case<Where<type, Equal<AccountType.expense>>, PMBudget.curyRevisedAmount>>,
				decimal0>), typeof(decimal))]
		public virtual decimal? ProjectedCostAtCompletion
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(PM.Messages.PMWipTotalBudget)]
	[Serializable]
	[PXProjection(typeof(
		SelectFrom<PMWipCostProjectionBudget>
			.AggregateTo<GroupBy<PMWipCostProjectionBudget.projectID,
				Sum<PMWipCostProjectionBudget.originalContractAmount,
				Sum<PMWipCostProjectionBudget.originalCostAmount,
				Sum<PMWipCostProjectionBudget.changeOrderContractAmount,
				Sum<PMWipCostProjectionBudget.changeOrderCostAmount,
				Sum<PMWipCostProjectionBudget.costToComplete,
				Sum<PMWipCostProjectionBudget.costProjectionCostAtCompletion,
				Sum<PMWipCostProjectionBudget.costProjectionCostToComplete,
				Sum<PMWipCostProjectionBudget.curyCommittedOrigAmount,
				Sum<PMWipCostProjectionBudget.curyCommittedCOAmount,
				Sum<PMWipCostProjectionBudget.projectedCostAtCompletion>>>>>>>>>>>>))]
	public class PMWipTotalBudget : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMWipCostProjectionBudget.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion

		#region OriginalContractAmount
		public abstract class originalContractAmount : PX.Data.BQL.BqlDecimal.Field<originalContractAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipCostProjectionBudget.originalContractAmount))]
		public virtual decimal? OriginalContractAmount
		{
			get;
			set;
		}
		#endregion
		#region OriginalCostAmount
		public abstract class originalCostAmount : PX.Data.BQL.BqlDecimal.Field<originalCostAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipCostProjectionBudget.originalCostAmount))]
		public virtual decimal? OriginalCostAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderContractAmount
		public abstract class changeOrderContractAmount : PX.Data.BQL.BqlDecimal.Field<changeOrderContractAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipCostProjectionBudget.changeOrderContractAmount))]
		public virtual decimal? ChangeOrderContractAmount
		{
			get;
			set;
		}
		#endregion
		#region ChangeOrderCostAmount
		public abstract class changeOrderCostAmount : PX.Data.BQL.BqlDecimal.Field<changeOrderCostAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipCostProjectionBudget.changeOrderCostAmount))]
		public virtual decimal? ChangeOrderCostAmount
		{
			get;
			set;
		}
		#endregion
		#region CostToComplete
		public abstract class costToComplete : PX.Data.BQL.BqlDecimal.Field<costToComplete> { }
		[PXDBDecimal(1, BqlField = typeof(PMWipCostProjectionBudget.costToComplete))]
		public virtual decimal? CostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CostProjectionCostAtCompletion
		public abstract class costProjectionCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<costProjectionCostAtCompletion> { }
		[PXDBDecimal(1, BqlField = typeof(PMWipCostProjectionBudget.costProjectionCostAtCompletion))]
		public virtual decimal? CostProjectionCostAtCompletion
		{
			get;
			set;
		}
		#endregion
		#region CostProjectionCostToComplete
		public abstract class costProjectionCostToComplete : PX.Data.BQL.BqlDecimal.Field<costProjectionCostToComplete> { }
		[PXDBDecimal(1, BqlField = typeof(PMWipCostProjectionBudget.costProjectionCostToComplete))]
		public virtual decimal? CostProjectionCostToComplete
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedOrigAmount
		public abstract class curyCommittedOrigAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedOrigAmount> { }

		[PXDBDecimal(1, BqlField = typeof(PMWipCostProjectionBudget.curyCommittedOrigAmount))]
		public virtual decimal? CuryCommittedOrigAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedCOAmount
		public abstract class curyCommittedCOAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedCOAmount> { }

		[PXDBDecimal(1, BqlField = typeof(PMWipCostProjectionBudget.curyCommittedCOAmount))]
		public virtual decimal? CuryCommittedCOAmount
		{
			get;
			set;
		}
		#endregion
		#region ProjectedCostAtCompletion
		public abstract class projectedCostAtCompletion : PX.Data.BQL.BqlDecimal.Field<projectedCostAtCompletion> { }
		[PXDBDecimal(1, BqlField = typeof(PMWipCostProjectionBudget.projectedCostAtCompletion))]
		public virtual decimal? ProjectedCostAtCompletion
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(PM.Messages.PMWipForecastHistory)]
	[Serializable]
	[PXProjection(typeof(Select2<PMForecastHistory,
		InnerJoin<PMAccountGroup, On<PMForecastHistory.accountGroupID, Equal<PMAccountGroup.groupID>>>>))]
	public class PMWipForecastHistory : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistory.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistory.projectTaskID))]
		public virtual int? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistory.accountGroupID))]
		public virtual int? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistory.inventoryID))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMForecastHistory.costCodeID))]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
		[PXDBString(IsKey = true, BqlField = typeof(PMForecastHistory.periodID))]
		public virtual string PeriodID
		{
			get;
			set;
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(1, BqlField = typeof(PMAccountGroup.type))]
		public virtual string Type
		{
			get;
			set;
		}
		#endregion

		#region IsExpense
		public abstract class isExpense : PX.Data.BQL.BqlBool.Field<isExpense> { }
		[PXDBBool(BqlField = typeof(PMAccountGroup.isExpense))]
		public virtual bool? IsExpense
		{
			get;
			set;
		}
		#endregion

		#region ActualCostAmount
		public abstract class actualCostAmount : PX.Data.BQL.BqlDecimal.Field<actualCostAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>, Or<isExpense, Equal<boolTrue>>>, PMForecastHistory.curyActualAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? ActualCostAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualRevenueAmount
		public abstract class actualRevenueAmount : PX.Data.BQL.BqlDecimal.Field<actualRevenueAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.income>>, PMForecastHistory.curyActualAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? ActualRevenueAmount
		{
			get;
			set;
		}
		#endregion
		#region ArRevenueAmount
		public abstract class arRevenueAmount : PX.Data.BQL.BqlDecimal.Field<arRevenueAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.income>>, PMForecastHistory.curyArAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? ArRevenueAmount
		{
			get;
			set;
		}
		#endregion
		#region ArRevenueInclTaxAmount
		public abstract class arRevenueInclTaxAmount : PX.Data.BQL.BqlDecimal.Field<arRevenueInclTaxAmount> { }

		/// <summary>
		/// The inclusive tax amount.
		/// </summary>
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.income>>, PMForecastHistory.curyInclTaxAmount>, decimal0>), typeof(decimal))]
		public virtual decimal? ArRevenueInclTaxAmount
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(PM.Messages.PMWipTotalForecastHistory)]
	[Serializable]
	[PXProjection(typeof(Select4<PMWipForecastHistory,
						Aggregate<GroupBy<PMWipForecastHistory.projectID,
						GroupBy<PMWipForecastHistory.periodID,
						Sum<PMWipForecastHistory.actualCostAmount,
						Sum<PMWipForecastHistory.actualRevenueAmount,
						Sum<PMWipForecastHistory.arRevenueAmount,
						Sum<PMWipForecastHistory.arRevenueInclTaxAmount>>>>>>>>))]
	public class PMWipTotalForecastHistory : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMWipForecastHistory.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
		[PXDBString(IsKey = true, BqlField = typeof(PMWipForecastHistory.periodID))]
		public virtual string PeriodID
		{
			get;
			set;
		}
		#endregion

		#region ActualCostAmount
		public abstract class actualCostAmount : PX.Data.BQL.BqlDecimal.Field<actualCostAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipForecastHistory.actualCostAmount))]
		public virtual decimal? ActualCostAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualRevenueAmount
		public abstract class actualRevenueAmount : PX.Data.BQL.BqlDecimal.Field<actualRevenueAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipForecastHistory.actualRevenueAmount))]
		public virtual decimal? ActualRevenueAmount
		{
			get;
			set;
		}
		#endregion
		#region ArRevenueAmount
		public abstract class arRevenueAmount : PX.Data.BQL.BqlDecimal.Field<arRevenueAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipForecastHistory.arRevenueAmount))]
		public virtual decimal? ArRevenueAmount
		{
			get;
			set;
		}
		#endregion
		#region ArRevenueInclTaxAmount
		public abstract class arRevenueInclTaxAmount : PX.Data.BQL.BqlDecimal.Field<arRevenueInclTaxAmount> { }

		/// <summary>
		/// The inclusive tax amount.
		/// </summary>
		[PXDBDecimal(BqlField = typeof(PMWipForecastHistory.arRevenueInclTaxAmount))]
		public virtual decimal? ArRevenueInclTaxAmount
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(PM.Messages.PMWipDetailTotalForecastHistory)]
	[Serializable]
	[PXProjection(typeof(Select4<PMWipForecastHistory,
						Aggregate<GroupBy<PMWipForecastHistory.projectID,
						GroupBy<PMWipForecastHistory.projectTaskID,
						GroupBy<PMWipForecastHistory.costCodeID,
						GroupBy<PMWipForecastHistory.accountGroupID,
						GroupBy<PMWipForecastHistory.periodID,
						Sum<PMWipForecastHistory.actualCostAmount,
						Sum<PMWipForecastHistory.actualRevenueAmount,
						Sum<PMWipForecastHistory.arRevenueAmount,
						Sum<PMWipForecastHistory.arRevenueInclTaxAmount>>>>>>>>>>>))]
	public class PMWipDetailTotalForecastHistory : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMWipForecastHistory.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMWipForecastHistory.projectTaskID))]
		public virtual int? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMWipForecastHistory.costCodeID))]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
		[PXDBString(IsKey = true, BqlField = typeof(PMWipForecastHistory.periodID))]
		public virtual string PeriodID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMWipForecastHistory.accountGroupID))]
		public virtual int? AccountGroupID
		{
			get;
			set;
		}
		#endregion

		#region ActualCostAmount
		public abstract class actualCostAmount : PX.Data.BQL.BqlDecimal.Field<actualCostAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipForecastHistory.actualCostAmount))]
		public virtual decimal? ActualCostAmount
		{
			get;
			set;
		}
		#endregion
		#region ActualRevenueAmount
		public abstract class actualRevenueAmount : PX.Data.BQL.BqlDecimal.Field<actualRevenueAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipForecastHistory.actualRevenueAmount))]
		public virtual decimal? ActualRevenueAmount
		{
			get;
			set;
		}
		#endregion
		#region ArRevenueAmount
		public abstract class arRevenueAmount : PX.Data.BQL.BqlDecimal.Field<arRevenueAmount> { }
		[PXDBDecimal(BqlField = typeof(PMWipForecastHistory.arRevenueAmount))]
		public virtual decimal? ArRevenueAmount
		{
			get;
			set;
		}
		#endregion
		#region ArRevenueInclTaxAmount
		public abstract class arRevenueInclTaxAmount : PX.Data.BQL.BqlDecimal.Field<arRevenueInclTaxAmount> { }

		/// <summary>
		/// The inclusive tax amount.
		/// </summary>
		[PXDBDecimal(BqlField = typeof(PMWipForecastHistory.arRevenueInclTaxAmount))]
		public virtual decimal? ArRevenueInclTaxAmount
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(PM.Messages.PMWipChangeOrder)]
	[Serializable]
	[PXProjection(typeof(Select<PMChangeOrder>))]
	public class PMWipChangeOrder : IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(PMChangeOrder.refNbr.Length, IsKey = true, BqlField = typeof(PMChangeOrder.refNbr))]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(BqlField = typeof(PMChangeOrder.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion

		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
		[PXDBBool(BqlField = typeof(PMChangeOrder.approved))]
		public virtual bool? Approved
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXDBBool(BqlField = typeof(PMChangeOrder.released))]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion
		#region CompletionDate
		public abstract class completionDate : PX.Data.BQL.BqlDateTime.Field<completionDate> { }
		[PXDBDate(BqlField = typeof(PMChangeOrder.completionDate))]
		public virtual DateTime? CompletionDate
		{
			get;
			set;
		}
		#endregion

		#region CostTotal
		public abstract class costTotal : PX.Data.BQL.BqlDecimal.Field<costTotal> { }
		[PXDBDecimal(BqlField = typeof(PMChangeOrder.costTotal))]
		public virtual decimal? CostTotal
		{
			get;
			set;
		}
		#endregion
		#region RevenueTotal
		public abstract class revenueTotal : PX.Data.BQL.BqlDecimal.Field<revenueTotal> { }
		[PXDBDecimal(BqlField = typeof(PMChangeOrder.revenueTotal))]
		public virtual decimal? RevenueTotal
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(BqlField = typeof(PMChangeOrder.status))]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(PM.Messages.PMWipChangeOrderBudget)]
	[Serializable]
	[PXProjection(typeof(Select2<PMChangeOrder,
		InnerJoin<PMChangeOrderBudget, On<PMChangeOrder.refNbr, Equal<PMChangeOrderBudget.refNbr>>>>))]
	public class PMWipChangeOrderBudget : IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(IsKey = true, BqlField = typeof(PMChangeOrderBudget.refNbr))]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMChangeOrderBudget.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMChangeOrderBudget.projectTaskID))]
		public virtual int? ProjectTaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMChangeOrderBudget.costCodeID))]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMChangeOrderBudget.accountGroupID))]
		public virtual int? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(PMChangeOrderBudget.inventoryID))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(1, BqlField = typeof(PMChangeOrderBudget.type))]
		public virtual string Type
		{
			get;
			set;
		}
		#endregion

		#region CompletionDate
		public abstract class completionDate : PX.Data.BQL.BqlDateTime.Field<completionDate> { }
		[PXDBDate(BqlField = typeof(PMChangeOrder.completionDate))]
		public virtual DateTime? CompletionDate
		{
			get;
			set;
		}
		#endregion

		#region ContractAmount
		public abstract class contractAmount : PX.Data.BQL.BqlDecimal.Field<contractAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.income>>, PMChangeOrderBudget.amount>, decimal0>), typeof(decimal))]
		public virtual decimal? ContractAmount
		{
			get;
			set;
		}
		#endregion
		#region CostAmount
		public abstract class costAmount : PX.Data.BQL.BqlDecimal.Field<costAmount> { }
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<type, Equal<AccountType.expense>>, PMChangeOrderBudget.amount>, decimal0>), typeof(decimal))]
		public virtual decimal? CostAmount
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(1, BqlField = typeof(PMChangeOrder.status))]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion
	}
}
