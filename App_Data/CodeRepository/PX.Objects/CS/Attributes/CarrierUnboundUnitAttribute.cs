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
using PX.Objects.IN;
using System;

namespace PX.Objects.CS
{
	public class CarrierUnboundUnitAttribute : INUnboundUnitAttribute, IPXFieldSelectingSubscriber, IPXFieldUpdatedSubscriber
	{
		public string UnitType { get; set; }
		public Type SourceUom { get; set; }

		public CarrierUnboundUnitAttribute(Type sourceUom, string unitType)
		{
			UnitType = unitType;
			SourceUom = sourceUom;
		}

		private bool ToDisplay(object row) => ((CarrierPlugin)row).UnitType == UnitType;

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
				e.ReturnValue = ToDisplay(e.Row) ? sender.GetValue(e.Row, SourceUom.Name) : null;
		}

		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null && ToDisplay(e.Row))
				sender.SetValue(e.Row, SourceUom.Name, sender.GetValue(e.Row, _FieldName));
		}

		public override void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.RowSelected(sender, e);

			if (e.Row == null)
				return;

			bool display = ToDisplay(e.Row);

			PXUIFieldAttribute.SetVisible(sender, e.Row, _FieldName, display);
			PXUIFieldAttribute.SetVisibility(sender, e.Row, _FieldName, display ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXDefaultAttribute.SetPersistingCheck(sender, _FieldName, e.Row, display ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}
	}
}
