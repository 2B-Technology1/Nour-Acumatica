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
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	public class TaxDetailProjectDefaultAttribute : ProjectDefaultAttribute
	{
		protected Type TaxCategoryField { get; set; }

		public TaxDetailProjectDefaultAttribute(Type taxCategoryField) : this(null, taxCategoryField)
		{
		}

		public TaxDetailProjectDefaultAttribute(string module, Type taxCategoryField) : base(module)
		{
			TaxCategoryField = taxCategoryField;
		}

		public TaxDetailProjectDefaultAttribute(string module, Type search, Type taxCategoryField) : this(module, search, null, taxCategoryField)
		{
		}

		public TaxDetailProjectDefaultAttribute(string module, Type search, Type account, Type taxCategoryField) : base(module, search, account)
		{
			TaxCategoryField = taxCategoryField;
		}

		public override void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && (string)cache.GetValue(e.Row, TaxCategoryField.Name) == TaxCategory.EmployerTax)
			{
				base.FieldDefaulting(cache, e);
			}
		}
	}
}
