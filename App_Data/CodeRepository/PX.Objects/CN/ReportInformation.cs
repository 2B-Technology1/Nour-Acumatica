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
using PX.Data.BQL.Fluent;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.CN
{
	[PXCacheName("Report Information")]
	public class ReportInformation : IBqlTable
	{
		[AnyInventory(typeof(Search<InventoryItem.inventoryID, Where2<Match<Current<AccessInfo.userName>>,
				And<InventoryItem.stkItem, Equal<False>,
					And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
						And<Where<InventoryItem.itemClassID, Equal<Optional<FilterItemByClass.itemClassID>>,
							Or<Optional<FilterItemByClass.itemClassID>, IsNull>>>>>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))]
		public virtual int? InventoryIdNonStock
		{
			get;
			set;
		}

		[PXInt]
		[PXSelector(typeof(Search<PMProject.contractID,
				Where<PMProject.status, In3<ProjectStatus.active, planned>>>),
			typeof(PMProject.contractCD),
			typeof(PMProject.description),
			typeof(PMProject.status),
			typeof(PMProject.ownerID),
			SubstituteKey = typeof(PMProject.contractCD))]
		public virtual int? ProjectId
		{
			get;
			set;
		}

		[PXInt]
		[PXSelector(typeof(Search5<EPEmployee.bAccountID,
				LeftJoin<PMProject, On<PMProject.ownerID, Equal<EPEmployee.defContactID>>>,
				Where<PMProject.contractID, Equal<Optional<projectId>>,
					Or<Optional<projectId>, IsNull>>,
				Aggregate<GroupBy<EPEmployee.bAccountID>>>),
			SubstituteKey = typeof(EPEmployee.acctCD))]
		public virtual int? ProjectManagerId
		{
			get;
			set;
		}

		[Project(typeof(Where<PMProject.baseType, Equal<CTPRType.project>>))]
		public virtual int? BudgetForecastProjectId
		{
			get;
			set;
		}

		[PXString]
		[PXSelector(
			typeof(SelectFrom<PMForecast>
				.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PMForecast.projectID>>
				.Where<
					PMForecast.projectID.IsEqual<budgetForecastProjectId.AsOptional>
					.And<MatchUserFor<PMProject>>>
				.OrderBy<PMForecast.revisionID.Desc>
				.SearchFor<PMForecast.revisionID>))]
		public virtual string RevisionId
		{
			get;
			set;
		}

		public abstract class projectId : IBqlField
		{
		}

		public abstract class projectManagerId : IBqlField
		{
		}

		public abstract class budgetForecastProjectId : BqlInt.Field<budgetForecastProjectId>
		{
		}

		public abstract class revisionId : BqlString.Field<revisionId>
		{
		}

		public class planned : BqlType<IBqlString, string>.Constant<planned>
		{
			public planned()
				: base(ProjectStatus.Planned)
			{
			}
		}
	}
}
