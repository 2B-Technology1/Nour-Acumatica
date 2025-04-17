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

namespace PX.Objects.IN.InventoryRelease.Accumulators.CostStatuses.Abstraction
{
	public class CostSiteIDAttribute : PXForeignSelectorAttribute
	{
		public CostSiteIDAttribute()
			: base(typeof(INTran.locationID))
		{ }

		protected override object GetValueExt(PXCache cache, object row)
		{
			int? costSiteID = (int?)cache.GetValue(row, _FieldOrdinal);

			string result = string.Empty;

			INLocation loc = INLocation.PK.Find(cache.Graph, costSiteID);
			INSite site;
			if (loc == null)
			{
				loc = PXSelectReadonly<INLocation, Where<INLocation.siteID, Equal<Required<INLocation.siteID>>>>.SelectWindowed(cache.Graph, 0, 1, costSiteID);
				if (loc == null)
				{
					var insetup = (INSetup)PXSetup<INSetup>.SelectWindowed(cache.Graph, 0, 1);
					if (insetup.TransitSiteID == costSiteID)
					{
						site = INSite.PK.Find(cache.Graph, costSiteID);
						return site.SiteCD;
					}
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExist, Messages.INLocation, nameof(INLocation.siteID) + " = " + costSiteID));
				}
				else
					site = INSite.PK.Find(cache.Graph, loc.SiteID);
			}
			else
				site = INSite.PK.Find(cache.Graph, loc.SiteID);

			object siteCD = site.SiteCD;
			cache.Graph.Caches<INSite>().RaiseFieldSelecting<INSite.siteCD>(loc, ref siteCD, true);

			if (siteCD is PXStringState siteStringState && string.IsNullOrEmpty(siteStringState.InputMask) == false)
			{
				result = PX.Common.Mask.Format(siteStringState.InputMask, (string)siteStringState.Value);
			}
			else if (siteCD is PXFieldState siteState && siteState.Value is string)
			{
				result = (string)siteState.Value;
			}

			if (loc.LocationID == costSiteID)
			{
				object locationCD = loc.LocationCD;
				cache.Graph.Caches<INLocation>().RaiseFieldSelecting<INLocation.locationCD>(loc, ref locationCD, true);

				if (locationCD is PXStringState locationStringState && string.IsNullOrEmpty(locationStringState.InputMask) == false)
				{
					result += " ";
					result += PX.Common.Mask.Format(locationStringState.InputMask, (string)locationStringState.Value);
				}
				else if (locationCD is PXFieldState locationState && locationState.Value is string)
				{
					result += " ";
					result += (string)locationState.Value;
				}
			}

			return result;
		}
	}
}
