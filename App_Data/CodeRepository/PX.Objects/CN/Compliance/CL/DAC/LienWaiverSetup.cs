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
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;
using PX.Objects.CN.Compliance.CL.Graphs;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.Compliance.CL.DAC
{
	/// <summary>
	/// Represents the tenant-level compliance preferences record that
	/// contains settings for configuring the generation of lien waivers.
	/// The single record of this type is created and edited on the Compliance Preferences (CL301000) form 
	/// (which corresponds to the <see cref="ComplianceDocumentSetupMaint"/> graph).
	/// </summary> 
    [PXCacheName("Compliance Preferences")]
    [PXPrimaryGraph(typeof(ComplianceDocumentSetupMaint))]
    public class LienWaiverSetup : BaseCache, IBqlTable
    {
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Warn Users During AP Bill Entry")]
        public virtual bool? ShouldWarnOnBillEntry
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Warn Users During Bill Selection for Payment")]
        public virtual bool? ShouldWarnOnPayment
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Prevent AP Bill Payment")]
        public virtual bool? ShouldStopPayments
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.AutomaticallyGenerateLienWaivers)]
        public virtual bool? ShouldGenerateConditional
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.AutomaticallyGenerateLienWaivers)]
        public virtual bool? ShouldGenerateUnconditional
        {
            get;
            set;
        }

		[PXDBBool]
		[PXUIEnabled(typeof(Where<shouldGenerateConditional.IsEqual<True>>))]
		[PXDefault(false)]
		[PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.GenerateWithoutCommitment)]
		public virtual bool? GenerateWithoutCommitmentConditional
		{
			get;
			set;
		}

		[PXDBBool]
		[PXUIEnabled(typeof(Where<shouldGenerateUnconditional.IsEqual<True>>))]
		[PXDefault(false)]
		[PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.GenerateWithoutCommitment)]
		public virtual bool? GenerateWithoutCommitmentUnconditional
		{
			get;
			set;
		}

        [PXDBString]
        [PXDefault(LienWaiverGenerationEvent.PayingBill, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverGenerationEvent.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.GenerateLienWaiversOn, Enabled = false)]
        public virtual string GenerationEventConditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXDefault(LienWaiverGenerationEvent.PayingBill, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverGenerationEvent.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.GenerateLienWaiversOn, Enabled = false)]
        public virtual string GenerationEventUnconditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXUIEnabled(typeof(Where<shouldGenerateConditional.IsEqual<True>>))]
        [PXDefault(LienWaiverThroughDateSource.PostingPeriodEndDate, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverThroughDateSource.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.ThroughDate)]
        public virtual string ThroughDateSourceConditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXUIEnabled(typeof(Where<shouldGenerateUnconditional.IsEqual<True>>))]
        [PXDefault(LienWaiverThroughDateSource.PaymentDate, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverThroughDateSource.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.ThroughDate)]
        public virtual string ThroughDateSourceUnconditional
        {
            get;
            set;
        }

		[PXDBString(10)]
		[PXUIEnabled(typeof(Where<shouldGenerateConditional.IsEqual<True>>))]
		[PXDefault(LienWaiverGroupBySource.CommitmentProject)]
		[LienWaiverGroupBySource.List]
		[PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.GroupBy)]
		public virtual string GroupByConditional
		{
			get;
			set;
		}

		[PXDBString(10)]
		[PXUIEnabled(typeof(Where<shouldGenerateUnconditional.IsEqual<True>>))]
		[PXDefault(LienWaiverGroupBySource.CommitmentProject)]
		[LienWaiverGroupBySource.List]
		[PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.GroupBy)]
		public virtual string GroupByUnconditional
		{
			get;
			set;
		}

        public abstract class shouldWarnOnBillEntry : BqlBool.Field<shouldWarnOnBillEntry>
        {
        }

        public abstract class shouldWarnOnPayment : BqlBool.Field<shouldWarnOnPayment>
        {
        }

        public abstract class shouldStopPayments : BqlBool.Field<shouldStopPayments>
        {
        }

        public abstract class shouldGenerateConditional : BqlBool.Field<shouldGenerateConditional>
        {
        }

        public abstract class shouldGenerateUnconditional : BqlBool.Field<shouldGenerateUnconditional>
        {
        }
		public abstract class generateWithoutCommitmentConditional : BqlBool.Field<generateWithoutCommitmentConditional>
		{
		}

		public abstract class generateWithoutCommitmentUnconditional : BqlBool.Field<generateWithoutCommitmentUnconditional>
		{
		}

        public abstract class generationEventConditional : BqlString.Field<generationEventConditional>
        {
        }

        public abstract class generationEventUnconditional : BqlString.Field<generationEventUnconditional>
        {
        }

        public abstract class throughDateSourceConditional : BqlString.Field<throughDateSourceConditional>
        {
        }

        public abstract class throughDateSourceUnconditional : BqlString.Field<throughDateSourceUnconditional>
        {
        }

		public abstract class groupByConditional : BqlString.Field<groupByConditional>
		{
		}

		public abstract class groupByUnconditional : BqlString.Field<groupByUnconditional>
		{
		}
    }
}
