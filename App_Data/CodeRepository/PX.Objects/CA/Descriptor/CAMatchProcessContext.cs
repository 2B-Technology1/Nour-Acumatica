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
using System;
using System.Runtime.Serialization;

namespace PX.Objects.CA
{
	public class CAMatchProcessContext : IDisposable
	{
		public class CashAccountLockedException : PXException
		{
			public CashAccountLockedException(SerializationInfo info, StreamingContext context)
					: base(info, context)
			{

			}

			public CashAccountLockedException(string message, params object[] prms) : base(message, prms)
			{

			}
		}


		private int? CashAccountID { get; set; }

		public CAMatchProcessContext(CABankMatchingProcess graph, int? cashAccountID, Guid? processorID)
		{
			CashAccountID = cashAccountID;
			VerifyRunningProcess(graph, cashAccountID);
			InsertMatchInfo(processorID, cashAccountID);
		}

		private static void VerifyRunningProcess(CABankMatchingProcess graph, int? cashAccountID)
		{
			var runnungProcess = graph.MatchProcessSelect.SelectSingle(cashAccountID);

			if (runnungProcess?.CashAccountID == null)
			{
				return;
			}

			var keyGUID = runnungProcess.ProcessUID ?? new Guid();
			if (PXLongOperation.GetStatus(keyGUID) == PXLongRunStatus.InProcess)
			{
				var cashAccount = graph.cashAccount.SelectSingle(cashAccountID);
				throw new CashAccountLockedException(Messages.CashAccountIsInMatchingProcess, cashAccount.CashAccountCD);
			}

			DeleteMatchInfo(cashAccountID);
		}

		private static void InsertMatchInfo(Guid? processUID, int? cashAccountID)
		{
			PXDatabase.Insert<CAMatchProcess>(
				new PXDataFieldAssign<CAMatchProcess.processUID>(processUID),
				new PXDataFieldAssign<CAMatchProcess.cashAccountID>(cashAccountID),
				new PXDataFieldAssign<CAMatchProcess.operationStartDate>(PXTimeZoneInfo.Now),
				new PXDataFieldAssign<CAMatchProcess.startedByID>(PXAccess.GetUserID()));
		}

		private static void DeleteMatchInfo(int? cashAccountID)
		{
			PXDatabase.Delete<CAMatchProcess>(new PXDataFieldRestrict<CAMatchProcess.cashAccountID>(PXDbType.Int, cashAccountID));
		}

		public void Dispose()
		{
			DeleteMatchInfo(CashAccountID);
		}
	}
}
