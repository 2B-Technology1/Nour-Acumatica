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
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.CarrierService;

namespace PX.Objects.SO.GraphExtensions.CarrierRates
{
	public abstract class CarrierRatesExtension<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		#region Views
		public PXSelectExtension<Document> Documents;

		public PXSelect<Carrier, Where<Carrier.isExternal, Equal<True>, And<Carrier.carrierPluginID, IsNotNull, And<Carrier.pluginMethod, IsNotNull>>>> PlugIns;

		public PXSelect<SOCarrierRate, Where<SOCarrierRate.method, IsNotNull>, OrderBy<Asc<SOCarrierRate.deliveryDate, Asc<SOCarrierRate.amount>>>> CarrierRates;
		public virtual IEnumerable carrierRates() => Documents.Current != null ? CarrierRates.Cache.Cached : Array.Empty<SOCarrierRate>();

		public PXSetupOptional<CommonSetup> commonsetup;
		public PXSetup<ARSetup> arsetup;
		public PXSelect<CurrencyInfo> CarrierRatesDummyCuryInfo;
		#endregion

		public PXAction<TPrimary> shopRates;
		[PXUIField(DisplayName = "Shop for Rates", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable ShopRates(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				if (AskForRateSelection() == WebDialogResult.OK)
				{
					foreach (SOCarrierRate cr in CarrierRates.Cache.Cached)
					{
						if (cr.Selected == true && cr.Method != null)
						{
							try
							{
								Documents.SetValueExt<Document.shipViaUpdateFromShopForRate>(Documents.Current, true);
							Documents.SetValueExt<Document.shipVia>(Documents.Current, cr.Method);
							}
							finally
							{
								Documents.SetValueExt<Document.shipViaUpdateFromShopForRate>(Documents.Current, false);
							}

							RateHasBeenSelected(cr);
						}
					}
				}

				CarrierRates.Cache.Clear();
			}
			return adapter.Get();
		}

		public PXAction<TPrimary> refreshRates;
		[PXUIField(DisplayName = Messages.RefreshRatesButton, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable RefreshRates(PXAdapter adapter)
		{
			UpdateRates();
			return adapter.Get();
		}

		public virtual void UpdateRates()
		{
			if (Documents.Current != null)
			{
				CarrierRates.Cache.Clear();

				ValidatePackages();

				List<CarrierRequestInfo> requests = new List<CarrierRequestInfo>();
				foreach (CarrierPlugin plugin in GetApplicableCarrierPlugins())
				{
					CarrierResult<ICarrierService> serviceResult = CarrierPluginMaint.CreateCarrierService(Base, plugin);
					if (serviceResult.IsSuccess)
					{
						List<string> messages = new List<string>();
						CarrierRequest cRequest = null;

						try
						{
							cRequest = BuildQuoteRequest(Documents.Current, plugin);

							if (cRequest == null)
								continue;

							if (cRequest.PackagesEx.Count == 0)
							{
								string warning = string.Format(PXLocalizer.Localize(Messages.AutoPackagingZeroPackWarning), plugin.CarrierPluginID);
								PXTrace.WriteWarning(warning);
								messages.Add(warning);
							}
						}
						catch (Exception ex)
						{
							LogException(messages, ex);
						}

						requests.Add(new CarrierRequestInfo
						{
							Plugin = plugin,
							Service = serviceResult.Result,
							Request = cRequest,
							Messages = messages
						});
					}
					else
					{
						requests.Add(new CarrierRequestInfo
						{
							Plugin = plugin,
							Service = serviceResult.Result,
							Request = null,
							Messages = new List<string> { serviceResult.Messages.FirstOrDefault().Description }
						});
					}
				}

				Parallel.ForEach(requests, info =>
				{
					try
					{
						if (info.Request != null && info.Service != null)
						{
							info.Result = info.Service.GetRateList(info.Request);
						}
					}
					catch (Exception ex)
					{
						LogException(info.Messages, ex);
					}
				});

				int cx = 0;
				
				foreach (CarrierRequestInfo info in requests)
				{
					CarrierResult<IList<RateQuote>> result = info.Result;

					if (result != null && result.IsSuccess)
					{
						foreach (RateQuote item in result.Result)
						{
							if (item.IsSuccess && item.Currency != Documents.Current.CuryID)
							{
								if (string.IsNullOrEmpty(arsetup.Current.DefaultRateTypeID))
								{
									throw new PXException(Messages.RateTypeNotSpecified);
								}
							}

							PXSelectBase<Carrier> selectCarrier = new PXSelectReadonly<Carrier,
								Where<Carrier.carrierPluginID, Equal<Required<Carrier.carrierPluginID>>,
								And<Carrier.pluginMethod, Equal<Required<Carrier.pluginMethod>>,
								And<Carrier.isExternal, Equal<True>>>>>(Base);

							foreach (Carrier shipVia in selectCarrier.Select(info.Plugin.CarrierPluginID, item.Method.Code))
							{
								var r = new SOCarrierRate
								{
									LineNbr = cx++,
									Method = shipVia.CarrierID,
									Description = item.Method.Description,
									Amount = ConvertAmt(item.Currency, Documents.Current.CuryID, arsetup.Current.DefaultRateTypeID, Documents.Current.DocumentDate.Value, item.Amount),
									DeliveryDate = item.DeliveryDate,
									CanBeSelected = true
								};

								r.Selected = r.Method == Documents.Current.ShipVia;
								if (item.DaysInTransit > 0)
									r.DaysInTransit = item.DaysInTransit;
								r = CarrierRates.Insert(r);

								PXErrorLevel errorLevel = item.IsSuccess
															? PXErrorLevel.RowWarning
															: PXErrorLevel.RowError;

								if (item.Messages.Any())
								{
									string messages = item.Messages.Select(x => string.IsNullOrEmpty(x.Code) ? x.Description : string.Format("{0}: {1}", x.Code, x.Description)).Aggregate((i, j) => i + Environment.NewLine + j + Environment.NewLine);
									CarrierRates.Cache.RaiseExceptionHandling(typeof(SOCarrierRate.method).Name, r, null, new PXSetPropertyException("{0}", errorLevel, messages));
								}
							}
						}
					}
					else
					{					
						if (result?.Messages?.Any() == true || info.Messages?.Any() == true)
						{
							var r = new SOCarrierRate
							{
								LineNbr = cx++,
								Method = info.Plugin.CarrierPluginID,
								Description = info.Plugin.Description,
								CanBeSelected = false
							};
							r = CarrierRates.Insert(r);

							string errorMessages = string.Empty;
							if (info.Messages?.Any() == true)
							{
								errorMessages = info.Messages.Aggregate((i, j) => i + Environment.NewLine + j + Environment.NewLine);
							}

							if (result?.Messages?.Any() == true)
							{
								errorMessages += result.Messages.Select(x => x.ToString()).ToList().Aggregate((i, j) => (PXMessages.LocalizeNoPrefix(Messages.ReturnedError), info.Plugin.CarrierPluginID, i) + Environment.NewLine + j + Environment.NewLine);
							}

							if (errorMessages.Length > 0)
							{
								CarrierRates.Cache.RaiseExceptionHandling(typeof(SOCarrierRate.method).Name, r, null, new PXSetPropertyException(errorMessages, PXErrorLevel.RowError));
							}

						}
						
						if (result != null)
						{
							if (!string.IsNullOrEmpty(result.RequestData))
							{
								PXTrace.WriteInformation(result.RequestData);
							}
						}
					}
				}
			}
		}

		private static void LogException(List<string> messages, Exception ex)
		{
			PXTrace.WriteError(ex);
			messages.Add(ex.Message);
			if (ex.InnerException != null)
			{
				PXTrace.WriteError(ex.InnerException);
				messages.Add(ex.InnerException.Message);
			}
		}

		public PXAction<TPrimary> recalculatePackages;
		[PXUIField(DisplayName = "Refresh Packages", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable RecalculatePackages(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				RecalculatePackagesForOrder(Documents.Current);
			}

			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<Document> eventArgs)
		{
			string error = null;

			if (eventArgs.Row?.ShipVia != null)
			{
				var carrier = Carrier.PK.Find(Base, eventArgs.Row.ShipVia);
				if (carrier != null)
				{
					var plugin = CarrierPlugin.PK.Find(Base, carrier.CarrierPluginID);

					if (plugin != null && string.IsNullOrEmpty(plugin.PluginTypeName))
					{
						error = Common.Exceptions.FieldIsEmptyException.GetErrorText(
							Base.Caches<CarrierPlugin>(), plugin, typeof(CarrierPlugin.pluginTypeName), plugin.CarrierPluginID);
					}
				}
			}

			PXUIFieldAttribute.SetError(eventArgs.Cache, eventArgs.Row, nameof(Document.shipVia),
				error, eventArgs.Row?.ShipVia, false, PXErrorLevel.Warning);
		}

		#region SOCarrierRate
		protected virtual void _(Events.RowSelected<SOCarrierRate> e)
		{
			if (e.Row == null) return;

			PXUIFieldAttribute.SetEnabled<SOCarrierRate.selected>(CarrierRates.Cache, e.Row, e.Row.CanBeSelected != false);
		}

		protected virtual void _(Events.RowUpdated<SOCarrierRate> e)
		{
			if (e.Row != null && e.OldRow != null && e.Row.Selected != e.OldRow.Selected)
			{
				if (e.Row.Selected == true)
				{
					try
					{
						Documents.SetValueExt<Document.shipViaUpdateFromShopForRate>(Documents.Current, true);
					Documents.SetValueExt<Document.shipVia>(Documents.Current, e.Row.Method);
					}
					finally
					{
						Documents.SetValueExt<Document.shipViaUpdateFromShopForRate>(Documents.Current, false);
					}

					foreach (SOCarrierRate cr in e.Cache.Cached)
						if (cr.LineNbr != e.Row.LineNbr)
							cr.Selected = false;

					CarrierRates.View.RequestRefresh();
				}
				else
				{
					Documents.SetValueExt<Document.shipVia>(Documents.Current, null);
				}

				Documents.Cache.Update(Documents.Current);
			}
		}

		protected virtual void _(Events.RowPersisting<SOCarrierRate> e) => e.Cancel = true;
		#endregion

		protected virtual IEnumerable<CarrierPlugin> GetApplicableCarrierPlugins()
		{
			return PXSelectReadonly<CarrierPlugin>.Select(Base).RowCast<CarrierPlugin>();
		}

		protected virtual UnitsType GetUnitsType(CarrierPlugin plugin)
			=> plugin.UnitType == CarrierUnitsType.US
				? UnitsType.US
				: UnitsType.SI;

		public virtual decimal ConvertWeightValue(decimal value, CarrierPlugin plugin)
			=> ConvertValue(value, commonsetup.Current.WeightUOM, plugin.UOM);

		public virtual decimal ConvertLinearValue(decimal value, CarrierPlugin plugin)
		{
			var linearUnit = commonsetup.Current.LinearUOM;
			if (string.IsNullOrEmpty(linearUnit))
				throw new PXSetPropertyException(CS.Messages.CompanyLinearUOMIsEmpty);
			return ConvertValue(value, linearUnit, plugin.LinearUOM);
		}

		protected virtual decimal ConvertValue(decimal value, string from, string to)
		{
			if (from == to)
				return value;

			bool reverse = false;
			INUnit unit = INUnit.UK.ByGlobal.Find(Base, from, to);

			if (unit == null)
			{
				unit = INUnit.UK.ByGlobal.Find(Base, to, from);
				reverse = true;
			}

			if (unit == null)
				throw new PXException(IN.Messages.ConversionNotFound, from, to);

			decimal result = INUnitAttribute.ConvertValue(value, unit, INPrecision.NOROUND, reverse);
			return decimal.Round(result, 6, MidpointRounding.AwayFromZero);
		}

		protected virtual CarrierRequest BuildRateRequest(Document doc)
		{
			if (string.IsNullOrEmpty(doc.ShipVia))
				return null;

			Carrier carrier = Carrier.PK.Find(Base, doc.ShipVia);
			if (carrier == null)
				throw new PXException(Messages.CarrierWithIdNotFound);

			if (carrier.IsExternal != true)
				return null;

			CarrierPlugin plugin = CarrierPlugin.PK.Find(Base, carrier.CarrierPluginID);
			ValidatePlugin(plugin);

			List<string> methods = GetCarrierMethods(plugin.CarrierPluginID);

			IList<SOPackageEngine.PackSet> packSets = GetPackages(doc, suppressRecalc: true);

			if (packSets.Count == 0)
			{
				throw new PXException(Messages.AtleastOnePackageIsRequired);
			}

			List<CarrierBoxEx> boxes = new List<CarrierBoxEx>();
			foreach (SOPackageEngine.PackSet packSet in packSets)
			{
				INSite shipToWarehouse = INSite.PK.Find(Base, packSet.SiteID);
				Address warehouseAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, shipToWarehouse.AddressID);
				boxes.Add(BuildCarrierBoxes(packSet.Packages, warehouseAddress, carrier.PluginMethod, plugin));
			}

			return GetCarrierRequest(doc, GetUnitsType(plugin), methods, boxes);
		}

		protected virtual CarrierRequest BuildQuoteRequest(Document doc, CarrierPlugin plugin)
		{
			ValidatePlugin(plugin);

			List<string> methods = GetCarrierMethods(plugin.CarrierPluginID);

			List<CarrierBoxEx> boxes = new List<CarrierBoxEx>();
			foreach (string method in methods)
			{
				IList<SOPackageEngine.PackSet> packSets = GetPackages(doc);

				foreach (SOPackageEngine.PackSet packSet in packSets)
				{
					INSite shipToWarehouse = INSite.PK.Find(Base, packSet.SiteID);
					Address warehouseAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, shipToWarehouse.AddressID);

					if (warehouseAddress == null)
					{
						PXTrace.WriteWarning(Messages.WarehouseAddressIsEmpty, shipToWarehouse.SiteCD);
						continue;
					}

					boxes.Add(BuildCarrierBoxes(packSet.Packages, warehouseAddress, method, plugin));
				}
			}

			return methods.Count != 0 ? GetCarrierRequest(doc, GetUnitsType(plugin), methods, boxes) : null;
		}

		protected abstract CarrierRequest GetCarrierRequest(Document doc, UnitsType unit, List<string> methods, List<CarrierBoxEx> boxes);

		protected abstract IList<SOPackageEngine.PackSet> GetPackages(Document doc, bool suppressRecalc = false);

		protected virtual CarrierBoxEx BuildCarrierBoxes(List<SOPackageInfoEx> list, IAddressBase origin, string method, CarrierPlugin plugin)
		{
			List<CarrierBox> boxes = new List<CarrierBox>();
			foreach (SOPackageInfoEx boxInfo in list)
			{
				boxes.Add(BuildCarrierPackage(boxInfo, plugin));
			}

			CarrierBoxEx result = new CarrierBoxEx(0, 0);
			result.Packages = boxes;
			result.Method = method;
			result.Origin = origin;

			return result;
		}

		protected virtual CarrierBox BuildCarrierPackage(SOPackageInfoEx boxInfo, CarrierPlugin plugin)
		{
			decimal weightInStandardUnit = ConvertWeightValue(boxInfo.GrossWeight ?? 0, plugin);

			CarrierBox box = new CarrierBox(0, weightInStandardUnit);
			box.DeclaredValue = boxInfo.DeclaredValue.GetValueOrDefault();
			box.CarrierPackage = boxInfo.CarrierBox;
			box.Length = ConvertLinearValue(boxInfo.Length ?? 0, plugin);
			box.Width = ConvertLinearValue(boxInfo.Width ?? 0, plugin);
			box.Height = ConvertLinearValue(boxInfo.Height ?? 0, plugin);
			if (boxInfo.COD == true)
			{
				box.COD = boxInfo.DeclaredValue ?? 1;
			}
			return box;
		}

		private List<string> GetCarrierMethods(string carrierPluginID)
		{
			List<string> methods = new List<string>();
			PXSelectBase<Carrier> selectMethods = new PXSelectReadonly<Carrier, Where<Carrier.carrierPluginID, Equal<Required<Carrier.carrierPluginID>>, And<Carrier.isExternal, Equal<True>>>>(Base);
			foreach (Carrier method in selectMethods.Select(carrierPluginID))
			{
				if (!string.IsNullOrEmpty(method.PluginMethod))
				{
					if (!methods.Contains(method.PluginMethod))
						methods.Add(method.PluginMethod);
				}
			}

			return methods;
		}

		protected virtual void ValidatePackages() { }

		protected virtual void ValidatePlugin(CarrierPlugin plugin)
		{
			if (plugin == null)
				throw new PXException(Messages.CarrierPluginWithIdNotFound);

			if (string.IsNullOrEmpty(plugin.UOM))
				throw new PXException(Messages.CarrierWeightUOMIsEmpty, plugin.CarrierPluginID);

			if (string.IsNullOrEmpty(plugin.LinearUOM))
				throw new PXException(Messages.CarrierLinearUOMIsEmpty, plugin.CarrierPluginID);
		}

		protected virtual void RateHasBeenSelected(SOCarrierRate cr) { }

		protected virtual WebDialogResult AskForRateSelection() => WebDialogResult.OK;

		protected virtual decimal ConvertAmt(string from, string to, string rateType, DateTime effectiveDate, decimal amount)
		{
			if (from == to)
				return amount;

			decimal result = amount;

			PXCache curyCache = CarrierRatesDummyCuryInfo.Cache;
			using (var rs = new ReadOnlyScope(curyCache))
			{
				if (from == Base.Accessinfo.BaseCuryID)
				{
					CurrencyInfo ci = new CurrencyInfo();
					ci.CuryRateTypeID = rateType;
					ci.CuryID = to;
					ci = (CurrencyInfo)curyCache.Insert(ci);
					ci.SetCuryEffDate(curyCache, effectiveDate);
					curyCache.Update(ci);
					PXCurrencyAttribute.CuryConvCury(curyCache, ci, amount, out result);
					curyCache.Delete(ci);
				}
				else if (to == Base.Accessinfo.BaseCuryID)
				{
					CurrencyInfo ci = new CurrencyInfo();
					ci.CuryRateTypeID = rateType;
					ci.CuryID = from;
					ci = (CurrencyInfo)curyCache.Insert(ci);
					ci.SetCuryEffDate(curyCache, effectiveDate);
					curyCache.Update(ci);
					PXCurrencyAttribute.CuryConvBase(curyCache, ci, amount, out result);
					curyCache.Delete(ci);
				}
				else
				{

					CurrencyInfo ciFrom = new CurrencyInfo();
					ciFrom.CuryRateTypeID = rateType;
					ciFrom.CuryID = from;
					ciFrom = (CurrencyInfo)curyCache.Insert(ciFrom);
					ciFrom.SetCuryEffDate(curyCache, effectiveDate);
					curyCache.Update(ciFrom);
					decimal inBase;
					PXCurrencyAttribute.CuryConvBase(curyCache, ciFrom, amount, out inBase, true);

					CurrencyInfo ciTo = new CurrencyInfo();
					ciTo.CuryRateTypeID = rateType;
					ciTo.CuryID = to;
					ciTo = (CurrencyInfo)curyCache.Insert(ciTo);
					ciTo.SetCuryEffDate(curyCache, effectiveDate);
					curyCache.Update(ciFrom);
					PXCurrencyAttribute.CuryConvCury(curyCache, ciTo, inBase, out result, true);

					curyCache.Delete(ciFrom);
					curyCache.Delete(ciTo);
				}
			}

			return result;
		}

		protected virtual IList<SOPackageEngine.PackSet> CalculatePackages(Document doc, string carrierID)
		{
			Dictionary<string, SOPackageEngine.ItemStats> stats = new Dictionary<string, SOPackageEngine.ItemStats>();
			SOPackageEngine.OrderInfo orderInfo = new SOPackageEngine.OrderInfo(carrierID);

			foreach (Tuple<ILineInfo, InventoryItem> res in GetLines(doc))
			{
				ILineInfo line = res.Item1;
				InventoryItem item = res.Item2;

				if (item.PackageOption == INPackageOption.Manual)
					continue;

				orderInfo.AddLine(item, line.BaseQty);

				int inventoryID = item.PackSeparately == true
					? item.InventoryID.Value
					: SOPackageEngine.ItemStats.Mixed;

				string key = string.Format("{0}.{1}.{2}.{3}", line.SiteID, inventoryID, item.PackageOption, line.Operation);

				SOPackageEngine.ItemStats stat;
				decimal lineBaseQty = Math.Abs(line.BaseQty.GetValueOrDefault());
				decimal curyLineAmt = Math.Abs(line.CuryLineAmt.GetValueOrDefault());
				if (stats.ContainsKey(key))
				{
					stat = stats[key];
					stat.BaseQty += lineBaseQty;
					stat.BaseWeight += line.ExtWeight.GetValueOrDefault();
					stat.DeclaredValue += curyLineAmt;
					stat.AddLine(item, lineBaseQty);
				}
				else
				{
					stat = new SOPackageEngine.ItemStats();
					stat.SiteID = line.SiteID;
					stat.InventoryID = inventoryID;
					stat.Operation = line.Operation;
					stat.PackOption = item.PackageOption;
					stat.BaseQty += lineBaseQty;
					stat.BaseWeight += line.ExtWeight.GetValueOrDefault();
					stat.DeclaredValue += curyLineAmt;
					stat.AddLine(item, lineBaseQty);
					stats.Add(key, stat);
				}
			}
			orderInfo.Stats.AddRange(stats.Values);

			SOPackageEngine engine = CreatePackageEngine();
			return engine.Pack(orderInfo);
		}

		protected virtual SOPackageEngine CreatePackageEngine() => new SOPackageEngine(Base);

		protected virtual void RecalculatePackagesForOrder(Document doc)
		{
			if (doc == null)
				throw new ArgumentNullException(nameof(doc));

			if (string.IsNullOrEmpty(doc.ShipVia))
			{
				throw new PXException(Messages.ShipViaMustBeSet);
			}

			ClearPackages(doc);

			IList<SOPackageEngine.PackSet> packsets = CalculatePackages(doc, doc.ShipVia);
			foreach (SOPackageEngine.PackSet ps in packsets)
			{
				if (ps.Packages.Count > 1000)
				{
					PXTrace.WriteWarning("During autopackaging more than 1000 packages were generated. Please check your configuration.");
				}

				InsertPackages(ps.Packages);
			}
			if (packsets.Count > 0)
			{
				Documents.Cache.SetValue<Document.isPackageValid>(doc, true);
			}
		}

		protected abstract void ClearPackages(Document doc);

		protected abstract void InsertPackages(IEnumerable<SOPackageInfoEx> packages);

		protected abstract IEnumerable<Tuple<ILineInfo, InventoryItem>> GetLines(Document doc);

		private class CarrierRequestInfo
		{
			public CarrierPlugin Plugin;
			public ICarrierService Service;
			public CarrierRequest Request;
			public CarrierResult<IList<RateQuote>> Result;
			public List<string> Messages;
		}

		protected interface ILineInfo
		{
			decimal? BaseQty { get; }
			decimal? CuryLineAmt { get; }
			decimal? ExtWeight { get; }
			int? SiteID { get; }
			string Operation { get; }
		}

		protected abstract DocumentMapping GetDocumentMapping();
		protected class DocumentMapping : IBqlMapping
		{
			public Type Table { get; }
			public Type Extension => typeof(Document);

			public DocumentMapping(Type table) { Table = table; }

			public Type ShipVia = typeof(Document.shipVia);
			public Type CuryID = typeof(Document.curyID);
			public Type CuryInfoID = typeof(Document.curyInfoID);
			public Type DocumentDate = typeof(Document.documentDate);
			public Type IsPackageValid = typeof(Document.isPackageValid);
			public Type ShipViaUpdateFromShopForRate = typeof(Document.shipViaUpdateFromShopForRate);
		}
	}

	/// <exclude />
	public class Document : PXMappedCacheExtension
	{
		#region CuryID
		public virtual String CuryID { get; set; }
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region CuryInfoID
		public virtual Int64? CuryInfoID { get; set; }
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		#endregion
		#region DocumentDate
		public virtual DateTime? DocumentDate { get; set; }
		public abstract class documentDate : PX.Data.BQL.BqlDateTime.Field<documentDate> { }
		#endregion
		#region ShipVia
		public virtual String ShipVia { get; set; }
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		#endregion
		#region IsPackageValid
		public virtual Boolean? IsPackageValid { get; set; }
		public abstract class isPackageValid : PX.Data.BQL.BqlBool.Field<isPackageValid> { }
		#endregion

		#region shipViaUpdateFromShopForRate
		public virtual Boolean? ShipViaUpdateFromShopForRate { get; set; }
		public abstract class shipViaUpdateFromShopForRate : PX.Data.BQL.BqlBool.Field<shipViaUpdateFromShopForRate> { }
		#endregion
	}
}
