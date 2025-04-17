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

using System.Collections;
using System.Reflection;
using PX.Data;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CN.Subcontracts.SM.Extension;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CN.Subcontracts.SM.GraphExtensions
{
    public class SmNotificationMaintExt : PXGraphExtension<SMNotificationMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected IEnumerable entityItems(string parent)
        {
			IEnumerable cacheEntityItems = Base.entityItems(parent); 

            var siteMap = PXSiteMap.Provider.FindSiteMapNodeByScreenID(Base.Notifications.Current.ScreenID);

            foreach (CacheEntityItem cacheEntityItem in cacheEntityItems)
            {
                if (siteMap.GraphType == typeof(SubcontractEntry).FullName)
                {
                    cacheEntityItem.Name = cacheEntityItem.GetSubcontractViewName();
                }
                yield return cacheEntityItem;
            }
        }
    }
}