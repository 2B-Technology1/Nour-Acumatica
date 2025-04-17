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

using PX.Data;

namespace PX.Objects.Common.Attributes
{
	/// <summary>
	/// A <see cref="PXDefault"/>-like attribute supporting arbitrary
	/// BQL formulas as the providers of the default value.
	/// </summary>
	public class FormulaDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
	{
		public virtual IBqlCreator Formula
		{
			get;
			protected set;
		}
		
		public FormulaDefaultAttribute(Type formulaType)
		{
			Formula = PXFormulaAttribute.InitFormula(formulaType);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			object record = e.Row;

			if (e.Row == null || e.NewValue != null) return;

			bool? result = false;
			object value = null;

			BqlFormula.Verify(sender, record, Formula, ref result, ref value);

			if (value != null && value != PXCache.NotSetValue)
			{
				e.NewValue = value;
			}
		}
	}
}
