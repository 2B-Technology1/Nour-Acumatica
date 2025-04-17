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

using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Web.UI;
using System;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.SO;

namespace PX.Objects.AM
{
    public class ConfigurationEntry : ConfigurationEntryBase
    {
		public PXSaveClose<AMConfigurationResults> SaveClose;

		#region Views

		public PXSelect<AMConfigTreeNode,
            Where<AMConfigTreeNode.lineNbr,
                Equal<Argument<int?>>,
                And<AMConfigTreeNode.optionLineNbr,
                    Equal<Argument<int?>>>>,
            OrderBy<Asc<AMConfigTreeNode.sortOrder>>> Features;
        public virtual IEnumerable features([PXInt]int? lineNbr, [PXInt]int? optionLineNbr)
        {
            if (lineNbr == null)
            {
                // Get features related to current config.
                var results = CurrentFeatures.Select();

                // Set the icon based on completed status.
                foreach (PXResult<AMConfigResultsFeature, AMConfigurationFeature> feature in results)
                {
                    var resultFeature = (AMConfigResultsFeature)feature;
                    var configurationFeature = (AMConfigurationFeature)feature;

                    var item = new AMConfigTreeNode();
                    item.LineNbr = configurationFeature.LineNbr;
                    item.Label = configurationFeature.Label;
                    item.SortOrder = configurationFeature.SortOrder;

                    string errorMessage;
                    var valid = Results.IsFeatureOptionValid(resultFeature, out errorMessage, false);
                    if (valid)
                    {
                        item.ToolTip = string.Empty;
                        item.Icon = Sprite.Main.GetFullUrl(Sprite.Main.Success);
                    }
                    else
                    {
                        item.ToolTip = errorMessage;
                        item.Icon = Sprite.Main.GetFullUrl(Sprite.Main.Fail);
                    }

                    yield return item;
                }
            }
            else if (optionLineNbr == null)
            {
                // Feature LineNbr is set, but not option. This is a feature node. Get related Options.
                var options = OptionsTree.Select(lineNbr);

                var result = new PXResultset<AMConfigTreeNode>();
                foreach (PXResult<AMConfigResultsOption, AMConfigurationOption> option in options)
                {
                    var resultOption = (AMConfigResultsOption)option;
                    var configOption = (AMConfigurationOption)option;
                    AMConfigTreeNode item = new AMConfigTreeNode();
                    item.LineNbr = resultOption.FeatureLineNbr;
                    item.OptionLineNbr = resultOption.OptionLineNbr;
                    item.Label = configOption.Label;

                    string errorMessage;
                    var valid = Results.ValidateOption(resultOption, out errorMessage);
                    if (valid)
                    {
                        item.ToolTip = string.Empty;
                        item.Icon = Sprite.Control.GetFullUrl(Sprite.Control.Info);
                    }
                    else
                    {
                        item.ToolTip = errorMessage;
                        item.Icon = Sprite.Control.GetFullUrl(Sprite.Control.Error);
                    }

                    yield return item;
                }
            }
        }

        public PXSelect<AMConfigResultsFeature,
            Where<AMConfigResultsFeature.configResultsID,
                Equal<Current<AMConfigurationResults.configResultsID>>,
                And<AMConfigResultsFeature.featureLineNbr,
                    Equal<Argument<int?>>>>> CurrentFeature;
        public virtual IEnumerable currentFeature([PXInt]int? lineNbr)
        {
            return PXSelect<AMConfigResultsFeature,
                Where<AMConfigResultsFeature.configResultsID,
                    Equal<Current<AMConfigurationResults.configResultsID>>,
                    And<AMConfigResultsFeature.featureLineNbr,
                        Equal<Required<AMConfigResultsFeature.featureLineNbr>>>>>.Select(this, lineNbr);
        }

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
                        Where<AMConfigResultsOption.included,
                            Equal<True>>,
                        Or<AMConfigResultsOption.ruleValid,
                            Equal<False>>>>>>> OptionsTree;

		#endregion

		public ConfigurationEntry()
		{
			Results.AllowDelete =

				Options.AllowInsert =
					Options.AllowDelete =

						Attributes.AllowInsert =
							Attributes.AllowDelete = false;
		}

		#region Handlers

		protected override void _(Events.FieldUpdated<AMConfigResultsOption.included> e)
		{
			OptionsTree.View.Clear();
			base._(e);
		}

		protected virtual void _(Events.RowSelected<AMConfigurationResults> e)
		{
			var sender = e.Cache;
			var row = (AMConfigurationResults)e.Row;
			if (row == null)
			{
				return;
			}

			AMProdItem prodItem = ProdItemRef.Select();
			var canFinish = prodItem == null || prodItem.Released == false;

			// All header records not allowed for update/insert in UI
			PXUIFieldAttribute.SetEnabled(sender, row, false);

			var isTestConfig = row.IsConfigurationTesting.GetValueOrDefault() ||
							   ConfigFilter?.Current?.ShowCanTestPersist == true;
			if (isTestConfig)
			{
				PXUIFieldAttribute.SetEnabled<AMConfigurationResults.customerID>(sender, row, true);
				PXUIFieldAttribute.SetVisible<AMConfigurationResults.customerID>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<AMConfigurationResults.isConfigurationTesting>(sender, row, true);
				PXUIFieldAttribute.SetVisible<AMConfigurationResults.isConfigurationTesting>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<AMConfigurationResults.siteID>(sender, row, true);
				PXUIFieldAttribute.SetVisible<AMConfigurationResults.siteID>(sender, row, true);
			}

			PXUIFieldAttribute.SetVisible<AMConfigResultsOption.inventoryID>(Options.Cache, null, isTestConfig);
			PXUIFieldAttribute.SetVisible<AMConfigResultsOption.subItemID>(Options.Cache, null, isTestConfig);

			Options.AllowUpdate =
				Results.AllowUpdate =
					Attributes.AllowUpdate = !row.Closed.GetValueOrDefault() && !row.Completed.GetValueOrDefault() && canFinish;

			Finish.SetEnabled(!row.Closed.GetValueOrDefault() && canFinish);
			Finish.SetCaption(row.Completed != true ? Messages.Finish : Messages.Unfinish);

			Save.SetEnabled(!row.IsConfigurationTesting.GetValueOrDefault());
			// There is no way to bring the configuration back when testing so disable
			Cancel.SetEnabled(!isTestConfig);
			SaveClose.SetCaption(!isTestConfig ? Messages.SaveAndClose : Messages.CloseTesting);
			ShowAll.SetCaption(OptionsSelectFilter.Current.ShowAll.GetValueOrDefault() ? PX.Objects.CA.Messages.HideTran : Messages.ShowAll);
		}

		#endregion

		public override void Persist()
		{
			var currentConfig = CurrentResults?.Current;
			if (currentConfig == null || currentConfig.IsConfigurationTesting == true)
			{
				return;
			}

			var status = CurrentResults.Cache.GetStatus(currentConfig);
			if (currentConfig.Completed.GetValueOrDefault() && (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated))
			{
				if (currentConfig?.IsSalesReferenced == true)
				{
					var soGraph = UpdateSalesOrderWithConfiguredLineChanges(currentConfig);
					if (soGraph != null && soGraph.IsDirty)
					{
						using (var ts = new PXTransactionScope())
						{
							soGraph.Actions.PressSave();
							base.Persist();
							ts.Complete();
						}
						return;
					}
				}

				if (currentConfig?.IsOpportunityReferenced == true)
				{
					var crGraph = UpdateOpportunityWithConfiguredLineChanges(currentConfig);
					if (crGraph != null && crGraph.IsDirty)
					{
						using (var ts = new PXTransactionScope())
						{
							// Using persist over Actions.PressSave because that call will fail when using External Tax Providers
							crGraph.Persist();
							base.Persist();
							ts.Complete();
						}

						return;
					}
				}
			}

			base.Persist();
		}

	}
}
