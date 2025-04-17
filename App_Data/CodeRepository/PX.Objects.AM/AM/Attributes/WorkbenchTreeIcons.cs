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

namespace PX.Objects.AM.Attributes
{
	public class WorkbenchTreeIcons
	{
		public const string BOM = "svg:manufacturing@billofmaterial";

		public class Operation
		{
			/// <summary>
			/// Outside / external operation
			/// </summary>
			public const string OutsideProcess = "svg:manufacturing@opexternal";
			/// <summary>
			/// Normal / Internal operation
			/// </summary>
			public const string Standard = "svg:manufacturing@opinternal";
		}

		public class Material
		{
			/// <summary>
			/// Non-stock Item – Phantom
			/// </summary>
			public const string NonStockPhantom = "svg:manufacturing@matlnonstockphantom";

			/// <summary>
			/// Non-stock Item – Regular
			/// </summary>
			public const string NonStockRegular = "svg:manufacturing@matlnonstockregular";

			/// <summary>
			/// Non-stock Item – Subcontract
			/// </summary>
			public const string NonStockSubcontract = "svg:manufacturing@matlnonstocksubcontract";

			/// <summary>
			/// Stock Item – Has BOM – Phantom
			/// </summary>
			public const string StockPhantomBOM = "svg:manufacturing@matlstockbomphantom";

			/// <summary>
			/// Stock Item – Has BOM – Regular
			/// </summary>
			public const string StockRegularBOM = "svg:manufacturing@matlstockbomregular";

			/// <summary>
			/// Stock Item – Has BOM – Subcontract
			/// </summary>
			public const string StockSubcontractBOM = "svg:manufacturing@matlstockbomsubcontract";

			/// <summary>
			/// Stock Item – No BOM – Phantom
			/// </summary>
			public const string StockPhantomNoBOM = "svg:manufacturing@matlstockphantom";

			/// <summary>
			/// Stock Item – No BOM – Regular
			/// </summary>
			public const string StockRegularNoBOM = "svg:manufacturing@matlstockregular";

			/// <summary>
			/// Stock Item – No BOM – Subcontract
			/// </summary>
			public const string StockSubcontractNoBOM = "svg:manufacturing@matlstocksubcontract";
		}
	}
}
