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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.TaxProvider;

namespace PX.Objects.TX
{
	public abstract class ExternalTaxBase<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		public static bool IsExternalTax(PXGraph graph, string taxZoneID)
		{
			if (string.IsNullOrEmpty(taxZoneID) || !PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>())
				return false;

			TX.TaxZone tz = PXSelect<TX.TaxZone, Where<TX.TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(graph, taxZoneID);
			if (tz != null)
				return tz.IsExternal.GetValueOrDefault(false) && !String.IsNullOrEmpty(tz.TaxPluginID);
			else
				return false;
		}

		public static bool IsEmptyAddress(IAddressLocation address)
		{
			bool isEmptyAddress =
				string.IsNullOrEmpty(address?.PostalCode) &&
				string.IsNullOrEmpty(address?.AddressLine1);

			bool isEmptyCoordinates =
				address?.Latitude == null || address?.Latitude == 0m ||
				address?.Longitude == null || address?.Longitude == 0m;

			return isEmptyAddress && isEmptyCoordinates;
		}

		public static string CompanyCodeFromBranch(PXGraph graph, string taxZoneID, int? branchID)
		{
			TaxPluginMapping m = PXSelectJoin<TaxPluginMapping,
				InnerJoin<TaxZone, On<TaxPluginMapping.taxPluginID, Equal<TaxZone.taxPluginID>>>,
				Where<TaxPluginMapping.branchID, Equal<Required<TaxPluginMapping.branchID>>,
					And<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>>
				.Select(graph, branchID, taxZoneID);
			if (m == null)
			{
				TaxPlugin taxPlugin = PXSelectJoin<TaxPlugin,
					InnerJoin<TaxZone, On<TaxPlugin.taxPluginID, Equal<TaxZone.taxPluginID>>>,
					Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>
					.Select(graph, taxZoneID);
				if (taxPlugin == null)
				{
					throw new PXSetPropertyException(Messages.ExternalTaxProviderNotConfigured);
				}

				throw new PXException(Messages.ExternalTaxProviderBranchToCompanyCodeMappingIsMissing);
			}

			return m.CompanyCode;
		}

		public static Func<PXGraph, string, ITaxProvider> TaxProviderFactory = (PXGraph graph, string taxZoneID) =>
		{
			TX.TaxZone tz =
				PXSelect<TX.TaxZone, Where<TX.TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(graph, taxZoneID);

			if (tz.IsExternal == true && PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>() &&
				!String.IsNullOrEmpty(tz.TaxPluginID))
			{
				var provider = TaxPluginMaint.CreateTaxProvider(graph, tz.TaxPluginID);

				return provider;
			}

			return null;
		};

		public static string GetTaxID(PX.TaxProvider.TaxDetail taxDetail)
		{
			return string.Format("{0} {1:G6}", taxDetail.TaxName.ToUpperInvariant(), taxDetail.Rate * 100);
		}
	}

	public abstract class ExternalTaxBase<TGraph, TPrimary> : ExternalTaxBase<TGraph>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		private const string UseTaxSuffix = "USE";
		private const string StrConnector = " ";

		public bool skipExternalTaxCalcOnSave = false;

		[PXOverride]
		public virtual bool IsExternalTax(string taxZoneID)
        {
	        return IsExternalTax(Base, taxZoneID);
        }

		public virtual bool IsNonTaxable(IAddressBase address)
		{
			PXSelectBase<State> select = new PXSelect
				<State, Where<State.countryID, Equal<Required<State.countryID>>, And<State.stateID, Equal<Required<State.stateID>>>>>(Base);

			var state = select.SelectSingle(address.CountryID, address.State);
			return state?.NonTaxable == true;
		}

		public virtual string CompanyCodeFromBranch(string taxZoneID, int? branchID)
		{
			return CompanyCodeFromBranch(Base, taxZoneID, branchID);
		}

		[PXOverride]
		public abstract TPrimary CalculateExternalTax(TPrimary document);

		protected virtual void LogMessages(ResultBase result)
		{
			foreach (var msg in result.Messages)
			{
				PXTrace.WriteError(msg);
			}
		}

		public abstract void SkipTaxCalcAndSave();

		protected virtual decimal? GetDocDiscount()
		{
			return null;
		}

		protected virtual string GetExternalTaxProviderLocationCode(TPrimary order)
		{
			return null;
		}

		protected string GetExternalTaxProviderLocationCode<TLine, TLineDocFK, TLineSiteID>(TPrimary document)
			where TLine : class, IBqlTable, new()
			where TLineDocFK : IParameterizedForeignKeyBetween<TLine, TPrimary>, new()
			where TLineSiteID : IBqlField
		{
			TLine lineWithSite = PXSelect<TLine, Where2<TLineDocFK, And<TLineSiteID, IsNotNull>>>.SelectSingleBound(Base, new[] { document });

			if (lineWithSite == null)
				return null;

			var site = PX.Objects.IN.INSite.PK.Find(Base, (int?)Base.Caches<TLine>().GetValue<TLineSiteID>(lineWithSite));
			return site?.SiteCD;
		}

		protected Lazy<SalesTaxMaint> LazySalesTaxMaint =
			new Lazy<SalesTaxMaint>(() => PXGraph.CreateInstance<SalesTaxMaint>());

		protected virtual Tax CreateTax(TGraph graph, TaxZone taxZone, AP.Vendor taxAgency, TaxProvider.TaxDetail taxDetail, string taxID = null)
		{
			Tax tax = null;
			taxDetail.TaxType = taxDetail.TaxType == TaxProvider.TaxType.Use && taxZone.ExternalAPTaxType == ExternalAPTaxTypes.Sales
				? CSTaxType.Sales : taxDetail.TaxType;

			taxID = taxID ?? GenerateTaxId(taxDetail);

			if (!string.IsNullOrEmpty(taxID))
			{
				tax = PXSelect<Tax, Where<Tax.taxID, Equal<Required<Tax.taxID>>>>.Select(graph, taxID);

				if (tax == null)
				{
					string taxDescr = PXMessages.LocalizeFormatNoPrefixNLA(TX.Messages.ExternalTaxProviderTaxFor, taxDetail.JurisType, taxDetail.JurisName);

					tax = new Tax
					{
						TaxID = taxID,
						Descr = taxDescr.Length > Tax.descr.MaxLength ? taxDescr.Substring(0, Tax.descr.MaxLength) : taxDescr,
						TaxType = taxDetail.TaxType,
						TaxCalcType = CSTaxCalcType.Doc,
						TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt,
						TaxApplyTermsDisc = CSTaxTermsDiscount.ToTaxableAmount,
						SalesTaxAcctID = taxAgency.SalesTaxAcctID,
						SalesTaxSubID = taxAgency.SalesTaxSubID,
						ExpenseAccountID = taxAgency.TaxExpenseAcctID,
						ExpenseSubID = taxAgency.TaxExpenseSubID,
						TaxVendorID = taxZone.TaxVendorID,
						IsExternal = true
					};

					var salesTaxMaint = LazySalesTaxMaint.Value;
					salesTaxMaint.Clear();

					salesTaxMaint.Tax.Insert(tax);
					salesTaxMaint.Save.Press();
				}
			}
			return tax;
		}

		protected virtual AP.Vendor GetTaxAgency(TGraph graph, TaxZone taxZone, bool checkSalesTaxAcct = false)
		{
			if (taxZone == null)
				throw new PXException(SO.Messages.TaxZoneIsNotSet);

			AP.Vendor vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>
				.Select(graph, taxZone.TaxVendorID);

			if (vendor == null)
				throw new PXException(TX.Messages.ExternalTaxVendorNotFound);

			if (checkSalesTaxAcct)
			{
				if (vendor.SalesTaxAcctID == null)
					throw new PXException(TX.Messages.TaxPayableAccountNotSpecified, vendor.AcctCD);

				if (vendor.SalesTaxSubID == null)
					throw new PXException(TX.Messages.TaxPayableSubNotSpecified, vendor.AcctCD);
			}

			return vendor;
		}

		protected virtual string GenerateTaxId(TaxProvider.TaxDetail taxDetail)
		{
			string taxID = taxDetail.TaxName;

			if (string.IsNullOrEmpty(taxID))
				taxID = taxDetail.JurisCode;

			if (!string.IsNullOrEmpty(taxID))
			{
				if (taxDetail.TaxType == PX.TaxProvider.TaxType.Use)
				{
					if (taxID.Length > Tax.taxID.MaxLengthForExternalTaxID)
						taxID = taxID.Substring(0, Tax.taxID.MaxLengthForExternalTaxID);

					taxID = taxID + StrConnector + UseTaxSuffix;
				}
			}
			else
			{
				PXTrace.WriteInformation(AP.Messages.EmptyValuesFromExternalTaxProvider);
			}

			return taxID;
		}
	}

	public abstract class ExternalTax<TGraph, TPrimary> : ExternalTaxBase<TGraph, TPrimary>
		where TGraph : PXGraph<TGraph, TPrimary>
		where TPrimary : class, IBqlTable, new()
	{
		public override void SkipTaxCalcAndSave()
		{
			try
			{
				skipExternalTaxCalcOnSave = true;
				Base.Save.Press();
			}
			finally
			{
				skipExternalTaxCalcOnSave = false;
			}
		}
	}

	sealed class ExternalTax : ExternalTaxBase<PXGraph>
	{

	}
}
