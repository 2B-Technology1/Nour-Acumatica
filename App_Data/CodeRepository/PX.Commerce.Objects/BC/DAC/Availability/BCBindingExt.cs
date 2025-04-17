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
using PX.Commerce.Core;

namespace PX.Commerce.Objects.Availability
{
	/// <summary>
	/// <inheritdoc cref="PX.Commerce.Objects.BCBindingExt"/>
	/// </summary>
	[PXHidden]
	public class BCBindingExt : IBqlTable
	{
		#region BindingID
		/// <summary>
		/// <inheritdoc cref="PX.Commerce.Objects.BCBindingExt"/>
		/// </summary>
		[PXDBIdentity]
		[PXUIField(DisplayName = "Store", Visible = false)]
		public int? BindingID { get; set; }
		/// <inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion
		#region Availability
		/// <summary>
		/// Specifies the store default for whether to export product availability for an inventory item.
		/// </summary>
		[PXDBString(1, IsUnicode = false)]
		[PXUIField(DisplayName = "Default Availability")]
		[BCItemAvailabilities.List]
		[PXDefault(BCItemAvailabilities.AvailableSkip)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductAvailability })]
		public virtual string Availability { get; set; }
		/// <inheritdoc cref="Availability"/>
		public abstract class availability : PX.Data.BQL.BqlString.Field<availability> { }
		#endregion
        #region WarehouseMode
        /// <summary>
        /// Specifies the store default for which warehouses to use when determining product availability.
        /// </summary>
        [PXDBString(1)]
        [PXUIField(DisplayName = "Warehouse Mode")]
        [PXDefault(BCWarehouseModeAttribute.AllWarehouse, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [BCWarehouseMode]
        public virtual string WarehouseMode { get; set; }
        /// <inheritdoc cref="WarehouseMode"/>
        public abstract class warehouseMode : PX.Data.BQL.BqlString.Field<warehouseMode> { }
        #endregion
	}
}
