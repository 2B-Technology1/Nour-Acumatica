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
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.Localizations.CA.TX;
using PX.Objects.CS.DAC;
using PX.Objects.GL.DAC;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using System.Collections.Generic;

namespace PX.Objects.Localizations.CA.CS
{
	public class OrganizationMaintExt : PXGraphExtension<OrganizationMaint>
    {
        #region IsActive

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
        }

		#endregion

		#region Constants
		private const string CanadaCountryID = "CA";
		#endregion

		#region Views

		public SelectFrom<T5018OrganizationSettings>.
			LeftJoin<Organization>.On<T5018OrganizationSettings.organizationID.IsEqual<Organization.organizationID>>.
			Where<T5018OrganizationSettings.organizationID.IsEqual<Organization.organizationID.FromCurrent>>.View
			T5018Settings;

		#endregion

		#region Event Handlers		
		protected virtual void TaxRegistration_TaxID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            TaxRegistration row = e.Row as TaxRegistration;
            if (row == null)
            {
                return;
            }

            string taxID = e.NewValue as string;
            if (string.IsNullOrWhiteSpace(taxID))
            {
                throw new PXSetPropertyException<TaxRegistration.taxID>(Messages.Common.CannotBeEmpty);
            }

            // Do not use PXSelectorAttribute.Select, there's currently an issue on Acumatica's side
            // (https://jira.acumatica.com/browse/AC-57782) and the data of an extension field is
            // accessible only to the first user session that reads it.
            //
            // Technically, no problem is caused here, but used PXSelect<> anyway to be consistent throughout
            // the project.
            //
            //Tax tax = PXSelectorAttribute.Select<TaxRegistration.taxID>(sender, row, taxID) as Tax;

            Tax tax = PXSelect<Tax,
                Where<Tax.taxID,
                    Equal<Required<Tax.taxID>>>>
                .Select(sender.Graph, taxID);

            if (tax == null)
            {
                throw new PXSetPropertyException<TaxRegistration.taxID>(Messages.Common.CannotBeFound, taxID);
            }

        }

		protected virtual void _(Events.FieldDefaulting<T5018OrganizationSettings.t5018ReportingYear> e) => e.NewValue = T5018OrganizationSettings.t5018ReportingYear.FiscalYear;

		protected void _(Events.FieldUpdated<T5018OrganizationSettings.t5018ReportingYear> e)
		{
			if (e.NewValue != e.OldValue)
			{
				if (SelectFrom<T5018MasterTable>.Where<T5018MasterTable.organizationID.IsEqual<OrganizationBAccount.organizationID.FromCurrent>>.View.SelectMultiBound(this.Base, null, null).Count == 0)
					T5018Settings.Ask(Messages.T5018Messages.TaxYear, MessageButtons.OK);
				else
				{
					T5018Settings.Ask(Messages.T5018Messages.TaxYearImmutable, MessageButtons.OK);
					e.Cache.SetValue<T5018OrganizationSettings.t5018ReportingYear>(e.Row, e.OldValue);
				}
			}			
		}

		protected void _(Events.FieldUpdated<Organization.organizationLocalizationCode> e)
		{
			if (LocalizationServiceExtensions.LocalizationEnabled<FeaturesSet.canadianLocalization>(((string)e.NewValue)?.Trim()))
			{
				var org = e.Row as Organization;
				if (T5018OrganizationSettings.PK.Find(Base, org.OrganizationID) == null)
				{
					T5018Settings.Cache.Clear();
					T5018Settings.Cache.Insert(new T5018OrganizationSettings()
					{
						OrganizationID = org.OrganizationID
					});
					T5018Settings.Cache.Normalize();
				}
			}
		}

		protected virtual void _(Events.RowSelected<Organization> e)
		{
			if (e.Row != null && e.Cache.Current == e.Row)
			{
				T5018Settings.AllowSelect = LocalizationServiceExtensions.LocalizationEnabled<FeaturesSet.canadianLocalization>(e.Row.OrganizationLocalizationCode?.Trim());
			}
		}
		#endregion
	}

}
