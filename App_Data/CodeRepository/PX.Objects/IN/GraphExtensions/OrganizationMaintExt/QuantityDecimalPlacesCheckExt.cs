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

using System.Linq;
using PX.Objects.CS;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN.GraphExtensions.OrganizationMaintExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class QuantityDecimalPlacesCheckExt : PXGraphExtension<OrganizationMaint>
	{
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		/// <summary>
		/// An extension for <see cref="CommonSetup"/> that is used to prevent
		/// <see cref="CommonSetup.decPlQty"/> change when inventory balances exist.
		/// </summary>
		public sealed class CommonSetupInBalanceCheckExt : PXCacheExtension<CommonSetup>
		{
			/// <summary>
			/// Indicates that inventory balances exist if True, or not exist if False
			/// </summary>
			internal bool? InventoryBalancesExist { get; set; }

			/// <summary>
			/// The buffer keeping the before-edit value of <see cref="CommonSetup.decPlQty"/> field.
			/// </summary>
			internal short? InitialDecPlQty { get; set; }
		}

		protected virtual void _(Events.FieldVerifying<CommonSetup.decPlQty> e)
		{
			var commonSetupExt = ((CommonSetup)e.Row).GetExtension<CommonSetupInBalanceCheckExt>();

			// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Justification]
			commonSetupExt.InventoryBalancesExist ??=
				SelectFrom<INLocationStatusByCostCenter>.
				Where<INLocationStatusByCostCenter.qtyOnHand.IsGreater<Zero>>.
				View.Select(Base).Any();

				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Non-BQL temporary property]
				commonSetupExt.InitialDecPlQty ??= (short?)e.OldValue;
		}
		protected virtual void _(Events.RowSelected<CommonSetup> e)
		{
			if (!(e.Row is CommonSetup row))
				return;

			var commonSetupExt = row.GetExtension<CommonSetupInBalanceCheckExt>();

			var shouldShowWaring =
				commonSetupExt.InventoryBalancesExist == true &&
				commonSetupExt.InitialDecPlQty != row.DecPlQty;

			if (shouldShowWaring)
			{
				e.Cache.RaiseExceptionHandling<CommonSetup.decPlQty>(
					row,
					row.DecPlQty,
					new PXSetPropertyException(
						Messages.DecPlQtyChandedWhileQtyOnHandExists,
						PXErrorLevel.RowWarning,
						commonSetupExt.InitialDecPlQty,
						e.Row.DecPlQty));
			}
		}
	}
}
