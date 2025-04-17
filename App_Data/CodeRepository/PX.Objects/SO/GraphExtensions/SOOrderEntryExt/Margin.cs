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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.TX;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class Margin: PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

		#region Attribute overrides

		#region SOLine fields

		[PXMergeAttributes]
		[PXUnboundFormula(typeof(Switch<
			Case<
				Where<
					SOLine.behavior.IsEqual<SOBehavior.tR>
					.Or<SOLine.pOCreate.IsEqual<True>
						.And<SOLine.pOSource.IsIn<INReplenishmentSource.dropShipToOrder, INReplenishmentSource.blanketDropShipToOrder>>>>,
				decimal0>,
			SOLine.curyExtCost
				.Multiply<int_1.When<SOLine.invtMult.IsEqual<short0>>.Else<SOLine.invtMult>>
				.Multiply<int_1>>),
			typeof(SumCalc<SOOrder.curySalesCostTotal>))]
		protected virtual void _(Events.CacheAttached<SOLine.curyExtCost> e) { }

		[PXMergeAttributes]
		[PXFormula(typeof(Switch<
			Case<
				Where<
					SOLine.completed.IsEqual<True>>,
				SOLine.curyNetSales,
			Case<
				Where<
					SOLine.curyExtCost.IsEqual<decimal0>
					.Or<SOLine.behavior.IsEqual<SOBehavior.tR>>
					.Or<SOLine.pOCreate.IsEqual<True>.And<SOLine.pOSource.IsIn<INReplenishmentSource.dropShipToOrder, INReplenishmentSource.blanketDropShipToOrder>>>>,
				decimal0>>,
			SOLine.curyLineAmt.Multiply<SOLine.groupDiscountRate>.Multiply<SOLine.documentDiscountRate>
					.When<SOLine.curyTaxableAmt.IsEqual<decimal0>.Or<SOOrder.taxCalcMode.FromParent.IsEqual<TaxCalculationMode.net>>>
					.Else<SOLine.curyTaxableAmt>
				.Multiply<int_1.When<SOLine.invtMult.IsEqual<short0>>.Else<SOLine.invtMult>>
				.Multiply<int_1>>))]
		protected virtual void _(Events.CacheAttached<SOLine.curyNetSales> e) { }

		[PXMergeAttributes]
		[PXFormula(typeof(Switch<
			Case<
				Where<
					SOLine.curyExtCost.IsEqual<decimal0>
					.Or<SOLine.behavior.IsEqual<SOBehavior.tR>>
					.Or<SOLine.pOCreate.IsEqual<True>.And<SOLine.pOSource.IsIn<INReplenishmentSource.dropShipToOrder, INReplenishmentSource.blanketDropShipToOrder>>>>,
				Null,
			Case<
				Where<
					SOLine.curyExtPrice.IsEqual<decimal0>>,
				decimal0>>,
			SOLine.curyNetSales
				.Subtract<SOLine.curyExtCost
					.Multiply<int_1.When<SOLine.invtMult.IsEqual<short0>>.Else<SOLine.invtMult>>
					.Multiply<int_1>>>))]
		[PXUnboundFormula(typeof(int1
			.When<SOLine.behavior.IsNotEqual<SOBehavior.tR>
				.And<SOLine.curyUnitCost.IsEqual<decimal0>
					.Or<SOLine.pOCreate.IsEqual<True>.And<SOLine.pOSource.IsIn<INReplenishmentSource.dropShipToOrder, INReplenishmentSource.blanketDropShipToOrder>>>>>
			.Else<int0>), typeof(SumCalc<SOOrder.noMarginLineCntr>))]
		protected virtual void _(Events.CacheAttached<SOLine.curyMarginAmt> e) { }

		[PXMergeAttributes]
		[PXFormula(typeof(Switch<
			Case<
				Where<
					SOLine.curyExtCost.IsEqual<decimal0>
					.Or<SOLine.behavior.IsEqual<SOBehavior.tR>>
					.Or<SOLine.pOCreate.IsEqual<True>.And<SOLine.pOSource.IsIn<INReplenishmentSource.dropShipToOrder, INReplenishmentSource.blanketDropShipToOrder>>>>,
				Null,
			Case<
				Where<
					SOLine.curyExtPrice.IsEqual<decimal0>.Or<SOLine.curyNetSales.IsEqual<decimal0>>>,
				decimal0>>,
			SOLine.curyMarginAmt.Divide<Data.BQL.Abs<SOLine.curyNetSales>>.Multiply<decimal100>>))]
		protected virtual void _(Events.CacheAttached<SOLine.marginPct> e) { }

		#endregion

		#region SOOrder fields

		[PXMergeAttributes]
		[PXFormula(typeof(SOOrder.curyNetSalesTotal
			.Add<decimal0
					.When<SOOrder.behavior.IsEqual<SOBehavior.tR>>
				.Else<SOOrder.curyFreightTot
					.When<SOOrder.curyTaxableFreightAmt.IsEqual<decimal0>.Or<SOOrder.curyTaxTotal.IsEqual<decimal0>>.Or<SOOrder.taxCalcMode.IsEqual<TaxCalculationMode.net>>>
				.Else<SOOrder.curyTaxableFreightAmt>>
				.Multiply<decimal_1.When<SOOrder.defaultOperation.IsEqual<SOOperation.receipt>>.Else<decimal1>>>))]
		protected virtual void _(Events.CacheAttached<SOOrder.curyOrderNetSales> e) { }

		[PXMergeAttributes]
		[PXFormula(typeof(SOOrder.curySalesCostTotal
			.Add<decimal0.When<SOOrder.behavior.IsEqual<SOBehavior.tR>>.Else<SOOrder.curyFreightCost>
				.Multiply<decimal_1.When<SOOrder.defaultOperation.IsEqual<SOOperation.receipt>>.Else<decimal1>>>))]
		protected virtual void _(Events.CacheAttached<SOOrder.curyOrderCosts> e) { }

		[PXMergeAttributes]
		[PXFormula(typeof(SOOrder.curyOrderNetSales.Subtract<SOOrder.curyOrderCosts>))]
		protected virtual void _(Events.CacheAttached<SOOrder.curyMarginAmt> e) { }

		[PXMergeAttributes]
		[PXFormula(typeof(Switch<
			Case<Where<SOOrder.curyOrderNetSales.IsEqual<decimal0>>,
				decimal0>,
			SOOrder.curyMarginAmt
				.Divide<Data.BQL.Abs<SOOrder.curyOrderNetSales>>
				.Multiply<decimal100>>))]
		protected virtual void _(Events.CacheAttached<SOOrder.marginPct> e) { }

		#endregion

		#endregion

		#region SOOrder event handlers

		protected virtual void _(Events.RowSelected<SOOrder> e)
		{
			var visible = e.Row?.Behavior != SOBehavior.TR;

			e.Cache
				.Adjust<PXUIFieldAttribute>(e.Row)
				.For<SOOrder.marginPct>(a => a.Visible = visible)
				.SameFor<SOOrder.curyMarginAmt>();

			Base.Transactions.Cache
				.Adjust<PXUIFieldAttribute>(null)
				.For<SOLine.marginPct>(a => a.Visible = visible)
				.SameFor<SOLine.curyMarginAmt>();

			RaiseMarginWarning(e.Row);
		}

		#endregion

		protected virtual void RaiseMarginWarning(SOOrder order)
		{
			PXSetPropertyException warning;
			if (order == null || order.Behavior == SOBehavior.TR || order.NoMarginLineCntr == 0
				|| order.Cancelled == true || order.Completed == true || order.Status == SOOrderStatus.Voided)
				warning = null;
			else
				warning = new PXSetPropertyException<SOOrder.curyMarginAmt>(Messages.OrderHaveLinesWithoutMargin, PXErrorLevel.Warning);

			Base.Document.Cache.RaiseExceptionHandling<SOOrder.curyMarginAmt>(order, order?.CuryMarginAmt, warning);
		}

		/// Overrides <see cref="SOOrderEntry.OrderCreated(SOOrder, SOOrder)"/>
		[PXOverride]
		public void OrderCreated(SOOrder document, SOOrder source, Action<SOOrder, SOOrder> baseImpl)
		{
			baseImpl(document, source);

			EvaluateFormula<SOOrder.curyOrderNetSales>(Base.Document.Cache, document, null);
			EvaluateFormula<SOOrder.curyOrderCosts>(Base.Document.Cache, document, null);
		}

		protected virtual void EvaluateFormula<TField>(PXCache cache, object row, object oldRow)
			where TField : IBqlField
		{
			var formulaAttribute = cache.GetAttributesReadonly<TField>(row)
				.OfType<PXFormulaAttribute>()
				.FirstOrDefault();
			if (formulaAttribute == null)
				throw new PXArgumentException(nameof(formulaAttribute));

			var args = new PXFieldDefaultingEventArgs(row);
			formulaAttribute.FormulaDefaulting(cache, args);
			cache.SetValueExt<TField>(row, args.NewValue);

			if (formulaAttribute.Aggregate != null)
				formulaAttribute.RowUpdated(cache, new PXRowUpdatedEventArgs(row, oldRow, false));
		}

		public virtual void RequestRefreshLines()
		{
			Base.Transactions.View.RequestRefresh();
		}
	}
}
