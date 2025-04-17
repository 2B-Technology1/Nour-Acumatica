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

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CT;
using PX.SM;

namespace PX.Objects.PM
{
	public interface IProjectGroupMaskHelper
	{
		void UpdateMaskForProjectGroupProjects(string projectGroupId, byte[] newGroupMask);

		void UpdateProjectMaskFromProjectGroup(PMProject project, string projectGroupID, PXCache cache);

		void UpdateNeighbourMasks(RelationGroup[] relationGroups, PXSelectBase<Neighbour> neighbours,
			Type leftEntity, Type rightEntity);

		void AddProjectsToNeghbourMasks(RelationGroup[] relationGroups, PXSelectBase<Neighbour> neighbours);
	}

	public abstract class ProjectGroupMaskHelper<TGraph> : PXGraphExtension<TGraph>, IProjectGroupMaskHelper
		where TGraph : PXGraph
	{
		public void UpdateMaskForProjectGroupProjects(string projectGroupId, byte[] newGroupMask)
		{
			if (string.IsNullOrEmpty(projectGroupId))
				return;

			PXDatabase.Update<Contract>(
				new PXDataFieldRestrict(nameof(PMProject.ProjectGroupID), projectGroupId),
				new PXDataFieldAssign(nameof(PMProject.GroupMask), newGroupMask)
			);
		}

		public void UpdateProjectMaskFromProjectGroup(PMProject project, string projectGroupID, PXCache cache)
		{
			if (project == null)
				return;

			if (string.IsNullOrEmpty(projectGroupID))
			{
				cache.SetValue<PMProject.groupMask>(project, Array.Empty<byte>());
				return;
			}

			PMProjectGroup projectGroup = SelectFrom<PMProjectGroup>
				.Where<PMProjectGroup.projectGroupID.IsEqual<P.AsString>>
				.View.Select(Base, projectGroupID);
			cache.SetValue<PMProject.groupMask>(project, projectGroup.GroupMask);
		}

		public void AddProjectsToNeghbourMasks(RelationGroup[] relationGroups, PXSelectBase<Neighbour> neighbours)
		{
			UpdateNeighbourMasks(relationGroups, neighbours, typeof(Contract), typeof(Contract));
			UpdateNeighbourMasks(relationGroups, neighbours, typeof(Contract), typeof(Users));
			UpdateNeighbourMasks(relationGroups, neighbours, typeof(Contract), typeof(PMProjectGroup));
			UpdateNeighbourMasks(relationGroups, neighbours, typeof(Users), typeof(Contract));
			UpdateNeighbourMasks(relationGroups, neighbours, typeof(PMProjectGroup), typeof(Contract));
		}

		public void UpdateNeighbourMasks(RelationGroup[] relationGroups, PXSelectBase<Neighbour> neighbours,
			Type leftEntity, Type rightEntity)
		{
			if (relationGroups == null)
				return;

			foreach (Neighbour neighbour in neighbours.Select())
			{
				if (neighbour.LeftEntityType != leftEntity.FullName
					&& neighbour.RightEntityType != rightEntity.FullName)
					continue;

				foreach (var relationGroup in relationGroups)
				{
					UpdateNeighbourMasks(relationGroup, neighbour);
				}
				neighbours.Update(neighbour);
			}

			var newNeighbour = new Neighbour()
			{
				LeftEntityType = leftEntity.FullName,
				RightEntityType = rightEntity.FullName
			};
			if (neighbours.Locate(newNeighbour) == null)
			{
				foreach (var relationGroup in relationGroups)
				{
					UpdateNeighbourMasks(relationGroup, newNeighbour);
				}
				neighbours.Insert(newNeighbour);
			}
		}

		protected virtual void UpdateNeighbourMasks(RelationGroup relationGroup, Neighbour neighbour)
		{
			var newMask = relationGroup.GroupMask;
			if (newMask == null)
				return;

			var emptyMask = new byte[newMask.Length];

			neighbour.CoverageMask = GroupMaskHelper.UpdateMask(true, neighbour.CoverageMask, emptyMask);
			neighbour.WinCoverageMask = GroupMaskHelper.UpdateMask(true, neighbour.WinCoverageMask, emptyMask);
			neighbour.InverseMask = GroupMaskHelper.UpdateMask(true, neighbour.InverseMask, emptyMask);
			neighbour.WinInverseMask = GroupMaskHelper.UpdateMask(true, neighbour.WinInverseMask, emptyMask);

			switch (relationGroup.GroupType)
			{
				case "IE":
					neighbour.CoverageMask = GroupMaskHelper.UpdateMask(true, neighbour.CoverageMask, newMask);
					break;

				case "IO":
					neighbour.WinCoverageMask = GroupMaskHelper.UpdateMask(true, neighbour.WinCoverageMask, newMask);
					break;

				case "EE":
					neighbour.InverseMask = GroupMaskHelper.UpdateMask(true, neighbour.InverseMask, newMask);
					break;

				case "EO":
					neighbour.WinInverseMask = GroupMaskHelper.UpdateMask(true, neighbour.WinInverseMask, newMask);
					break;

				default:
					break;
			}
		}
	}
}
