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
using V2 = PX.CCProcessingBase.Interfaces.V2;
using V1 = PX.CCProcessingBase;
using PX.Data;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class V2SettingsGenerator
	{
		private Repositories.ICardProcessingReadersProvider _provider;

		public V2SettingsGenerator(Repositories.ICardProcessingReadersProvider provider)
		{
			_provider = provider;
		}

		public IEnumerable<V2.SettingsValue> GetSettings()
		{
			Dictionary<string, string> settingsDict = new Dictionary<string, string>();
			_provider.GetProcessingCenterSettingsStorage().ReadSettings(settingsDict);
			List<V2.SettingsValue> result = new List<V2.SettingsValue>();
			foreach (var setting in settingsDict)
			{
				V2.SettingsValue newSetting = new V2.SettingsValue() { DetailID = setting.Key, Value = setting.Value };
				result.Add(newSetting);
			}
			return result;
		}
	}
}
