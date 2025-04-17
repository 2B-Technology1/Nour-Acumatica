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
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Subcontracts.PO.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.PO;
using ScMessages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.CS.GraphExtensions
{
    public class NumberingMaintExt : PXGraphExtension<NumberingMaint>
    {
        [InjectDependency]
        public INumberingSequenceUsage NumberingSequenceUsage
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected virtual void _(Events.RowDeleting<Numbering> args)
        {
            NumberingSequenceUsage.CheckForNumberingUsage<POSetup, PoSetupExt.subcontractNumberingID>(
                args.Row, Base, ScMessages.SubcontractsPreferencesScreenName);
        }
    }
}