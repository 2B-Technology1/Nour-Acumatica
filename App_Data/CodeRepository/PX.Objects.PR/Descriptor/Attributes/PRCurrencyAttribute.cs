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

namespace PX.Objects.PR
{
	public class PRCurrencyAttribute : PXDBDecimalAttribute, IPXRowInsertingSubscriber, IPXRowUpdatingSubscriber
	{
		public PRCurrencyAttribute() { }
		public PRCurrencyAttribute(int precision) : base(precision) { }

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			UpdateField(sender, e.Row);
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			UpdateField(sender, e.NewRow);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			UpdateField(sender, e.Row);

			base.RowPersisting(sender, e);
		}

		private void UpdateField(PXCache sender, object row)
		{
			object newValue = sender.GetValue(row, _FieldName);
			sender.RaiseFieldUpdating(_FieldName, row, ref newValue);
			sender.SetValue(row, _FieldName, newValue);
		}

		public static int? GetPrecision(PXCache cache, object data, string name)
		{
			foreach (PXEventSubscriberAttribute attribute in cache.GetAttributes(data, name))
			{
				if (attribute is PRCurrencyAttribute prCurrencyAttribute)
					return prCurrencyAttribute._Precision;
			}

			return null;
		}
	}
}
