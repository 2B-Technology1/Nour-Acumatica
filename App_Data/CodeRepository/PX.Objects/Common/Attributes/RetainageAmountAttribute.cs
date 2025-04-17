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
using PX.Objects.CS;
using PX.Objects.CM.Extensions;

namespace PX.Objects.Common
{
	/// Provides a special formula for the retainage amount calculation 
	/// depending on related retainage percent field. 
	/// The attribute will do next things: 
	/// correctly default retainage amount on the FieldDefaulting event; 
	/// calculate retainage amount value once any dependent field has been updated;
	/// verify retainage amount value and raise an error if it is incorrect.
	/// Note this attribute will affect only retainage amount value,
	/// for the retainage percent field use <see cref="RetainagePercentAttribute"/>.
	/// <param name="curyInfoIDField">CurrencyInfo field for the current line.</param>
	/// <param name="retainedAmtFormula">final amount for which retainage
	/// calculation should be applied. Expression or field may be used. Note, all
	/// included fields should NOT be surrounded by <see cref="Current{Field}"/> BQL 
	/// statement.</param>
	/// <param name="curyRetainageAmtField">field to which the attribute belongs
	/// (Presented in the currency of the document, see <see cref="APRegister.CuryID"/>).</param> 
	/// <param name="retainageAmtField">field to which the attribute belongs.
	/// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>).</param> 
	/// <param name="retainagePctField">retainage percent field.</param> 
	/// <example>
	/// [DBRetainageAmount(
	///    typeof(APTran.curyInfoID), 
	///     typeof(Sub<APTran.curyLineAmt, APTran.curyDiscAmt>),
	///     typeof(APTran.curyRetainageAmt),
	///     typeof(APTran.retainageAmt),
	///     typeof(APTran.retainagePct))]
	/// </example>
	/// </summary>
	[PXUIField(
		DisplayName = "Retainage Amount",
		Visibility = PXUIVisibility.Visible,
		FieldClass = nameof(FeaturesSet.Retainage))]
	public class RetainageAmountAttribute : PXAggregateAttribute
	{
		protected int _UIAttrIndex = -1;
		protected PXUIFieldAttribute UIAttribute => _UIAttrIndex == -1 ? null : (PXUIFieldAttribute)_Attributes[_UIAttrIndex];

		protected Type formulaType;
		protected Type verifyType;

		public RetainageAmountAttribute(
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainagePctField)
			: base()
		{
			Initialize();

			formulaType = BqlCommand.Compose(
				typeof(Switch<,>), typeof(Case<,>),
					typeof(Where<,,>), typeof(PendingValue<>), curyRetainageAmtField, typeof(IsPending),
					typeof(And<,>), typeof(UnattendedMode), typeof(Equal<False>),
					typeof(decimal0),
				typeof(Switch<,>), typeof(Case<,>), typeof(Where<,,>), retainagePctField, typeof(LessEqual<decimal100>),
					typeof(And<,>), retainagePctField, typeof(GreaterEqual<decimal0>),
					typeof(Mult<,>), retainedAmtFormula, typeof(Div<,>), typeof(IsNull<,>), retainagePctField, typeof(decimal0), typeof(decimal100), curyRetainageAmtField);

			verifyType = BqlCommand.Compose(
				typeof(Where<,,>), retainedAmtFormula, typeof(GreaterEqual<decimal0>),
					typeof(And<,,>), curyRetainageAmtField, typeof(GreaterEqual<decimal0>),
					typeof(And<,,>), curyRetainageAmtField, typeof(LessEqual<>), retainedAmtFormula,
					typeof(Or<,,>), retainedAmtFormula, typeof(LessEqual<decimal0>),
					typeof(And<,,>), curyRetainageAmtField, typeof(LessEqual<decimal0>),
					typeof(And<,>), curyRetainageAmtField, typeof(GreaterEqual<>), retainedAmtFormula);
		}

		protected virtual void Initialize()
		{
			_UIAttrIndex = -1;

			foreach (PXEventSubscriberAttribute attr in _Attributes)
			{
				if (attr is PXUIFieldAttribute)
				{
					_UIAttrIndex = _Attributes.IndexOf(attr);
				}
			}
		}

		public string DisplayName
		{
			get
			{
				return UIAttribute?.DisplayName;
			}
			set
			{
				if (UIAttribute != null)
				{
					UIAttribute.DisplayName = value;
				}
			}
		}

		internal static void AssertRetainageAmount(decimal availableRetainageAmount, decimal newRetainageAmountValue)
		{
			if (Math.Sign(availableRetainageAmount) * Math.Sign(newRetainageAmountValue) < 0m)
			{
				throw new PXSetPropertyException(AR.Messages.IncorrectExtPriceAndCuryRetainageAmt);
			}

			if (Math.Abs(availableRetainageAmount) < Math.Abs(newRetainageAmountValue))
			{
				throw new PXSetPropertyException(Math.Sign(newRetainageAmountValue) > 0
					? AR.Messages.IncorrectRetainagePositiveAmount
					: AR.Messages.IncorrectRetainageNegativeAmount, availableRetainageAmount);
			}
		}
	}

	public class DBRetainageAmountAttribute : RetainageAmountAttribute
	{
		public DBRetainageAmountAttribute(
			Type curyInfoIDField,
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainageAmtField,
			Type retainagePctField)
			: base(retainedAmtFormula, curyRetainageAmtField, retainagePctField)
		{
			_Attributes.Add(new PXDBCurrencyAttribute(curyInfoIDField, retainageAmtField));
			_Attributes.Add(new PXDefaultAttribute(TypeCode.Decimal, "0.0") { PersistingCheck = PXPersistingCheck.Nothing });
			_Attributes.Add(new PXFormulaAttribute(formulaType));
			_Attributes.Add(new PXUIVerifyAttribute(verifyType, PXErrorLevel.Error, AP.Messages.IncorrectRetainageAmount));
		}
	}

	[Obsolete(Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
	public class DBRetainageAmountOldCMAttribute : RetainageAmountAttribute
	{
		public DBRetainageAmountOldCMAttribute(
			Type curyInfoIDField,
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainageAmtField,
			Type retainagePctField)
			: base(retainedAmtFormula, curyRetainageAmtField, retainagePctField)
		{
			_Attributes.Add(new CM.PXDBCurrencyAttribute(curyInfoIDField, retainageAmtField));
			_Attributes.Add(new PXDefaultAttribute(TypeCode.Decimal, "0.0") { PersistingCheck = PXPersistingCheck.Nothing });
			_Attributes.Add(new PXFormulaAttribute(formulaType));
			_Attributes.Add(new PXUIVerifyAttribute(verifyType, PXErrorLevel.Error, AP.Messages.IncorrectRetainageAmount));
		}
	}

	public class UnboundRetainageAmountAttribute : RetainageAmountAttribute
	{
		public UnboundRetainageAmountAttribute(
			Type curyInfoIDField,
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainageAmtField,
			Type retainagePctField)
			: base(retainedAmtFormula, curyRetainageAmtField, retainagePctField)
		{
			_Attributes.Add(new PXCurrencyAttribute(curyInfoIDField, retainageAmtField));
			_Attributes.Add(new PXUnboundDefaultAttribute(TypeCode.Decimal, "0.0"));
			_Attributes.Add(new PXFormulaAttribute(formulaType));
			_Attributes.Add(new PXUIVerifyAttribute(verifyType, PXErrorLevel.Error, AP.Messages.IncorrectRetainageAmount, true));
		}
	}
}
