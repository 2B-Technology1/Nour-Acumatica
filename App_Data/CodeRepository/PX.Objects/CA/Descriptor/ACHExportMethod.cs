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
using System;

namespace PX.Objects.CA
{
	public class ACHExportMethod
	{
		public const string ExportScenario = "E";
		public const string PlugIn = "P";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(new string[] { ExportScenario, PlugIn, },
										  new string[] { "Export Scenario", "U.S. ACH Plug-In" })
			{ }
		}

		public class exportScenario : PX.Data.BQL.BqlString.Constant<exportScenario>
		{
			public exportScenario() : base(ExportScenario) { }
		}

		public class plugIn : PX.Data.BQL.BqlString.Constant<plugIn>
		{
			public plugIn() : base(PlugIn) { }
		}
	}
}
