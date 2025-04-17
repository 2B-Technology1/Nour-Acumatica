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
using PX.Data.SQLTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.Attributes
{
	public class PXDBCountAttribute : PXDBIntAttribute
	{
		protected Type DistinctByField { get; }

		public PXDBCountAttribute() { }

		public PXDBCountAttribute(Type distinctByField)
		{
			DistinctByField = distinctByField;
		}

		protected override void PrepareFieldName(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareFieldName(dbFieldName, e);

			if (DistinctByField == null)
				e.Expr = SQLExpression.Count();
			else
				e.Expr = SQLExpression.CountDistinct(new Column(DistinctByField.Name, new SimpleTable(BqlCommand.GetItemType(DistinctByField).Name)));
		}
	}
}
