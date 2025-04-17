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
using PX.Objects.CR;
using PX.Data.Localization;
using PX.Objects.CS;
using PX.Objects.GL.DAC;
using PX.Objects.Localizations.CA.TX;
using PX.Objects.TX;

namespace PX.Objects.Localizations.CA.CS
{
	public class CompanyLevelOrganizationMaint : PXGraphExtension<OrganizationMaint>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();


		public PXSelectJoin<TaxRegistration,
			InnerJoin<Tax, On<Tax.taxID, Equal<TaxRegistration.taxID>>>,
			Where<TaxRegistration.bAccountID, Equal<Current<BAccount.bAccountID>>>>
			Taxes;

		public virtual void _(Events.RowSelected<Organization> e)
		{
			if (e.Row == null) return;
			if (LocalizationServiceExtensions.LocalizationEnabled<FeaturesSet.canadianLocalization>(e.Row?.OrganizationLocalizationCode?.Trim()))
			{
				Taxes.Cache.AllowSelect = true;
			}
			else
			{
				Taxes.Cache.AllowSelect = false;
			}
		}
	}
}
