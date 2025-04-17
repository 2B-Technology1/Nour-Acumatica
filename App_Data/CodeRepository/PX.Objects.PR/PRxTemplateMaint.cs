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
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PR
{
	/// <summary>
	/// Extends the TemplateMaint graph to adapt business logic / UI to work with PR fields
	/// </summary>
	public class PRxTemplateMaint : PXGraphExtension<TemplateMaint>
	{
		public class PRxProjectGraph : PRxProjectGraph<TemplateMaint, PMProject>
		{
			public static bool IsActive()
			{
				return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
			}
		}

		public class PRxTaskGraph : PRxTaskGraph<TemplateMaint, PMTask>
		{
			public static bool IsActive()
			{
				return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
			}
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		[PXOverride]
		public virtual void OnCopyPasteTaskInserted(PMProject target, PMTask dstTask, PMTask srcTask)
		{
			PRxPMTask srcTaskExtension = srcTask.GetExtension<PRxPMTask>();
			PRxPMTask dstTaskExtension = dstTask.GetExtension<PRxPMTask>();
			Base.GetExtension<PRxTaskGraph>().Copy(dstTaskExtension, srcTaskExtension);
		}

		[PXOverride]
		public virtual void OnCopyPasteCompleted(PMProject dstProject, PMProject srcProject)
		{
			PMProjectExtension srcProjectExtension = srcProject.GetExtension<PMProjectExtension>();
			PMProjectExtension dstProjectExtension = dstProject.GetExtension<PMProjectExtension>();
			Base.GetExtension<PRxProjectGraph>().Copy(dstProjectExtension, srcProjectExtension);
		}
	}
}
