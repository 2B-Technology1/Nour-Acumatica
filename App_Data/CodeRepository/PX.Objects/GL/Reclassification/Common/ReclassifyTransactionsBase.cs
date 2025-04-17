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
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.GL.Reclassification.Common
{
	public abstract class ReclassifyTransactionsBase<TGraph> : PXGraph<TGraph> 
		where TGraph : PXGraph
	{
		public PXFilter<ReclassGraphState> StateView;
		public PXSetup<GLSetup> GLSetup;

        protected ReclassGraphState State
		{
			get { return StateView.Current; }
			set { StateView.Current = value; }
		}

		protected ReclassifyTransactionsBase()
		{
			var setup = GLSetup.Current;
        }

		public static bool IsReclassAttrChanged(GLTranForReclassification tranForReclass)
		{
			return tranForReclass.NewBranchID != tranForReclass.BranchID ||
					tranForReclass.NewAccountID != tranForReclass.AccountID ||
					tranForReclass.NewSubID != tranForReclass.SubID ||
					IsReclassProjectAttrChanged(tranForReclass);
		}

		public static bool IsReclassProjectAttrChanged(GLTranForReclassification tranForReclass)
		{
			return (tranForReclass.NewProjectID != tranForReclass.ProjectID ||
					tranForReclass.NewTaskID != tranForReclass.TaskID ||
					tranForReclass.NewCostCodeID != tranForReclass.CostCodeID && PXAccess.FeatureInstalled<FeaturesSet.costCodes>()) 
					&& PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		public GLTranForReclassification GetGLTranForReclassByKey(GLTranKey key)
		{
			return new GLTranForReclassification
			{
				Module = key.Module,
				BatchNbr = key.BatchNbr,
				LineNbr = key.LineNbr
			};
		}

		protected virtual IEnumerable<GLTran> GetTransReclassTypeSorted(PXGraph graph, string module, string batchNbr)
		{
			return PXSelect<GLTran,
							Where<GLTran.module, Equal<Required<GLTran.module>>,
									And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>>>,
							OrderBy<Desc<GLTran.reclassType>>>
							.Select(graph, module, batchNbr)
							.RowCast<GLTran>();
			}
	}
}
