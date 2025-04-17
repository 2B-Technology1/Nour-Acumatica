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
	public class RemainderToLastLineService<Item> : RemainderToBiggestLineService<Item>
		where Item : class, IAmountItem
	{
		public RemainderToLastLineService(DistributionParameter<Item> distributeParameter)
			: base(distributeParameter)
		{
		}

		protected Item _lastLine;

		protected override void Clear()
		{ 
			_lastLine = null;
			base.Clear();
		}

		protected override void ProcessItem(Item item, decimal? sumOfWeight, ref decimal? sumOfAmt, ref decimal? curySumOfAmt)
		{
			base.ProcessItem(item, sumOfWeight, ref sumOfAmt, ref curySumOfAmt);
			SetLastLine(item);
		}

		protected virtual void SetLastLine(Item item)
		{
			if (item.Weight > 0m)
				_lastLine = item;
		}

		protected override void DistributeRoundingDifference(Item item, decimal? roundingDifference, decimal? curyRoundingDifference)
			=> base.DistributeRoundingDifference(_lastLine, roundingDifference, curyRoundingDifference);
	}
}
