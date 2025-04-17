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
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.TX;

namespace PX.Objects.Localizations.CA.TX
{
	public class CompanyLevelSalesTaxMaint : PXGraphExtension<SalesTaxMaint>
	{
		/// <summary>
		/// Defines the active status of the extension if the feature is currently active
		/// </summary>
		/// <returns>True, if the feature is set, false otherwise</returns>
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();

		private const string CANADA_COUNTRY_CODE = "CA";

		public SelectFrom<Address>.Where<Address.bAccountID.IsEqual<Vendor.bAccountID.AsOptional>>.View TaxAgencyLocation;

		/// <summary>
		/// Hides printing label preferences if Tax Agency is not in Canada
		/// </summary>
		/// <param name="e">Event Arguments</param>
		public virtual void _(Events.RowSelected<Tax> e)
		{
			Vendor vendor = VendorMaint.FindByID(e.Cache.Graph, e.Row.TaxVendorID);
			Address address = vendor != null ? TaxAgencyLocation.SelectSingle(vendor.BAccountID) : null;
			Base.TaxForPrintingParametersTab.AllowSelect = vendor != null && address != null && address.CountryID == CANADA_COUNTRY_CODE;
		}

		public virtual void _(Events.FieldUpdated<Tax.taxVendorID> e)
		{
			Vendor vendor = VendorMaint.FindByID(e.Cache.Graph, e.NewValue as int?);
			Address address = vendor != null ? TaxAgencyLocation.SelectSingle(vendor.BAccountID) : null;
			Base.TaxForPrintingParametersTab.AllowSelect = vendor != null && address != null && address.CountryID == CANADA_COUNTRY_CODE;
		}
	}
}
