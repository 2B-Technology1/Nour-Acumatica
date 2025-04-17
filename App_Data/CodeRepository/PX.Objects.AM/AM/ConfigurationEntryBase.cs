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
using PX.Objects.CM;
using System.Collections;
using PX.Objects.AR;
using static PX.Objects.AM.ConfigurationEntry;
using PX.Objects.CR;
using PX.Objects.SO;
using System;

namespace PX.Objects.AM
{
	public class ConfigurationEntryBase : PXGraph<ConfigurationEntryBase>
	{
		public PXSave<AMConfigurationResults> Save;
		public PXCancel<AMConfigurationResults> Cancel;

		#region Views

		public ConfigurationSelect Results;

		public PXSelect<AMConfigurationResults,
			Where<AMConfigurationResults.configResultsID,
				Equal<Current<AMConfigurationResults.configResultsID>>>> CurrentResults;

		public PXSelectJoin<AMConfigResultsFeature,
			InnerJoin<AMConfigurationFeature,
				On<AMConfigResultsFeature.configurationID,
					Equal<AMConfigurationFeature.configurationID>,
					And<AMConfigResultsFeature.revision,
						Equal<AMConfigurationFeature.revision>,
						And<AMConfigResultsFeature.featureLineNbr,
							Equal<AMConfigurationFeature.lineNbr>>>>>,
			Where<AMConfigurationFeature.visible,
				Equal<True>,
				And<AMConfigResultsFeature.configResultsID,
					Equal<Current<AMConfigurationResults.configResultsID>>>>> CurrentFeatures;


		public PXSelectJoin<AMConfigResultsAttribute,
			InnerJoin<AMConfigurationAttribute,
				On<AMConfigResultsAttribute.configurationID,
					Equal<AMConfigurationAttribute.configurationID>,
					And<AMConfigResultsAttribute.revision,
						Equal<AMConfigurationAttribute.revision>,
						And<AMConfigResultsAttribute.attributeLineNbr,
							Equal<AMConfigurationAttribute.lineNbr>>>>>,
			Where<AMConfigResultsAttribute.configResultsID,
				Equal<Current<AMConfigurationResults.configResultsID>>,
				And<AMConfigResultsAttribute.visible, Equal<True>>>,
			OrderBy<Asc<AMConfigResultsAttribute.configurationID,
				Asc<AMConfigResultsAttribute.revision,
				Asc<AMConfigurationAttribute.sortOrder,
				Asc<AMConfigResultsAttribute.attributeLineNbr>>>>>> Attributes;

		public PXSelectJoin<AMConfigResultsOption,
			InnerJoin<AMConfigurationOption,
				On<AMConfigResultsOption.configurationID,
					Equal<AMConfigurationOption.configurationID>,
					And<AMConfigResultsOption.revision,
						Equal<AMConfigurationOption.revision>,
						And<AMConfigResultsOption.featureLineNbr,
							Equal<AMConfigurationOption.configFeatureLineNbr>,
							And<AMConfigResultsOption.optionLineNbr,
								Equal<AMConfigurationOption.lineNbr>>>>>>,
			Where<AMConfigResultsOption.configResultsID,
				Equal<Current<AMConfigResultsFeature.configResultsID>>,
				And<AMConfigResultsOption.featureLineNbr,
					Equal<Optional<AMConfigResultsFeature.featureLineNbr>>,
					And<Where2<
						Where<Current<SelectOptionsFilter.showAll>,
							Equal<True>>,
						Or<AMConfigResultsOption.available,
							Equal<True>>>>>>,
			OrderBy<Asc<AMConfigResultsOption.configurationID,
				Asc<AMConfigResultsOption.revision,
				Asc<AMConfigResultsOption.featureLineNbr,
				Asc<AMConfigurationOption.sortOrder,
				Asc<AMConfigResultsOption.optionLineNbr>>>>>>> Options;

		public PXSelect<AMConfigResultsOption,
	Where<AMConfigResultsOption.configResultsID,
		Equal<Optional<AMConfigResultsOption.configResultsID>>,
		And<AMConfigResultsOption.featureLineNbr,
			Equal<Optional<AMConfigResultsOption.featureLineNbr>>,
			And<AMConfigResultsOption.optionLineNbr,
				Equal<Optional<AMConfigResultsOption.optionLineNbr>>>>>> CurrentOption;

		public PXFilter<ConfigEntryFilter> ConfigFilter;

		#region Select Option SmartPanel

		public PXFilter<SelectOptionsFilter> OptionsSelectFilter;

		#endregion

		#region Multi-Currency

		public ToggleCurrency<AMConfigurationResults> CurrencyView;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<AMConfigurationResults.curyInfoID>>>> currencyinfo;

		#endregion

		#region External Refs


		public PXSelect<AMProdItem,
			Where<AMProdItem.orderType,
				Equal<Current<AMConfigurationResults.prodOrderType>>,
				And<AMProdItem.prodOrdID,
					Equal<Current<AMConfigurationResults.prodOrderNbr>>>>> ProdItemRef;
		#endregion

		#endregion

		/// <summary>
		/// Is the processing initializing a configuration
		/// </summary>
		internal bool IsInitConfiguration;


		//Avoid prompt to save when in test mode
		public override bool IsDirty
		{
			get
			{
				if (Results?.Current?.IsConfigurationTesting == true)
				{
					return false;
				}

				return base.IsDirty;
			}
		}

		#region Actions

		public PXAction<AMConfigurationResults> Finish;
		[PXUIField(DisplayName = Messages.Finish)]
		[PXButton(CommitChanges = true)]
		protected virtual IEnumerable finish(PXAdapter a)
		{
			AMConfigurationResults results = Results.Current;
			if (results == null
				|| results.Closed.GetValueOrDefault())
			{
				return a.Get();
			}

			if (results.Completed != true)
			{
				string errorMessage;
				bool documentValid = Results.IsDocumentValid(out errorMessage);

				if (!documentValid)
				{
					throw new PXException(errorMessage);
				}

				results.Completed = true;
			}
			else
			{
				// Remove Supplemental line items 
				ConfigSupplementalItemsHelper.RemoveSupplementalLineItems(this, results);

				results.Completed = false;
			}

			results = Results.Update(results);
			if (results != null && results.IsConfigurationTesting != true)
			{
				var retSave = Save.Press(a);

				if (ConfigFilter?.Current?.ShowCanTestPersist == true)
				{
					// implemented this way to get over a refresh issue when saving a test configuration
					return retSave;
				}

				AMProdItem prodItem = ProdItemRef.Select();
				if (prodItem != null && prodItem.Released == true)
				{
					throw new PXException(Messages.ProductionAlreadyPlanned);
				}
				if (prodItem != null)
				{
					var prodGraph = CreateInstance<ProdMaint>();

					prodItem.BuildProductionBom = true;
					prodGraph.ProdMaintRecords.Current = prodGraph.ProdMaintRecords.Update(prodItem);
					prodGraph.ItemConfiguration.Current = prodGraph.ItemConfiguration.Select();
					// Need to pass in the current results so later the correct value for Completed is found
					prodGraph.ItemConfiguration.Current = results;
					prodGraph.Actions.PressSave();
				}

				return retSave;
			}

			return a.Get();
		}

		public PXAction<AMConfigurationResults> ShowAll;
		[PXUIField(DisplayName = "Show All")]
		[PXButton]
		protected virtual void showAll()
		{
			OptionsSelectFilter.Current.ShowAll = !OptionsSelectFilter.Current.ShowAll.GetValueOrDefault();
		}

		#endregion

		#region Handlers


		protected virtual void _(Events.RowSelected<AMConfigResultsAttribute> e)
		{
			var sender = e.Cache;
			var row = (AMConfigResultsAttribute)e.Row;
			if (row == null || IsInitConfiguration)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<AMConfigResultsAttribute.value>(sender, row, row.Enabled == true);
			sender.RaiseExceptionHandling<AMConfigResultsAttribute.value>(row, row.Value, RuleSummary.GetException(this, row, this.Results, CurrentResults?.Current?.IsConfigurationTesting == true));
		}

		protected virtual void _(Events.FieldUpdated<AMConfigResultsAttribute.value> e)
		{
			Options.View.Clear();
			Attributes.View.RequestRefresh();
		}
		
		protected virtual void _(Events.FieldUpdated<AMConfigResultsOption.included> e)
		{
			Options.View.RequestRefresh();
		}

		protected virtual void _(Events.RowSelected<SelectOptionsFilter> e)
		{
			var row = (SelectOptionsFilter)e.Row;
			if (row == null) return;

			PXUIFieldAttribute.SetVisible<AMConfigResultsOption.available>(Options.Cache, null, row.ShowAll.GetValueOrDefault());
		}

		protected virtual void _(Events.RowSelected<AMConfigResultsOption> e)
		{
			var sender = e.Cache;
			var row = (AMConfigResultsOption)e.Row;
			if (row == null || IsInitConfiguration)
			{
				return;
			}

			// Only shown if Show All is checked
			PXUIFieldAttribute.SetEnabled<AMConfigResultsOption.selected>(sender, row, row.Available == true && row.Included != true);

			var option = (AMConfigurationOption)PXSelect<AMConfigurationOption,
				Where<AMConfigurationOption.configurationID,
					Equal<Current<AMConfigResultsFeature.configurationID>>,
					And<AMConfigurationOption.revision,
						Equal<Current<AMConfigResultsFeature.revision>>,
						And<AMConfigurationOption.configFeatureLineNbr,
							Equal<Current<AMConfigResultsFeature.featureLineNbr>>,
							And<AMConfigurationOption.lineNbr,
								Equal<Required<AMConfigurationOption.lineNbr>>>>>>>.Select(this, row.OptionLineNbr);
			if (option != null)
			{
				PXUIFieldAttribute.SetEnabled<AMConfigResultsOption.qty>(sender, row, option.QtyEnabled == true);
			}

			PXUIFieldAttribute.SetEnabled<AMConfigResultsOption.included>(sender, row, !row.FixedInclude.GetValueOrDefault());

			//We are doing it in row selected instead of field verifying because the value of row.Qty could be calculated
			//and we want to warn the user if it's not respecting the rules without removing the qty. He could then adjust 
			//the attributes so it fits the rules. 
			sender.RaiseExceptionHandling<AMConfigResultsOption.qty>(row, row.Qty, RuleSummary.GetException(this, row, this.Results, CurrentResults.Current.IsConfigurationTesting == true));
		}

		protected virtual void _(Events.RowSelected<CurrencyInfo> e)
		{
			var sender = e.Cache;
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Results.Cache);
				Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<AMConfigurationResults.customerID>>>>.Select(this);
				if (customer != null && !(bool)customer.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}

		#endregion



		// Copy over from ConfigurationSelect - UpdateSalesOrderWithConfiguredLineChanges
		protected virtual SOOrderEntry UpdateSalesOrderWithConfiguredLineChanges(AMConfigurationResults configResults)
		{
			if (configResults == null)
			{
				return null;
			}

			var soLine = (SOLine)PXSelect<SOLine,
				Where<SOLine.orderType, Equal<Required<AMConfigurationResults.ordTypeRef>>,
					And<SOLine.orderNbr, Equal<Required<AMConfigurationResults.ordNbrRef>>,
						And<SOLine.lineNbr, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
			>.Select(this, configResults.OrdTypeRef, configResults.OrdNbrRef, configResults.OrdLineRef);

			if (soLine?.OrderNbr == null)
			{
				return null;
			}

			var soOrderEntryGraph = CreateInstance<SOOrderEntry>();
			soOrderEntryGraph.RecalculateExternalTaxesSync = true;
			soOrderEntryGraph.Document.Current = soOrderEntryGraph.Document.Search<SOOrder.orderNbr>(soLine.OrderNbr, soLine.OrderType);
			if (soOrderEntryGraph.Document?.Current == null)
			{
				return null;
			}

			//Need to set the config into cache for correct results when query later
			soOrderEntryGraph.Caches<AMConfigurationResults>().Update(PXCache<AMConfigurationResults>.CreateCopy(configResults));
			soOrderEntryGraph.Caches<AMConfigurationResults>().SetStatus(configResults, PXEntryStatus.Notchanged);

			ConfigurationSelect.UpdateSalesOrderWithConfiguredLineChanges(soOrderEntryGraph, soLine, configResults, ConfigSupplementalItemsHelper.GetSupplementalOptions(this, configResults), false, false);

			return soOrderEntryGraph;
		}

		// Copy over from ConfigurationSelect - UpdateOpportunityWithConfiguredLineChanges
		protected virtual PXGraph UpdateOpportunityWithConfiguredLineChanges(AMConfigurationResults configResults)
		{
			if (configResults == null)
			{
				throw new PXArgumentException(nameof(configResults));
			}

			var result = (PXResult<CROpportunityProducts, CRQuote>)PXSelectJoin<CROpportunityProducts,
				LeftJoin<CRQuote, On<CRQuote.quoteID, Equal<CROpportunityProducts.quoteID>>>,
				Where<CROpportunityProducts.quoteID, Equal<Required<CROpportunityProducts.quoteID>>,
					And<CROpportunityProducts.lineNbr, Equal<Required<CROpportunityProducts.lineNbr>>>>
			>.SelectWindowed(this, 0, 1, configResults.OpportunityQuoteID, configResults.OpportunityLineNbr);

			var product = (CROpportunityProducts)result;
			if (product?.QuoteID == null)
			{
				return null;
			}

			var quote = (CRQuote)result;
			if (string.IsNullOrWhiteSpace(quote?.QuoteNbr))
			{
				//return ConfigurationSelect.UpdateOpportunityWithConfiguredLineChangesInt(product, configResults);
				var opportunityGraph = PXGraph.CreateInstance<OpportunityMaint>();
				opportunityGraph.Opportunity.Current = opportunityGraph.Opportunity.Search<CROpportunity.quoteNoteID>(configResults.OpportunityQuoteID);

				//Need to set the config into cache for correct results when query later
				opportunityGraph.Caches<AMConfigurationResults>().Update(PXCache<AMConfigurationResults>.CreateCopy(configResults));
				opportunityGraph.Caches<AMConfigurationResults>().SetStatus(configResults, PXEntryStatus.Notchanged);

				ConfigurationSelect.UpdateOpportunityWithConfiguredLineChangesInt(opportunityGraph, product, configResults, ConfigSupplementalItemsHelper.GetSupplementalOptions(this, configResults), false);
				return opportunityGraph;
			}

			if (quote.IsDisabled.GetValueOrDefault())
			{
				return null;
			}

			var quoteGraph = PXGraph.CreateInstance<QuoteMaint>();
			quoteGraph.Quote.Current = quote;

			//Need to set the config into cache for correct results when query later
			quoteGraph.Caches<AMConfigurationResults>().Update(PXCache<AMConfigurationResults>.CreateCopy(configResults));
			quoteGraph.Caches<AMConfigurationResults>().SetStatus(configResults, PXEntryStatus.Notchanged);

			ConfigurationSelect.UpdateQuoteWithConfiguredLineChanges(quoteGraph, product, configResults, ConfigSupplementalItemsHelper.GetSupplementalOptions(this, configResults), false);

			return quoteGraph;
		}

		#region Unbound Dacs

		[Serializable]
		[PXHidden]
		public class SelectOptionsFilter : IBqlTable
		{
			#region ShowAll
			public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = Messages.ShowAll)]
			public virtual Boolean? ShowAll { get; set; }
			#endregion
		}


		[Serializable]
		[PXHidden]
		public class ConfigEntryFilter : IBqlTable
		{
			#region CanTestPersist
			public abstract class canTestPersist : PX.Data.BQL.BqlBool.Field<canTestPersist> { }
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Save Test Results")]
			public virtual Boolean? ShowCanTestPersist { get; set; }
			#endregion
		}
		#endregion


	}
}
