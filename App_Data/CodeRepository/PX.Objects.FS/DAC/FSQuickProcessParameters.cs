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

using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using System;

namespace PX.Objects.FS
{
    using TrueCondition = Where<True, Equal<True>>;
    using ServiceOrderRequiresAllowBilling = Where<Current<FSServiceOrder.allowInvoice>, Equal<False>>;
    using ServiceOrderCanBeCompleted = Where<Current<FSServiceOrder.completed>, Equal<False>, And<Current<FSServiceOrder.openDoc>, Equal<True>>>;
    using ServiceOrderCanBeClosed = Where<Current<FSServiceOrder.closed>, Equal<False>, And<Where<Current<FSServiceOrder.completed>, Equal<True>, Or<Current<FSServiceOrder.openDoc>, Equal<True>>>>>;
    using ServiceOrderCanBeInvoiced = Where<Current<FSServiceOrder.allowInvoice>, Equal<True>, And2<Where<Current<FSServiceOrder.completed>, Equal<True>>, Or<FSServiceOrder.closed, Equal<True>>>>;
    using OrderTypeRequiresShipping = Where<Current<SOOrderTypeQuickProcess.behavior>, In3<SOBehavior.sO, SOBehavior.tR>>;
    using OrderTypeRequiresInvoicing = Where<Current<SOOrderTypeQuickProcess.behavior>, In3<SOBehavior.sO, SOBehavior.iN, SOBehavior.cM>, And<Current<SOOrderTypeQuickProcess.aRDocType>, NotEqual<AR.ARDocType.noUpdate>>>;
    using OrderTypePostToSOInvoice = Where<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Sales_Order_Invoice>>;
    using AppointmentCanBeClosed = Where<Current<FSAppointment.completed>, Equal<True>, And<Current<FSAppointment.closed>, Equal<False>>>;
    using AppointmentCanBeInvoiced = Where<Current<FSAppointment.closed>, Equal<True>>;

    [PXLocalizable]
    public static class QPMessages
    {
        public const string SUCCESS = "Success";
        public const string SUCCESS_DOCUMENT = "Document <*> is created.";
        public const string FAILURE = "Failure";
        public const string OnEmailSalesOrderSuccess = "An email with the sales order has been sent.";
        public const string OnEmailSalesOrderFailure = "Sending the sales order by email.";
    }

    [Serializable]
    public class FSQuickProcessParameters : PX.Data.IBqlTable
	{
		#region SrvOrdType
		[PXDBString(4, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(FSSrvOrdType.srvOrdType))]
        [PXParent(typeof(Select<FSSrvOrdType, Where<FSSrvOrdType.srvOrdType, Equal<Current<srvOrdType>>>>))]
        public virtual string SrvOrdType { get; set; }
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion
        #region AllowInvoiceServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(allowInvoiceServiceOrder.Step))]
        [PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderRequiresAllowBilling))]
        [PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderRequiresAllowBilling))]
        public virtual bool? AllowInvoiceServiceOrder { get; set; }
        public abstract class allowInvoiceServiceOrder : PX.Data.BQL.BqlBool.Field<allowInvoiceServiceOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Allow Billing";
			}
		}
        #endregion
        #region CompleteServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(completeServiceOrder.Step))]
		[PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderCanBeCompleted))]
		[PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderCanBeCompleted))]
		public virtual bool? CompleteServiceOrder { get; set; }
        public abstract class completeServiceOrder : PX.Data.BQL.BqlBool.Field<completeServiceOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Complete";
			}
		}
        #endregion
        #region CloseAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(closeAppointment.Step))]
        [PXQuickProcess.Step.IsStartPoint(typeof(TrueCondition))]
        public virtual bool? CloseAppointment { get; set; }
		public abstract class closeAppointment : PX.Data.BQL.BqlBool.Field<closeAppointment>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Close";
			}
		}
		#endregion
		#region CloseServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(closeServiceOrder.Step))]
        [PXQuickProcess.Step.RequiresSteps(typeof(completeServiceOrder))]
        public virtual bool? CloseServiceOrder { get; set; }
		public abstract class closeServiceOrder : PX.Data.BQL.BqlBool.Field<closeServiceOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Close";
			}
		}
		#endregion
		#region EmailInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(emailInvoice.Step))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<TrueCondition>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice))]
        public virtual bool? EmailInvoice { get; set; }
		public abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Email Invoice";
			}
		}
		#endregion
		#region EmailSalesOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSalesOrder.Step))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoice))]
        public virtual bool? EmailSalesOrder { get; set; }
		public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Email Sales Order/Quote";
			}
		}
		#endregion
		#region EmailSignedAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSignedAppointment.Step))]
        public virtual bool? EmailSignedAppointment { get; set; }
		public abstract class emailSignedAppointment : PX.Data.BQL.BqlBool.Field<emailSignedAppointment>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Email Signed Appointment";
			}
		}
		#endregion
		#region GenerateInvoiceFromAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(generateInvoiceFromAppointment.Step), DisplayName = "Run Billing")]
        [PXQuickProcess.Step.RequiresSteps(typeof(closeAppointment))]
        public virtual bool? GenerateInvoiceFromAppointment { get; set; }
		public abstract class generateInvoiceFromAppointment : PX.Data.BQL.BqlBool.Field<generateInvoiceFromAppointment>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Generate Invoice From Appointment";
			}
		}
		#endregion
		#region GenerateInvoiceFromServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(generateInvoiceFromServiceOrder.Step), DisplayName = "Run Billing")]
        [PXQuickProcess.Step.RequiresSteps(typeof(allowInvoiceServiceOrder))]
        public virtual bool? GenerateInvoiceFromServiceOrder { get; set; }
		public abstract class generateInvoiceFromServiceOrder : PX.Data.BQL.BqlBool.Field<generateInvoiceFromServiceOrder>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Generate Invoice From Service Order";
			}
		}
		#endregion
		#region PayBill
        [PXQuickProcess.Step.IsBoundTo(typeof(payBill.Step))]
        [PXQuickProcess.Step.RequiresSteps(typeof(releaseBill))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<TrueCondition>))]
        public virtual bool? PayBill { get; set; }
		public abstract class payBill : PX.Data.BQL.BqlBool.Field<payBill>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Pay Bill";
			}
		}
		#endregion
		#region PrepareInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(prepareInvoice.Step))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoice))]
        public virtual bool? PrepareInvoice { get; set; }
		public abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Prepare Invoice";
			}
		}
		#endregion
		#region ReleaseBill
        [PXQuickProcess.Step.IsBoundTo(typeof(releaseBill.Step))]
        public virtual bool? ReleaseBill { get; set; }
		public abstract class releaseBill : PX.Data.BQL.BqlBool.Field<releaseBill>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Release Bill";
			}
		}
		#endregion
		#region ReleaseInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(releaseInvoice.Step))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<TrueCondition>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice))]
        public virtual bool? ReleaseInvoice { get; set; }
		public abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Release Invoice";
			}
		}
		#endregion
		#region SOQuickProcess
        [PXQuickProcess.Step.IsBoundTo(typeof(sOQuickProcess.Step))]
        public virtual bool? SOQuickProcess { get; set; }
		public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Use Sales Order Quick Processing";
			}
		}
		#endregion

		// Dummy step action
		#region GenerateInvoice
		[PXQuickProcess.Step.IsBoundTo(typeof(generateInvoice.Step), nonDatabaseField: true)]
		public virtual bool? GenerateInvoice => GenerateInvoiceFromAppointment != null ? (GenerateInvoiceFromAppointment.Value || GenerateInvoiceFromServiceOrder.Value) : false;
		public abstract class generateInvoice : PX.Data.BQL.BqlBool.Field<generateInvoice>
		{
			public class Step : FSQuickProcessDummyStepDefinition
			{
				public override String ActionName => "Generate Invoice";
			}
		}
		#endregion
		public abstract class FSQuickProcessDummyStepDefinition : PXQuickProcess.Step.IDefinition
		{
			public Type Graph => typeof(SvrOrdTypeMaint);
			public abstract String ActionName { get; }
			public String OnSuccessMessage => "";
			public String OnFailureMessage => "";
		}
	}

    [Serializable]
    public class FSSrvOrdQuickProcessParams : SOQuickProcessParameters
    {
        #region SrvOrdType
        [PXString(4, IsFixed = true)]
        [PXUnboundDefault(typeof(FSSrvOrdType.srvOrdType))]
        public virtual string SrvOrdType { get; set; }
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion
        #region AllowInvoiceServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(allowInvoiceServiceOrder.Step), true, DisplayName = "Allow Billing")]
		[PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderRequiresAllowBilling))]
		[PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderRequiresAllowBilling))]
		public bool? AllowInvoiceServiceOrder { get; set; }
        public abstract class allowInvoiceServiceOrder : PX.Data.BQL.BqlBool.Field<allowInvoiceServiceOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<ServiceOrderEntry>
            {
                public Step() : base(g => g.allowBilling) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region CompleteServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(completeServiceOrder.Step), true, DisplayName = "Complete")]
        [PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderCanBeCompleted))]
        [PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderCanBeCompleted))]
        [PXQuickProcess.Step.IsInsertedJustBefore(typeof(generateInvoiceFromServiceOrder))]
        public bool? CompleteServiceOrder { get; set; }
        public abstract class completeServiceOrder : PX.Data.BQL.BqlBool.Field<completeServiceOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<ServiceOrderEntry>
            {
                public Step() : base(g => g.completeOrder) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region CloseServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(closeServiceOrder.Step), true, DisplayName = "Close")]
        [PXQuickProcess.Step.IsApplicable(typeof(ServiceOrderCanBeClosed))]
        [PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderCanBeClosed), PreventStepPresenceChanging = false)]
        [PXQuickProcess.Step.IsInsertedJustBefore(typeof(generateInvoiceFromServiceOrder))]
        [PXQuickProcess.Step.RequiresSteps(typeof(completeServiceOrder))]
        public bool? CloseServiceOrder { get; set; }
        public abstract class closeServiceOrder : PX.Data.BQL.BqlBool.Field<closeServiceOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<ServiceOrderEntry>
            {
                public Step() : base(g => g.closeOrder) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region GenerateInvoiceFromServiceOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(generateInvoiceFromServiceOrder.Step), true, DisplayName = "Run Billing")]
        [PXQuickProcess.Step.IsInsertedJustAfter(typeof(allowInvoiceServiceOrder))]
        [PXQuickProcess.Step.RequiresSteps(typeof(allowInvoiceServiceOrder))]
        [PXQuickProcess.Step.IsStartPoint(typeof(ServiceOrderCanBeInvoiced))]
        [PXUIEnabled(typeof(Where<sOQuickProcess, Equal<False>>))]
        public bool? GenerateInvoiceFromServiceOrder { get; set; }
        public abstract class generateInvoiceFromServiceOrder : PX.Data.BQL.BqlBool.Field<generateInvoiceFromServiceOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<ServiceOrderEntry>
            {
                public Step() : base(g => g.invoiceOrder) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS_DOCUMENT;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region SOQuickProcess
        [PXUIField(DisplayName = "Use Sales Order Quick Processing")]
        public bool? SOQuickProcess { get; set; }
        public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess> { }
        #endregion
        #region EmailSalesOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSalesOrder.Step), true, DisplayName = "Email Sales Order/Quote")]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromServiceOrder))]
        public virtual bool? EmailSalesOrder { get; set; }
        public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<SOOrderEntry>
            {
                public Step() : base(g => g.emailSalesOrder) { }
                public override string OnSuccessMessage => QPMessages.OnEmailSalesOrderSuccess;
                public override string OnFailureMessage => QPMessages.OnEmailSalesOrderFailure;
            }
        }
        #endregion

        #region SOQuickProcessParameters
        #region OrderType
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(SOOrderTypeQuickProcess.orderType))]
        public override String OrderType { get; set; }
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
        #region CreateShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.createShipment.Step), DisplayName = "Create Shipment")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        [PXQuickProcess.Step.IsStartPoint(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? CreateShipment { get; set; }
        public new abstract class createShipment : PX.Data.BQL.BqlBool.Field<createShipment> { }
        #region CreateShipment Parameters
        #region SiteID
        [PXInt]
        [PXUIField(DisplayName = "Warehouse ID", FieldClass = IN.SiteAttribute.DimensionName)]
        [PXQuickProcess.Step.RelatedParameter(typeof(createShipment), nameof(siteID))]
        [FSOrderSiteSelector]
        public override Int32? SiteID { get; set; }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        #endregion
        #endregion
        #endregion
        #region ConfirmShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.confirmShipment.Step), DisplayName = "Confirm Shipment")]
        [PXQuickProcess.Step.RequiresSteps(typeof(createShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? ConfirmShipment { get; set; }
        public new abstract class confirmShipment : PX.Data.BQL.BqlBool.Field<confirmShipment> { }
        #endregion
        #region UpdateIN
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.updateIN.Step), DisplayName = "Update IN")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? UpdateIN { get; set; }
        public new abstract class updateIN : PX.Data.BQL.BqlBool.Field<updateIN> { }
        #endregion
        #region PrepareInvoiceFromShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.prepareInvoiceFromShipment.Step), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<OrderTypeRequiresShipping>>))]
        public override bool? PrepareInvoiceFromShipment { get; set; }
        public new abstract class prepareInvoiceFromShipment : PX.Data.BQL.BqlBool.Field<prepareInvoiceFromShipment> { }
        #endregion
        #region PrepareInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.prepareInvoice.Step), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<Not<OrderTypeRequiresShipping>>>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromServiceOrder))]
        public override bool? PrepareInvoice { get; set; }
        public new abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice> { }
        #endregion
        #region EmailInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.emailInvoice.Step), DisplayName = "Email Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, Or<OrderTypePostToSOInvoice>>))]
        public override bool? EmailInvoice { get; set; }
        public new abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice> { }
        #endregion
        #region ReleaseInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.releaseInvoice.Step), DisplayName = "Release Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment), typeof(generateInvoiceFromServiceOrder))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<True>))]
        public override bool? ReleaseInvoice { get; set; }
        public new abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice> { }
        #endregion
        #region AutoRedirect
        [PXUIField(DisplayName = "Open All Created Documents in New Tabs")]
        [PXUIEnabled(typeof(Where<autoDownloadReports, Equal<False>>))]
        [PXQuickProcess.AutoRedirectOption]
        public override bool? AutoRedirect { get; set; }
        public new abstract class autoRedirect : PX.Data.BQL.BqlBool.Field<autoRedirect> { }
        #endregion
        #region AutoDownloadReports
        [PXUIField(DisplayName = "Download All Created Print Forms")]
        [PXUIEnabled(typeof(Where<autoRedirect, Equal<True>>))]
        [PXQuickProcess.AutoDownloadReportsOption]
        public override bool? AutoDownloadReports { get; set; }
        public new abstract class autoDownloadReports : PX.Data.BQL.BqlBool.Field<autoDownloadReports> { }
        #endregion
        #endregion
    }

    [Serializable]
    public class FSAppQuickProcessParams : SOQuickProcessParameters
    {
        #region SrvOrdType
        [PXString(4, IsFixed = true)]
        [PXUnboundDefault(typeof(FSSrvOrdType.srvOrdType))]
        public virtual string SrvOrdType { get; set; }
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        #endregion
        #region CloseAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(closeAppointment.Step), true, DisplayName = "Close")]
        [PXQuickProcess.Step.IsStartPoint(typeof(TrueCondition))]
        [PXQuickProcess.Step.IsApplicable(typeof(AppointmentCanBeClosed))]
        public virtual bool? CloseAppointment { get; set; }
        public abstract class closeAppointment : PX.Data.BQL.BqlBool.Field<closeAppointment>
        {
            public class Step : PXQuickProcess.Step.Definition<AppointmentEntry>
            {
                public Step() : base(g => g.closeAppointment) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region EmailSignedAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSignedAppointment.Step), true, DisplayName = "Email Signed Appointment")]
        [PXQuickProcess.Step.IsInsertedJustAfter(typeof(generateInvoiceFromAppointment))]
        public virtual bool? EmailSignedAppointment { get; set; }
        public abstract class emailSignedAppointment : PX.Data.BQL.BqlBool.Field<emailSignedAppointment>
        {
            public class Step : PXQuickProcess.Step.Definition<AppointmentEntry>
            {
                public Step() : base(g => g.emailSignedAppointment) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region GenerateInvoiceFromAppointment
        [PXQuickProcess.Step.IsBoundTo(typeof(generateInvoiceFromAppointment.Step), true, DisplayName = "Run Billing")]
        [PXQuickProcess.Step.IsStartPoint(typeof(AppointmentCanBeInvoiced))]
        [PXQuickProcess.Step.IsInsertedJustAfter(typeof(closeAppointment))]
        [PXQuickProcess.Step.RequiresSteps(typeof(closeAppointment))]
        [PXUIEnabled(typeof(Where<sOQuickProcess, Equal<False>>))]
        public virtual bool? GenerateInvoiceFromAppointment { get; set; }
        public abstract class generateInvoiceFromAppointment : PX.Data.BQL.BqlBool.Field<generateInvoiceFromAppointment>
        {
            public class Step : PXQuickProcess.Step.Definition<AppointmentEntry>
            {
                public Step() : base(g => g.invoiceAppointment) { }
                public override String OnSuccessMessage => QPMessages.SUCCESS_DOCUMENT;
                public override String OnFailureMessage => QPMessages.FAILURE;
            }
        }
        #endregion
        #region SOQuickProcess
        [PXUIField(DisplayName = "Use Sales Order Quick Processing")]
        public bool? SOQuickProcess { get; set; }
        public abstract class sOQuickProcess : PX.Data.BQL.BqlBool.Field<sOQuickProcess> { }
        #endregion
        #region EmailSalesOrder
        [PXQuickProcess.Step.IsBoundTo(typeof(emailSalesOrder.Step), true, DisplayName = "Email Sales Order/Quote")]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromAppointment))]
        public virtual bool? EmailSalesOrder { get; set; }
        public abstract class emailSalesOrder : PX.Data.BQL.BqlBool.Field<emailSalesOrder>
        {
            public class Step : PXQuickProcess.Step.Definition<SOOrderEntry>
            {
                public Step() : base(g => g.emailSalesOrder) { }
                public override string OnSuccessMessage => QPMessages.OnEmailSalesOrderSuccess;
                public override string OnFailureMessage => QPMessages.OnEmailSalesOrderFailure;
            }
        }
        #endregion

        #region SOQuickProcessParameters
        #region OrderType
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(SOOrderTypeQuickProcess.orderType))]
        public override String OrderType { get; set; }
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
        #region CreateShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.createShipment.Step), DisplayName = "Create Shipment")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        [PXQuickProcess.Step.IsStartPoint(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? CreateShipment { get; set; }
        public new abstract class createShipment : PX.Data.BQL.BqlBool.Field<createShipment> { }
        #region CreateShipment Parameters
        #region SiteID
        [PXInt]
        [PXUIField(DisplayName = "Warehouse ID", FieldClass = IN.SiteAttribute.DimensionName)]
        [PXQuickProcess.Step.RelatedParameter(typeof(createShipment), nameof(siteID))]
        [FSOrderSiteSelector]
        public override Int32? SiteID { get; set; }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        #endregion
        #endregion
        #endregion
        #region ConfirmShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.confirmShipment.Step), DisplayName = "Confirm Shipment")]
        [PXQuickProcess.Step.RequiresSteps(typeof(createShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? ConfirmShipment { get; set; }
        public new abstract class confirmShipment : PX.Data.BQL.BqlBool.Field<confirmShipment> { }
        #endregion
        #region UpdateIN
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.updateIN.Step), DisplayName = "Update IN")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<OrderTypeRequiresShipping>))]
        public override bool? UpdateIN { get; set; }
        public new abstract class updateIN : PX.Data.BQL.BqlBool.Field<updateIN> { }
        #endregion
        #region PrepareInvoiceFromShipment
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.prepareInvoiceFromShipment.Step), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(confirmShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<OrderTypeRequiresShipping>>))]
        public override bool? PrepareInvoiceFromShipment { get; set; }
        public new abstract class prepareInvoiceFromShipment : PX.Data.BQL.BqlBool.Field<prepareInvoiceFromShipment> { }
        #endregion
        #region PrepareInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.prepareInvoice.Step), DisplayName = "Prepare Invoice")]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, And<Not<OrderTypeRequiresShipping>>>))]
        [PXQuickProcess.Step.RequiresSteps(typeof(generateInvoiceFromAppointment))]
        public override bool? PrepareInvoice { get; set; }
        public new abstract class prepareInvoice : PX.Data.BQL.BqlBool.Field<prepareInvoice> { }
        #endregion
        #region EmailInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.emailInvoice.Step), DisplayName = "Email Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where2<OrderTypeRequiresInvoicing, Or<OrderTypePostToSOInvoice>>))]
        public override bool? EmailInvoice { get; set; }
        public new abstract class emailInvoice : PX.Data.BQL.BqlBool.Field<emailInvoice> { }
        #endregion
        #region ReleaseInvoice
        [PXQuickProcess.Step.IsBoundTo(typeof(SOQuickProcessParameters.releaseInvoice.Step), DisplayName = "Release Invoice")]
        [PXQuickProcess.Step.RequiresSteps(typeof(prepareInvoice), typeof(prepareInvoiceFromShipment), typeof(generateInvoiceFromAppointment))]
        [PXQuickProcess.Step.IsApplicable(typeof(Where<True>))]
        public override bool? ReleaseInvoice { get; set; }
        public new abstract class releaseInvoice : PX.Data.BQL.BqlBool.Field<releaseInvoice> { }
        #endregion
        #region AutoRedirect
        [PXUIField(DisplayName = "Open All Created Documents in New Tabs")]
        [PXUIEnabled(typeof(Where<autoDownloadReports, Equal<False>>))]
        [PXQuickProcess.AutoRedirectOption]
        public override bool? AutoRedirect { get; set; }
        public new abstract class autoRedirect : PX.Data.BQL.BqlBool.Field<autoRedirect> { }
        #endregion
        #region AutoDownloadReports
        [PXUIField(DisplayName = "Download All Created Print Forms")]
        [PXUIEnabled(typeof(Where<autoRedirect, Equal<True>>))]
        [PXQuickProcess.AutoDownloadReportsOption]
        public override bool? AutoDownloadReports { get; set; }
        public new abstract class autoDownloadReports : PX.Data.BQL.BqlBool.Field<autoDownloadReports> { }
        #endregion
        #endregion
    }

    public class FSOrderSiteSelectorAttribute : PXSelectorAttribute
    {
        protected string _InputMask = null;

        public FSOrderSiteSelectorAttribute()
            : base(typeof(Search<INSite.siteID,
                Where<Match<INSite, Current<AccessInfo.userName>>>>),
                typeof(INSite.siteCD), typeof(INSite.descr), typeof(INSite.replenishmentClassID)
            )
        {
            this.DirtyRead = true;
            this.SubstituteKey = typeof(INSite.siteCD);
            this.DescriptionField = typeof(INSite.descr);
            this._UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteID, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>));
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            PXDimensionAttribute attr = new PXDimensionAttribute(SiteAttribute.DimensionName);
            attr.CacheAttached(sender);
            attr.FieldName = _FieldName;
            PXFieldSelectingEventArgs e = new PXFieldSelectingEventArgs(null, null, true, false);
            attr.FieldSelecting(sender, e);

            _InputMask = ((PXSegmentedState)e.ReturnState).InputMask;
        }

        public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            base.SubstituteKeyFieldSelecting(sender, e);
            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, null, null, null, _InputMask, null, null, null, null);
            }
        }
    }
}
