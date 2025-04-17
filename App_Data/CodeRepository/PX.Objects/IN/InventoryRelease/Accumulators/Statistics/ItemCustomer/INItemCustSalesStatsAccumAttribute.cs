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

namespace PX.Objects.IN.InventoryRelease.Accumulators.Statistics.ItemCustomer
{
	public abstract class INItemCustSalesStatsAccumAttribute : PXAccumulatorAttribute
	{
		public INItemCustSalesStatsAccumAttribute()
		{
			SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
				return false;

			PrepareInsertImpl((INItemCustSalesStats)row, columns);

			return true;
		}

		protected abstract void PrepareInsertImpl(INItemCustSalesStats stats, PXAccumulatorCollection columns);

		public override bool PersistInserted(PXCache sender, object row)
		{
			try
			{
				return base.PersistInserted(sender, row);
			}
			catch (PXLockViolationException)
			{
				return false;
			}
		}
	}
}
