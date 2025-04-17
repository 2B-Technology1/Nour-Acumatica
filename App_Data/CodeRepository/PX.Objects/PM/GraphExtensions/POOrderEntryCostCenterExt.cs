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
using PX.Objects.PO;
using PX.Objects.PO.GraphExtensions.POOrderEntryExt;
using System;
using System.Collections.Generic;

namespace PX.Objects.PM
{
    public class POOrderEntryCostCenterExt : ProjectCostCenterBase<POOrderEntry>, IN.ICostCenterSupport<POLine>
	{		
		public virtual int SortOrder => 200;

		public static new bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		public virtual IEnumerable<Type> GetFieldsDependOn()
		{
			yield return typeof(POLine.isSpecialOrder);
			yield return typeof(POLine.siteID);
			yield return typeof(POLine.projectID);
			yield return typeof(POLine.taskID);
		}

		public bool IsSpecificCostCenter(POLine line) => line.IsSpecialOrder != true && IsSpecificCostCenter(line.SiteID, line.ProjectID, line.TaskID);

		public virtual int GetCostCenterID(POLine tran)
		{
			return (int)FindOrCreateCostCenter(tran.SiteID, tran.ProjectID, tran.TaskID);
		}
	}
}
