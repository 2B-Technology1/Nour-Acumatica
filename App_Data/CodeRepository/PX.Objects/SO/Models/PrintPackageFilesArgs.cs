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
using PX.Data;
using PX.SM;

namespace PX.Objects.SO
{
	public class PrintPackageFilesArgs
	{
		public List<SOShipment> Shipments { get; set; }
		public PXAdapter Adapter { get; set; }
		public PackageFileCategory Category { get; set; }

		public string PrintFormID { get; set; }

		public Dictionary<Guid, ShipmentRelatedReports> PrinterToReportsMap { get; set; }
		public PXReportRequiredException RedirectToReport { get; set; }
	}

	[Flags]
	public enum PackageFileCategory
	{
		None = 0,
		CarrierLabel = 0b0001,
		CommercialInvoice = 0b0010,

		All = CarrierLabel | CommercialInvoice
	}

	public class ShipmentRelatedReports
	{
		public List<string> LaserLabels { get; } = new List<string>();
		public List<FileInfo> LabelFiles { get; } = new List<FileInfo>();
		public PXReportRequiredException ReportRedirect { get; set; } = null;
	}
}
