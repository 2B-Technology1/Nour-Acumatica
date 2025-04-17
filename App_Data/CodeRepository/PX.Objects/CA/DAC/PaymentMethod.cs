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
using PX.Api;
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.SM;
using PX.Objects.CS;
using System.Collections.Generic;

namespace PX.Objects.CA
{
	public static class PaymentMethodType
	{
		public const string EFT = "EFT";
		public const string CreditCard = "CCD";
		public const string CashOrCheck = "CHC";
		public const string DirectDeposit = "DDT";

		public class ListAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
		{
			public ListAttribute() : base(GetValuesAndLabels())
			{ }

			public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				PaymentMethod row = e.Row as PaymentMethod;

				SetList(sender, row, FieldName, GetValuesAndLabels());
			}
		}

		public class eft : PX.Data.BQL.BqlString.Constant<eft>
		{
			public eft() : base(EFT) { }
		}

		public class creditCard : PX.Data.BQL.BqlString.Constant<creditCard>
		{
			public creditCard() : base(CreditCard) { }
		}

		public class cashOrCheck : PX.Data.BQL.BqlString.Constant<cashOrCheck>
		{
			public cashOrCheck() : base(CashOrCheck) { }
		}

		public class directDeposit : PX.Data.BQL.BqlString.Constant<directDeposit>
		{
			public directDeposit() : base(DirectDeposit) { }
		}

		static (string, string)[] GetValuesAndLabels()
		{
			List<(string, string)> result = new List<(string, string)>();

			if (PXAccess.FeatureInstalled<CS.FeaturesSet.acumaticaPayments>())
			{
				result.Add((EFT, "EFT"));
			}
			result.AddRange(new (string, string)[] { (CreditCard, "Credit Card"), (CashOrCheck, "Cash/Check"), (DirectDeposit, "Direct Deposit") });
			return result.ToArray();
		}
	}

	/// <summary>
	/// The payment method settings.
	/// </summary>
	[PXCacheName(Messages.PaymentMethod)]
	[Serializable]
	[PXPrimaryGraph(typeof(PaymentMethodMaint))]
	public partial class PaymentMethod : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PaymentMethod>.By<paymentMethodID>
		{
			public static PaymentMethod Find(PXGraph graph, string paymentMethodID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, paymentMethodID, options);
		}
		public static class FK
		{
			public class CustomerPaymentMethod : AR.CustomerPaymentMethod.PK.ForeignKeyOf<PaymentMethod>.By<pMInstanceID> { }
			public class ExportScenario : Api.SYMapping.PK.ForeignKeyOf<PaymentMethod>.By<aPBatchExportSYMappingID> { }
			public class APCheckReport : PX.SM.SiteMap.UK.ForeignKeyOf<PaymentMethod>.By<aPCheckReportID> { }
			public class RemittanceReport : PX.SM.SiteMap.UK.ForeignKeyOf<PaymentMethod>.By<aPRemittanceReportID> { }
		}
		#endregion

		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = " Payment Method ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
		[PXReferentialIntegrityCheck]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		[PXDBForeignIdentity(typeof(PMInstance))]
		public virtual int? PMInstanceID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual string Descr
		{
			get;
			set;
		}
		#endregion
		#region PaymentType
		public abstract class paymentType : PX.Data.BQL.BqlString.Field<paymentType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(PaymentMethodType.CashOrCheck, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PaymentMethodType.List]
		[PXUIField(DisplayName = "Means of Payment", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string PaymentType
		{
			get;
			set;
		}
		#endregion
		#region DirectDepositFileFormat
		public abstract class directDepositFileFormat : PX.Data.BQL.BqlString.Field<directDepositFileFormat> { }
		[PXDBString(10)]
		[PXUIField(DisplayName = "Direct Deposit File Format")]
		[DirectDepositTypeList]
		public virtual string DirectDepositFileFormat
		{
			get;
			set;
		}
		#endregion
		#region DefaultCashAccountID
		public abstract class defaultCashAccountID : PX.Data.BQL.BqlInt.Field<defaultCashAccountID> { }
        [PXInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? DefaultCashAccountID
		{
			get;
			set;
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? IsActive
		{
			get;
			set;
		}
		#endregion
		#region UseForAR
		public abstract class useForAR : PX.Data.BQL.BqlBool.Field<useForAR> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use in AR", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? UseForAR
		{
			get;
			set;
		}
		#endregion
		#region UseForAP
		public abstract class useForAP : PX.Data.BQL.BqlBool.Field<useForAP> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use in AP", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? UseForAP
		{
			get;
			set;
		}
		#endregion
		#region UseForCA
		public abstract class useForCA : PX.Data.BQL.BqlBool.Field<useForCA> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Require Remittance Information for Cash Account")]
		public virtual bool? UseForCA
		{
			get;
			set;
		}
		#endregion
		#region APAdditionalProcessing

		public abstract class aPAdditionalProcessing : PX.Data.BQL.BqlString.Field<aPAdditionalProcessing>
		{
			public const string PrintChecks = "P";
			public const string CreateBatchPayment = "B";
			public const string NotRequired = "N";

			public class printChecks : PX.Data.BQL.BqlString.Constant<printChecks>
			{
				public printChecks() : base(PrintChecks) { }
			}

			public class createBatchPayment : PX.Data.BQL.BqlString.Constant<createBatchPayment>
			{
				public createBatchPayment() : base(CreateBatchPayment) { }
			}

			public class notRequired : PX.Data.BQL.BqlString.Constant<notRequired>
			{
				public notRequired() : base(NotRequired) { }
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(new string[] { PrintChecks, CreateBatchPayment, NotRequired },
							new string[] { "Print Checks", "Create Batch Payments", "Not Required" })
				{
				}
			}
		}
		[PXString]
		[aPAdditionalProcessing.List]
		[PXDefault(aPAdditionalProcessing.NotRequired)]
		[PXDBCalced(typeof(Switch<Case<Where<PaymentMethod.aPPrintChecks, Equal<True>>, aPAdditionalProcessing.printChecks,
								  Case<Where<PaymentMethod.aPCreateBatchPayment, Equal<True>>, aPAdditionalProcessing.createBatchPayment>>,
								  aPAdditionalProcessing.notRequired>), typeof(string))]
		public virtual string APAdditionalProcessing
		{
			get;
			set;
		}
		#endregion
		#region SkipExport
		public abstract class skipExport : PX.Data.BQL.BqlBool.Field<skipExport> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Release Batch Payment Before Export")]
		[PXUIEnabled(typeof(Where<Current<aPAdditionalProcessing>, Equal<aPAdditionalProcessing.createBatchPayment>>))]
		[PXUIVisible(typeof(Where<Current<aPAdditionalProcessing>, Equal<aPAdditionalProcessing.createBatchPayment>>))]
		public virtual bool? SkipExport
		{
			get;
			set;
		}
		#endregion
		#region APCreateBatchPayment
		public abstract class aPCreateBatchPayment : PX.Data.BQL.BqlBool.Field<aPCreateBatchPayment> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Create Batch Payment")]
		public virtual bool? APCreateBatchPayment
		{
			get;
			set;
		}
		#endregion
		#region APBatchExportMethod
		public abstract class aPBatchExportMethod : PX.Data.BQL.BqlString.Field<aPBatchExportMethod> { }

		[PXDBString(1, IsFixed = true)]
		[PXDefault(ACHExportMethod.ExportScenario, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[ACHExportMethod.List]
		[PXUIField(DisplayName = "Export Method", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIVisible(typeof(Where<PaymentMethod.useForAP, Equal<True>,
			And<PaymentMethod.aPCreateBatchPayment, Equal<True>>>))]
		public virtual string APBatchExportMethod { get; set; }
		#endregion
		#region APBatchExportSYMappingID
		public abstract class aPBatchExportSYMappingID : PX.Data.BQL.BqlGuid.Field<aPBatchExportSYMappingID> { }
		[PXDBGuid]
		[PXUIField(DisplayName = "Export Scenario", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<SYMapping.mappingID, Where<SYMapping.mappingType, Equal<SYMapping.mappingType.typeExport>>>), SubstituteKey = typeof(SYMapping.name))]
		[PXDefault]
		[PXUIVisible(typeof(Where<PaymentMethod.useForAP, Equal<True>,
			And<PaymentMethod.aPCreateBatchPayment, Equal<True>,
			And<PaymentMethod.aPBatchExportMethod, Equal<ACHExportMethod.exportScenario>>>>))]
		[PXUIRequired(typeof(Where<PaymentMethod.useForAP, Equal<True>,
			And<PaymentMethod.aPCreateBatchPayment, Equal<True>,
			And<PaymentMethod.aPBatchExportMethod, Equal<ACHExportMethod.exportScenario>>>>))]
		public virtual Guid? APBatchExportSYMappingID
		{
			get;
			set;
		}
		#endregion
		#region APBatchExportPlugInTypeName
		public abstract class aPBatchExportPlugInTypeName : PX.Data.BQL.BqlGuid.Field<aPBatchExportPlugInTypeName> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Export Plug-In (Type)", Visibility = PXUIVisibility.Visible, Visible = false)]
		[ACHPlugInType]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string APBatchExportPlugInTypeName
		{
			get;
			set;
		}
		#endregion
		#region SkipPaymentsWithZeroAmt
		public abstract class skipPaymentsWithZeroAmt : PX.Data.BQL.BqlBool.Field<skipPaymentsWithZeroAmt> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that payments with 0 amount should not be exported to EFT file.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Skip Payments with Zero Amount")]
		public virtual bool? SkipPaymentsWithZeroAmt
		{
			get;
			set;
		}
		#endregion
		#region RequireBatchSeqNum
		public abstract class requireBatchSeqNum : PX.Data.BQL.BqlBool.Field<requireBatchSeqNum> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Batch Seq. Number")]
		public virtual bool? RequireBatchSeqNum
		{
			get;
			set;
		}
		#endregion
		#region APPrintChecks
		public abstract class aPPrintChecks : PX.Data.BQL.BqlBool.Field<aPPrintChecks> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Print Checks")]
		public virtual bool? APPrintChecks
		{
			get;
			set;
		}
		#endregion
		#region APCheckReportID
		public abstract class aPCheckReportID : PX.Data.BQL.BqlString.Field<aPCheckReportID> { }
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXSelector(typeof(Search<SiteMap.screenID, Where<SiteMap.screenID, Like<PXModule.ap_>, And<SiteMap.url, Like<Common.urlReports>>>>), typeof(SiteMap.screenID), typeof(SiteMap.title), Headers = new string[] { Messages.ReportID, Messages.ReportName }, DescriptionField = typeof(SiteMap.title))]
		[PXDefault]
		[PXUIRequired(typeof(Where<PaymentMethod.useForAP, Equal<True>,
			And<PaymentMethod.aPPrintChecks, Equal<True>>>))]
		public virtual string APCheckReportID
		{
			get;
			set;
		}
		#endregion
		#region APStubLines
		public abstract class aPStubLines : PX.Data.BQL.BqlShort.Field<aPStubLines> { }
		[PXDBShort(MinValue = 1)]
		[PXDefault((short)10)]
		[PXUIField(DisplayName = "Lines per Stub")]
		public virtual short? APStubLines
		{
			get;
			set;
		}
		#endregion
		#region APPrintRemittance
		public abstract class aPPrintRemittance : PX.Data.BQL.BqlBool.Field<aPPrintRemittance> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Print Remittance Report", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual bool? APPrintRemittance
		{
			get;
			set;
		}
		#endregion
		#region APRemittanceReportReportID
		public abstract class aPRemittanceReportID : PX.Data.BQL.BqlString.Field<aPRemittanceReportID> { }
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Remittance Report")]
		[PXSelector(typeof(Search<SiteMap.screenID, Where<SiteMap.screenID, Like<PXModule.ap_>, And<SiteMap.url, Like<Common.urlReports>>>>), typeof(SiteMap.screenID), typeof(SiteMap.title), Headers = new string[] { Messages.ReportID, Messages.ReportName }, DescriptionField = typeof(SiteMap.title))]
		[PXDefault]
		[PXUIRequired(typeof(Where<PaymentMethod.useForAP, Equal<True>,
			And<PaymentMethod.aPPrintChecks, Equal<True>,
			And<PaymentMethod.aPPrintRemittance, Equal<True>>>>))]
		public virtual string APRemittanceReportID
		{
			get;
			set;
		}
		#endregion
		#region APRequirePaymentRef
		public abstract class aPRequirePaymentRef : PX.Data.BQL.BqlBool.Field<aPRequirePaymentRef> { }


		[PXDBBool]
		[PXUIField(DisplayName = "Require Unique Payment Ref.")]
		[PXDefault(true)]
		public virtual bool? APRequirePaymentRef
		{
			get;
			set;
		}
		#endregion

		#region ARIsProcessingRequired
		public abstract class aRIsProcessingRequired : PX.Data.BQL.BqlBool.Field<aRIsProcessingRequired> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Integrated Processing")]
		public virtual bool? ARIsProcessingRequired
		{
			get;
			set;
		}
		#endregion
		#region ARIsOnePerCustomer
		public abstract class aRIsOnePerCustomer : PX.Data.BQL.BqlBool.Field<aRIsOnePerCustomer> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "One Instance Per Customer")]
		public virtual bool? ARIsOnePerCustomer
		{
			get;
			set;
		}
		#endregion
		#region ARDepositAsBatch
		public abstract class aRDepositAsBatch : PX.Data.BQL.BqlBool.Field<aRDepositAsBatch> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Batch Deposit")]
		public virtual bool? ARDepositAsBatch
		{
			get;
			set;
		}
		#endregion
		#region ARVoidOnDepositAccount
		public abstract class aRVoidOnDepositAccount : PX.Data.BQL.BqlBool.Field<aRVoidOnDepositAccount> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Void On Clearing Account")]
		public virtual bool? ARVoidOnDepositAccount
		{
			get;
			set;
		}
		#endregion
		#region ARDefaultVoidDateToDocumentDate
		public abstract class aRDefaultVoidDateToDocumentDate : PX.Data.BQL.BqlBool.Field<aRDefaultVoidDateToDocumentDate> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default Void Date to Document Date")]
		public virtual bool? ARDefaultVoidDateToDocumentDate
		{
			get;
			set;
		}
		#endregion
		#region ARHasBillingInfo
		public abstract class aRHasBillingInfo : PX.Data.BQL.BqlBool.Field<aRHasBillingInfo> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Has Billing Information")]
		public virtual bool? ARHasBillingInfo
		{
			get;
			set;
		}
		#endregion

		#region ContainsPersonalData
		public abstract class containsPersonalData : PX.Data.BQL.BqlBool.Field<containsPersonalData> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Contains Personal Data", FieldClass = FeaturesSet.gDPRCompliance.FieldClass)]
		public virtual bool? ContainsPersonalData { get; set; }
		#endregion
		#region PaymentDateToBankDate
		public abstract class paymentDateToBankDate : PX.Data.BQL.BqlBool.Field<paymentDateToBankDate> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Set Payment Date to Bank Transaction Date")]
		public virtual bool? PaymentDateToBankDate { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(PaymentMethod.paymentMethodID))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region IsAccountNumberRequired
		public abstract class isAccountNumberRequired : PX.Data.BQL.BqlBool.Field<isAccountNumberRequired> { }

		[PXBool]
		[PXUIField(DisplayName = "Require Card/Account Number")]
		public virtual bool? IsAccountNumberRequired
		{
			[PXDependsOnFields(typeof(aRIsOnePerCustomer))]
			get
			{
				return this.ARIsOnePerCustomer.HasValue ? (!this.ARIsOnePerCustomer.Value) : this.ARIsOnePerCustomer;
			}

			set
			{
				this.ARIsOnePerCustomer = (value.HasValue ? !(value.Value) : value);
			}
		}
		#endregion
		#region PrintOrExport
		public abstract class printOrExport : PX.Data.BQL.BqlBool.Field<printOrExport> { }
		[PXBool]
		[PXUIField(DisplayName = "Print Checks/Export", Visibility = PXUIVisibility.Visible)]
		[PXFormula(typeof(IIf<Where<PaymentMethod.aPPrintChecks, Equal<True>, Or<PaymentMethod.aPCreateBatchPayment, Equal<True>>>, True, False>))]
		public virtual bool? PrintOrExport
		{
			get;
			set;
		}
		#endregion

		#region HasProcessingCenters
		public abstract class hasProcessingCenters : PX.Data.BQL.BqlBool.Field<hasProcessingCenters> { }

		[PXBool]
		public virtual bool? HasProcessingCenters
		{
			[PXDependsOnFields(typeof(aRIsProcessingRequired))]
			get
			{
				return PXAccess.FeatureInstalled<FeaturesSet.integratedCardProcessing>() && this.ARIsProcessingRequired == true;
			}
			set
			{
			}
		}
		#endregion
		#region IsUsingPlugin
		public abstract class isUsingPlugin : PX.Data.BQL.BqlBool.Field<isUsingPlugin> { }

		[PXBool]
		public virtual bool? IsUsingPlugin
		{
			[PXDependsOnFields(typeof(aPAdditionalProcessing), typeof(aPBatchExportMethod), typeof(aPBatchExportPlugInTypeName))]
			get
			{
				return APAdditionalProcessing == aPAdditionalProcessing.CreateBatchPayment && APBatchExportMethod == ACHExportMethod.PlugIn && !string.IsNullOrEmpty(APBatchExportPlugInTypeName);
			}
			set
			{
			}
		}
		#endregion
	}

	[Serializable]
	[PXHidden]
	[PXCacheName(Messages.PMInstance)]
	public partial class PMInstance : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PMInstance>.By<pMInstanceID>
		{
			public static PMInstance Find(PXGraph graph, int? pMInstanceID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, pMInstanceID, options);
		}
		#endregion

		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		[PXDBIdentity]
		public virtual int? PMInstanceID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}

	[PXHidden]
	public partial class PaymentMethodActive : PaymentMethod
	{
		#region PaymentMethodID
		new public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		#endregion
		#region IsActive
		new public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion
	}
}
