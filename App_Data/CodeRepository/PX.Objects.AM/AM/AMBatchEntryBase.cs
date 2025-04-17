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

namespace PX.Objects.AM
{
    /// <summary>
    /// Base graph with split/LS members
    /// </summary>
#pragma warning disable PX1018 // The graph with the specified primary DAC type does not contain a view of this type
    public abstract class AMBatchEntryBase : AMBatchSimpleEntryBase
#pragma warning restore PX1018
    {
        public abstract PXSelectBase<AMMTran> LSSelectDataMember { get; }
        public abstract PXSelectBase<AMMTranSplit> AMMTranSplitDataMember { get; }
    }

    /// <summary>
    /// Base graph without split/LS members
    /// (Implementation copy of INRegisterEntryBase)
    /// </summary>
#pragma warning disable PX1093 // Graph declaration should contain graph type as first type paramenter
#pragma warning disable PX1018 // The graph with the specified primary DAC type does not contain a view of this type
    public abstract class AMBatchSimpleEntryBase : PXGraph<PXGraph, AMBatch>
#pragma warning restore PX1093
#pragma warning restore PX1018
    {
        public PXSetup<AMPSetup> ampsetup;

        public abstract PXSelectBase<AMBatch> AMBatchDataMember { get; }
        public abstract PXSelectBase<AMMTran> AMMTranDataMember { get; }
    }
}
