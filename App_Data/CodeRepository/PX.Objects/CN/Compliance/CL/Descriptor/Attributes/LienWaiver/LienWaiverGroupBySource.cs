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

using PX.Data.BQL;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver
{
	public class LienWaiverGroupBySource
	{
		public const string CommitmentProjectTask = "CPT";
		public const string CommitmentProject = "CP";
		public const string ProjectTask = "PT";
		public const string Project = "P";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base
			(
				new string[]
				{
					CommitmentProjectTask,
					CommitmentProject,
					ProjectTask,
					Project
				},
				new string[]
				{
					ComplianceLabels.LienWaiverSetup.CommitmentProjectTask,
					ComplianceLabels.LienWaiverSetup.CommitmentProject,
					ComplianceLabels.LienWaiverSetup.ProjectTask,
					ComplianceLabels.LienWaiverSetup.Project
				}
			)
			{
			}
		}
	}
}
