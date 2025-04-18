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

using PX.Common;
using PX.Data;
using PX.Data.BQL;

using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC.Unbound;
using PX.Objects.IN.Matrix.Interfaces;
using PX.Objects.Common.Exceptions;
using PX.Objects.IN.Matrix.DAC;
using PX.Objects.IN.Matrix.Utility;
using PX.Objects.IN.Matrix.Attributes;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public abstract class SmartPanelExt<Graph, MainItemType> : MatrixGridExt<Graph, MainItemType>
			where Graph : PXGraph, new()
			where MainItemType : class, IBqlTable, new()
	{
		public override bool AddTotals => true;
		public override bool ShowDisabledValue => false;

		#region Types

		public class InventoryMatrixResult
		{
			public int InventoryID { get; set; }
			public decimal Qty { get; set; }
		}

		#endregion // Types

		#region Views

		[PXCopyPasteHiddenView]
		public PXSelectOrderBy<MatrixInventoryItem,
			OrderBy<Asc<MatrixInventoryItem.createdDateTime>>> MatrixItems;

		public virtual IEnumerable matrixItems()
		{
			return MatrixItems.Cache.Cached;
		}

		#endregion // Views

		public override void Initialize()
		{
			base.Initialize();

			Base.Views.Caches.Remove(typeof(MatrixInventoryItem));
		}

		public PXAction<MainItemType> showMatrixPanel;
		[PXUIField(DisplayName = "Add Matrix Items", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ShowMatrixPanel(PXAdapter adapter)
		{
			const WebDialogResult ChangeViewDialogResult = WebDialogResult.Yes;

			var tableAnswer = Header.View.GetAnswer(null);
			Header.View.SetAnswer(null, WebDialogResult.None);
			
			var matrixAnswer = Matrix.View.GetAnswer(null);
			Matrix.View.SetAnswer(null, WebDialogResult.None);

			if (tableAnswer == WebDialogResult.None && matrixAnswer == WebDialogResult.None)
			{
				ShowPanel();
			}
			else if (tableAnswer == ChangeViewDialogResult || matrixAnswer == ChangeViewDialogResult)
			{
				ShowPanel(true);
			}
			else if (tableAnswer == WebDialogResult.OK)
			{
				AddItemsToOrder(Header.Current.SiteID);
			}
			else if (matrixAnswer == WebDialogResult.OK)
			{
				AddMatrixItemsToOrder(Header.Current.SiteID);
			}

			return adapter.Get();
		}

		protected virtual void ShowPanel(bool changeType = false)
		{
			string panelType = Header.Current.SmartPanelType;

			if (changeType)
			{
				panelType = (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Entry) ?
					EntryHeader.smartPanelType.Lookup : EntryHeader.smartPanelType.Entry;
			}

			PXView.InitializePanel initPanel = (g, v) =>
			{
				Header.Current.SmartPanelType = panelType;
				RecalcAttributesGrid();
				RecalcMatrixGrid();
			};

			if (panelType == EntryHeader.smartPanelType.Entry)
			{
				Header.AskExt(initPanel);
			}
			else
			{
				Matrix.AskExt(initPanel);
			}
		}

		protected abstract bool IsDocumentOpen();

		protected abstract void UpdateLine(IMatrixItemLine line);

		protected abstract void CreateNewLine(int? siteID, int? inventoryID, decimal qty);

		protected abstract void CreateNewLine(int? siteID, int? inventoryID, string taxCategoryID, decimal qty, string uom);

		protected abstract IEnumerable<IMatrixItemLine> GetLines(int? siteID, int? inventoryID);

		protected abstract IEnumerable<IMatrixItemLine> GetLines(int? siteID, int? inventoryID, string taxCategoryID, string uom);

		protected abstract int? GetDefaultBranch();

		protected virtual bool IsItemStatusDisabled(InventoryItem item)
			=> item?.ItemStatus.IsIn(InventoryItemStatus.Inactive, InventoryItemStatus.MarkedForDeletion) == true;

		protected virtual string GetDefaultUOM(int? inventoryID)
			=> InventoryItem.PK.Find(Base, inventoryID)?.SalesUnit;

		#region Entry (Quick) View

		protected override CSAttribute[] GetAdditionalAttributes()
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				return base.GetAdditionalAttributes();
			}

			var item = GetTemplateItem();

			return new PXSelectReadonly2<CSAttribute,
				InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
				Where<CSAttributeGroup.isActive, Equal<True>,
					And<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>,
					And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
					And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
					And<NotExists<Select<CSAnswers,
						Where<CSAnswers.isActive.IsEqual<False>.
							And<CSAnswers.attributeID.IsEqual<CSAttribute.attributeID>>.
							And<CSAnswers.refNoteID.IsEqual<@P.AsGuid>>>>>>>>>>>(Base)
					.SelectMain(item?.ItemClassID, item?.NoteID);
		}

		protected override void AddFieldToAttributeGrid(PXCache cache, int attributeNumber)
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				base.AddFieldToAttributeGrid(cache, attributeNumber);
				return;
			}

			base.AddFieldToAttributeGrid(MatrixItems.Cache, attributeNumber);
		}

		protected override string GetAttributeValue(object row, int attributeNumber)
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				return base.GetAttributeValue(row, attributeNumber);
			}

			var item = row as MatrixInventoryItem;

			string returnValue = 0 <= attributeNumber && attributeNumber < item?.AttributeValueDescrs?.Length ? item.AttributeValueDescrs[attributeNumber] : null;
			if (string.IsNullOrEmpty(returnValue))
				returnValue = 0 <= attributeNumber && attributeNumber < item?.AttributeValues?.Length ? item.AttributeValues[attributeNumber] : null;

			return returnValue;
		}

		protected override void AttributeValueFieldUpdating(int attributeNumber, PXFieldUpdatingEventArgs e)
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				base.AttributeValueFieldUpdating(attributeNumber, e);
				return;
			}

			var row = e.Row as MatrixInventoryItem;
			if (row == null)
				return;

			string newValue = e.NewValue as string;

			if (attributeNumber < row.AttributeValueDescrs?.Length && row.AttributeValueDescrs[attributeNumber] != newValue)
			{
				var attributeDetailSelect = new PXSelect<CSAttributeDetail,
					Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>,
						And<CSAttributeDetail.valueID, Equal<Required<CSAttributeDetail.valueID>>>>>(Base);

				if (!ShowDisabledValue)
					attributeDetailSelect.WhereAnd<Where<CSAttributeDetail.disabled, NotEqual<True>>>();

				CSAttributeDetail attributeDetail = attributeDetailSelect.Select(
					AdditionalAttributes.Current.AttributeIdentifiers[attributeNumber], newValue);

				if (attributeDetail == null)
					throw new RowNotFoundException(Base.Caches<CSAttributeDetail>(), AdditionalAttributes.Current.AttributeIdentifiers[attributeNumber], newValue);

				row.AttributeValues[attributeNumber] = attributeDetail.ValueID;
				row.AttributeValueDescrs[attributeNumber] = attributeDetail.Description;

				if (AllAttributesArePopulated(row))
				{
					if (FindMatrixInventoryItem(row))
						throw new PXException(ErrorMessages.DuplicateEntryAdded);

					int? inventoryID = FindInventoryItem(row);

					if (inventoryID != null)
					{
						OnExisingItemSelected(row, inventoryID);
					}
					else
					{
						OnNewItemSelected(Header.Current.TemplateItemID, row);
						return;
					}
				}
			}
		}

		protected virtual bool FindMatrixInventoryItem(MatrixInventoryItem row)
		{
			return MatrixItems.Cache.Inserted
				.Cast<MatrixInventoryItem>()
				.Any(item =>
					item != row &&
					item.AttributeValues?.SequenceEqual(row.AttributeValues) == true);
		}

		protected virtual int? FindInventoryItem(MatrixInventoryItem row)
		{
			int? lastInventoryId = null;
			string[] attributeValues = new string[row.AttributeIDs.Length];

			foreach (PXResult<CSAnswers, InventoryItem> result in SelectInventoryWithAttributes())
			{
				InventoryItem inventoryItem = result;
				CSAnswers attribute = result;

				if (lastInventoryId != inventoryItem.InventoryID)
				{
					lastInventoryId = inventoryItem.InventoryID;
					for (int attributeIndex = 0; attributeIndex < attributeValues.Length; attributeIndex++)
						attributeValues[attributeIndex] = null;
				}

				for (int attributeIndex = 0; attributeIndex < attributeValues.Length; attributeIndex++)
				{
					if (string.Equals(row.AttributeIDs[attributeIndex], attribute.AttributeID, StringComparison.OrdinalIgnoreCase) &&
						row.AttributeValues[attributeIndex] == attribute.Value)
					{
						attributeValues[attributeIndex] = attribute.Value;
						break;
					}
				}


				if (lastInventoryId != null && attributeValues.All(v => v != null))
				{
					return lastInventoryId;
				}
			}

			return null;
		}

		protected virtual void OnExisingItemSelected(MatrixInventoryItem row, int? inventoryID)
		{
			var inventoryItem = InventoryItem.PK.Find(Base, inventoryID);
			if (inventoryItem == null)
				throw new RowNotFoundException(Base.Caches<InventoryItem>(), inventoryID);

			row.InventoryCD = inventoryItem.InventoryCD;
			row.InventoryID = inventoryItem.InventoryID;
			row.Descr = inventoryItem.Descr;
			row.New = false;

			int? branchID = GetDefaultBranch();
			var branch = GL.Branch.PK.Find(Base, branchID);
			var itemCurySettings = InventoryItemCurySettings.PK.Find(Base, inventoryID, branch?.BaseCuryID);
			row.BasePrice = itemCurySettings?.BasePrice;

			row.TaxCategoryID = inventoryItem.TaxCategoryID;
			row.Exists = false;
			var qtyInfo = GetQty(null, inventoryID);
			row.UOM = qtyInfo.UOM;
			row.UOMDisabled = (qtyInfo.Count > 0);
			row.Qty = IsItemStatusDisabled(inventoryItem) ? (decimal?)null : 0m;
			MatrixItems.Cache.Normalize();
			MatrixItems.View.RequestRefresh();
		}

		protected virtual void OnNewItemSelected(int? templateItemID, MatrixInventoryItem row)
		{
			InventoryItem templateItem = InventoryItem.PK.Find(Base, templateItemID);
			if (templateItem == null)
				throw new RowNotFoundException(Base.Caches<InventoryItem>(), templateItemID);

			var createHelper = GetCreateMatrixItemsHelper(Base);

			createHelper.GetGenerationRules(templateItemID,
				out List<INMatrixGenerationRule> idGenerationRules,
				out List<INMatrixGenerationRule> descrGenerationRules);

			object newCD = createHelper.GenerateMatrixItemID(templateItem, idGenerationRules, row);
			MatrixItems.Cache.RaiseFieldUpdating<MatrixInventoryItem.inventoryCD>(row, ref newCD);
			row.InventoryCD = (string)newCD;
			row.InventoryID = null;
			row.TemplateItemID = templateItem.InventoryID;

			if (PXDBLocalizableStringAttribute.IsEnabled)
			{
				PXCache templateCache = Base.Caches<InventoryItem>();

				DBMatrixLocalizableDescriptionAttribute.SetTranslations<MatrixInventoryItem.descr>
					(MatrixItems.Cache, row, (locale) =>
					{
						object newTranslation = createHelper.GenerateMatrixItemID(templateItem, descrGenerationRules, row, locale: locale);
						MatrixItems.Cache.RaiseFieldUpdating<MatrixInventoryItem.descr>(row, ref newTranslation);
						return (string)newTranslation;
					});
			}
			else
			{
				row.Descr = createHelper.GenerateMatrixItemID(templateItem, descrGenerationRules, row);
			}

			row.New = true;

			int? branchID = GetDefaultBranch();
			var branch = GL.Branch.PK.Find(Base, branchID);
			var itemCurySettings = InventoryItemCurySettings.PK.Find(Base, Header.Current?.TemplateItemID, branch?.BaseCuryID);
			row.BasePrice = itemCurySettings?.BasePrice;

			row.TaxCategoryID = templateItem.TaxCategoryID;
			row.Exists = (InventoryItem.UK.Find(Base, row.InventoryCD) != null);
			row.UOM = GetDefaultUOM(templateItemID);
			row.UOMDisabled = false;
			row.Qty = 0m;
			MatrixItems.Cache.Normalize();
			MatrixItems.View.RequestRefresh();
		}

		protected virtual CreateMatrixItemsHelper GetCreateMatrixItemsHelper(PXGraph graph)
		{
			return new CreateMatrixItemsHelper(graph);
		}

		protected virtual AttributeGroupHelper GetAttributeGroupHelper(PXGraph graph)
		{
			return new AttributeGroupHelper(graph);
		}

		protected override void RecalcMatrixGrid()
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				base.RecalcMatrixGrid();
				return;
			}

			MatrixItems.Cache.Clear();
		}

		[InventoryRaw(DisplayName = "Inventory ID", IsKey = true)]
		protected virtual void _(Events.CacheAttached<MatrixInventoryItem.inventoryCD> eventArgs)
		{
		}

		[PXInt]
		protected virtual void _(Events.CacheAttached<MatrixInventoryItem.inventoryID> eventArgs)
		{
		}

		protected virtual void _(Events.RowInserting<MatrixInventoryItem> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			eventArgs.Row.AttributeIDs = (string[])AdditionalAttributes.Current.AttributeIdentifiers.Clone();
			eventArgs.Row.AttributeValueDescrs = new string[eventArgs.Row.AttributeIDs.Length];
			eventArgs.Row.AttributeValues = new string[eventArgs.Row.AttributeIDs.Length];
		}

		protected virtual void _(Events.RowSelected<MainItemType> eventArgs)
		{
			showMatrixPanel.SetEnabled(IsDocumentOpen());
		}

		protected virtual void _(Events.RowSelected<MatrixInventoryItem> eventArgs)
		{
			bool allowEdit = IsDocumentOpen() && AdditionalAttributes.Current.AttributeIdentifiers?.Length > 0;
			MatrixItems.AllowInsert = allowEdit;
			MatrixItems.AllowDelete = allowEdit;
			MatrixItems.AllowUpdate = allowEdit;

			if (eventArgs.Row == null)
				return;

			eventArgs.Cache.Adjust<PXUIFieldAttribute>(eventArgs.Row).ForAllFields(a => a.Enabled = false);

			bool allowEditRow = AllAttributesArePopulated(eventArgs.Row);
			Exception inventoryException = null;

			if (allowEditRow && eventArgs.Row?.InventoryID != null)
			{
				var item = InventoryItem.PK.Find(Base, eventArgs.Row.InventoryID);
				if (IsItemStatusDisabled(item))
				{
					allowEditRow = false;
					string label = PXStringListAttribute.GetLocalizedLabel<InventoryItem.itemStatus>(Base.Caches<InventoryItem>(), item);
					inventoryException = new PXSetPropertyException(Messages.InventoryItemIsInStatus, PXErrorLevel.Warning, label);
				}
			}

			eventArgs.Cache.Adjust<PXUIFieldAttribute>(eventArgs.Row)
				.For<MatrixInventoryItem.qty>(a => a.Enabled = allowEditRow)
				.SameFor<MatrixInventoryItem.taxCategoryID>();

			eventArgs.Cache.Adjust<PXUIFieldAttribute>(eventArgs.Row)
				.For<MatrixInventoryItem.uOM>(a => a.Enabled = allowEditRow && eventArgs.Row.UOMDisabled != true);

			if (eventArgs.Row.Exists == true)
				inventoryException = new PXSetPropertyException(Messages.InventoryIDExists, PXErrorLevel.Error);
			eventArgs.Cache.RaiseExceptionHandling<MatrixInventoryItem.inventoryCD>(eventArgs.Row, eventArgs.Row.InventoryCD, inventoryException);
		}

		protected virtual bool AllAttributesArePopulated(MatrixInventoryItem row)
			=> row?.AttributeValues?.Any(v => string.IsNullOrEmpty(v)) == false;

		protected override void _(Events.FieldUpdated<EntryHeader, EntryHeader.templateItemID> eventArgs)
		{
			base._(eventArgs);
			if (eventArgs.Row != null)
				eventArgs.Row.Description = InventoryItem.PK.Find(Base, eventArgs.Row.TemplateItemID)?.Descr;
		}

		protected virtual void IncreaseQty(int? siteID, int inventoryID, string taxCategoryID, decimal addQty, string uom)
		{
			var line = GetLines(siteID, inventoryID, taxCategoryID, uom).FirstOrDefault();

			if (line != null)
			{
				line.Qty += addQty;
				UpdateLine(line);
			}
			else
				CreateNewLine(siteID, inventoryID, taxCategoryID, addQty, uom);
		}

		protected virtual void AddItemsToOrder(int? siteID)
		{
			var templateItem = InventoryItem.PK.Find(Base, Header.Current.TemplateItemID);
			var itemsToCreate = MatrixItems.Cache.Cached.RowCast<MatrixInventoryItem>().Where(mi => mi.New == true).ToList();

			if (templateItem != null)
			{
				var clone = Base.Clone();

				PXLongOperation.StartOperation(Base, delegate ()
				{
					var ext = clone.FindImplementation<SmartPanelExt<Graph, MainItemType>>();
					if (ext == null)
						throw new PXArgumentException(nameof(SmartPanelExt<Graph, MainItemType>));

					var inventoryItemGraph = (templateItem.StkItem == true) ?
						(InventoryItemMaintBase)PXGraph.CreateInstance<InventoryItemMaint>() :
						PXGraph.CreateInstance<NonStockItemMaint>();
					inventoryItemGraph.DefaultSiteFromItemClass = false;

					var helper = ext.GetCreateMatrixItemsHelper(inventoryItemGraph);
					var attributeGroupHelper = ext.GetAttributeGroupHelper(inventoryItemGraph);

					PXLongOperation.SetCustomInfo(clone);

					helper.CreateUpdateMatrixItems(inventoryItemGraph, templateItem, itemsToCreate, true,
						(t, item) => attributeGroupHelper.OnNewItem(templateItem, item, t.AttributeIDs, t.AttributeValues));

					foreach (MatrixInventoryItem item in ext.MatrixItems.Cache.Cached)
					{
						if (item.Qty == null)
							continue;

						if (item.InventoryID == null)
						{
							string attributeIDs = string.Join(";", item.AttributeIDs ?? throw new PXArgumentException(nameof(item.AttributeIDs)));
							string values = string.Join(";", item.AttributeIDs ?? throw new PXArgumentException(nameof(item.AttributeValues)));
							throw new RowNotFoundException(ext.MatrixItems.Cache, string.Join("; ", item.AttributeIDs), string.Join(", ", item.AttributeValues));
						}


						ext.IncreaseQty(siteID, (int)item.InventoryID, item.TaxCategoryID, (decimal)item.Qty, item.UOM);
					}
				});
			}
		}

		#endregion // Entry (Quick) View

		#region Lookup (Matrix) View

		protected virtual IEnumerable<InventoryMatrixResult> GetResult()
		{
			foreach (var row in Matrix.Cache.Cached)
			{
				var matrix = row as EntryMatrix;
				for (int columnIndex = 0; columnIndex < matrix.Quantities.Length; columnIndex++)
				{
					int? inventoryID = matrix.InventoryIDs[columnIndex];
					decimal? qty = matrix.Quantities[columnIndex];

					if (inventoryID != null)
						yield return new InventoryMatrixResult() { InventoryID = (int)inventoryID, Qty = qty ?? 0m };
				}
			}
		}
		
		protected virtual void AddMatrixItemsToOrder(int? siteID)
		{
			var clone = Base.Clone();

			PXLongOperation.StartOperation(Base, delegate ()
			{
				PXLongOperation.SetCustomInfo(clone);

				var ext = clone.FindImplementation<SmartPanelExt<Graph, MainItemType>>();
				if (ext == null)
					throw new PXArgumentException(nameof(SmartPanelExt<Graph, MainItemType>));

				foreach (var row in ext.GetResult())
				{
					var addQty = row.Qty - (ext.GetQty(siteID, row.InventoryID).Qty ?? 0m);

					if (addQty > 0)
						ext.IncreaseQty(siteID, row.InventoryID, addQty);
					else if (addQty < 0)
						ext.DecreaseQty(siteID, row.InventoryID, addQty);
				}
			});
		}

		protected virtual void IncreaseQty(int? siteID, int inventoryID, decimal addQty)
		{
			var line = GetLines(siteID, inventoryID).FirstOrDefault();

			if (line != null)
			{
				line.Qty += addQty;
				UpdateLine(line);
			}
			else
				CreateNewLine(siteID, inventoryID, addQty);
		}

		protected virtual void DecreaseQty(int? siteID, int inventoryID, decimal addQty)
		{
			decimal accumQty = addQty * -1;

			foreach (var line in GetLines(siteID, inventoryID))
			{
				if (line.Qty >= accumQty)
				{
					line.Qty -= accumQty;
					UpdateLine(line);
					accumQty = 0;
					break;
				}

				accumQty -= line.Qty ?? 0m;
				line.Qty = 0;
				UpdateLine(line);
			}
		}

		protected override void FillInventoryMatrixItem(EntryMatrix newRow, int colAttributeIndex, InventoryMapValue inventoryValue)
		{
			if (inventoryValue?.InventoryID == null) return;
			var item = InventoryItem.PK.Find(Base, inventoryValue.InventoryID);

			if (!IsItemStatusDisabled(item))
			{
				newRow.InventoryIDs[colAttributeIndex] = inventoryValue.InventoryID;

				var qtyInfo = GetQty(Header.Current.SiteID, inventoryValue.InventoryID);
				if (qtyInfo.Qty != null)
				{ 
					newRow.Quantities[colAttributeIndex] = qtyInfo.Qty;
					newRow.UOMs[colAttributeIndex] = qtyInfo.UOM;

					if (qtyInfo.UOM == item.BaseUnit)
					{
						newRow.BaseQuantities[colAttributeIndex] = qtyInfo.Qty;
					}
					else
					{
						newRow.BaseQuantities[colAttributeIndex] =
							INUnitAttribute.ConvertToBase(Matrix.Cache, inventoryValue.InventoryID, qtyInfo.UOM, qtyInfo.Qty ?? 0m, INPrecision.QUANTITY);
					}
				}
				else
				{
					newRow.InventoryIDs[colAttributeIndex] = null;
					newRow.Errors[colAttributeIndex] = PXLocalizer.Localize(Messages.LinesWithSameInventoryHaveDifferentUOM);
				}
			}
			else
			{
				string label = PXStringListAttribute.GetLocalizedLabel<InventoryItem.itemStatus>(Base.Caches<InventoryItem>(), item);
				newRow.Errors[colAttributeIndex] = PXLocalizer.LocalizeFormat(Messages.InventoryItemIsInStatus, label);
			}
		}

		protected virtual (string UOM, decimal? Qty, int Count) GetQty(int? siteID, int? inventoryID)
		{
			var transactions = GetLines(siteID, inventoryID).ToArray();

			string firstUOM = transactions.FirstOrDefault()?.UOM ?? GetDefaultUOM(inventoryID);

			if (transactions.Any(l => l.UOM != firstUOM))
			{
				return (firstUOM, null, transactions.Length);
			}

			return (firstUOM, transactions.Sum(l => l.Qty) ?? 0m, transactions.Length);
		}

		protected override void FieldSelectingImpl(int attributeNumber, PXCache s, PXFieldSelectingEventArgs e, string fieldName)
		{
			var matrix = e.Row as EntryMatrix;
			int? inventoryId = GetValueFromArray(matrix?.InventoryIDs, attributeNumber);
			decimal? qty = GetValueFromArray(matrix?.Quantities, attributeNumber);
			string error = GetValueFromArray(matrix?.Errors, attributeNumber);

			var state = PXDecimalState.CreateInstance(e.ReturnState, _precision.Value, fieldName, false, 0, 0m, null);
			state.Enabled = inventoryId != null && IsDocumentOpen();
			state.Error = error;
			state.ErrorLevel = string.IsNullOrEmpty(error) ? PXErrorLevel.Undefined : PXErrorLevel.Warning;
			e.ReturnState = state;
			e.ReturnValue = (inventoryId != null || matrix?.IsTotal == true) ? qty : null;

			var firstMatrix = s.Cached.FirstOrDefault_() as EntryMatrix;
			if (attributeNumber < firstMatrix?.ColAttributeValueDescrs?.Length)
			{
				state.DisplayName = firstMatrix.ColAttributeValueDescrs[attributeNumber] ?? firstMatrix.ColAttributeValues[attributeNumber];
				state.Visibility = PXUIVisibility.Visible;
				state.Visible = true;
			}
			else
			{
				state.DisplayName = null;
				state.Visibility = PXUIVisibility.Invisible;
				state.Visible = false;
			}
		}

		protected override void FieldUpdatingImpl(int attributeNumber, PXCache s, PXFieldUpdatingEventArgs e, string fieldName)
		{
			var row = e.Row as EntryMatrix;
			if (row == null)
				return;

			if (attributeNumber < row.Quantities?.Length)
			{
				decimal qty = (e.NewValue == null) ?  0m : Convert.ToDecimal(e.NewValue);
				var currentUOM = row.UOMs[attributeNumber];
				var inventoryItem = InventoryItem.PK.Find(Base, row.InventoryIDs[attributeNumber]);

				row.Quantities[attributeNumber] = qty;
				row.BaseQuantities[attributeNumber] = (currentUOM == inventoryItem?.BaseUnit) ? qty :
					INUnitAttribute.ConvertToBase(Matrix.Cache, inventoryItem?.InventoryID, currentUOM, qty, INPrecision.QUANTITY);

				Matrix.View.RequestRefresh();
			}
		}

		protected override void TotalFieldSelecting(PXCache s, PXFieldSelectingEventArgs e, string fieldName)
		{
			var matrix = e.Row as EntryMatrix;

			var state = PXDecimalState.CreateInstance(e.ReturnState, _precision.Value, fieldName, false, 0, 0m, null);
			e.ReturnState = state;
			state.Enabled = false;

			state.DisplayName = PXLocalizer.Localize(Messages.TotalQty);

			var firstMatrix = s.Cached.FirstOrDefault_() as EntryMatrix;
			if (firstMatrix?.ColAttributeValueDescrs?.Length > 0)
			{
				state.Visibility = PXUIVisibility.Visible;
				state.Visible = true;
			}
			else
			{
				state.Visibility = PXUIVisibility.Invisible;
				state.Visible = false;
			}

			decimal sum = 0;
			for (int columnIndex = 0; columnIndex < matrix?.Quantities?.Length; columnIndex++)
				sum += (matrix.IsTotal == true || matrix.InventoryIDs[columnIndex] != null) ? (matrix.BaseQuantities[columnIndex] ?? 0m) : 0m;

			e.ReturnValue = sum;
		}

		protected override EntryMatrix GenerateTotalRow(IEnumerable<EntryMatrix> rows)
		{
			bool rowsExist = false;
			var totalRow = (EntryMatrix)Matrix.Cache.CreateInstance();

			foreach (EntryMatrix row in Matrix.Cache.Cached)
			{
				rowsExist = true;

				if (totalRow.Quantities == null)
					totalRow.Quantities = new decimal?[row.Quantities.Length];

				if (totalRow.BaseQuantities == null)
					totalRow.BaseQuantities = new decimal?[row.Quantities.Length];

				if (totalRow.UOMs == null)
					totalRow.UOMs = new string[row.Quantities.Length];

				if (totalRow.BaseUOM == null)
					totalRow.BaseUOM = row.BaseUOM;

				for (int columnIndex = 0; columnIndex < row.Quantities.Length; columnIndex++)
				{
					totalRow.Quantities[columnIndex] = totalRow.Quantities[columnIndex] ?? 0m;
					totalRow.Quantities[columnIndex] += row.InventoryIDs[columnIndex] != null ? row.BaseQuantities[columnIndex] : 0m;
					totalRow.BaseQuantities[columnIndex] = totalRow.Quantities[columnIndex];
					totalRow.UOMs[columnIndex] = totalRow.BaseUOM;
				}
			}

			totalRow.RowAttributeValueDescr = PXLocalizer.Localize(Messages.TotalQty);
			totalRow.IsTotal = true;
			totalRow.LineNbr = int.MaxValue;

			return rowsExist ? totalRow : null;
		}

		#region Availability

		protected virtual string GetAvailability(int? siteID, int? inventoryID, decimal? qty, string uom)
		{
			if (inventoryID == null)
				return null;

			InventoryItem item = InventoryItem.PK.Find(Base, inventoryID);
			if (item == null)
				throw new Common.Exceptions.RowNotFoundException(Base.Caches<InventoryItem>(), inventoryID);

			if (item.StkItem != true)
				return null;

			int? inventorySiteID = siteID;

			if (inventorySiteID == null)
			{
				GL.Branch branch = GL.Branch.PK.Find(Base, GetDefaultBranch());
				inventorySiteID = InventoryItemCurySettings.PK.Find(Base, inventoryID, branch?.BaseCuryID)?.DfltSiteID;
			}

			SiteStatusByCostCenter allocated = new SiteStatusByCostCenter
			{
				InventoryID = inventoryID,
				SubItemID = item.DefaultSubItemID,
				SiteID = inventorySiteID,
				CostCenterID = CostCenter.FreeStock
			};

			allocated = InsertWith(Base, allocated,
				(cache, e) =>
				{
					cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
					cache.IsDirty = false;
				});
			allocated = PXCache<SiteStatusByCostCenter>.CreateCopy(allocated);

			INSiteStatusByCostCenter status = INSiteStatusByCostCenter.PK.Find(Base, inventoryID, item.DefaultSubItemID, inventorySiteID, CostCenter.FreeStock);

			if (status != null)
			{
				allocated.QtyOnHand += status.QtyOnHand;
				allocated.QtyAvail += status.QtyAvail;
				allocated.QtyHardAvail += status.QtyHardAvail;
				allocated.QtyActual += status.QtyActual;
				allocated.QtyPOOrders += status.QtyPOOrders;
			}

			foreach (var line in GetLines(inventorySiteID, inventoryID))
				DeductAllocated(allocated, line);

			if (uom != item.BaseUnit)
			{
				decimal unitRate = INUnitAttribute.ConvertFromBase(Matrix.Cache, inventoryID, uom, 1m, INPrecision.NOROUND);
				allocated.QtyOnHand = PXDBQuantityAttribute.Round((allocated.QtyOnHand ?? 0m) * unitRate);
				allocated.QtyAvail = PXDBQuantityAttribute.Round((allocated.QtyAvail ?? 0m) * unitRate);
				allocated.QtyHardAvail = PXDBQuantityAttribute.Round((allocated.QtyHardAvail ?? 0m) * unitRate);
				allocated.QtyHardAvail = PXDBQuantityAttribute.Round((allocated.QtyHardAvail ?? 0m) * unitRate);
				allocated.QtyPOOrders = PXDBQuantityAttribute.Round((allocated.QtyPOOrders ?? 0m) * unitRate);
			}

			return GetAvailabilityMessage(inventorySiteID, item, allocated, uom);
		}

		protected abstract string GetAvailabilityMessage(int? siteID, InventoryItem item, SiteStatusByCostCenter allocated, string uom);

		protected T InsertWith<T>(PXGraph graph, T row, PXRowInserted handler)
			where T : class, IBqlTable, new()
		{
			graph.RowInserted.AddHandler<T>(handler);
			try
			{
				return PXCache<T>.Insert(graph, row);
			}
			finally
			{
				graph.RowInserted.RemoveHandler<T>(handler);
			}
		}

		protected virtual string FormatQty(decimal? value)
		{
			return (value == null) ? string.Empty : ((decimal)value).ToString("N" + CommonSetupDecPl.Qty.ToString(),
				System.Globalization.NumberFormatInfo.CurrentInfo);
		}

		protected abstract void DeductAllocated(SiteStatusByCostCenter allocated, IMatrixItemLine line);

		protected virtual void _(Events.FieldSelecting<EntryMatrix, EntryMatrix.matrixAvailability> eventArgs)
		{
			eventArgs.ReturnValue = null;

			EntryMatrix row = eventArgs.Row;

			if (row != null && row.SelectedColumn != null && Header.Current?.ShowAvailable == true)
			{
				int columnIndex = (int)row.SelectedColumn;

				eventArgs.ReturnValue = GetAvailability(Header.Current.SiteID,
					GetValueFromArray(row.InventoryIDs, columnIndex),
					GetValueFromArray(row.Quantities, columnIndex),
					GetValueFromArray(row.UOMs, columnIndex));
			}
		}

		protected override void OnMatrixGridCellCahnged()
		{
			// Trigger update of the Availability, which is likely to be different per cell, since each sell may be a different item
			object temp = null;
			Matrix.Cache.RaiseFieldSelecting<EntryMatrix.matrixAvailability>(Matrix.Current, ref temp, true);
		}
		#endregion // Availability

		#endregion // Lookup (Matrix) View
	}
}
