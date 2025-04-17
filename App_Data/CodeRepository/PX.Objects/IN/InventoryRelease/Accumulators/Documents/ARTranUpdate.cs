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
using PX.Data.BQL;
using PX.Objects.CM;

namespace PX.Objects.IN.InventoryRelease.Accumulators.Documents
{
	using static PXDataFieldAssign.AssignBehavior;

	[PXHidden]
	[Accumulator(BqlTable = typeof(AR.ARTran))]
	public class ARTranUpdate : IBqlTable
	{
		#region TranType
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault]
		public virtual string TranType
		{
			get;
			set;
		}
		public abstract class tranType : BqlString.Field<tranType> { }
		#endregion
		#region RefNbr
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public virtual string RefNbr
		{
			get;
			set;
		}
		public abstract class refNbr : BqlString.Field<refNbr> { }
		#endregion
		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		#endregion
		#region TranCost
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranCost
		{
			get;
			set;
		}
		public abstract class tranCost : BqlDecimal.Field<tranCost> { }
		#endregion
		#region IsTranCostFinal
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsTranCostFinal
		{
			get;
			set;
		}
		public abstract class isTranCostFinal : BqlBool.Field<isTranCostFinal> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#region tstamp
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute()
			{
				SingleRecord = true;
			}

			protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(cache, row, columns))
					return false;

				columns.UpdateOnly = true;
				columns.Update<tranCost>(Summarize);
				columns.Update<isTranCostFinal>(Replace);

				return true;
			}
		}
	}
}
