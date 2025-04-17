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

using PX.Data;
using PX.Objects.CA;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.SM;
using CaMessages = PX.Objects.CA.Messages;
using Constants = PX.Objects.CN.Common.Descriptor.Constants;
using urlReports = PX.Objects.Common.urlReports;

namespace PX.Objects.CN.Subcontracts.SC.DAC
{
    [PXProjection(typeof(Select<NotificationSetup,
        Where<module, Equal<PXModule.sc>>>), Persistent = true)]
    [PXCacheName("Subcontract Notification")]
    public class SubcontractNotification : NotificationSetup
    {
        [PXDBString(2, IsFixed = true, IsKey = true)]
        [PXDefault(PXModule.Sc)]
        public override string Module
        {
            get;
            set;
        }

        [PXDefault(PONotificationSource.Vendor)]
        [PXDBString(10, IsKey = true, InputMask = "")]
        public override string SourceCD
        {
            get;
            set;
        }

        [PXDBString(8, InputMask = "CC.CC.CC.CC")]
        [PXUIField(DisplayName = Constants.Report)]
        [PXSelector(typeof(Search<SiteMap.screenID,
                Where<SiteMap.screenID, Like<PXModule.sc_>,
                    And<SiteMap.url, Like<urlReports>>>,
                OrderBy<Asc<SiteMap.screenID>>>),
            typeof(SiteMap.screenID),
            typeof(SiteMap.title),
            Headers = new[]
            {
                CaMessages.ReportID,
                CaMessages.ReportName
            },
            DescriptionField = typeof(SiteMap.title))]
        public override string ReportID
        {
            get;
            set;
        }

        public new abstract class setupID : IBqlField
        {
        }
    }
}