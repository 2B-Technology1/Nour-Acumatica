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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.Common.Exceptions;
using PX.Common;
using PX.Objects.PM;
using SOLineSplit3 = PX.Objects.PO.POOrderEntry.SOLineSplit3;
using SOLine5 = PX.Objects.PO.POOrderEntry.SOLine5;

namespace PX.Objects.PO.GraphExtensions.POOrderEntryExt
{
	public class Special : PXGraphExtension<POOrderEntry.MultiCurrency, POOrderEntry>
	{
		[PXLocalizable]
		public static class ExtensionMessages
		{
			public const string SpecialLineExistsDeleteOrder = "At least one line with a special-order item in the purchase order is linked to a sales order. Do you want to delete the purchase order?";
		}

		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.specialOrders>();

		public override void Initialize()
		{
			base.Initialize();
			Base.Delete.SetDynamicText(true);
		}

		#region Events
		#region POOrder

		protected virtual void _(Events.RowSelected<POOrder> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.SpecialLineCntr > 0)
			{
				Base.Delete.SetConfirmationMessage(ExtensionMessages.SpecialLineExistsDeleteOrder);
			}
			else
			{
				Base.Delete.SetConfirmationMessage(ActionsMessages.ConfirmDeleteExplicit);
			}
		}

		protected virtual void _(Events.FieldVerifying<POOrder, POOrder.cancelled> e)
		{
			if (e.Row?.SpecialLineCntr > 0 && e.Row.Cancelled == false && (bool?)e.NewValue == true)
			{
				if (Base.Document.Ask(Messages.Warning, Messages.SpecialLineExistsCancelOrder, MessageButtons.YesNo) != WebDialogResult.Yes)
				{
					e.NewValue = false;
					e.Cancel = true;
				}
			}
		}

		#endregion // POOrder
		#region POLine
		protected virtual void _(Events.RowSelected<POLine> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.IsSpecialOrder == true)
			{
				string message = GetUnitCostChangedMessage(e.Cache, e.Row);

				var error = PXUIFieldAttribute.GetErrorOnly<POLine.curyUnitCost>(e.Cache, e.Row);
				if (error == null)
				{
					PXUIFieldAttribute.SetWarning<POLine.curyUnitCost>(e.Cache, e.Row, message);
				}
			}

			PXSetPropertyException isSpecialOrderException = null;
			if (e.Row.IsSpecialOrder == true && e.Row.Completed != true)
			{
				var soline = GetSOLine(e.Row); // TODO: Special: Add a flag to POLine?
				if (soline?.Completed == true)
				{
					isSpecialOrderException = new PXSetPropertyException(
						Messages.SpecialLineCompleted, PXErrorLevel.RowWarning, soline.OrderType, soline.OrderNbr);
				}
			}
			else if (e.Row.SODeleted == true && e.Row.Completed != true)
			{
				isSpecialOrderException = new PXSetPropertyException(Messages.SpecialLineDeleted, PXErrorLevel.RowWarning);
			}
			e.Cache.RaiseExceptionHandling<POLine.isSpecialOrder>(e.Row, true, isSpecialOrderException);
		}

		protected virtual void _(Events.FieldVerifying<POLine, POLine.curyUnitCost> e)
		{
			if (e.Row.IsSpecialOrder == true && (decimal?)e.NewValue != e.Row.CuryUnitCost)
			{
				var soline = GetSOLine(e.Row);
				if (soline?.Completed == true)
					throw new PXSetPropertyException(Messages.TheUnitCostCannotBeChangedDueToCompletedSOLine, soline.OrderType, soline.OrderNbr);

				if (!IsSingleSpecialPOLine(e.Row))
					throw new PXSetPropertyException(Messages.TheUnitCostCannotBeChangedDueToPOLines, soline.OrderType, soline.OrderNbr);
			}
		}

		protected virtual void _(Events.FieldVerifying<POLine, POLine.uOM> e)
		{
			if (e.Row?.IsSpecialOrder == true && !object.Equals(e.OldValue, e.NewValue))
			{
				var soline = GetSOLine(e.Row);
				throw new PXSetPropertyException(Messages.CannotChangeFieldOnSpecialOrder, soline?.OrderType, soline?.OrderNbr);
			}
		}

		protected virtual void _(Events.FieldVerifying<POLine, POLine.projectID> e)
		{
			if (e.Row?.IsSpecialOrder == true && !object.Equals(e.OldValue, e.NewValue))
			{
				var project = PMProject.PK.Find(Base, e.Row.ProjectID);
				e.NewValue = project?.ContractCD;
				var soline = GetSOLine(e.Row);
				throw new PXSetPropertyException(Messages.CannotChangeFieldOnSpecialOrder, soline?.OrderType, soline?.OrderNbr);
			}
		}

		protected virtual void _(Events.FieldVerifying<POLine, POLine.taskID> e)
		{
			if (e.Row?.IsSpecialOrder == true && !object.Equals(e.OldValue, e.NewValue))
			{
				var task = PMTask.PK.Find(Base, e.Row.ProjectID, e.Row.TaskID);
				e.NewValue = task?.TaskCD;
				var soline = GetSOLine(e.Row);
				throw new PXSetPropertyException(Messages.CannotChangeFieldOnSpecialOrder, soline?.OrderType, soline?.OrderNbr);
			}
		}

		protected virtual void _(Events.FieldVerifying<POLine, POLine.costCodeID> e)
		{
			if (e.Row?.IsSpecialOrder == true && !object.Equals(e.OldValue, e.NewValue))
			{
				var costCode = PMCostCode.PK.Find(Base, e.Row.CostCodeID);
				e.NewValue = costCode?.CostCodeCD;
				var soline = GetSOLine(e.Row);
				throw new PXSetPropertyException(Messages.CannotChangeFieldOnSpecialOrder, soline?.OrderType, soline?.OrderNbr);
			}
		}

		protected virtual void _(Events.FieldVerifying<POLine, POLine.cancelled> e)
		{
			if (e.Row?.IsSpecialOrder == true && e.Row.Cancelled != true && (bool?)e.NewValue == true && Base.Document.Current?.Cancelled != true)
			{
				if (Base.Document.Ask(Messages.Warning, Messages.SpecialLineExistsCancelLine, MessageButtons.YesNo) != WebDialogResult.Yes)
				{
					e.NewValue = false;
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<POLine, POLine.cancelled> e)
		{
			if (e.Row?.IsSpecialOrder == true && e.Row.Cancelled == true && (bool?)e.OldValue != true)
				e.Cache.SetValueExt<POLine.isSpecialOrder>(e.Row, false);
		}

		protected virtual void _(Events.RowUpdated<POLine> e)
		{
			if (e.Row.IsSpecialOrder == true && e.Row.CuryUnitCost != e.OldRow.CuryUnitCost)
			{
				SOLine5 sOLine = GetSOLine(e.Row);

				if (sOLine == null)
					throw new RowNotFoundException(Base.Caches<SOLineSplit>(), e.Row.OrderType, e.Row.OrderNbr, e.Row.LineNbr);

				decimal? origCuryUnitCost = (decimal?)e.Cache.GetValueOriginal<POLine.curyUnitCost>(e.Row);

				sOLine.CuryUnitCost = e.Row.CuryUnitCost;
				sOLine.IsCostUpdatedOnPO = (origCuryUnitCost != e.Row.CuryUnitCost);
				Base.FixedDemandOrigSOLine.Update(sOLine);

				Base.Transactions.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.RowDeleting<POLine> e)
		{
			if (e.Row.IsSpecialOrder == true && Base.Document.Cache.GetStatus(Base.Document.Current) != PXEntryStatus.Deleted)
			{
				if (Base.Document.Ask(Messages.Warning, Messages.SpecialLineExistsDeleteLine, MessageButtons.YesNo) != WebDialogResult.Yes)
				{
					e.Cancel = true;
				}
			}
		}

		#endregion // POLine
		#endregion // Events

		#region Methods

		protected virtual string GetUnitCostChangedMessage(PXCache cache, POLine row)
		{
			const int DefaultPrecision = 2;
			int precision = ((PXDecimalState)cache.GetValueExt<POLine.curyUnitCost>(row))?.Precision ?? DefaultPrecision;

			decimal origCuryUnitCost = (decimal?)cache.GetValueOriginal<POLine.curyUnitCost>(row) ?? 0m;
			if (origCuryUnitCost == row.CuryUnitCost)
				return null;

			return PXMessages.LocalizeFormatNoPrefix(Messages.TheUnitCostInLinkedSOOrderWillBeChanged,
				origCuryUnitCost.ToString($"N{precision}"), (row.CuryUnitCost ?? 0m).ToString($"N{precision}"));
		}

		protected virtual SOLine5 GetSOLine(POLine poline)
		{
			return SelectFrom<SOLine5>
				.InnerJoin<SOLineSplit3>.On<SOLineSplit3.FK.OrderLine>
				.Where<SOLineSplit3.FK.POLine.SameAsCurrent>
				.View.SelectSingleBound(Base, new object[] { poline });
		}

		protected virtual bool IsSingleSpecialPOLine(POLine poline)
		{
			return SelectFrom<POLine>
				.Where<Exists<SelectFrom<SOLineSplit>
					.InnerJoin<SOLineSplit3>
						.On<SOLineSplit3.orderType.IsEqual<SOLineSplit.orderType>
							.And<SOLineSplit3.orderNbr.IsEqual<SOLineSplit.orderNbr>>
							.And<SOLineSplit3.lineNbr.IsEqual<SOLineSplit.lineNbr>>>
					.Where<SOLineSplit.FK.POLine
						.And<SOLineSplit3.FK.POLine.SameAsCurrent>>>>
				.View.ReadOnly.SelectMultiBound(Base, new object[] { poline }).Count == 1;
		}

		#endregion // Methods

		#region Overrides

		/// <summary>
		/// Overrides <see cref="Extensions.MultiCurrency.MultiCurrencyGraph{TGraph, TPrimary}.AllowOverrideCury"/>
		/// </summary>
		[PXOverride]
		public virtual bool AllowOverrideCury(Func<bool> baseMethod)
		{
			return baseMethod() && (Base.Document.Current?.SpecialLineCntr ?? 0) == 0;
		}

		/// <summary>
		/// Overrides <see cref="POOrderEntry.POLine_CuryUnitCost_FieldDefaulting(PXCache, PXFieldDefaultingEventArgs)"/>
		/// </summary>
		public virtual void _(Events.FieldDefaulting<POLine, POLine.curyUnitCost> e, PXFieldDefaulting baseEventHandler)
		{
			if (e.Row?.IsSpecialOrder == true)
			{
				e.NewValue = e.Row.CuryUnitCost ?? 0m;
				e.Cancel = true;
				return;
			}

			baseEventHandler?.Invoke(e.Cache, e.Args);
		}

		#endregion // Overrides
	}
}
