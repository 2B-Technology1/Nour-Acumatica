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
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using System;
using System.Collections.Generic;

namespace PX.Objects.PM
{
    public class SOOrderEntryExt : ProjectCostCenterBase<SOOrderEntry>, IN.ICostCenterSupport<SOLine>
	{
		public int SortOrder => 200;

		
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		public virtual IEnumerable<Type> GetFieldsDependOn()
		{
			yield return typeof(SOLine.isSpecialOrder);
			yield return typeof(SOLine.siteID);
			yield return typeof(SOLine.projectID);
			yield return typeof(SOLine.taskID);
		}

		public bool IsSpecificCostCenter(SOLine line) => line.IsSpecialOrder != true && IsSpecificCostCenter(line.SiteID, line.ProjectID, line.TaskID);

		public virtual int GetCostCenterID(SOLine tran)
		{
			return (int)FindOrCreateCostCenter(tran.SiteID, tran.ProjectID, tran.TaskID);
		}
	}
}
