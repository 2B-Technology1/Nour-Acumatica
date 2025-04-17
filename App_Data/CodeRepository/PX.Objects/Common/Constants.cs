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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common
{
	public class Constants
	{
		public const int TranDescLength = 256;
		public const int TranDescLength512 = 512;
		public const string ProjectsWorkspaceID = "6dbfa68e-79e9-420b-9f64-e1036a28998c";
		public const string ProjectsWorkspaceIcon = "project";
		public const string ConstructionWorkspaceIcon = "cran";

		public class DACName<DAC> : PX.Data.BQL.BqlString.Constant<DACName<DAC>>
			where DAC : IBqlTable
		{
			public DACName() : base(typeof(DAC).FullName) { }
		}

		public class sevenInt : PX.Data.BQL.BqlInt.Constant<sevenInt>
		{
			public sevenInt() : base(7) { }
		}
	}
}
