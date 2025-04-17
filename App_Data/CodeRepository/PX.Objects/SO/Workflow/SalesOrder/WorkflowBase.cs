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
using PX.Data.BQL;
using PX.Data.WorkflowAPI;
using PX.Objects.CR;

namespace PX.Objects.SO.Workflow.SalesOrder
{
	using static SOOrder;
	using static BoundedTo<SOOrderEntry, SOOrder>;

	public abstract class WorkflowBase : PXGraphExtension<ScreenConfiguration, SOOrderEntry>
	{
		public class Conditions : Condition.Pack
		{
			public Condition IsOnHold => GetOrCreate(b => b.FromBql<
				hold.IsEqual<True>
			>());

			public Condition IsCompleted => GetOrCreate(b => b.FromBql<
				completed.IsEqual<True>
			>());

			public Condition IsCancelled => GetOrCreate(b => b.FromBql<
				cancelled.IsEqual<True>
			>());

			public Condition IsOnCreditHold => GetOrCreate(b => b.FromBql<
				creditHold.IsEqual<True>
			>());

			public Condition IsHoldEntryOrLSEntryEnabled => GetOrCreate(b => b.FromBql<
				Where<SOOrderType.holdEntry.FromCurrent, Equal<True>,
				   Or<SOOrderType.requireLocation.FromCurrent, Equal<True>,
				   Or<SOOrderType.requireLotSerial.FromCurrent, Equal<True>>>>
			>());
		}

		public static void DisableWholeScreen(FieldState.IContainerFillerFields states)
		{
			states.AddTable<SOOrder>(state => state.IsDisabled());
			states.AddTable<SOLine>(state => state.IsDisabled());
			states.AddTable<SOTaxTran>(state => state.IsDisabled());
			states.AddTable<SOBillingAddress>(state => state.IsDisabled());
			states.AddTable<SOBillingContact>(state => state.IsDisabled());
			states.AddTable<SOShippingAddress>(state => state.IsDisabled());
			states.AddTable<SOShippingContact>(state => state.IsDisabled());
			states.AddTable<SOLineSplit>(state => state.IsDisabled());
			states.AddTable<CRRelation>(state => state.IsDisabled());
		}
	}
}
