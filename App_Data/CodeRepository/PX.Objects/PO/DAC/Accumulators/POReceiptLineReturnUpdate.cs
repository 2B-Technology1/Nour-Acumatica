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

namespace PX.Objects.PO
{
	[Serializable]
	[PXHidden]
	[POReceiptLineReturnUpdate.Accumulator(BqlTable = typeof(POReceiptLine))]
	public partial class POReceiptLineReturnUpdate : IBqlTable
	{
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		[PXDBString(2, IsFixed = true, IsKey = true)]
		public virtual string ReceiptType
		{
			get;
			set;
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		public virtual string ReceiptNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region BaseReturnedQty
		public abstract class baseReturnedQty : PX.Data.BQL.BqlDecimal.Field<baseReturnedQty> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseReturnedQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOrigQty
		public abstract class baseOrigQty : PX.Data.BQL.BqlDecimal.Field<baseOrigQty> { }
		[PXDecimal(6)]
		public virtual decimal? BaseOrigQty
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute()
			{
				this.SingleRecord = true;
			}

			protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(sender, row, columns))
				{
					return false;
				}

				var returnRow = (POReceiptLineReturnUpdate)row;

				columns.Update<POReceiptLineReturnUpdate.baseReturnedQty>(returnRow.BaseReturnedQty, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<POReceiptLineReturnUpdate.lastModifiedByID>(returnRow.LastModifiedByID, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<POReceiptLineReturnUpdate.lastModifiedDateTime>(returnRow.LastModifiedDateTime, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<POReceiptLineReturnUpdate.lastModifiedByScreenID>(returnRow.LastModifiedByScreenID, PXDataFieldAssign.AssignBehavior.Replace);

				if (returnRow.BaseOrigQty != null)
				{
					columns.AppendException(Messages.ReturnedQtyMoreThanReceivedQty,
						new PXAccumulatorRestriction<POReceiptLineReturnUpdate.baseReturnedQty>(PXComp.LE, returnRow.BaseOrigQty));
				}

				return true;
			}
		}
	}
}
