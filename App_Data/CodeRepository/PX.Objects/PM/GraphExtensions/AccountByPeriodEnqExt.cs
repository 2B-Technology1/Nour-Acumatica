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
using PX.Objects.GL;
using System.Collections;

namespace PX.Objects.PM.GraphExtensions
{
	/// <summary>
	/// AccountByPeriodEnq extension
	/// </summary>
	public class AccountByPeriodEnqExt : PXGraphExtension<AccountByPeriodEnq>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
		}

		#region Actions/Buttons

		public PXAction<AccountByPeriodFilter> ViewPMTran;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewPMTran(PXAdapter adapter)
		{
			GLTranR tran = Base.GLTranEnq.Current;

			if (tran?.PMTranID != null)
			{
				var graph = PXGraph.CreateInstance<TransactionInquiry>();
				var filter = graph.Filter.Insert();
				filter.TranID = tran.PMTranID;

				throw new PXRedirectRequiredException(graph, true, "ViewPMTran") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return Base.Filter.Select();
		}

		#endregion
	}
}