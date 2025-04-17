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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.BarcodeProcessing;

namespace PX.Objects.IN.WMS
{
	using WMSBase = BarcodeDrivenStateMachine<StoragePlaceLookup, StoragePlaceLookup.Host>;

	public class StoragePlaceLookup : WMSBase
	{
		public class Host : StoragePlaceEnq { }

		#region State
		public StorageLookupScanHeader StorageHeader => Header.Get<StorageLookupScanHeader>();
		public ValueSetter<ScanHeader>.Ext<StorageLookupScanHeader> StorageSetter => HeaderSetter.With<StorageLookupScanHeader>();

		#region SiteID
		public int? SiteID
		{
			get => StorageHeader.SiteID;
			set => StorageSetter.Set(h => h.SiteID, value);
		}
		#endregion
		#region StorageID
		public int? StorageID
		{
			get => StorageHeader.StorageID;
			set => StorageSetter.Set(h => h.StorageID, value);
		}
		#endregion
		#region IsCart
		public bool? IsCart
		{
			get => StorageHeader.IsCart;
			set => StorageSetter.Set(h => h.IsCart, value);
		}
		#endregion
		#endregion

		#region Buttons
		public PXAction<ScanHeader> ReviewAvailability;
		[PXButton, PXUIField(DisplayName = "Review")]
		protected virtual IEnumerable reviewAvailability(PXAdapter adapter) => adapter.Get();
		#endregion

		#region Event Handlers
		protected virtual void _(Events.RowUpdated<StoragePlaceEnq.StoragePlaceFilter> e) => e.Cache.IsDirty = false;
		protected virtual void _(Events.RowInserted<StoragePlaceEnq.StoragePlaceFilter> e) => e.Cache.IsDirty = false;
		protected override void _(Events.RowSelected<ScanHeader> e)
		{
			base._(e);
			ReviewAvailability.SetVisible(Base.IsMobile);
			//Base.storages.Cache.Adjust<PXUIFieldAttribute>().For<StoragePlaceStatus.splittedIcon>(a => a.Visible = e.Row.IsCart == false);
		}
		//protected virtual void _(Events.FieldDefaulting<StoragePlaceEnq.StoragePlaceFilter, StoragePlaceEnq.StoragePlaceFilter.expandByLotSerialNbr> e) => e.NewValue = true;
		#endregion

		#region Views
		public PXSetupOptional<INScanSetup, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>> Setup;
		#endregion

		#region State Machine
		protected override IEnumerable<ScanMode<StoragePlaceLookup>> CreateScanModes() => new[]
		{
			new ScanMode.Simple("STOR", Msg.ModeDescription)
				.Intercept.CreateStates.ByReplace(() => new ScanState<StoragePlaceLookup>[]
				{
					new WarehouseState(),
					new StorageState()
				})
				.Intercept.CreateCommands.ByReplace(() => new ScanCommand<StoragePlaceLookup>[]
				{
					new SwitchLotSerialViewCommand()
				})
				.Intercept.CreateRedirects.ByReplace(() => AllWMSRedirects.CreateFor<StoragePlaceLookup>())
				.Intercept.ResetMode.ByReplace((basis, fullReset) =>
				{
					basis.Clear<WarehouseState>(when: fullReset);
					basis.Clear<StorageState>();
				})
		};
		#endregion

		#region States
		public sealed class WarehouseState : WarehouseState<StoragePlaceLookup>
		{
			protected override int? SiteID
			{
				get => Basis.SiteID;
				set => Basis.SiteID = value;
			}

			protected override Validation Validate(INSite site) => Basis.IsValid<StorageLookupScanHeader.siteID>(site.SiteID, out string error) ? Validation.Ok : Validation.Fail(error);

			protected override void SetNextState() => Basis.SetScanState<StorageState>();

			protected override bool UseDefaultWarehouse => Basis.Setup.Current.DefaultWarehouse == true;
		}

		public sealed class StorageState : EntityState<StoragePlace>
		{
			public const string Value = "STOR";
			public class value : BqlString.Constant<value> { public value() : base(StorageState.Value) { } }

			public override string Code => Value;
			protected override string StatePrompt => Msg.Prompt;

			protected override StoragePlace GetByBarcode(string barcode)
			{
				return
					SelectFrom<StoragePlace>.
					Where<
						StoragePlace.siteID.IsEqual<StorageLookupScanHeader.siteID.FromCurrent>.
						And<StoragePlace.storageCD.IsEqual<@P.AsString>>>.
					View.ReadOnly.Select(Basis, barcode);
			}

			protected override AbsenceHandling.Of<StoragePlace> HandleAbsence(string barcode)
			{
				if (Basis.TryProcessBy<WarehouseState>(barcode, StateSubstitutionRule.KeepPositiveReports | StateSubstitutionRule.KeepApplication))
					return AbsenceHandling.Done;
				return base.HandleAbsence(barcode);
			}

			protected override void ReportMissing(string barcode) => Basis.Reporter.Error(Msg.Missing, barcode);
			protected override Validation Validate(StoragePlace storage) => storage.Active == true ? Validation.Ok : Validation.Fail(Messages.InactiveLocation, storage.StorageCD);

			protected override void Apply(StoragePlace storage)
			{
				Basis.StorageID = storage.StorageID;
				Basis.IsCart = storage.IsCart;

				Basis.Graph.Filter.Insert();
				Basis.Graph.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.siteID>(Basis.Graph.Filter.Current,
					storage.SiteID);
				Basis.Graph.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.storageType>(Basis.Graph.Filter.Current,
					storage.IsCart == true ? StoragePlaceEnq.StoragePlaceFilter.storageType.Carts : StoragePlaceEnq.StoragePlaceFilter.storageType.Locations);
				Basis.Graph.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.locationID>(Basis.Graph.Filter.Current,
					storage.IsCart == true ? null : storage.StorageID);
				Basis.Graph.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.cartID>(Basis.Graph.Filter.Current,
					storage.IsCart == true ? storage.StorageID : null);
				Basis.Graph.Filter.Cache.IsDirty = false;
				Basis.Graph.Filter.UpdateCurrent();
			}
			protected override void ClearState()
			{
				Basis.StorageID = null;
				Basis.IsCart = false;

				if (Basis.Graph.Filter.Current != null)
				{
					Basis.Graph.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.siteID>(Basis.Graph.Filter.Current, null);
					Basis.Graph.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.locationID>(Basis.Graph.Filter.Current, null);
					Basis.Graph.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.cartID>(Basis.Graph.Filter.Current, null);
					Basis.Graph.Filter.Cache.IsDirty = false;
					Basis.Graph.Filter.UpdateCurrent();
				}
			}

			protected override void ReportSuccess(StoragePlace storage) => Basis.Reporter.Info(Msg.Ready, storage.StorageCD);
			protected override void SetNextState() { }

			#region Messages
			[PXLocalizable]
			public abstract class Msg
			{
				public static string Prompt => PXAccess.FeatureInstalled<CS.FeaturesSet.wMSCartTracking>() ? PromptWithCart : PromptNoCart;
				public const string PromptNoCart = "Scan the barcode of the location.";
				public const string PromptWithCart = "Scan the barcode of the location or of the cart.";
				public const string Ready = "The {0} storage is selected.";
				public const string Missing = "The {0} storage is not found.";
			}
			#endregion
		}
		#endregion

		public sealed class SwitchLotSerialViewCommand : ScanCommand
		{
			public override string Code => "SWITCH*LS";
			public override string ButtonName => "scanSwitchLotSerialView";
			public override string DisplayName => Msg.DisplayName;
			protected override bool IsEnabled => true;

			protected override bool Process()
			{
				Basis.Graph.Filter.Cache.SetValueExt<StoragePlaceEnq.StoragePlaceFilter.expandByLotSerialNbr>(Basis.Graph.Filter.Current,
					!(Basis.Graph.Filter.Current.ExpandByLotSerialNbr == true));
				Basis.Graph.Filter.Cache.IsDirty = false;
				Basis.Graph.Filter.UpdateCurrent();
				return true;
			}

			[PXLocalizable]
			public class Msg
			{
				public const string DisplayName = "Switch Lot/Serial View";
			}
		}

		#region Redirect
		public new sealed class RedirectFrom<TForeignBasis> : WMSBase.RedirectFrom<TForeignBasis>
			where TForeignBasis : PXGraphExtension, IBarcodeDrivenStateMachine
		{
			public override string Code => "STORAGE";
			public override string DisplayName => Msg.ModeDescription;

			public override bool IsPossible => PXAccess.FeatureInstalled<CS.FeaturesSet.wMSInventory>();
		}
		#endregion

		#region Messages
		[PXLocalizable]
		public new abstract class Msg : WMSBase.Msg
		{
			public const string ModeDescription = "Storage Lookup";
		}
		#endregion
	}

	public sealed class StorageLookupScanHeader : PXCacheExtension<ScanHeader>
	{
		#region SiteID
		[Site(Enabled = false)]
		public int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region StorageID
		[PXInt]
		[PXUIField(DisplayName = "Storage ID", Enabled = false)]
		[PXSelector(
			typeof(Search<StoragePlace.storageID, Where<StoragePlace.active, Equal<True>, And<StoragePlace.siteID, Equal<Current<siteID>>>>>),
			typeof(StoragePlace.siteID), typeof(StoragePlace.storageCD), typeof(StoragePlace.isCart), typeof(StoragePlace.active),
			SubstituteKey = typeof(StoragePlace.storageCD),
			DescriptionField = typeof(StoragePlace.descr),
			ValidateValue = false)]
		public int? StorageID { get; set; }
		public abstract class storageID : BqlInt.Field<storageID> { }
		#endregion
		#region IsCart
		[PXBool]
		[PXUIField(DisplayName = "Cart", IsReadOnly = true, FieldClass = "Carts")]
		public Boolean? IsCart { get; set; }
		public abstract class isCart : BqlBool.Field<isCart> { }
		#endregion
	}
}
