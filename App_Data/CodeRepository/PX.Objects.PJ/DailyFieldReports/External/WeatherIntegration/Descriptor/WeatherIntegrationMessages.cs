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

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor
{
    [PXLocalizable]
    public static class WeatherIntegrationMessages
    {
        public const string ToLoadWeatherConditionsMustBeSpecifiedForDailyFieldReport =
            "To load weather conditions {0} and {1} must be specified for the daily field report.";

        public const string ToLoadWeatherConditionsMustBeSpecifiedOnWeatherIntegrationSettingsTab =
            "To load weather conditions weather integration settings must be specified on Weather Integration" +
            " Settings tab (Project Management Preferences screen).";

        public const string WeatherLogIsAvailableIfSettingsAreEnabled =
		    "The Weather Log Details form is available if the Enable Weather Processing Log check box is selected on the Weather Services tab of the Project Management Preferences (PJ101000) form.";

		public const string ClearWeatherLogIsAvailableIfSettingsAreEnabled =
			"The Clear Weather Log form is available if the Enable Weather Processing Log check box is selected on the Weather Services tab of the Project Management Preferences (PJ101000) form.";

		public const string LoadWeatherConditionsIsAvailableIfSettingsAreEnabled =
			"The Load Weather Conditions form is available if the Enable Weather Service Integration for Daily Field Reports check box is selected on the Weather Services tab of the Project Management Preferences (PJ101000) form.";

        public const string WeatherApiServiceMustBeSelected = "Weather API Service must be selected.";
    }
}
