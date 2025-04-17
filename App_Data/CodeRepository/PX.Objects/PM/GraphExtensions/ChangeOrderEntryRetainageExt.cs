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
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO;
using System;

namespace PX.Objects.PM.GraphExtensions
{
	public class ChangeOrderEntryRetainageExt: PXGraphExtension<ChangeOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.changeOrder>()
				&& PXAccess.FeatureInstalled<FeaturesSet.retainage>();
		}

		protected virtual void _(Events.RowSelected<PMChangeOrderLine> e)
		{
			if (e.Row != null)
			{
				if (e.Row.LineType != ChangeOrderLineType.NewDocument)
				{
					var defaultRetainagePct = GetDefaultRetainagePct(e.Row);
					if (e.Row.RetainagePct.HasValue && e.Row.RetainagePct.GetValueOrDefault() != defaultRetainagePct)
					{
						e.Cache.RaiseExceptionHandling<PMChangeOrderLine.retainagePct>(
							e.Row,
							e.Row.RetainagePct,
							new PXSetPropertyException(
								Messages.CommitmentRetainagesDiffersFromDefaultWarning,
								PXErrorLevel.Warning,
								e.Row.RetainagePct.Value.ToString("f6"),
								defaultRetainagePct.ToString("f6")));
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.vendorID> e)
		{
			var defaultRetainagePct = GetDefaultRetainagePct(e.Row);

			if (!e.Row.RetainagePct.HasValue || defaultRetainagePct != e.Row.RetainagePct.GetValueOrDefault())
			{
				e.Cache.SetValueExt<PMChangeOrderLine.retainagePct>(e.Row, defaultRetainagePct);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.pOOrderNbr> e)
		{
			var defaultRetainagePct = GetDefaultRetainagePct(e.Row);

			if (!e.Row.RetainagePct.HasValue || defaultRetainagePct != e.Row.RetainagePct.GetValueOrDefault())
			{
				e.Cache.SetValueExt<PMChangeOrderLine.retainagePct>(e.Row, defaultRetainagePct);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.pOLineNbr> e)
		{
			var defaultRetainagePct = GetDefaultRetainagePct(e.Row);

			if (!e.Row.RetainagePct.HasValue || defaultRetainagePct != e.Row.RetainagePct.GetValueOrDefault())
			{
				e.Cache.SetValueExt<PMChangeOrderLine.retainagePct>(e.Row, defaultRetainagePct);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.amount> e)
		{
			e.Cache.RaiseFieldDefaulting<PMChangeOrderLine.retainageAmt>(e.Row, out var newValue);
			e.Cache.SetValueExt<PMChangeOrderLine.retainageAmt>(e.Row, newValue);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.retainagePct> e)
		{
			if (e.Row != null)
			{
				e.Cache.RaiseFieldDefaulting<PMChangeOrderLine.retainageAmt>(e.Row, out var newValue);

				var newRetainageAmt = newValue as decimal?;
				if (e.Row.RetainageAmt != newRetainageAmt)
				{
					e.Cache.SetValueExt<PMChangeOrderLine.retainageAmt>(e.Row, newRetainageAmt);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.retainageAmt> e)
		{
			if (e.Row != null)
			{
				e.Cache.RaiseFieldDefaulting<PMChangeOrderLine.retainagePct>(e.Row, out var newValue);

				var newRetainagePct = newValue as decimal?;
				if (e.Row.RetainagePct != newRetainagePct)
				{
					e.Cache.SetValueExt<PMChangeOrderLine.retainagePct>(e.Row, newRetainagePct);
				}

				e.Cache.SetValueExt<PMChangeOrderLine.retainageAmtInProjectCury>(e.Row, Base.GetAmountInProjectCurrency(e.Row.CuryID, e.Row.RetainageAmt));
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderLine, PMChangeOrderLine.retainagePct> e)
		{
			if (e.Row != null)
			{
				e.NewValue = e.Row.RetainageAmt.HasValue && e.Row.Amount.GetValueOrDefault() != 0m
					? Math.Abs(Math.Round(e.Row.RetainageAmt.Value / e.Row.Amount.GetValueOrDefault() * 100m, PMChangeOrderLine.retainagePct.Precision))
					: 0m;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderLine, PMChangeOrderLine.retainageAmt> e)
		{
			if (e.Row != null)
			{
				e.NewValue = CalculateRetainage(e.Row.Amount, e.Row.RetainagePct);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderLine, PMChangeOrderLine.retainageAmt> e)
		{
			if (e.Row != null)
			{
				var newValue = e.NewValue as decimal?;

				if (newValue.HasValue && e.Row.Amount.HasValue && newValue.Value * e.Row.Amount.Value < 0)
				{
					e.NewValue = e.OldValue;
				}
			}
		}

		[PXOverride]
		public virtual PMChangeOrderLine CreateChangeOrderLine(POLinePM poLine, Func<POLinePM, PMChangeOrderLine> baseMethod)
		{
			var result = baseMethod(poLine);
			result.RetainagePct = GetDefaultRetainagePct(result);

			return result;
		}

		[PXOverride]
		public virtual POLine ModifyExistsingLineInOrder(POOrderEntry target, PMChangeOrderLine line, Func<POOrderEntry, PMChangeOrderLine, POLine> baseMethod)
		{
			var result = baseMethod(target, line);

			if (line.RetainagePct.HasValue)
			{
				result.RetainagePct = line.RetainagePct;
				result.RetainageAmt = CalculateRetainage(result.CuryLineAmt, line.RetainagePct);

				result = target.Transactions.Update(result);
			}

			return result;
		}

		private static decimal CalculateRetainage(decimal? amount, decimal? retainagePct)
			=> amount.GetValueOrDefault() * retainagePct.GetValueOrDefault() / 100m;

		private decimal GetDefaultRetainagePct(PMChangeOrderLine line)
		{
			if (line == null)
				return 0m;

			if (!string.IsNullOrWhiteSpace(line.POOrderNbr))
			{
				var orderline = POLine.PK.Find(Base, line.POOrderType, line.POOrderNbr, line.POLineNbr);

				if (orderline != null)
				{
					return orderline.RetainagePct.GetValueOrDefault();
				}

				var order = POOrder.PK.Find(Base, line.POOrderType, line.POOrderNbr);

				if (order?.RetainageApply == true)
				{
					return order.DefRetainagePct.GetValueOrDefault();
				}
			}

			var vendor = Vendor.PK.Find(Base, line.VendorID);

			if (vendor?.RetainageApply == true)
			{
				return vendor.RetainagePct.GetValueOrDefault();
			}

			return 0m;
		}
	}
}
