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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PO
{
	[PXCacheName(Messages.POReceivePutAwayUserSetup, PXDacType.Config)]
	public class POReceivePutAwayUserSetup : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POReceivePutAwayUserSetup>.By<userID>
		{
			public static POReceivePutAwayUserSetup Find(PXGraph graph, Guid? userID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, userID, options);
		}
		public static class FK
		{
			public class User : Users.PK.ForeignKeyOf<POReceivePutAwayUserSetup>.By<userID> { }
			public class InventoryLabelsReport : SiteMap.PK.ForeignKeyOf<POReceivePutAwaySetup>.By<inventoryLabelsReportID> { }
			public class ScaleDevice : SMScale.PK.ForeignKeyOf<POReceivePutAwayUserSetup>.By<scaleDeviceID> { }
		}
		#endregion
		#region UserID
		[PXDBGuid(IsKey = true)]
		[PXDefault(typeof(Search<Users.pKID, Where<Users.pKID, Equal<Current<AccessInfo.userID>>>>))]
		[PXUIField(DisplayName = "User")]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		#endregion
		#region IsOverridden
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Overridden", Enabled = false)]
		public virtual bool? IsOverridden { get; set; }
		public abstract class isOverridden : PX.Data.BQL.BqlBool.Field<isOverridden> { }
		#endregion

		#region DefaultLotSerialNumber
		[PXDBBool]
		[PXDefault(false, typeof(Search<POReceivePutAwaySetup.defaultLotSerialNumber, Where<POReceivePutAwaySetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Auto-Generated Lot/Serial Nbr.")]
		public virtual bool? DefaultLotSerialNumber { get; set; }
		public abstract class defaultLotSerialNumber : PX.Data.BQL.BqlBool.Field<defaultLotSerialNumber> { } 
		#endregion
		#region DefaultExpireDate
		[PXDBBool]
		[PXDefault(true, typeof(Search<POReceivePutAwaySetup.defaultExpireDate, Where<POReceivePutAwaySetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Expiration Date")]
		public virtual bool? DefaultExpireDate { get; set; }
		public abstract class defaultExpireDate : PX.Data.BQL.BqlBool.Field<defaultExpireDate> { } 
		#endregion
		#region SingleLocation
		[PXDBBool]
		[PXDefault(false, typeof(Search<POReceivePutAwaySetup.singleLocation, Where<POReceivePutAwaySetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Single Receiving Location", FieldClass = IN.LocationAttribute.DimensionName)]
		public virtual bool? SingleLocation { get; set; }
		public abstract class singleLocation : PX.Data.BQL.BqlBool.Field<singleLocation> { }
		#endregion

		#region PrintInventoryLabelsAutomatically
		[PXDBBool]
		[PXDefault(false, typeof(Search<POReceivePutAwaySetup.printInventoryLabelsAutomatically, Where<POReceivePutAwaySetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Print Inventory Labels Automatically", FieldClass = "DeviceHub")]
		public virtual bool? PrintInventoryLabelsAutomatically { get; set; }
		public abstract class printInventoryLabelsAutomatically : PX.Data.BQL.BqlBool.Field<printInventoryLabelsAutomatically> { }
		#endregion
		#region InventoryLabelsReportID
		[PXDefault("IN619200", typeof(Search<POReceivePutAwaySetup.inventoryLabelsReportID, Where<POReceivePutAwaySetup.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Inventory Labels Report ID", FieldClass = "DeviceHub")]
		[PXSelector(typeof(Search<SiteMap.screenID,
			Where<SiteMap.screenID, Like<CA.PXModule.in_>, And<SiteMap.url, Like<Common.urlReports>>>,
			OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
			Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
			DescriptionField = typeof(SiteMap.title))]
		[PXUIEnabled(typeof(printInventoryLabelsAutomatically))]
		[PXUIRequired(typeof(Where<printInventoryLabelsAutomatically, Equal<True>, And<FeatureInstalled<FeaturesSet.deviceHub>>>))]
		public virtual String InventoryLabelsReportID { get; set; }
		public abstract class inventoryLabelsReportID : PX.Data.BQL.BqlString.Field<inventoryLabelsReportID> { }
		#endregion

		#region PrintPurchaseReceiptAutomatically
		[PXDBBool]
		[PXDefault(false, typeof(Search<POReceivePutAwaySetup.printPurchaseReceiptAutomatically, Where<POReceivePutAwaySetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Print Purchase Receipts Automatically", FieldClass = "DeviceHub")]
		public virtual bool? PrintPurchaseReceiptAutomatically { get; set; }
		public abstract class printPurchaseReceiptAutomatically : PX.Data.BQL.BqlBool.Field<printPurchaseReceiptAutomatically> { }
		#endregion

		#region UseScale
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Digital Scale", FieldClass = "DeviceHub", Visible = false)]
		public virtual bool? UseScale { get; set; }
		public abstract class useScale : PX.Data.BQL.BqlBool.Field<useScale> { }
		#endregion
		#region ScaleDeviceID
		[PXScaleSelector]
		[PXUIEnabled(typeof(useScale))]
		[PXUIField(DisplayName = "Scale", FieldClass = "DeviceHub", Visible = false)]
		[PXForeignReference(typeof(Field<scaleDeviceID>.IsRelatedTo<SMScale.scaleDeviceID>))]
		public virtual Guid? ScaleDeviceID { get; set; }
		public abstract class scaleDeviceID : PX.Data.BQL.BqlGuid.Field<scaleDeviceID> { }
		#endregion

		public virtual bool SameAs(POReceivePutAwaySetup setup)
		{
			return
				SingleLocation == setup.SingleLocation &&
				DefaultLotSerialNumber == setup.DefaultLotSerialNumber &&
				DefaultExpireDate == setup.DefaultExpireDate &&
				PrintPurchaseReceiptAutomatically == setup.PrintPurchaseReceiptAutomatically &&
				PrintInventoryLabelsAutomatically == setup.PrintInventoryLabelsAutomatically &&
				InventoryLabelsReportID == setup.InventoryLabelsReportID;
		}

		public virtual POReceivePutAwayUserSetup ApplyValuesFrom(POReceivePutAwaySetup setup)
		{
			SingleLocation = setup.SingleLocation;
			DefaultLotSerialNumber = setup.DefaultLotSerialNumber;
			DefaultExpireDate = setup.DefaultExpireDate;
			PrintPurchaseReceiptAutomatically = setup.PrintPurchaseReceiptAutomatically;
			PrintInventoryLabelsAutomatically = setup.PrintInventoryLabelsAutomatically;
			InventoryLabelsReportID = setup.InventoryLabelsReportID;
			return this;
		}
	}
}