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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.GL;
using PX.Objects.CM;
using static PX.Objects.AR.ARDocumentEnq;

namespace ReconciliationTools
{
	#region Internal Types

	[Serializable]
	public partial class ARGLDiscrepancyByDocumentEnqResult : ARDocumentResult, IDiscrepancyEnqResult
	{
		#region XXTurnover
		public abstract class xXTurnover : PX.Data.BQL.BqlDecimal.Field<xXTurnover> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "AR Turnover")]
		public virtual decimal? XXTurnover
		{
			get;
			set;
		}
		#endregion
		#region Discrepancy
		public abstract class discrepancy : PX.Data.BQL.BqlDecimal.Field<discrepancy> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Discrepancy")]
		public virtual decimal? Discrepancy
		{
			get
			{
				return GLTurnover - XXTurnover;
			}
		}
		#endregion
	}

	#endregion

	[TableAndChartDashboardType]
	public class ARGLDiscrepancyByDocumentEnq : ARGLDiscrepancyEnqGraphBase<ARGLDiscrepancyByAccountEnq, ARGLDiscrepancyByCustomerEnqFilter, ARGLDiscrepancyByDocumentEnqResult>
	{
		public ARGLDiscrepancyByDocumentEnq()
		{
			PXUIFieldAttribute.SetRequired<ARGLDiscrepancyByDocumentEnqResult.refNbr>(Caches[typeof(ARGLDiscrepancyByDocumentEnqResult)], false);
			PXUIFieldAttribute.SetRequired<ARGLDiscrepancyByDocumentEnqResult.finPeriodID>(Caches[typeof(ARGLDiscrepancyByDocumentEnqResult)], false);
		}

		#region CacheAttached

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Financial Period")]
		protected virtual void ARGLDiscrepancyByCustomerEnqFilter_PeriodFrom_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault]
		protected virtual void ARGLDiscrepancyByCustomerEnqFilter_CustomerID_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Original Amount")]
		protected virtual void ARGLDiscrepancyByDocumentEnqResult_OrigDocAmt_CacheAttached(PXCache sender) { }

		#endregion

		protected override List<ARGLDiscrepancyByDocumentEnqResult> SelectDetails()
		{
			var list = new List<ARGLDiscrepancyByDocumentEnqResult>();
			ARGLDiscrepancyByCustomerEnqFilter header = Filter.Current;

			if (header == null ||
				header.BranchID == null ||
				header.PeriodFrom == null ||
				header.CustomerID == null)
			{
				return list;
			}
			
			#region AR balances

			ARDocumentEnq graphAR = PXGraph.CreateInstance<ARDocumentEnq>();
			ARDocumentEnq.ARDocumentFilter filterAR = PXCache<ARDocumentEnq.ARDocumentFilter>.CreateCopy(graphAR.Filter.Current);
			var branch = PXAccess.GetBranch(header.BranchID);
			filterAR.OrgBAccountID = branch?.BAccountID;
			filterAR.OrganizationID = branch?.Organization?.OrganizationID;
			filterAR.BranchID = header.BranchID;
			filterAR.CustomerID = header.CustomerID;
			filterAR.Period = header.PeriodFrom;
			filterAR.ARAcctID = header.AccountID;
			filterAR.SubCD = header.SubCD;
			filterAR.IncludeChildAccounts = false;
			filterAR.IncludeGLTurnover = true;
			filterAR = graphAR.Filter.Update(filterAR);
			foreach (ARDocumentResult document in graphAR.Documents.Select())
			{
				var result = new ARGLDiscrepancyByDocumentEnqResult {XXTurnover = (document.ARTurnover ?? 0m)};
				PXCache<ARDocumentResult>.RestoreCopy(result, document);
				if (header.ShowOnlyWithDiscrepancy != true || result.Discrepancy != 0m)
					list.Add(result);
			}

			return list;

			#endregion


		}

		public PXAction<ARGLDiscrepancyByCustomerEnqFilter> viewDocument;


		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton()]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
					{
			if (this.Rows.Current != null)
			{
				PXRedirectHelper.TryRedirect(Rows.Cache, Rows.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
				}
			return Filter.Select();
			}

	}
}