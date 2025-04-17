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
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Localization;
using PX.Objects.FA;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;

namespace PX.Objects.CS
{
	public static class OrganizationLocalizationHelper
	{
		public static CurrentLocalization GetCurrentLocalization(PXGraph graph)
		{
			return new CurrentLocalization(GetCurrentLocalizationCode(graph));
		}

		public static string GetCurrentLocalizationCode(PXGraph graph)
		{
			return GetCurrentLocalizationCode(graph.Views[graph.PrimaryView]);
		}

		public static CurrentLocalization GetCurrentLocalization(PXView primaryView)
		{
			return new CurrentLocalization(GetCurrentLocalizationCode(primaryView));
		}

		public static string GetCurrentLocalizationCode(PXView primaryView)
		{
			string branchField = primaryView.Cache.Fields
				.Find(s => string.Equals(s, "branchID", StringComparison.OrdinalIgnoreCase));
			if (!string.IsNullOrEmpty(branchField))
			{
				object branchIdObj = primaryView.Cache.GetValue(primaryView.Cache.Current, branchField);
				return GetCurrentLocalizationCodeForBranch(branchIdObj as int?);
			}

			string organizationField = primaryView.Cache.Fields
				.Find(s => string.Equals(s, "organizationID", StringComparison.OrdinalIgnoreCase));
			if (!string.IsNullOrEmpty(organizationField))
			{
				object orgIdObj = primaryView.Cache.GetValue(primaryView.Cache.Current, organizationField);
				return GetCurrentLocalizationCodeForOrg(orgIdObj as int?);
			}

			return Constants.StandardLocalizationCode;
		}

		public static string GetCurrentLocalizationCodeForBranch(int? branchId)
		{
			if (branchId != null)
			{
				PXAccess.Organization org = PXAccess.GetParentOrganization(branchId);
				if (!string.IsNullOrEmpty(org?.OrganizationLocalizationCode)) return org?.OrganizationLocalizationCode;
			}
			return Constants.StandardLocalizationCode;
		}

		public static string GetCurrentLocalizationCodeForOrg(int? organizationId)
		{
			if (organizationId != null)
			{
				PXAccess.Organization org = PXAccess.GetOrganizationByID(organizationId);
				if (!string.IsNullOrEmpty(org?.OrganizationLocalizationCode)) return org?.OrganizationLocalizationCode;
			}
			return Constants.StandardLocalizationCode;
		}
	}

	public static class LocalizationServiceExtensions
	{
		public static bool LocalizationEnabled<TFeature>(string localizationCode)
			where TFeature : IBqlField
		{
			return PXAccess.LocalizationEnabled<TFeature>(localizationCode);
		}

		public static bool LocalizationEnabled<TFeature>(PXView primaryView) where TFeature : IBqlField
		{
			string localizationCode = OrganizationLocalizationHelper.GetCurrentLocalizationCode(primaryView);
			return LocalizationEnabled<TFeature>(localizationCode);
		}

		public static bool LocalizationEnabled<TFeature>(PXGraph graph) where TFeature : IBqlField
		{
			string localizationCode = OrganizationLocalizationHelper.GetCurrentLocalizationCode(graph);
			return LocalizationEnabled<TFeature>(localizationCode);
		}
	}

	public class LocalizationFeatureScope : IDisposable
	{
		private readonly string _previousLocalizationCode;

		public LocalizationFeatureScope(PXGraph graph)
			: this(OrganizationLocalizationHelper.GetCurrentLocalizationCode(graph))
		{
		}

		public LocalizationFeatureScope(PXView primaryView)
			: this(OrganizationLocalizationHelper.GetCurrentLocalizationCode(primaryView))
		{
		}

		public LocalizationFeatureScope(string localizationCode)
		{
			CurrentLocalization CurrentLocalization =
				PXContext.GetSlot<CurrentLocalization>();

			if (!String.IsNullOrEmpty(CurrentLocalization?.LocalizationCode))
			{
				_previousLocalizationCode = CurrentLocalization.LocalizationCode;
			}

			PXContext.SetSlot(new CurrentLocalization(localizationCode));
		}

		public void Dispose()
		{
			if (!String.IsNullOrEmpty(_previousLocalizationCode))
			{
				PXContext.SetSlot(new CurrentLocalization(_previousLocalizationCode));
			}
			else
			{
				PXContext.ClearSlot<CurrentLocalization>();
			}
		}
	}
}
