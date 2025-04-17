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
using PX.Objects.CR;

namespace PX.Objects.PM
{
	public class ChangeOrderClassMaint : PXGraph<ChangeOrderClassMaint, PMChangeOrderClass>
	{
		public PXSelect<PMChangeOrderClass> Item;
		public PXSelect<PMChangeOrderClass, Where<PMChangeOrderClass.classID, Equal<Current<PMChangeOrderClass.classID>>>> ItemSettings;

		[PXViewName(CR.Messages.Attributes)]
		public CSAttributeGroupList<PMChangeOrderClass, PMChangeOrder> Mapping;

		protected virtual void _(Events.FieldVerifying<PMChangeOrderClass, PMChangeOrderClass.isCostBudgetEnabled> e)
		{
			if ((bool?)e.NewValue != true)
			{
				var select = new PXSelectJoin<PMChangeOrderBudget,
				InnerJoin<PMChangeOrder, On<PMChangeOrderBudget.refNbr, Equal<PMChangeOrder.refNbr>>>,
				Where<PMChangeOrderBudget.type, Equal<GL.AccountType.expense>,
				And<PMChangeOrder.classID, Equal<Current<PMChangeOrderClass.classID>>>>>(this);

				PMChangeOrderBudget res = select.SelectWindowed(0, 1);
				if (res != null)
				{
					throw new PXSetPropertyException<PMChangeOrderClass.isCostBudgetEnabled>(Messages.ClassContainsCostBudget);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderClass, PMChangeOrderClass.isRevenueBudgetEnabled> e)
		{
			if ((bool?)e.NewValue != true)
			{
				var select = new PXSelectJoin<PMChangeOrderBudget,
				InnerJoin<PMChangeOrder, On<PMChangeOrderBudget.refNbr, Equal<PMChangeOrder.refNbr>>>,
				Where<PMChangeOrderBudget.type, Equal<GL.AccountType.income>,
				And<PMChangeOrder.classID, Equal<Current<PMChangeOrderClass.classID>>>>>(this);

				PMChangeOrderBudget res = select.SelectWindowed(0, 1);
				if (res != null)
				{
					throw new PXSetPropertyException<PMChangeOrderClass.isRevenueBudgetEnabled>(Messages.ClassContainsRevenueBudget);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderClass, PMChangeOrderClass.isPurchaseOrderEnabled> e)
		{
			if ((bool?)e.NewValue != true)
			{
				var select = new PXSelectJoin<PMChangeOrderLine,
					InnerJoin<PMChangeOrder, On<PMChangeOrderLine.refNbr, Equal<PMChangeOrder.refNbr>>>,
					Where<PMChangeOrder.classID, Equal<Current<PMChangeOrderClass.classID>>>>(this);

				PMChangeOrderLine res = select.SelectWindowed(0, 1);
				if (res != null)
				{
					throw new PXSetPropertyException<PMChangeOrderClass.isPurchaseOrderEnabled>(Messages.ClassContainsDetails);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderClass, PMChangeOrderClass.isAdvance> e)
		{
			if ((bool?)e.NewValue != true)
			{
				var select = new PXSelectJoin<PMChangeRequest,
				InnerJoin<PMChangeOrder, On<PMChangeRequest.changeOrderNbr, Equal<PMChangeOrder.refNbr>>>,
				Where<PMChangeOrder.classID, Equal<Current<PMChangeOrderClass.classID>>>>(this);

				PMChangeRequest res = select.SelectWindowed(0, 1);
				if (res != null)
				{
					throw new PXSetPropertyException<PMChangeOrderClass.isAdvance>(Messages.ClassContainsCRs);
				}
			}
		}
	}
}
