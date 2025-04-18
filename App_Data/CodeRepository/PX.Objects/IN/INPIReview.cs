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
using PX.Data.BQL.Fluent;

using PX.Objects.CM;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.IN.PhysicalInventory;
using PX.Data.WorkflowAPI;
using PX.Objects.Common.Extensions;

namespace PX.Objects.IN
{
	public class INPIController : PXGraph<INPIController>
	{
		public PXSave<INPIHeader> Save;
		public PXCancel<INPIHeader> Cancel;

		public
			SelectFrom<INPIHeader>.
			InnerJoin<INSite>.On<INPIHeader.FK.Site>.
			Where<MatchUserFor<INSite>>.
			View PIHeader;

		public SelectFrom<INPIDetailUpdate>.View PIDetailUpdate;
		public SelectFrom<INPIStatusItem>.View PIStatusItem;
		public SelectFrom<INPIStatusLoc>.View PIStatusLoc;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		protected virtual void _(Events.CacheAttached<INPIStatusItem.pIID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		protected virtual void _(Events.CacheAttached<INPIStatusLoc.pIID> e) { }

		public virtual void ReopenPI(string piId)
		{
			INPIHeader header = PIHeader.Current = PIHeader.Search<INPIHeader.pIID>(piId);
			if (header?.Status != INPIHdrStatus.InReview)
				return;

			header.Status = INPIHdrStatus.Entering;
			header.PIAdjRefNbr = null;
			PIHeader.Update(header);

			Save.Press();
		}

		public virtual INPIDetailUpdate AccumulateFinalCost(string piId, int piLineNbr, decimal costAmt)
		{
			var row = new INPIDetailUpdate
			{
				PIID = piId,
				LineNbr = piLineNbr,
			};
			row = PIDetailUpdate.Insert(row);

			row.FinalExtVarCost += costAmt;

			return PIDetailUpdate.Update(row);
		}

		public virtual void ReleasePI(string piId)
		{
			var header = PIHeader.Current = PIHeader.Search<INPIHeader.pIID>(piId);
			CreatePILocksManager().UnlockInventory();
			header.TotalVarCost = PIDetailUpdate.Cache.Inserted.RowCast<INPIDetailUpdate>().Sum(d => d.FinalExtVarCost) ?? 0m;
			header.Status = INPIHdrStatus.Completed;
			PIHeader.Update(header);

			Save.Press();
		}

		protected virtual PILocksManager CreatePILocksManager()
		{
			INPIHeader header = PIHeader.Current;
			return new PILocksManager(this, PIStatusItem, PIStatusLoc, (int)header.SiteID, header.PIID);
		}
	}

	public class INPIEntry : INPIController, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		#region Navigation Actions
		public PXAction<INPIHeader> Insert; // see insert(PXAdapter) delegate below
		public PXCopyPasteAction<INPIHeader> CopyPaste;
		public PXDelete<INPIHeader> Delete;
		public PXFirst<INPIHeader> First;
		public PXPrevious<INPIHeader> Previous;
		public PXNext<INPIHeader> Next;
		public PXLast<INPIHeader> Last;
		#endregion

		#region Views
		[PXFilterable]
		[PXImport(typeof(INPIHeader))]
		public
			SelectFrom<INPIDetail>.
			LeftJoin<InventoryItem>.On<INPIDetail.FK.InventoryItem>.
			LeftJoin<INSubItem>.On<INPIDetail.FK.SubItem>.
			Where<
				INPIDetail.pIID.IsEqual<INPIHeader.pIID.AsOptional>.
				And<
					INPIDetail.inventoryID.IsNull.
					Or<InventoryItem.inventoryID.IsNotNull>>>.
			OrderBy<INPIDetail.lineNbr.Asc>.
			View PIDetail;

		public
			SelectFrom<INPIDetail>.
			Where<INPIDetail.FK.PIHeader.SameAsCurrent>.
			View PIDetailPure;

		public
			SelectFrom<INPIHeader>.
			Where<INPIHeader.pIID.IsEqual<INPIHeader.pIID.FromCurrent>>.
			View PIHeaderInfo;

		public SelectFrom<INSetup>.View INSetup;

		public
			PXSetup<INSite>.
			Where<INSite.siteID.IsEqual<INPIHeader.siteID.FromCurrent>>
			insite;

		public PXFilter<PIGeneratorSettings> GeneratorSettings;
		public INBarCodeItemLookup<INBarCodeItem> AddByBarCode;
		public PXSetup<INSetup> Setup;
		#endregion

		#region DAC overrides
		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXSelector(typeof(
			Search2<INPIHeader.pIID,
			InnerJoin<INSite, On<INPIHeader.FK.Site>>,
			Where<MatchUserFor<INSite>>,
			OrderBy<Desc<INPIHeader.pIID>>>), Filterable = true)]
		protected void _(Events.CacheAttached<INPIHeader.pIID> e) { }

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Type ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INPIClass.pIClassID, Where<INPIClass.method, Equal<PIMethod.fullPhysicalInventory>>>))]
		[PXDefault]
		protected virtual void _(Events.CacheAttached<PIGeneratorSettings.pIClassID> e) { }
		#endregion

		#region Initialization
		public INPIEntry()
		{
			PXDefaultAttribute.SetPersistingCheck<PIGeneratorSettings.randomItemsLimit>(GeneratorSettings.Cache, null, PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<PIGeneratorSettings.lastCountPeriod>(GeneratorSettings.Cache, null, PXPersistingCheck.Nothing);
		}
		#endregion

		[PXInsertButton]
		[PXUIField(DisplayName = ActionsMessages.Insert, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		protected virtual IEnumerable insert(PXAdapter adapter)
		{
			if (GeneratorSettings.AskExt((graph, name) => ResetGenerateSettings(graph)) == WebDialogResult.OK && GeneratorSettings.VerifyRequired())
			{
				PIGenerator generator = PXGraph.CreateInstance<PIGenerator>();
				generator.GeneratorSettings.Current = GeneratorSettings.Current;
				generator.CalcPIRows(true);
				if (generator.piheader.Current != null)
				{
					Clear();
					return PIHeader.Search<INPIHeader.pIID>(generator.piheader.Current.PIID);
				}
			}
			return adapter.Get();

			void ResetGenerateSettings(PXGraph graph)
			{
				var entry = graph as INPIEntry;
				if (entry == null) return;
				entry.GeneratorSettings.Cache.Clear();
				entry.GeneratorSettings.Insert(new PIGeneratorSettings());
			}
		}

		#region AddLineByBarCode

		public PXAction<INPIHeader> addLine;
		[PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton(Tooltip = Messages.AddNewLine)]
		public virtual IEnumerable AddLine(PXAdapter adapter)
		{
			if (AddByBarCode.AskExt(
				(graph, view) => ((INPIEntry)graph).AddByBarCode.Reset(false)) == WebDialogResult.OK &&
				AddByBarCode.VerifyRequired())
				UpdatePhysicalQty();
			return adapter.Get();
		}

		public PXAction<INPIHeader> addLine2;
		[PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton(Tooltip = Messages.AddNewLine)]
		public virtual IEnumerable AddLine2(PXAdapter adapter)
		{
			if (AddByBarCode.VerifyRequired())
				UpdatePhysicalQty();

			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<INBarCodeItem> e)
		{
			PXUIFieldAttribute.SetEnabled<INBarCodeItem.uOM>(AddByBarCode.Cache, null, false);
		}

		protected virtual void _(Events.FieldDefaulting<INBarCodeItem, INBarCodeItem.expireDate> e)
		{
			INPIDetail exists =
				SelectFrom<INPIDetail>.
				Where<
					INPIDetail.pIID.IsEqual<INPIHeader.pIID.FromCurrent>.
					And<INPIDetail.inventoryID.IsEqual<INBarCodeItem.inventoryID.FromCurrent>>.
					And<INPIDetail.lotSerialNbr.IsEqual<INBarCodeItem.lotSerialNbr.FromCurrent>>>.
				View.ReadOnly.SelectWindowed(this, 0, 1);
			if (exists != null)
			{
				e.NewValue = exists.ExpireDate;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldDefaulting<INBarCodeItem, INBarCodeItem.siteID> e)
		{
			if (PIHeader.Current != null)
				e.NewValue = PIHeader.Current.SiteID;
			e.Cancel = true;
		}

		protected virtual void _(Events.RowUpdated<INBarCodeItem> e)
		{
			if (e.Row != null && e.Row.AutoAddLine == true && AddByBarCode.VerifyRequired(true) && e.Row.Qty > 0)
				UpdatePhysicalQty();
		}

		private void UpdatePhysicalQty()
		{
			INBarCodeItem item = AddByBarCode.Current;

			INPIDetail detail =
				SelectFrom<INPIDetail>.
				Where<
					INPIDetail.pIID.IsEqual<INPIHeader.pIID.FromCurrent>.
					And<INPIDetail.inventoryID.IsEqual<INBarCodeItem.inventoryID.FromCurrent>>.
					And<INPIDetail.subItemID.IsEqual<INBarCodeItem.subItemID.FromCurrent>>.
					And<INPIDetail.locationID.IsEqual<INBarCodeItem.locationID.FromCurrent>>.
					And<
						INPIDetail.lotSerialNbr.IsNull.
						Or<INPIDetail.lotSerialNbr.IsEqual<INBarCodeItem.lotSerialNbr.FromCurrent>>>>.
				View.SelectWindowed(this, 0, 1);
			if (detail == null)
			{
				detail = new INPIDetail { InventoryID = item.InventoryID };
				detail = PXCache<INPIDetail>.CreateCopy(PIDetail.Insert(detail));
				detail.SubItemID = item.SubItemID;
				detail.LocationID = item.LocationID;
				detail.LotSerialNbr = item.LotSerialNbr;
				detail = PXCache<INPIDetail>.CreateCopy(PIDetail.Update(detail));
				detail.PhysicalQty = item.BaseQty;
				detail.ExpireDate = item.ExpireDate;
			}
			else
			{
				detail = PXCache<INPIDetail>.CreateCopy(detail);
				detail.PhysicalQty = detail.PhysicalQty.GetValueOrDefault() + item.BaseQty.GetValueOrDefault();
			}

			if (!string.IsNullOrEmpty(item.ReasonCode))
				detail.ReasonCode = item.ReasonCode;

			detail = PIDetail.Update(detail);

			item.Description = PXMessages.LocalizeFormatNoPrefixNLA(
				Messages.PILineUpdated,
				AddByBarCode.GetValueExt<INBarCodeItem.inventoryID>(item).ToString().Trim(),
				Setup.Current.UseInventorySubItem == true ? ":" + AddByBarCode.GetValueExt<INBarCodeItem.subItemID>(item) : string.Empty,
				AddByBarCode.GetValueExt<INBarCodeItem.qty>(item),
				item.UOM,
				detail.LineNbr);

			AddByBarCode.Reset(true);
			AddByBarCode.View.RequestRefresh();
		}

		#endregion

		#region Event Handlers
		#region INPIHeader
		protected virtual void _(Events.RowSelected<INPIHeader> e)
		{
			if (e.Row == null || IsContractBasedAPI)
				return;

			PIHeader.Cache.AllowDelete = e.Row.Status.IsNotIn(INPIHdrStatus.InReview, INPIHdrStatus.Completed);

			PIHeader.Cache.AllowUpdate =
			PIDetail.Cache.AllowInsert =
			PIDetail.Cache.AllowDelete =
			PIDetail.Cache.AllowUpdate = e.Row.Status.IsIn(INPIHdrStatus.Counting, INPIHdrStatus.Entering);

			addLine.SetEnabled(PIDetail.Cache.AllowUpdate);
		}

		protected virtual void _(Events.RowDeleting<INPIHeader> e)
		{
			if (e.Row != null)
				CreatePILocksManager().UnlockInventory();
		}
		#endregion

		#region INPIDetail
		protected virtual void _(Events.RowSelected<INPIDetail> e)
		{
			if (e.Row == null)
				return;

			INLotSerClass lotSer = SelectLotSerClass(e.Row.InventoryID);
			bool notNormal = e.Row.LineType != INPIDetLineType.Normal;
			bool notNormalSerial = notNormal & LSRequired(lotSer);
			bool requestExpireDate = notNormalSerial && lotSer.LotSerTrackExpiration == true;
			bool isDebitLine = e.Row?.VarQty > 0;
			bool isEntering = PIHeader.Current.Status == INPIHdrStatus.Entering;

			PXUIFieldAttribute.SetEnabled<INPIDetail.inventoryID>(e.Cache, e.Row, notNormal);
			PXUIFieldAttribute.SetEnabled<INPIDetail.subItemID>(e.Cache, e.Row, notNormal);
			PXUIFieldAttribute.SetEnabled<INPIDetail.locationID>(e.Cache, e.Row, notNormal);
			PXUIFieldAttribute.SetEnabled<INPIDetail.lotSerialNbr>(e.Cache, e.Row, notNormalSerial);
			PXUIFieldAttribute.SetEnabled<INPIDetail.expireDate>(e.Cache, e.Row, requestExpireDate);
			PXUIFieldAttribute.SetEnabled<INPIDetail.physicalQty>(e.Cache, e.Row, AreKeysFieldsEntered(e.Row));
			PXUIFieldAttribute.SetEnabled<INPIDetail.unitCost>(e.Cache, e.Row, isEntering && isDebitLine);
			PXUIFieldAttribute.SetEnabled<INPIDetail.manualCost>(e.Cache, e.Row, isEntering && isDebitLine);
			PXUIFieldAttribute.SetVisible<INPIDetail.tagNumber>(e.Cache, null, INSetup.Current.PIUseTags == true);
		}

		public virtual decimal GetBookQty(INPIDetail detail)
		{
			if (!LSRequired(detail.InventoryID))
			{
				var status = INLocationStatus.PK.Find(this, detail.InventoryID, detail.SubItemID, detail.SiteID, detail.LocationID);

				return status?.QtyActual ?? 0m;
			}
			else
			{
				var status = INLotSerialStatus.PK.Find(this, detail.InventoryID, detail.SubItemID, detail.SiteID, detail.LocationID, detail.LotSerialNbr);

				return status?.QtyActual ?? 0m;
			}
		}

		protected virtual void _(Events.RowInserting<INPIDetail> e)
		{
			if (e.Row != null)
				e.Row.BookQty = GetBookQty(e.Row);
		}

		protected virtual void _(Events.RowUpdating<INPIDetail> e)
		{
			if (e.NewRow != null && !e.Cache.ObjectsEqual<INPIDetail.inventoryID, INPIDetail.siteID, INPIDetail.subItemID, INPIDetail.locationID, INPIDetail.lotSerialNbr>(e.Row, e.NewRow))
				e.NewRow.BookQty = GetBookQty(e.NewRow);
		}

		protected virtual void _(Events.RowDeleting<INPIDetail> e)
		{
			if (e.Row.LineType != INPIDetLineType.UserEntered && e.ExternalCall)
				throw new PXException(Messages.PILineDeleted);
		}

		protected virtual void _(Events.FieldVerifying<INPIDetail, INPIDetail.inventoryID> e)
		{
			if (e.NewValue == null || e.Row == null)
				return;

			try
			{
				ValidateDuplicate(e.Row.Status, (int?)e.NewValue, e.Row.SubItemID, e.Row.LocationID, e.Row.LotSerialNbr, e.Row.LineNbr);
				// Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [Justification]
				ValidatePIInventoryLocation((int?)e.NewValue, e.Row.LocationID);
			}
			catch (PXSetPropertyException ex)
			{
				var invalidItem = InventoryItem.PK.Find(this, (int)e.NewValue);
				e.NewValue = invalidItem?.InventoryCD;
				throw ex;
			}
		}

		protected virtual void _(Events.FieldVerifying<INPIDetail, INPIDetail.subItemID> e)
		{
			if (e.NewValue as int? == null || e.Row == null || !PXAccess.FeatureInstalled<FeaturesSet.subItem>())
				return;

			try
			{
				ValidateDuplicate(e.Row.Status, e.Row.InventoryID, (int?)e.NewValue, e.Row.LocationID, e.Row.LotSerialNbr, e.Row.LineNbr);
			}
			catch (PXSetPropertyException ex)
			{
				var subItem = INSubItem.PK.Find(this, (int)e.NewValue);
				e.NewValue = subItem?.SubItemCD;
				throw ex;
			}
		}

		protected virtual void _(Events.FieldVerifying<INPIDetail, INPIDetail.locationID> e)
		{
			if (e.NewValue as int? == null || e.Row == null)
				return;

			try
			{
				ValidateDuplicate(e.Row.Status, e.Row.InventoryID, e.Row.SubItemID, (int?)e.NewValue, e.Row.LotSerialNbr, e.Row.LineNbr);
				// Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [Justification]
				ValidatePIInventoryLocation(e.Row.InventoryID, (int?)e.NewValue);
			}
			catch (PXSetPropertyException ex)
			{
				var invalidLocation = INLocation.PK.Find(this, (int)e.NewValue);
				e.NewValue = invalidLocation?.LocationCD;
				throw ex;
			}
		}

		protected virtual void _(Events.FieldVerifying<INPIDetail, INPIDetail.lotSerialNbr> e)
		{
			if (e.Row != null && e.NewValue is string strVal)
			{
				if (!string.IsNullOrEmpty(strVal) && Char.IsWhiteSpace(strVal[0]))
					throw new PXSetPropertyException(Messages.LotSerialLeadingSpaceIsNotAllowed);
				ValidateDuplicate(e.Row.Status, e.Row.InventoryID, e.Row.SubItemID, e.Row.LocationID, strVal, e.Row.LineNbr);
		}
		}

		protected virtual void _(Events.FieldVerifying<INPIDetail, INPIDetail.physicalQty> e)
		{
			if (e.NewValue == null) return;

			decimal value = (decimal)e.NewValue;
			INLotSerClass inclass = SelectLotSerClass(e.Row.InventoryID);
			if (value < 0m)
				throw new PXSetPropertyException(CS.Messages.Entry_GE, PXErrorLevel.Error, (int)0);

			if (inclass != null && LSRequired(inclass) && inclass.LotSerTrack == INLotSerTrack.SerialNumbered && value != 0m && value != 1m)
				throw new PXSetPropertyException(Messages.PIPhysicalQty);
		}

		protected virtual void _(Events.RowPersisting<INPIDetail> e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update)
				&& e.Row.LineType != INPIDetLineType.Normal && e.Row.Status == INPIDetStatus.Entered)
			{
				CheckDefault<INPIDetail.inventoryID>(e.Cache, e.Row);
				CheckDefault<INPIDetail.subItemID>(e.Cache, e.Row);
				CheckDefault<INPIDetail.locationID>(e.Cache, e.Row);
				INLotSerClass lotSer = SelectLotSerClass(e.Row.InventoryID);
				if (LSRequired(lotSer))
				{
					CheckDefault<INPIDetail.lotSerialNbr>(e.Cache, e.Row);
					if (lotSer.LotSerTrackExpiration == true)
						CheckDefault<INPIDetail.expireDate>(e.Cache, e.Row);
				}
			}

			if (e.Row != null && e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted)
			{
				INSetup setup = INSetup.Select();

				if (setup.PIUseTags == true)
				{
					setup.PILastTagNumber++;
					e.Row.TagNumber = setup.PILastTagNumber;
					INSetup.Update(setup);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<INPIDetail, INPIDetail.inventoryID> e)
		{
			if (e.Row == null || PIHeader.Cache.Current == null)
				return;

			if (e.Row.InventoryID != null)
				e.Cache.SetDefaultExt<INPIDetail.subItemID>(e.Row);
			else
				e.Cache.SetValue<INPIDetail.subItemID>(e.Row, null);

			if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
				e.Cache.SetValue<INPIDetail.locationID>(e.Row, null);
			e.Cache.SetValue<INPIDetail.lotSerialNbr>(e.Row, null);
			e.Cache.SetValueExt<INPIDetail.physicalQty>(e.Row, null);
		}

		protected virtual void _(Events.FieldUpdated<INPIDetail, INPIDetail.subItemID> e)
		{
			if (PIHeader.Cache.Current != null && e.Row != null)
				e.Cache.SetValueExt<INPIDetail.physicalQty>(e.Row, null);
		}

		protected virtual void _(Events.FieldUpdated<INPIDetail, INPIDetail.locationID> e)
		{
			if (PIHeader.Cache.Current != null && e.Row != null && e.Row.LocationID == null)
				e.Cache.SetValueExt<INPIDetail.physicalQty>(e.Row, null);
		}

		protected virtual void _(Events.FieldUpdated<INPIDetail, INPIDetail.lotSerialNbr> e)
		{
			if (PIHeader.Cache.Current != null && e.Row != null && string.IsNullOrWhiteSpace(e.Row.LotSerialNbr))
				e.Cache.SetValueExt<INPIDetail.physicalQty>(e.Row, null);
		}

		protected virtual void _(Events.FieldUpdated<INPIDetail, INPIDetail.physicalQty> e)
		{
			if (e.Row == null)
				return;

			if (!AreKeysFieldsEntered(e.Row) || e.Row.PhysicalQty == null)
			{
				e.Cache.SetValue<INPIDetail.manualCost>(e.Row, false);
				e.Cache.SetValue<INPIDetail.varQty>(e.Row, null);
				e.Cache.SetValueExt<INPIDetail.unitCost>(e.Row, null);
				e.Cache.SetValueExt<INPIDetail.extVarCost>(e.Row, null);
				e.Cache.SetValue<INPIDetail.status>(e.Row, INPIDetStatus.NotEntered);

				return;
			}

			decimal varQty = (decimal)(e.Row.PhysicalQty - e.Row.BookQty);
			e.Cache.SetValue<INPIDetail.varQty>(e.Row, varQty);
			e.Cache.SetValue<INPIDetail.status>(e.Row, INPIDetStatus.Entered);

			if (PIHeader.Current?.Status != INPIHdrStatus.Entering)
				return;

			if (varQty <= 0m && e.Row.ManualCost == true)
				e.Cache.SetValueExt<INPIDetail.manualCost>(e.Row, false);

			decimal? oldVarQty = e.OldValue != null ? (decimal?)e.OldValue - e.Row.BookQty : null;
			if (varQty >= 0)
			{
				if (e.OldValue == null || oldVarQty < 0)
					DefaultDebitLineCost(e.Cache, e.Row);

				e.Cache.SetValueExt<INPIDetail.extVarCost>(e.Row, e.Row.UnitCost * varQty);
			}
		}

		protected virtual void _(Events.FieldUpdated<INPIDetail, INPIDetail.manualCost> e)
		{
			if (e.Row?.ManualCost == false && (bool)e.OldValue == true)
				DefaultDebitLineCost(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldUpdating<INPIDetail, INPIDetail.unitCost> e)
		{
			if (e.Row != null && e.Row.VarQty > 0 && e.NewValue == null)
				e.NewValue = 0m;
		}

		protected virtual void _(Events.FieldUpdated<INPIDetail, INPIDetail.unitCost> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.VarQty > 0 && e.ExternalCall && e.Row.IsCostDefaulting != true)
			{
				e.Cache.SetValue<INPIDetail.manualCost>(e.Row, true);
				e.Cache.SetValueExt<INPIDetail.extVarCost>(
					e.Row,
					e.Row.UnitCost != null && e.Row.VarQty != null
						? e.Row.UnitCost.Value * e.Row.VarQty.Value
						: (decimal?)null);
			}
		}

		protected virtual void _(Events.RowUpdated<INPIDetail> e)
		{
			int updatedLinesCount = 0;
			if (IsCostCalculationEnabled())
			{
				if (e.OldRow.PhysicalQty == null && AreKeysFieldsEntered(e.Row))
				{
					var relatedLines = GetRelatedLines(e.Row);
					UpdateRelatedLinesCost(e.Cache, relatedLines, ref updatedLinesCount);

					if (updatedLinesCount > 0 || AreTotalsAffected(e.OldRow, e.Row))
					RecalcTotals();
					if (updatedLinesCount > 0) // Request refresh only if other rows are affected.
					PIDetail.View.RequestRefresh();

					return;
				}

				if (e.Row.PhysicalQty == null && AreKeysFieldsEntered(e.OldRow))
				{
					var prevRelatedLines = GetRelatedLines(e.OldRow);
					UpdateRelatedLinesCost(e.Cache, prevRelatedLines, ref updatedLinesCount);

					if (updatedLinesCount > 0 || AreTotalsAffected(e.OldRow, e.Row))
					RecalcTotals();
					if (updatedLinesCount > 0) // Request refresh only if other rows are affected.
					PIDetail.View.RequestRefresh();

					return;
				}

				// Next, VarQty can't be null.
				if (!e.Cache.ObjectsEqual<INPIDetail.inventoryID, INPIDetail.subItemID, INPIDetail.locationID, INPIDetail.lotSerialNbr>(e.Row, e.OldRow)
					&& AreKeysFieldsEntered(e.OldRow))
				{
					var prevRelatedLines = GetRelatedLines(e.OldRow);
					UpdateRelatedLinesCost(e.Cache, prevRelatedLines, ref updatedLinesCount);
				}

				if (AreKeysFieldsEntered(e.Row))
				{
					var relatedLines = GetRelatedLines(e.Row);
					UpdateRelatedLinesCost(e.Cache, relatedLines, ref updatedLinesCount);
				}
			}

			if (updatedLinesCount > 0 || AreTotalsAffected(e.OldRow, e.Row))
			RecalcTotals();
			if (updatedLinesCount > 0) // Request refresh only if other rows are affected.
			PIDetail.View.RequestRefresh();
		}

		protected virtual void _(Events.RowDeleted<INPIDetail> e)
		{
			if (PIHeader.Current == null || PIHeader.Cache.GetStatus(PIHeader.Current).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
				return;

			if (!IsCostCalculationEnabled() || !AreKeysFieldsEntered(e.Row) || (e.Row.VarQty ?? 0m) == 0m)
			{
				if ((e.Row.VarQty ?? 0m) != 0m)
				{
					RecalcTotals();
					PIDetail.View.RequestRefresh();
				}
				return;
			}

			// Updating costs for items related by the cost layers.
			var relatedLines = GetRelatedLines(e.Row);
			UpdateRelatedLinesCost(e.Cache, relatedLines);

			RecalcTotals();
			PIDetail.View.RequestRefresh();
		}
		#endregion

		#region INSetup
		protected virtual void _(Events.RowPersisting<INSetup> e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
			{
				PXDefaultAttribute.SetPersistingCheck<INSetup.iNTransitAcctID>(e.Cache, e.Row, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<INSetup.iNTransitSubID>(e.Cache, e.Row, PXPersistingCheck.Nothing);
			}
		}
		#endregion

		#region PIGeneratorSettings
		protected virtual void _(Events.FieldUpdated<PIGeneratorSettings, PIGeneratorSettings.pIClassID> e)
		{
			if (e.Row == null) return;
			INPIClass pi = (INPIClass)PXSelectorAttribute.Select<PIGeneratorSettings.pIClassID>(e.Cache, e.Row);
			if (pi != null)
			{
				PXCache source = Caches[typeof(INPIClass)];
				foreach (string field in e.Cache.Fields)
					if (string.Compare(field, typeof(PIGeneratorSettings.pIClassID).Name, true) != 0 && source.Fields.Contains(field))
						e.Cache.SetValuePending(e.Row, field, source.GetValueExt(pi, field));
			}
		}
		#endregion
		#endregion

		#region misc.
		public bool DisableCostCalculation = false;

		public virtual bool IsCostCalculationEnabled()
		{
			INPIHeader header = PIHeader.Current;
			return !DisableCostCalculation && header?.Status == INPIHdrStatus.Entering;
		}

		private static void CheckDefault<Field>(PXCache sender, INPIDetail row)
			where Field : IBqlField
		{
			string fieldname = typeof(Field).Name;
			if (sender.GetValue<Field>(row) == null && sender.RaiseExceptionHandling(fieldname, row, null, new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, $"[{fieldname}]"))))
				throw new PXRowPersistingException(fieldname, null, ErrorMessages.FieldIsEmpty, fieldname);
		}

		protected virtual void ValidatePIInventoryLocation(int? inventoryID, int? locationID)
		{
			if (inventoryID == null || locationID == null) return;

			var inspector = new PILocksInspector(PIHeader.Current.SiteID.Value);
			if (!inspector.IsInventoryLocationIncludedInPI(inventoryID, locationID, PIHeader.Current.PIID))
				throw new PXSetPropertyException(Messages.InventoryShouldBeUsedInCurrentPI);
		}

		private void ValidateDuplicate(string status, int? inventoryID, int? subItemID, int? locationID, string lotSerialNbr, int? lineNbr)
		{
			if (inventoryID == null || subItemID == null || locationID == null)
				return;

			foreach (INPIDetail it in
				SelectFrom<INPIDetail>.
				Where<
					INPIDetail.pIID.IsEqual<INPIHeader.pIID.FromCurrent>.
					And<INPIDetail.inventoryID.IsEqual<@P.AsInt>>.
					And<INPIDetail.subItemID.IsEqual<@P.AsInt>>.
					And<INPIDetail.locationID.IsEqual<@P.AsInt>>.
					And<INPIDetail.lineNbr.IsNotEqual<@P.AsInt>>>.
				View.Select(this, inventoryID, subItemID, locationID, lineNbr))
			{
				if (string.Equals((it.LotSerialNbr ?? "").Trim(), (lotSerialNbr ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
					throw new PXSetPropertyException(Messages.ThisCombinationIsUsedAlready, it.LineNbr);
			}

			if (string.IsNullOrEmpty(lotSerialNbr))
				return;

			INLotSerClass lsc_rec = SelectLotSerClass(inventoryID);
			if (lsc_rec.LotSerTrack == INLotSerTrack.SerialNumbered &&
				 lsc_rec.LotSerAssign == INLotSerAssign.WhenReceived)
			{
				INPIDetail serialDuplicate =
					SelectFrom<INPIDetail>.
					Where<
						INPIDetail.pIID.IsEqual<INPIHeader.pIID.FromCurrent>.
						And<INPIDetail.inventoryID.IsEqual<@P.AsInt>>.
						And<INPIDetail.lotSerialNbr.IsEqual<@P.AsString>>.
						And<INPIDetail.lineType.IsEqual<INPIDetLineType.userEntered>>.
						And<INPIDetail.lineNbr.IsNotEqual<@P.AsInt>>>.
					View.Select(this, inventoryID, lotSerialNbr, lineNbr);

				if (serialDuplicate != null)
					throw new PXSetPropertyException(Messages.ThisSerialNumberIsUsedAlready, serialDuplicate.LineNbr);

				if (status == INPIDetStatus.NotEntered)
				{
					INLotSerialStatus lotstatus =
						SelectFrom<INLotSerialStatus>.
						Where<
							INLotSerialStatus.siteID.IsEqual<INPIHeader.siteID.FromCurrent>.
							And<INLotSerialStatus.inventoryID.IsEqual<@P.AsInt>>.
							And<INLotSerialStatus.lotSerialNbr.IsEqual<@P.AsString>>.
							And<INLotSerialStatus.qtyOnHand.IsGreater<decimal0>>>.
						View.SelectWindowed(this, 0, 1, inventoryID, lotSerialNbr);

					if (lotstatus != null)
					{
						INPIDetail locationPI =
							SelectFrom<INPIDetail>.
							Where<
								INPIDetail.pIID.IsEqual<INPIHeader.pIID.FromCurrent>.
								And<INPIDetail.inventoryID.IsEqual<@P.AsInt>>.
								And<INPIDetail.locationID.IsEqual<@P.AsInt>>.
								And<INPIDetail.lotSerialNbr.IsEqual<@P.AsString>>.
								And<INPIDetail.lineNbr.IsNotEqual<@P.AsInt>>>.
							View.Select(this, inventoryID, lotstatus.LocationID, lotSerialNbr, lineNbr);

						if (locationPI == null)
							throw new PXSetPropertyException(Messages.ThisSerialNumberIsUsedInItem);
					}
				}
			}
		}

		protected virtual bool LSRequired(int? p_InventoryID)
		{
			return LSRequired(SelectLotSerClass(p_InventoryID));
		}

		protected virtual bool LSRequired(INLotSerClass lsc_rec)
		{
			return lsc_rec == null ? false :
				 lsc_rec.LotSerTrack != INLotSerTrack.NotNumbered &&
				 lsc_rec.LotSerAssign == INLotSerAssign.WhenReceived;
		}

		protected virtual INLotSerClass SelectLotSerClass(int? inventoryID)
		{
			if (inventoryID == null)
				return null;

			InventoryItem ii_rec = InventoryItem.PK.Find(this, inventoryID);
			return ii_rec.LotSerClassID == null
				? null
				: INLotSerClass.PK.Find(this, ii_rec.LotSerClassID);
		}

		protected virtual bool AreKeysFieldsEntered(INPIDetail detail)
		{
			if (detail == null)
				return false;

			bool lsIsValid = !LSRequired(detail.InventoryID) || !string.IsNullOrWhiteSpace(detail.LotSerialNbr);
			return detail.InventoryID != null && detail.SubItemID != null && detail.LocationID != null && lsIsValid;
		}

		protected virtual string GetDetailsGroupingKey(INPIDetail detail)
		{
			if (!AreKeysFieldsEntered(detail))
				throw new InvalidOperationException();

			var keyParts = new List<string>(4)
			{
				detail.InventoryID.Value.ToString(),
				detail.SubItemID.Value.ToString()
			};

			var inventoryItem = INPIDetail.FK.InventoryItem.FindParent(this, detail);
			var location = INLocation.PK.Find(this, (int)detail.LocationID);

			// CostSeparately option works only with Average and FIFO valuated items.
			if (inventoryItem.ValMethod.IsIn(INValMethod.Average, INValMethod.FIFO) && location.IsCosted == true)
			{
				keyParts.Add(detail.LocationID.Value.ToString());
			}
			else if (inventoryItem.ValMethod == INValMethod.Specific)
			{
				keyParts.Add(detail.LotSerialNbr);
			}

			return string.Join("-", keyParts);
		}
		#endregion misc.

		#region Recalc Cost methods
		protected struct CostStatusSupplInfoRec
		{
			public decimal ProjectedQty; // OnHand + Var
			public decimal ProjectedCost; // OnHand + Var
		}

		public class CostLayerInfo
		{
			public INCostStatus CostLayer { get; private set; }
			public string CostLayerType { get; private set; }
			public decimal RestQty { get; set; }
			public decimal RestCost { get; set; }

			public CostLayerInfo(INCostStatus layer, string layerType)
			{
				CostLayer = layer;
				CostLayerType = layerType;
				RestQty = layer.QtyOnHand.Value;
				RestCost = layer.TotalCost.Value;
			}
		}

		public class ProjectedTranRec
		{
			public bool AdjNotReceipt;
			public bool? ManualCost;
			public int LineNbr;
			public string UOM;
			public decimal? UnitCost;
			public decimal VarQtyPortion;
			public decimal VarCostPortion;

			// acct & sub from cost layer
			public int? AcctID;
			public int? SubID;

			public int? ProjectID;
			public int? TaskID;
			public int? CostCenterID;

			public int? InventoryID;
			public int? SubItemID;
			public int? LocationID;
			public string LotSerialNbr;
			public DateTime? ExpireDate;

			public string OrigRefNbr;
			public string ReasonCode;

			public bool IsSpecialOrder;
			public string SOOrderType;
			public string SOOrderNbr;
			public int? SOOrderLineNbr;
		}

		protected virtual ICollection<INPIDetail> GetRelatedLines(INPIDetail detail)
		{
			if (!AreKeysFieldsEntered(detail))
				throw new InvalidOperationException();

			if (detail.VarQty == null)
				return new List<INPIDetail>();

			var arguments = new List<object>();

			var relatedLinesQuery = new
				SelectFrom<INPIDetail>.
				Where<
					INPIDetail.inventoryID.IsEqual<@P.AsInt>.
					And<INPIDetail.subItemID.IsEqual<@P.AsInt>>.
					And<INPIDetail.pIID.IsEqual<@P.AsString>>>.
				View(this);
			arguments.Add(detail.InventoryID);
			arguments.Add(detail.SubItemID);
			arguments.Add(detail.PIID);

			var inventoryItem = INPIDetail.FK.InventoryItem.FindParent(this, detail);
			var location = INLocation.PK.Find(this, (int)detail.LocationID);

			// CostSeparately option works only with Average and FIFO valuated items.
			if (inventoryItem.ValMethod.IsIn(INValMethod.Average, INValMethod.FIFO))
			{
				if (location?.IsCosted == true)
				{
					relatedLinesQuery.WhereAnd<Where<INPIDetail.locationID.IsEqual<@P.AsInt>>>();
					arguments.Add(detail.LocationID);
				}
				else
				{
					relatedLinesQuery.Join<InnerJoin<INLocation, On<
						INLocation.locationID.IsEqual<INPIDetail.locationID>.
						And<INLocation.isCosted.IsEqual<False>>>>>();
				}
			}
			else if (inventoryItem.ValMethod == INValMethod.Specific)
			{
				relatedLinesQuery.WhereAnd<Where<INPIDetail.lotSerialNbr.IsEqual<@P.AsString>>>();
				arguments.Add(detail.LotSerialNbr);
			}

			var relatedLines = relatedLinesQuery.Select(arguments.ToArray())
				.RowCast<INPIDetail>()
				.Where(line => AreKeysFieldsEntered(line) && line.VarQty != null)
				.ToList();

			// Adding item with location created on-the-fly as it can't satisfy InnerJoin<INLocation, > condition.
			if (location == null && relatedLines.FirstOrDefault(line => line.LineNbr == detail.LineNbr) == null)
			{
				relatedLines.Add(detail);
			}

			return relatedLines;
		}

		protected virtual IEnumerable<ProjectedTranRec> UpdateRelatedLinesCost(
			PXCache detailCache,
			IEnumerable<INPIDetail> relatedLines,
			bool adjustmentCreation = false,
			bool forseDebitLinesRecalculation = false)
		{
			int _p = 0;
			return UpdateRelatedLinesCost(
				detailCache,
				relatedLines,
				ref _p,
				adjustmentCreation,
				forseDebitLinesRecalculation = false);
		}

		protected virtual IEnumerable<ProjectedTranRec> UpdateRelatedLinesCost(
			PXCache detailCache,
			IEnumerable<INPIDetail> relatedLines,
			ref int updatedLinesCount,
			bool adjustmentCreation = false,
			bool forseDebitLinesRecalculation = false)
		{
			var debitLines = new List<INPIDetail>();
			var creditLines = new List<INPIDetail>();
			foreach (var detail in relatedLines)
			{
				if (!AreKeysFieldsEntered(detail) || detail.VarQty == null)
					throw new InvalidOperationException();

				if (detail.VarQty >= 0m && (forseDebitLinesRecalculation
					|| detail.UnitCost == null || detail.ExtVarCost == null))
					DefaultDebitLineCost(detailCache, detail, ref updatedLinesCount);

				if (detail.VarQty == 0m)
					continue;

				if (detail.VarQty > 0)
					debitLines.Add(detail);
				else
					creditLines.Add(detail);
			}

			decimal additionalDebitQty = 0m;
			decimal additionalDebitExtCost = 0m;
			var projectedTrans = new List<ProjectedTranRec>();
			foreach (var debitLine in debitLines)
			{
				additionalDebitQty += (decimal)debitLine.VarQty;
				// We can't use debitLine.ExtVarCost here as we will lose preceision. 
				additionalDebitExtCost += (decimal)debitLine.UnitCost * (decimal)debitLine.VarQty;

				projectedTrans.Add(CreateProjectedTran(debitLine, false));
			}

			if (creditLines.Count == 0)
				return projectedTrans;

			projectedTrans.AddRange(
				UpdateCreditLinesCost(
					detailCache,
					creditLines,
					additionalDebitQty,
					additionalDebitExtCost,
					adjustmentCreation,
					ref updatedLinesCount));

			return projectedTrans;
		}

		protected virtual void DefaultDebitLineCost(PXCache detailCache, INPIDetail debitLine)
		{
			int _p = 0;
			DefaultDebitLineCost(detailCache, debitLine, ref _p);
		}

		protected virtual void DefaultDebitLineCost(PXCache detailCache, INPIDetail debitLine, ref int updatedLinesCount)
		{
			if (!AreKeysFieldsEntered(debitLine) || debitLine.VarQty == null)
				throw new InvalidOperationException();

			if (debitLine.ManualCost == true)
				return;

			debitLine.IsCostDefaulting = true;

			decimal unitCost = 0m;
			var item = InventoryItem.PK.Find(this, (int)debitLine.InventoryID);
			// INItemSite may not exist for just created items. But after the Adjustment release it will be created.
			var itemSite = INItemSite.PK.Find(this, debitLine.InventoryID, debitLine.SiteID);
			var site = INSite.PK.Find(this, debitLine.SiteID);
			
			if (item.ValMethod == INValMethod.Standard)
			{
				var itemCurySettings = InventoryItemCurySettings.PK.Find(this, item.InventoryID, site.BaseCuryID);

				unitCost = (itemSite?.StdCost ?? 0m) != 0m ? (decimal)itemSite?.StdCost
					: itemCurySettings?.StdCost ?? 0m;
			}
			else
			{
				var itemCost = INItemCost.PK.Find(this, debitLine.InventoryID, site.BaseCuryID);
				if (item.ValMethod == INValMethod.Specific)
				{
					var costFromLayer = GetCostFromLastSpecificLayer(debitLine);

					unitCost = costFromLayer != 0m ? costFromLayer
						: (itemSite?.LastCost ?? 0m) != 0m ? (decimal)itemSite?.LastCost
						: itemCost?.LastCost ?? 0m;
				}
				else // Average or FIFO
				{
					const string averageCost = INSite.avgDefaultCost.AverageCost;
					string defaultCost = item.ValMethod == INValMethod.Average ? site.AvgDefaultCost : site.FIFODefaultCost;

					unitCost = defaultCost == averageCost && (itemSite?.AvgCost ?? 0m) > 0m ? (decimal)itemSite?.AvgCost
						: (itemSite?.LastCost ?? 0m) > 0m ? (decimal)itemSite?.LastCost
						: defaultCost == averageCost && (itemCost?.AvgCost ?? 0m) > 0m ? (decimal)itemCost.AvgCost
						: itemCost?.LastCost ?? 0m;
				}
			}

			decimal prevExtCost = debitLine.ExtVarCost ?? 0m;
			detailCache.SetValueExt<INPIDetail.unitCost>(debitLine, unitCost);
			detailCache.SetValueExt<INPIDetail.extVarCost>(debitLine, PXDBPriceCostAttribute.Round(unitCost) * debitLine.VarQty.Value);

			debitLine.IsCostDefaulting = false;
			detailCache.MarkUpdated(debitLine, assertError: true);

			if (Math.Abs(prevExtCost - (debitLine.ExtVarCost ?? 0m)) > 0.000001m)
				updatedLinesCount++;
		}

		protected decimal GetCostFromLastSpecificLayer(INPIDetail specificDetail)
		{
			INCostStatus lastLayer =
				SelectFrom<INCostStatus>.
				InnerJoin<INCostSubItemXRef>.On<INCostSubItemXRef.costSubItemID.IsEqual<INCostStatus.costSubItemID>>.
				Where<
					INCostStatus.inventoryID.IsEqual<@P.AsInt>.
					And<INCostStatus.qtyOnHand.IsGreaterEqual<decimal0>>. // ignore OVERSOLD and zero layers.
					And<INCostSubItemXRef.subItemID.IsEqual<@P.AsInt>>.
					And<INCostStatus.valMethod.IsEqual<INValMethod.specific>>.
					And<INCostStatus.costSiteID.IsEqual<@P.AsInt>>.
					And<INCostStatus.lotSerialNbr.IsEqual<@P.AsString>>>.
				OrderBy<INCostStatus.receiptDate.Desc, INCostStatus.receiptNbr.Desc>.
				View.ReadOnly.SelectWindowed(this, 0, 1, specificDetail.InventoryID, specificDetail.SubItemID, specificDetail.SiteID, specificDetail.LotSerialNbr);

			if (lastLayer == null || (lastLayer.QtyOnHand ?? 0m) == 0m)
				return 0m;

			return (lastLayer.TotalCost ?? 0m) / (decimal)lastLayer.QtyOnHand;
		}

		protected virtual IEnumerable<CostLayerInfo> ReadCostLayers(INPIDetail detail)
		{
			return SelectFrom<INCostStatus>.
				InnerJoin<INCostSubItemXRef>.On<INCostSubItemXRef.costSubItemID.IsEqual<INCostStatus.costSubItemID>>.
				InnerJoin<INLocation>.On<INLocation.locationID.IsEqual<@P.AsInt>>.
				Where<
					INCostStatus.inventoryID.IsEqual<@P.AsInt>.
					And<INCostStatus.qtyOnHand.IsGreater<decimal0>>. // ignore OVERSOLD and zero layers
					And<INCostSubItemXRef.subItemID.IsEqual<@P.AsInt>>.
					And<
						Brackets<
							INCostStatus.costSiteID.IsEqual<@P.AsInt>.
							And<
								INCostStatus.valMethod.IsIn<INValMethod.standard, INValMethod.specific>.
								Or<INLocation.isCosted.IsEqual<False>>>
						>.
						Or<
							INCostStatus.costSiteID.IsEqual<@P.AsInt>.
							And<Not<
								INCostStatus.valMethod.IsIn<INValMethod.standard, INValMethod.specific>.
								Or<INLocation.isCosted.IsEqual<False>>>>
						>
					>.
					And<
						INCostStatus.lotSerialNbr.IsEqual<@P.AsString>.
						Or<INCostStatus.lotSerialNbr.IsNull>.
						Or<INCostStatus.lotSerialNbr.IsEqual<Empty>>>>.
				View.Select(
					this,
					detail.LocationID,
					detail.InventoryID,
					detail.SubItemID,
					detail.SiteID,
					detail.LocationID,
					detail.LotSerialNbr)
				.RowCast<INCostStatus>().AsEnumerable()
				.Select(layer => new CostLayerInfo(layer, CostLayerType.Normal))
				.ToList();
		}

		protected virtual IComparer<CostLayerInfo> GetCostLayerComparer(INItemSiteSettings itemSite)
		{
			return new CostLayerComparer(itemSite);
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		protected virtual IEnumerable<ProjectedTranRec> UpdateCreditLinesCost(
			PXCache detailCache,
			IEnumerable<INPIDetail> creditLines,
			decimal additionalDebitQty,
			decimal additionalDebitExtCost,
			bool adjustmentCreation)
		{
			int _p = 0;
			return UpdateCreditLinesCost(
				detailCache,
				creditLines,
				additionalDebitQty,
				additionalDebitExtCost,
				adjustmentCreation,
				ref _p);
		}

		protected virtual IEnumerable<ProjectedTranRec> UpdateCreditLinesCost(
			PXCache detailCache,
			IEnumerable<INPIDetail> creditLines,
			decimal additionalDebitQty,
			decimal additionalDebitExtCost,
			bool adjustmentCreation,
			ref int updatedLinesCount)
		{
			var sampleLine = creditLines.First();
			decimal debitLinesUnitCost = GetAdditionalUnitCost(sampleLine, additionalDebitQty, additionalDebitExtCost);

			var itemSiteSettings = INItemSiteSettings.PK.Find(this, sampleLine.InventoryID, sampleLine.SiteID);
			var costLayers = ReadCostLayers(sampleLine).ToList();
			costLayers.Sort(GetCostLayerComparer(itemSiteSettings));

			var projectedTrans = new List<ProjectedTranRec>();
			foreach (var creditLine in creditLines.OrderBy(l => (int)l.LineNbr))
			{
				// Let restQty, issuedQty and issuedExtCost be positive.
				decimal issuedQty = 0m;
				decimal issuedExtCost = 0m;
				decimal extrapolatedExtCost = 0m;
				decimal restQty = -(decimal)creditLine.VarQty;

				// Let tranQty and tranTotalCost be negative.
				decimal tranQty = 0;
				decimal tranTotalCost = 0;
				decimal tranTotalCostNotRounded = 0;

				foreach (var costLayerInfo in costLayers)
				{
					decimal intersectionQty = GetIntersectionQty(costLayerInfo, creditLine);
					if (intersectionQty == 0m) continue;

					decimal creditQty = Math.Min(intersectionQty, restQty);
					tranQty = -creditQty;
					if (creditQty == costLayerInfo.RestQty)
					{
						tranTotalCostNotRounded = tranTotalCost = -costLayerInfo.RestCost;
					}
					else
					{
						tranTotalCostNotRounded = -costLayerInfo.RestCost * creditQty / costLayerInfo.RestQty;
						tranTotalCost = PXDBCurrencyAttribute.BaseRound(this, tranTotalCostNotRounded);
					}

					issuedQty += creditQty;
					issuedExtCost += -tranTotalCostNotRounded;

					restQty -= creditQty;
					costLayerInfo.RestQty -= creditQty;
					costLayerInfo.RestCost -= -tranTotalCost;

					var projectedTran = CreateProjectedTran(costLayerInfo, creditLine, itemSiteSettings);
					projectedTran.UnitCost = PXDBPriceCostAttribute.Round(tranTotalCostNotRounded / tranQty);
					projectedTran.VarQtyPortion = tranQty;
					projectedTran.VarCostPortion = tranTotalCost;
					projectedTran.AcctID = costLayerInfo.CostLayer.AccountID;
					projectedTran.SubID = costLayerInfo.CostLayer.SubID;
					projectedTrans.Add(projectedTran);

					if (restQty == 0m)
						break;
				}

				costLayers = costLayers.Where(layer => layer.RestQty != 0m).ToList();

				if (restQty > 0)
				{
					if (additionalDebitQty >= restQty)
					{
						tranQty = -restQty;
						tranTotalCostNotRounded = -(debitLinesUnitCost * restQty);
						tranTotalCost = PXDBCurrencyAttribute.BaseRound(this, tranTotalCostNotRounded);

						additionalDebitQty -= restQty;

						issuedQty += restQty;
						issuedExtCost += -tranTotalCostNotRounded;

						projectedTrans.Add(CreateProjectedTran(
							creditLine,
							true,
							debitLinesUnitCost,
							tranQty,
							tranTotalCost));
					}
					else if (adjustmentCreation)
					{
						var location = INLocation.PK.Find(this, creditLine.LocationID);
						if (location.IsCosted == true)
						{
							throw new PXException(Messages.PINotEnoughQtyOnLocation,
								creditLine.LineNbr,
								detailCache.GetValueExt<INPIDetail.inventoryID>(creditLine),
								detailCache.GetValueExt<INPIDetail.subItemID>(creditLine),
								detailCache.GetValueExt<INPIDetail.siteID>(creditLine),
								detailCache.GetValueExt<INPIDetail.locationID>(creditLine));
						}
						else
						{
							throw new PXException(Messages.PINotEnoughQtyInWarehouse,
								creditLine.LineNbr,
								detailCache.GetValueExt<INPIDetail.inventoryID>(creditLine),
								detailCache.GetValueExt<INPIDetail.subItemID>(creditLine),
								detailCache.GetValueExt<INPIDetail.siteID>(creditLine));
						}
					}
					// If credit lines were entered first and there is not enough qty on cost layers.
					// We are going to extrapolate cost for the rest qty.
					else if (issuedQty > 0)
					{
						var extrapolationUnitCost = PXDBPriceCostAttribute.Round(issuedExtCost / issuedQty);
						extrapolatedExtCost += extrapolationUnitCost * restQty;
					}
				}

				decimal prevExtCost = creditLine.ExtVarCost ?? 0m;
				detailCache.SetValueExt<INPIDetail.unitCost>(creditLine, issuedQty != 0m ? issuedExtCost / issuedQty : 0m);
				detailCache.SetValueExt<INPIDetail.extVarCost>(creditLine, -(issuedExtCost + extrapolatedExtCost));
				detailCache.MarkUpdated(creditLine, assertError: true);

				if (Math.Abs(prevExtCost - (creditLine.ExtVarCost ?? 0m)) > 0.000001m)
					updatedLinesCount++;
			}

			return projectedTrans;
		}

		protected virtual decimal GetIntersectionQty(CostLayerInfo costLayer, INPIDetail line) => costLayer.RestQty;

		protected virtual ProjectedTranRec CreateProjectedTran(CostLayerInfo costLayerInfo, INPIDetail line, INItemSiteSettings itemSiteSettings)
        {
			ProjectedTranRec tran = CreateProjectedTran(line, true);
			tran.OrigRefNbr = itemSiteSettings.ValMethod == INValMethod.FIFO ? costLayerInfo.CostLayer.ReceiptNbr : null;
			
			return tran;
		}

		private ProjectedTranRec CreateProjectedTran(
			INPIDetail detail,
			bool adjNotReceipt,
			decimal? tranUnitCost = null,
			decimal? tranQty = null,
			decimal? tranTotalCost = null,
			string receiptNbr = null,
			int? invAcctID = null,
			int? invSubID = null)
		{
			var projTran = new ProjectedTranRec
			{
				AdjNotReceipt = adjNotReceipt,
				OrigRefNbr = receiptNbr,
				LineNbr = detail.LineNbr.Value,

				ManualCost = detail.ManualCost,
				UnitCost = tranUnitCost ?? detail.UnitCost,
				VarQtyPortion = tranQty ?? detail.VarQty.GetValueOrDefault(),
				VarCostPortion = tranTotalCost ?? detail.ExtVarCost.GetValueOrDefault(),

				AcctID = invAcctID,
				SubID = invSubID,

				InventoryID = detail.InventoryID,
				SubItemID = detail.SubItemID,
				LocationID = detail.LocationID,
				LotSerialNbr = detail.LotSerialNbr,
				ReasonCode = detail.ReasonCode,
				ExpireDate = detail.ExpireDate
			};

			return projTran;
		}

		protected virtual decimal GetAdditionalUnitCost(INPIDetail detail, decimal additionalDebitQty, decimal additionalDebitExtCost)
		{
			decimal avgDebitLinesUnitCost = additionalDebitQty != 0m ? PXDBPriceCostAttribute.Round(additionalDebitExtCost / additionalDebitQty) : 0m;
			var item = InventoryItem.PK.Find(this, detail.InventoryID);
			if (item.ValMethod == INValMethod.Standard)
			{
				var itemSite = INItemSite.PK.Find(this, detail.InventoryID, detail.SiteID);
				var site = INSite.PK.Find(this, detail.SiteID);
				var itemCurySettings = InventoryItemCurySettings.PK.Find(this, item.InventoryID, site.BaseCuryID);

				return (itemSite?.StdCost ?? 0m) != 0m ? (decimal)itemSite.StdCost
					: itemCurySettings?.StdCost ?? avgDebitLinesUnitCost;
			}

			return avgDebitLinesUnitCost;
		}

		protected virtual List<ProjectedTranRec> RecalcDemandCost(bool adjustmentCreation = false, bool forseDebitLinesRecalculation = false)
		{
			var header = (INPIHeader)PIHeader.Cache.Current;
			if (header == null || !IsCostCalculationEnabled())
				return new List<ProjectedTranRec>();

			var projectedTrans = new List<ProjectedTranRec>();
			var relatedDetailGroups = PIDetailPure.Select()
				.AsEnumerable()
				.RowCast<INPIDetail>()
				.Where(detail => AreKeysFieldsEntered(detail) && detail.Status == INPIDetStatus.Entered)
				.GroupBy(GetDetailsGroupingKey);

			foreach (var relatedDetails in relatedDetailGroups)
			{
				projectedTrans.AddRange(
					UpdateRelatedLinesCost(
						PIDetail.Cache,
						relatedDetails,
						adjustmentCreation,
						forseDebitLinesRecalculation));
			}

			return projectedTrans;
		}

		private bool skipRecalcTotals = false;
		protected virtual void RecalcTotals()
		{
			if (skipRecalcTotals)
				return;

			decimal total_var_qty = 0m, total_var_cost = 0m, total_phys_qty = 0m;
			//  manually , not via PXFormula because of the problems in INPIReview with double-counting during cost recalculation called from FieldUpdated event

			foreach (INPIDetail detail in PIDetailPure.Select())
			{
				if (detail == null || detail.Status == INPIDetStatus.Skipped)
					continue;

				total_phys_qty += detail.PhysicalQty ?? 0m;
				total_var_qty += detail.VarQty ?? 0m;
				total_var_cost += detail.FinalExtVarCost ?? detail.ExtVarCost ?? 0m;
			}

			var header = (INPIHeader)PIHeader.Cache.Current;
			if (header != null)
			{
				header.TotalPhysicalQty = total_phys_qty;
				header.TotalVarQty = total_var_qty;
				header.TotalVarCost = total_var_cost;
				PIHeader.Update(header);
			}
		}

		protected virtual bool AreTotalsAffected(INPIDetail oldDetail, INPIDetail newDetail)
		{
			if (oldDetail == null && newDetail == null)
				return false;

			if (oldDetail == null || newDetail == null)
				return true;

			return !PIDetail.Cache.ObjectsEqual<INPIDetail.physicalQty, INPIDetail.varQty,
				INPIDetail.finalExtVarCost, INPIDetail.extVarCost>(oldDetail, newDetail);
		}
		#endregion Recalc Cost methods

		#region IPXPrepareItems
		public int excelRowNumber = 2;
		public bool importHasError = false;

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			skipRecalcTotals = true;
			DisableCostCalculation = true;
			if (string.Compare(viewName, PIDetail.View.Name, true) == 0)
			{
				PXCache barCodeCache = AddByBarCode.Cache;
				INBarCodeItem item = (INBarCodeItem)(AddByBarCode.Current ?? barCodeCache.CreateInstance());
				try
				{
					barCodeCache.SetValueExt<INBarCodeItem.inventoryID>(item, GetImportedValue<INPIDetail.inventoryID>(values, true));
					if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
						barCodeCache.SetValueExt<INBarCodeItem.subItemID>(item, GetImportedValue<INPIDetail.subItemID>(values, true));
					if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
						barCodeCache.SetValueExt<INBarCodeItem.locationID>(item, GetImportedValue<INPIDetail.locationID>(values, true));
					if (PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>())
					{
						barCodeCache.SetValueExt<INBarCodeItem.lotSerialNbr>(item, GetImportedValue<INPIDetail.lotSerialNbr>(values, false));
						barCodeCache.SetValueExt<INBarCodeItem.expireDate>(item, GetImportedValue<INPIDetail.expireDate>(values, false));
					}
					barCodeCache.SetValueExt<INBarCodeItem.qty>(item, GetImportedValue<INPIDetail.physicalQty>(values, true));
					barCodeCache.SetValueExt<INBarCodeItem.autoAddLine>(item, false);
					barCodeCache.SetValueExt<INBarCodeItem.reasonCode>(item, GetImportedValue<INPIDetail.reasonCode>(values, false));

					barCodeCache.Update(item);
					UpdatePhysicalQty();
				}
				catch (Exception e)
				{
					PXTrace.WriteError(IN.Messages.RowError, excelRowNumber, e.Message);
					importHasError = true;
				}
				finally
				{
					excelRowNumber++;
				}
			}
			return false;
		}

		public bool RowImporting(string viewName, object row) => false;
		public bool RowImported(string viewName, object row, object oldRow) => false;
		public void PrepareItems(string viewName, IEnumerable items) { }

		private object GetImportedValue<Field>(IDictionary values, bool isRequired)
			where Field : IBqlField
		{
			INPIDetail item = (INPIDetail)PIDetail.Cache.CreateInstance();
			string displayName = PXUIFieldAttribute.GetDisplayName<Field>(PIDetail.Cache);
			if (!values.Contains(typeof(Field).Name) && isRequired)
				throw new PXException(Messages.CollumnIsMandatory, displayName);
			object value = values[typeof(Field).Name];
			PIDetail.Cache.RaiseFieldUpdating<Field>(item, ref value);
			if (isRequired && value == null)
				throw new PXException(ErrorMessages.FieldIsEmpty, displayName);
			return value;
		}
		#endregion

		#region IPXProcess
		public void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			skipRecalcTotals = false;
			DisableCostCalculation = false;

			RecalcDemandCost();
			RecalcTotals();

			if (importHasError)
				throw new Exception(IN.Messages.ImportHasError);
		}
		#endregion
	}

	public class INPIReview : INPIEntry  // (= PIEntry + some extended functionality)
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<INPIReview, INPIHeader>());
		protected static void Configure(WorkflowContext<INPIReview, INPIHeader> context)
		{
			var processingCategory = Common.CommonActionCategories.Get(context).Processing;

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add(g => g.finishCounting, a => a.WithCategory(processingCategory));
						actions.Add(g => g.updateCost, a => a.WithCategory(processingCategory));
						actions.Add(g => g.setNotEnteredToZero, a => a.WithCategory(processingCategory));
						actions.Add(g => g.setNotEnteredToSkipped, a => a.WithCategory(processingCategory));
						actions.Add(g => g.cancelPI, a => a.WithCategory(processingCategory));
						actions.Add(g => g.completePI, a => a.WithCategory(processingCategory));
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
					});
			});
		}

		#region Actions
		public PXAction<INPIHeader> actionsFolder;
		[PXUIField(DisplayName = Common.Messages.Actions, MapEnableRights = PXCacheRights.Select)]
		[PXButton(CommitChanges = true, MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ToolbarFolder)]
		protected virtual IEnumerable ActionsFolder(PXAdapter adapter) => adapter.Get();

		public PXAction<INPIHeader> setNotEnteredToZero;
		[PXButton]
		[PXUIField(DisplayName = Messages.SetNotEnteredToZero)]
		protected virtual IEnumerable SetNotEnteredToZero(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;
			if (header == null || !IsSetNotEnteredToZeroAllowed(header))
				return adapter.Get();

			Save.Press();

			PXLongOperation.StartOperation(this, () =>
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					INPIReview docgraph = PXGraph.CreateInstance<INPIReview>();
					docgraph.PIHeader.Current = docgraph.PIHeader.Search<INPIHeader.pIID>(header.PIID);
					if (!docgraph.IsSetNotEnteredToZeroAllowed(docgraph.PIHeader.Current))
					{
						throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
							docgraph.setNotEnteredToZero.GetCaption(),
							docgraph.PIHeader.Cache.GetRowDescription(docgraph.PIHeader.Current));
					}

					var details =
						SelectFrom<INPIDetail>.
						LeftJoin<INItemSite>.On<
							INItemSite.inventoryID.IsEqual<INPIDetail.inventoryID>.
							And<INItemSite.siteID.IsEqual<INPIDetail.siteID>>>.
						LeftJoin<INItemCost>.On<INItemCost.inventoryID.IsEqual<INPIDetail.inventoryID>.
							And<INItemCost.curyID.IsEqual<@P.AsString>>>.
						Where<INPIDetail.pIID.IsEqual<@P.AsString>>.
						OrderBy<INPIDetail.lineNbr.Asc>.
						View.Select(docgraph, header.BaseCuryID, header.PIID);

					docgraph.DisableCostCalculation = true;
					foreach (PXResult<INPIDetail, INItemSite, INItemCost> row in details)
					{
						INPIDetail detail = row;
						if (detail.Status != INPIDetStatus.NotEntered || !docgraph.AreKeysFieldsEntered(detail))
							continue;

						INItemSite.PK.StoreResult(docgraph, row);
						INItemCost.PK.StoreResult(docgraph, row);

						docgraph.PIDetail.Cache.SetValueExt<INPIDetail.physicalQty>(detail, 0m);
						docgraph.PIDetail.Cache.MarkUpdated(detail, assertError: true);
					}
					docgraph.DisableCostCalculation = false;

					docgraph.RecalcDemandCost();
					docgraph.RecalcTotals();
					docgraph.PIDetail.Cache.IsDirty = true;
					docgraph.Save.Press();

					ts.Complete();
				}
			});

			return adapter.Get();
		}

		public PXAction<INPIHeader> setNotEnteredToSkipped;
		[PXButton]
		[PXUIField(DisplayName = Messages.SetNotEnteredToSkipped)]
		protected virtual IEnumerable SetNotEnteredToSkipped(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;
			if (header == null || !IsSetNotEnteredToSkippedAllowed(header))
				return adapter.Get();

			Save.Press();

			PXLongOperation.StartOperation(this, () =>
			{
				INPIReview docgraph = PXGraph.CreateInstance<INPIReview>();
				docgraph.PIHeader.Current = docgraph.PIHeader.Search<INPIHeader.pIID>(header.PIID);
				if (!docgraph.IsSetNotEnteredToSkippedAllowed(docgraph.PIHeader.Current))
				{
					throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
						docgraph.setNotEnteredToSkipped.GetCaption(),
						docgraph.PIHeader.Cache.GetRowDescription(docgraph.PIHeader.Current));
				}

				foreach (INPIDetail detail in docgraph.PIDetailPure.Select())
				{
					if (detail.Status != INPIDetStatus.NotEntered)
						continue;

					docgraph.PIDetail.Cache.SetValue<INPIDetail.status>(detail, INPIDetStatus.Skipped);
					docgraph.PIDetail.Cache.MarkUpdated(detail, assertError: true);
				}

				docgraph.PIDetail.Cache.IsDirty = true;
				docgraph.Save.Press();
			});

			return adapter.Get();
		}

		public PXAction<INPIHeader> updateCost;
		[PXButton]
		[PXUIField(DisplayName = Messages.UpdateCost)]
		protected virtual IEnumerable UpdateCost(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;
			if (header == null || !IsUpdateCostAllowed(header))
				return adapter.Get();

			Save.Press();

			PXLongOperation.StartOperation(this, () =>
			{
				INPIReview docgraph = PXGraph.CreateInstance<INPIReview>();
				docgraph.PIHeader.Current = docgraph.PIHeader.Search<INPIHeader.pIID>(header.PIID);
				if (!docgraph.IsUpdateCostAllowed(docgraph.PIHeader.Current))
				{
					throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
						docgraph.updateCost.GetCaption(),
						docgraph.PIHeader.Cache.GetRowDescription(docgraph.PIHeader.Current));
				}

				docgraph.RecalcDemandCost(false, true);
				docgraph.RecalcTotals();

				docgraph.PIDetail.Cache.IsDirty = true;
				docgraph.Save.Press();
			});

			return adapter.Get();
		}

		public PXAction<INPIHeader> completePI;
		[PXButton]
		[PXUIField(DisplayName = Messages.CompletePI, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		public virtual IEnumerable CompletePI(PXAdapter adapter)
		{
			foreach (INPIHeader header in adapter.Get().RowCast<INPIHeader>())
			{
				PIHeader.Current = header;

				if (!IsCompletePIAllowed(header))
					throw new PXException(Messages.Document_Status_Invalid);

				bool hasNotEnteredLines = false;
				foreach (INPIDetail pd in this.PIDetail.Select())
				{
					if (pd.Status == INPIDetStatus.NotEntered && pd.InventoryID != null)
					{
						PIDetail.Cache.RaiseExceptionHandling<INPIDetail.lineNbr>(pd, pd.LineNbr,
							new PXSetPropertyException(Messages.NotEnteredLineDataError, PXErrorLevel.RowError));
						hasNotEnteredLines = true;
					}
				}

				if (hasNotEnteredLines)
				{
					throw new PXException(Messages.EnterAllLinesToCompletePI);
				}

					Save.Press();
					INPIHeader h = header;
					PXLongOperation.StartOperation(this, () => PXGraph.CreateInstance<INPIReview>().FinishEntering(h));

				yield return header;
			}
		}

		public PXAction<INPIHeader> finishCounting;
		[PXButton]
		[PXUIField(DisplayName = Messages.FinishCounting, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		public virtual IEnumerable FinishCounting(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;

			if (header == null || !IsFinishCountingAllowed(header))
				return adapter.Get();

			header.Status = INPIHdrStatus.Entering;
			PIHeader.Update(header);

			RecalcDemandCost();
			RecalcTotals();

			var piClass = INPIClass.PK.Find(this, header.PIClassID);
			if (piClass != null && piClass.UnlockSiteOnCountingFinish == true)
				CreatePILocksManager().UnlockInventory(false);

			Save.Press();

			return adapter.Get();
		}

		public PXAction<INPIHeader> cancelPI;
		[PXButton]
		[PXUIField(DisplayName = Messages.CancelPI, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		public virtual IEnumerable CancelPI(PXAdapter adapter)
		{
			INPIHeader header = PIHeader.Current;
			if (header == null || !IsCancelPIAllowed(header))
				return adapter.Get();

			header.Status = INPIHdrStatus.Cancelled;
			PIHeader.Update(header);

			CreatePILocksManager().UnlockInventory();

			Save.Press();

			return adapter.Get();
		}
		#endregion

		#region DAC overrides
		public PXFilter<PXImportAttribute.CSVSettings> cSVSettings;
		public PXFilter<PXImportAttribute.XLSXSettings> xLSXSettings;

		[PXString]
		[PXUIField(Visible = false)]
		public virtual void CSVSettings_Mode_CacheAttached(PXCache sender) { }

		[PXString]
		[PXUIField(Visible = false)]
		public virtual void XLSXSettings_Mode_CacheAttached(PXCache sender) { }
		#endregion

		public virtual void FinishEntering(INPIHeader p_h)
		{
			INSetup insetup = INSetup.Select();

			INPIHeader header = PIHeader.Current = PIHeader.Search<INPIHeader.pIID>(p_h.PIID);
			if (header == null || insetup == null || insite.Current == null)
				return;
			if (!IsCompletePIAllowed(header))
			{
				throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
					completePI.GetCaption(), PIHeader.Cache.GetRowDescription(header));
			}

			VerifyBookQty(header);

			List<ProjectedTranRec> projectedTrans = RecalcDemandCost(true);

			INRegister inAdjustment;
			bool onlyNormalLayers = true;
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				INAdjustmentEntry je = CreateAdjustmentEntry();

				je.insetup.Current.RequireControlTotal = false;
				je.insetup.Current.HoldEntry = false;

				INRegister newdoc = new INRegister();
				newdoc.BranchID = insite.Current.BranchID;
				newdoc.OrigModule = INRegister.origModule.PI;
				newdoc.PIID = header.PIID;
				je.adjustment.Cache.Insert(newdoc);
				int serialNumberAlreadyReceivedItemCntr = 0;

				var adjustmentProductionExceptions = new List<PXException>();

				foreach (ProjectedTranRec projectedTran in projectedTrans)
				{
					INLotSerClass lotSerClass =
						SelectFrom<INLotSerClass>.
						InnerJoin<InventoryItem>.On<InventoryItem.FK.LotSerialClass>.
						Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>.
						View.SelectWindowed(this, 0, 1, projectedTran.InventoryID);

					if (lotSerClass != null &&
						lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered &&
						lotSerClass.LotSerAssign == INLotSerAssign.WhenReceived)
					{
						INSiteLotSerial siteLotSerial =
							SelectFrom<INSiteLotSerial>.
							Where<INSiteLotSerial.inventoryID.IsEqual<@P.AsInt>.
							  And<INSiteLotSerial.lotSerialNbr.IsEqual<@P.AsString>.
							  And<INSiteLotSerial.siteID.IsNotEqual<@P.AsInt>.
							  And<INSiteLotSerial.qtyOnHand.IsEqual<decimal1>>>>>.
							View.Select(this, projectedTran.InventoryID, projectedTran.LotSerialNbr, p_h.SiteID);

						if (siteLotSerial != null)
						{
							InventoryItem inItem = InventoryItem.PK.Find(this, siteLotSerial.InventoryID);
							INSite site = INSite.PK.Find(this, siteLotSerial.SiteID);
							PXTrace.WriteError(Messages.ItemWithSerialNumberAlreadyReceivedInWarehouse, inItem.InventoryCD, siteLotSerial.LotSerialNbr, site.SiteCD);
							serialNumberAlreadyReceivedItemCntr++;
						}
						else
						{
							Sign sign = projectedTran.VarQtyPortion >= 0 ? Sign.Plus : Sign.Minus;
							int origVarQty = (int)Math.Abs(projectedTran.VarQtyPortion); // should be only integers
							projectedTran.VarCostPortion = projectedTran.VarCostPortion / origVarQty;
							projectedTran.VarQtyPortion = sign * 1;

							while (origVarQty > 0)
							{
								SilentProduceAdjustment(je, header, projectedTran, adjustmentProductionExceptions);
								origVarQty -= 1;
							}
						}
					}
					else
					{
						SilentProduceAdjustment(je, header, projectedTran, adjustmentProductionExceptions);
					}
				}

				HandleAdjustmentExceptions(je, adjustmentProductionExceptions);

				if (serialNumberAlreadyReceivedItemCntr > 0)
				{
					throw serialNumberAlreadyReceivedItemCntr == 1 ?
						new PXException(Messages.OneSerialNumberHaveAlreadyBeenReceived) :
						new PXException(Messages.SomeSerialNumbersHaveAlreadyBeenReceived, serialNumberAlreadyReceivedItemCntr);
				}				

				onlyNormalLayers = je.transactions.Cache.Inserted.Cast<INTran>().All(t => t.CostLayerType == CostLayerType.Normal);
				je.Save.Press();
				header.PIAdjRefNbr = je.adjustment.Current.RefNbr;
				inAdjustment = je.adjustment.Current;

				PIHeader.Current = header;
				header.Status = INPIHdrStatus.InReview;
				RecalcTotals();

				Save.Press();
				ts.Complete();
			}

			if (Setup.Current.AutoReleasePIAdjustment == true && inAdjustment != null)
			{
				if (onlyNormalLayers)
					INDocumentRelease.ReleaseDoc(new List<INRegister> { inAdjustment }, false);
				else
					throw new PXException(Messages.SpecialCostLayerPIAdjustmentShouldBeReviewed);
			}
		}

		protected virtual INAdjustmentEntry CreateAdjustmentEntry()
		{
			INAdjustmentEntry je = PXGraph.CreateInstance<INAdjustmentEntry>();
			return je;
		}

		protected virtual void HandleAdjustmentExceptions(INAdjustmentEntry je, List<PXException> exceptions)
		{
			if (exceptions.Count > 0)
			{
				throw exceptions.First();
			}
		}

		private void SilentProduceAdjustment(INAdjustmentEntry adjustmentGraph, INPIHeader header, ProjectedTranRec projectedTran, IList<PXException> exceptionsList)
		{
			try
			{
				ProduceAdjustment(adjustmentGraph, header, projectedTran);
			}
			catch (PXException ex)
			{
				exceptionsList.Add(ex);
			}
		}

		protected virtual void ProduceAdjustment(INAdjustmentEntry adjustmentGraph, INPIHeader header, ProjectedTranRec projectedTran)
		{
			// TODO: Get rid of AdjNotReceipt property and unify credit/debit adjustment lines creation.
			if (projectedTran.AdjNotReceipt)
			{
				INTran tran = new INTran();
				tran.BranchID = insite.Current.BranchID;
				tran.TranType = INTranType.Adjustment;
				tran.PIID = header.PIID;
				tran.PILineNbr = projectedTran.LineNbr;
				// INTranType.StandardCostAdjustment for standard-costed items ? ..not

				tran.InvtAcctID = projectedTran.AcctID;
				tran.InvtSubID = projectedTran.SubID;
				
				tran.AcctID = null; // left to be defaulted during release
				tran.SubID = null;
				if (projectedTran.ProjectID != null && projectedTran.TaskID != null)
				{
					tran.ProjectID = projectedTran.ProjectID;
					tran.TaskID = projectedTran.TaskID;
					tran.CostCenterID = projectedTran.CostCenterID;
				}

				tran.CostCodeID = PM.CostCodeAttribute.DefaultCostCode;
				tran.InventoryID = projectedTran.InventoryID;
				tran.SubItemID = projectedTran.SubItemID;
				tran.SiteID = header.SiteID;
				tran.LocationID = projectedTran.LocationID;

				tran.ManualCost = projectedTran.ManualCost;
				tran.UnitCost = projectedTran.UnitCost;

				tran.Qty = projectedTran.VarQtyPortion;
				tran.TranCost = projectedTran.VarCostPortion;
				tran.ReasonCode = projectedTran.ReasonCode;


				tran.IsSpecialOrder = projectedTran.IsSpecialOrder;
				tran.SOOrderType = projectedTran.SOOrderType;
				tran.SOOrderNbr = projectedTran.SOOrderNbr;
				tran.SOOrderLineNbr = projectedTran.SOOrderLineNbr;

				adjustmentGraph.CostCenterDispatcherExt?.SetCostLayerType(tran);
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Insert(tran));

				tran.OrigRefNbr = projectedTran.OrigRefNbr;
				tran.LotSerialNbr = projectedTran.LotSerialNbr;
				tran.ExpireDate = projectedTran.ExpireDate;
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Update(tran));
			}
			else
			{
				INTran tran = new INTran();
				tran.BranchID = insite.Current.BranchID;
				tran.TranType = INTranType.Adjustment;
				tran.PIID = header.PIID;
				tran.PILineNbr = projectedTran.LineNbr;
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Insert(tran));
				tran.InvtAcctID = projectedTran.AcctID;
				tran.InvtSubID = projectedTran.SubID;

				tran.AcctID = null; // left to be defaulted during release
				tran.SubID = null;
				if (projectedTran.ProjectID != null && projectedTran.TaskID != null)
				{
					tran.ProjectID = projectedTran.ProjectID;
					tran.TaskID = projectedTran.TaskID;
					tran.CostCodeID = PM.CostCodeAttribute.DefaultCostCode;
					tran.CostCenterID = projectedTran.CostCenterID;
				}

				tran.IsStockItem = true;
				tran.InventoryID = projectedTran.InventoryID;
				tran.SubItemID = projectedTran.SubItemID;
				tran.SiteID = header.SiteID;
				tran.LocationID = projectedTran.LocationID;
				tran.UOM = projectedTran.UOM;

				tran.ManualCost = projectedTran.ManualCost;
				tran.UnitCost = projectedTran.UnitCost;

				tran.Qty = projectedTran.VarQtyPortion;
				tran.TranCost = projectedTran.VarCostPortion;
				tran.ReasonCode = projectedTran.ReasonCode;

				tran.IsSpecialOrder = projectedTran.IsSpecialOrder;
				tran.SOOrderType = projectedTran.SOOrderType;
				tran.SOOrderNbr = projectedTran.SOOrderNbr;
				tran.SOOrderLineNbr = projectedTran.SOOrderLineNbr;

				adjustmentGraph.CostCenterDispatcherExt?.SetCostLayerType(tran);
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Update(tran));
				tran.LotSerialNbr = projectedTran.LotSerialNbr;
				tran.ExpireDate = projectedTran.ExpireDate;
				tran = PXCache<INTran>.CreateCopy(adjustmentGraph.transactions.Update(tran));
			}
		}

		protected virtual void VerifyBookQty(INPIHeader header)
		{
			var creditLinesAndStatus = SelectFrom<INPIDetail>.
				InnerJoin<INLocationStatus>.On<INPIDetail.FK.LocationStatus>.
				LeftJoin<INLotSerialStatus>.On<INPIDetail.FK.LotSerialStatus>.
				Where<INPIDetail.pIID.IsEqual<@P.AsString>.
					And<INPIDetail.varQty.IsLess<decimal0>>>.
				View.Select(this, header.PIID);

			int errors = 0;
			PXException lastError = null;

			foreach (PXResult<INPIDetail, INLocationStatus, INLotSerialStatus> rec in creditLinesAndStatus)
			{
				INPIDetail detail = rec;
				INLotSerialStatus lotSerialStatus = rec;
				IStatus status = lotSerialStatus.LotSerialNbr != null ? (IStatus)lotSerialStatus : rec.GetItem<INLocationStatus>();
		 
				decimal actualBookQty = status.GetBookQty();
				decimal? restOfBookQty = actualBookQty + detail.VarQty.GetValueOrDefault();
				if (restOfBookQty < 0m)
				{
					var item = InventoryItem.PK.Find(this, detail.InventoryID);
					var location = INLocation.PK.Find(this, detail.LocationID);

					if (!string.IsNullOrEmpty(detail.LotSerialNbr))
						lastError = new PXOperationCompletedWithErrorException(Messages.PIBookQtyDecreasedLS,
							item.InventoryCD.TrimEnd(), location.LocationCD.TrimEnd(), (-restOfBookQty).ToFormattedString(), item.BaseUnit, detail.LotSerialNbr);
					else
						lastError = new PXOperationCompletedWithErrorException(Messages.PIBookQtyDecreased,
							item.InventoryCD.TrimEnd(), location.LocationCD.TrimEnd(), (-restOfBookQty).ToFormattedString(), item.BaseUnit);

					errors++;
					PXTrace.WriteError(lastError);
				}
			}

			if (errors > 0)
			{
				throw errors == 1 ? lastError
					: new PXOperationCompletedWithErrorException(Messages.PIBookQtyDecreasedGeneral);
			}
		}

		#region Event handlers
		protected override void _(Events.RowSelected<INPIHeader> e)
		{
			base._(e);
			if (e.Row == null)
				return;

			cancelPI.SetEnabled(IsCancelPIAllowed(e.Row));
			finishCounting.SetEnabled(IsFinishCountingAllowed(e.Row));

			updateCost.SetEnabled(IsUpdateCostAllowed(e.Row));
			setNotEnteredToZero.SetEnabled(IsSetNotEnteredToZeroAllowed(e.Row));
			setNotEnteredToSkipped.SetEnabled(IsSetNotEnteredToSkippedAllowed(e.Row));

			completePI.SetEnabled(IsCompletePIAllowed(e.Row));
		}
		#endregion

		protected virtual bool IsCancelPIAllowed(INPIHeader h) => h.Status.IsIn(INPIHdrStatus.Counting, INPIHdrStatus.Entering);
		protected virtual bool IsFinishCountingAllowed(INPIHeader h) => h.Status == INPIHdrStatus.Counting;
		protected virtual bool IsUpdateCostAllowed(INPIHeader h) => h.Status == INPIHdrStatus.Entering;
		protected virtual bool IsSetNotEnteredToZeroAllowed(INPIHeader h) => h.Status == INPIHdrStatus.Entering;
		protected virtual bool IsSetNotEnteredToSkippedAllowed(INPIHeader h) => h.Status == INPIHdrStatus.Entering;
		protected virtual bool IsCompletePIAllowed(INPIHeader h) => h.Status == INPIHdrStatus.Entering;
	}

	#region PIInvtSiteLoc projection
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<INPIHeader>.
		InnerJoin<INPIDetail>.On<INPIDetail.FK.PIHeader>.
		AggregateTo<
			GroupBy<INPIHeader.pIID>,
			GroupBy<INPIDetail.inventoryID>,
			GroupBy<INPIHeader.siteID>,
			GroupBy<INPIDetail.locationID>>))]
	public partial class PIInvtSiteLoc : IBqlTable
	{
		#region PIID
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(INPIHeader.pIID))]
		public virtual String PIID { get; set; }
		public abstract class pIID : BqlString.Field<pIID> { }
		#endregion
		#region InventoryID
		[StockItem(IsKey = true, BqlField = typeof(INPIDetail.inventoryID))]
		[PXDefault]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region SiteID
		[Site(IsKey = true, BqlField = typeof(INPIHeader.siteID))]
		[PXDefault]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), IsKey = true, BqlField = typeof(INPIDetail.locationID))]
		[PXDefault]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : BqlInt.Field<locationID> { }
		#endregion
	}
	#endregion PIInvtSiteLoc projection
}
