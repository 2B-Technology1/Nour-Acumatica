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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN.RelatedItems
{
    public interface ISubstitutableLine
    {
        int? LineNbr { get; set; }

        int? BranchID { get; set; }

        int? CustomerID { get; set; }

        int? SiteID { get; set; }

        int? InventoryID { get; set; }

        int? SubItemID { get; set; }

        string UOM { get; set; }

        decimal? Qty { get; set; }

        decimal? BaseQty { get; set; }

        decimal? UnitPrice { get; set; }

        decimal? CuryUnitPrice { get; set; }

		decimal? CuryExtPrice { get; set; }

        bool? ManualPrice { get; set; }

        bool? SubstitutionRequired { get; set; }
		bool? SkipLineDiscounts { get; set; }
	}
}
