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
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using System.Collections;
using System.Linq;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.IN.InventoryRelease;

namespace PX.Objects.IN
{
    [Serializable]
	public partial class INSiteFilter : IBqlTable
	{
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region PendingStdCostDate
		public abstract class pendingStdCostDate : PX.Data.BQL.BqlDateTime.Field<pendingStdCostDate> { }
		protected DateTime? _PendingStdCostDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Max. Pending Cost Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? PendingStdCostDate
		{
			get
			{
				return this._PendingStdCostDate;
			}
			set
			{
				this._PendingStdCostDate = value;
			}
		}
		#endregion
		#region RevalueInventory
		public abstract class revalueInventory : PX.Data.BQL.BqlBool.Field<revalueInventory> { }
		protected Boolean? _RevalueInventory;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Revalue Inventory")]
		public virtual Boolean? RevalueInventory
		{
			get
			{
				return this._RevalueInventory;
			}
			set
			{
				this._RevalueInventory = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// Represents an inventory item, that needs to update standard cost.
	/// </summary>
	[PXProjection(typeof(
		SelectFrom<InventoryItem>.
		LeftJoin<INItemSite>.
			On<INItemSite.FK.InventoryItem.
			And<INItemSite.valMethod.IsEqual<INValMethod.standard>.
			And<INItemSite.active.IsEqual<True>.
			And<INItemSite.siteStatus.IsEqual<INItemStatus.active>.
			And<InventoryItem.stkItem.IsEqual<True>>>>>>.
		LeftJoin<InventoryItemCurySettings>.
			On<InventoryItemCurySettings.FK.Inventory.
			And<InventoryItem.stkItem.IsEqual<False>>>.
		LeftJoin<INSite>.
			On<INItemSite.FK.Site>.
		Where<InventoryItem.itemStatus.IsNotIn<INItemStatus.inactive, INItemStatus.toDelete>.
			And<InventoryItem.isTemplate.IsEqual<False>>>))]
    [Serializable]
	public partial class INUpdateStdCostRecord : IBqlTable
	{
		#region Selected
		/// <inheritdoc cref="Selected"/>
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected Boolean? _Selected = false;

		/// <summary>
		/// Specifies (if set to <see langword="true"/>) than user selected record.
		/// </summary>
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual Boolean? Selected
		{
			get
			{
				return this._Selected;
			}
			set
			{
				this._Selected = value;
			}
		}
		#endregion
		#region InventoryID
		/// <inheritdoc cref="InventoryID"/>
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;

		/// <inheritdoc cref="InventoryItem.inventoryID"/>
		[Inventory(IsKey = true, DirtyRead = true, DisplayName = "Inventory ID", BqlField = typeof(InventoryItem.inventoryID))]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region SiteID
		/// <inheritdoc cref="SiteID"/>
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;

		/// <inheritdoc cref="INItemSite.siteID"/>
		[IN.Site(BqlField = typeof(INItemSite.siteID))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
        #region RecordID
		/// <inheritdoc cref="RecordID"/>
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

		/// <summary>
		/// Record identifier.
		/// </summary>
        [PXInt(IsKey = true)]
		[PXDBCalced(typeof(Switch<Case<Where<InventoryItem.stkItem, Equal<boolTrue>>, INItemSite.siteID>, int1>), typeof(int))]
		public virtual int? RecordID { get; set; }
        #endregion
		#region InvtAcctID
		/// <inheritdoc cref="InvtAcctID"/>
		public abstract class invtAcctID : BqlInt.Field<invtAcctID> { }
		protected Int32? _InvtAcctID;

		/// <inheritdoc cref="InventoryItem.invtAcctID"/>
		[Account(DisplayName = "Inventory Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), IsDBField = false)]
		[PXDBCalced(typeof(Switch<Case<Where<InventoryItem.stkItem, Equal<boolTrue>>, INItemSite.invtAcctID>, InventoryItem.invtAcctID>), typeof(int))]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		/// <inheritdoc cref="InvtSubID"/>
		public abstract class invtSubID : BqlInt.Field<invtSubID> { }
		protected Int32? _InvtSubID;

		/// <inheritdoc cref="InventoryItem.invtSubID"/>
		[SubAccount(typeof(INUpdateStdCostRecord.invtAcctID), DisplayName = "Inventory Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), IsDBField = false)]
		[PXDBCalced(typeof(Switch<Case<Where<InventoryItem.stkItem, Equal<boolTrue>>, INItemSite.invtSubID>, InventoryItem.invtSubID>), typeof(int))]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region PendingStdCost
		/// <inheritdoc cref="PendingStdCost"/>
		public abstract class pendingStdCost : PX.Data.BQL.BqlDecimal.Field<pendingStdCost> { }
		protected Decimal? _PendingStdCost;

		/// <inheritdoc cref="INItemSite.pendingStdCost"/>
		[PXPriceCost]
		[PXDBCalced(typeof(Switch<Case<
				Where<InventoryItem.stkItem, Equal<boolTrue>>,
				INItemSite.pendingStdCost>,
			IsNull<InventoryItemCurySettings.pendingStdCost, decimal0>>), typeof(decimal))]
		[PXUIField(DisplayName = "Pending Cost")]
		public virtual Decimal? PendingStdCost
		{
			get
			{
				return this._PendingStdCost;
			}
			set
			{
				this._PendingStdCost = value;
			}
		}
		#endregion
		#region PendingStdCostDate
		/// <inheritdoc cref="PendingStdCostDate"/>
		public abstract class pendingStdCostDate : PX.Data.BQL.BqlDateTime.Field<pendingStdCostDate> { }
		protected DateTime? _PendingStdCostDate;

		/// <inheritdoc cref="INItemSite.pendingStdCostDate"/>
		[PXDate]
		[PXDBCalced(typeof(Switch<Case<
				Where<InventoryItem.stkItem, Equal<boolTrue>>,
				INItemSite.pendingStdCostDate>,
			InventoryItemCurySettings.pendingStdCostDate>), typeof(DateTime))]
		[PXUIField(DisplayName = "Pending Cost Date")]
		public virtual DateTime? PendingStdCostDate
		{
			get
			{
				return this._PendingStdCostDate;
			}
			set
			{
				this._PendingStdCostDate = value;
			}
		}
		#endregion
		#region PendingStdCostReset
		/// <inheritdoc cref="PendingStdCostReset"/>
		public abstract class pendingStdCostReset : PX.Data.BQL.BqlBool.Field<pendingStdCostReset> { }

		/// <inheritdoc cref="INItemSite.pendingStdCostReset"/>
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<
			Where<InventoryItem.stkItem, Equal<boolTrue>>,
				INItemSite.pendingStdCostReset>,
				boolFalse>), typeof(bool))]
		public virtual Boolean? PendingStdCostReset { get; set; }
		#endregion
		#region StdCost
		/// <inheritdoc cref="StdCost"/>
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }
		protected Decimal? _StdCost;

		/// <inheritdoc cref="INItemSite.stdCost"/>
		[PXPriceCost]
		[PXDBCalced(typeof(Switch<Case<
				Where<InventoryItem.stkItem, Equal<boolTrue>>,
				INItemSite.stdCost>,
			IsNull<InventoryItemCurySettings.stdCost, decimal0>>), typeof(decimal))]
		[PXUIField(DisplayName = "Current Cost", Enabled = false)]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region StdCostOverride
		/// <inheritdoc cref="StdCostOverride"/>
		public abstract class stdCostOverride : PX.Data.BQL.BqlBool.Field<stdCostOverride> { }
		protected Boolean? _StdCostOverride;

		/// <inheritdoc cref="INItemSite.stdCostOverride"/>
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<
				Where<InventoryItem.stkItem, Equal<boolTrue>>,
				INItemSite.stdCostOverride>,
			boolFalse>), typeof(bool))]
		[PXUIField(DisplayName = "Std. Cost Override")]
		public virtual Boolean? StdCostOverride
		{
			get
			{
				return this._StdCostOverride;
			}
			set
			{
				this._StdCostOverride = value;
			}
		}
		#endregion
		#region CuryID
		/// <inheritdoc cref="CuryID"/>
		public abstract class curyID : BqlString.Field<curyID> { }

		/// <inheritdoc cref="INSite.baseCuryID"/>
		[PXString(5, IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(Switch<Case<
				Where<InventoryItem.stkItem, Equal<boolTrue>>,
				INSite.baseCuryID>,
			InventoryItemCurySettings.curyID>), typeof(string))]
		[PXUIField(DisplayName = "Currency", Enabled = false, FieldClass = nameof(FeaturesSet.MultipleBaseCurrencies))]
		public virtual string CuryID { get; set; }
		#endregion
		#region IsTemplate
		/// <inheritdoc cref="IsTemplate"/>
		public abstract class isTemplate : Data.BQL.BqlBool.Field<isTemplate> { }

		/// <inheritdoc cref="InventoryItem.IsTemplate"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [Justification: false alert, see ATR-741]
		[PXDBBool(BqlField = typeof(InventoryItem.isTemplate))]
		public virtual bool? IsTemplate { get; set; }
		#endregion
		#region StkItem
		/// <inheritdoc cref="StkItem"/>
		public abstract class stkItem : BqlBool.Field<stkItem> { }

		/// <inheritdoc cref="InventoryItem.StkItem"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [Justification: false alert, see ATR-741]
		[PXDBBool(BqlField = typeof(InventoryItem.stkItem))]
		public virtual Boolean? StkItem { get; set; }
		#endregion
		#region GroupMask
		/// <inheritdoc cref="GroupMask"/>
		public abstract class groupMask : BqlByteArray.Field<groupMask> { }

		/// <inheritdoc cref="InventoryItem.GroupMask"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty [Justification: false alert, see ATR-741]
		[PXDBBinary(BqlField = typeof(InventoryItem.groupMask))]
		public virtual Byte[] GroupMask { get; set; }
		#endregion
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class INUpdateStdCost : PXGraph<INUpdateStdCost>
	{
		public PXCancel<INSiteFilter> Cancel;
		public PXFilter<INSiteFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<INUpdateStdCostRecord, INSiteFilter,
			Where2<Match<INUpdateStdCostRecord, Current<AccessInfo.userName>>,
			And<Where2<
				Where<Current<INSiteFilter.siteID>, IsNull,
					And<Where<INUpdateStdCostRecord.pendingStdCostDate, LessEqual<Current<INSiteFilter.pendingStdCostDate>>,
						Or<INUpdateStdCostRecord.pendingStdCostReset, Equal<boolTrue>>>>>,
				Or<Current<INSiteFilter.siteID>, Equal<INUpdateStdCostRecord.siteID>,
					And<Where<INUpdateStdCostRecord.pendingStdCostDate, LessEqual<Current<INSiteFilter.pendingStdCostDate>>,
						Or<Current<INSiteFilter.revalueInventory>, Equal<boolTrue>,
						Or<INUpdateStdCostRecord.pendingStdCostReset, Equal<boolTrue>>>>>>>>>> INItemList;
		public PXSetup<INSetup> insetup;

        public override bool IsDirty
        {
            get
            {
                return false;
            }
        }

		public INUpdateStdCost()
		{
			INSetup record = insetup.Current;

			INItemList.SetProcessCaption(Messages.Process);
			INItemList.SetProcessAllCaption(Messages.ProcessAll);
		}

		protected virtual void INSiteFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				if (((INSiteFilter)e.Row).RevalueInventory == true && ((INSiteFilter)e.Row).SiteID != null)
				{
					INItemList.SetProcessDelegate<INUpdateStdCostProcess>(RevalueInventory, ReleaseAdjustment);
				}
				else
				{
					INItemList.SetProcessDelegate<INUpdateStdCostProcess>(UpdateStdCost, ReleaseAdjustment);
				}
			}
		}

		[Obsolete(Objects.Common.Messages.MethodIsObsoleteAndWillBeRemoved2023R2)]
		protected virtual IEnumerable initemlist() => null;

		protected virtual void INSiteFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			INItemList.Cache.Clear();
		}

		public static void UpdateStdCost(INUpdateStdCostProcess graph, INUpdateStdCostRecord itemsite)
		{
			graph.UpdateStdCost(itemsite);
		}

		public static void RevalueInventory(INUpdateStdCostProcess graph, INUpdateStdCostRecord itemsite)
		{
			graph.RevalueInventory(itemsite);
		}
		public static void ReleaseAdjustment(INUpdateStdCostProcess graph)
		{
			graph.ReleaseAdjustment();
		}
	}

	public class INUpdateStdCostProcess : PXGraph<INUpdateStdCostProcess>
	{
		public INAdjustmentEntry je = PXGraph.CreateInstance<INAdjustmentEntry>();

		public DocumentList<INRegister> Adjustments { get; }

		public INUpdateStdCostProcess()
			: base()
		{
			je.insetup.Current.RequireControlTotal = false;
			je.insetup.Current.HoldEntry = false;

			Adjustments = new DocumentList<INRegister>(je);
		}

		protected virtual ICollection<INCostStatus> LoadCostStatuses(INUpdateStdCostRecord stdCostRecord)
		{
			var cmd = new SelectFrom<INCostStatus>
				.InnerJoin<INItemSite>
					.On<INCostStatus.FK.CostItemSite
						.And<INCostStatus.inventoryID.IsEqual<INUpdateStdCostRecord.inventoryID.FromCurrent>
						.And<INItemSite.active.IsEqual<True>>>>
				.InnerJoin<INSite>
					.On<INCostStatus.FK.CostSite>
				.Where<INSite.baseCuryID.IsEqual<INUpdateStdCostRecord.curyID.FromCurrent>
					.And<INCostStatus.costSiteID.IsEqual<INUpdateStdCostRecord.siteID.FromCurrent>
						.Or<INUpdateStdCostRecord.stdCostOverride.FromCurrent.IsEqual<False>
							.And<INUpdateStdCostRecord.pendingStdCostReset.FromCurrent.IsEqual<False>
							.And<INItemSite.stdCostOverride.IsEqual<False>
							.And<INItemSite.pendingStdCostReset.IsEqual<False>>>>>>>();

			var view = TypedViews.GetView(cmd, false);
			var costStatuses = view.SelectMultiBound(new object[] { stdCostRecord });

			return costStatuses.Select(x => (Layer: (INCostStatus)(PXResult<INCostStatus>)x, Site: ((PXResult<INCostStatus>)x).GetItem<INSite>()))
				.OrderBy(x => x.Site.BranchID)
				.Select(x => x.Layer).ToArray();
		}

		protected virtual INTran PrepareTransaction(INCostStatus layer, INSite site, InventoryItem inventoryItem, decimal? tranCost)
		{
			try
			{
				ValidateProjectLocation(site, inventoryItem);
			}
			catch (PXException ex)
			{
				PXProcessing<INUpdateStdCostRecord>.SetError(ex.Message);
				return null;
			}

			INTran tran = new INTran();

			if (layer.LayerType == INLayerType.Oversold)
			{
				tran.TranType = INTranType.NegativeCostAdjustment;
			}
			else
			{
				tran.TranType = INTranType.StandardCostAdjustment;
			}
			tran.BranchID = site.BranchID;
			tran.InvtAcctID = layer.AccountID;
			tran.InvtSubID = layer.SubID;
			var postClass = INPostClass.PK.Find(this, inventoryItem.PostClassID);
			tran.AcctID = INReleaseProcess.GetAccountDefaults<INPostClass.stdCstRevAcctID>
				(je, inventoryItem, site, postClass);
			tran.SubID = INReleaseProcess.GetAccountDefaults<INPostClass.stdCstRevSubID>
				(je, inventoryItem, site, postClass);

			tran.InventoryID = layer.InventoryID;
			tran.SubItemID = layer.CostSubItemID;
			tran.SiteID = layer.CostSiteID;
			tran.Qty = 0m;
			tran.TranCost = tranCost;
			return tran;
		}

		protected virtual void SaveAdjustment()
		{
			if (je.adjustment.Current != null && je.IsDirty)
				je.Save.Press();
		}

		protected virtual INRegister AddToAdjustment(INCostStatus layer, decimal? tranCost)
		{
			if (tranCost == 0m)
				return null;

			var site = INSite.PK.Find(this, layer.CostSiteID);
			var inventoryItem = InventoryItem.PK.Find(this, layer.InventoryID);

			bool newAdjustment = true;
			var adjustment = Adjustments.Find<INRegister.branchID>(site.BranchID);
			if (adjustment != null)
			{
				newAdjustment = false;
				INTran existTran = SelectFrom<INTran>
					.Where<INTran.docType.IsEqual<@P.AsString.ASCII>
					.And<INTran.refNbr.IsEqual<@P.AsString>
					.And<INTran.inventoryID.IsEqual<@P.AsInt>
					.And<INTran.siteID.IsEqual<@P.AsInt>>>>>
					.View.ReadOnly.SelectWindowed(je, 0, 1,
						adjustment.DocType, adjustment.RefNbr,
						layer.InventoryID, layer.CostSiteID);
				if (existTran != null)
					return null;

				if(je.adjustment.Current != adjustment)
				{
					SaveAdjustment();

					je.adjustment.Current = adjustment;
				}
			}

			var tran = PrepareTransaction(layer, site, inventoryItem, tranCost);
			if (tran == null)
				return null;

			if (newAdjustment)
			{
				SaveAdjustment();

				adjustment = (INRegister)je.adjustment.Cache.CreateInstance();
				adjustment.BranchID = site.BranchID;
				adjustment = (INRegister)je.adjustment.Cache.Insert(adjustment);
			}
			tran = je.transactions.Insert(tran);
			if (tran == null)
			{
				if (newAdjustment)
					je.Clear();

				return null;
			}
			if(newAdjustment)
				Adjustments.Add(adjustment);
			return adjustment;
		}

		public virtual void CreateAdjustments(INUpdateStdCostRecord itemsite, Func<INCostStatus, decimal?> getTranCost)
		{
			var localAdjustments = new List<INRegister>();

			foreach (INCostStatus layer in LoadCostStatuses(itemsite))
			{
				decimal? tranCost = getTranCost(layer);

				var doc = AddToAdjustment(layer, tranCost);
				if (doc != null && !localAdjustments.Contains(doc))
					localAdjustments.Add(doc);
			}

			if (localAdjustments.Any())
			{
				SaveAdjustment();

				PXProcessing<INUpdateStdCostRecord>.SetInfo(
					PXMessages.LocalizeFormatNoPrefixNLA(Messages.AdjustmentsCreated, string.Join(", ", localAdjustments.Select(x => x.RefNbr))));
			}
		}

		public virtual void UpdateStdCost(INUpdateStdCostRecord itemsite)
		{
			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					//will be true for non-stock items as well
					if (itemsite.StdCostOverride == false && itemsite.PendingStdCostReset == false)
					{
						PXResultset<INItemSite> itemSites = PXSelectJoin<INItemSite,
							InnerJoin<INSite, On<INItemSite.FK.Site>>,
							Where<INSite.baseCuryID, Equal<Required<INSite.baseCuryID>>,
								And<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
								And<INItemSite.valMethod, Equal<INValMethod.standard>,
								And<INItemSite.stdCostOverride, Equal<False>>>>>>.Select(this, itemsite.CuryID, itemsite.InventoryID);
						
						if (itemSites.Any(x => ((INItemSite)x).Active == false))
						{
							//Update standard cost for active warehouses only
							foreach (INItemSite site in itemSites.Where(x => ((INItemSite)x).Active == true))
							{
								PXUpdateJoin<
									Set<INItemSite.stdCost, INItemSite.pendingStdCost,
									Set<INItemSite.stdCostDate, Required<INItemSite.stdCostDate>,
									Set<INItemSite.pendingStdCost, decimal0,
									Set<INItemSite.pendingStdCostDate, Null,
									Set<INItemSite.lastStdCost, INItemSite.stdCost>>>>>,
									INItemSite,
									InnerJoin<INSite, On<INItemSite.FK.Site>>,
									Where<INItemSite.inventoryID.IsEqual<@P.AsInt>
										.And<INSite.baseCuryID.IsEqual<@P.AsString>>
										.And<INItemSite.stdCostOverride.IsEqual<False>>
										.And<INItemSite.pendingStdCostDate.IsLessEqual<@P.AsDateTime>>
										.And<INItemSite.pendingStdCostReset.IsEqual<False>>
										.And<INSite.siteID.IsEqual<@P.AsInt>>>>
								.Update(this,
									// Set:
									itemsite.PendingStdCostDate,
									// Where:
									itemsite.InventoryID,
									itemsite.CuryID,
									itemsite.PendingStdCostDate,
									site.SiteID
								);
							}
						}
						else
						{
							PXUpdateJoin<
								Set<INItemSite.stdCost, INItemSite.pendingStdCost,
								Set<INItemSite.stdCostDate, Required<INItemSite.stdCostDate>,
								Set<INItemSite.pendingStdCost, decimal0,
								Set<INItemSite.pendingStdCostDate, Null,
								Set<INItemSite.lastStdCost, INItemSite.stdCost>>>>>,
								INItemSite,
								InnerJoin<INSite, On<INItemSite.FK.Site>>,
								Where<INItemSite.inventoryID.IsEqual<@P.AsInt>
									.And<INSite.baseCuryID.IsEqual<@P.AsString>>
									.And<INItemSite.stdCostOverride.IsEqual<False>>
									.And<INItemSite.pendingStdCostDate.IsLessEqual<@P.AsDateTime>>
									.And<INItemSite.pendingStdCostReset.IsEqual<False>>>>
							.Update(this,
								// Set:
								itemsite.PendingStdCostDate,
								// Where:
								itemsite.InventoryID,
								itemsite.CuryID,
								itemsite.PendingStdCostDate
							);
						}

						if (itemsite.CuryID == CurrencyCollection.GetBaseCurrency()?.CuryID)
							PXDatabase.Update<InventoryItem>(
								new PXDataFieldAssign(nameof(InventoryItem.StdCost), PXDbType.DirectExpression, nameof(InventoryItem.PendingStdCost)),
								new PXDataFieldAssign(nameof(InventoryItem.StdCostDate), PXDbType.DateTime, itemsite.PendingStdCostDate),
								new PXDataFieldAssign(nameof(InventoryItem.PendingStdCost), PXDbType.Decimal, 0m),
								new PXDataFieldAssign(nameof(InventoryItem.PendingStdCostDate), PXDbType.DateTime, null),
								new PXDataFieldAssign(nameof(InventoryItem.LastStdCost), PXDbType.DirectExpression, nameof(InventoryItem.StdCost)),
								new PXDataFieldRestrict(nameof(InventoryItem.InventoryID), PXDbType.Int, itemsite.InventoryID),
								new PXDataFieldRestrict(nameof(InventoryItem.PendingStdCostDate), PXDbType.DateTime, 4, itemsite.PendingStdCostDate, PXComp.LE));

						PXDatabase.Update<InventoryItemCurySettings>(
							new PXDataFieldAssign(nameof(InventoryItemCurySettings.StdCost), PXDbType.DirectExpression, nameof(InventoryItemCurySettings.PendingStdCost)),
							new PXDataFieldAssign(nameof(InventoryItemCurySettings.StdCostDate), PXDbType.DateTime, itemsite.PendingStdCostDate),
							new PXDataFieldAssign(nameof(InventoryItemCurySettings.PendingStdCost), PXDbType.Decimal, 0m),
							new PXDataFieldAssign(nameof(InventoryItemCurySettings.PendingStdCostDate), PXDbType.DateTime, null),
							new PXDataFieldAssign(nameof(InventoryItemCurySettings.LastStdCost), PXDbType.DirectExpression, nameof(InventoryItemCurySettings.StdCost)),
							new PXDataFieldRestrict(nameof(InventoryItemCurySettings.InventoryID), PXDbType.Int, itemsite.InventoryID),
							new PXDataFieldRestrict(nameof(InventoryItemCurySettings.CuryID), PXDbType.VarChar, 5, itemsite.CuryID),
							new PXDataFieldRestrict(nameof(InventoryItemCurySettings.PendingStdCostDate), PXDbType.DateTime, 4, itemsite.PendingStdCostDate, PXComp.LE));
					}
					else
					{
						var updateParams = new List<PXDataFieldParam>
						{
							new PXDataFieldAssign(nameof(INItemSite.StdCost), PXDbType.DirectExpression, nameof(INItemSite.PendingStdCost)),
							new PXDataFieldAssign(nameof(INItemSite.StdCostDate), PXDbType.DateTime, itemsite.PendingStdCostDate ?? Accessinfo.BusinessDate),
							new PXDataFieldAssign(nameof(INItemSite.PendingStdCost), PXDbType.Decimal, 0m),
							new PXDataFieldAssign(nameof(INItemSite.PendingStdCostDate), PXDbType.DateTime, null),
							new PXDataFieldAssign(nameof(INItemSite.PendingStdCostReset), PXDbType.Bit, false),
							new PXDataFieldAssign(nameof(INItemSite.LastStdCost), PXDbType.DirectExpression, nameof(INItemSite.StdCost)),
							new PXDataFieldRestrict(nameof(INItemSite.InventoryID), PXDbType.Int, itemsite.InventoryID),
							new PXDataFieldRestrict(nameof(INItemSite.SiteID), PXDbType.Int, itemsite.SiteID)
						};
						
						// Restriction was added within AC-32883 and looks useless as in this 'else' branch double update is impossible.
						if (itemsite.PendingStdCostReset == false)
						{
							updateParams.Add(new PXDataFieldRestrict("PendingStdCostDate", PXDbType.DateTime, 4, itemsite.PendingStdCostDate, PXComp.LE));
						}

						PXDatabase.Update<INItemSite>(updateParams.ToArray());
					}

					CreateAdjustments(itemsite, 
						(layer) => PXDBCurrencyAttribute.BaseRound(this, (decimal)(layer.QtyOnHand * itemsite.PendingStdCost)) - layer.TotalCost);

					ts.Complete();
				}
			}
		}

		public virtual void RevalueInventory(INUpdateStdCostRecord itemsite)
		{
			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					CreateAdjustments(itemsite,
						(layer) => PXDBCurrencyAttribute.BaseRound(this, (decimal)(layer.QtyOnHand * itemsite.StdCost)) - layer.TotalCost);

					ts.Complete();
				}
			}			
		}


		/// <summary>
		/// Validates that there are no Quantities on hand on any Project Locations for the given Warehouse.
		/// </summary>
		/// <param name="site">Warehouse</param>
		public virtual void ValidateProjectLocation(INSite site, InventoryItem item)
		{
			var select = new PXSelectJoin<INLocationStatusByCostCenter,
				InnerJoin<INLocation, On<INLocationStatusByCostCenter.FK.Location>>,
				Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
				And<INLocation.isCosted, Equal<True>,
				And<INLocation.taskID, IsNotNull,
				And<INLocationStatusByCostCenter.inventoryID, Equal<Required<INLocationStatusByCostCenter.inventoryID>>,
				And<INLocationStatusByCostCenter.qtyOnHand, NotEqual<decimal0>>>>>>>(this);

			var res = select.SelectWindowed(0, 1, site.SiteID, item.InventoryID).ToList();
			if (res.Count == 1)
			{
				INLocation loc = (PXResult<INLocationStatusByCostCenter, INLocation>)res.First();

				var project = PM.PMProject.PK.Find(this, loc.ProjectID);

				throw new PXException(Messages.StandardCostItemOnProjectLocation, loc.LocationCD, project?.ContractCD);
			}
		}

		public virtual void ReleaseAdjustment()
		{
			if (Adjustments.Any())
			{
				INDocumentRelease.ReleaseDoc(Adjustments, false);
			}
		}
	}
}
