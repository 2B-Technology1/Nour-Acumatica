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

namespace PX.Objects.PO.Services.AmountDistribution
{
	public class AccumulateRemainderToNonZeroLineService<Item> : RemainderToBiggestLineService<Item>
		where Item : class, IAmountItem
	{
		public AccumulateRemainderToNonZeroLineService(DistributionParameter<Item> distributeParameter)
			: base(distributeParameter)
		{
		}

		protected decimal? _accumulatedAmount;
		protected decimal? _curyAccumulatedAmount;

		protected override void Clear()
		{
			_accumulatedAmount = 0m;
			_curyAccumulatedAmount = 0m;
			base.Clear();
		}

		protected override void RoundAmount(Item item, ref decimal? currentAmount, ref decimal? curyCurrentAmount)
		{
			currentAmount += _accumulatedAmount;
			curyCurrentAmount += _curyAccumulatedAmount;

			var oldAmount = currentAmount;
			var curyOldAmount = curyCurrentAmount;

			base.RoundAmount(item, ref currentAmount, ref curyCurrentAmount);

			_accumulatedAmount = oldAmount - currentAmount;
			_curyAccumulatedAmount = curyOldAmount - curyCurrentAmount;
		}
	}
}