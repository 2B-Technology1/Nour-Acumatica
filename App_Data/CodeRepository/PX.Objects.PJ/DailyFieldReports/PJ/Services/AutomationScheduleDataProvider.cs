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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.SM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Services
{
    public class AutomationScheduleDataProvider
    {
        private readonly PXGraph graph;

        public AutomationScheduleDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public AUSchedule GetAutomationSchedule(string screenId)
        {
            return SelectFrom<AUSchedule>.Where<AUSchedule.screenID.IsEqual<P.AsString>>.View.Select(graph, screenId);
        }
    }
}