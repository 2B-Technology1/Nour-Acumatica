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

namespace PX.Objects.GL
{
	public class GLTranInterCompanyComparer : IEqualityComparer<GLTran>
	{
		public bool Equals(GLTran t1, GLTran t2)
		{
			return
				t1.Module == t2.Module &&
				t1.BatchNbr == t2.BatchNbr &&
				t1.RefNbr == t2.RefNbr &&
				t1.CuryInfoID == t2.CuryInfoID &&
				t1.BranchID == t2.BranchID &&
				t1.AccountID == t2.AccountID &&
				t1.SubID == t2.SubID &&
				t1.IsInterCompany == t2.IsInterCompany;
		}

		public int GetHashCode(GLTran tran)
		{
			unchecked
			{
				int ret = 37;
				ret = ret * 397 + tran.Module.GetHashCode();
				ret = ret * 397 + tran.BatchNbr.GetHashCode();
				ret = ret * 397 + (tran.RefNbr ?? string.Empty).GetHashCode();
				ret = ret * 397 + tran.CuryInfoID.GetHashCode();
				ret = ret * 397 + tran.BranchID.GetHashCode();
				ret = ret * 397 + tran.AccountID.GetHashCode();
				ret = ret * 397 + tran.SubID.GetHashCode();
				ret = ret * 397 + tran.IsInterCompany.GetHashCode();

				return ret;
			}
		}
	}
}
