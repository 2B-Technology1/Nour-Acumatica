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
using System.Linq;

namespace PX.Objects.CA
{
	public class CABankFeedStatementStartDay : PXIntListAttribute
	{
		private static int[] days = Enumerable.Range(1, 31).ToArray();
		private static string[] dayLabels = Enumerable.Range(1, 31).Select(i => i.ToString()).ToArray();
		private static int[] daysOfWeek = Enumerable.Range(1, 7).ToArray();
		private static string[] dayOfWeekLabels = {Messages.Sunday, Messages.Monday, Messages.Tuesday,
			Messages.Wednesday, Messages.Thursday, Messages.Friday, Messages.Saturday
		};

		Type statementPeriodField;
		public CABankFeedStatementStartDay(Type periodField)
		{
			statementPeriodField = periodField;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string statementPeriod = sender.GetValue(e.Row, statementPeriodField.Name) as string;

			int[] allowedValues = new int[0];
			string[] allowedLabels = new string[0];

			if (statementPeriod == CABankFeedStatementPeriod.Month)
			{
				allowedValues = days;
				allowedLabels = dayLabels;
			}
			else if (statementPeriod == CABankFeedStatementPeriod.Week)
			{
				allowedValues = daysOfWeek;
				allowedLabels = dayOfWeekLabels;
			}
			else if(statementPeriod == CABankFeedStatementPeriod.Day)
			{
				allowedValues = new int[] { 1 };
				allowedLabels = new string[] { "1" };
			}

			_AllowedValues = allowedValues;
			_AllowedLabels = allowedLabels;
			base.FieldSelecting(sender, e);
		}
	}
}
