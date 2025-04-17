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
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.Graphs;
using PX.Objects.PJ.PhotoLogs.PJ.Services;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Graphs;

namespace PX.Objects.PJ.PhotoLogs.PJ.GraphExtensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ProjectIssueMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<ProjectIssueMaint_ActivityDetailsExt, ProjectIssueMaint, ProjectIssue, ProjectIssue.noteID> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ProjectIssueMaint_ActivityDetailsExt : ProjectManagementActivityDetailsExt<ProjectIssueMaint, ProjectIssue, ProjectIssue.noteID>
	{
		public override void Initialize()
		{
			base.Initialize();

			PXUIFieldAttribute.SetVisible<CRPMTimeActivity.categoryID>(this.Activities.Cache, null, false);
		}
	}
}
