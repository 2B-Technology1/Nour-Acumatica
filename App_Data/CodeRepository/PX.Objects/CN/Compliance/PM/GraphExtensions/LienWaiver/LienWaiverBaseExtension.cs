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

using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.PM.CacheExtensions;
using PX.Objects.CN.Compliance.PM.DAC;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.GraphExtensions.LienWaiver
{
    public abstract class LienWaiverBaseExtension<TGraph> : PXGraphExtension<TGraph>
        where TGraph : PXGraph
    {
		public PXSetup<LienWaiverSetup> LienWaiverSetup;

        public SelectFrom<LienWaiverRecipient>.
            Where<LienWaiverRecipient.projectId.IsEqual<PMProject.contractID.FromCurrent>>.View LienWaiverRecipients;

        public PXAction<PMProject> AddAllVendorClasses;

        [PXUIField(DisplayName = "Add all vendor classes")]
        [PXButton]
        public virtual void addAllVendorClasses()
        {
            var vendorClasses = GetAvailableVendorClasses();
            var recipients = vendorClasses.Select(vc => new LienWaiverRecipient
            {
                VendorClassId = vc.VendorClassID
            });
            LienWaiverRecipients.Cache.InsertAll(recipients);
        }

        public virtual void _(Events.RowSelected<PMProject> args)
        {
            var project = args.Row;
            if (project != null)
            {
                UpdateComplianceSettingsTabVisibility(args.Cache, project);
            }
        }

        private void UpdateComplianceSettingsTabVisibility(PXCache cache, PMProject project)
        {
            var shouldShowComplianceSettings = LienWaiverSetup.Current.ShouldGenerateConditional == true ||
                LienWaiverSetup.Current.ShouldGenerateUnconditional == true;
            PXUIFieldAttribute.SetVisible<PmProjectExtension.throughDateSourceConditional>(cache, project,
                shouldShowComplianceSettings);
            PXUIFieldAttribute.SetVisible<PmProjectExtension.throughDateSourceUnconditional>(cache, project,
                shouldShowComplianceSettings);
            LienWaiverRecipients.AllowSelect = shouldShowComplianceSettings;
        }

        private IEnumerable<VendorClass> GetAvailableVendorClasses()
        {
            var vendorClassIds = LienWaiverRecipients.SelectMain().Select(r => r.VendorClassId);
            return SelectFrom<VendorClass>
                .LeftJoin<EPEmployeeClass>.On<EPEmployeeClass.vendorClassID.IsEqual<VendorClass.vendorClassID>>
                .Where<EPEmployeeClass.vendorClassID.IsNull>
                .View.Select(Base).FirstTableItems.Where(vc => vc.VendorClassID.IsNotIn(vendorClassIds));
        }
    }
}
