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
using System.Linq;
using PX.Data;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote
{
    public class ComplianceDocumentEntityHelper
    {
        private static readonly ComplianceDocumentEntityStrategy[] Strategies =
        {
            new ApInvoiceStrategy(),
            new ApPaymentStrategy(),
            new ArInvoiceStrategy(),
            new ArPaymentStrategy(),
            new PoOrderStrategy(),
            new PmRegisterStrategy()
        };

        private readonly Type itemType;
        private readonly ComplianceDocumentEntityStrategy complianceDocumentEntityStrategy;

        public ComplianceDocumentEntityHelper(Type itemType)
        {
            this.itemType = itemType;
            complianceDocumentEntityStrategy = Strategies.Single(x => x.EntityType == itemType);
        }

        public bool IsStrategyExist => complianceDocumentEntityStrategy != null;

        public PXView CreateView(PXGraph graph)
        {
            var command = BqlCommand.CreateInstance(typeof(Select<>), itemType);
            if (IsStrategyExist)
            {
                if (complianceDocumentEntityStrategy.FilterExpression != null)
                {
                    command = command.WhereNew(complianceDocumentEntityStrategy.FilterExpression);
                }
            }
            return new PXView(graph, true, command);
        }

        public Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            return complianceDocumentEntityStrategy.GetNoteId(graph, clDisplayName);
        }
    }
}
