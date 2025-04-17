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

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INScanUserSetup, PXDacType.Config)]
	public class INScanUserSetup : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INScanUserSetup>.By<userID, mode>
		{
			public static INScanUserSetup Find(PXGraph graph, Guid? userID, string mode, PKFindOptions options = PKFindOptions.None) => FindBy(graph, userID, mode, options);
		}
		public static class FK
		{ 
			public class User : Users.PK.ForeignKeyOf<INScanUserSetup>.By<userID> { }
			public class ScaleDevice : SMScale.PK.ForeignKeyOf<INScanUserSetup>.By<scaleDeviceID> { }
			public class InventoryLabelsReport : SiteMap.PK.ForeignKeyOf<INScanUserSetup>.By<inventoryLabelsReportID> { }
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

		#region Mode
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Mode", Enabled = false, Visible = false)]
		public virtual string Mode { get; set; }
		public abstract class mode : PX.Data.BQL.BqlString.Field<mode> { }
		#endregion

		#region DefaultWarehouse
		[PXDBBool]
		[PXDefault(true, typeof(Search<INScanSetup.defaultWarehouse, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Default Warehouse from User Profile")]
		public virtual bool? DefaultWarehouse { get; set; }
		public abstract class defaultWarehouse : PX.Data.BQL.BqlBool.Field<defaultWarehouse> { }
		#endregion

		#region PrintInventoryLabelsAutomatically
		[PXDBBool]
		[PXDefault(false, typeof(Search<INScanSetup.printInventoryLabelsAutomatically, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Print Inventory Labels Automatically", FieldClass = "DeviceHub")]
		public virtual bool? PrintInventoryLabelsAutomatically { get; set; }
		public abstract class printInventoryLabelsAutomatically : PX.Data.BQL.BqlBool.Field<printInventoryLabelsAutomatically> { }
		#endregion
		#region InventoryLabelsReportID
		[PXDefault("IN619200", typeof(Search<INScanSetup.inventoryLabelsReportID, Where<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
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

		#region UseDefaultLotSerialNbr
		[PXDBBool]
		[PXDefault(false, typeof(
			Search<INScanSetup.useDefaultLotSerialNbrInTransfer, Where<mode.FromCurrent, Equal<INDocType.transfer>, And<INScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>>
			))]
		[PXUIField(DisplayName = "Use Default Lot/Serial Nbr.")]
		public virtual bool? UseDefaultLotSerialNbr { get; set; }
		public abstract class useDefaultLotSerialNbr : PX.Data.BQL.BqlBool.Field<useDefaultLotSerialNbr> { }
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
		[PXForeignReference(typeof(FK.ScaleDevice))]
		public virtual Guid? ScaleDeviceID { get; set; }
		public abstract class scaleDeviceID : PX.Data.BQL.BqlGuid.Field<scaleDeviceID> { }
		#endregion

		#region tstamp
		public abstract class Tstamp : Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		public virtual bool SameAs(INScanSetup setup)
		{
			return
				DefaultWarehouse == setup.DefaultWarehouse &&
				PrintInventoryLabelsAutomatically == setup.PrintInventoryLabelsAutomatically &&
				InventoryLabelsReportID == setup.InventoryLabelsReportID &&
				(
					Mode == INDocType.Transfer && UseDefaultLotSerialNbr == setup.UseDefaultLotSerialNbrInTransfer
					//another modes
				);
		}

		public virtual INScanUserSetup ApplyValuesFrom(INScanSetup setup)
		{
			DefaultWarehouse = setup.DefaultWarehouse;
			PrintInventoryLabelsAutomatically = setup.PrintInventoryLabelsAutomatically;
			InventoryLabelsReportID = setup.InventoryLabelsReportID;
			switch(Mode)
			{
				case INDocType.Transfer:
					UseDefaultLotSerialNbr = setup.UseDefaultLotSerialNbrInTransfer;
					break;
				default:
					UseDefaultLotSerialNbr = false;
					break;
			}
			return this;
		}
	}
}
