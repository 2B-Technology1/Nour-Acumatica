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
using PX.Data.SQLTree;

namespace PX.Objects.AP.BQL
{
	/// <summary>
	/// A BQL function that returns <see cref="APDocType.SignAmount(string)"/>
	/// </summary>
	public sealed class SignAmount<TDocType> : BqlFunction, IBqlOperand, IBqlCreator
		where TDocType : IBqlOperand
	{
		private IBqlCreator _source;

		/// <exclude/>
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			if (!getValue<TDocType>(ref _source, cache, item, pars, ref result, out value) || value == null)
				return;
			value = APDocType.SignAmount((string)value);
		}

		/// <exclude />
		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection) => true;
	}
}
