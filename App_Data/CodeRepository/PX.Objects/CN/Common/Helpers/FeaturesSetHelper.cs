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
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CN.Common.Helpers
{
	public static class FeaturesSetHelper
	{
		private const string FeatureRequiredError = "Feature '{0}' should be enabled";
		private const string ProcoreIntegrationFeatureName = "Procore Integration";
		private const string ConstructionFeatureName = "Construction";
		private const string ProjectManagementFeatureName = "Construction Project Management";

		public static void CheckProcoreIntegrationFeature()
		{
			CheckFeature<FeaturesSet.procoreIntegration>(ProcoreIntegrationFeatureName);
		}

		public static void CheckConstructionFeature()
		{
			CheckFeature<FeaturesSet.construction>(ConstructionFeatureName);
		}

		private static void CheckFeature<TFeature>(string featureName)
			where TFeature : IBqlField
		{
			if (!PXAccess.FeatureInstalled<TFeature>())
			{
				throw new Exception(string.Format(FeatureRequiredError, featureName));
			}
		}
	}
}