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
using PX.Objects.CN.Common.Extensions;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;

namespace PX.Objects.PJ.ProjectManagement.PM.Services
{
    public abstract class ConversionServiceBase
    {
        protected ChangeRequestEntry Graph;

        protected ConversionServiceBase(ChangeRequestEntry graph)
        {
            Graph = graph;
        }

        public void ProcessConvertedChangeRequestIfRequired(PMChangeRequest changeRequest)
        {
            if (IsNewChangeRequest(changeRequest) && !Graph.HasErrors())
            {
                ProcessConvertedChangeRequest(changeRequest);
            }
        }

        public abstract void UpdateConvertedEntity(PMChangeRequest changeRequest);

        public abstract void SetFieldReadonly(PMChangeRequest changeRequest);

        protected abstract void ProcessConvertedChangeRequest(PMChangeRequest changeRequest);

        protected void CopyFilesToChangeRequest<TTable>(object row, PMChangeRequest changeRequest)
            where TTable : class, IBqlTable, new()
        {
            PXNoteAttribute.CopyNoteAndFiles(Graph.Caches<TTable>(), row, Graph.Document.Cache, changeRequest, false,
                true);
            Graph.Caches<NoteDoc>().Persist(PXDBOperation.Insert);
        }

        protected void SetFieldReadOnly<TField>(PMChangeRequest changeRequest)
            where TField : IBqlField
        {
            PXUIFieldAttribute.SetEnabled<TField>(Graph.Document.Cache, changeRequest, false);
        }

        private bool IsNewChangeRequest(PMChangeRequest changeRequest)
        {
            return Graph.Document.Cache.GetStatus(changeRequest) == PXEntryStatus.Inserted;
        }
    }
}
