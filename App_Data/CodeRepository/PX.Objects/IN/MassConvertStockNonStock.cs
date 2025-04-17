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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN.DAC.Unbound;
using PX.Objects.IN.GraphExtensions.InventoryItemMaintExt;
using PX.Objects.IN.GraphExtensions.NonStockItemMaintExt;
using PX.Objects.SO.Interfaces;
using static PX.Data.PXImportAttribute;

namespace PX.Objects.IN
{
	[TableAndChartDashboardType]
	public class MassConvertStockNonStock : PXGraph<MassConvertStockNonStock>, IPXPrepareItems, IPXProcess
	{
		#region Data Members

		public PXSetup<INSetup> insetup;

		public PXFilter<MassConvertStockNonStockFilter> Filter;

		[PXFilterable]
		public PXFilteredProcessing<
			InventoryItem,
			MassConvertStockNonStockFilter,
			Where<InventoryItem.stkItem, Equal<Current<MassConvertStockNonStockFilter.stkItem>>,
				And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
				And<InventoryItem.isTemplate, Equal<False>,
				And<InventoryItem.templateItemID, IsNull,
				And<InventoryItem.kitItem, Equal<False>,
				And2<Where<InventoryItem.stkItem, Equal<True>,
					Or<InventoryItem.nonStockReceipt, Equal<True>, And<InventoryItem.nonStockShip, Equal<True>, And<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>>>>>,
				And2<Where<Current<MassConvertStockNonStockFilter.itemClassID>, IsNull, Or<InventoryItem.itemClassID, Equal<Current<MassConvertStockNonStockFilter.itemClassID>>>>,
				And<MatchUser>>>>>>>>>
			ItemList;
		public IEnumerable itemList() => GetItemsForProcessing();
		#endregion

		#region Actions

		public PXCancel<MassConvertStockNonStockFilter> Cancel;

		#endregion

		#region Initialization

		public MassConvertStockNonStock()
		{
			INSetup _ = insetup.Current;
			ItemList.SetSelected<InventoryItem.selected>();
			ItemList.SetProcessCaption(Messages.Process);
			ItemList.SetProcessAllCaption(Messages.ProcessAll);
			Actions["Schedule"].SetVisible(false);
		}

		#endregion

		#region Methods

		protected virtual IEnumerable GetItemsForProcessing()
		{
			BqlCommand cmd = ItemList.View.BqlSelect;
			var parameters = new List<object>();
			cmd = AppendFilter(cmd, parameters, Filter.Current);

			int startRow = PXView.StartRow;
			int totalRows = 0;

			var rows = new PXDelegateResult()
			{
				IsResultFiltered = true,
				IsResultSorted = true,
				IsResultTruncated = true,
			};
			rows.AddRange(cmd
				.CreateView(this, mergeCache: true)
				.Select(PXView.Currents, parameters.ToArray(), PXView.Searches,
						PXView.SortColumns, PXView.Descendings, PXView.Filters,
						ref startRow, PXView.MaximumRows, ref totalRows));
			PXView.StartRow = 0;

			return rows;
		}

		#endregion		

		#region Event Handlers

		protected virtual void _(Events.RowSelected<MassConvertStockNonStockFilter> e)
		{
			if (e.Row == null) return;

			e.Cache.AdjustUI()
				.For<MassConvertStockNonStockFilter.nonStockItemClassID>(a => a.Visible = (e.Row.StkItem == true))
				.For<MassConvertStockNonStockFilter.nonStockPostClassID>(a => a.Visible = a.Required = (e.Row.StkItem == true))
				.For<MassConvertStockNonStockFilter.stockItemClassID>(a => a.Visible = a.Required = (e.Row.StkItem == false))
				.SameFor<MassConvertStockNonStockFilter.stockPostClassID>()
				.SameFor<MassConvertStockNonStockFilter.stockValMethod>()
				.SameFor<MassConvertStockNonStockFilter.stockLotSerClassID>();

			ItemList.SetParametersDelegate(ValidateParameters);
			switch (e.Row.StkItem)
			{
				case true:
					ItemList.SetProcessDelegate<InventoryItemMaint>((graph, item) => ConvertStockToNonStock(graph, item, e.Row));
					break;
				case false:
					ItemList.SetProcessDelegate<NonStockItemMaint>((graph, item) => ConvertNonStockToStock(graph, item, e.Row));
					break;
			}

			ItemList.SetProcessEnabled(e.Row.StkItem != null);
			ItemList.SetProcessAllEnabled(e.Row.StkItem != null);
		}

		protected virtual void _(Events.FieldVerifying<MassConvertStockNonStockFilter.inventoryID> e)
		{
			if (e.ExternalCall && e.NewValue is int
				&& (e.Cache.GetValuePending<MassConvertStockNonStockFilter.action>(e.Row) ?? PXCache.NotSetValue) != PXCache.NotSetValue
				&& (e.Cache.GetValuePending<MassConvertStockNonStockFilter.stkItem>(e.Row) ?? PXCache.NotSetValue) != PXCache.NotSetValue
				&& ((e.Cache.GetValuePending<MassConvertStockNonStockFilter.stockPostClassID>(e.Row) ?? PXCache.NotSetValue) != PXCache.NotSetValue
				|| (e.Cache.GetValuePending<MassConvertStockNonStockFilter.nonStockPostClassID>(e.Row) ?? PXCache.NotSetValue) != PXCache.NotSetValue))
			{
				// it's the case of recommiting the filter after finished processing
				// crutch to clear inventoryID as it should be already converted and will break the UI
				e.NewValue = null;
			}
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.selected> e)
		{
			if (e.Row.StkItem != Filter.Current.StkItem)
			{
				e.Row.Selected = false;
			}
		}

		protected virtual void _(Events.RowUpdated<MassConvertStockNonStockFilter> e)
		{
			if (!e.Cache.ObjectsEqual<MassConvertStockNonStockFilter.stkItem>(e.Row, e.OldRow))
			{
				ItemList.Cache.Clear();
				ItemList.Cache.ClearQueryCache();
			}			
		}

		#endregion

		#region Processing Delegates

		protected virtual bool ValidateParameters(List<InventoryItem> list)
		{
			if (Filter.Current.StkItem == null)
			{
				throw new PXInvalidOperationException();
			}

			ValidateNonEmptyRequiredField<MassConvertStockNonStockFilter.nonStockPostClassID>(true);
			ValidateNonEmptyRequiredField<MassConvertStockNonStockFilter.stockItemClassID>(false);
			ValidateNonEmptyRequiredField<MassConvertStockNonStockFilter.stockPostClassID>(false);
			ValidateNonEmptyRequiredField<MassConvertStockNonStockFilter.stockValMethod>(false);

			if (PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>())
				ValidateNonEmptyRequiredField<MassConvertStockNonStockFilter.stockLotSerClassID>(false);

			var errors = PXUIFieldAttribute.GetErrors(Filter.Cache, Filter.Current, PXErrorLevel.Error);
			if (errors.Any())
			{
				throw new PXException(string.Join(Environment.NewLine, errors.Values));
			}

			return true;
		}

		private string ValidateNonEmptyRequiredField<TField>(bool stkField)
			where TField : IBqlField
		{
			object val = Filter.Cache.GetValue<TField>(Filter.Current);
			string message = (val == null) && (stkField == Filter.Current.StkItem)
				? PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, $"[{ typeof(TField).Name }]")
				: null;
			var exc = (message != null) ? new PXSetPropertyException(message, PXErrorLevel.Error) : null;
			Filter.Cache.RaiseExceptionHandling<TField>(Filter.Current, null, exc);
			return message;
		}

		private static void ConvertStockToNonStock(InventoryItemMaint graph, InventoryItem item, MassConvertStockNonStockFilter filter)
		{			
			graph.Clear();
			var convertExt = graph.FindImplementation<ConvertStockToNonStockExt>();
			graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(item.InventoryID);
			try
			{
				convertExt.convert.Press();
			}
			catch (PXRedirectRequiredException redirectExc)
			{
				var newGraph = (NonStockItemMaint)redirectExc.Graph;
				newGraph.Answers.ForceValidationInUnattendedMode = true;
				var copy = PXCache<InventoryItem>.CreateCopy(newGraph.Item.Current);
				copy.ItemClassID = filter.NonStockItemClassID;
				copy = PXCache<InventoryItem>.CreateCopy(newGraph.Item.Update(copy));
				copy.PostClassID = filter.NonStockPostClassID;
				newGraph.Item.Update(copy);
				newGraph.Save.Press();
			}
		}

		private static void ConvertNonStockToStock(NonStockItemMaint graph, InventoryItem item, MassConvertStockNonStockFilter filter)
		{			
			graph.Clear();
			var convertExt = graph.FindImplementation<ConvertNonStockToStockExt>();
			graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(item.InventoryID);
			try
			{
				convertExt.convert.Press();
			}
			catch (PXRedirectRequiredException redirectExc)
			{
				var newGraph = (InventoryItemMaint)redirectExc.Graph;
				newGraph.Answers.ForceValidationInUnattendedMode = true;
				var copy = PXCache<InventoryItem>.CreateCopy(newGraph.Item.Current);
				copy.ItemClassID = filter.StockItemClassID;
				copy = PXCache<InventoryItem>.CreateCopy(newGraph.Item.Update(copy));
				copy.PostClassID = filter.StockPostClassID;
				copy.ValMethod = filter.StockValMethod;
				copy.LotSerClassID = filter.StockLotSerClassID;
				newGraph.Item.Update(copy);
				newGraph.Save.Press();
			}
		}

		#endregion

		public virtual BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, MassConvertStockNonStockFilter filter)
		{
			return cmd;
		}

		#region PXImportAttribute.IPXPrepareItems and PXImportAttribute.IPXProcess implementations
		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values) =>  true;

		public virtual bool RowImporting(string viewName, object row) => row == null;

		public virtual bool RowImported(string viewName, object row, object oldRow) => oldRow == null;

		public virtual void PrepareItems(string viewName, IEnumerable items){}

		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode){}
		#endregion
	}
}
