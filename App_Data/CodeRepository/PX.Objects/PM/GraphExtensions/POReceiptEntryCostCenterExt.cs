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
using System;
using System.Collections.Generic;

namespace PX.Objects.PM
{
    public class POReceiptEntryCostCenterExt : ProjectCostCenterBase<POReceiptEntry>, IN.ICostCenterSupport<POReceiptLine>
	{		
		public virtual int SortOrder => 200;

		public static new bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		public virtual IEnumerable<Type> GetFieldsDependOn()
		{
			yield return typeof(POReceiptLine.isSpecialOrder);
			yield return typeof(POReceiptLine.siteID);
			yield return typeof(POReceiptLine.projectID);
			yield return typeof(POReceiptLine.taskID);
		}

		public bool IsSpecificCostCenter(POReceiptLine line) => line.IsSpecialOrder != true && IsSpecificCostCenter(line.SiteID, line.ProjectID, line.TaskID);

		public virtual int GetCostCenterID(POReceiptLine tran)
		{
			return (int)FindOrCreateCostCenter(tran.SiteID, tran.ProjectID, tran.TaskID);
		}
	}
}
