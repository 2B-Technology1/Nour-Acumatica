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
	[Accumulator(BqlTable = typeof(SO.SOShipLine))]
	public class SOShipLineUpdate : IBqlTable
	{
		#region ShipmentNbr
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		public virtual string ShipmentNbr
		{
			get;
			set;
		}
		public abstract class shipmentNbr : BqlString.Field<shipmentNbr> { }
		#endregion
		#region ShipmentType
		[PXDBString(1, IsFixed = true, IsKey = true)]
		public virtual string ShipmentType
		{
			get;
			set;
		}
		public abstract class shipmentType : BqlString.Field<shipmentType> { }
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
		#region UnitCost
		protected decimal? _UnitCost;
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? UnitCost
		{
			get;
			set;
		}
		public abstract class unitCost : BqlDecimal.Field<unitCost> { }
		#endregion
		#region ExtCost
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ExtCost
		{
			get;
			set;
		}
		public abstract class extCost : BqlDecimal.Field<extCost> { }
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
				columns.Update<unitCost>(Replace);
				columns.Update<extCost>(Replace);

				return true;
			}

			public override bool PersistInserted(PXCache cache, object row)
			{
				try
				{
					return base.PersistInserted(cache, row);
				}
				catch (PXLockViolationException)
				{
					return false;
				}
			}
		}
	}
}
