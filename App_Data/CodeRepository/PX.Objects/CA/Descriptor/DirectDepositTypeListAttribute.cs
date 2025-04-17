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

using System.Collections.Generic;
using CommonServiceLocator;
using PX.Data;

namespace PX.Objects.CA
{
	public partial class DirectDepositTypeListAttribute : PXStringListAttribute
	{
		[InjectDependency]
		protected DirectDepositTypeService DirectDepositService { get; set; }

		public override void CacheAttached(PXCache sender)
		{
			//workaround for unit tests environment
			var records = DirectDepositService?.GetDirectDepositTypes() ?? new List<DirectDepositType>();
			List<string> codes = new List<string>();
			List<string> descriptions = new List<string>();

			foreach (var record in records)
			{
				codes.Add(record.Code);
				descriptions.Add(record.Description);
			}
			_AllowedValues = codes.ToArray();
			_AllowedLabels = descriptions.ToArray();

			base.CacheAttached(sender);
		}
	}
}
