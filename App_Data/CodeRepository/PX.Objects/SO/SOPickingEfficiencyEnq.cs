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
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Common;
using PX.SM;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	/// <exclude />
	public class SOPickingEfficiencyEnq : PXGraph<SOPickingEfficiencyEnq> // SO402020
	{
		public
			PXSetupOptional<SOPickPackShipSetup,
			Where<SOPickPackShipSetup.branchID.IsEqual<AccessInfo.branchID.FromCurrent>>>
			PPSSetup;

		public PXFilter<EfficiencyFilter> Filter;

		public SelectFrom<PickingEfficiency>.View Efficiency;
		public IEnumerable efficiency()
		{
			var filter = Filter.Current;
			var ppsSetup = PPSSetup.Current;

			Func<Guid?, SOPickPackShipUserSetup> GetUserSetup = Func.Memorize((Guid? userID) => SOPickPackShipUserSetup.PK.Find(this, userID) ?? new SOPickPackShipUserSetup().ApplyValuesFrom(ppsSetup));
			Func<string, CS.CSBox> GetBox = Func.Memorize((string boxID) => CS.CSBox.PK.Find(this, boxID));
			Func<int?, (InventoryItem, INLotSerClass)> GetItem = Func.Memorize((int? inventoryID) =>
			{
				var item = InventoryItem.PK.Find(this, inventoryID);
				var lsClass = INLotSerClass.PK.Find(this, item.LotSerClassID)
					?? new INLotSerClass
					{
						LotSerTrack = INLotSerTrack.NotNumbered,
						LotSerAssign = INLotSerAssign.WhenReceived,
						LotSerTrackExpiration = false,
						AutoNextNbr = true
					};
				return (item, lsClass);
			});

			var shipmentsByUser =
				SelectFrom<SOShipmentProcessedByUser>.
				InnerJoin<Users>.On<SOShipmentProcessedByUser.FK.User>.
				InnerJoin<SOShipment>.On<SOShipmentProcessedByUser.FK.Shipment>.
				InnerJoin<INSite>.On<SOShipment.FK.Site>.
				Where<
					SOShipmentProcessedByUser.overallEndDateTime.IsNotNull.
					And<MatchUserFor<INSite>>.
					And<SOShipmentProcessedByUser.overallStartDateTime.IsGreaterEqual<EfficiencyFilter.startDate.FromCurrent>>.
					And<SOShipmentProcessedByUser.overallStartDateTime.IsLessEqual<EfficiencyFilter.endDate.FromCurrent>.
						Or<EfficiencyFilter.endDate.FromCurrent.IsNull>>.
					And<SOShipmentProcessedByUser.userID.IsEqual<EfficiencyFilter.userID.FromCurrent>.
						Or<EfficiencyFilter.userID.FromCurrent.IsNull>>.
					And<SOShipmentProcessedByUser.shipmentNbr.IsEqual<EfficiencyFilter.shipmentNbr.FromCurrent>.
						Or<EfficiencyFilter.shipmentNbr.FromCurrent.IsNull>>.
					And<SOShipment.siteID.IsEqual<EfficiencyFilter.siteID.FromCurrent>.
						Or<EfficiencyFilter.siteID.FromCurrent.IsNull>>>.
				OrderBy<
					SOShipmentProcessedByUser.jobType.Asc,
					SOShipment.siteID.Asc,
					SOShipmentProcessedByUser.shipmentNbr.Asc,
					Users.username.Asc>.
				View.Select(this);

			var result = new List<PickingEfficiency>();

			PickingEfficiency aggregatedRow = null;
			var aggregatedDistinctItems = new HashSet<int?>();
			var aggregatedDistinctLocations = new HashSet<int?>();

			foreach (PXResult<SOShipmentProcessedByUser, Users, SOShipment, INSite> row in shipmentsByUser)
			{
				(var shipByUser, var user, var shipment, var site) = row;

				bool isNewLine = false;
				if (aggregatedRow == null
					|| aggregatedRow.JobType != shipByUser.JobType
					|| aggregatedRow.SiteID != shipment.SiteID
					|| filter.ExpandByUser == true && aggregatedRow.UserID != shipByUser.UserID
					|| filter.ExpandByShipment == true && aggregatedRow.ShipmentNbr != shipByUser.ShipmentNbr
					|| filter.ExpandByDay == true && aggregatedRow.StartDate.Value.Date != shipByUser.StartDateTime.Value.Date)
				{
					aggregatedRow = new PickingEfficiency
					{
						JobType = shipByUser.JobType,
						ShipmentNbr = shipByUser.ShipmentNbr,
						UserID = shipByUser.UserID,
						SiteID = shipment.SiteID,
						OverallStartDate = shipByUser.OverallStartDateTime.Value,
						StartDate = shipByUser.StartDateTime.Value.Date,
						EndDate = shipByUser.EndDateTime.Value.Date,
						OverallEndDate = shipByUser.OverallEndDateTime.Value,
						NumberOfShipments = 0,
						NumberOfLines = 0,
						NumberOfInventories = 0,
						NumberOfPackages = 0,
						TotalSeconds = 0,
						EffectiveSeconds = 0,
						TotalQty = 0,
						NumberOfUsefulOperations = 0
					};

					aggregatedDistinctItems.Clear();
					aggregatedDistinctLocations.Clear();

					isNewLine = true;
				}

				aggregatedRow.EffectiveSeconds += (decimal)(shipByUser.EndDateTime - shipByUser.StartDateTime).Value.TotalSeconds;

				aggregatedRow.StartDate = Tools.Min(aggregatedRow.StartDate.Value.Date, shipByUser.StartDateTime.Value.Date);
				aggregatedRow.EndDate = Tools.Max(aggregatedRow.EndDate.Value.Date, shipByUser.EndDateTime.Value.Date);

				aggregatedRow.OverallStartDate = Tools.Min(aggregatedRow.OverallStartDate.Value, shipByUser.OverallStartDateTime.Value);
				aggregatedRow.OverallEndDate = Tools.Max(aggregatedRow.OverallEndDate.Value, shipByUser.OverallEndDateTime.Value);
				aggregatedRow.TotalSeconds = (decimal)(aggregatedRow.OverallEndDate - aggregatedRow.OverallStartDate).Value.TotalSeconds;

				if (isNewLine || aggregatedRow.ShipmentNbr != shipByUser.ShipmentNbr)
				{
					aggregatedRow.ShipmentNbr = shipByUser.ShipmentNbr;

					var packages =
						SelectFrom<SOPackageDetailEx>.
						Where<SOPackageDetailEx.shipmentNbr.IsEqual<@P.AsString>>.
						View.Select(this, shipByUser.ShipmentNbr)
						.RowCast<SOPackageDetailEx>().ToArray();

					int packagesCount = packages.Length;
					int flexiblePackagesCount = packages.Count(p => GetBox(p.BoxID).AllowOverrideDimension == true);

					var shipmentSplits =
						SelectFrom<SOShipLineSplit>.
						Where<SOShipLineSplit.shipmentNbr.IsEqual<@P.AsString>>.
						View.Select(this, shipByUser.ShipmentNbr).RowCast<SOShipLineSplit>().ToArray();

					var distinctItems = shipmentSplits
						.Where(s => s.InventoryID != null)
						.GroupBy(s => s.InventoryID)
						.ToDictionary(
							g => g.Key,
							g =>
							(
								TotalBaseQty: g.Sum(s => s.BaseQty),
								DistinctLotSerials: g.Where(s => !string.IsNullOrWhiteSpace(s.LotSerialNbr)).Distinct(s => s.LotSerialNbr).Count()
							));
					aggregatedDistinctItems.AddRange(distinctItems.Keys);
					aggregatedRow.NumberOfInventories = aggregatedDistinctItems.Count;

					var distinctLocations = shipmentSplits
						.Where(s => s.LocationID != null)
						.Select(s => s.LocationID)
						.ToHashSet();
					aggregatedDistinctLocations.AddRange(distinctLocations);
					aggregatedRow.NumberOfLocations = aggregatedDistinctLocations.Count;

					aggregatedRow.NumberOfShipments++;
					aggregatedRow.NumberOfLines += shipmentSplits.Distinct(s => s.LineNbr).Count();
					aggregatedRow.NumberOfPackages += packagesCount;
					aggregatedRow.TotalQty += shipmentSplits.Sum(line => line.BaseQty ?? 0);

					var userSetup = GetUserSetup(aggregatedRow.UserID);
					bool requireCarts = ppsSetup.UseCartsForPick == true && aggregatedRow.JobType.IsIn(SOShipmentProcessedByUser.jobType.Pick, SOShipmentProcessedByUser.jobType.PackOnly);
					//bool requireTotes = isPaperless && aggregatedRow.JobType.IsIn(SOShipmentProcessedByUser.jobType.Pick, SOShipmentProcessedByUser.jobType.PackOnly);
					bool requireBoxes = aggregatedRow.JobType.IsIn(SOShipmentProcessedByUser.jobType.Pack, SOShipmentProcessedByUser.jobType.PackOnly);
					bool requireLocations = userSetup.DefaultLocationFromShipment == false && aggregatedRow.JobType.IsIn(SOShipmentProcessedByUser.jobType.Pick, SOShipmentProcessedByUser.jobType.PackOnly);
					bool mayRequireLotSerialAssign = aggregatedRow.JobType.IsIn(SOShipmentProcessedByUser.jobType.Pick, SOShipmentProcessedByUser.jobType.PackOnly);
					bool mayRequireLotSerialSelect = aggregatedRow.JobType == SOShipmentProcessedByUser.jobType.Pack;

					int allUsefulOps = 0;
					{
						// for each shipment a user must
						int scanShipment = 1;
						int confirmShipment = 1;
						int assignCart = requireCarts ? 1 : 0;
						// int assignTotes = requireTotes ? totesCount ? 0;

						allUsefulOps += 1 * (scanShipment + assignCart /*+ assignTotes*/ + confirmShipment);
					}

					if (requireBoxes)
					{
						// for each package a user must
						int scanBox = 1;
						int confirmPackage = 1;
						int inputWeight = ppsSetup.ConfirmEachPackageWeight == true ? 1 : 0;
						int inputDimensions = ppsSetup.ConfirmEachPackageDimensions == true ? 1 : 0;

						allUsefulOps += packagesCount * (scanBox + confirmPackage + inputWeight) + flexiblePackagesCount * (inputDimensions);
					}

					if (requireLocations && ppsSetup.RequestLocationForEachItem == false)
					{
						// for each unique location a user must
						int scanLocation = 1;

						allUsefulOps += distinctLocations.Count * (scanLocation);
					}

					foreach (var item in distinctItems)
					{
						(var inventory, var lsClass) = GetItem(item.Key);

						bool needLotSerial =
							lsClass.LotSerTrack != INLotSerTrack.NotNumbered &&
							(mayRequireLotSerialAssign && (userSetup.DefaultLotSerialFromShipment == false || lsClass.LotSerAssign == INLotSerAssign.WhenUsed) ||
							mayRequireLotSerialSelect && userSetup.DefaultLotSerialFromShipment == false);

						// for each unique item a user must
						int scanItem = 1;
						int scanLotSerialNbr = needLotSerial
							? item.Value.DistinctLotSerials
							: 0;
						int scanExpireDate = needLotSerial && mayRequireLotSerialAssign
							? item.Value.DistinctLotSerials
							: 0;
						int scanLocation = requireLocations && ppsSetup.RequestLocationForEachItem == true
							? Math.Max(scanItem, scanLotSerialNbr)
							: 0;
						int inputQty =
							lsClass.LotSerTrack == INLotSerTrack.SerialNumbered ? 0 :
							needLotSerial ? item.Value.DistinctLotSerials :
							1;
						//int confirmTote = requireTotes && ppsSetup.ConfirmToteForEachItem == true
						//	? Math.Max(scanItem, scanLotSerialNbr)
						//	: 0;
						int explicitlyConfirmLine = ppsSetup.ExplicitLineConfirmation == true
							? Math.Max(scanItem, scanLotSerialNbr)
							: 0;

						allUsefulOps += 1 * (scanLocation + scanItem + scanLotSerialNbr + scanExpireDate + inputQty /*+ confirmTote*/ + explicitlyConfirmLine);
					}

					aggregatedRow.NumberOfUsefulOperations += allUsefulOps;
				}

				if (isNewLine)
					result.Add(aggregatedRow);
			}

			return result.OrderByDescending(r => r.Day).ThenByDescending(r => r.Efficiency).ToList();
		}

		protected virtual void _(Events.RowSelected<EfficiencyFilter> e)
		{
			if (e.Row == null) return;
			Efficiency.Cache.Adjust<PXUIFieldAttribute>()
				.For<PickingEfficiency.day>(a => a.Visible = e.Row.ExpandByDay == true)
				.For<PickingEfficiency.startDate>(a => a.Visible = e.Row.ExpandByDay == false)
				.For<PickingEfficiency.endDate>(a => a.Visible = e.Row.ExpandByDay == false)
				.For<PickingEfficiency.userID>(a => a.Visible = e.Row.ExpandByUser == true)
				.For<PickingEfficiency.numberOfShipments>(a => a.Visible = e.Row.ExpandByShipment == false)
				.For<PickingEfficiency.shipmentNbr>(a => a.Visible = e.Row.ExpandByShipment == true);
		}
	}

	[PXHidden]
	public class EfficiencyFilter : IBqlTable
	{
		#region SiteID
		[Site]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region StartDate
		[PXDBDate]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : BqlDateTime.Field<startDate> { }
		#endregion
		#region EndDate
		[PXDBDate]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : BqlDateTime.Field<endDate> { }
		#endregion
		#region ExpandByUser
		[PXBool]
		[PXUIField(DisplayName = "Expand by User")]
		public bool? ExpandByUser { get; set; }
		public abstract class expandByUser : BqlBool.Field<expandByUser> { }
		#endregion
		#region UserID
		[PXGuid]
		[PXUIField(DisplayName = "User")]
		[PXUIEnabled(typeof(expandByUser))]
		[PXFormula(typeof(Null.When<expandByUser.IsEqual<False>>.Else<userID>))]
		[PXSelector(typeof(SearchFor<Users.pKID>.Where<Users.isHidden.IsEqual<False>>), SubstituteKey = typeof(Users.username))]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : BqlGuid.Field<userID> { }
		#endregion
		#region ExpandByShipment
		[PXBool]
		[PXUIField(DisplayName = "Expand by Shipment")]
		public bool? ExpandByShipment { get; set; }
		public abstract class expandByShipment : BqlBool.Field<expandByShipment> { }
		#endregion
		#region ShipmentNbr
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIEnabled(typeof(expandByShipment))]
		[PXFormula(typeof(Null.When<expandByShipment.IsEqual<False>>.Else<shipmentNbr>))]
		[PXFormula(typeof(Default<siteID>))]
		[PXUIField(DisplayName = "Shipment Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(
			SearchFor<SOShipment.shipmentNbr>.
			Where<
				SOShipment.confirmed.IsEqual<True>.
				And<siteID.FromCurrent.NoDefault.IsNull.
					Or<SOShipment.siteID.IsEqual<siteID.FromCurrent.NoDefault>>>>))]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : BqlString.Field<shipmentNbr> { }
		#endregion
		#region ExpandByDay
		[PXBool]
		[PXUIField(DisplayName = "Expand by Day")]
		public bool? ExpandByDay { get; set; }
		public abstract class expandByDay : BqlBool.Field<expandByDay> { }
		#endregion
	}

	[PXHidden]
	public class PickingEfficiency : IBqlTable
	{
		#region SiteID
		[Site(Enabled = false)]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region OverallStartDate
		[PXDateAndTime]
		public virtual DateTime? OverallStartDate { get; set; }
		public abstract class overallStartDate : BqlDateTime.Field<overallStartDate> { }
		#endregion
		#region StartDate
		[PXDateAndTime]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : BqlDateTime.Field<startDate> { }
		#endregion
		#region EndDate
		[PXDateAndTime]
		[PXUIField(DisplayName = "End Date", Enabled = false)]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : BqlDateTime.Field<endDate> { }
		#endregion
		#region OverallEndDate
		[PXDateAndTime]
		public virtual DateTime? OverallEndDate { get; set; }
		public abstract class overallEndDate : BqlDateTime.Field<overallEndDate> { }
		#endregion
		#region Day
		[PXDate]
		[PXUIField(DisplayName = "Day", Enabled = false)]
		public virtual DateTime? Day { get => StartDate; set { } }
		public abstract class day : BqlDateTime.Field<day> { }
		#endregion
		#region JobType
		[PXString(4, IsFixed = true, IsUnicode = false)]
		[SOShipmentProcessedByUser.jobType.List]
		[PXUIField(DisplayName = "Operation Type", Enabled = false)]
		public virtual String JobType { get; set; }
		public abstract class jobType : BqlString.Field<jobType> { }
		#endregion

		#region ShipmentNbr
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Shipment Nbr.", Enabled = false)]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : BqlString.Field<shipmentNbr> { }
		#endregion
		#region UserID
		[PXGuid]
		[PXUIField(DisplayName = "User", Enabled = false)]
		[PXSelector(typeof(SearchFor<Users.pKID>.Where<Users.isHidden.IsEqual<False>>), SubstituteKey = typeof(Users.username))]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : BqlGuid.Field<userID> { }
		#endregion
		#region NumberOfUsefulOperations
		[PXInt]
		[PXUIField(DisplayName = "Number of Useful Operations", Enabled = false)]
		public virtual int? NumberOfUsefulOperations { get; set; }
		public abstract class numberOfUsefulOperations : BqlInt.Field<numberOfUsefulOperations> { }
		#endregion
		#region NumberOfShipments
		[PXInt]
		[PXUIField(DisplayName = "Number of Shipments", Enabled = false)]
		public virtual int? NumberOfShipments { get; set; }
		public abstract class numberOfShipments : BqlInt.Field<numberOfShipments> { }
		#endregion
		#region NumberOfLines
		[PXInt]
		[PXUIField(DisplayName = "Number of Lines", Enabled = false)]
		public virtual int? NumberOfLines { get; set; }
		public abstract class numberOfLines : BqlInt.Field<numberOfLines> { }
		#endregion
		#region NumberOfLocations
		[PXInt]
		[PXUIField(DisplayName = "Number of Unique Locations", Enabled = false)]
		public virtual int? NumberOfLocations { get; set; }
		public abstract class numberOfLocations : BqlInt.Field<numberOfLocations> { }
		#endregion
		#region NumberOfInventories
		[PXInt]
		[PXUIField(DisplayName = "Number of Unique Items", Enabled = false)]
		public virtual int? NumberOfInventories { get; set; }
		public abstract class numberOfInventories : BqlInt.Field<numberOfInventories> { }
		#endregion
		#region NumberOfPackages
		[PXInt]
		[PXUIField(DisplayName = "Number of Packages", Enabled = false)]
		public virtual int? NumberOfPackages { get; set; }
		public abstract class numberOfPackages : BqlInt.Field<numberOfPackages> { }
		#endregion
		#region TotalQty
		[PXDecimal]
		[PXUIField(DisplayName = "Total Qty.", Enabled = false)]
		public virtual Decimal? TotalQty { get; set; }
		public abstract class totalQty : BqlDecimal.Field<totalQty> { }
		#endregion
		#region TotalSeconds
		[PXDecimal]
		public virtual Decimal? TotalSeconds { get; set; }
		public abstract class totalSeconds : BqlDecimal.Field<totalSeconds> { }
		#endregion
		#region TotalTime
		[PXString]
		[PXUIField(DisplayName = "Total Time", Enabled = false)]
		public virtual String TotalTime { get => $"{(int)(TotalSeconds/3600)}:{(int)(TotalSeconds/60) % 60:00}:{TotalSeconds % 60:00}"; set { } }
		public abstract class totalTime : BqlDecimal.Field<totalTime> { }
		#endregion
		#region EffectiveSeconds
		[PXDecimal]
		public virtual Decimal? EffectiveSeconds { get; set; }
		public abstract class effectiveSeconds : BqlDecimal.Field<effectiveSeconds> { }
		#endregion
		#region EffectiveTime
		[PXString]
		[PXUIField(DisplayName = "Actual Time", Enabled = false)]
		public virtual String EffectiveTime { get => $"{(int)(EffectiveSeconds / 3600)}:{(int)(EffectiveSeconds / 60) % 60:00}:{EffectiveSeconds % 60:00}"; set { } }
		public abstract class effectiveTime : BqlDecimal.Field<effectiveTime> { }
		#endregion
		#region Efficiency
		[PXDecimal]
		[PXUIField(DisplayName = "Efficiency (Operations per Min.)", Enabled = false)]
		public virtual Decimal? Efficiency { get => NumberOfUsefulOperations / (EffectiveSeconds / 60); set { } }
		public abstract class efficiency : BqlDecimal.Field<efficiency> { }
		#endregion
	}
}
