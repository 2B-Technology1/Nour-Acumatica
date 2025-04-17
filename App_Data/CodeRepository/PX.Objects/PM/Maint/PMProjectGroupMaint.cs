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

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.CS;

using MaintMessages = PX.SM.Messages;

namespace PX.Objects.PM
{
	public class PMProjectGroupMaint : PXGraph<PMProjectGroupMaint>
	{
		public sealed class ProjectGroupMaskHelperExt : ProjectGroupMaskHelper<PMProjectGroupMaint>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		public IProjectGroupMaskHelper ProjectGroupMaskHelper => GetExtension<ProjectGroupMaskHelperExt>();

		public PMProjectGroupMaint()
		{
			var isRlsEnabled = PXAccess.FeatureInstalled<FeaturesSet.rowLevelSecurity>();

			if (isRlsEnabled)
			{
				otherActions.AddMenuAction(updateRestrictions);
				otherActions.AddMenuAction(manageRestrictionGroups);
			}

			updateRestrictions.SetVisible(isRlsEnabled);
			manageRestrictionGroups.SetVisible(isRlsEnabled);
		}

		[PXImport(typeof(PMProjectGroup))]
		public SelectFrom<PMProjectGroup>
			.Where<MatchUser>
			.View ProjectGroups;

		public PXSavePerRow<PMProjectGroup> Save;
		public PXCancel<PMProjectGroup> Cancel;

		public PXAction<PMProjectGroup> otherActions;

		[PXUIField(DisplayName = Messages.Other)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder, MenuAutoOpen = true)]
		public void OtherActions() {}

		public PXAction<PMProjectGroup> updateRestrictions;

		[PXUIField(DisplayName = "Update Restriction Settings for Projects")]
		[PXButton]
		public IEnumerable UpdateRestrictions(PXAdapter adapter)
		{
			var projectGroup = ProjectGroups.Current;
			if (string.IsNullOrEmpty(projectGroup?.ProjectGroupID))
				return adapter.Get();

			var message = string.Format(Messages.UpdateRestrictionsForProjectsMessage, projectGroup.ProjectGroupID);

			if (WebDialogResult.Yes !=
				ProjectGroups.Ask(
					Messages.UpdateRestrictionsForProjectsCaption,
					message,
					MessageButtons.YesNo,
					new Dictionary<WebDialogResult, string>
					{
						{ WebDialogResult.Yes, MaintMessages.UpdateButton },
						{ WebDialogResult.No, MaintMessages.CancelButton }
					}))
				return adapter.Get();

			PXLongOperation.StartOperation(this, () =>
			{
				var graph = CreateInstance<PMProjectGroupMaint>();
				graph.ProjectGroupMaskHelper.UpdateMaskForProjectGroupProjects(
					projectGroup.ProjectGroupID, projectGroup.GroupMask);
			});

			return adapter.Get();
		}

		public PXAction<PMProjectGroup> manageRestrictionGroups;

		[PXUIField(DisplayName = "Manage Restriction Groups")]
		[PXButton]
		public void ManageRestrictionGroups()
		{
			var projectGroup = ProjectGroups.Current;
			if (projectGroup == null)
				return;

			var target = CreateInstance<PMAccessByProjectGroupMaint>();
			target.ProjectGroup.Current = projectGroup;
			throw new PXRedirectRequiredException(target, nameof(ManageRestrictionGroups));
		}

		protected void _(Events.RowUpdating<PMProjectGroup> e)
		{
			var groupId = e.NewRow?.ProjectGroupID;
			if (string.IsNullOrEmpty(groupId))
				return;

			PMProjectGroup group = SelectFrom<PMProjectGroup>
				.Where<
					PMProjectGroup.projectGroupID.IsEqual<P.AsString>
					.And<Not<MatchUser>>>
				.View.Select(this, groupId);

			if (group == null)
				return;

			e.Cache.RaiseExceptionHandling<PMProjectGroup.projectGroupID>(
				e.NewRow,
				groupId,
				new PXSetPropertyException(Messages.InsufficientAccessToProjectGroup, groupId));
			e.Cancel = true;
		}

		protected void _(Events.FieldVerifying<PMProjectGroup, PMProjectGroup.projectGroupID> e)
		{
			if (e.Row == null)
				return;

			var groupId = (string) e.OldValue;
			if (IsNotEmpty(groupId))
			{
				throw new PXSetPropertyException(Messages.CannotRenameProjectGroup, groupId);
			}
		}

		private bool IsNotEmpty(string groupId)
		{
			if (string.IsNullOrEmpty(groupId))
				return false;

			PMProject project = SelectFrom<PMProject>
				.Where<PMProject.projectGroupID.IsEqual<P.AsString>>
				.View.Select(this, groupId);
			return project != null;
		}
	}
}
