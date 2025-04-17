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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.SO.GraphExtensions
{
	public class ShipmentWorkLog<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		public SelectFrom<SOShipmentProcessedByUser>.View ShipmentWorkLogView;

		public virtual void EnsureFor(string shipmentNbr, Guid? userID, [SOShipmentProcessedByUser.jobType.List] string jobType)
		{
			TimeSpan pickingTimeout = GetPickingTimeOut();
			DateTime now = GetServerTime();

			SOShipmentProcessedByUser NewLink(DateTime? groupStartDateTime = null) => new SOShipmentProcessedByUser
			{
				JobType = jobType,
				ShipmentNbr = shipmentNbr,
				UserID = userID,
				OverallStartDateTime = groupStartDateTime ?? now,
				StartDateTime = now,
				LastModifiedDateTime = now
			};

			SOShipmentProcessedByUser openLink =
				SelectFrom<SOShipmentProcessedByUser>.
				Where<
					SOShipmentProcessedByUser.jobType.IsEqual<@P.AsString.Fixed.ASCII>.
					And<SOShipmentProcessedByUser.shipmentNbr.IsEqual<@P.AsString>>.
					And<SOShipmentProcessedByUser.userID.IsEqual<@P.AsGuid>>.
					And<SOShipmentProcessedByUser.endDateTime.IsNull>>.
				View.Select(Base, jobType, shipmentNbr, userID);

			if (openLink == null)
			{
				ShipmentWorkLogView.Insert(NewLink());
			}
			else if (openLink.LastModifiedDateTime.Value.Add(pickingTimeout) > now)
			{
				openLink.LastModifiedDateTime = openLink.LastModifiedDateTime.Value.Add(pickingTimeout);
				ShipmentWorkLogView.Update(openLink);
			}
			else // open log has expired
			{
				openLink.EndDateTime = openLink.LastModifiedDateTime.Value.Add(pickingTimeout);
				ShipmentWorkLogView.Update(openLink);

				ShipmentWorkLogView.Insert(NewLink(openLink.OverallStartDateTime));
			}

			var otherOpenLinks =
				SelectFrom<SOShipmentProcessedByUser>.
				Where<
					SOShipmentProcessedByUser.shipmentNbr.IsNotEqual<@P.AsString>.
					And<SOShipmentProcessedByUser.userID.IsEqual<@P.AsGuid>>.
					And<SOShipmentProcessedByUser.endDateTime.IsNull>>.
				View.Select(Base, shipmentNbr, userID);
			foreach (SOShipmentProcessedByUser shipByUser in otherOpenLinks)
			{
				shipByUser.EndDateTime = Tools.Min(now, shipByUser.LastModifiedDateTime.Value.Add(pickingTimeout));
				ShipmentWorkLogView.Update(shipByUser);
			}
		}

		public virtual bool SuspendFor(string shipmentNbr, Guid? userID, [SOShipmentProcessedByUser.jobType.List] string jobType)
		{
			TimeSpan pickingTimeout = GetPickingTimeOut();
			DateTime now = GetServerTime();

			SOShipmentProcessedByUser openLink =
				SelectFrom<SOShipmentProcessedByUser>.
				Where<
					SOShipmentProcessedByUser.jobType.IsEqual<@P.AsString.Fixed.ASCII>.
					And<SOShipmentProcessedByUser.shipmentNbr.IsEqual<@P.AsString>>.
					And<SOShipmentProcessedByUser.userID.IsEqual<@P.AsGuid>>.
					And<SOShipmentProcessedByUser.endDateTime.IsNull>>.
				View.Select(Base, jobType, shipmentNbr, userID);

			if (openLink != null)
			{
				openLink.EndDateTime = Tools.Min(now, openLink.LastModifiedDateTime.Value.Add(pickingTimeout));
				ShipmentWorkLogView.Update(openLink);
				return true;
			}

			return false;
		}

		public virtual void CloseFor(string shipmentNbr)
		{
			TimeSpan pickingTimeout = GetPickingTimeOut();
			DateTime now = GetServerTime();

			foreach (SOShipmentProcessedByUser shipByUser in
				SelectFrom<SOShipmentProcessedByUser>.
				Where<SOShipmentProcessedByUser.shipmentNbr.IsEqual<@P.AsString>>.
				View.Select(Base, shipmentNbr))
			{
				shipByUser.Confirmed = true;

				DateTime endDate = Tools.Min(now, shipByUser.LastModifiedDateTime.Value.Add(pickingTimeout));

				if (shipByUser.EndDateTime == null)
					shipByUser.EndDateTime = endDate;

				shipByUser.OverallEndDateTime = endDate;

				ShipmentWorkLogView.Update(shipByUser);
			}
		}

		public virtual void PersistWorkLog()
		{
			using (var tran = new PXTransactionScope())
			{
				ShipmentWorkLogView.Cache.Persist(PXDBOperation.Insert);
				ShipmentWorkLogView.Cache.Persist(PXDBOperation.Update);

				tran.Complete(Base);
			}

			ShipmentWorkLogView.Cache.Persisted(false);
		}

		protected virtual TimeSpan GetPickingTimeOut() => TimeSpan.FromMinutes(10);
		protected virtual DateTime GetServerTime()
		{
			PXDatabase.SelectDate(out DateTime _, out var dbNow);
			dbNow = PXTimeZoneInfo.ConvertTimeFromUtc(dbNow, LocaleInfo.GetTimeZone());
			return dbNow;
		}
	}
}
