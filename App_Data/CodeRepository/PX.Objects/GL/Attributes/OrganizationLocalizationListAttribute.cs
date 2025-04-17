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
using System.Globalization;
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Data.Localization;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CR;
using static PX.Data.PXAccess;

namespace PX.Objects.GL.Attributes
{
	public class OrganizationLocalizationListAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
	{
		[InjectDependencyOnTypeLevel]
		protected ILocalizationFeaturesService LocalizationFeaturesService { get; set; }

		public override void CacheAttached(PXCache sender)
		{
			List<(string, string)> localizations = new List<(string, string)>()
				{ (PX.Data.Localization.Constants.StandardLocalizationCode,
					PX.Data.Localization.Constants.StandardLocalizationName) };
			var enabledLocalizations = LocalizationFeaturesService.GetEnabledLocalizations();
			if (enabledLocalizations != null)
			{
				localizations.AddRange(enabledLocalizations.Select(s =>
				{
					try
					{
						RegionInfo info = new RegionInfo(s);
						return (s, info.DisplayName);
					}
					catch (Exception e)
					{
						throw new PXException(e, Messages.IncorrectCountryCodeinFeature, s);
					}
				}));
			}

			int i = 0;
			_AllowedValues = new string[localizations.Count];
			_AllowedLabels = new string[localizations.Count];

			foreach (var (code, name) in localizations)
			{
				//temporary workaround until GB localization is made consistent with Company-level feature
				if (!string.Equals(code, "GB"))
				{
					_AllowedValues[i] = code;
					_AllowedLabels[i] = name;
					i++;
				}
			}

			if (i < localizations.Count)
			{
				Array.Resize(ref _AllowedValues, i);
				Array.Resize(ref _AllowedLabels, i);
			}

			base.CacheAttached(sender);
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (_AllowedValues != null && _AllowedValues.Length > 1)
			{
				PXUIFieldAttribute.SetEnabled<Organization.organizationLocalizationCode>(sender, e.Row, true);
				PXUIFieldAttribute.SetVisible<Organization.organizationLocalizationCode>(sender, e.Row, true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<Organization.organizationLocalizationCode>(sender, e.Row, false);
				PXUIFieldAttribute.SetVisible<Organization.organizationLocalizationCode>(sender, e.Row, false);
			}
		}
	}
}
