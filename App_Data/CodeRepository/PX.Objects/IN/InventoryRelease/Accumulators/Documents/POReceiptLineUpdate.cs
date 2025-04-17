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
	[Accumulator(BqlTable = typeof(PO.POReceiptLine))]
	public class POReceiptLineUpdate : IBqlTable
	{
		#region ReceiptType
		[PXDBString(PO.POReceiptLine.receiptType.Length, IsFixed = true, IsKey = true)]
		public virtual string ReceiptType
		{
			get;
			set;
		}
		public abstract class receiptType : BqlString.Field<receiptType> { }
		#endregion
		#region ReceiptNbr
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		public virtual string ReceiptNbr
		{
			get;
			set;
		}
		public abstract class receiptNbr : BqlString.Field<receiptNbr> { }
		#endregion
		#region LineNbr
		[PXDBInt(IsKey = true)]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		#endregion

		#region INReleased
		[PXDBBool, PXDefault(false)]
		public virtual bool? INReleased
		{
			get;
			set;
		}
		public abstract class iNReleased : BqlBool.Field<iNReleased> { }
		#endregion
		#region UpdateTranCostFinal
		[PXBool, PXUnboundDefault(false)]
		public virtual bool? UpdateTranCostFinal
		{
			get;
			set;
		}
		public abstract class updateTranCostFinal : BqlBool.Field<updateTranCostFinal> { }
		#endregion
		#region TranCostFinal
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranCostFinal
		{
			get;
			set;
		}
		public abstract class tranCostFinal : BqlDecimal.Field<tranCostFinal> { }
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
				columns.Update<iNReleased>(Replace);
				columns.Update<tranCostFinal>(((POReceiptLineUpdate)row).UpdateTranCostFinal == true ? Replace : Initialize);
				columns.Update<lastModifiedByID>(Replace);
				columns.Update<lastModifiedDateTime>(Replace);
				columns.Update<lastModifiedByScreenID>(Replace);

				return true;
			}
		}
	}
}
