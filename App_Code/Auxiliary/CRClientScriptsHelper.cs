using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;
using PX.Data.Maintenance.GI;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.SM;
using PX.Web.UI;

namespace CRHelpers
{
	[PXInternalUseOnly]
	public static class CRClientScriptsHelper
	{
		public static void ChangeRedirectWindowMode(this PXBaseDataSource ds, PXNavigateEventArgs e, PXBaseRedirectException.WindowMode newWindowMode)
		{
			try
			{
				if (string.IsNullOrEmpty(e.NavigateUrl) is false)
					throw new PXRedirectToUrlException(e.NavigateUrl, newWindowMode, "");
			}
			catch (PXBaseRedirectException ex)
			{
				new PXBaseDataSource.RedirectHelper(ds).TryRedirect(ex);
			}
		}


		public static void TryRedirectToInquiry(this PXBaseDataSource ds, PXBaseDataSource.PXNavigateEventArgs e)
		{
			if (e.Row is GIDesign gi)
			{
				try
				{
					throw new PXRedirectToGIRequiredException(gi);
				}
				catch (PXRedirectToUrlException ex)
				{
					e.NewUrl = ex.Url;
				}
			}
		}

		public static void TryRedirectToScreen(this PXBaseDataSource ds, PXBaseDataSource.PXNavigateEventArgs e)
		{
			try
			{
				var row   = PXResult.Unwrap<SiteMap>(e.Row);
				if (row?.ScreenID != null
				    && PXSiteMap.Provider.FindSiteMapNodeByScreenID(row.ScreenID) is PXSiteMapNode node)
				{
					e.NewUrl = node.Url;
				}
			}
			catch
			{
				// ignore
			}
		}

		public static void SubscribeRedirectsToInquiries(this PXBaseDataSource ds, params Type[] contactGiFields)
		{
			SubscribeRedirects(ds, TryRedirectToInquiry, contactGiFields);
		}

		public static void SubscribeRedirectsToScreens(this PXBaseDataSource ds, params Type[] screenIdFields)
		{
			SubscribeRedirects(ds, TryRedirectToScreen, screenIdFields);
		}


		public static void SubscribeRedirects(this PXBaseDataSource ds,
		                                      Action<PXBaseDataSource, PXBaseDataSource.PXNavigateEventArgs> subscription,
		                                      params Type[] selectorFields)
		{
			var views = selectorFields
			           .SelectMany(field => ds.DataGraph
			                                  .Caches[field.DeclaringType]
			                                  .GetAttributesOfType<PXSelectorAttribute>(null, field.Name)
			                                  .Select(attr => attr.ViewName))
			           .ToList();
			if (views.Count == 0)
				return;

			ds.OnNavigateEvent += (s, e) =>
			{
				if(views.Contains(e.ViewName))
					subscription(ds, e);
			};
		}
	}
}
