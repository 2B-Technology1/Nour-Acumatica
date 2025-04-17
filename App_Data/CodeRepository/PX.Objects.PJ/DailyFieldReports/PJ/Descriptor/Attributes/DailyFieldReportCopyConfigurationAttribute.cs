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

using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    [PXDBBool]
    public class DailyFieldReportCopyConfigurationAttribute : PXAggregateAttribute, IPXRowUpdatedSubscriber
    {
        public DailyFieldReportCopyConfigurationAttribute(string displayName, bool defaultValue)
        {
            var uiFieldAttribute = new PXUIFieldAttribute
            {
                DisplayName = displayName
            };
            var defaultAttribute = new PXDefaultAttribute(defaultValue)
            {
                PersistingCheck = PXPersistingCheck.Nothing
            };
            var uiEnabledAttribute = new PXUIEnabledAttribute(
                typeof(DailyFieldReportCopyConfiguration.isConfigurationEnabled.IsEqual<True>));
            _Attributes.Add(uiFieldAttribute);
            _Attributes.Add(defaultAttribute);
            _Attributes.Add(uiEnabledAttribute);
        }

        public bool IsRequired
        {
            get;
            set;
        }

        public void RowUpdated(PXCache cache, PXRowUpdatedEventArgs args)
        {
            if (args.Row is DailyFieldReportCopyConfiguration copyConfiguration && IsRequired)
            {
                var fieldValue = cache.GetValue(args.Row, FieldName) as bool?;
                if (fieldValue == false && copyConfiguration.IsConfigurationEnabled == true)
                {
                    cache.RaiseException(FieldName, copyConfiguration,
                        DailyFieldReportMessages.ThisIsRequiredSettingForDailyFieldReport, null, PXErrorLevel.Warning);
                }
                else
                {
                    cache.RaiseExceptionHandling(FieldName, copyConfiguration, null, null);
                }
            }
        }
    }
}