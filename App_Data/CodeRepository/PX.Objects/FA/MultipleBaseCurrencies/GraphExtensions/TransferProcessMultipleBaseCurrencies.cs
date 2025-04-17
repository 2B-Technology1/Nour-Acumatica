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
using static PX.Objects.FA.TransferProcess;

namespace PX.Objects.FA
{
	public class TransferProcessMultipleBaseCurrencies : PXGraphExtension<TransferProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		protected virtual void _(Events.FieldUpdating<TransferFilter, TransferFilter.branchTo> e)
		{
			if (e.Row == null) return;

			TransferFilter filter = e.Row;

			if (e.Cache.GetValuePending<TransferFilter.branchFrom>(filter) != PXCache.NotSetValue)
			{
				object branchFrom = PXAccess.GetBranchID((string)e.Cache.GetValuePending<TransferFilter.branchFrom>(filter));
				object branchTo = e.NewValue is int? ? e.NewValue : PXAccess.GetBranchID((string)e.NewValue);

				if (branchTo == null || PXAccess.GetBranch((int?)branchFrom)?.BaseCuryID == PXAccess.GetBranch((int?)branchTo)?.BaseCuryID)
				{
					try
					{
						e.Cache.SetValue<TransferFilter.branchTo>(filter, branchTo);
						e.Cache.SetValueExt<TransferFilter.branchFrom>(filter, branchFrom);
						e.Cache.RaiseExceptionHandling<TransferFilter.branchFrom>(filter, branchFrom, null);
						e.Cache.RaiseFieldVerifying<TransferFilter.branchFrom>(filter, ref branchFrom);
					}
					catch (PXSetPropertyException ex)
					{
						e.Cache.RaiseExceptionHandling<TransferFilter.branchFrom>(filter, branchFrom, ex);
					}
				}
			}
		}
	}
}


