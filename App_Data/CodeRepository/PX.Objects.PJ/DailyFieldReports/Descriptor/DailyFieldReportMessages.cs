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

namespace PX.Objects.PJ.DailyFieldReports.Descriptor
{
    [PXLocalizable]
    public static class DailyFieldReportMessages
    {
        public const string ThereAreRelatedEntitiesToDfrOnDelete = "This daily field report cannot be deleted because it has at least one related {0}.";
        public const string DepartureTimeMustBeLaterThanArrivalTime = "The departure time must be later than the arrival time.";
        public const string ValueMustBePositive = "Value must be positive.";
        public const string WorkingHoursCannotExceedDefaultValue = "Working Hours cannot exceed {0}.";
        public const string EntityCannotBeSelectedTwice = "You have already selected this {0}.";

        public const string DfrDateMustNotBeEarlierThenProjectStartDate =
            "The [date] must not be earlier than the project start date.";

        public const string ThereIsOneOrMoreEntitiesRelatedToTheProjectOnTheTab =
            "The project cannot be changed because at least one {0} is linked to this project on the {1} tab.";

        public const string ThisEquipmentIsAssociatedWithSubmittedTimeCard =
            "This equipment utilization record is associated with submitted time card. Use the Equipment Time Card form to make changes to this record.";

        public const string EntityCannotBeDeletedBecauseItIsLinked =
            "{0} cannot be deleted because it is linked to the daily field report.";

        public const string TheFileIsReferredToTheDailyFieldReport =
            "The file is referred to the Daily Field Report and cannot be deleted.";

        public const string ThisIsRequiredSettingForDailyFieldReport =
            "This is a required setting for daily field reports. A user will have to fill it in manually for a new copy of the document.";

		public const string NoChangeDateByProgressWorksheet =
			"The daily field report is linked to the progress worksheet and cannot be edited. To be able to change the date, assign the On Hold status to the progress worksheet.";

		public const string NoDeleteProgressWorksheetLine =
			"The daily field report is linked to the progress worksheet and cannot be edited. To be able to delete the line, assign the On Hold status to the progress worksheet.";

		public const string CompleteDFRBeforeReleasingTimeActivity =
			"The time activity cannot be released because it is linked to the daily field report that has the {0} status. To be able to release the time activity, complete the {1} daily field report.";
	}
}
