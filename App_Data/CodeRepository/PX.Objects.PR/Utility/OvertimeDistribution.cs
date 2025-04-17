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

namespace PX.Objects.PR
{
	public class OvertimeDistribution
	{
		/// <summary>
		/// An existing Earning Detail record that should be updated by reducing the overtime hours (above the overtime threshold).
		/// </summary>
		public PREarningDetail BaseEarningDetail { get; private set; }
		/// <summary>
		/// A new Earning Detail record that should be added with overtime earning type and overtime hours taken from <see cref="BaseEarningDetail"/>.
		/// </summary>
		public PREarningDetail OvertimeEarningDetail { get; private set; }

		public OvertimeDistribution(PREarningDetail baseEarningDetail, PREarningDetail overtimeEarningDetail)
		{
			BaseEarningDetail = baseEarningDetail;
			OvertimeEarningDetail = overtimeEarningDetail;
		}
	}
}
