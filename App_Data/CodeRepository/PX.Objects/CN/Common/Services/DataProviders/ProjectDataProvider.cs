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
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class ProjectDataProvider : IProjectDataProvider
	{
		public PMProject GetProject(PXGraph graph, int? projectId)
		{
			return graph.Select<PMProject>().SingleOrDefault(p => p.ContractID == projectId);
		}

		public IEnumerable<PMProject> GetProjects(PXGraph graph)
		{
			return SelectFrom<PMProject>
				.Where<PMProject.nonProject.IsEqual<False>
					.And<PMProject.baseType.IsEqual<CTPRType.project>>>.View.Select(graph).FirstTableItems;
		}

		public bool IsActiveProject(PXGraph graph, int? projectId)
		{
			var project = GetProject(graph, projectId);
			return project != null && project.Status
				       .IsIn(ProjectStatus.Active, ProjectStatus.Planned);
		}
	}
}