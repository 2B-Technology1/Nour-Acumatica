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

using PX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
    public static class FSToolbarCategory
    {
        public const string CorrectionCategoryID = "Corrections Category";
        public const string SchedulingCategoryID = "Scheduling Category";
        public const string ReplenishmentCategoryID = "Replenishment Category";
        public const string InquiriesCategoryID = "Inquiries Category";
        public const string TravelingID = "Traveling Category";

        [PXLocalizable]
        public static class CategoryNames
        {
            public const string Corrections = "Corrections";
            public const string Scheduling = "Scheduling";
            public const string Replenishment = "Replenishment";
            public const string Inquiries = "Inquiries";
            public const string Traveling = "Traveling";
        }
    }
}
