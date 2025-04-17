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
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Payroll.Proxy;
using PX.Payroll.Data;
using System.IO;

namespace PX.Objects.PR
{
	public static class TaxUpdateHelpers
	{
		public static bool CheckTaxUpdateTimestamp(PXView updateHistoyView)
		{
			PRTaxUpdateHistory updateHistory = updateHistoyView.SelectSingle() as PRTaxUpdateHistory;
			DateTime utcNow = DateTime.UtcNow;

			if (PXAccess.FeatureInstalled<FeaturesSet.payrollUS>() &&
				CheckTaxUpdateTimestamp<PRTaxUpdateHistory.serverTaxDefinitionTimestamp>(updateHistoyView.Cache, updateHistory, utcNow, LocationConstants.USCountryCode))
			{
				return true;
			}
			if (PXAccess.FeatureInstalled<FeaturesSet.payrollCAN>() && 
				CheckTaxUpdateTimestamp<PRTaxUpdateHistory.serverCanadaTaxDefinitionTimestamp>(updateHistoyView.Cache, updateHistory, utcNow, LocationConstants.CanadaCountryCode))
			{
				return true;
			}

			return false;
		}

		private static bool CheckTaxUpdateTimestamp<TTimestampField>(PXCache cache, PRTaxUpdateHistory updateHistory, DateTime utcNow, string country)
			where TTimestampField : IBqlField
		{
			DateTime? serverTimestamp = updateHistory != null ? cache.GetValue<TTimestampField>(updateHistory) as DateTime? : null;
			if (updateHistory != null &&
				(serverTimestamp == null || updateHistory.LastCheckTime == null || updateHistory.LastCheckTime < utcNow.AddDays(-1)))
			{
				try
				{
					serverTimestamp = GetTaxDefinitionTimestamp(country);
					cache.SetValue<TTimestampField>(updateHistory, serverTimestamp);
					updateHistory.LastCheckTime = utcNow;
					cache.Update(updateHistory);

					using (PXTransactionScope ts = new PXTransactionScope())
					{
						cache.PersistUpdated(updateHistory);
						ts.Complete();
					}
					cache.Persisted(false);
				}
				catch { }
			}

			return !(updateHistory == null || updateHistory.LastUpdateTime < serverTimestamp) ;
		}

		private static DateTime GetTaxDefinitionTimestamp(string country)
		{
			if (country == LocationConstants.USCountryCode)
			{
				return new PayrollUpdateClient().GetTaxDefinitionTimestamp();
			}
			else
			{
				return new PRWebServiceRestClient().GetTaxDefinitionTimestamp(country);
			}
		}

		[Serializable]
		[PXHidden]
		public class UpdateTaxesWarning : IBqlTable
		{
			#region Message
			public abstract class message : BqlString.Field<message> { }
			[PXString]
			public string Message { get; set; }
			#endregion
		}
	}
}
