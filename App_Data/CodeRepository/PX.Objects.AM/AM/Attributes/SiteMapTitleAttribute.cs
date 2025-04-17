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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Attribute to display a screen title related to the identified source screen ID
    /// </summary>
    public class SiteMapTitleAttribute : PXStringAttribute, IPXFieldSelectingSubscriber
    {
        private Type _SiteMapScreenID;
        public SiteMapTitleAttribute(Type siteMapScreenID) : base(50)
        {
            _SiteMapScreenID = siteMapScreenID;
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.Row == null) return;

            string screenID = sender.GetValue(e.Row, _SiteMapScreenID.Name) as string;
            if (!string.IsNullOrEmpty(screenID))
            {
                var siteMapNode = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID);
                if (siteMapNode != null)
                {
                    e.ReturnValue = siteMapNode.Title;
                }
            }
        }
    }
}