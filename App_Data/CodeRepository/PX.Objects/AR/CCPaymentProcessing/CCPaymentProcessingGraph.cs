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
using PX.CCProcessingBase;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public class CCPaymentProcessingGraph : PXGraph<CCPaymentProcessingGraph>, ICCPaymentProcessor
	{
		public ICCPaymentProcessingRepository Repository { get; set; }
		
		#region Public Auxiliary Functions
		public virtual bool TestCredentials(PXGraph callerGraph, string processingCenterID)
 		{
			return new CCPaymentProcessing(callerGraph).TestCredentials(callerGraph, processingCenterID);
		}	

		public virtual void ValidateSettings(PXGraph callerGraph, string processingCenterID, PluginSettingDetail settingDetail)
		{
			new CCPaymentProcessing(callerGraph).ValidateSettings(callerGraph, processingCenterID, settingDetail);
		}

		public virtual IList<PluginSettingDetail> ExportSettings(PXGraph callerGraph, string processingCenterID)
		{
			return new CCPaymentProcessing(callerGraph).ExportSettings(callerGraph, processingCenterID);
		}
		
		#endregion
		#region Public Processing Functions

		public virtual TranOperationResult Authorize(ICCPayment aPmtInfo, bool aCapture)
		{
			return GetPaymentProcessing().Authorize(aPmtInfo, aCapture);
		}

		public virtual TranOperationResult Capture(ICCPayment payment, int? transactionId)
		{
			return GetPaymentProcessing().Capture(payment, transactionId);
		}

		public virtual TranOperationResult CaptureOnly(ICCPayment payment, string aAuthorizationNbr)
		{
			return GetPaymentProcessing().CaptureOnly(payment, aAuthorizationNbr);
		}

		public virtual TranOperationResult Void(ICCPayment payment, int? aRefTranNbr)
		{
			return GetPaymentProcessing().Void(payment, aRefTranNbr);
		}

		public virtual TranOperationResult VoidOrCredit(ICCPayment payment, int? transactionId)
		{
			return GetPaymentProcessing().VoidOrCredit(payment, transactionId);
		}

		public virtual TranOperationResult Credit(ICCPayment aPmtInfo, string aExtRefTranNbr, string procCenterId)
		{
			return GetPaymentProcessing().Credit(aPmtInfo, aExtRefTranNbr, procCenterId);
		}

		public virtual TranOperationResult Credit(ICCPayment aPmtInfo, int? transactionId)
		{
			return GetPaymentProcessing().Credit(aPmtInfo, transactionId);
		}

		public virtual TranOperationResult Credit(ICCPayment payment, int? transactionId, string aCuryID, decimal? aAmount)
		{
			return GetPaymentProcessing().Credit(payment, transactionId, aCuryID, aAmount);
		}

		public virtual bool RecordAuthorization(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordAuthorization(payment, data);
		}

		public virtual bool RecordCapture(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordCapture(payment, data);
		}

		public virtual bool RecordPriorAuthorizedCapture(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordPriorAuthorizedCapture(payment, data);
		}

		public virtual bool RecordCaptureOnly(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordCaptureOnly(payment, data);
		}

		public virtual bool RecordVoid(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordVoid(payment, data);
		}

		public virtual bool RecordCredit(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordCredit(payment, data);
		}

		public virtual bool RecordUnknown(ICCPayment payment, TranRecordData data)
		{
			return GetPaymentProcessing().RecordUnknown(payment, data);
		}

		protected virtual CCPaymentProcessing GetPaymentProcessing()
		{
			if (Repository == null)
			{
				return new CCPaymentProcessing();
			}
			else
			{
				return new CCPaymentProcessing(Repository);
			}
		}
		#endregion
	}
}