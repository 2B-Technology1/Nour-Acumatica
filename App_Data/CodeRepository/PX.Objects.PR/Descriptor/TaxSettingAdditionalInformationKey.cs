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

using PX.Payroll.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class TaxSettingAdditionalInformationKey : IEquatable<TaxSettingAdditionalInformationKey>
	{
		public const string StateFallback = "DEF";

		public string SettingName { get; set; }
		public string State { get; set; }
		public string CountryID { get; set; }

		public TaxSettingAdditionalInformationKey(PRTaxSettingAdditionalInformation dbRecord)
		{
			SettingName = dbRecord.SettingName;
			State = dbRecord.State == StateFallback ? string.Empty : dbRecord.State;
			CountryID = dbRecord.CountryID;
		}

		public TaxSettingAdditionalInformationKey(TaxSettingDescription csvRecord)
		{
			SettingName = csvRecord.SettingName;
			State = csvRecord.State;
			CountryID = csvRecord.CountryID;
		}

		public TaxSettingAdditionalInformationKey(IPRSetting setting, bool checkState)
		{
			SettingName = setting.SettingName;
			CountryID = PRCountryAttribute.GetPayrollCountry();
			if (checkState && setting is IStateSpecific stateSpecific)
			{
				State = stateSpecific.State;
			}
		}

		public bool Equals(TaxSettingAdditionalInformationKey other)
		{
			return SettingName == other.SettingName
				&& (State == other.State || string.IsNullOrEmpty(State) && string.IsNullOrEmpty(other.State))
				&& CountryID == other.CountryID;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + SettingName.GetHashCode();
				hash = hash * 23 + (string.IsNullOrEmpty(State) ? 0 : State.GetHashCode());
				hash = hash * 23 + CountryID.GetHashCode();
				return hash;
			}
		}
	}
}
