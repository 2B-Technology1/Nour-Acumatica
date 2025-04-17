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
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CR.Extensions
{
	public abstract class ActivityDetailsExt_Child_Actions<TActivityDetailsExt, TGraph, TPrimaryEntity, TPrimaryEntity_NoteID>
		: ActivityDetailsExt_Actions<
			TActivityDetailsExt,
			TGraph,
			TPrimaryEntity,
			CRChildActivity,
			CRChildActivity.noteID>
		where TActivityDetailsExt : ActivityDetailsExt_Child<TGraph, TPrimaryEntity, TPrimaryEntity_NoteID>, IActivityDetailsExt
		where TGraph : PXGraph, new()
		where TPrimaryEntity : CRActivity, IBqlTable, INotable, new()
		where TPrimaryEntity_NoteID : IBqlField, IImplement<IBqlCastableTo<IBqlGuid>>
	{
	}

	public abstract class ActivityDetailsExt_Child<TGraph, TPrimaryEntity, TPrimaryEntity_NoteID>
		: ActivityDetailsExt<
			TGraph,
			TPrimaryEntity,
			CRChildActivity,
			CRChildActivity.noteID>
		where TGraph : PXGraph, new()
		where TPrimaryEntity : CRActivity, IBqlTable, INotable, new()
		where TPrimaryEntity_NoteID : IBqlField, IImplement<IBqlCastableTo<IBqlGuid>>
	{
		public override Type GetLinkConditionClause() => typeof(
			Where<
				CRChildActivity.parentNoteID, Equal<Current<TPrimaryEntity_NoteID>>,
				And<
					Where<CRChildActivity.isCorrected, NotEqual<True>,
						Or<CRChildActivity.isCorrected, IsNull>>>>);

		public override Type GetOrderByClause() => typeof(OrderBy<Desc<CRChildActivity.createdDateTime>>);

		public override Type GetClassConditionClause() => typeof(Where<CRChildActivity.classID, GreaterEqual<Zero>>);

		public override Type GetPrivateConditionClause() => PXSiteMap.IsPortal
			? typeof(Where<CRChildActivity.isPrivate.IsNull.Or<CRChildActivity.isPrivate.IsEqual<False>>>)
			: null;

		public override void CreateTimeActivity(PXGraph targetGraph, int classID, string activityType)
		{
			base.CreateTimeActivity(targetGraph, classID, activityType);

			var sourceTimeActivity = Base.Caches[typeof(PMTimeActivity)]?.Current as PMTimeActivity;

			var targetCache = targetGraph.Caches[typeof(PMTimeActivity)];
			var targetTimeActivity = targetCache?.Current as PMTimeActivity;

			if (sourceTimeActivity == null || targetTimeActivity == null)
				return;

			targetCache.SetValueExt<PMTimeActivity.projectID>(targetTimeActivity, sourceTimeActivity.ProjectID);
			targetCache.SetValueExt<PMTimeActivity.projectTaskID>(targetTimeActivity, sourceTimeActivity.ProjectTaskID);
			targetCache.SetValueExt<PMTimeActivity.costCodeID>(targetTimeActivity, sourceTimeActivity.CostCodeID);
		}

		public override void InitializeActivity(CRActivity row)
		{
			base.InitializeActivity(row);

			row.ParentNoteID ??= (Base.Caches[typeof(TPrimaryEntity)]?.Current as TPrimaryEntity)?.NoteID;
		}

		public override Guid? GetRefNoteID()
		{
			var primaryCache = Base.Caches[typeof(TPrimaryEntity)];
			var primaryEntity = primaryCache.Current as TPrimaryEntity;

			if (primaryEntity == null)
				return null;

			return primaryEntity.RefNoteID;
		}

		/// <summary>
		/// Check is any billable child time activity exist or not 
		/// </summary>
		/// <param name="parentNoteId"></param>
		/// <returns>true if exists, otherwise false</returns>
		public bool AnyBillableChildExists(object parentNoteId)
		{
			return SelectFrom<CRChildActivity>
				.Where<
					CRChildActivity.isBillable.IsEqual<True>
					.And<CRChildActivity.parentNoteID.IsEqual<@P.AsGuid>>>
				.View
				.Select(Base, parentNoteId)
				.Any();
		}

		/// <summary>
		/// Return time totals of children activities
		/// </summary>
		public (int timeSpent, int overtimeSpent, int timeBillable, int overtimeBillable)
			GetChildrenTimeTotals(object parentNoteId)
		{
			BqlCommand childActivitiescmd = this.Activities.View.BqlSelect.OrderByNew<BqlNone>();
			Type aggregate = BqlTemplate.FromType(
					typeof(Aggregate<
						Sum<CRChildActivity.timeSpent,
						Sum<CRChildActivity.overtimeSpent,
						Sum<CRChildActivity.timeBillable,
						Sum<CRChildActivity.overtimeBillable,
						GroupBy<CRChildActivity.parentNoteID>>>>>>))
				.ToType();
			childActivitiescmd = childActivitiescmd.AggregateNew(aggregate);

			var timeFields = new List<Type>()
			{
				typeof(CRChildActivity.timeSpent),
				typeof(CRChildActivity.overtimeSpent),
				typeof(CRChildActivity.timeBillable),
				typeof(CRChildActivity.overtimeBillable)
			};

			PXView childActivitiesView = new PXView(Base, true, childActivitiescmd);

			using (new PXFieldScope(childActivitiesView, timeFields, false))
			{
				CRChildActivity child = (childActivitiesView.SelectSingle(parentNoteId) as PXResult<CRChildActivity>);

				return (
					timeSpent: (child?.TimeSpent ?? 0),
					overtimeSpent: (child?.OvertimeSpent ?? 0),
					timeBillable: (child?.TimeBillable ?? 0),
					overtimeBillable: (child?.OvertimeBillable ?? 0)
				);
			}
		}
	}
}
