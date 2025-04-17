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
using Autofac;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.Localizations.GB
{
	/// <summary>
	/// Direct deposit type specific to the Lloyds bank in the UK.
	/// </summary>
	public class LloydsDirectDepositType : IDirectDepositType
	{
		#region Constants
		public const string Code = "BACSLLOYDS";
		public const string Decsription = "BACS Lloyds payment";
		// TO DO: generate a new GUID
		private const string DefaultExportScenarioGuid = "60601315-FA5C-48E9-8323-61DCE1E62AC4";

		private static class BACSLloydsDefaults
		{
			public enum AttributeName
			{
				// AP
				DestinationSortingCodeNumber,
				DestinationAccountNumber,
				DestinationAccountName,
				// Remittance
				OriginatingSortingCodeNumber,
				OriginatingAccountNumber
			}

			// AP
			public const string DestinationSortingCodeNumber = "Destination sorting code number";
			public const string DestinationSortingCodeNumberMask = "000000";
			public const string DestinationSortingCodeNumberValExp = @"^\d{6,6}$";

			public const string DestinationAccountNumber = "Destination Account Number";
			public const string DestinationAccountNumberMask = "00000000";
			public const string DestinationAccountNumberValExp = @"^\d{8,8}$";

			public const string DestinationAccountName = "Destination Account Name";
			public const string DestinationAccountNameMask = "CCCCCCCCCCCCCCCCCC";

			// Remittance
			public const string OriginatingSortingCodeNumber = "Originating sorting code number";
			public const string OriginatingSortingCodeNumberMask = "000000";
			public const string OriginatingSortingCodeNumberValExp = @"^\d{6,6}$";

			public const string OriginatingAccountNumber = "Originating account number";
			public const string OriginatingAccountNumberMask = "00000000";
			public const string OriginatingAccountNumberValExp = @"^\d{8,8}$";
		}
		#endregion

		#region IsActive
		public bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.uKLocalization>();
		}
		#endregion
		#region GetDirectDepositType
		public DirectDepositType GetDirectDepositType() => new DirectDepositType() { Code = Code, Description = Decsription };
		#endregion
		#region GetDefaults
		public IEnumerable<PaymentMethodDetail> GetDefaults()
		{
			return new List<PaymentMethodDetail>()
			{
				// AP
				new PaymentMethodDetail() { DetailID = "1", OrderIndex = 1, Descr = BACSLloydsDefaults.DestinationSortingCodeNumber, IsRequired = true, EntryMask = BACSLloydsDefaults.DestinationSortingCodeNumberMask,
					ValidRegexp = BACSLloydsDefaults.DestinationSortingCodeNumberValExp, UseFor = PaymentMethodDetailUsage.UseForVendor.ToString() },
				new PaymentMethodDetail() { DetailID = "2", OrderIndex = 2, Descr = BACSLloydsDefaults.DestinationAccountNumber, IsRequired = true, EntryMask = BACSLloydsDefaults.DestinationAccountNumberMask,
					ValidRegexp = BACSLloydsDefaults.DestinationAccountNumberValExp, UseFor = PaymentMethodDetailUsage.UseForVendor.ToString() },
				new PaymentMethodDetail() { DetailID = "3", OrderIndex = 3, Descr = BACSLloydsDefaults.DestinationAccountName, IsRequired = true, EntryMask = BACSLloydsDefaults.DestinationAccountNameMask,
					ValidRegexp = string.Empty, UseFor = PaymentMethodDetailUsage.UseForVendor.ToString() },
				// Remittance
				new PaymentMethodDetail() { DetailID = "1", OrderIndex = 1, Descr = BACSLloydsDefaults.OriginatingSortingCodeNumber, IsRequired = true, EntryMask = BACSLloydsDefaults.OriginatingSortingCodeNumberMask,
					ValidRegexp = BACSLloydsDefaults.OriginatingSortingCodeNumberValExp, UseFor = PaymentMethodDetailUsage.UseForCashAccount.ToString() },
				new PaymentMethodDetail() { DetailID = "2", OrderIndex = 2, Descr = BACSLloydsDefaults.OriginatingAccountNumber, IsRequired = true, EntryMask = BACSLloydsDefaults.OriginatingAccountNumberMask,
					ValidRegexp = BACSLloydsDefaults.OriginatingAccountNumberValExp, UseFor = PaymentMethodDetailUsage.UseForCashAccount.ToString() }
			};
		}
		#endregion
		#region SetPaymentMethodDefaults
		public void SetPaymentMethodDefaults(PXCache cache)
		{
			PaymentMethod paymentMethod = (PaymentMethod)cache.Current;

			if (paymentMethod.DirectDepositFileFormat == Code)
			{
				cache.SetValueExt<PaymentMethod.useForAP>(cache.Current, true);
				cache.SetValueExt<PaymentMethod.useForAR>(cache.Current, false);
				cache.SetValueExt<PaymentMethod.useForCA>(cache.Current, true);
				cache.SetValueExt<PaymentMethod.aPAdditionalProcessing>(cache.Current, PaymentMethod.aPAdditionalProcessing.CreateBatchPayment);
				cache.SetValueExt<PaymentMethod.requireBatchSeqNum>(cache.Current, true);
				cache.SetValueExt<PaymentMethod.aPBatchExportSYMappingID>(cache.Current, Guid.Parse(DefaultExportScenarioGuid));
			}
		}
		#endregion
	}
}
