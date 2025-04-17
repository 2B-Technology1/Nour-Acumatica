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
using PX.Objects.AM.Attributes;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM.CacheExtensions
{
	/// <summary>
	/// Manufacturing extension of the Item Warehouse Detail
	/// </summary>
	[Serializable]
    public sealed class INItemClassExt : PXCacheExtension<INItemClass>
    {
        public static bool IsActive()
        {
			return Features.ManufacturingOrDRPOrReplenishmentEnabled();
		}

		#region AMDaysSupply
		public abstract class aMDaysSupply : PX.Data.BQL.BqlInt.Field<aMDaysSupply> { }

		/// <summary>
		/// Days of Supply
		/// </summary>
		[PXDBInt(MinValue=0)]
		[PXUIField(DisplayName="Days of Supply", FieldClass = Features.MRPDRP)]
		[PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
		public int? AMDaysSupply { get; set; }
		#endregion

		#region AMSourceSiteID
		public abstract class aMSourceSiteID : PX.Data.BQL.BqlInt.Field<aMSourceSiteID> { }

		/// <summary>
		/// Source Warehouse
		/// </summary>
		[AMSite(DisplayName = "Source Warehouse", FieldClass = nameof(FeaturesSet.Warehouse))]
		[PXUIEnabled(typeof(Where<INItemClass.replenishmentSource, Equal<INReplenishmentSource.transfer>,
			Or2<Where<INItemClass.planningMethod, Equal<INPlanningMethod.mRP>, Or<INItemClass.planningMethod, Equal<INPlanningMethod.dRP>>>,
				And<Where<INItemClass.replenishmentSource , Equal<INReplenishmentSource.purchased>,Or<INItemClass.replenishmentSource, Equal<INReplenishmentSource.purchaseToOrder>>>>>>))]
		public int? AMSourceSiteID { get; set; }
		#endregion
		#region AMSafetyStock
		public abstract class aMSafetyStock : PX.Data.BQL.BqlDecimal.Field<aMSafetyStock> { }

		/// <summary>
		/// Safety Stock
		/// </summary>
        [PXDBQuantity(MinValue = 0)]
		[PXUIField(DisplayName = "Safety Stock", FieldClass = Features.MRPDRP)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? AMSafetyStock { get; set; }
		#endregion
		#region AMMinQty
		public abstract class aMMinQty : PX.Data.BQL.BqlDecimal.Field<aMMinQty> { }

		/// <summary>
		/// Reorder Point
		/// </summary>
        [PXDBQuantity(MinValue = 0)]
		[PXUIField(DisplayName = "Reorder Point", FieldClass = Features.MRPDRP)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? AMMinQty { get; set; }
		#endregion
		#region AMMinOrdQty
		public abstract class aMMinOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMinOrdQty> { }

		/// <summary>
		/// Min Order Qty.
		/// </summary>
        [PXDBQuantity(MinValue = 0)]
		[PXUIField(DisplayName = "Min Order Qty.")]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? AMMinOrdQty { get; set; }
		#endregion
		#region AMMaxOrdQty
		public abstract class aMMaxOrdQty : PX.Data.BQL.BqlDecimal.Field<aMMaxOrdQty> { }

		/// <summary>
		/// Max Order Qty.
		/// </summary>
        [PXDBQuantity(MinValue = 0)]
		[PXUIField(DisplayName = "Max Order Qty.")]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? AMMaxOrdQty { get; set; }
		#endregion
		#region AMLotSize
		public abstract class aMLotSize : PX.Data.BQL.BqlDecimal.Field<aMLotSize> { }

		/// <summary>
		/// Lot Size
		/// </summary>
        [PXDBQuantity(MinValue = 0)]
		[PXUIField(DisplayName = "Lot Size")]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? AMLotSize { get; set; }
		#endregion
		#region AMMFGLeadTime
		public abstract class aMMFGLeadTime : PX.Data.BQL.BqlInt.Field<aMMFGLeadTime> { }

		/// <summary>
		/// Manufacturing Lead Time
		/// </summary>
		[PXUIField(DisplayName = "Manufacturing Lead Time", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
		[PXDBInt]
		[PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
		public int? AMMFGLeadTime { get; set; }
		#endregion

	}
}
