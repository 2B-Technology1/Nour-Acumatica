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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.TaxProvider;
using PX.Objects.Common.Extensions;
using PX.Api.Helpers;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.SO
{
	public class SOOrderEntryBlanketExternalTax : PXGraphExtension<SOOrderEntryExternalTax, SOOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		[PXOverride]
		public virtual IAddressLocation GetToAddress(SOOrder order, SOLine line, Func<SOOrder, SOLine, IAddressLocation> baseFunc)
		{
			if (line?.Behavior == SOBehavior.BL)
			{
				PXResult<CRLocation> customerLocation = SelectFrom<CRLocation>.
					InnerJoin<Address>.On<Address.bAccountID.IsEqual<CRLocation.bAccountID>.
						And<Address.addressID.IsEqual<CRLocation.defAddressID>>>.
					LeftJoin<Carrier>.On<Carrier.carrierID.IsEqual<CRLocation.cCarrierID>>.
					Where<CRLocation.bAccountID.IsEqual<@P.AsInt>.
						And<CRLocation.locationID.IsEqual<@P.AsInt>>>.View.Select(Base, line.CustomerID, line.CustomerLocationID);

				if (customerLocation != null)
				{
					CRLocation shipToLocation = customerLocation;
					Address shipToAddress = customerLocation.GetItem<Address>();
					Carrier shipVia = customerLocation.GetItem<Carrier>();

					if (line.ShipVia != null && line.ShipVia != shipVia.CarrierID)
					{
						shipVia = SelectFrom<Carrier>.Where<Carrier.carrierID.IsEqual<@P.AsString>>.View.Select(Base, line.ShipVia);
					}

					bool willCall = true;

					if (shipVia?.CarrierID != null && shipVia.IsCommonCarrier == true)
					{
						willCall = false;
					}

					if (willCall == true && line.SiteID != null && !(line.POCreate == true && line.POSource == INReplenishmentSource.DropShipToOrder))
						return Base1.GetFromAddress(order, line);
					else
					{
						if (!ExternalTaxBase<SOOrderEntry>.IsEmptyAddress(shipToAddress))
							return shipToAddress;
						else
						throw new PXException(Messages.ExternalTaxesCannotBeCalculatedMissingAddress, shipToLocation.LocationCD);
					}
				}
			}

			return baseFunc(order, line);
		}

		protected virtual void _(Events.RowSelected<SOOrder> e, PXRowSelected baseMethod)
		{
			baseMethod(e.Cache, e.Args);

			if (e.Row == null)
				return;

			if (e.Row.Behavior == SOBehavior.BL)
			{
				Base.Taxes.Cache.AllowInsert = false;
				Base.Taxes.Cache.AllowUpdate = false;
			}
		}

		protected virtual void _(Events.RowSelected<SOLine> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.Behavior == SOBehavior.BL && Base.Document.Current != null && Base1.CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID))
			{
				PXUIFieldAttribute.SetEnabled<SOLine.taxZoneID>(Base.Transactions.Cache, e.Row, false);
			}
		}

		protected virtual void _(Events.RowUpdated<SOOrder> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.Behavior == SOBehavior.BL && Base1.CalculateTaxesUsingExternalProvider(e.Row.TaxZoneID))
			{
				if (!e.Cache.ObjectsEqual<SOOrder.taxZoneID>(e.Row, e.OldRow))
				{
					foreach (SOLine row in Base.Transactions.Select())
					{
						Base.Transactions.Cache.SetValue<SOLine.taxZoneID>(row, e.Row.TaxZoneID);
						Base.Transactions.Cache.MarkUpdated(row, assertError: true);
					}
				}
			}
		}

		protected virtual void _(Events.RowUpdated<SOLine> e)
		{
			//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
			if (Base.Document.Current != null && Base1.CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID))
			{
				if (!e.Cache.ObjectsEqual<SOLine.taxZoneID, SOLine.customerLocationID, SOLine.shipVia>(e.Row, e.OldRow))
				{
					Base1.InvalidateExternalTax(Base.Document.Current);
				}
			}
		}
	}
}
