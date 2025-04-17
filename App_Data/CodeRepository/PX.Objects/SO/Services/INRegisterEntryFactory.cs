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

namespace PX.Objects.SO.Services
{
	public class INRegisterEntryFactory
	{
		protected SOShipmentEntry _shipGraph;

		protected string _lastShipmentType;
		protected INRegisterEntryBase _lastCreatedGraph;

		public INRegisterEntryFactory(SOShipmentEntry shipGraph)
		{
			_shipGraph = shipGraph;
		}

		public virtual INRegisterEntryBase GetOrCreateINRegisterEntry(SOShipment shipment)
		{
			if (shipment.ShipmentType == _lastShipmentType)
			{
				_lastCreatedGraph.Clear();
				return _lastCreatedGraph;
			}
			
			var ie = (shipment.ShipmentType == SOShipmentType.Transfer)
				? (INRegisterEntryBase)PXGraph.CreateInstance<INTransferEntry>()
				: (INRegisterEntryBase)PXGraph.CreateInstance<INIssueEntry>();

			_shipGraph.MergeCachesWithINRegisterEntry(ie);

			_lastShipmentType = shipment.ShipmentType;
			_lastCreatedGraph = ie;

			return ie;
		}
	}
}
