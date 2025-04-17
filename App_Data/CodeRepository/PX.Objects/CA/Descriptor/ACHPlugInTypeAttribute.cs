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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.ACHPlugInBase;

namespace PX.Objects.CA
{
	public class ACHPlugInTypeAttribute : PXProviderTypeSelectorAttribute
	{
		private static Type[] _interfaces = { typeof(PX.ACHPlugInBase.IACHPlugIn) };

		public ACHPlugInTypeAttribute() : base(_interfaces)
		{ }

		public static IEnumerable<ProviderRec> GetPlugIns()
		{
			var records = PXProviderTypeSelectorAttribute.GetProviderRecs(_interfaces);
			return records;
		}

		protected override IEnumerable GetRecords()
		{
			return GetPlugIns();
		}
	}
}
