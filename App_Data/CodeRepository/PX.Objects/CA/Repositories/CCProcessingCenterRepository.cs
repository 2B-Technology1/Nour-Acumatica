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
using PX.Objects.AR;
using PX.Objects.CC;
using System;

namespace PX.Objects.CA.Repositories
{
	public class CCProcessingCenterRepository
	{
		protected readonly PXGraph _graph;
		public CCProcessingCenterRepository(PXGraph graph)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));

			_graph = graph;
		}

		public CCProcessingCenter GetProcessingCenterByID(string processingCenterID)
		{
			return CCProcessingCenter.PK.Find(_graph, processingCenterID);
		}

		public CCProcessingCenterBranch GetProcessingCenterBranchByBranchAndProcCenterIDs(int? branchId, string procCenterId)
		{
			return CCProcessingCenterBranch.PK.Find(_graph, procCenterId, branchId);
		}

		public CCProcessingCenter FindProcessingCenter(int? pMInstanceID, string aCuryId)
		{
			CCProcessingCenter result;
			result = PXSelectJoin<CCProcessingCenter, InnerJoin<CustomerPaymentMethod,
				On<CustomerPaymentMethod.cCProcessingCenterID, Equal<CCProcessingCenter.processingCenterID>>>,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(_graph, pMInstanceID);
			if (result != null)
			{
				return result;
			}
			if (aCuryId != null)
			{
				result = PXSelectJoin<CCProcessingCenter,
					InnerJoin<CCProcessingCenterPmntMethod,
						On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>,
							And<CCProcessingCenter.isActive, Equal<BQLConstants.BitOn>>>,
						InnerJoin<CustomerPaymentMethod,
							On<CustomerPaymentMethod.paymentMethodID,
								Equal<CCProcessingCenterPmntMethod.paymentMethodID>,
								And<CCProcessingCenterPmntMethod.isActive, Equal<BQLConstants.BitOn>>>,
							InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CCProcessingCenter.cashAccountID>>>>>,
					Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>,
						And<CashAccount.curyID, Equal<Required<CashAccount.curyID>>>>,
					OrderBy<Desc<CCProcessingCenterPmntMethod.isDefault>>>.Select(_graph, pMInstanceID, aCuryId);
			}
			else
			{
				result = PXSelectJoin<CCProcessingCenter,
					InnerJoin<CCProcessingCenterPmntMethod,
						On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>,
							And<CCProcessingCenter.isActive, Equal<BQLConstants.BitOn>>>,
						InnerJoin<CustomerPaymentMethod,
							On<CustomerPaymentMethod.paymentMethodID,
								Equal<CCProcessingCenterPmntMethod.paymentMethodID>,
								And<CCProcessingCenterPmntMethod.isActive, Equal<BQLConstants.BitOn>>>,
							InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CCProcessingCenter.cashAccountID>>>>>,
					Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>,
					OrderBy<Desc<CCProcessingCenterPmntMethod.isDefault>>>.Select(_graph, pMInstanceID);
			}
			return result;
		}
	}
}
