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
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.GL;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
    public abstract class DocumentWithLinesGraphExtension<TGraph> : DocumentWithLinesGraphExtension<TGraph, Document, DocumentMapping>
        where TGraph : PXGraph
    {

    }

    public abstract class DocumentWithLinesGraphExtension<TGraph, TDocument, TDocumentMapping> : PXGraphExtension<TGraph>,
        IDocumentWithFinDetailsGraphExtension
        where TGraph : PXGraph 
        where TDocument : Document, new()
        where TDocumentMapping: IBqlMapping
    {
        protected abstract TDocumentMapping GetDocumentMapping();

        protected abstract DocumentLineMapping GetDocumentLineMapping();

        /// <summary>A mapping-based view of the <see cref="Document" /> data.</summary>
        public PXSelectExtension<TDocument> Documents;
        /// <summary>A mapping-based view of the <see cref="DocumentLine" /> data.</summary>
        public PXSelectExtension<DocumentLine> Lines;

        public List<int?> GetOrganizationIDsInDetails()
        {
            return Lines.Select()
                .AsEnumerable()
                .Select(row => PXAccess.GetParentOrganizationID(((DocumentLine) row).BranchID))
                .Distinct()
                .ToList();
        }

        protected virtual void _(Events.RowUpdated<TDocument> e)
        {
            if (ShouldUpdateLinesOnDocumentUpdated(e))
            {
                foreach (DocumentLine line in Lines.Select())
                {
                    ProcessLineOnDocumentUpdated(e, line);

                    (e.Cache as PXModelExtension<DocumentLine>)?.UpdateExtensionMapping(line);

                    Lines.Cache.MarkUpdated(line);
                }
            }
        }

        protected virtual bool ShouldUpdatePeriodOnDocumentUpdated(Events.RowUpdated<TDocument> e)
        {
            return !e.Cache.ObjectsEqual<Document.headerTranPeriodID>(e.Row, e.OldRow);
        }

        protected virtual bool ShouldUpdateDetailsOnDocumentUpdated(Events.RowUpdated<TDocument> e)
        {
            return ShouldUpdatePeriodOnDocumentUpdated(e);
        }

        protected virtual bool ShouldUpdateLinesOnDocumentUpdated(Events.RowUpdated<TDocument> e)
        {
            return ShouldUpdateDetailsOnDocumentUpdated(e);
        }

        protected virtual void ProcessLineOnDocumentUpdated(Events.RowUpdated<TDocument> e, DocumentLine line)
        {
            if (ShouldUpdatePeriodOnDocumentUpdated(e))
            {
                FinPeriodIDAttribute.DefaultPeriods<DocumentLine.finPeriodID>(Lines.Cache, line);
            }
        }
    }
}
