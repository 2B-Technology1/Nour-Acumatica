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
using PX.Objects.IN;
using PX.Objects.CM.Extensions;
using System;

namespace PX.Objects.PM
{
	[PXHidden]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMTaskAllocTotal : PX.Data.IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
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
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
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
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
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
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[PXDBInt(IsKey = true)]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion

		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		protected Decimal? _Amount;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? Amount
		{
			get
			{
				return this._Amount;
			}
			set
			{
				this._Amount = value;
			}
		}
		#endregion
		#region Quantity
		public abstract class quantity : PX.Data.BQL.BqlDecimal.Field<quantity> { }
		protected Decimal? _Quantity;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? Quantity
		{
			get
			{
				return this._Quantity;
			}
			set
			{
				this._Quantity = value;
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
	}

	[PXHidden]
	[Serializable]
	[TaskAllocTotalAccum]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMTaskAllocTotalAccum : PMTaskAllocTotal
	{
		#region ProjectID
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		#endregion
		#region TaskID
		public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		#endregion
		#region AccountGroupID
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region CostCodeID
		public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		#endregion
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class TaskAllocTotalAccumAttribute : PXAccumulatorAttribute
	{
		public TaskAllocTotalAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			PMTaskAllocTotal item = (PMTaskAllocTotal)row;

			columns.Update<PMTaskAllocTotal.amount>(item.Amount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMTaskAllocTotal.quantity>(item.Quantity, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}
}
