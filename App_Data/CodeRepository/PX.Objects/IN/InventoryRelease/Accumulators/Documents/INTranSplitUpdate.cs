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

namespace PX.Objects.IN.InventoryRelease.Accumulators.Documents
{
	using static PXDataFieldAssign.AssignBehavior;

	[PXHidden]
	[Accumulator(BqlTable = typeof(INTranSplit))]
	public class INTranSplitUpdate : IBqlTable
	{
		#region TranType
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault]
		public virtual string DocType
		{
			get;
			set;
		}
		public abstract class docType : BqlString.Field<docType> { }
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
		#region CostSiteID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? CostSiteID
		{
			get;
			set;
		}
		public abstract class costSiteID : BqlInt.Field<costSiteID> { }
		#endregion
		#region CostSubItemID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? CostSubItemID
		{
			get;
			set;
		}
		public abstract class costSubItemID : BqlInt.Field<costSubItemID> { }
		#endregion
		#region TotalQty
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TotalQty
		{
			get;
			set;
		}
		public abstract class totalQty : BqlDecimal.Field<totalQty> { }
		#endregion
		#region TotalCost
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TotalCost
		{
			get;
			set;
		}
		public abstract class totalCost : BqlDecimal.Field<totalCost> { }
		#endregion
		#region PlanID
		[PXDBLong]
		public virtual long? PlanID
		{
			get;
			set;
		}
		public abstract class planID : BqlLong.Field<planID> { }
		#endregion

		#region ResetPlanID
		[PXBool]
		public virtual bool? ResetPlanID
		{
			get;
			set;
		}
		public abstract class resetPlanID : BqlBool.Field<resetPlanID> { }
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
				columns.Update<totalQty>(Summarize);
				columns.Update<totalCost>(Summarize);

				//only valid for transfers, so planid must be null
				if (((INTranSplitUpdate)row).ResetPlanID == true)
					columns.Update<planID>(Nullout);

				return true;
			}
		}
	}
}
