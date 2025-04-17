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
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.DR
{
	public class InventoryItemMaintBaseASC606 : PXGraphExtension<InventoryItemMaintBase>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.aSC606>();
		}

		protected virtual void _(Events.FieldUpdated<INComponent, INComponent.deferredCode> e)
		{
			if (e.Row == null)
				return;

			DRDeferredCode code = (DRDeferredCode)PXSelectorAttribute.Select(e.Cache, e.Row, typeof(INComponent.deferredCode).Name);
			if (code != null)
			{
				e.Cache.SetValueExt< INComponent.overrideDefaultTerm>(e.Row, DeferredMethodType.RequiresTerms(code));
			}
			else
			{
				e.Cache.SetValueExt<INComponent.overrideDefaultTerm>(e.Row, false);
			}
		}

		protected virtual void _(Events.FieldUpdated<INComponent, INComponent.componentID> e)
		{
			if (e.Row == null)
				return;

			DRDeferredCode code = (DRDeferredCode)PXSelectorAttribute.Select(e.Cache, e.Row, typeof(INComponent.deferredCode).Name);
			if (code != null)
			{
				e.Cache.SetValueExt<INComponent.overrideDefaultTerm>(e.Row, DeferredMethodType.RequiresTerms(code));
			}
			else
			{
				e.Cache.SetValueExt<INComponent.overrideDefaultTerm>(e.Row, false);
			}
		}
	}
}
