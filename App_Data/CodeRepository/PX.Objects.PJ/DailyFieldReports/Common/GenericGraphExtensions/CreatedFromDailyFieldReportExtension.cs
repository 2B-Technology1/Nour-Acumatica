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

using System.Linq;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common.Extensions;

namespace PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions
{
    public abstract class CreatedFromDailyFieldReportExtension<TGraph> : PXGraphExtension<TGraph>
        where TGraph : PXGraph
    {
        public PXSelectExtension<DailyFieldReportRelatedDocument> Documents;

        public PXSelectExtension<DailyFieldReportRelation> Relations;

        protected abstract string EntityName
        {
            get;
        }

        public override void Initialize()
        {
            Relations = CreateRelationsExtension();
        }

        public virtual void _(Events.RowPersisted<DailyFieldReportRelatedDocument> args)
        {
            var document = args.Row;
            if (document == null || args.TranStatus != PXTranStatus.Open)
            {
                return;
            }
            if (document.DailyFieldReportId != null)
            {
                var relation = CreateDailyFieldReportRelation(document.DailyFieldReportId);
                Relations.Cache.Insert(relation);
                document.DailyFieldReportId = null;
            }
        }

        public virtual void _(Events.RowSelected<DailyFieldReportRelatedDocument> args)
        {
            if (args.Row is DailyFieldReportRelatedDocument document &&
                args.Cache.GetEnabled<DailyFieldReportRelatedDocument.projectId>(document))
            {
                var isEditable = IsProjectEditable();
                PXUIFieldAttribute.SetEnabled<DailyFieldReportRelatedDocument.projectId>(args.Cache, document, isEditable);
            }
        }

        public virtual void _(Events.RowDeleting<DailyFieldReportRelatedDocument> args)
        {
            var hasRelatedDailyFieldReport = Relations.Select().Any();
            if (hasRelatedDailyFieldReport)
            {
                var message = string.Format(DailyFieldReportMessages.EntityCannotBeDeletedBecauseItIsLinked,
                    EntityName.Capitalize());
                throw new PXException(message);
            }
        }

        public bool IsProjectEditable()
        {
            var hasRelatedDailyFieldReport = Relations.Select().Any();
            var isCreatedFromDailyFieldReport = IsCreatedFromDailyFieldReport();
            return !hasRelatedDailyFieldReport && !isCreatedFromDailyFieldReport;
        }

        protected abstract DailyFieldReportRelatedDocumentMapping GetDailyFieldReportMapping();

        protected abstract DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping();

        protected abstract PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension();

        /// <summary>
        /// You should override this method in case of Base DAC key field isn't of string type.
        /// </summary>
        protected virtual DailyFieldReportRelation CreateDailyFieldReportRelation(int? dailyFieldReportId)
        {
            return new DailyFieldReportRelation
            {
                RelationNumber = Documents.Current.ReferenceNumber,
                DailyFieldReportId = dailyFieldReportId
            };
        }

        private bool IsCreatedFromDailyFieldReport()
        {
            return Documents.Cache.GetStatus(Documents.Current) == PXEntryStatus.Inserted &&
                Documents.Current.DailyFieldReportId != null;
        }
    }
}
