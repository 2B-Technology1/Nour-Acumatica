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

using PX.Common;
using PX.Data;
using System;

namespace PX.Objects.EP
{
	public class EPVerifyEndDateAttribute : PXVerifyEndDateAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertedSubscriber
	{
		public EPVerifyEndDateAttribute(Type startDateField) :
			base(startDateField) { }

		void IPXRowInsertedSubscriber.RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DateTime? startDateTime = (DateTime?)sender.GetValue(e.Row, _startDateField);
			DateTime? endDateTime = (DateTime?)sender.GetValue(e.Row, _FieldName);

			try
			{
				Verifying(sender, e.Row, startDateTime, endDateTime, _startDateField, endDateTime);
			}
			catch (PXSetPropertyException ex)
			{
				sender.RaiseExceptionHandling(_FieldName, e.Row, endDateTime?.ToShortDateString(), ex);
			}
		}

		protected override void Verifying(PXCache sender, object row, DateTime? startDateTime, DateTime? endDateTime, string fieldName, DateTime? newValue)
		{
			if (
				startDateTime != null
				&& endDateTime != null
				&& startDateTime?.Date > endDateTime?.Date
			)
			{
				if (AllowAutoChange)
				{
					sender.SetValueExt(row, fieldName, newValue);

					if (newValue?.Date < PXTimeZoneInfo.Now.Date)
					{
						return;
					}

					if (AutoChangeWarning)
						sender.RaiseExceptionHandling(
							fieldName,
							row,
							endDateTime,
							new PXSetPropertyException(
								InfoMessages.ChangedAutomatically,
								PXErrorLevel.Warning,
								$"[{fieldName}]",
								newValue?.ToShortDateString()
							)
						);
				}
				else if (fieldName == _FieldName) // start date changed
				{
					throw new PXSetPropertyException(
						ErrorMessages.StartDateGreaterThanEndDate,
						PXUIFieldAttribute.GetDisplayName(sender, _startDateField),
						$"[{_FieldName}]",
						endDateTime?.ToShortDateString());
				}
				else
				{
					throw new PXSetPropertyException(
						ErrorMessages.EndDateLessThanStartDate,
						$"[{_FieldName}]",
						PXUIFieldAttribute.GetDisplayName(sender, _startDateField),
						startDateTime?.ToShortDateString());
				}
			}
		}
	}
}