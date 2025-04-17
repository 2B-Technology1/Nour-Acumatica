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
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM.Extensions;
using PX.Objects.CN.ProjectAccounting;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.TM;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// Contains the main properties of a cost projection revision.
	/// Cost projections are used for tracking the project costs during project completion
	/// in comparison to the initially estimated costs.
	/// The records of this type are created and edited through the Cost Projection (PM305000) form
	/// (which corresponds to the <see cref="CostProjectionEntry"/> graph).
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.CostProjection)]
	[PXPrimaryGraph(typeof(CostProjectionEntry))]
	[Serializable]
	public class PMCostProjection : PX.Data.IBqlTable, IAssign
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PMCostProjection>.By<projectID, revisionID>
		{
			public static PMCostProjection Find(PXGraph graph, int? projectID, string revisionID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, projectID, revisionID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Class
			/// </summary>
			public class CostProjectionClass : PMCostProjectionClass.PK.ForeignKeyOf<PMCostProjection>.By<classID> { }

			/// <summary>
			/// Owner
			/// </summary>
			public class OwnerContact : PX.Objects.CR.Contact.PK.ForeignKeyOf<PMCostProjection>.By<ownerID> { }

			/// <summary>
			/// Project
			/// </summary>
			/// <exclude />
			public class Project : PMProject.PK.ForeignKeyOf<PMRevenueBudget>.By<projectID> { }
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// The <see cref="PMProject">project</see> for which the cost projection revision is created.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[Project(typeof(Where<PMProject.baseType, Equal<PX.Objects.CT.CTPRType.project>, And<PMProject.nonProject, Equal<False>>>), IsKey = true, WarnIfCompleted = false)]
		[PXDefault()]
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
		[PXSelector(typeof(Search<PMCostProjection.revisionID, Where<PMCostProjection.projectID, Equal<Current<projectID>>>, OrderBy<Desc<PMCostProjection.revisionID>>>), DescriptionField = typeof(description))]
		[PXDBString(30, IsKey = true, InputMask = ">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string RevisionID
		{
			get;
			set;
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		/// <summary>
		/// The <see cref="PMCostProjectionClass">cost projection class</see> of the cost projection revision.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostProjectionClass.ClassID"/> field.
		/// </value>
		[PXForeignReference(typeof(Field<classID>.IsRelatedTo<PMCostProjectionClass.classID>))]
		[PXDBString(PMCostProjectionClass.classID.Length, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Cost Projection Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PMCostProjectionClass.classID>), DescriptionField = typeof(PMCostProjectionClass.description))]
		[PXRestrictor(typeof(Where<PMCostProjectionClass.isActive, Equal<True>>), Messages.ClassIsInactive)]
		public virtual String ClassID
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// The status of the cost projection revision.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="CostProjectionStatus.ListAttribute"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[CostProjectionStatus.List()]
		[PXDefault(CostProjectionStatus.OnHold)]
		[PXUIField(DisplayName = "Status", Required = true, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Status
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the cost projection revision is on hold.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold")]
		[PXDefault(true)]
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the cost projection revision is approved.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Approved
		{
			get;
			set;
		}
		#endregion
		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the cost projection revision is rejected.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public bool? Rejected
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the cost projection revision is released.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Released")]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The description of the cost projection revision.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		/// <summary>
		/// The date when the cost projection revision was created.
		/// </summary>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Revision Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? Date
		{
			get;
			set;
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		/// <summary>
		/// The number of lines in the cost projection revision which is used to set the default value of the <see cref="PMCostProjectionLine.LineNbr" /> field.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedQuantity
		public abstract class totalBudgetedQuantity : PX.Data.BQL.BqlDecimal.Field<totalBudgetedQuantity> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.BudgetedQuantity"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Budgeted Quantity", Enabled = false)]
		public virtual Decimal? TotalBudgetedQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedAmount
		public abstract class totalBudgetedAmount : PX.Data.BQL.BqlDecimal.Field<totalBudgetedAmount> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.BudgetedAmount"/>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted Cost at Completion", Enabled = false)]
		public virtual Decimal? TotalBudgetedAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalActualQuantity
		public abstract class totalActualQuantity : PX.Data.BQL.BqlDecimal.Field<totalActualQuantity> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.ActualQuantity"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Actual Quantity", Enabled = false)]
		public virtual Decimal? TotalActualQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalActualAmount
		public abstract class totalActualAmount : PX.Data.BQL.BqlDecimal.Field<totalActualAmount> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.ActualAmount"/>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Actual Amount", Enabled = false)]
		public virtual Decimal? TotalActualAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalUnbilledQuantity
		public abstract class totalUnbilledQuantity : PX.Data.BQL.BqlDecimal.Field<totalUnbilledQuantity> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.UnbilledQuantity"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Unbilled Quantity", Enabled = false)]
		public virtual Decimal? TotalUnbilledQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalUnbilledAmount
		public abstract class totalUnbilledAmount : PX.Data.BQL.BqlDecimal.Field<totalUnbilledAmount> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.UnbilledAmount"/>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Unbilled Amount", Enabled = false)]
		public virtual Decimal? TotalUnbilledAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalQuantity
		public abstract class totalQuantity : PX.Data.BQL.BqlDecimal.Field<totalQuantity> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.Quantity"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Projected Quantity to Complete")]
		public virtual Decimal? TotalQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalAmount
		public abstract class totalAmount : PX.Data.BQL.BqlDecimal.Field<totalAmount> { }


		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.Amount"/>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost to Complete")]
		public virtual Decimal? TotalAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalProjectedQuantity
		public abstract class totalProjectedQuantity : PX.Data.BQL.BqlDecimal.Field<totalProjectedQuantity> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.ProjectedQuantity"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Projected Quantity at Completion")]
		public virtual Decimal? TotalProjectedQuantity
		{
			get;
			set;
		}
		#endregion
		#region TotalProjectedAmount
		public abstract class totalProjectedAmount : PX.Data.BQL.BqlDecimal.Field<totalProjectedAmount> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.ProjectedAmount"/>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Projected Cost at Completion")]
		public virtual Decimal? TotalProjectedAmount
		{
			get;
			set;
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		/// <summary>
		/// The workgroup that is responsible for the document.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="PX.TM.EPCompanyTree.WorkGroupID">EPCompanyTree.WorkGroupID</see> field.
		/// </value>
		[PXDBInt]
		[PXDefault(typeof(PMProject.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
		public virtual int? WorkgroupID
		{
			get;
			set;
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		/// <summary>
		/// The <see cref="PX.Objects.EP.EPEmployee">Employee</see> responsible for the document.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="PX.Objects.CR.BAccount.BAccountID"/> field.
		/// </value>
		[PXDefault(typeof(PMProject.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Owner(typeof(PMChangeRequest.workgroupID))]
		public virtual int? OwnerID
		{
			get;
			set;
		}
		#endregion
		#region FormCaptionDescription

		/// <summary>
		/// The description of the corresponding project.
		/// </summary>
		[PXString]
		[PXFormula(typeof(Selector<projectID, PMProject.description>))]
		public string FormCaptionDescription { get; set; }
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
		#region TotalQuantityToComplete
		public abstract class totalQuantityToComplete : PX.Data.BQL.BqlDecimal.Field<totalQuantityToComplete> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.QuantityToComplete"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity to Complete", Enabled = false)]
		public virtual Decimal? TotalQuantityToComplete
		{
			get;
			set;
		}
		#endregion
		#region TotalAmountToComplete
		public abstract class totalAmountToComplete : PX.Data.BQL.BqlDecimal.Field<totalAmountToComplete> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.AmountToComplete"/>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted Cost to Complete", Enabled = false)]
		public virtual Decimal? TotalAmountToComplete
		{
			get;
			set;
		}
		#endregion
		#region TotalVarianceAmount
		public abstract class totalVarianceAmount : PX.Data.BQL.BqlDecimal.Field<totalVarianceAmount> { }

		/// <summary>
		/// The sum of <see cref="PMCostProjectionLine.VarianceAmount"/>.
		/// </summary>
		[PXFormula(typeof(Sub<totalProjectedAmount, totalBudgetedAmount>))]
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cost Variance", Enabled = false)]
		public virtual Decimal? TotalVarianceAmount
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName(Messages.Project)]
	[Serializable]
	[PXProjection(typeof(Select2<PMProject,
		LeftJoin<PMProjectRevenueTotal, On<PMProjectRevenueTotal.projectID, Equal<PMProject.contractID>>,
		LeftJoin<PMProjectCostForecastTotal, On<PMProjectCostForecastTotal.projectID, Equal<PMProject.contractID>>>>>))]
	public class PMForecastProject : IBqlTable
	{
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

		/// <inheritdoc cref="PMProject.ContractID"/>
		[PXDBInt(IsKey = true, BqlField = typeof(PMProject.contractID))]
		public virtual Int32? ContractID
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedRevenueAmount
		public abstract class totalBudgetedRevenueAmount : PX.Data.BQL.BqlDecimal.Field<totalBudgetedRevenueAmount> { }

		/// <inheritdoc cref="PMProjectRevenueTotal.CuryRevisedAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMProjectRevenueTotal.curyRevisedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Budgeted Revenue", Enabled = false)]
		public virtual Decimal? TotalBudgetedRevenueAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedCostAmount
		public abstract class curyCommittedCostAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedCostAmount> { }

		/// <inheritdoc cref="PMProjectCostForecastTotal.CuryCommittedAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMProjectCostForecastTotal.curyCommittedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Cost")]
		public virtual Decimal? CuryCommittedCostAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryActualCostAmount
		public abstract class curyActualCostAmount : PX.Data.BQL.BqlDecimal.Field<curyActualCostAmount> { }

		/// <inheritdoc cref="PMProjectCostForecastTotal.CuryActualAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMProjectCostForecastTotal.curyActualAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Cost", Enabled = false)]
		public virtual Decimal? CuryActualCostAmount
		{
			get;
			set;
		}
		#endregion
		#region CuryCommittedInvoicedCostAmount
		public abstract class curyCommittedInvoicedCostAmount : PX.Data.BQL.BqlDecimal.Field<curyCommittedInvoicedCostAmount> { }

		/// <inheritdoc cref="PMProjectCostForecastTotal.CuryCommittedInvoicedAmount"/>
		[PXDBBaseCury(BqlField = typeof(PMProjectCostForecastTotal.curyCommittedInvoicedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Cost")]
		public virtual Decimal? CuryCommittedInvoicedCostAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedCostAmount
		public abstract class totalBudgetedCostAmount : PX.Data.BQL.BqlDecimal.Field<totalBudgetedCostAmount> { }

		[PXDBBaseCury(BqlField = typeof(PMProjectCostForecastTotal.curyRevisedAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Cost at Completion", Enabled = false)]
		public virtual Decimal? TotalBudgetedCostAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedCompletedAmount
		public abstract class totalBudgetedCompletedAmount : PX.Data.BQL.BqlDecimal.Field<totalBudgetedCompletedAmount> { }

		[PXFormula(typeof(Add<IsNull<curyActualCostAmount, decimal0>, Sub<IsNull<curyCommittedCostAmount, decimal0>, IsNull<curyCommittedInvoicedCostAmount, decimal0>>>))]
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Actual + Committed Costs", Enabled = false)]
		public virtual Decimal? TotalBudgetedCompletedAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedAmountToComplete
		public abstract class totalBudgetedAmountToComplete : PX.Data.BQL.BqlDecimal.Field<totalBudgetedAmountToComplete> { }
		[PXFormula(typeof(Sub<IsNull<totalBudgetedCostAmount, decimal0>, IsNull<totalBudgetedCompletedAmount, decimal0>>))]
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cost to Complete")]
		public virtual Decimal? TotalBudgetedAmountToComplete
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedGrossProfit
		public abstract class totalBudgetedGrossProfit : PX.Data.BQL.BqlDecimal.Field<totalBudgetedGrossProfit> { }
		[PXFormula(typeof(Sub<IsNull<totalBudgetedRevenueAmount, decimal0>, IsNull<totalBudgetedCostAmount, decimal0>>))]
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gross Profit")]
		public virtual Decimal? TotalBudgetedGrossProfit
		{
			get;
			set;
		}
		#endregion
		#region TotalBudgetedVarianceAmount
		public abstract class totalBudgetedVarianceAmount : PX.Data.BQL.BqlDecimal.Field<totalBudgetedVarianceAmount> { }
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cost Variance")]
		public virtual Decimal? TotalBudgetedVarianceAmount
		{
			get;
			set;
		}
		#endregion
		#region TotalProjectedGrossProfit
		public abstract class totalProjectedGrossProfit : PX.Data.BQL.BqlDecimal.Field<totalProjectedGrossProfit> { }
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Projected Gross Profit")]
		public virtual Decimal? TotalProjectedGrossProfit
		{
			get;
			set;
		}
		#endregion
	}
}
