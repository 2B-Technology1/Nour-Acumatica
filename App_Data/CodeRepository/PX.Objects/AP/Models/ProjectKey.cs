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

namespace PX.Objects.AP
{
	public class ProjectKey
	{
		public int? BranchID { get; set; }
		public int? AccountID { get; set; }
		public int? SubID { get; set; }
		public int? ProjectID { get; set; }
		public int? TaskID { get; set; }
		public int? CostCodeID { get; set; }
		public int? InventoryID { get; set; }

		public ProjectKey(int? branchID, int? accountID, int? subID, int? projectID, int? taskID, int? costCodeID, int? inventoryID)
		{
			BranchID = branchID;
			AccountID = accountID;
			SubID = subID;
			ProjectID = projectID;
			TaskID = taskID;
			CostCodeID = costCodeID;
			InventoryID = inventoryID;
		}

		public override int GetHashCode()
		{
			return
				BranchID.GetHashCode() ^
				AccountID.GetHashCode() ^
				SubID.GetHashCode() ^
				ProjectID.GetHashCode() ^
				TaskID.GetHashCode() ^
				CostCodeID.GetHashCode() ^
				InventoryID.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ProjectKey))
				return false;

			var projectKey = (ProjectKey)obj;
			return 
				(this.BranchID == projectKey.BranchID) &&
				(this.AccountID == projectKey.AccountID) &&
				(this.SubID == projectKey.SubID) &&
				(this.ProjectID == projectKey.ProjectID) &&
				(this.TaskID == projectKey.TaskID) &&
				(this.CostCodeID == projectKey.CostCodeID) &&
				(this.InventoryID == projectKey.InventoryID);
		}
	}
}
