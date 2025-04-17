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

namespace PX.Objects.PR
{
	public class AnnualBaseForCertifiedAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
	{
		private Type _AnnualizationExceptionField;
		private BaseRange _Range;

		public enum BaseRange
		{
			Weeks,
			Hours
		}

		public AnnualBaseForCertifiedAttribute(Type annualizationExceptionField, BaseRange range)
		{
			_AnnualizationExceptionField = annualizationExceptionField;
			_Range = range;
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			if ((GetValueExt(sender, e.Row, _AnnualizationExceptionField.Name) as bool?) == true)
			{
				return;
			}

			PRPayment payment = PXParentAttribute.SelectParent<PRPayment>(sender, e.Row);
			if (payment == null)
			{
				return;
			}

			PREmployee employee = PXSelectorAttribute.Select<PRPayment.employeeID>(sender.Graph.Caches[typeof(PRPayment)], payment) as PREmployee;
			if (employee == null)
			{
				return;
			}

			switch (_Range)
			{
				case BaseRange.Hours:
					e.NewValue = GetHoursPerYear(sender.Graph.Caches[typeof(PREmployee)], employee);
					break;
				case BaseRange.Weeks:
					e.NewValue = employee.StdWeeksPerYear;
					break;
			}
		}

		public static int GetHoursPerYear(PXCache cache, PREmployee employee)
		{
			decimal? hoursPerYear = employee.OverrideHoursPerYearForCertified == true && employee.HoursPerYearForCertified != null
						? employee.HoursPerYearForCertified : GetValueExt(cache, employee, nameof(employee.HoursPerYear)) as decimal?;
			return (int)Math.Round(hoursPerYear.GetValueOrDefault());
		}

		private static object GetValueExt(PXCache sender, object row, string fieldName)
		{
			object valueExt = sender.GetValueExt(row, fieldName);
			return valueExt is PXFieldState ? ((PXFieldState)valueExt).Value : valueExt;
		}
	}
}
