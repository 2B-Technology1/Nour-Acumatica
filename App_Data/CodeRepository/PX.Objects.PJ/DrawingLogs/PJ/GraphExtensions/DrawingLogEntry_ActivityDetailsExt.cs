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
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Api.Models;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.Objects.PM;
using PX.SM;
using Constants = PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Constants;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;

namespace PX.Objects.PJ.DrawingLogs.PJ.GraphExtensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DrawingLogEntry_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<DrawingLogEntry_ActivityDetailsExt, DrawingLogEntry, DrawingLog, DrawingLog.noteID> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DrawingLogEntry_ActivityDetailsExt : ProjectManagementActivityDetailsExt<DrawingLogEntry, DrawingLog, DrawingLog.noteID>
	{
		[InjectDependency]
		public IProjectDataProvider ProjectDataProvider
		{
			get;
			set;
		}

		public virtual void _(Events.RowSelected<DrawingLog> args)
		{
			SetActivityDefaultSubject();
		}

		private void SetActivityDefaultSubject()
		{
			var drawingLog = Base.DrawingLog.Current;

			if (drawingLog.ProjectId != null)
			{
				var projectCd = ProjectDataProvider.GetProject(Base, drawingLog.ProjectId).ContractCD;
				SetActivityDefaultSubject(DrawingLogMessages.DrawingLogEmailDefaultSubject,
					drawingLog.DrawingLogCd, projectCd, drawingLog.Title);
			}
		}
	}
}
