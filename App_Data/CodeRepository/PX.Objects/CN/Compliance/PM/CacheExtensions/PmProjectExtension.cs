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
using PX.Data.BQL;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.CacheExtensions
{
    public sealed class PmProjectExtension : PXCacheExtension<PMProject>
    {
        [PXDBString]
        [PXDefault(typeof(LienWaiverSetup.throughDateSourceConditional), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(Where<LienWaiverSetup.shouldGenerateConditional.FromCurrent.IsEqual<True>>))]
        [LienWaiverThroughDateSource.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.ThroughDate)]
        public string ThroughDateSourceConditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXDefault(typeof(LienWaiverSetup.throughDateSourceUnconditional), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(Where<LienWaiverSetup.shouldGenerateUnconditional.FromCurrent.IsEqual<True>>))]
        [LienWaiverThroughDateSource.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.ThroughDate)]
        public string ThroughDateSourceUnconditional
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class throughDateSourceConditional : BqlString.Field<throughDateSourceConditional>
        {
        }

        public abstract class throughDateSourceUnconditional : BqlString.Field<throughDateSourceUnconditional>
        {
        }
    }
}
