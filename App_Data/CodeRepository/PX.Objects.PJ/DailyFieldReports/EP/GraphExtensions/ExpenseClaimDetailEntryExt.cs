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

using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.EP.GraphExtensions
{
    public class ExpenseClaimDetailEntryExt : CreatedFromDailyFieldReportExtension<ExpenseClaimDetailEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportEmployeeExpense>
            .Where<DailyFieldReportEmployeeExpense.employeeExpenseId.IsEqual<
                EPExpenseClaimDetails.claimDetailCD.FromCurrent>>
            .View DailyFieldReportEmployeeExpense;

        protected override string EntityName => DailyFieldReportEntityNames.EmployeeExpense;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        /// <summary>
        /// Project availabitily logic moved to <see cref="ExpenseClaimDetailEntryProjectAvailabilityExt" />
        /// because another Acumatica Graph Extension overrides our logic.
        /// </summary>
        public override void _(Events.RowSelected<DailyFieldReportRelatedDocument> args)
        {
        }

        protected override DailyFieldReportRelatedDocumentMapping GetDailyFieldReportMapping()
        {
            return new DailyFieldReportRelatedDocumentMapping(typeof(EPExpenseClaimDetails))
            {
                ReferenceNumber = typeof(EPExpenseClaimDetails.claimDetailCD),
                ProjectId = typeof(EPExpenseClaimDetails.contractID)
            };
        }

		public virtual void _(Events.RowPersisting<DailyFieldReportRelatedDocument> args)
		{
			if (args.Operation == PXDBOperation.Insert)
			{
				if (args.Row.DailyFieldReportId != null)
				{
					var relation = CreateDailyFieldReportRelation(args.Row.DailyFieldReportId);
					Relations.Cache.Insert(relation);
					args.Row.DailyFieldReportId = null;
				}
			}
		}

		public override void _(Events.RowPersisted<DailyFieldReportRelatedDocument> args)
		{
			
		}

		protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportEmployeeExpense))
            {
                RelationNumber = typeof(DailyFieldReportEmployeeExpense.employeeExpenseId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportEmployeeExpense);
        }
    }
}
