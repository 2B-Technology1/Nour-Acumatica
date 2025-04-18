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

namespace PX.Objects.PR
{
	public class DedBenApplicableEarningsAttribute : PXStringListAttribute, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber, IPXFieldSelectingSubscriber, IPXRowSelectedSubscriber
	{
		private Type _CalculationMethodField;

		public const string TotalEarnings = "TOT";
		public const string TotalEarningsWithOTMult = "TOM";
		public const string RegularEarnings = "REG";
		public const string RegularAndOTEarnings = "ROT";
		public const string RegularAndOTEarningsWithPTO = "ROV";
		public const string RegularAndOTEarningsWithOTMult = "ROM";
		public const string StraightTimeEarnings = "STR";
		public const string StraightTimeEarningsWithPTO = "STV";

		private readonly string[] AmountPerHourValues = new string[] { TotalEarnings, RegularEarnings, RegularAndOTEarnings, TotalEarningsWithOTMult, RegularAndOTEarningsWithOTMult };
		private readonly string[] AmountPerHourLabels = new string[] { Messages.TotalEarnings, Messages.RegularEarnings, Messages.RegularAndOTEarnings, Messages.TotalEarningsWithOTMultiplier, Messages.RegularAndOTEarningsWithOTMultiplier };
		private readonly string[] PercentOfGrossValues = new string[] { TotalEarnings, RegularEarnings, RegularAndOTEarnings, RegularAndOTEarningsWithPTO, StraightTimeEarnings, StraightTimeEarningsWithPTO };
		private readonly string[] PercentOfGrossLabels = new string[] { Messages.TotalEarnings, Messages.RegularEarnings, Messages.RegularAndOTEarnings, Messages.RegularAndOTEarningsWithPTO, Messages.StraightTimeEarnings, Messages.StraightTimeEarningsWithPTO };
		private readonly string[] PercentOfCustomValues = new string[] { TotalEarnings, StraightTimeEarnings };
		private readonly string[] PercentOfCustomLabels = new string[] { Messages.TotalEarnings, Messages.StraightTimeEarnings };

		public DedBenApplicableEarningsAttribute(Type calculationMethodField)
		{
			_CalculationMethodField = calculationMethodField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CalculationMethodField.Name, (cache, e) =>
			{
				string calcType = (string)sender.GetValue(e.Row, _CalculationMethodField.Name);
				if (!IsVisible(calcType))
				{
					cache.SetValue(e.Row, _FieldName, null);
				}
				else if (!IsVisible(e.OldValue as string))
				{
					cache.SetDefaultExt(e.Row, _FieldName);
				}
				else
				{
					GetAvailableValuesAndLabels(calcType, out string[] values, out string[] _);
					if (!values.Contains(cache.GetValue(e.Row, _FieldName)))
					{
						cache.SetDefaultExt(e.Row, _FieldName);
					}
				}
			});
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (IsVisible(sender, e.Row))
			{
				e.NewValue = TotalEarnings;
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsVisible(sender, e.Row) && sender.GetValue(e.Row, _FieldName) == null)
			{
				sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{_FieldName}]"));
			}
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			string calcType = (string)sender.GetValue(e.Row, _CalculationMethodField.Name);
			bool visible = IsVisible(calcType);

			if (visible)
			{
				GetAvailableValuesAndLabels(calcType, out string[] values, out string[] labels);
				SetList(sender, e.Row, _FieldName, values, labels);
			}

			PXUIFieldAttribute.SetVisible(sender, e.Row, _FieldName, visible);
			PXUIFieldAttribute.SetRequired(sender, _FieldName, visible);
		}

		private void GetAvailableValuesAndLabels(string calcType, out string[] values, out string[] labels)
		{
			labels = null;
			values = null;
			switch (calcType)
			{
				case DedCntCalculationMethod.AmountPerHour:
					labels = AmountPerHourLabels;
					values = AmountPerHourValues;
					break;
				case DedCntCalculationMethod.PercentOfGross:
					labels = PercentOfGrossLabels;
					values = PercentOfGrossValues;
					break;
				case DedCntCalculationMethod.PercentOfCustom:
					labels = PercentOfCustomLabels;
					values = PercentOfCustomValues;
					break;
			}
		}

		private bool IsVisible(PXCache sender, object row)
		{
			if (row == null)
			{
				return false;
			}

			string calcType = (string)sender.GetValue(row, _CalculationMethodField.Name);
			return IsVisible(calcType);
		}

		private bool IsVisible(string calculationType)
		{
			return calculationType == DedCntCalculationMethod.PercentOfGross
				|| calculationType == DedCntCalculationMethod.PercentOfCustom
				|| calculationType == DedCntCalculationMethod.AmountPerHour;
		}

		public class totalEarnings : PX.Data.BQL.BqlString.Constant<totalEarnings>
		{
			public totalEarnings() : base(TotalEarnings) { }
		}

		public class regularEarnings : PX.Data.BQL.BqlString.Constant<regularEarnings>
		{
			public regularEarnings() : base(RegularEarnings) { }
		}

		public class regularAndOTEarnings : PX.Data.BQL.BqlString.Constant<regularAndOTEarnings>
		{
			public regularAndOTEarnings() : base(RegularAndOTEarnings) { }
		}

		public class regularAndOTEarningsWithPTO : PX.Data.BQL.BqlString.Constant<regularAndOTEarningsWithPTO>
		{
			public regularAndOTEarningsWithPTO() : base(RegularAndOTEarningsWithPTO) { }
		}

		public class straightTimeEarnings : PX.Data.BQL.BqlString.Constant<straightTimeEarnings>
		{
			public straightTimeEarnings() : base(StraightTimeEarnings) { }
		}

		public class straightTimeEarningsWithPTO : PX.Data.BQL.BqlString.Constant<straightTimeEarningsWithPTO>
		{
			public straightTimeEarningsWithPTO() : base(StraightTimeEarningsWithPTO) { }
		}
	}
}
