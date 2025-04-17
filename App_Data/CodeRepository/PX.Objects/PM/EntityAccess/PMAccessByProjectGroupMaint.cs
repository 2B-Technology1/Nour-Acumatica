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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.SM;

using MaintMessages = PX.SM.Messages;

namespace PX.Objects.PM
{
	public class PMAccessByProjectGroupMaint : UserAccess
	{
		public sealed class ProjectGroupMaskHelperExt : ProjectGroupMaskHelper<PMAccessByProjectGroupMaint>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		public IProjectGroupMaskHelper ProjectGroupMaskHelper => GetExtension<ProjectGroupMaskHelperExt>();

		#region Select

		public SelectFrom<PMProjectGroup>
			.View ProjectGroup;

		/// <see cref="UserAccess.Groups">UserAccess.Groups</see>
		protected override IEnumerable groups()
		{
			foreach (RelationGroup group
				in SelectFrom<RelationGroup>.View.Select(this))
			{
				if (group.SpecificModule == null
					|| group.SpecificModule == typeof(PMProjectGroup).Namespace
					|| IsIncluded(getMask(), group))
				{
					Groups.Current = group;
					yield return group;
				}
			}
		}

		[PXHidden]
		public SelectFrom<PMProject>.View Projects;

		[PXHidden]
		public SelectFrom<Neighbour>.View Neighbours;

		#endregion

		#region Actions

		public new PXSave<PMProjectGroup> Save;
		public new PXCancel<PMProjectGroup> Cancel;
		public new PXFirst<PMProjectGroup> First;
		public new PXPrevious<PMProjectGroup> Prev;
		public new PXNext<PMProjectGroup> Next;
		public new PXLast<PMProjectGroup> Last;

		#endregion

		#region Constructor

		public PMAccessByProjectGroupMaint()
		{
			ProjectGroup.Cache.AllowDelete = false;
			ProjectGroup.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetRequired(ProjectGroup.Cache, null, false);
			Views.Caches.Remove(Groups.GetItemType());
			Views.Caches.Add(Groups.GetItemType());
		}

		#endregion

		#region Runtime

		protected override byte[] getMask()
		{
			if (User.Current != null)
				return User.Current.GroupMask;

			if (ProjectGroup.Current != null)
				return ProjectGroup.Current.GroupMask;

			return null;
		}

		public override void Persist()
		{
			if (User.Current != null)
			{
				PopulateNeighbours(User, Groups);
				PXSelectorAttribute.ClearGlobalCache<Users>();
				base.Persist();
			}
			else if (ProjectGroup.Current != null)
			{
				PopulateNeighbours(ProjectGroup, Groups);

				var relationGroups = Groups.Cache.Updated.RowCast<RelationGroup>().ToArray();
				ProjectGroupMaskHelper.AddProjectsToNeghbourMasks(relationGroups, Neighbours);

				PXSelectorAttribute.ClearGlobalCache<PMProjectGroup>();
				PXSelectorAttribute.ClearGlobalCache<PMProject>();

				base.Persist();

				ProjectGroupMaskHelper.UpdateMaskForProjectGroupProjects(
					ProjectGroup.Current.ProjectGroupID, ProjectGroup.Current.GroupMask);
			}
		}

		protected void _(Events.RowUpdated<RelationGroup> e)
		{
			var projectGroupId = ProjectGroup.Current?.ProjectGroupID;
			if (string.IsNullOrEmpty(projectGroupId))
				return;

			if (e.Row?.Included != false)
				return;

			var message = string.Format(Messages.ExcludeAllProjectsInProjectGroupMessage, projectGroupId);

			if (WebDialogResult.No ==
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
				e.Cache.SetValue<RelationGroup.included>(e.Row, true);
			}
		}

		#endregion

		#region DAC overrides

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(SelectFrom<PMProjectGroup>
			.SearchFor<PMProjectGroup.projectGroupID>))]
		[PXUIField(DisplayName = "Project Group", Visibility = PXUIVisibility.SelectorVisible)]
		protected void _(Events.CacheAttached<PMProjectGroup.projectGroupID> _) {}

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

		#endregion
	}
}
