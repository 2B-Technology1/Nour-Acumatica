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

using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PX.Commerce.Objects
{
	public abstract class ShipmentProcessorBase<TGraph, TEntityBucket, TPrimaryMapped> : BCProcessorSingleBase<TGraph, TEntityBucket, TPrimaryMapped>
		where TGraph : PXGraph
		where TEntityBucket : class, IEntityBucket, new()
		where TPrimaryMapped : class, IMappedEntity, new()
	{
		public virtual void MapFilterFields(List<BCShipmentsResult> results, BCShipments impl, CancellationToken cancellationToken = default)
		{
			var result = results.Last();

			impl.ShippingNoteID = result.NoteID;
			impl.VendorRef = result.InvoiceNbr;
			impl.ShipmentNumber = result.ShipmentNumber;
			impl.ShipmentType = result.ShipmentType;
			impl.Status = result.Status;
			impl.ShipmentDate = result.ShipDate;
			impl.Description = result.ShipmentDesc;
			impl.Operation = result.Operation;
			impl.LastModified = result.LastModifiedDateTime;
			impl.Confirmed = result.Confirmed;
			impl.ExternalShipmentUpdated = result.ExternalShipmentUpdated;
			impl.ReceiptNbr = result.ReceiptNbr;
			impl.ReceiptDate = result.ReceiptDate;
			impl.ReceiptType = result.ReceiptType;
			impl.WillCall = result.WillCall;

			impl.OrderNoteIds = new List<Guid?>();
			impl.OrderNoteIds.AddRange(results.Select(x => x.OrderNoteID.Value).ToList());
		}

		/// <summary>
		/// Returns the list of orders' ids part of the Shipment.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public string GetFormattedSyncDescriptionField(List<string> values)
		{
			return string.Join(" | ", values);
		}

		/// <summary>
		/// Returns the list of orderid;shipmentid (in BC) part of the Shipment.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public string GetFormtattedSyncExternalIdField(List<(string, string)> values)
		{
			StringBuilder result = new StringBuilder();

			foreach (var value in values)
			{
				var key = string.IsNullOrEmpty(value.Item2) ? value.Item1 : new object[] { value.Item1, value.Item2 }.KeyCombine();
				result.Append(result.Length > 0 ? "|" + key : key);
			}

			return result.ToString();
		}
	}
}
