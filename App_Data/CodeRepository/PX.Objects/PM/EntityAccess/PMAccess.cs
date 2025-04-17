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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.SM;

using MaintMessages = PX.SM.Messages;

namespace PX.Objects.PM
{
	public class PMAccessDetail : UserAccess
	{
		#region Select

		public SelectFrom<PMProject>
			.Where<PMProject.baseType.IsEqual<CT.CTPRType.project>
				.And<PMProject.nonProject.IsEqual<False>>>
			.View Project;

		protected override IEnumerable groups()
		{
			foreach (RelationGroup group in PXSelect<RelationGroup>.Select(this))
			{
				if (group.SpecificModule == null
					|| group.SpecificModule == typeof(PMProject).Namespace
					|| IsIncluded(getMask(), group))
				{
					Groups.Current = group;
					yield return group;
				}
			}
		}
		#endregion

		#region Actions
		public new PXSave<PMProject> Save;
		public new PXCancel<PMProject> Cancel;
		public new PXFirst<PMProject> First;
		public new PXPrevious<PMProject> Prev;
		public new PXNext<PMProject> Next;
		public new PXLast<PMProject> Last;
		#endregion

		#region Constructor
		public PMAccessDetail()
		{
			Project.Cache.AllowDelete = false;
			Project.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetRequired(Project.Cache, null, false);
			Views.Caches.Remove(Groups.GetItemType());
			Views.Caches.Add(Groups.GetItemType());
		}
		#endregion

		#region Runtime

		protected override byte[] getMask()
		{
			byte[] mask = null;
			if (User.Current != null)
			{
				mask = User.Current.GroupMask;
			}
			else if (Project.Current != null)
			{
				mask = Project.Current.GroupMask;
			}
			return mask;
		}

		public override void Persist()
		{
			if (User.Current != null)
			{
				PopulateNeighbours(User, Groups);
				PXSelectorAttribute.ClearGlobalCache<Users>();
				base.Persist();
			}
			else if (Project.Current != null)
			{
				PopulateNeighbours(Project, Groups);
				PXSelectorAttribute.ClearGlobalCache<PMProject>();
				base.Persist();
			}
		}

		#endregion

		#region DAC overrides
		[PXDimensionSelector(ProjectAttribute.DimensionName,
			typeof(Search<PMProject.contractCD, Where<PMProject.baseType, Equal<CT.CTPRType.project>>>),
			typeof(PMProject.contractCD),
			typeof(PMProject.contractCD), typeof(PMProject.customerID), typeof(PMProject.description), typeof(PMProject.status), DescriptionField = typeof(PMProject.description), Filterable = true)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Project ID", Visibility = PXUIVisibility.SelectorVisible)]
		[Data.EP.PXFieldDescription]
		public void _(Events.CacheAttached<PMProject.contractCD> e)
		{
		}
		#endregion
	}

	public class PMAccess : BaseAccess
	{
		#region Views

		public SelectFrom<PMProject>.View Project;

		protected IEnumerable project()
		{
			if (string.IsNullOrEmpty(Group.Current?.GroupName))
				yield break;

			var groupMap = ProjectGroup.Cache.Updated
				.RowCast<PMProjectGroup>()
				.ToDictionary(g => g.ProjectGroupID, g => g.Included);

			foreach (PMProject item in SelectFrom<PMProject>
				.Where<
					Brackets<
						PMProject.baseType.IsEqual<CT.CTPRType.project>
						.Or<PMProject.baseType.IsEqual<CT.CTPRType.projectTemplate>>>
					.And<PMProject.nonProject.IsEqual<False>>>
				.View.Select(this))
			{
				if (Project.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
				{
					if (groupMap.TryGetValue(item.ProjectGroupID ?? string.Empty,
						out var isGroupIncluded))
					{
						item.Included = isGroupIncluded;
					}
					else
					{
						item.Included = GroupMaskHelper.IsIncluded(item.GroupMask, Group.Current.GroupMask);
					}
				}
				yield return item;
			}
		}

		public SelectFrom<PMProjectGroup>.View ProjectGroup;

		protected IEnumerable projectGroup()
		{
			if (string.IsNullOrEmpty(Group.Current?.GroupName))
				yield break;

			foreach (PMProjectGroup item in SelectFrom<PMProjectGroup>.View.Select(this))
			{
				if (ProjectGroup.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
				{
					item.Included = GroupMaskHelper.IsIncluded(item.GroupMask, Group.Current.GroupMask);
				}
				yield return item;
			}
		}

		/// <see cref="BaseAccess.Group">BaseAccess.Group</see>
		protected virtual IEnumerable group()
		{
			PXResultset<Neighbour> set = PXSelectGroupBy<Neighbour,
				Where<
					Neighbour.leftEntityType, Equal<projectType>,
					Or<Neighbour.leftEntityType, Equal<projectGroupType>>>,
				Aggregate<
					GroupBy<Neighbour.coverageMask,
					GroupBy<Neighbour.inverseMask,
					GroupBy<Neighbour.winCoverageMask,
					GroupBy<Neighbour.winInverseMask>>>>>>
				.Select(this);

			foreach (RelationGroup group in PXSelect<RelationGroup>.Select(this))
			{
				if ((group.SpecificModule == null || group.SpecificModule == typeof(PMProject).Namespace)
					&& (group.SpecificType == null || group.SpecificType == typeof(PMProject).FullName || group.SpecificType == typeof(PMProjectGroup).FullName)
					|| UserAccess.InNeighbours(set, group))
				{
					yield return group;
				}
			}
		}

		#endregion

		#region Event handlers

		protected void _(Events.RowPersisting<PMProject> e)
		{
			var project = e.Row;
			var group = Group.Current;

			if (project == null || group == null || e.Operation == PXDBOperation.Delete)
				return;

			project.GroupMask = GroupMaskHelper.UpdateMask(
				project.Included == true,
				project.GroupMask,
				group.GroupMask);
		}

		protected void _(Events.RowPersisting<PMProjectGroup> e)
		{
			var projectGroup = e.Row;
			var group = Group.Current;

			if (projectGroup == null || group == null || e.Operation == PXDBOperation.Delete)
				return;

			projectGroup.GroupMask = GroupMaskHelper.UpdateMask(
				projectGroup.Included == true,
				projectGroup.GroupMask,
				group.GroupMask);
		}

		protected void _(Events.RowSelected<RelationGroup> e)
		{
			var group = e.Row;
			if (group != null)
			{
				Save.SetEnabled(!string.IsNullOrEmpty(group.GroupName));
			}
		}

		protected override void RelationGroup_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			base.RelationGroup_RowInserted(cache, e);
			RelationGroup group = (RelationGroup)e.Row;
			group.SpecificModule = typeof(PMProject).Namespace;
			group.SpecificType = typeof(PMProject).FullName;
		}

		protected void _(Events.RowUpdated<PMProjectGroup> e)
		{
			var projectGroupId = e.Row?.ProjectGroupID;
			if (e.Row?.Included == null || string.IsNullOrEmpty(projectGroupId))
				return;

			var isIncluded = (bool) e.Row.Included;
			var message = string.Format(Messages.ExcludeAllProjectsInProjectGroupMessage, projectGroupId);

			if (!isIncluded && WebDialogResult.No ==
				ProjectGroup.Ask(
					Messages.UpdateRestrictionsForProjectsCaption,
					message,
					MessageButtons.YesNo,
					new Dictionary<WebDialogResult, string>
					{
						{ WebDialogResult.Yes, MaintMessages.OKButton },
						{ WebDialogResult.No, MaintMessages.CancelButton }
					}))
			{
				e.Cache.SetValue<PMProjectGroup.included>(e.Row, true);
				return;
			}

			// Unchanged projects will be corrected in view delegate project().
			foreach (PMProject project in Project.Cache.Updated)
			{
				if (!string.Equals(projectGroupId, project.ProjectGroupID, StringComparison.OrdinalIgnoreCase))
					continue;

				project.Included = isIncluded;
				Project.Update(project);
			}
		}

		#endregion

		#region Constructor

		public PMAccess()
		{
			Project.Cache.AllowInsert = false;
			Project.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(Project.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<PMProject.included>(Project.Cache, null);

			ProjectGroup.Cache.AllowInsert = false;
			ProjectGroup.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(ProjectGroup.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<PMProjectGroup.included>(ProjectGroup.Cache, null);
		}

		#endregion

		#region Persist

		public override void Persist()
		{
			AdjustProjectGroupMasks();

			populateNeighbours(Users);
			populateNeighbours(ProjectGroup);
			populateNeighbours(Project);

			base.Persist();

			PXSelectorAttribute.ClearGlobalCache<Users>();
			PXSelectorAttribute.ClearGlobalCache<PMProjectGroup>();
			PXSelectorAttribute.ClearGlobalCache<PMProject>();
		}

		private void AdjustProjectGroupMasks()
		{
			var changedProjectGroups = ProjectGroup.Cache.Updated
				.RowCast<PMProjectGroup>()
				.ToDictionary(g => g.ProjectGroupID, g => g.Included);

			foreach (PMProject project in Project.Select())
			{
				if (!changedProjectGroups.TryGetValue(project.ProjectGroupID ?? string.Empty,
					out var isGroupIncluded))
					continue;

				if (Project.Cache.GetStatus(project) != PXEntryStatus.Notchanged)
					continue;

				Project.Cache.SetValue<PMProject.included>(project, isGroupIncluded);
				Project.Cache.MarkUpdated(project);
			}
		}

		#endregion

		#region DAC overrides

		[GL.SubAccount(DisplayName = "Default Sales Subaccount",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(GL.Sub.description))]
		protected void _(Events.CacheAttached<PMProject.defaultSalesSubID> _) {}

		[GL.SubAccount(DisplayName = "Default Cost Subaccount",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(GL.Sub.description))]
		protected void _(Events.CacheAttached<PMProject.defaultExpenseSubID> _) {}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected void _(Events.CacheAttached<PMProject.baseCuryID> _) {}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected void _(Events.CacheAttached<PMProject.billingCuryID> _) {}

		[PXRemoveBaseAttribute(typeof(CM.Extensions.CurrencyInfoAttribute))]
		protected void _(Events.CacheAttached<PMProject.curyInfoID> _) {}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected void _(Events.CacheAttached<PMProject.startDate> _) {}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(SelectFrom<PMProjectGroup>
			.Where<MatchUser>
			.SearchFor<PMProjectGroup.projectGroupID>))]
		protected void _(Events.CacheAttached<PMProjectGroup.projectGroupID> _) {}

		#endregion
	}
}
