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
using PX.Data;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using PX.Objects.CS;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using System.Linq;
using PX.Objects.AP;

namespace PX.Objects.TX
{
	public class TaxZoneMaint : PXGraph<TaxZoneMaint, TaxZone>
	{
		public PXSelect<TaxZone> TxZone;
		public PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<TaxZone.taxZoneID>>>> TxZoneCurrent;
		public PXSelectJoin<TaxZoneDet, InnerJoin<Tax, On<TaxZoneDet.taxID, Equal<Tax.taxID>>>, Where<TaxZoneDet.taxZoneID, Equal<Current<TaxZone.taxZoneID>>>> Details;
        public PXSelect<TaxZoneDet> TxZoneDet;

		[PXImport(typeof(TaxZone))]
		[PXCopyPasteHiddenView]
		public PXSelect<TaxZoneAddressMapping, Where<TaxZoneAddressMapping.taxZoneID, Equal<Current<TaxZone.taxZoneID>>>> TaxZoneAddressMappings;

		public TaxZoneMaint()
		{
			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(GL.Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
		}
		public PXSetup<GL.Branch> Company;

		[PXHidden]
		public PXSelect<VendorClass, Where<VendorClass.taxZoneID, Equal<Current<TaxZone.taxZoneID>>>> VendorClass;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search<Tax.taxID, Where<Tax.isExternal, Equal<False>>>), new Type[] { typeof(Tax.taxID), typeof(Tax.descr), typeof(Tax.directTax) })]
		public virtual void _(Events.CacheAttached<TaxZoneDet.taxID> e) { }

		protected virtual void TaxZoneDet_TaxID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			TaxZoneDet tax = (TaxZoneDet)e.Row;
			string taxId = (string) e.NewValue;
			if (tax.TaxID != taxId) 
			{
				List<string> allTaxes = new List<string>() { taxId };

				foreach (TaxZoneDet iTax in this.Details.Select())
				{
					if (iTax.TaxID == taxId) 
					{
						e.Cancel = true;
						throw new PXSetPropertyException(Messages.TaxAlreadyInList);
					}
					allTaxes.Add(iTax.TaxID);
				}

				if (!TryValidateTaxCategoryCombinationWithDirectTax(allTaxes.ToArray(), out PXResultset<TaxCategoryDet> invalidCategoryCombinations))
				{
					e.Cancel = true;
					throw new PXSetPropertyException(Messages.SeveralImportTaxesCanNotBeInSameZoneAndCategory,
						((TaxCategoryDet)invalidCategoryCombinations)?.TaxCategoryID, TxZone.Current?.TaxZoneID);
				}
			}
		}

		protected virtual void TaxZone_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			TaxZone row = e.Row as TaxZone;
			if (row == null) return;

			var externalTaxActive = PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();

			PXUIFieldAttribute.SetVisible<TaxZone.isExternal>(cache, null, externalTaxActive);
			PXUIFieldAttribute.SetVisible<TaxZone.taxPluginID>(cache, e.Row, row.IsExternal == true);
			PXUIFieldAttribute.SetVisible<TaxZone.taxVendorID>(cache, e.Row, row.IsExternal == true);
			PXUIFieldAttribute.SetVisible<TaxZone.taxID>(cache, e.Row, row.IsManualVATZone == true);
			PXDefaultAttribute.SetPersistingCheck<TaxZone.taxID>(cache, e.Row, 
				PXAccess.FeatureInstalled<FeaturesSet.manualVATEntryMode>() && row.IsManualVATZone == true ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<TaxZone.taxVendorID>(cache, e.Row, row.IsExternal == true ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			
			PXUIFieldAttribute.SetVisible<TaxZoneAddressMapping.countryID>(TaxZoneAddressMappings.Cache, null, row.MappingType == MappingTypesAttribute.OneOrMoreCountires);
			PXUIFieldAttribute.SetVisible<TaxZoneAddressMapping.stateID>(TaxZoneAddressMappings.Cache, null, row.MappingType == MappingTypesAttribute.OneOrMoreStates);
			PXUIFieldAttribute.SetVisible<TaxZoneAddressMapping.fromPostalCode>(TaxZoneAddressMappings.Cache, null, row.MappingType == MappingTypesAttribute.OneOrMorePostalCodes);
			PXUIFieldAttribute.SetVisible<TaxZoneAddressMapping.toPostalCode>(TaxZoneAddressMappings.Cache, null, row.MappingType == MappingTypesAttribute.OneOrMorePostalCodes);

			PXUIFieldAttribute.SetEnabled<TaxZoneAddressMapping.countryID>(TaxZoneAddressMappings.Cache, null, row.MappingType == MappingTypesAttribute.OneOrMoreCountires);
			PXUIFieldAttribute.SetEnabled<TaxZoneAddressMapping.stateID>(TaxZoneAddressMappings.Cache, null,  row.MappingType == MappingTypesAttribute.OneOrMoreStates);
			PXUIFieldAttribute.SetEnabled<TaxZoneAddressMapping.fromPostalCode>(TaxZoneAddressMappings.Cache, null, row.MappingType == MappingTypesAttribute.OneOrMorePostalCodes);
			PXUIFieldAttribute.SetEnabled<TaxZoneAddressMapping.toPostalCode>(TaxZoneAddressMappings.Cache, null,  row.MappingType == MappingTypesAttribute.OneOrMorePostalCodes);

			bool isMigratedData = row.CountryID == string.Empty && row.MappingType == MappingTypesAttribute.OneOrMorePostalCodes;
			PXUIFieldAttribute.SetEnabled<TaxZone.mappingType>(cache, null, !isMigratedData);
			PXUIFieldAttribute.SetEnabled<TaxZone.countryID>(cache, null, row.MappingType != MappingTypesAttribute.OneOrMoreCountires);

			bool isMappingTypeGridEnabled = !isMigratedData && (row.CountryID != null || row.MappingType == MappingTypesAttribute.OneOrMoreCountires);
			TaxZoneAddressMappings.Cache.AllowInsert = isMappingTypeGridEnabled;
			TaxZoneAddressMappings.Cache.AllowUpdate = isMappingTypeGridEnabled;
			TaxZoneAddressMappings.Cache.AllowDelete = isMappingTypeGridEnabled;

			Details.Cache.AllowInsert = row.IsExternal != true;
			Details.Cache.AllowUpdate = row.IsExternal != true;
			Details.Cache.AllowDelete = row.IsExternal != true;

			TaxPlugin plugin = PXSelect<TaxPlugin, Where<TaxPlugin.taxPluginID, Equal<Required<TaxPlugin.taxPluginID>>>>
				.Select(this, row.TaxPluginID);
			bool processUseTaxVisible = plugin?.PluginTypeName != null && plugin.PluginTypeName.Contains("PX.TaxProvider.AvalaraRest.AvalaraRestTaxProvider");
			PXUIFieldAttribute.SetVisible<TaxZone.externalAPTaxType>(cache, e.Row, row.IsExternal == true && processUseTaxVisible);
		}

		protected virtual void _(Events.RowSelected<TaxZoneAddressMapping> e)
		{
			PXDefaultAttribute.SetPersistingCheck<TaxZoneAddressMapping.stateID>(e.Cache, e.Row, TxZone.Current.MappingType == MappingTypesAttribute.OneOrMoreStates ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<TaxZoneAddressMapping.fromPostalCode>(e.Cache, e.Row, TxZone.Current.MappingType == MappingTypesAttribute.OneOrMorePostalCodes ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<TaxZoneAddressMapping.toPostalCode>(e.Cache, e.Row, TxZone.Current.MappingType == MappingTypesAttribute.OneOrMorePostalCodes ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXUIFieldAttribute.SetVisibility<TaxZoneAddressMapping.countryID>(TaxZoneAddressMappings.Cache, e.Row, TxZone.Current.MappingType == MappingTypesAttribute.OneOrMoreCountires ? PXUIVisibility.Visible : PXUIVisibility.HiddenByAccessRights);
			PXUIFieldAttribute.SetVisibility<TaxZoneAddressMapping.stateID>(TaxZoneAddressMappings.Cache, e.Row, TxZone.Current.MappingType == MappingTypesAttribute.OneOrMoreStates ? PXUIVisibility.Visible : PXUIVisibility.HiddenByAccessRights);
			PXUIFieldAttribute.SetVisibility<TaxZoneAddressMapping.fromPostalCode>(TaxZoneAddressMappings.Cache, e.Row, TxZone.Current.MappingType == MappingTypesAttribute.OneOrMorePostalCodes ? PXUIVisibility.Visible : PXUIVisibility.HiddenByAccessRights);
			PXUIFieldAttribute.SetVisibility<TaxZoneAddressMapping.toPostalCode>(TaxZoneAddressMappings.Cache, e.Row, TxZone.Current.MappingType == MappingTypesAttribute.OneOrMorePostalCodes ? PXUIVisibility.Visible : PXUIVisibility.HiddenByAccessRights);
		}

		protected virtual void TaxZone_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			TaxZone newrow = e.NewRow as TaxZone;
			if (newrow == null)
				return;

			TaxZone row = e.Row as TaxZone;
			if (row == null)
				return;

			if (newrow.IsManualVATZone == false && row.IsManualVATZone != false)
			{
				cache.SetValueExt<TaxZone.taxID>(newrow, null);
			}
		}

		protected virtual void _(Events.FieldUpdated<TaxZoneAddressMapping, TaxZoneAddressMapping.fromPostalCode> e)
		{
			if (TxZone.Cache.GetValueOriginal<TaxZone.countryID>(TxZone.Current) as string == string.Empty) return;

			string fromPostalCode = e.NewValue as string;

			if (string.IsNullOrEmpty(fromPostalCode) || string.IsNullOrEmpty(e.Row.ToPostalCode)
				|| TxZone?.Current?.MappingType != MappingTypesAttribute.OneOrMorePostalCodes) return;
			

			if (string.Compare(e.Row.ToPostalCode, fromPostalCode) < 0)
			{
				e.Cache.RaiseExceptionHandling<TaxZoneAddressMapping.toPostalCode>(e.Row, e.Row.ToPostalCode,
						new PXSetPropertyException(Messages.ToValueMustNotBeLess, PXErrorLevel.Error, fromPostalCode));
			}
		}

		protected virtual void _(Events.FieldVerifying<TaxZoneAddressMapping, TaxZoneAddressMapping.fromPostalCode> e)
		{
			if (TxZone.Cache.GetValueOriginal<TaxZone.countryID>(TxZone.Current) as string == string.Empty)
			{
				e.Cancel = true;
				return;
			}

			string fromPostalCode = e.NewValue as string;

			if (string.IsNullOrEmpty(fromPostalCode) || string.IsNullOrEmpty(e.Row.ToPostalCode) || e.OldValue == null
				|| TxZone?.Current?.MappingType != MappingTypesAttribute.OneOrMorePostalCodes && string.Compare(e.Row.ToPostalCode, fromPostalCode) < 0) return;

			TaxZoneAddressMapping item = FindOverlapingRecordByPostalCode(fromPostalCode, e.Row.ToPostalCode);

			if (item != null)
			{
				throw new PXSetPropertyException(Messages.PostalCodeIsAlreadyAssociated, PXErrorLevel.Error, item.FromPostalCode, item.ToPostalCode, item.TaxZoneID);
			}
		}

		protected virtual void _(Events.FieldVerifying<TaxZoneAddressMapping, TaxZoneAddressMapping.toPostalCode> e)
		{
			if (TxZone.Cache.GetValueOriginal<TaxZone.countryID>(TxZone.Current) as string == string.Empty)
			{
				e.Cancel = true;
				return;
			}

			string toPostalCode = e.NewValue as string;

			if (string.IsNullOrEmpty(toPostalCode) || string.IsNullOrEmpty(e.Row.FromPostalCode) || e.OldValue == null
				|| TxZone?.Current?.MappingType != MappingTypesAttribute.OneOrMorePostalCodes) return;

			if (string.Compare(toPostalCode, e.Row.FromPostalCode) < 0)
			{
				throw new PXSetPropertyException(Messages.ToValueMustNotBeLess, PXErrorLevel.Error, e.Row.FromPostalCode);
			}

			TaxZoneAddressMapping item = FindOverlapingRecordByPostalCode(e.Row.FromPostalCode, toPostalCode);

			if (item != null)
			{
				throw new PXSetPropertyException(Messages.PostalCodeIsAlreadyAssociated, PXErrorLevel.Error, item.FromPostalCode, item.ToPostalCode, item.TaxZoneID);
			}
		}

		protected virtual TaxZoneAddressMapping FindOverlapingRecordByPostalCode(string fromPostalCode, string toPostalCode)
		{
			TaxZone taxZone = TxZoneCurrent.Current;
			TaxZoneAddressMapping item = SelectFrom<TaxZoneAddressMapping>
										.Where<TaxZoneAddressMapping.taxZoneID.IsNotEqual<@P.AsString>
										.And<TaxZoneAddressMapping.countryID.IsEqual<@P.AsString>>
										.And<TaxZoneAddressMapping.stateID.IsEqual<@P.AsString>>
										.And<Not<Brackets<TaxZoneAddressMapping.fromPostalCode.IsGreater<@P.AsString>
											.Or<@P.AsString.IsGreater<TaxZoneAddressMapping.toPostalCodeSuffixed>>>>>>
										.View.Select(this, taxZone.TaxZoneID, taxZone.CountryID, string.Empty, string.Concat(toPostalCode, ToPostalCodeSuffix.ToPostalCodeSuffixCharacter), fromPostalCode);

			if (item == null)
			{
				item = TaxZoneAddressMappings.Select().RowCast<TaxZoneAddressMapping>()
					.Where(a => !string.IsNullOrEmpty(a.FromPostalCode) &&
							!string.IsNullOrEmpty(a.ToPostalCode) &&
							!(a.FromPostalCode.CompareTo(string.Concat(toPostalCode, ToPostalCodeSuffix.ToPostalCodeSuffixCharacter)) > 0 ||
							string.Concat(a.ToPostalCode, ToPostalCodeSuffix.ToPostalCodeSuffixCharacter).CompareTo(fromPostalCode) < 0))
					.FirstOrDefault();
				item = item != null && !TaxZoneAddressMappings.Current.Equals(item) ? item : null;
			}

			return item;
		}

		protected virtual void _(Events.FieldUpdating<TaxZone, TaxZone.mappingType> e)
		{
			TaxZone taxZone = e.Row;
			if (taxZone == null) return;

			PXResultset<TaxZoneAddressMapping> results = TaxZoneAddressMappings.Select();

			if (results.Count == 0 || TxZone.Ask(Common.Messages.Warning, Messages.ClearAddressMappingData, MessageButtons.YesNo) == WebDialogResult.Yes)
			{
				foreach (TaxZoneAddressMapping item in results)
				{
					TaxZoneAddressMappings.Cache.Delete(item);
				}

				TaxZoneAddressMappings.View.Clear();
				TaxZoneAddressMappings.View.RequestRefresh();

				if (e.NewValue as string == MappingTypesAttribute.OneOrMoreCountires)
				{
					e.Cache.SetValueExt<TaxZone.countryID>(taxZone, null);
				}
			}
			else
			{
				e.NewValue = e.OldValue;
			}
		}

		protected virtual void _(Events.FieldUpdating<TaxZone, TaxZone.countryID> e)
		{
			if (e?.Row == null || e.Cache.GetValueOriginal<TaxZone.countryID>(e.Row) as string == string.Empty) return;

			PXResultset<TaxZoneAddressMapping> results = TaxZoneAddressMappings.Select();

			if (results.Count == 0) return;

			if (TxZone.Ask(Common.Messages.Warning, Messages.ClearAddressMappingData, MessageButtons.YesNo) == WebDialogResult.Yes)
			{
				foreach (TaxZoneAddressMapping item in results)
				{
					TaxZoneAddressMappings.Cache.Delete(item);
				}

				TaxZoneAddressMappings.View.Clear();
				TaxZoneAddressMappings.View.RequestRefresh();
			}
			else
			{
				e.NewValue = e.OldValue;
			}
		}

		protected virtual void _(Events.FieldUpdated<TaxZone, TaxZone.countryID> e)
		{
			if (e.Row != null && !(e.Cache.GetValueOriginal<TaxZone.countryID>(e.Row) as string == string.Empty)) return;
			
			foreach (TaxZoneAddressMapping item in TaxZoneAddressMappings.Select())
			{
				TaxZoneAddressMapping newItem = (TaxZoneAddressMapping) TaxZoneAddressMappings.Cache.CreateInstance();
				newItem.CountryID = e.NewValue as string;
				newItem.TaxZoneID = item.TaxZoneID;
				newItem.StateID = item.StateID;
				newItem.FromPostalCode = item.FromPostalCode;
				newItem.ToPostalCode = item.ToPostalCode;

				TaxZoneAddressMapping inserted = null;
				try
				{
					inserted = TaxZoneAddressMappings.Cache.Insert(newItem) as TaxZoneAddressMapping;
				}
				finally
				{
					if (inserted != null) {
						TaxZoneAddressMappings.Cache.Delete(item);
					}
				}
			}
			TaxZoneAddressMappings.View.RequestRefresh();
		}

		protected virtual void _(Events.FieldVerifying<TaxZone, TaxZone.countryID> e)
		{
			TaxZone taxZone = e.Row;
			if (taxZone == null || string.IsNullOrWhiteSpace(e.NewValue as string)) return;

			Country country = PXSelectorAttribute.Select<TaxZone.countryID>(e.Cache, taxZone, e.NewValue) as Country;
			if (country != null) return;

			if (taxZone.MappingType != MappingTypesAttribute.OneOrMoreCountires)
				throw new PXSetPropertyException(Messages.CountryNotFound, e.NewValue);

			e.NewValue = null;
		}

		protected virtual void _(Events.FieldVerifying<TaxZoneAddressMapping, TaxZoneAddressMapping.countryID> e)
		{
			TaxZoneAddressMapping taxZoneMapping = e.Row;
			if (taxZoneMapping == null || string.IsNullOrWhiteSpace(e.NewValue as string)) return;
			
			Country country = PXSelectorAttribute.Select<TaxZoneAddressMapping.countryID>(e.Cache, taxZoneMapping, e.NewValue) as Country;
			if (country == null)
				throw new PXSetPropertyException(Messages.CountryNotFound, PXErrorLevel.Error, e.NewValue);

			if (taxZoneMapping == null || string.IsNullOrWhiteSpace(e.NewValue as string)) return;

			TaxZone taxZone = TxZoneCurrent.Current;
			if (taxZone?.MappingType == MappingTypesAttribute.OneOrMoreCountires)
			{
				TaxZoneAddressMapping item = FindDuplicateRecordByCountryAndState((string)e.NewValue, string.Empty);

				if (item != null)
				{
					throw new PXSetPropertyException(Messages.CountryIsAlreadyAssociated , PXErrorLevel.Error, item.TaxZoneID, item.CountryID);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<TaxZoneAddressMapping, TaxZoneAddressMapping.stateID> e)
		{
			TaxZoneAddressMapping taxZoneMapping = e.Row;
			if (taxZoneMapping == null || string.IsNullOrWhiteSpace(e.NewValue as string) || e.OldValue == null) return;

			TaxZone taxZone = TxZoneCurrent.Current;
			if (taxZone?.MappingType == MappingTypesAttribute.OneOrMoreStates)
			{
				TaxZoneAddressMapping item = FindDuplicateRecordByCountryAndState(taxZone.CountryID, (string) e.NewValue);

				if (item != null)
				{
					throw new PXSetPropertyException(Messages.StateIsAlreadyAssociated, PXErrorLevel.Error, item.TaxZoneID, item.StateID);
				}
			}
		}

		protected virtual TaxZoneAddressMapping FindDuplicateRecordByCountryAndState(string countryId, string stateId)
		{
			TaxZoneAddressMapping item = SelectFrom<TaxZoneAddressMapping>
													.Where<TaxZoneAddressMapping.taxZoneID.IsNotEqual<@P.AsString>
													.And<TaxZoneAddressMapping.countryID.IsEqual<@P.AsString>>
													.And<TaxZoneAddressMapping.stateID.IsEqual<@P.AsString>>
													.And<TaxZoneAddressMapping.fromPostalCode.IsEqual<@P.AsString>>
													.And<TaxZoneAddressMapping.toPostalCode.IsEqual<@P.AsString>>>
													.View.Select(this, TxZoneCurrent.Current.TaxZoneID, countryId, stateId, string.Empty, string.Empty);

			if (item == null)
			{
				item = TaxZoneAddressMappings.Select().RowCast<TaxZoneAddressMapping>().Where(a =>a.CountryID == countryId && a.StateID == stateId).FirstOrDefault();
			}

			return item;
		}

		protected virtual bool TryValidateTaxCategoryCombinationWithDirectTax(string[] taxIds, out PXResultset<TaxCategoryDet> invalidCategoryCombinations)
		{
			if (taxIds.Length < 1)
			{
				invalidCategoryCombinations = new PXResultset<TaxCategoryDet>();
				return true;
			}

			invalidCategoryCombinations = SelectFrom<TaxCategoryDet>
			.InnerJoin<TaxCategory>
			.On<TaxCategory.taxCategoryID.IsEqual<TaxCategoryDet.taxCategoryID>>
			.InnerJoin<Tax>
			.On<Tax.taxID.IsEqual<TaxCategoryDet.taxID>>
			.Where<TaxCategoryDet.taxID.IsIn<P.AsString>
				.And<TaxCategory.taxCatFlag.IsEqual<False>>
				.And<Tax.directTax.IsEqual<True>>>
			.AggregateTo<GroupBy<TaxCategoryDet.taxCategoryID>, Count<TaxCategoryDet.taxID>>
			.Having<TaxCategoryDet.taxID.Counted.IsGreater<decimal1>>
			.View.Select(this, new object[] { taxIds });

			return invalidCategoryCombinations.Count == 0;
		}
	}
}
