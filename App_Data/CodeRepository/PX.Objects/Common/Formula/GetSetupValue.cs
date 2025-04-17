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

using PX.Data;

namespace PX.Objects.Common
{
	public class GetSetupValue<Field> : BqlFormulaEvaluator<Field>, IBqlOperand
		where Field : IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			Type table = BqlCommand.GetItemType(typeof(Field));
			PXCache cacheSetup = cache.Graph.Caches[table];
			string fieldName = cacheSetup.GetField(typeof(Field));

			object setup = new PXView(cache.Graph, true, BqlCommand.CreateInstance(typeof(Select<>), table)).SelectSingle();

			return cacheSetup.GetValue(setup, fieldName);
		}
	}
}
