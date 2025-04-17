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
using System.Globalization;

namespace PX.Objects.PR
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DayOfWeekAttribute : PXEventSubscriberAttribute, IPXRowUpdatedSubscriber, IPXRowInsertedSubscriber, IPXFieldSelectingSubscriber
	{
		private Type _DateField;

		public DayOfWeekAttribute(Type dateField)
		{
			_DateField = dateField;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			e.ReturnValue = GetDayOfWeek(sender, e.Row);
		}

		public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			UpdateDayOfWeek(sender, e.Row);
		}

		public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			UpdateDayOfWeek(sender, e.Row);
		}

		private void UpdateDayOfWeek(PXCache sender, object row)
		{
			if (row == null)
			{
				return;
			}

			sender.SetValue(row, base.FieldOrdinal, GetDayOfWeek(sender, row));
		}

		private string GetDayOfWeek(PXCache sender, object row)
		{
			DateTime? date = sender.GetValue(row, _DateField.Name) as DateTime?;
			if (date == null)
			{
				return null;
			}

			return CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(date.Value.DayOfWeek);
		}
	}
}
