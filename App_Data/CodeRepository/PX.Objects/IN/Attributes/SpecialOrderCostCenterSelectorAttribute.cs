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
using PX.Data.BQL.Dynamic;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.SO;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	public class SpecialOrderCostCenterSelectorAttribute : PXSelectorAttribute,
		IPXFieldUpdatedSubscriber, IPXRowSelectingSubscriber, IPXRowSelectedSubscriber
	{
		public Type InventoryIDField { get; protected set; }
		public Type SiteIDField { get; protected set; }
		public Type InvtMultField { get; protected set; }
		public Type CostCenterIDField { get; set; }
		public Type SOOrderTypeField { get; set; }
		public Type SOOrderNbrField { get; set; }
		public Type SOOrderLineNbrField { get; set; }
		public Type CostLayerTypeField { get; set; }
		public Type IsSpecialOrderField { get; set; }
		public Type OrigModuleField { get; set; }
		public Type ReleasedField { get; set; }
		public Type SelectorFromField { get; set; }
		public Type ProjectIDField { get; set; }
		public Type TaskIDField { get; set; }
		public Type CostCodeIDField { get; set; }
		public bool AllowEnabled { get; set; } = true;
		public bool CopyValueFromCostCenterID { get; set; }

		protected SpecialOrderCostCenterSelectorAttribute(Type selectType)
			: base(selectType)
		{
			_FieldList = new string[]
			{
				nameof(INCostCenter.SOOrderType),
				nameof(INCostCenter.SOOrderNbr),
				$"{nameof(SOLine)}__{nameof(SOLine.TranDesc)}",
				$"{nameof(SOLine)}__{nameof(SOLine.CuryUnitCost)}",
				nameof(INCostCenter.SOOrderLineNbr),
			};

			SubstituteKey = typeof(INCostCenter.costCenterCD);
		}

		public SpecialOrderCostCenterSelectorAttribute(Type inventoryIDField, Type siteIDField)
			: this(BqlTemplate.OfCommand<
								Search2<INCostCenter.costCenterID,
									InnerJoin<SOLine, On<INCostCenter.FK.OrderLine>>,
								Where<SOLine.inventoryID, Equal<Current2<BqlPlaceholder.A>>,
									And<INCostCenter.siteID, Equal<Current2<BqlPlaceholder.B>>>>>>
							.Replace<BqlPlaceholder.A>(inventoryIDField)
							.Replace<BqlPlaceholder.B>(siteIDField)
							.ToType())
		{
			InventoryIDField = inventoryIDField;
			SiteIDField = siteIDField;
		}

		public SpecialOrderCostCenterSelectorAttribute(Type inventoryIDField, Type siteIDField, Type invtMultField)
			: this(BqlTemplate.OfCommand<
								Search2<INCostCenter.costCenterID,
									InnerJoin<SOLine, On<INCostCenter.FK.OrderLine>,
									InnerJoin<INSiteStatusByCostCenter, On<SOLine.inventoryID, Equal<INSiteStatusByCostCenter.inventoryID>,
										And<SOLine.subItemID, Equal<INSiteStatusByCostCenter.subItemID>,
										And<INCostCenter.siteID, Equal<INSiteStatusByCostCenter.siteID>,
										And<INCostCenter.costCenterID, Equal<INSiteStatusByCostCenter.costCenterID>>>>>>>,
								Where<SOLine.inventoryID, Equal<Current2<BqlPlaceholder.A>>,
									And<INCostCenter.siteID, Equal<Current2<BqlPlaceholder.B>>,
									And<Where<
										Current2<BqlPlaceholder.C>, GreaterEqual<int0>,
										Or<INSiteStatusByCostCenter.qtyOnHand, Greater<decimal0>>>>>>>>
							.Replace<BqlPlaceholder.A>(inventoryIDField)
							.Replace<BqlPlaceholder.B>(siteIDField)
							.Replace<BqlPlaceholder.C>(invtMultField)
							.ToType())
		{
			InventoryIDField = inventoryIDField;
			SiteIDField = siteIDField;
			InvtMultField = invtMultField;
		}

		public SpecialOrderCostCenterSelectorAttribute(Type inventoryIDField, Type toSiteIDField, Type invtMultField, Type selectorFromField)
			: this(BqlTemplate.OfCommand<
								Search2<INCostCenter.costCenterID,
									InnerJoin<SOLine, On<INCostCenter.FK.OrderLine>>,
								Where<INCostCenter.costCenterID, Equal<Current2<BqlPlaceholder.A>>>>>
							.Replace<BqlPlaceholder.A>(selectorFromField)
							.ToType())
		{
			InventoryIDField = inventoryIDField;
			SelectorFromField = selectorFromField;
			InvtMultField = invtMultField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			PXFieldUpdated ClearValue(string fieldName)
				=> (sender, e) =>
				{
					if (!object.Equals(e.OldValue, sender.GetValue(e.Row, fieldName)))
						sender.SetValueExt(e.Row, FieldName, null);
				};

			if (AllowEnabled)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(InventoryIDField), InventoryIDField.Name, ClearValue(InventoryIDField.Name));

				if (SiteIDField != null)
					sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(SiteIDField), SiteIDField.Name, ClearValue(SiteIDField.Name));
			}

			if (SelectorFromField != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(SelectorFromField), SelectorFromField.Name, CopySpecialOrderNumber);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(CostLayerTypeField), CostLayerTypeField.Name, CopySpecialOrderNumber);
			}

			if (CopyValueFromCostCenterID)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(CostCenterIDField), CostCenterIDField.Name, (s,e) => CopyValue(s, e.Row));
			}
		}

		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			CopyValue(sender, e.Row);
		}

		protected virtual void CopyValue(PXCache sender, object row)
		{
			bool copyValue = false;

			if (CostLayerTypeField != null)
			{
				var costLayerType = sender.GetValue(row, CostLayerTypeField.Name) as string;
				copyValue = costLayerType == CostLayerType.Special;
			}
			else if (IsSpecialOrderField != null)
			{
				var isSpecialOrder = sender.GetValue(row, IsSpecialOrderField.Name) as bool?;
				copyValue = (isSpecialOrder == true);
			}

			if (copyValue && CostCenterIDField != null)
			{
				var value = sender.GetValue(row, CostCenterIDField.Name);
				sender.SetValue(row, FieldName, value);
			}
		}

		protected virtual void CopySpecialOrderNumber(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var toCostLayerType = (string)cache.GetValue(e.Row, CostLayerTypeField.Name);
			var fromSpecialOrder = (int?)cache.GetValue(e.Row, SelectorFromField.Name);
			var toSpecialOrder = (int?)cache.GetValue(e.Row, FieldName);

			if (toCostLayerType == CostLayerType.Special && fromSpecialOrder != toSpecialOrder)
				cache.SetValueExt(e.Row, FieldName, fromSpecialOrder);
		}

		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var newValue = sender.GetValue(e.Row, FieldName) as int?;
			if (object.Equals(newValue, e.OldValue))
				return;

			var costCenter = INCostCenter.PK.Find(sender.Graph, newValue);
			if (costCenter == null && newValue != null)
				throw new Common.Exceptions.RowNotFoundException(sender.Graph.Caches<INCostCenter>(), newValue);

			if (SOOrderTypeField != null)
				sender.SetValueExt(e.Row, SOOrderTypeField.Name, costCenter?.SOOrderType);

			if (SOOrderNbrField != null)
				sender.SetValueExt(e.Row, SOOrderNbrField.Name, costCenter?.SOOrderNbr);

			if (SOOrderLineNbrField != null)
				sender.SetValueExt(e.Row, SOOrderLineNbrField.Name, costCenter?.SOOrderLineNbr);

			if (CostCenterIDField != null)
				sender.SetValueExt(e.Row, CostCenterIDField.Name, newValue);

			if (IsSpecialOrderField != null)
				sender.SetValueExt(e.Row, IsSpecialOrderField.Name, newValue != null);

			var soLine = SOLine.PK.Find(sender.Graph, costCenter?.SOOrderType, costCenter?.SOOrderNbr, costCenter?.SOOrderLineNbr);
			if (soLine == null && newValue != null)
			{
				throw new Common.Exceptions.RowNotFoundException(sender.Graph.Caches<SOLine>(),
					costCenter?.SOOrderType, costCenter?.SOOrderNbr, costCenter?.SOOrderLineNbr);
			}

			if (ProjectIDField != null)
				sender.SetValueExt(e.Row, ProjectIDField.Name, soLine?.ProjectID ?? ProjectDefaultAttribute.NonProject());

			if (TaskIDField != null)
				sender.SetValueExt(e.Row, TaskIDField.Name, soLine?.TaskID);

			if (CostCodeIDField != null)
				sender.SetValueExt(e.Row, CostCodeIDField.Name, soLine?.CostCodeID);
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			bool enabled = AllowEnabled;
			if (enabled)
			{
				var costLayerType = sender.GetValue(e.Row, CostLayerTypeField.Name) as string;
				var origModule = sender.GetValue(e.Row, OrigModuleField.Name) as string;
				var released = sender.GetValue(e.Row, ReleasedField.Name) as bool?;
				enabled = costLayerType == CostLayerType.Special && released != true && origModule.IsIn(BatchModule.IN, INRegister.origModule.PI);
			}
			PXUIFieldAttribute.SetEnabled(sender, e.Row, FieldName, enabled);
		}
	}
}
