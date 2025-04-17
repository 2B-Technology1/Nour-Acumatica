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
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class UpdateSOOrder : PXGraphExtension<SOInvoiceEntry>
	{
		public PXSelect<SOLine2> soline;
		public PXSelect<SOMiscLine2> somiscline;
		public PXSelect<SOTax> sotax;
		public PXSelect<SOTaxTran> sotaxtran;
		public PXSelect<SOOrder> soorder;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<SOLine2, Where<SOLine2.orderType, Equal<Current<ARTran.sOOrderType>>, And<SOLine2.orderNbr, Equal<Current<ARTran.sOOrderNbr>>, And<SOLine2.lineNbr, Equal<Current<ARTran.sOOrderLineNbr>>>>>>), LeaveChildren = true)]
		[PXParent(typeof(Select<SOMiscLine2, Where<SOMiscLine2.orderType, Equal<Current<ARTran.sOOrderType>>, And<SOMiscLine2.orderNbr, Equal<Current<ARTran.sOOrderNbr>>, And<SOMiscLine2.lineNbr, Equal<Current<ARTran.sOOrderLineNbr>>>>>>), LeaveChildren = true)]
		protected virtual void ARTran_SOOrderLineNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUnboundFormula(typeof(BaseBilledQtyFormula), typeof(AddCalc<SOLine2.baseBilledQty>), EnableAggregation = false)]
		[PXUnboundFormula(typeof(BaseBilledQtyFormula), typeof(SubCalc<SOLine2.baseUnbilledQty>), EnableAggregation = false)]
		[PXUnboundFormula(typeof(BaseBilledQtyFormula), typeof(AddCalc<SOMiscLine2.baseBilledQty>), EnableAggregation = false)]
		[PXUnboundFormula(typeof(BaseBilledQtyFormula), typeof(SubCalc<SOMiscLine2.baseUnbilledQty>), EnableAggregation = false)]
		protected virtual void ARTran_BaseQty_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUnboundFormula(typeof(CuryBilledAmtFormula), typeof(AddCalc<SOMiscLine2.curyBilledAmt>), EnableAggregation = false)]
		[PXUnboundFormula(typeof(CuryBilledAmtFormula), typeof(SubCalc<SOMiscLine2.curyUnbilledAmt>), EnableAggregation = false)]
		protected virtual void ARTran_CuryTranAmt_CacheAttached(PXCache sender)
		{
		}

		[PXOverride]
		public virtual void ARTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e, PXRowDeleted baseMethod)
		{
			baseMethod(sender, e);

			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, null, e.Row, typeof(SOLine2.baseBilledQty));
			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, null, e.Row, typeof(SOLine2.baseUnbilledQty));
			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, null, e.Row, typeof(SOMiscLine2.baseBilledQty));
			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, null, e.Row, typeof(SOMiscLine2.baseUnbilledQty));

			PXFormulaAttribute.CalcAggregate<ARTran.curyTranAmt>(sender, null, e.Row, typeof(SOMiscLine2.curyBilledAmt));
			PXFormulaAttribute.CalcAggregate<ARTran.curyTranAmt>(sender, null, e.Row, typeof(SOMiscLine2.curyUnbilledAmt));
		}

		[PXOverride]
		public virtual void ARTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted baseMethod)
		{
			baseMethod(sender, e);

			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, e.Row, null, typeof(SOLine2.baseBilledQty));
			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, e.Row, null, typeof(SOLine2.baseUnbilledQty));
			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, e.Row, null, typeof(SOMiscLine2.baseBilledQty));
			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, e.Row, null, typeof(SOMiscLine2.baseUnbilledQty));

			PXFormulaAttribute.CalcAggregate<ARTran.curyTranAmt>(sender, e.Row, null, typeof(SOMiscLine2.curyBilledAmt));
			PXFormulaAttribute.CalcAggregate<ARTran.curyTranAmt>(sender, e.Row, null, typeof(SOMiscLine2.curyUnbilledAmt));
		}

		[PXOverride]
		public virtual void ARTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e, PXRowUpdated baseMethod)
		{
			baseMethod(sender, e);

			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, e.Row, e.OldRow, typeof(SOLine2.baseBilledQty));
			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, e.Row, e.OldRow, typeof(SOLine2.baseUnbilledQty));
			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, e.Row, e.OldRow, typeof(SOMiscLine2.baseBilledQty));
			PXFormulaAttribute.CalcAggregate<ARTran.baseQty>(sender, e.Row, e.OldRow, typeof(SOMiscLine2.baseUnbilledQty));

			PXFormulaAttribute.CalcAggregate<ARTran.curyTranAmt>(sender, e.Row, e.OldRow, typeof(SOMiscLine2.curyBilledAmt));
			PXFormulaAttribute.CalcAggregate<ARTran.curyTranAmt>(sender, e.Row, e.OldRow, typeof(SOMiscLine2.curyUnbilledAmt));
		}

		protected virtual void SOLine2_BaseShippedQty_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.ExcludeFromInsertUpdate();
			}
		}

		protected virtual void SOLine2_ShippedQty_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.ExcludeFromInsertUpdate();
			}
		}

		protected virtual void _(Events.RowInserted<SOOrderShipment> e)
		{
			if (e.Row.OrderTaxAllocated != true)
				return;

			var order = (SOOrder)PXParentAttribute.SelectParent(e.Cache, e.Row, typeof(SOOrder));
			if (order != null)
			{
				order.OrderTaxAllocated = true;
				e.Cache.Graph.Caches<SOOrder>().Update(order);
			}
		}
		protected virtual void _(Events.RowUpdated<SOOrderShipment> e)
		{
			if (e.Row.OrderTaxAllocated != true && e.OldRow.OrderTaxAllocated != true || e.Row.OrderTaxAllocated == e.OldRow.OrderTaxAllocated)
				return;

			var order = (SOOrder)PXParentAttribute.SelectParent(e.Cache, e.Row, typeof(SOOrder));
			if (order != null)
			{
				order.OrderTaxAllocated = (e.Row.OrderTaxAllocated == true);
				e.Cache.Graph.Caches<SOOrder>().Update(order);
			}
		}
		protected virtual void _(Events.RowDeleted<SOOrderShipment> e)
		{
			if (e.Row.OrderTaxAllocated != true)
				return;

			var order = (SOOrder)PXParentAttribute.SelectParent(e.Cache, e.Row, typeof(SOOrder));
			if (order != null)
			{
				order.OrderTaxAllocated = false;
				e.Cache.Graph.Caches<SOOrder>().Update(order);
			}
		}
	}
}
