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
using SP.Objects.IN;
using PX.Objects.AM;

namespace SP.Objects.AM
{
    public class PortalConfigurationSelect<TWhere> : PortalConfigurationSelect
        where TWhere : IBqlWhere, new()
    {
        public PortalConfigurationSelect(PXGraph graph) : base(graph)
        {
            base.View.WhereNew(typeof(TWhere));
        }
    }

    public class PortalConfigurationSelect : ConfigurationSelect
    {
        public PortalConfigurationSelect(PXGraph graph) : base(graph)
        {
        }

        protected override void AMConfigurationResults_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            //SKIPPING BASE CALL
        }

        public static AMConfigurationResults GetConfigurationResult(PXGraph graph, PortalCardLines row)
        {
            return PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.createdByID, Equal<Current<PortalCardLines.userID>>,
                    And<AMConfigurationResults.inventoryID, Equal<Current<PortalCardLines.inventoryID>>,
                        And<AMConfigurationResults.siteID, Equal<Current<PortalCardLines.siteID>>,
                            And<AMConfigurationResults.uOM, Equal<Current<PortalCardLines.uOM>>,
                                And<AMConfigurationResults.ordNbrRef, IsNull,
                                    And<AMConfigurationResults.opportunityQuoteID, IsNull>>>>>>>.SelectSingleBound(graph, new object[] { row });
        }
    }
}