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
using System.Web.Configuration;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.SO
{
	[Serializable]
	[PXCacheName(Messages.SOPickPackShipUserSetup, PXDacType.Config)]
	public class SOPickPackShipUserSetup : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOPickPackShipUserSetup>.By<userID>
		{
			public static SOPickPackShipUserSetup Find(PXGraph graph, Guid? userID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, userID, options);
		}
		public static class FK
		{ 
			public class User : Users.PK.ForeignKeyOf<SOPickPackShipUserSetup>.By<userID> { }
			public class ScaleDevice : SMScale.PK.ForeignKeyOf<SOPickPackShipUserSetup>.By<scaleDeviceID> { }
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

		#region DefaultLocationFromShipment
		[PXDBBool]
		[PXDefault(false, typeof(Search<SOPickPackShipSetup.defaultLocationFromShipment, Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Location")]
		public virtual bool? DefaultLocationFromShipment { get; set; }
		public abstract class defaultLocationFromShipment : PX.Data.BQL.BqlBool.Field<defaultLocationFromShipment> { }
		#endregion

		#region DefaultLotSerialFromShipment
		[PXDBBool]
		[PXDefault(false, typeof(Search<SOPickPackShipSetup.defaultLotSerialFromShipment, Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Lot/Serial Nbr.")]
		public virtual bool? DefaultLotSerialFromShipment { get; set; }
		public abstract class defaultLotSerialFromShipment : PX.Data.BQL.BqlBool.Field<defaultLotSerialFromShipment> { }
		#endregion

		#region PrintShipmentConfirmation
		[PXDBBool]
		[PXDefault(false, typeof(Search<SOPickPackShipSetup.printShipmentConfirmation, Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Print Shipment Confirmation Automatically", FieldClass = "DeviceHub")]
		public virtual bool? PrintShipmentConfirmation { get; set; }
		public abstract class printShipmentConfirmation : PX.Data.BQL.BqlBool.Field<printShipmentConfirmation> { }
		#endregion
		#region PrintShipmentLabels
		[PXDBBool]
		[PXDefault(false, typeof(Search<SOPickPackShipSetup.printShipmentLabels, Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Print Shipment Labels Automatically", FieldClass = "DeviceHub")]
		public virtual bool? PrintShipmentLabels { get; set; }
		public abstract class printShipmentLabels : PX.Data.BQL.BqlBool.Field<printShipmentLabels> { }
		#endregion
		#region PrintCommercialInvoices
		[PXDBBool]
		[PXDefault(false, typeof(Search<SOPickPackShipSetup.printCommercialInvoices, Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Print Commercial Invoices Automatically", FieldClass = "DeviceHub")]
		public virtual bool? PrintCommercialInvoices { get; set; }
		public abstract class printCommercialInvoices : PX.Data.BQL.BqlBool.Field<printCommercialInvoices> { }
		#endregion
		#region EnterSizeForPackages
		[PXDBBool]
		[PXDefault(false, typeof(Search<SOPickPackShipSetup.enterSizeForPackages, Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Enter Length/Width/Height for Packages", Visible = false)]
		public virtual bool? EnterSizeForPackages { get; set; }
		public abstract class enterSizeForPackages : PX.Data.BQL.BqlBool.Field<enterSizeForPackages> { }
		#endregion

		#region UseScale
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Digital Scale", FieldClass = "DeviceHub")]
		public virtual bool? UseScale { get; set; }
		public abstract class useScale : PX.Data.BQL.BqlBool.Field<useScale> { }
		#endregion
		#region ScaleDeviceID
		[PXScaleSelector]
		[PXUIEnabled(typeof(useScale))]
		[PXUIField(DisplayName = "Scale", FieldClass = "DeviceHub")]
		[PXForeignReference(typeof(FK.ScaleDevice))]
		public virtual Guid? ScaleDeviceID { get; set; }
		public abstract class scaleDeviceID : PX.Data.BQL.BqlGuid.Field<scaleDeviceID> { }
		#endregion

		public virtual bool SameAs(SOPickPackShipSetup setup)
		{
			return
				DefaultLocationFromShipment == setup.DefaultLocationFromShipment &&
				DefaultLotSerialFromShipment == setup.DefaultLotSerialFromShipment &&
				PrintShipmentLabels == setup.PrintShipmentLabels &&
				PrintCommercialInvoices == setup.PrintCommercialInvoices &&
				PrintShipmentConfirmation == setup.PrintShipmentConfirmation &&
				EnterSizeForPackages == setup.EnterSizeForPackages;

		}

		public virtual SOPickPackShipUserSetup ApplyValuesFrom(SOPickPackShipSetup setup)
		{
			DefaultLocationFromShipment = setup.DefaultLocationFromShipment;
			DefaultLotSerialFromShipment = setup.DefaultLotSerialFromShipment;
			PrintShipmentLabels = setup.PrintShipmentLabels;
			PrintCommercialInvoices = setup.PrintCommercialInvoices;
			PrintShipmentConfirmation = setup.PrintShipmentConfirmation;
			EnterSizeForPackages = setup.EnterSizeForPackages;
			return this;
		}
	}
}
