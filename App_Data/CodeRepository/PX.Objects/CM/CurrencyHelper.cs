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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.CM
{
	public class CurrencyHelper
	{
		public static bool IsSameCury(long? CuryInfoIDA, long? CuryInfoIDB, CurrencyInfo curyInfoA, CurrencyInfo curyInfoB)
		{
			return CuryInfoIDA == CuryInfoIDB || curyInfoA != null && curyInfoB != null && curyInfoA.CuryID == curyInfoB.CuryID;
		}

		public static bool IsSameCury(PXGraph graph, long? curyInfoIDA, long? curyInfoIDB)
		{
			CurrencyInfo curyInfoA = PXSelect<CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.SelectSingleBound(graph, null, curyInfoIDA);
			CurrencyInfo curyInfoB = PXSelect<CurrencyInfo,
				Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.SelectSingleBound(graph, null, curyInfoIDB);
			return CurrencyHelper.IsSameCury(curyInfoIDA, curyInfoIDB, curyInfoA, curyInfoB);
		}
    }
}
