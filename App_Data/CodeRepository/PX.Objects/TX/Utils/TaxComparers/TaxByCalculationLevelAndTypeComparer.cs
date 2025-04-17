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
using PX.Common;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	/// <summary>
	/// A comparer of taxes by tax calculation level and by tax type for taxes with the same calculation level
	/// </summary>
	public class TaxByCalculationLevelAndTypeComparer : TaxByCalculationLevelComparer
	{
		public static new readonly TaxByCalculationLevelAndTypeComparer Instance = new TaxByCalculationLevelAndTypeComparer();

		protected TaxByCalculationLevelAndTypeComparer() : base() { }

		/// <summary>
		/// Tax comparison by tax type. Compares tax types via <see cref="string.Compare(string, string)"/>.
		/// </summary>
		/// <param name="taxX">The tax x.</param>
		/// <param name="taxY">The tax y.</param>
		/// <returns/>
		protected override int CompareTaxesByTaxTypes(Tax taxX, Tax taxY) => 
			string.Compare(taxX.TaxType, taxY.TaxType);
	}
}
