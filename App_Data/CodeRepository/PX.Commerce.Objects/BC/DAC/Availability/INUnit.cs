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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects.Availability
{
	/// <summary>
	/// Represents a unit of measurement conversion for an Inventory Item.
	/// </summary>
	[PXHidden]
	public class InventoryItemINUnit : PX.Objects.IN.INUnit
	{
		/// <summary>
		/// The ID of the inventory item this unit conversion belongs to.
		/// </summary>
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		/// <summary>
		/// The unit this unit conversion converts from.
		/// </summary>
		public new abstract class fromUnit : PX.Data.BQL.BqlString.Field<fromUnit> { }
		/// <summary>
		/// The unit this unit conversion converts to.
		/// </summary>
		public new abstract class toUnit : PX.Data.BQL.BqlString.Field<toUnit> { }
		/// <summary>
		/// The conversion rate for this unit conversion
		/// </summary>
		public new abstract class unitRate : PX.Data.BQL.BqlDecimal.Field<unitRate> { }
		/// <summary>
		/// Indicates the operation for this conversion, either multiplication or division.
		/// </summary>
		public new abstract class unitMultDiv : PX.Data.BQL.BqlString.Field<unitMultDiv> { }
	}

	/// <summary>
	/// Represents a unit of measurement conversion for an Inventory Item Class.
	/// </summary>
	[PXHidden]
	public class ItemClassINUnit : PX.Objects.IN.INUnit
	{
		/// <summary>
		/// The ID of the inventory class this unit conversion belongs to.
		/// </summary>
		public new abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		/// <summary>
		/// The unit this unit conversion converts from.
		/// </summary>
		public new abstract class fromUnit : PX.Data.BQL.BqlString.Field<fromUnit> { }
		/// <summary>
		/// The unit this unit conversion converts to.
		/// </summary>
		public new abstract class toUnit : PX.Data.BQL.BqlString.Field<toUnit> { }
		/// <summary>
		/// The conversion rate for this unit conversion
		/// </summary>
		public new abstract class unitRate : PX.Data.BQL.BqlDecimal.Field<unitRate> { }
		/// <summary>
		/// Indicates the operation for this conversion, either multiplication or division.
		/// </summary>
		public new abstract class unitMultDiv : PX.Data.BQL.BqlString.Field<unitMultDiv> { }
	}

	/// <summary>
	/// Represents a global unit of measurement conversion for an Inventory Items in the instance.
	/// </summary>
	[PXHidden]
	public class GlobalINUnit : PX.Objects.IN.INUnit
	{
		/// <summary>
		/// The global unit type this unit conversion belongs to.
		/// </summary>
		public sealed class globalUnitType : PX.Data.BQL.BqlShort.Constant<globalUnitType> { public globalUnitType() : base(3) { } }
		/// <summary>
		/// Indicates if this unit conversion is global, item class, or inventory item specific.
		/// </summary>
		public new abstract class unitType : PX.Data.BQL.BqlShort.Field<unitType> { }
		/// <summary>
		/// The unit this unit conversion converts from.
		/// </summary>
		public new abstract class fromUnit : PX.Data.BQL.BqlString.Field<fromUnit> { }
		/// <summary>
		/// The unit this unit conversion converts to.
		/// </summary>
		public new abstract class toUnit : PX.Data.BQL.BqlString.Field<toUnit> { }
		/// <summary>
		/// The conversion rate for this unit conversion
		/// </summary>
		public new abstract class unitRate : PX.Data.BQL.BqlDecimal.Field<unitRate> { }
		/// <summary>
		/// Indicates the operation for this conversion, either multiplication or division.
		/// </summary>
		public new abstract class unitMultDiv : PX.Data.BQL.BqlString.Field<unitMultDiv> { }
	}
}
