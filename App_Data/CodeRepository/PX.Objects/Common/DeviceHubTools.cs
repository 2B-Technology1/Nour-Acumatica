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
using PX.SM;
using System.Collections.Generic;
using System.Threading;

namespace PX.Objects.Common
{
	public static class DeviceHubTools
	{
		public static async System.Threading.Tasks.Task PrintReportViaDeviceHub<TBAccount>(PXGraph graph, string reportID, Dictionary<string, string> reportParameters, string notificationSource, TBAccount baccount, CancellationToken cancellationToken)
			where TBAccount : CR.BAccount, new()
		{
			CR.NotificationUtility notificationUtility = new CR.NotificationUtility(graph);
			var reportsToPrint = SMPrintJobMaint.AssignPrintJobToPrinter(
				new Dictionary<PrintSettings, PXReportRequiredException>(),
				reportParameters,
				new PrintSettings { PrintWithDeviceHub = true, DefinePrinterManually = false },
				notificationUtility.SearchPrinter,
				notificationSource,
				reportID,
				baccount == null ? reportID : notificationUtility.SearchReport<TBAccount>(reportID, baccount.BAccountID, graph.Accessinfo.BranchID),
				graph.Accessinfo.BranchID);

			await SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint, cancellationToken);
		}
	}
}
