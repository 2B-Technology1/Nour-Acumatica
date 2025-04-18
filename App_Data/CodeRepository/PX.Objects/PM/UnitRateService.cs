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

using CommonServiceLocator;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
    public interface IUnitRateService
    {
		decimal? CalculateUnitPrice(PXCache sender, int? projectID, int? projectTaskID, int? inventoryID, string UOM, decimal? qty, DateTime? date, long? curyInfoID);
		
		decimal? CalculateUnitCost(PXCache sender, int? projectID, int? projectTaskID, int? inventoryID, string UOM, int? employeeID, DateTime? date, long? curyInfoID);
	}

    public class UnitRateService : IUnitRateService
	{
		public virtual decimal? CalculateUnitPrice(PXCache sender, int? projectID, int? projectTaskID, int? inventoryID, string UOM, decimal? qty, DateTime? date, long? curyInfoID)
		{
			if (inventoryID != null && inventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				if (date == null)
					date = sender.Graph.Accessinfo.BusinessDate;

				string customerPriceClass = ARPriceClass.EmptyPriceClass;

				if (projectTaskID != null)
				{
					PMTask projectTask = PMTask.PK.FindDirty(sender.Graph, projectID, projectTaskID);
					CR.Location c = PXSelect<CR.Location, Where<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>.Select(sender.Graph, projectTask.LocationID);
					if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
						customerPriceClass = c.CPriceClassID;
				}

				PMProject project = PMProject.PK.Find(sender.Graph, projectID);
				CurrencyInfo curyInfo = null;

				if (curyInfoID != null)
                {
					curyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, curyInfoID);
				}

				if (curyInfo != null)
                {
					return ARSalesPriceMaint.CalculateSalesPrice(sender.Graph.Caches[typeof(PMTran)], customerPriceClass, project?.CustomerID, inventoryID, null, curyInfo, UOM, qty, date.Value, 0);
				}
                else
                {
					//retrive price without conversion. (used in Templates when Currency is set but to Fx Rate)
					var baseCuryId = GetProjectBaseCuryID(project, sender);

					curyInfo = new CM.CurrencyInfo();
					curyInfo.CuryID = project?.CuryID ?? baseCuryId;
					curyInfo.BaseCuryID = baseCuryId;

					var salePriceMaint = ARSalesPriceMaint.SingleARSalesPriceMaint;
					bool alwaysFromBase = salePriceMaint.GetAlwaysFromBaseCurrencySetting(sender);
					ARSalesPriceMaint.SalesPriceItem spItem = salePriceMaint.CalculateSalesPriceItem(sender, customerPriceClass, project?.CustomerID, inventoryID, null, curyInfo, qty, UOM, date.Value, alwaysFromBase, false);

					if (spItem != null)
                    {
						decimal salesPrice = spItem.Price;
						if (spItem.UOM != UOM)
						{
							decimal salesPriceInBase = INUnitAttribute.ConvertFromBase(sender, inventoryID, spItem.UOM, salesPrice, INPrecision.UNITCOST);
							salesPrice = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, salesPriceInBase, INPrecision.UNITCOST);
						}

						return salesPrice;
					}
				}
			}

			return null;
		}

		public virtual decimal? CalculateUnitCost(PXCache sender, int? projectID, int? projectTaskID, int? inventoryID, string UOM, int? employeeID, DateTime? date, long? curyInfoID)
		{
			if (inventoryID != null && inventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				if (date == null)
					date = sender.Graph.Accessinfo.BusinessDate;

				bool lookForLaborRates = employeeID != null;
				InventoryItem item = InventoryItem.PK.Find(sender.Graph, inventoryID);
				PMProject project = PMProject.PK.Find(sender.Graph, projectID);
				int? laborItemID = null;
				
				if (item != null)
				{
					if (item.ItemType == INItemTypes.LaborItem)
					{
						laborItemID = item.InventoryID;
					}
					else
					{
						lookForLaborRates = false;
					}
				}

				CurrencyInfo curyInfo = GetCurrencyInfo(sender, curyInfoID, GetProjectBaseCuryID(project, sender));
				
				decimal unitcostInBaseCury = 0;
				string unitCostCuryID = curyInfo.BaseCuryID;
				if (lookForLaborRates)
				{
					if (laborItemID == null && inventoryID == null)
					{
						EP.EPEmployee employee = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(sender.Graph, employeeID);

						if (employee != null)
						{
							laborItemID = employee.LabourItemID;
						}
					}

					//EmployeeID and LaborItemID.
					var cost = CreateEmployeeCostEngine(sender).CalculateEmployeeCost(null, GetRegulatHoursType(sender), laborItemID, projectID, projectTaskID, project?.CertifiedJob, null, employeeID, date.GetValueOrDefault(DateTime.Now));
					if (cost == null && laborItemID != null)
					{
						//EmployeeID only
						cost = CreateEmployeeCostEngine(sender).CalculateEmployeeCost(null, GetRegulatHoursType(sender), null, projectID, projectTaskID, project?.CertifiedJob, null, employeeID, date.GetValueOrDefault(DateTime.Now));
					}

					if (cost == null && laborItemID != null)
					{
						//LaborItemID only
						cost = CreateEmployeeCostEngine(sender).CalculateEmployeeCost(null, GetRegulatHoursType(sender), laborItemID, projectID, projectTaskID, project.CertifiedJob, null, null, date.GetValueOrDefault(DateTime.Now));
					}

					if (cost != null)
					{
						decimal unitCostForBaseUnit = cost.Rate.GetValueOrDefault();
						unitcostInBaseCury = unitCostForBaseUnit;
						unitCostCuryID = cost.CuryID;

						if (inventoryID != null || laborItemID != null)
							unitcostInBaseCury = INUnitAttribute.ConvertToBase(sender, inventoryID ?? laborItemID, UOM ?? EPSetup.Hour, unitCostForBaseUnit, INPrecision.UNITCOST);
					}
					else if (laborItemID != null && item != null)//fallback to Items Std Cost.
					{
						var itemCurySettings = InventoryItemCurySettings.PK.Find(sender.Graph, item.InventoryID, curyInfo.BaseCuryID);
						decimal unitCostForBaseUnit = (itemCurySettings?.StdCost ?? 0m);
						unitcostInBaseCury = INUnitAttribute.ConvertToBase(sender, inventoryID ?? laborItemID, UOM ?? EPSetup.Hour, unitCostForBaseUnit, INPrecision.UNITCOST);
					}
				}
				else if (item != null)
				{
					decimal unitCostForBaseUnit = 0;
					if (item.ItemType == INItemTypes.LaborItem)
					{
						var cost = CreateEmployeeCostEngine(sender).CalculateEmployeeCost(null, GetRegulatHoursType(sender), inventoryID, projectID, projectTaskID, project?.CertifiedJob, null, null, date.GetValueOrDefault(DateTime.Now));

						if (cost != null)
						{
							unitCostForBaseUnit = cost.Rate.GetValueOrDefault();
							unitCostCuryID = cost.CuryID;
						}
						else //fallback to Items Std Cost.
						{
							var itemCurySettings = InventoryItemCurySettings.PK.Find(sender.Graph, item.InventoryID, curyInfo.BaseCuryID);
							unitCostForBaseUnit = GetUnitCostForDate(itemCurySettings, date);
						}
					} 
					else if (item.StkItem == true)
					{
						INItemCost itemCost = INItemCost.PK.Find(sender.Graph, inventoryID, curyInfo.BaseCuryID);
						unitCostForBaseUnit = itemCost?.AvgCost ?? 0m;
					}
					else
					{
						var itemCurySettings = InventoryItemCurySettings.PK.Find(sender.Graph, item.InventoryID, curyInfo.BaseCuryID);
						unitCostForBaseUnit = GetUnitCostForDate(itemCurySettings, date);
					}
					unitcostInBaseCury = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, unitCostForBaseUnit, INPrecision.UNITCOST);
				}

				decimal unitCostInCury;

				if (unitCostCuryID != curyInfo.BaseCuryID)
				{
					if (unitCostCuryID == curyInfo.CuryID)
					{
						unitCostInCury = unitcostInBaseCury;
					}
					else
					{
						//Convert from unitCostCuryID to curyInfo.CuryID
						unitCostInCury = 0;
						PX.Objects.CM.Extensions.IPXCurrencyService currencyService = ServiceLocator.Current.GetInstance<Func<PXGraph, PX.Objects.CM.Extensions.IPXCurrencyService>>()(sender.Graph);
						var rate = currencyService.GetRate(unitCostCuryID, curyInfo.CuryID, curyInfo.CuryRateTypeID, curyInfo.CuryEffDate.GetValueOrDefault(DateTime.Now));

						if (rate != null)
						{
							int precision = currencyService.CuryDecimalPlaces(project.CuryID);
							unitCostInCury = CuryConvCury(rate, unitcostInBaseCury, precision);
						}
					}
				}
				else
				{
					PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury(sender, curyInfo, unitcostInBaseCury, out unitCostInCury);
				}
				
				return unitCostInCury;
			}

			return null;
		}

		protected virtual bool GetAlwaysFromBaseCurrencySetting(PXCache sender)
		{
			ARSetup arsetup = (ARSetup)sender.Graph.Caches[typeof(ARSetup)].Current ?? PXSelect<ARSetup>.Select(sender.Graph);

			return arsetup != null
				? arsetup.AlwaysFromBaseCury == true
				: false;
		}

		protected virtual CurrencyInfo GetCurrencyInfo(PXCache sender, long? curyInfoID, string projectBaseCuryID)
		{
			CurrencyInfo curyInfo;
			if (curyInfoID != null)
			{
				curyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, curyInfoID);

				if (curyInfo == null)
				{
					CM.Extensions.CurrencyInfo ext = PXSelect<CM.Extensions.CurrencyInfo, Where<CM.Extensions.CurrencyInfo.curyInfoID, Equal<Required<CM.Extensions.CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, curyInfoID);
					curyInfo = ext.GetCM();
				}
			}
			else
			{
				curyInfo = new CM.CurrencyInfo();
				curyInfo.CuryID = projectBaseCuryID;
				curyInfo.BaseCuryID = projectBaseCuryID;
				curyInfo.CuryRate = 1;
			}

			return curyInfo;
		}

		protected virtual EmployeeCostEngine CreateEmployeeCostEngine(PXCache sender)
		{
			return new EmployeeCostEngine(sender.Graph);
		}

		protected virtual string GetRegulatHoursType(PXCache sender)
        {
			EPSetup setup = PXSelect<EPSetup>.Select(sender.Graph);
			if (setup != null)
            {
				return setup.RegularHoursType;
            }

			return "RG";
		}

		protected virtual decimal CuryConvCury(CM.Extensions.IPXCurrencyRate foundRate, decimal baseval, int? precision)
		{
			if (baseval == 0) return 0m;

			if (foundRate == null)
				throw new ArgumentNullException(nameof(foundRate));

			decimal rate;
			decimal curyval;
			try
			{
				rate = (decimal)foundRate.CuryRate;
			}
			catch (InvalidOperationException)
			{
				throw new CM.PXRateNotFoundException();
			}
			if (rate == 0.0m)
			{
				rate = 1.0m;
			}
			bool mult = foundRate.CuryMultDiv != "D";
			curyval = mult ? (decimal)baseval * rate : (decimal)baseval / rate;

			if (precision.HasValue)
			{
				curyval = Decimal.Round(curyval, precision.Value, MidpointRounding.AwayFromZero);
			}

			return curyval;
		}

		protected static string GetProjectBaseCuryID(PMProject project, PXCache cache)
			=> project?.BaseCuryID ?? cache.Graph.Accessinfo.BaseCuryID;

		protected static decimal GetUnitCostForDate(InventoryItemCurySettings inventoryItemCurySettings, DateTime? date)
		{
			if (inventoryItemCurySettings == null)
				return 0m;

			if (!date.HasValue)
				return inventoryItemCurySettings.StdCost ?? 0m;

			return (date < inventoryItemCurySettings.StdCostDate
				? inventoryItemCurySettings.LastStdCost
				: inventoryItemCurySettings.StdCost) ?? 0m;
		}
	}
}
