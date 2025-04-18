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
using PX.Objects.CM.Extensions;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// Represents the aggregated history of the project budget.
	/// Contains the project's actual amounts and actual quantities aggregated by financial periods.
	/// </summary>
	[System.SerializableAttribute]
	[PXCacheName(Messages.PMHistory)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMHistory : PX.Data.IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
        [PXUIField(DisplayName = "Project ID")]
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
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		protected Int32? _ProjectTaskID;

		/// <summary>
		/// The identifier of the <see cref="PMTask">project task</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
        [PXUIField(DisplayName = "Project Task ID")]
        public virtual Int32? ProjectTaskID
		{
			get
			{
				return this._ProjectTaskID;
			}
			set
			{
				this._ProjectTaskID = value;
			}
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;

		/// <summary>
		/// The identifier of the <see cref="PMAccountGroup">project account group</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.GroupID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
        [PXUIField(DisplayName = "Account Group ID")]
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
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;

		/// <summary>
		/// The identifier of the <see cref="InventoryItem">inventory item</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
        [PXUIField(DisplayName = "Inventory ID")]
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
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;

		/// <summary>
		/// The identifier of the <see cref="PMCostCode">project cost code</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostCode.CostCodeID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Cost Code")]
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
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
		protected String _PeriodID;

		/// <summary>
		/// The financial period of the master calendar.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="GL.FinPeriods.TableDefinition.FinPeriod.FinPeriodID"/> field.
		/// </value>
		[GL.FinPeriodID(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Financial Period", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(MasterFinPeriod.finPeriodID), DescriptionField = typeof(MasterFinPeriod.descr))]
		public virtual String PeriodID
		{
			get
			{
				return this._PeriodID;
			}
			set
			{
				this._PeriodID = value;
			}
		}
		#endregion
		#region BranchID
		protected Int32? _BranchID;

		/// <summary>
		/// The identifier of the <see cref="Branch">branch</see> associated with the history record.
		/// This field is a part of the compound key of the record.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="Branch.BAccountID"/> field.
		/// </value>
		[Branch(IsKey = true)]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion


		#region FinPTDQty
		public abstract class finPTDQty : PX.Data.BQL.BqlDecimal.Field<finPTDQty> { }
		protected Decimal? _FinPTDQty;

		/// <summary>
		/// The financial PTD quantity calculated based on <see cref="PMTran.Qty"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = Messages.FinPTDQuantity)]
        public virtual Decimal? FinPTDQty
		{
			get
			{
				return this._FinPTDQty;
			}
			set
			{
				this._FinPTDQty = value;
			}
		}
		#endregion
		#region TranPTDQty
		public abstract class tranPTDQty : PX.Data.BQL.BqlDecimal.Field<tranPTDQty> { }
		protected Decimal? _TranPTDQty;

		/// <summary>
		/// The transaction PTD quantity calculated based on <see cref="PMTran.Qty"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPTDQty
		{
			get
			{
				return this._TranPTDQty;
			}
			set
			{
				this._TranPTDQty = value;
			}
		}
		#endregion
		#region FinPTDCuryAmount
		public abstract class finPTDCuryAmount : PX.Data.BQL.BqlDecimal.Field<finPTDCuryAmount>
		{
		}

		/// <summary>
		/// The financial PTD amount calculated based on <see cref="PMTran.ProjectCuryAmount"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = Messages.FinPTDAmount)]
		public virtual Decimal? FinPTDCuryAmount
		{
			get;
			set;
		}
		#endregion
		#region FinPTDAmount
		public abstract class finPTDAmount : PX.Data.BQL.BqlDecimal.Field<finPTDAmount> { }

		/// <summary>
		/// The financial PTD amount calculated based on <see cref="PMTran.Amount"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.FinPTDAmount)]
		public virtual Decimal? FinPTDAmount
		{
			get;
			set;
		}
		#endregion
		#region TranPTDCuryAmount
		public abstract class tranPTDCuryAmount : PX.Data.BQL.BqlDecimal.Field<tranPTDCuryAmount>
		{
		}

		/// <summary>
		/// The transaction PTD amount calculated based on <see cref="PMTran.ProjectCuryAmount"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPTDCuryAmount
		{
			get;
			set;
		}
		#endregion
		#region TranPTDAmount
		public abstract class tranPTDAmount : PX.Data.BQL.BqlDecimal.Field<tranPTDAmount> { }

		/// <summary>
		/// The transaction PTD amount calculated based on <see cref="PMTran.Amount"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPTDAmount
		{
			get;
			set;
		}
		#endregion
		#region FinYTDQty
		public abstract class finYTDQty : PX.Data.BQL.BqlDecimal.Field<finYTDQty> { }
		protected Decimal? _FinYTDQty;

		/// <summary>
		/// The financial YTD quantity calculated based on <see cref="PMTran.Qty"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYTDQty
		{
			get
			{
				return this._FinYTDQty;
			}
			set
			{
				this._FinYTDQty = value;
			}
		}
		#endregion
		#region TranYTDQty
		public abstract class tranYTDQty : PX.Data.BQL.BqlDecimal.Field<tranYTDQty> { }
		protected Decimal? _TranYTDQty;

		/// <summary>
		/// The transaction YTD quantity calculated based on <see cref="PMTran.Qty"/>.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYTDQty
		{
			get
			{
				return this._TranYTDQty;
			}
			set
			{
				this._TranYTDQty = value;
			}
		}
		#endregion
		#region FinYTDCuryAmount
		public abstract class finYTDCuryAmount : PX.Data.BQL.BqlDecimal.Field<finYTDCuryAmount>
		{
		}

		/// <summary>
		/// The financial YTD amount calculated based on <see cref="PMTran.ProjectCuryAmount"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYTDCuryAmount
		{
			get;
			set;
		}
		#endregion
		#region FinYTDAmount
		public abstract class finYTDAmount : PX.Data.BQL.BqlDecimal.Field<finYTDAmount> { }

		/// <summary>
		/// The financial YTD amount calculated based on <see cref="PMTran.Amount"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYTDAmount
		{
			get;
			set;
		}
		#endregion
		#region TranYTDCuryAmount
		public abstract class tranYTDCuryAmount : PX.Data.BQL.BqlDecimal.Field<tranYTDCuryAmount>
		{
		}

		/// <summary>
		/// The transaction YTD amount calculated based on <see cref="PMTran.ProjectCuryAmount"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYTDCuryAmount
		{
			get;
			set;
		}
		#endregion
		#region TranYTDAmount
		public abstract class tranYTDAmount : PX.Data.BQL.BqlDecimal.Field<tranYTDAmount> { }


		/// <summary>
		/// The transaction YTD amount calculated based on <see cref="PMTran.Amount"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYTDAmount
		{
			get;
			set;
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
	}

	[PXHidden]
	[PMProjectAccum]
    [Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMHistoryAccum : PMHistory
	{
		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		#endregion
		#region ProjectTaskID
		public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		#endregion
		#region AccountGroupID
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region CostCodeID
		public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

		/// <inheritdoc cref="PMHistory.CostCodeID"/>
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
		#region PeriodID
		public new abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }

		/// <inheritdoc cref="PMHistory.PeriodID"/>
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String PeriodID
		{
			get
			{
				return this._PeriodID;
			}
			set
			{
				this._PeriodID = value;
			}
		}
		#endregion
	}
		
	public class PMProjectAccumAttribute : PXAccumulatorAttribute
	{
		public PMProjectAccumAttribute()
			: base(new Type[] {
					typeof(PMHistory.finYTDQty),
					typeof(PMHistory.tranYTDQty),
					typeof(PMHistory.finYTDCuryAmount),
					typeof(PMHistory.finYTDAmount),
					typeof(PMHistory.tranYTDCuryAmount),
					typeof(PMHistory.tranYTDAmount)
					},
					new Type[] {
					typeof(PMHistory.finYTDQty),
					typeof(PMHistory.tranYTDQty),
					typeof(PMHistory.finYTDCuryAmount),
					typeof(PMHistory.finYTDAmount),
					typeof(PMHistory.tranYTDCuryAmount),
					typeof(PMHistory.tranYTDAmount)
					}
			)
		{
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			PMHistory hist = (PMHistory)row;

			columns.RestrictPast<PMHistory.periodID>(PXComp.GE, hist.PeriodID.Substring(0, 4) + "01");
			columns.RestrictFuture<PMHistory.periodID>(PXComp.LE, hist.PeriodID.Substring(0, 4) + "99");

			return true;
		}
	}
}
