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
using PX.Payroll.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class SubjectToTaxes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { PerTaxEngine, All, None, AllButList, NoneButList },
					new string[] { Messages.PerTaxEngine, Messages.All, Messages.None, Messages.AllButList, Messages.NoneButList })
			{ }

			public ListAttribute(bool allowPerTaxEngine)
			{
				Stack<string> allowedValues = new Stack<string>(new[] { NoneButList, AllButList, None, All });
				Stack<string> allowedLabels = new Stack<string>(new[] { Messages.NoneButList, Messages.AllButList, Messages.None, Messages.All });
				if (allowPerTaxEngine)
				{
					allowedValues.Push(PerTaxEngine);
					allowedLabels.Push(Messages.PerTaxEngine);
				}

				_AllowedValues = allowedValues.ToArray();
				_AllowedLabels = allowedLabels.ToArray();
			}
		}

		public class ListForDeductionTaxabilityAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
		{
			private readonly Type _IsPayableBenefitField;

			public ListForDeductionTaxabilityAttribute(Type isPayableBenefitField = null)
				: base(
					new string[] { PerTaxEngine, NoneButList, AllButList },
					new string[] { Messages.DeductionPerTaxEngine, Messages.ReducedByDeduction, Messages.IncreasedByContribution })
			{
				_IsPayableBenefitField = isPayableBenefitField;
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				if (_IsPayableBenefitField != null)
					sender.Graph.FieldUpdated.AddHandler(_IsPayableBenefitField.DeclaringType, _IsPayableBenefitField.Name, CheckList);
			}

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				if (e.Row != null && _IsPayableBenefitField != null)
					SetList(sender, e.Row, _FieldName, GetAllowedValuesAndLabels(sender, e.Row));
			}

			private void CheckList(PXCache sender, PXFieldUpdatedEventArgs args)
			{
				if (args.Row == null)
					return;

				string currentValue = sender.GetValue(args.Row, FieldName) as string;
				Tuple<string, string>[] allowedValuesToLabels = GetAllowedValuesAndLabels(sender, args.Row);

				if (allowedValuesToLabels.Length == 1)
				{
					if (currentValue == null || allowedValuesToLabels.All(item => item.Item1 != currentValue))
						sender.SetValue(args.Row, FieldName, allowedValuesToLabels[0].Item1);
				}
				else if (currentValue != null && allowedValuesToLabels.All(item => item.Item1 != currentValue))
				{
					sender.SetValue(args.Row, FieldName, null);
				}
			}

			private Tuple<string, string>[] GetAllowedValuesAndLabels(PXCache sender, object row)
			{
				if (_IsPayableBenefitField != null && true.Equals(sender.GetValue(row, _IsPayableBenefitField.Name)))
				{
					return new[]
					{
						new Tuple<string, string>(AllButList, Messages.IncreasedByContribution)
					};
				}

				return new[]
				{
					new Tuple<string, string>(PerTaxEngine, Messages.DeductionPerTaxEngine),
					new Tuple<string, string>(NoneButList, Messages.ReducedByDeduction),
					new Tuple<string, string>(AllButList, Messages.IncreasedByContribution)
				};
			}
		}

		public class perTaxEngine : PX.Data.BQL.BqlString.Constant<perTaxEngine>
		{
			public perTaxEngine()
				: base(PerTaxEngine)
			{
			}
		}

		public class all : PX.Data.BQL.BqlString.Constant<all>
		{
			public all()
				: base(All)
			{
			}
		}

		public class none : PX.Data.BQL.BqlString.Constant<none>
		{
			public none()
				: base(None)
			{
			}
		}

		public class allButList : PX.Data.BQL.BqlString.Constant<allButList>
		{
			public allButList()
				: base(AllButList)
			{
			}
		}

		public class noneButList : PX.Data.BQL.BqlString.Constant<noneButList>
		{
			public noneButList()
				: base(NoneButList)
			{
			}
		}

		public const string PerTaxEngine = "PTE";
		public const string All = "ALL";
		public const string None = "NON";
		public const string AllButList = "ABL";
		public const string NoneButList = "NBL";

		public static bool IsFromList(string type)
		{
			return type == AllButList || type == NoneButList;
		}

		public static PRCustomItemCalculationMethod Get(string subjectTax)
		{
			switch (subjectTax)
			{
				case All:
					return PRCustomItemCalculationMethod.All;
				case None:
					return PRCustomItemCalculationMethod.None;
				case AllButList:
				case NoneButList:
					return PRCustomItemCalculationMethod.FromList;
				default:
					return 0;
			}
		}
	}
}
