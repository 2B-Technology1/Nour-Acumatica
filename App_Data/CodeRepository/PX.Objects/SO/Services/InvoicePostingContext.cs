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
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN;
using System;
using System.Linq;

namespace PX.Objects.SO.Services
{
	public class InvoicePostingContext
	{
		private IFinPeriodUtils _finPeriodUtils;

		protected Lazy<INIssueEntry> _issueEntry;
		protected Lazy<SOShipmentEntry> _shipmentEntry;
		protected Lazy<SOShipmentEntry> _shipmentEntryDS;
		protected Lazy<SOOrderEntry> _orderEntry;

		protected virtual INIssueEntry InitIssueEntry()
		{
			var issueEntry = PXGraph.CreateInstance<INIssueEntry>();
			issueEntry.FieldVerifying.AddHandler<INTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			issueEntry.FieldVerifying.AddHandler<INTran.siteID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			issueEntry.FieldVerifying.AddHandler<INTran.locationID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			issueEntry.FieldVerifying.AddHandler<INTran.sOOrderNbr>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			issueEntry.FieldVerifying.AddHandler<INTran.sOShipmentNbr>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			issueEntry.FieldVerifying.AddHandler<INTran.pOReceiptNbr>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			issueEntry.FieldVerifying.AddHandler<INTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			issueEntry.FieldVerifying.AddHandler<INTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

			issueEntry.FieldVerifying.AddHandler<INTranSplit.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			issueEntry.FieldVerifying.AddHandler<INTranSplit.siteID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			issueEntry.FieldVerifying.AddHandler<INTranSplit.locationID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

			issueEntry.RowPersisting.AddHandler<INRegister>((PXCache sender, PXRowPersistingEventArgs e) =>
			{
				if (e.Operation.Command() == PXDBOperation.Delete)
					return;

				INRegister document = (INRegister)e.Row;

				var trans =
					PXSelectJoin<INTran,
					InnerJoin<INSite, On<INTran.FK.Site>>,
					Where<INTran.docType, Equal<Required<INRegister.docType>>,
						And<INTran.refNbr, Equal<Required<INRegister.refNbr>>>>>
					.Select(sender.Graph, document.DocType, document.RefNbr).AsEnumerable();

				_finPeriodUtils.ValidateMasterFinPeriod(
					trans.Cast<PXResult<INTran, INSite>>(),
					row => ((INTran)row).TranPeriodID,
					row => new[]
					{
						((INTran)row).BranchID,
						((INSite)row).BranchID,
					},
					typeof(OrganizationFinPeriod.iNClosed));
			});
			return issueEntry;
		}

		protected virtual SOShipmentEntry InitShipmentEntry()
		{
			var shipmentEntry = PXGraph.CreateInstance<SOShipmentEntry>();
			shipmentEntry.MergeCachesWithINRegisterEntry(_issueEntry.Value);
			return shipmentEntry;
		}

		protected virtual SOShipmentEntry InitShipmentEntryDS()
		{
			return PXGraph.CreateInstance<SOShipmentEntry>();;
		}

		protected virtual SOOrderEntry InitOrderEntry()
		{
			return PXGraph.CreateInstance<SOOrderEntry>();;
		}


		public InvoicePostingContext(IFinPeriodUtils finPeriodUtils)
		{
			_finPeriodUtils = finPeriodUtils;
			_issueEntry = new Lazy<INIssueEntry>(InitIssueEntry);
			_shipmentEntry = new Lazy<SOShipmentEntry>(InitShipmentEntry);
			_shipmentEntryDS = new Lazy<SOShipmentEntry>(InitShipmentEntryDS);
			_orderEntry = new Lazy<SOOrderEntry>(InitOrderEntry);
		}

		public virtual INIssueEntry IssueEntry => _issueEntry.Value;

		public virtual SOShipmentEntry GetClearShipmentEntry()
		{
			if (_shipmentEntry.IsValueCreated)
			{
				_shipmentEntry.Value.Clear();
			}

			return _shipmentEntry.Value;
		}

		public virtual SOShipmentEntry GetClearShipmentEntryDS()
		{
			if (_shipmentEntryDS.IsValueCreated)
			{
				_shipmentEntryDS.Value.Clear();
			}

			return _shipmentEntryDS.Value;
		}

		public virtual SOOrderEntry GetClearOrderEntry()
		{
			if (_orderEntry.IsValueCreated)
			{
				_orderEntry.Value.Clear();
			}

			return _orderEntry.Value;
		}
	}
}
