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
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Work Center Schedule Generic Inquiry
    /// </summary>
    public class GIWorkCenterSchedule : AMGenericInquiry
    {
        protected const string GIID = "15035559-bd12-47d4-b89c-542ce2995e65";

        public GIWorkCenterSchedule() 
            : base(new Guid(GIID))
        {
        }

        /// <summary>
        /// Available parameters for use with this generic inquiry
        /// </summary>
        public static class Parameters
        {
            public const string Warehouse = "WAREHOUSE";
            public const string WorkCenter = "WCID";
            public const string OrderType = "ORDERTYPE";
            public const string ProductionNbr = "PRODORDID";
            public const string DateFrom = "DATEFROM";
            public const string DateTo = "DATETO";
        }

        public virtual void SetParameter(string key, int? idValue, PXGraph graph)
        {
            if (graph == null ||
                idValue == null ||
                string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (key.EqualsWithTrim(Parameters.Warehouse))
            {
                INSite inSite = PXSelect<INSite,
                    Where<INSite.siteID, Equal<Required<INSite.siteID>>>
                >.Select(graph, idValue);

                if (inSite != null)
                {
                    base.SetParameter(key, inSite.SiteCD);
                }
            }
        }
    }
}