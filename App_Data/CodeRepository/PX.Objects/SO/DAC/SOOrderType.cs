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
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.SQLTree;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.SO
{
/*
1. Non-inventory orders cannot require location and cannot create shipments
2. Non-ar orders cannot have inventory doc type IN,DM,CM
*/
	[PXPrimaryGraph(typeof(SOOrderTypeMaint))]
	[PXCacheName(Messages.OrderType, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class SOOrderType : IBqlTable, PXNoteAttribute.IPXCopySettings
	{
		#region Keys
		public class PK: PrimaryKeyOf<SOOrderType>.By<orderType>
		{
			public static SOOrderType Find(PXGraph graph, string orderType, PKFindOptions options = PKFindOptions.None) => FindBy(graph, orderType, options);
		}
		public static class FK
		{
			public class Template : SOOrderType.PK.ForeignKeyOf<SOOrderType>.By<template> { }
			public class OrderPlanType : INPlanType.PK.ForeignKeyOf<SOOrderType>.By<orderPlanType> { }
			public class ShipmentPlanType : INPlanType.PK.ForeignKeyOf<SOOrderType>.By<shipmentPlanType> { }
			public class OrderNumbering : Numbering.PK.ForeignKeyOf<SOOrderType>.By<orderNumberingID> { }
			public class InvoiceNumbering : Numbering.PK.ForeignKeyOf<SOOrderType>.By<invoiceNumberingID> { }
			public class ManualInvoiceNumbering : Numbering.PK.ForeignKeyOf<SOOrderType>.By<userInvoiceNumbering> { }
			public class FreightAccount : Account.PK.ForeignKeyOf<SOOrderType>.By<freightAcctID> { }
			public class FreightSubaccount : Sub.PK.ForeignKeyOf<SOOrderType>.By<freightSubID> { }
			public class DiscountAccount : Account.PK.ForeignKeyOf<SOOrderType>.By<discountAcctID> { }
			public class DiscountSubaccount : Sub.PK.ForeignKeyOf<SOOrderType>.By<discountSubID> { }
			public class ShippedNotInvoicedAccount : Account.PK.ForeignKeyOf<SOOrderType>.By<shippedNotInvoicedAcctID> { }
			public class ShippedNotInvoicedSubaccount : Sub.PK.ForeignKeyOf<SOOrderType>.By<shippedNotInvoicedSubID> { }

		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class PreventEditIfChildOrderExists : PreventEditOf<active>.On<SOOrderTypeMaint>.IfExists<
				Select<SOOrderType, Where<SOOrderType.dfltChildOrderType.IsEqual<SOOrderType.orderType.FromCurrent>>>>
			{
				protected override String CreateEditPreventingReason(GetEditPreventingReasonArgs arg, Object firstPreventingEntity, String fieldName, String currentTableName, String foreignTableName)
				{
					var currentOrderType = arg.Row as SOOrderType;
					if (currentOrderType == null || (arg.NewValue as bool?) == true)
						return null;

					var parentOrderType = firstPreventingEntity as SOOrderType;
					return PXMessages.LocalizeFormatNoPrefix(Messages.TheOrderTypeIsDefaultChildOrderType, currentOrderType.OrderType, parentOrderType?.OrderType);
				}
			}
		}
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask=">aa")]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(Search2<SOOrderType.orderType, InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.orderType, Equal<SOOrderType.orderType>, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>,
			Where<SOOrderType.requireShipping, Equal<boolFalse>, And<SOOrderType.behavior, NotEqual<SOBehavior.bL>, Or<FeatureInstalled<FeaturesSet.inventory>>>>>))]
		[PXRestrictor(typeof(Where<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, Or<FeatureInstalled<FeaturesSet.warehouse>>>), null)]
		[PXRestrictor(typeof(Where<SOOrderType.requireAllocation, NotEqual<True>, Or<AllocationAllowed>>), null)]
		public virtual String OrderType
		{
			get { return this._OrderType; }
			set { this._OrderType = value; }
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected Boolean? _Active;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? Active
		{
			get { return this._Active; }
			set { this._Active = value; }
		}
		#endregion
		#region DaysToKeep
		public abstract class daysToKeep : PX.Data.BQL.BqlShort.Field<daysToKeep> { }
		protected Int16? _DaysToKeep;
		[PXDBShort]
		[PXDefault((short)30)]
		[PXUIField(DisplayName = "Days To Keep")]
		public virtual Int16? DaysToKeep
		{
			get { return this._DaysToKeep; }
			set { this._DaysToKeep = value; }
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get { return this._Descr; }
			set { this._Descr = value; }
		}
		#endregion		
		#region Template
		public abstract class template : PX.Data.BQL.BqlString.Field<template> { }
		protected String _Template;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Order Template", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SOOrderTypeT.orderType, Where2<Where<SOOrderTypeT.requireAllocation, NotEqual<True>
				, Or2<FeatureInstalled<FeaturesSet.warehouseLocation>
					, Or2<FeatureInstalled<FeaturesSet.lotSerialTracking>
						, Or2<FeatureInstalled<FeaturesSet.subItem>
							, Or2<FeatureInstalled<FeaturesSet.replenishment>
								, Or<FeatureInstalled<FeaturesSet.sOToPOLink>>
							>
						>
					>
				>>, And<Where<SOOrderTypeT.requireShipping, Equal<boolFalse>, Or<FeatureInstalled<FeaturesSet.inventory>>>>
			>
		>
			), DirtyRead = true, DescriptionField = typeof(SOOrderTypeT.descr))]
		public virtual String Template
		{
			get { return this._Template; }
			set { this._Template = value; }
		}
		#endregion
		#region IsSystem
		public abstract class isSystem : PX.Data.BQL.BqlBool.Field<isSystem> { }
		protected Boolean? _IsSystem;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is System Template", Enabled = false)]
		public virtual Boolean? IsSystem
		{
			get
			{
				return this._IsSystem;
			}
			set
			{
				this._IsSystem = value;
			}
		}
		#endregion
		#region Behavior
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		protected String _Behavior;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Automation Behavior", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[SOBehavior.List]
		public virtual String Behavior
		{
			get { return this._Behavior; }
			set { this._Behavior = value; }
		}
		#endregion
		#region DefaultOperation
		public abstract class defaultOperation : PX.Data.BQL.BqlString.Field<defaultOperation>
		{
			public const int Length = 1;
		}
		protected String _DefaultOperation;
		[PXDBString(defaultOperation.Length, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Default Operation")]
		[PXDefault(typeof(Search<SOOrderType.defaultOperation,
			Where<SOOrderType.orderType, Equal<Current<SOOrderType.behavior>>>>))]
		[SOOperation.List]
		public virtual String DefaultOperation
		{
			get { return this._DefaultOperation; }
			set { this._DefaultOperation = value; }
		}
		#endregion		
		#region INDocType
		public abstract class iNDocType : PX.Data.BQL.BqlString.Field<iNDocType> { }
		protected String _INDocType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[INTranType.SOList]
		[PXUIEnabled(typeof(Where<behavior.IsNotEqual<SOBehavior.tR>>))]
		[PXFormula(typeof(INTranType.transfer.When<behavior.IsEqual<SOBehavior.tR>>.Else<iNDocType>))]
		[PXUIField(DisplayName = "Inventory Transaction Type")]
		public virtual String INDocType
		{
			get { return this._INDocType; }
			set { this._INDocType = value; }
		}
		#endregion
		#region ARDocType
		public abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
		protected String _ARDocType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[ARDocType.SOFullList]
		[PXUIField(DisplayName = "AR Document Type")]
		public virtual String ARDocType
		{
			get { return this._ARDocType; }
			set { this._ARDocType = value; }
		}
		#endregion
		#region OrderPlanType
		public abstract class orderPlanType : PX.Data.BQL.BqlString.Field<orderPlanType> { }
		protected String _OrderPlanType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Order Plan Type")]
		[PXSelector(typeof(Search<INPlanType.planType>), DescriptionField = typeof(INPlanType.localizedDescr))]
		public virtual String OrderPlanType
		{
			get { return this._OrderPlanType; }
			set { this._OrderPlanType = value; }
		}
		#endregion
		#region ShipmentPlanType
		public abstract class shipmentPlanType : PX.Data.BQL.BqlString.Field<shipmentPlanType> { }
		protected String _ShipmentPlanType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Shipment Plan Type")]
		[PXSelector(typeof(Search<INPlanType.planType>), DescriptionField=typeof(INPlanType.localizedDescr))]
		public virtual String ShipmentPlanType
		{
			get { return this._ShipmentPlanType; }
			set { this._ShipmentPlanType = value; }
		}
		#endregion
		#region OrderNumberingID
		public abstract class orderNumberingID : PX.Data.BQL.BqlString.Field<orderNumberingID> { }
		protected String _OrderNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXSelector(typeof(Search<Numbering.numberingID>))]
		[PXUIField(DisplayName = "Order Numbering Sequence")]
		public virtual String OrderNumberingID
		{
			get { return this._OrderNumberingID; }
			set { this._OrderNumberingID = value; }
		}
		#endregion
		#region InvoiceNumberingID
		public abstract class invoiceNumberingID : PX.Data.BQL.BqlString.Field<invoiceNumberingID> { }
		protected String _InvoiceNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Search<Numbering.numberingID>))]
		[PXUIField(DisplayName = "Invoice Numbering Sequence")]
		public virtual String InvoiceNumberingID
		{
			get { return this._InvoiceNumberingID; }
			set { this._InvoiceNumberingID = value; }
		}
		#endregion
		#region UserInvoiceNumbering
		public abstract class userInvoiceNumbering : PX.Data.BQL.BqlBool.Field<userInvoiceNumbering> { }
		protected Boolean? _UserInvoiceNumbering;
		[PXBool]
		[PXFormula(typeof(Selector<SOOrderType.invoiceNumberingID, Numbering.userNumbering>))]
		[PXUIField(DisplayName = "Manual Invoice Numbering")]
		public virtual Boolean? UserInvoiceNumbering
		{
			get { return this._UserInvoiceNumbering; }
			set { this._UserInvoiceNumbering = value; }
		}
		#endregion
		#region MarkInvoicePrinted
		public abstract class markInvoicePrinted : PX.Data.BQL.BqlBool.Field<markInvoicePrinted> { }
		protected Boolean? _MarkInvoicePrinted;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Mark as Printed")]
		public virtual Boolean? MarkInvoicePrinted
		{
			get { return this._MarkInvoicePrinted; }
			set { this._MarkInvoicePrinted = value; }
		}
		#endregion
		#region MarkInvoiceEmailed
		public abstract class markInvoiceEmailed : PX.Data.BQL.BqlBool.Field<markInvoiceEmailed> { }
		protected Boolean? _MarkInvoiceEmailed;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Mark as Emailed")]
		public virtual Boolean? MarkInvoiceEmailed
		{
			get { return this._MarkInvoiceEmailed; }
			set { this._MarkInvoiceEmailed = value; }
		}
		#endregion
		#region HoldEntry
		public abstract class holdEntry : PX.Data.BQL.BqlBool.Field<holdEntry> { }
		protected Boolean? _HoldEntry;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hold Orders on Entry")]
		public virtual Boolean? HoldEntry
		{
			get { return this._HoldEntry; }
			set { this._HoldEntry = value; }
		}
		#endregion
		#region InvoiceHoldEntry
		public abstract class invoiceHoldEntry : PX.Data.BQL.BqlBool.Field<invoiceHoldEntry> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIEnabled(typeof(Where<behavior.IsNotEqual<SOBehavior.tR>>))]
		[PXFormula(typeof(False.When<behavior.IsEqual<SOBehavior.tR>>.Else<invoiceHoldEntry>))]
		[PXUIField(DisplayName = "Hold Invoices on Entry")]
		public virtual Boolean? InvoiceHoldEntry { get; set; }
		#endregion
		#region CreditHoldEntry
		public abstract class creditHoldEntry : PX.Data.BQL.BqlBool.Field<creditHoldEntry> { }
		protected Boolean? _CreditHoldEntry;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIEnabled(typeof(Where<behavior.IsIn<SOBehavior.sO, SOBehavior.iN, SOBehavior.rM, SOBehavior.mO>>))]
		[PXFormula(typeof(False.When<behavior.IsNotIn<SOBehavior.sO, SOBehavior.iN, SOBehavior.rM, SOBehavior.mO>>.Else<creditHoldEntry>))]
		[PXUIField(DisplayName = "Hold Document on Failed Credit Check")]
		public virtual Boolean? CreditHoldEntry
		{
			get { return this._CreditHoldEntry; }
			set { this._CreditHoldEntry = value; }
		}
		#endregion
		#region UseCuryRateFromSO
		public abstract class useCuryRateFromSO : Data.BQL.BqlBool.Field<useCuryRateFromSO> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Currency Rate from Sales Order", FieldClass = nameof(FeaturesSet.Multicurrency))]
		public virtual bool? UseCuryRateFromSO
		{
			get;
			set;
		}
		#endregion
		#region UseCuryRateFromBL
		public abstract class useCuryRateFromBL : Data.BQL.BqlBool.Field<useCuryRateFromBL> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Currency Rate from Blanket Sales Order", FieldClass = nameof(FeaturesSet.Multicurrency))]
		[PXUIVisible(typeof(Where<SOOrderType.behavior.IsEqual<SOBehavior.bL>>))]
		public virtual bool? UseCuryRateFromBL
		{
			get;
			set;
		}
		#endregion

		#region RequireAllocation
		public abstract class requireAllocation : PX.Data.BQL.BqlBool.Field<requireAllocation> { }
		protected Boolean? _RequireAllocation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Stock Allocation")]
		public virtual Boolean? RequireAllocation
		{
			get { return this._RequireAllocation; }
			set { this._RequireAllocation = value; }
		}
		#endregion		
		#region RequireLocation
		public abstract class requireLocation : PX.Data.BQL.BqlBool.Field<requireLocation> { }
		[PXBool]
		[PXDBCalced(typeof(IIf<Where<requireShipping, NotEqual<True>, And<iNDocType, NotEqual<INTranType.noUpdate>>>, True, False>), typeof(bool))]
		[PXUIField(DisplayName = "Require Location", Enabled = false)]
		public virtual Boolean? RequireLocation
		{
			get;
			set;
		}
		#endregion
		#region RequireLotSerial
		public abstract class requireLotSerial : PX.Data.BQL.BqlBool.Field<requireLotSerial> { }
		protected Boolean? _RequireLotSerial;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Lot/Serial Entry")]
		public virtual Boolean? RequireLotSerial
		{
			get { return this._RequireLotSerial; }
			set { this._RequireLotSerial = value; }
		}
		#endregion
		#region AllowQuickProcess
		public abstract class allowQuickProcess : PX.Data.BQL.BqlBool.Field<allowQuickProcess> { }
		protected Boolean? _AllowQuickProcess;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Quick Process")]
		[PXUIVisible(typeof(Where<behavior, In3<SOBehavior.sO, SOBehavior.tR, SOBehavior.iN, SOBehavior.cM, SOBehavior.mO>>))]
		public virtual Boolean? AllowQuickProcess
		{
			get { return this._AllowQuickProcess; }
			set { this._AllowQuickProcess = value; }
		}
		#endregion

		#region RequireControlTotal
		public abstract class requireControlTotal : PX.Data.BQL.BqlBool.Field<requireControlTotal> { }
		protected Boolean? _RequireControlTotal;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Control Total")]
		public virtual Boolean? RequireControlTotal
		{
			get { return this._RequireControlTotal; }
			set { this._RequireControlTotal = value; }
		}
		#endregion
		#region RequireShipping
		public abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }
		protected Boolean? _RequireShipping;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Process Shipments")]
		public virtual Boolean? RequireShipping
		{
			get
			{
				return SOBehavior.GetRequireShipmentValue(this.Behavior, this._RequireShipping);
			}
			set
			{
				this._RequireShipping = SOBehavior.GetRequireShipmentValue(this.Behavior, value);
			}
		}
		#endregion
		#region CopyLotSerialFromShipment
		public abstract class copyLotSerialFromShipment : PX.Data.BQL.BqlBool.Field<copyLotSerialFromShipment> { }
		protected Boolean? _CopyLotSerialFromShipment;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Lot/Serial numbers from Shipment back to Sales Order")]
		public virtual Boolean? CopyLotSerialFromShipment
		{
			get { return this._CopyLotSerialFromShipment; }
			set { this._CopyLotSerialFromShipment = value; }
		}
		#endregion

		#region BillSeparately
		public abstract class billSeparately : PX.Data.BQL.BqlBool.Field<billSeparately> { }
		protected Boolean? _BillSeparately;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bill Separately")]
		public virtual Boolean? BillSeparately
		{
			get { return this._BillSeparately; }
			set { this._BillSeparately = value; }
		}
		#endregion
		#region ShipSeparately
		public abstract class shipSeparately : PX.Data.BQL.BqlBool.Field<shipSeparately> { }
		protected Boolean? _ShipSeparately;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Ship Separately")]
		public virtual Boolean? ShipSeparately
		{
			get { return this._ShipSeparately; }
			set { this._ShipSeparately = value; }
		}
		#endregion
		#region SalesAcctDefault
		public abstract class salesAcctDefault : PX.Data.BQL.BqlString.Field<salesAcctDefault> { }
		protected String _SalesAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Sales Account from")]
		[SOSalesAcctSubDefault.AcctList]
		[PXDefault(SOSalesAcctSubDefault.MaskItem)]
		public virtual String SalesAcctDefault
		{
			get { return this._SalesAcctDefault; }
			set { this._SalesAcctDefault = value; }
		}
		#endregion
		#region MiscAcctDefault
		public abstract class miscAcctDefault : PX.Data.BQL.BqlString.Field<miscAcctDefault> { }
		protected String _MiscAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Misc. Account from")]
		[SOMiscAcctSubDefault.AcctList]
		[PXDefault(SOMiscAcctSubDefault.MaskItem)]
		public virtual String MiscAcctDefault
		{
			get { return this._MiscAcctDefault; }
			set { this._MiscAcctDefault = value; }
		}
		#endregion
		#region FreightAcctDefault
		public abstract class freightAcctDefault : PX.Data.BQL.BqlString.Field<freightAcctDefault> { }
		protected String _FreightAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Freight Account from")]
		[SOFreightAcctSubDefault.AcctList]
		[PXDefault(SOFreightAcctSubDefault.MaskShipVia)]
		public virtual String FreightAcctDefault
		{
			get { return this._FreightAcctDefault; }
			set { this._FreightAcctDefault = value; }
		}
		#endregion
		#region DiscAcctDefault
		public abstract class discAcctDefault : PX.Data.BQL.BqlString.Field<discAcctDefault> { }
		protected String _DiscAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Discount Account from")]
		[SODiscAcctSubDefault.AcctList]
		[PXDefault(SODiscAcctSubDefault.MaskLocation)]
		public virtual String DiscAcctDefault
		{
			get { return this._DiscAcctDefault; }
			set { this._DiscAcctDefault = value; }
		}
		#endregion
		#region COGSAcctDefault
		public abstract class cOGSAcctDefault : PX.Data.BQL.BqlString.Field<cOGSAcctDefault> { }
		protected String _COGSAcctDefault;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use COGS Account from", Visible = false, Enabled = false)]
		[SOCOGSAcctSubDefault.AcctList]
		//[PXDefault(SOCOGSAcctSubDefault.MaskItem)]
		public virtual String COGSAcctDefault
		{
			get { return this._COGSAcctDefault; }
			set { this._COGSAcctDefault = value; }
		}
		#endregion
		#region SalesSubMask
		public abstract class salesSubMask : PX.Data.BQL.BqlString.Field<salesSubMask> { }
		protected String _SalesSubMask;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>>))]
		[SOSalesSubAccountMask(DisplayName = "Combine Sales Sub. From")]
		public virtual String SalesSubMask
		{
			get { return this._SalesSubMask; }
			set { this._SalesSubMask = value; }
		}
		#endregion
		#region MiscSubMask
		public abstract class miscSubMask : PX.Data.BQL.BqlString.Field<miscSubMask> { }
		protected String _MiscSubMask;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>>))]
		[SOMiscSubAccountMask(DisplayName = "Combine Misc. Sub. from")]
		public virtual String MiscSubMask
		{
			get { return this._MiscSubMask; }
			set { this._MiscSubMask = value; }
		}
		#endregion
		#region FreightSubMask
		public abstract class freightSubMask : PX.Data.BQL.BqlString.Field<freightSubMask> { }
		protected String _FreightSubMask;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>>))]
		[SOFreightSubAccountMask(DisplayName = "Combine Freight Sub. from")]
		public virtual String FreightSubMask
		{
			get { return this._FreightSubMask; }
			set { this._FreightSubMask = value; }
		}
		#endregion
		#region DiscSubMask
		public abstract class discSubMask : PX.Data.BQL.BqlString.Field<discSubMask> { }
		protected String _DiscSubMask;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>, And<FeatureInstalled<FeaturesSet.customerDiscounts>>>))]
		[SODiscSubAccountMask(DisplayName = "Combine Discount Sub. from")]
		public virtual String DiscSubMask
		{
			get { return this._DiscSubMask; }
			set { this._DiscSubMask = value; }
		}
		#endregion
		#region FreightAcctID
		public abstract class freightAcctID : PX.Data.BQL.BqlInt.Field<freightAcctID> { }
		protected Int32? _FreightAcctID;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>, And<behavior, NotEqual<SOBehavior.bL>>>))]
		[Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<SOOrderType.freightAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? FreightAcctID
		{
			get { return this._FreightAcctID; }
			set { this._FreightAcctID = value; }
		}
		#endregion
		#region FreightSubID
		public abstract class freightSubID : PX.Data.BQL.BqlInt.Field<freightSubID> { }
		protected Int32? _FreightSubID;
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>, And<behavior, NotEqual<SOBehavior.bL>>>))]
		[SubAccount(typeof(SOOrderType.freightAcctID), DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<SOOrderType.freightSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? FreightSubID
		{
			get { return this._FreightSubID; }
			set { this._FreightSubID = value; }
		}
		#endregion
		#region UseShippedNotInvoiced
		public abstract class useShippedNotInvoiced : PX.Data.BQL.BqlBool.Field<useShippedNotInvoiced> { }
		protected Boolean? _UseShippedNotInvoiced;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Shipped-Not-Invoiced Account")]
		public virtual Boolean? UseShippedNotInvoiced
		{
			get
			{
				return this._UseShippedNotInvoiced;
			}
			set
			{
				this._UseShippedNotInvoiced = value;
			}
		}
		#endregion
		#region ShippedNotInvoicedAcctID
		public abstract class shippedNotInvoicedAcctID : PX.Data.BQL.BqlInt.Field<shippedNotInvoicedAcctID> { }
		protected Int32? _ShippedNotInvoicedAcctID;
		[GL.Account(DisplayName = "Shipped-Not-Invoiced Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(GL.Account.description), ControlAccountForModule = GL.ControlAccountModule.SO)]
		[PXForeignReference(typeof(FK.ShippedNotInvoicedAccount))]
		public virtual Int32? ShippedNotInvoicedAcctID
		{
			get
			{
				return this._ShippedNotInvoicedAcctID;
			}
			set
			{
				this._ShippedNotInvoicedAcctID = value;
			}
		}
		#endregion
		#region ShippedNotInvoicedSubID
		public abstract class shippedNotInvoicedSubID : PX.Data.BQL.BqlInt.Field<shippedNotInvoicedSubID> { }
		protected Int32? _ShippedNotInvoicedSubID;
		[GL.SubAccount(typeof(SOOrderType.shippedNotInvoicedAcctID), DisplayName = "Shipped-Not-Invoiced Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(GL.Sub.description))]
		[PXForeignReference(typeof(Field<SOOrderType.shippedNotInvoicedSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? ShippedNotInvoicedSubID
		{
			get
			{
				return this._ShippedNotInvoicedSubID;
			}
			set
			{
				this._ShippedNotInvoicedSubID = value;
			}
		}
		#endregion
		#region DisableAutomaticDiscountCalculation
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Automatic Discount Update")]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion
		#region DeferPriceDiscountRecalculation
		public abstract class deferPriceDiscountRecalculation : PX.Data.BQL.BqlBool.Field<deferPriceDiscountRecalculation> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Defer Discount Recalculation")]
		public virtual Boolean? DeferPriceDiscountRecalculation
		{
			get; set;
		} 
		#endregion
		#region RecalculateDiscOnPartialShipment
		public abstract class recalculateDiscOnPartialShipment : PX.Data.BQL.BqlBool.Field<recalculateDiscOnPartialShipment> { }
		protected bool? _RecalculateDiscOnPartialShipment;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Recalculate Discount On Partial Shipment")]
		public virtual bool? RecalculateDiscOnPartialShipment
		{
			get { return _RecalculateDiscOnPartialShipment; }
			set { _RecalculateDiscOnPartialShipment = value; }
		}
		#endregion
		#region DisableAutomaticTaxCalculation
		public abstract class disableAutomaticTaxCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticTaxCalculation> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Automatic Tax Calculation")]
		public virtual Boolean? DisableAutomaticTaxCalculation
		{
			get;
			set;
		}
		#endregion
		#region ShipFullIfNegQtyAllowed
		public abstract class shipFullIfNegQtyAllowed : PX.Data.BQL.BqlBool.Field<shipFullIfNegQtyAllowed>
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Ship in Full if Negative Quantity Is Allowed")]
		public virtual bool? ShipFullIfNegQtyAllowed
		{
			get;
			set;
		}
		#endregion
		#region CalculateFreight
		public abstract class calculateFreight : PX.Data.BQL.BqlBool.Field<calculateFreight> { }
		protected bool? _CalculateFreight;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Calculate Freight")]
		public virtual bool? CalculateFreight
		{
			get { return _CalculateFreight; }
			set { _CalculateFreight = value; }
		}
		#endregion
		

		#region COGSSubMask
		public abstract class cOGSSubMask : PX.Data.BQL.BqlString.Field<cOGSSubMask> { }
		protected String _COGSSubMask;
		//[PXDefault()]
		[SOCOGSSubAccountMask(DisplayName = "Combine COGS Sub. From", Visible = false, Enabled = false)]
		public virtual String COGSSubMask
		{
			get { return this._COGSSubMask; }
			set { this._COGSSubMask = value; }
		}
		#endregion
		#region DiscountAcctID
		public abstract class discountAcctID : PX.Data.BQL.BqlInt.Field<discountAcctID> { }
		protected Int32? _DiscountAcctID;
		[Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>, And<behavior, NotEqual<SOBehavior.bL>, And<FeatureInstalled<FeaturesSet.customerDiscounts>>>>))]
		[PXForeignReference(typeof(Field<SOOrderType.discountAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? DiscountAcctID
		{
			get { return this._DiscountAcctID; }
			set { this._DiscountAcctID = value; }
		}
		#endregion
		#region DiscountSubID
		public abstract class discountSubID : PX.Data.BQL.BqlInt.Field<discountSubID> { }
		protected Int32? _DiscountSubID;
		[SubAccount(typeof(SOOrderType.discountAcctID), DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault]
		[PXUIRequired(typeof(Where<active, Equal<True>, And<behavior, NotEqual<SOBehavior.bL>, And2<FeatureInstalled<FeaturesSet.customerDiscounts>, And<FeatureInstalled<FeaturesSet.subAccount>>>>>))]
		[PXForeignReference(typeof(Field<SOOrderType.discountSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? DiscountSubID
		{
			get { return this._DiscountSubID; }
			set { this._DiscountSubID = value; }
		}
		#endregion
		#region OrderPriority
		public abstract class orderPriority : PX.Data.BQL.BqlShort.Field<orderPriority> { }
		protected Int16? _OrderPriority;
		[PXDBShort]
		public virtual Int16? OrderPriority
		{
			get { return this._OrderPriority; }
			set { this._OrderPriority = value; }
		}
		#endregion
		#region CopyNotes
		public abstract class copyNotes : PX.Data.BQL.BqlBool.Field<copyNotes> { }
		protected bool? _CopyNotes;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Notes")]
		public virtual bool? CopyNotes
		{
			get { return _CopyNotes; }
			set { _CopyNotes = value; }
		}
		#endregion
		#region CopyFiles
		public abstract class copyFiles : PX.Data.BQL.BqlBool.Field<copyFiles> { }
		protected bool? _CopyFiles;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Attachments")]
		public virtual bool? CopyFiles
		{
			get { return _CopyFiles; }
			set { _CopyFiles = value; }
		}
		#endregion

		#region CopyHeaderNotesToShipment
		public abstract class copyHeaderNotesToShipment : PX.Data.BQL.BqlBool.Field<copyHeaderNotesToShipment> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Header Notes to Shipment")]
		public virtual bool? CopyHeaderNotesToShipment
		{
			get;
			set;
		}
		#endregion
		#region CopyHeaderFilesToShipment
		public abstract class copyHeaderFilesToShipment : PX.Data.BQL.BqlBool.Field<copyHeaderFilesToShipment> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Header Attachments to Shipment")]
		public virtual bool? CopyHeaderFilesToShipment
		{
			get;
			set;
		}
		#endregion
		#region CopyHeaderNotesToInvoice
		public abstract class copyHeaderNotesToInvoice : PX.Data.BQL.BqlBool.Field<copyHeaderNotesToInvoice> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Header Notes to Invoice")]
		public virtual bool? CopyHeaderNotesToInvoice
		{
			get;
			set;
		}
		#endregion
		#region CopyHeaderFilesToInvoice
		public abstract class copyHeaderFilesToInvoice : PX.Data.BQL.BqlBool.Field<copyHeaderFilesToInvoice> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Header Attachments to Invoice")]
		public virtual bool? CopyHeaderFilesToInvoice
		{
			get; 
			set;
		}
		#endregion

		#region CopyLineNotesToShipment
		public abstract class copyLineNotesToShipment : PX.Data.BQL.BqlBool.Field<copyLineNotesToShipment> { }
		protected bool? _CopyLineNotesToShipment;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Notes To Shipment")]
		public virtual bool? CopyLineNotesToShipment
		{
			get { return _CopyLineNotesToShipment; }
			set { _CopyLineNotesToShipment = value; }
		}
		#endregion
		#region CopyLineFilesToShipment
		public abstract class copyLineFilesToShipment : PX.Data.BQL.BqlBool.Field<copyLineFilesToShipment> { }
		protected bool? _CopyLineFilesToShipment;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Attachments To Shipment")]
		public virtual bool? CopyLineFilesToShipment
		{
			get { return _CopyLineFilesToShipment; }
			set { _CopyLineFilesToShipment = value; }
		}
		#endregion
		#region CopyLineNotesToInvoice
		public abstract class copyLineNotesToInvoice : PX.Data.BQL.BqlBool.Field<copyLineNotesToInvoice> { }
		protected bool? _CopyLineNotesToInvoice;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Notes To Invoice")]
		public virtual bool? CopyLineNotesToInvoice
		{
			get { return _CopyLineNotesToInvoice; }
			set { _CopyLineNotesToInvoice = value; }
		}
		#endregion
		#region CopyLineNotesToInvoiceOnlyNS
		public abstract class copyLineNotesToInvoiceOnlyNS : PX.Data.BQL.BqlBool.Field<copyLineNotesToInvoiceOnlyNS> { }
		protected bool? _CopyLineNotesToInvoiceOnlyNS;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Only Non-Stock")]
		public virtual bool? CopyLineNotesToInvoiceOnlyNS
		{
			get { return _CopyLineNotesToInvoiceOnlyNS; }
			set { _CopyLineNotesToInvoiceOnlyNS = value; }
		}
		#endregion
		#region CopyLineFilesToInvoice
		public abstract class copyLineFilesToInvoice : PX.Data.BQL.BqlBool.Field<copyLineFilesToInvoice> { }
		protected bool? _CopyLineFilesToInvoice;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Attachments To Invoice")]
		public virtual bool? CopyLineFilesToInvoice
		{
			get { return _CopyLineFilesToInvoice; }
			set { _CopyLineFilesToInvoice = value; }
		}
		#endregion
		#region CopyLineFilesToInvoiceOnlyNS
		public abstract class copyLineFilesToInvoiceOnlyNS : PX.Data.BQL.BqlBool.Field<copyLineFilesToInvoiceOnlyNS> { }
		protected bool? _CopyLineFilesToInvoiceOnlyNS;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Only Non-Stock")]
		public virtual bool? CopyLineFilesToInvoiceOnlyNS
		{
			get { return _CopyLineFilesToInvoiceOnlyNS; }
			set { _CopyLineFilesToInvoiceOnlyNS = value; }
		}
		#endregion
		#region CopyLineNotesToChildOrder
		public abstract class copyLineNotesToChildOrder : PX.Data.BQL.BqlBool.Field<copyLineNotesToChildOrder> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Notes To Child Order")]
		public virtual bool? CopyLineNotesToChildOrder
		{
			get;
			set;
		}
		#endregion
		#region CopyLineFilesToChildOrder
		public abstract class copyLineFilesToChildOrder : PX.Data.BQL.BqlBool.Field<copyLineFilesToChildOrder> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Line Attachments To Child Order")]
		public virtual bool? CopyLineFilesToChildOrder
		{
			get;
			set;
		}
		#endregion
		#region CustomerOrderIsRequired
		public abstract class customerOrderIsRequired : PX.Data.BQL.BqlBool.Field<customerOrderIsRequired> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Customer Order Nbr.")]
		public virtual bool? CustomerOrderIsRequired
		{
			get;
			set;
		}
		#endregion
		#region CustomerOrderValidation
		public abstract class customerOrderValidation : PX.Data.BQL.BqlString.Field<customerOrderValidation> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Customer Order Nbr. Validation")]
		[CustomerOrderValidationType.List]
		[PXDefault(CustomerOrderValidationType.None)]
		public virtual String CustomerOrderValidation
		{
			get;
			set;
		}
		#endregion

		#region PostLineDiscSeparately
		public abstract class postLineDiscSeparately : PX.Data.BQL.BqlBool.Field<postLineDiscSeparately> { }
		protected bool? _PostLineDiscSeparately;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Post Line Discounts Separately")]
		public virtual bool? PostLineDiscSeparately
		{
			get { return _PostLineDiscSeparately; }
			set { _PostLineDiscSeparately = value; }
		}
		#endregion
		#region UseDiscountSubFromSalesSub
		public abstract class useDiscountSubFromSalesSub : PX.Data.BQL.BqlBool.Field<useDiscountSubFromSalesSub> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Discount Sub. from Sales Sub.", FieldClass = SubAccountAttribute.DimensionName)]
		public virtual bool? UseDiscountSubFromSalesSub { get; set; }
		#endregion
		#region AutoWriteOff
		public abstract class autoWriteOff : PX.Data.BQL.BqlBool.Field<autoWriteOff> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Auto Write-Off")]
		public virtual bool? AutoWriteOff { get; set; }
		#endregion

		#region IntercompanySalesAcctDefault
		public abstract class intercompanySalesAcctDefault : PX.Data.BQL.BqlString.Field<intercompanySalesAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Sales Account from", FieldClass = nameof(FeaturesSet.InterBranch))]
		[SOIntercompanyAcctDefault.AcctSalesList]
		[PXDefault(SOIntercompanyAcctDefault.MaskItem)]
		public virtual String IntercompanySalesAcctDefault { get; set; }
		#endregion
		#region IntercompanyCOGSAcctDefault
		public abstract class intercompanyCOGSAcctDefault : PX.Data.BQL.BqlString.Field<intercompanyCOGSAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use COGS Account from", FieldClass = nameof(FeaturesSet.InterBranch))]
		[SOIntercompanyAcctDefault.AcctCOGSList]
		[PXDefault(SOIntercompanyAcctDefault.MaskItem)]
		public virtual String IntercompanyCOGSAcctDefault { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(SOOrderType.orderType),
			Selector = typeof(SOOrderType.orderType), 
			FieldList = new [] { typeof(SOOrderType.orderType), typeof(SOOrderType.descr) })]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get { return this._CreatedByID; }
			set { this._CreatedByID = value; }
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID
		{
			get { return this._CreatedByScreenID; }
			set { this._CreatedByScreenID = value; }
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get { return this._CreatedDateTime; }
			set { this._CreatedDateTime = value; }
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get { return this._LastModifiedByID; }
			set { this._LastModifiedByID = value; }
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
		{
			get { return this._LastModifiedByScreenID; }
			set { this._LastModifiedByScreenID = value; }
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get { return this._LastModifiedDateTime; }
			set { this._LastModifiedDateTime = value; }
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp
		{
			get { return this._tstamp; }
			set { this._tstamp = value; }
		}
		#endregion

		#region ActiveOperationsCntr
		public abstract class activeOperationsCntr : Data.BQL.BqlInt.Field<activeOperationsCntr> { }
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? ActiveOperationsCntr
		{
			get;
			set;
		}
		#endregion
		#region AllowRefundBeforeReturn
		public abstract class allowRefundBeforeReturn : Data.BQL.BqlBool.Field<allowRefundBeforeReturn> { }
		[PXDBBool]
		[PXDefault(typeof(IIf<
			Where<behavior, In3<SOBehavior.rM, SOBehavior.cM>,
				And<aRDocType, NotEqual<ARDocType.noUpdate>,
				And<defaultOperation, Equal<SOOperation.receipt>,
				And<activeOperationsCntr, Equal<int1>,
				Or<behavior, Equal<SOBehavior.mO>>>>>>, True, False>))]
		[PXUIField(DisplayName = "Allow Refund Before Return")]
		public virtual bool? AllowRefundBeforeReturn
		{
			get;
			set;
		}
		#endregion

		#region CanHavePayments
		public abstract class canHavePayments : Data.BQL.BqlBool.Field<canHavePayments> { }
		[PXDBBool]
		[PXFormula(typeof(IIf<Where<aRDocType, In3<ARDocType.invoice, ARDocType.debitMemo>,
			Or<behavior, In3<SOBehavior.bL, SOBehavior.mO>>>, True, False>))]
		public virtual bool? CanHavePayments
		{
			get;
			set;
		}
		#endregion
		#region CanHaveRefunds
		public abstract class canHaveRefunds : Data.BQL.BqlBool.Field<canHaveRefunds> { }
		[PXDBBool]
		[PXFormula(typeof(IIf<
			Where<aRDocType, Equal<ARDocType.creditMemo>,
				And<defaultOperation, Equal<SOOperation.receipt>,
				And<activeOperationsCntr, Equal<int1>,
				Or<behavior, Equal<SOBehavior.mO>>>>>, True, False>))]
		public virtual bool? CanHaveRefunds
		{
			get;
			set;
		}
		#endregion
		#region ValidateCCRefundsOrigTransactions
		public abstract class validateCCRefundsOrigTransactions : PX.Data.BQL.BqlBool.Field<validateCCRefundsOrigTransactions> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIVisible(typeof(Where<canHaveRefunds, Equal<True>>))]
		[PXUIField(DisplayName = "Validate Card Refunds Against Original Transactions")]
		public virtual bool? ValidateCCRefundsOrigTransactions
		{
			get;
			set;
		}
		#endregion
		#region DfltChildOrderType
		public abstract class dfltChildOrderType : Data.BQL.BqlString.Field<dfltChildOrderType> { }
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXUIField(DisplayName = "Default Child Order Type")]
		[PXUIRequired(typeof(Where<active, Equal<True>, And<behavior, Equal<SOBehavior.bL>>>))]
		[PXSelector(typeof(Search<orderType, Where<behavior, Equal<SOBehavior.sO>>>))]
		[PXRestrictor(typeof(Where<active, Equal<True>>), Messages.OrderTypeInactive, ShowWarning = true)]
		public virtual string DfltChildOrderType
		{
			get;
			set;
		}
		#endregion
	}

	[PXProjection(typeof(Select<SOOrderType>))]
	[Serializable]
	[PXHidden]
	public partial class SOOrderTypeT : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOOrderTypeT>.By<orderType>
		{
			public static SOOrderTypeT Find(PXGraph graph, string orderType, PKFindOptions options = PKFindOptions.None) => FindBy(graph, orderType, options);
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask = ">aa", BqlField = typeof(SOOrderType.orderType))]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(Search<SOOrderType.orderType>))]
		public virtual String OrderType
		{
			get { return this._OrderType; }
			set { this._OrderType = value; }
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(60, IsUnicode = true, BqlField = typeof(SOOrderType.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get { return this._Descr; }
			set { this._Descr = value; }
		}
		#endregion
		#region Behavior
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		protected String _Behavior;
		[PXDBString(2, IsFixed = true, InputMask = ">aa", BqlField = typeof(SOOrderType.behavior))]
		[PXUIField(DisplayName = "Automation Behavior", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[SOBehavior.List]
		public virtual String Behavior
		{
			get { return this._Behavior; }
			set { this._Behavior = value; }
		}
		#endregion
		#region RequireAllocation
		public abstract class requireAllocation : PX.Data.BQL.BqlBool.Field<requireAllocation> { }
		protected Boolean? _RequireAllocation;
		[PXDBBool(BqlField = typeof(SOOrderType.requireAllocation))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Stock Allocation")]
		public virtual Boolean? RequireAllocation
		{
			get { return this._RequireAllocation; }
			set { this._RequireAllocation = value; }
		}
		#endregion
		#region RequireShipping
		public abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }
		protected Boolean? _RequireShipping;
		[PXDBBool(BqlField = typeof(SOOrderType.requireShipping))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Process Shipments")]
		public virtual Boolean? RequireShipping
		{
			get { return this._RequireShipping; }
			set { this._RequireShipping = value; }
		}
		#endregion
	}

	public class SOAutomation
	{
		public const string Behavior = "Behavior";
		public const string GraphType = "PX.Objects.SO.SOOrderEntry";

		public class behavior : PX.Data.BQL.BqlString.Constant<behavior>
		{
			public behavior() : base(Behavior) { }
		}
	}

	public class SOBehavior
	{
		public const string SO = "SO";
		public const string TR = "TR";
		public const string IN = "IN";
		public const string QT = "QT";
		public const string RM = "RM";
		public const string CM = "CM";
		public const string BL = "BL";
		public const string MO = "MO";

		public class sO : PX.Data.BQL.BqlString.Constant<sO> { public sO() : base(SO) { } }
		public class tR : PX.Data.BQL.BqlString.Constant<tR> { public tR() : base(TR) { } }
		public class iN : PX.Data.BQL.BqlString.Constant<iN> { public iN() : base(IN) { } }
		public class qT : PX.Data.BQL.BqlString.Constant<qT> { public qT() : base(QT) { } }
		public class rM : PX.Data.BQL.BqlString.Constant<rM> { public rM() : base(RM) { } }
		public class cM : PX.Data.BQL.BqlString.Constant<cM> { public cM() : base(CM) { } }
		public class bL : PX.Data.BQL.BqlString.Constant<bL> { public bL() : base(BL) { } }
		public class mO : PX.Data.BQL.BqlString.Constant<mO> { public mO() : base(MO) { } }

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				Pair(SO, Messages.SOName),
				Pair(TR, Messages.TRName),
				Pair(IN, Messages.INName),
				Pair(QT, Messages.QTName),
				Pair(CM, Messages.CMName),
				Pair(RM, Messages.RMName),
				Pair(BL, Messages.BLName),
				Pair(MO, Messages.MOName))
			{ }
		}

		public static bool? GetRequireShipmentValue(string behavior, bool? value)
		{
			if (behavior.IsIn(TR, SO, RM))
				return true;
			if (behavior.IsIn(IN, QT, CM, BL, MO))
				return false;
			return value;
		}

		public class RequireShipment<TSOBehavior, TSOTypeRequireShipment> : IBqlUnary
			where TSOBehavior : IBqlOperand
			where TSOTypeRequireShipment : IBqlOperand
		{
			private readonly IBqlCreator requireShipment =
				new Where<TSOBehavior, Equal<SOBehavior.tR>,
				Or<TSOBehavior, Equal<SOBehavior.sO>,
				Or<TSOBehavior, Equal<SOBehavior.rM>,
				Or<TSOBehavior, NotEqual<SOBehavior.iN>,
				And<TSOBehavior, NotEqual<SOBehavior.qT>,
				And<TSOBehavior, NotEqual<SOBehavior.cM>,
				And<TSOBehavior, NotEqual<SOBehavior.bL>,
				And<TSOBehavior, NotEqual<SOBehavior.mO>,
				And<TSOTypeRequireShipment, Equal<True>>>>>>>>>>();

			public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
				=> requireShipment.AppendExpression(ref exp, graph, info, selection);

			public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value) =>
				requireShipment.Verify(cache, item, pars, ref result, ref value);
		}

		public static bool IsPredefinedBehavior(string behavior)
		{
			return behavior.IsIn(IN, QT, CM, SO, TR, RM, BL, MO);
		}

		public static string DefaultOperation(string behavior, string ardoctype)
		{
			switch (behavior)
			{
				case SO:
				case TR:
				case IN:
					switch (ardoctype)
					{
						case ARDocType.Invoice:
						case ARDocType.DebitMemo:
						case ARDocType.CashSale:
						case ARDocType.NoUpdate:
							return SOOperation.Issue;
						case ARDocType.CreditMemo:
						case ARDocType.CashReturn:
							return SOOperation.Receipt;
						default:
							return null;
					}
				case QT:
				case BL:
				case MO:
					return SOOperation.Issue;
				case CM:
				case RM:
					return SOOperation.Receipt;
			}
			return null;
		}
	}

	public class AllocationAllowed : WhereBase<FeatureInstalled<FeaturesSet.warehouseLocation>,
			Or2<FeatureInstalled<FeaturesSet.lotSerialTracking>,
				Or2<FeatureInstalled<FeaturesSet.subItem>,
					Or2<FeatureInstalled<FeaturesSet.replenishment>,
						Or<FeatureInstalled<FeaturesSet.sOToPOLink>>>>>>
	{
		public override bool UseParenthesis() { return true; }
	}

	public static class CustomerOrderValidationType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				Pair(None, Messages.CustOrdeNoValidation),
				Pair(Warn, Messages.CustOrderWarnOnDuplicates),
				Pair(Error, Messages.CustOrderErrorOnDuplicates))
			{ }
		}

		public const string None = "N";
		public const string Warn = "W";
		public const string Error = "E";
	}
}
