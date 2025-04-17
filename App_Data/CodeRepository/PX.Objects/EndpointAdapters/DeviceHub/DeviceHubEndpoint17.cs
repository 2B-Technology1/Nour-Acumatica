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

using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Models;
using PX.Data;
using System.Linq;
using System;
using PX.Data.BQL.Fluent;
using PX.SM;
using PX.Data.BQL;

namespace PX.Objects.EndpointAdapters
{
	[PXVersion("17.200.001", "DeviceHub")]
	public class DeviceHubEndpoint17 : DefaultEndpointImpl
	{
		private string defaultDeviceHubID = "DEFAULT";

		[FieldsProcessed(new[] {
			"PrinterID",
			"PrinterName",
			"Description",
			"IsActive"
		})]
		protected void Printer_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var printerName = targetEntity.Fields.SingleOrDefault(f => f.Name == "PrinterName") as EntityValueField;
			var description = targetEntity.Fields.SingleOrDefault(f => f.Name == "Description") as EntityValueField;
			var isActive = targetEntity.Fields.SingleOrDefault(f => f.Name == "IsActive") as EntityValueField;

			if (printerName != null && printerName.Value != null)
			{
				PX.SM.SMPrinterMaint newPrinterGraph = (PX.SM.SMPrinterMaint)PXGraph.CreateInstance(typeof(PX.SM.SMPrinterMaint));

				SMPrinter existingPrinter = SelectFrom<SMPrinter>.Where<SMPrinter.deviceHubID.IsEqual<@P.AsString>.And<SMPrinter.printerName.IsEqual<@P.AsString>>>.View.Select(newPrinterGraph, defaultDeviceHubID, printerName.Value);
				if (existingPrinter != null)
					return;

				PX.SM.SMPrinter printer = new PX.SM.SMPrinter();
				printer.PrinterID = Guid.NewGuid();
				printer.DeviceHubID = defaultDeviceHubID;
				printer.PrinterName = printerName.Value;
				if (description != null)
					printer.Description = description.Value;
				if (isActive != null)
					printer.IsActive = isActive.Value == "true";

				newPrinterGraph.Printers.Insert(printer);
				newPrinterGraph.Save.Press();
			}
		}

		[FieldsProcessed(new[] {
			"PrinterName",
			"Description",
			"IsActive"
		})]
		protected void Printer_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			PX.SM.SMPrinterMaint newPrinterGraph = (PX.SM.SMPrinterMaint)PXGraph.CreateInstance(typeof(PX.SM.SMPrinterMaint));
			var isActive = targetEntity.Fields.SingleOrDefault(f => f.Name == "IsActive") as EntityValueField;
			string printerName = entity.InternalKeys["Printers"]["PrinterName"];

			foreach (PX.SM.SMPrinter existingPrinter in newPrinterGraph.Printers.Select())
			{
				if (existingPrinter.PrinterName == printerName && isActive != null && isActive.Value != null)
				{
					existingPrinter.IsActive = isActive.Value == "true";
					newPrinterGraph.Printers.Update(existingPrinter);
				}
			}
			if (newPrinterGraph.Printers.Cache.IsDirty)
			{
				newPrinterGraph.Save.Press();
			}
		}

		[FieldsProcessed(new[] {
			"JobID",
			"Printer",
			"ReportID",
			"Status"
		})]
		protected void PrintJob_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			PX.SM.SMPrintJobMaint newPrintJobGraph = (PX.SM.SMPrintJobMaint)PXGraph.CreateInstance(typeof(PX.SM.SMPrintJobMaint));
			var status = targetEntity.Fields.SingleOrDefault(f => f.Name == "Status") as EntityValueField;
			int jobID;
			int.TryParse(entity.InternalKeys["Job"]["JobID"], out jobID);

			if (jobID != 0 && status != null && status.Value != null)
			{
				foreach (PX.SM.SMPrintJob existingPrintJob in PXSelect<PX.SM.SMPrintJob, Where<PX.SM.SMPrintJob.jobID, Equal<Required<PX.SM.SMPrintJob.jobID>>>>.Select(newPrintJobGraph, jobID))
				{
					existingPrintJob.Status = status.Value; //status is expected in char form - D, P, F or U
					newPrintJobGraph.Job.Update(existingPrintJob);
				}
				if (newPrintJobGraph.Job.Cache.IsDirty)
				{
					newPrintJobGraph.Save.Press();
				}
			}
		}

	}
}