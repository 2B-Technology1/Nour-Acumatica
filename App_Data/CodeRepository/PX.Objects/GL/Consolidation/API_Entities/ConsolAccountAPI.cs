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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.IO;

namespace PX.Objects.GL.Consolidation
{
	internal class ConsolAccountAPITmp
	{
		public virtual ApiProperty<string> AccountCD { get; set; }
		public virtual ApiProperty<string> Description { get; set; }

		public ConsolAccountAPITmp() { }
		public ConsolAccountAPITmp(string accountCD, string description)
		{
			AccountCD = new ApiProperty<string>(accountCD);
			Description = new ApiProperty<string>(description);
		}
	}
	internal class ConsolAccountAPI
	{
		public virtual string AccountCD { get; set; }
		public virtual string Description { get; set; }
	}
}
