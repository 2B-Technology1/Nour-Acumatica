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

using PX.Api;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.SM;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Payroll Module's extension of the PaymentMethod DAC.
	/// </summary>
	public class PRxPaymentMethod : PXCacheExtension<PaymentMethod>
	{
		private const short _DefaultStubLines = 12;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region UseForPR
		public abstract class useForPR : PX.Data.BQL.BqlBool.Field<useForPR> { }
		[PXDBBool]
		[PXUIEnabled(typeof(Where<PaymentMethod.paymentType, NotEqual<PaymentMethodType.eft>>))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Use in PR", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? UseForPR { get; set; }
		#endregion

		#region PRProcessing
		[PXString]
		[PRxPaymentMethod.prProcessing.List]
		[PXDefault(prProcessing.PrintChecks, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCalced(typeof(Switch<Case<Where<PRxPaymentMethod.prCreateBatchPayment, Equal<True>>, prProcessing.createBatchPayment>, prProcessing.printChecks>), typeof(string))]
		public virtual string PRProcessing { get; set; }
		public abstract class prProcessing : PX.Data.BQL.BqlString.Field<prProcessing>
		{
			public const string PrintChecks = "P";
			public const string CreateBatchPayment = "B";

			public class printChecks : PX.Data.BQL.BqlString.Constant<printChecks>
			{
				public printChecks() : base(PrintChecks) { }
			}

			public class createBatchPayment : PX.Data.BQL.BqlString.Constant<createBatchPayment>
			{
				public createBatchPayment() : base(CreateBatchPayment) { }
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(new string[] { PrintChecks, CreateBatchPayment },
							new string[] { Messages.PrintChecks, Messages.CreateBatchPayment })
				{
				}
			}
		}
		#endregion

		#region PRBatchExportSYMappingID
		public abstract class prBatchExportSYMappingID : PX.Data.BQL.BqlGuid.Field<prBatchExportSYMappingID> { }
		[PXDBGuid]
		[PXUIField(DisplayName = "Export Scenario", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<SYMapping.mappingID, Where<SYMapping.mappingType, Equal<SYMapping.mappingType.typeExport>>>), SubstituteKey = typeof(SYMapping.name))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<PRxPaymentMethod.useForPR, Equal<True>,
			And<PRxPaymentMethod.prCreateBatchPayment, Equal<True>>>))]
		[PXUIVisible(typeof(Where<PRxPaymentMethod.prCreateBatchPayment.IsEqual<True>>))]
		public virtual Guid? PRBatchExportSYMappingID { get; set; }
		#endregion

		#region PRPrintChecks
		public abstract class prPrintChecks : PX.Data.BQL.BqlBool.Field<prPrintChecks> { }
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Print Checks")]
		public virtual bool? PRPrintChecks { get; set; }
		#endregion

		#region PRCreateBatchPayment
		public abstract class prCreateBatchPayment : PX.Data.BQL.BqlBool.Field<prCreateBatchPayment> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Create Batch Payment")]
		public virtual bool? PRCreateBatchPayment { get; set; }
		#endregion

		#region PRCheckReportID
		public abstract class prCheckReportID : PX.Data.BQL.BqlString.Field<prCheckReportID> { }
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXSelector(typeof(Search<SiteMap.screenID,
				Where<SiteMap.screenID, Like<pr_>, And<SiteMap.url, Like<Common.urlReports>>>>),
			typeof(SiteMap.screenID), typeof(SiteMap.title),
			Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
			DescriptionField = typeof(SiteMap.title))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<PRxPaymentMethod.useForPR, Equal<True>,
			And<PRxPaymentMethod.prPrintChecks, Equal<True>>>))]
		[PXUIVisible(typeof(Where<PRxPaymentMethod.prPrintChecks.IsNotEqual<False>>))]
		public virtual string PRCheckReportID { get; set; }
		#endregion
	}
}
