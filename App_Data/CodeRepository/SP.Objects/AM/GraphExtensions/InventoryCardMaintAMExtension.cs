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
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using SP.Objects.IN;
using PX.Objects.AM;
using PX.Objects.AM.Attributes;

namespace SP.Objects.AM.GraphExtensions
{
    /// <summary>
    /// Manufacturing extension to "My Cart" (SP700001)
    /// </summary>
    [Serializable]
    public class InventoryCardMaintAMExtension : PXGraphExtension<InventoryCardMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturingProductConfigurator>();
        }

        [PXHidden]
        [PXCopyPasteHiddenView]
        public PortalConfigurationSelect<
            Where<AMConfigurationResults.createdByID, Equal<Current<PortalCardLines.userID>>,
                And<AMConfigurationResults.inventoryID, Equal<Current<PortalCardLines.inventoryID>>,
                    And<AMConfigurationResults.siteID, Equal<Current<PortalCardLines.siteID>>,
                        And<AMConfigurationResults.uOM, Equal<Current<PortalCardLines.uOM>>,
                            And<AMConfigurationResults.ordNbrRef, IsNull,
                                And<Current<AMConfigurationResults.opportunityQuoteID>, IsNull>>>>>>> ItemConfiguration;


        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXParent(typeof(Select<PortalCardLines, 
            Where<PortalCardLines.userID, Equal<Current<AMConfigurationResults.createdByID>>,
                And<PortalCardLines.inventoryID, Equal<Current<AMConfigurationResults.inventoryID>>,
                    And<PortalCardLines.siteID, Equal<Current<AMConfigurationResults.siteID>>,
                        And<PortalCardLines.uOM, Equal<Current<AMConfigurationResults.uOM>>,
                            And<Current<AMConfigurationResults.ordNbrRef>, IsNull,
                            And<Current<AMConfigurationResults.opportunityQuoteID>, IsNull>>>>>>>))]
        protected virtual void AMConfigurationResults_SiteID_CacheAttached(PXCache sender)
        {
        }

        #region Proceed to Checkout - Button

        public PXAction<PortalCardLine> ProceedToCheckOut;
        /// <summary>
        /// Override to InventoryCardMaint.proceedToCheckOut
        /// </summary>
        [PXUIField(DisplayName = "Proceed to Checkout")]
        [PXButton]
        public virtual IEnumerable proceedToCheckOut(PXAdapter adapter)
        {
            string configFinishedMessages;
            if (!ConfiguraitonsFinished(out configFinishedMessages))
            {
                throw new PXException(configFinishedMessages);
            }
            return Base.proceedToCheckOut(adapter);
        }

        #endregion

        /// <summary>
        /// Check for the existing configured lines for configuations not complete
        /// </summary>
        protected virtual bool ConfiguraitonsFinished(out string message)
        {
            var sb = new System.Text.StringBuilder();
            foreach (PXResult<PortalCardLines, InventoryItem, INSite, AMConfigurationResults> result in PXSelectJoin<PortalCardLines,
                InnerJoin<InventoryItem, 
                    On<PortalCardLines.inventoryID, Equal<InventoryItem.inventoryID>>,
                InnerJoin<INSite,
                        On<PortalCardLines.siteID, Equal<INSite.siteID>>,
                InnerJoin<AMConfigurationResults, 
                    On<PortalCardLines.userID, Equal<AMConfigurationResults.createdByID>,
                        And<PortalCardLines.inventoryID, Equal<AMConfigurationResults.inventoryID>,
                        And<PortalCardLines.siteID, Equal<AMConfigurationResults.siteID>,
                        And<PortalCardLines.uOM, Equal<AMConfigurationResults.uOM>>>>>>>>, 
                Where<PortalCardLines.userID, Equal<Required<PortalCardLines.userID>>,
                    And<AMConfigurationResults.completed, Equal<False>,
                    And<AMConfigurationResults.ordNbrRef, IsNull,
                    And<AMConfigurationResults.opportunityQuoteID, IsNull>>>>>.Select(Base, PXAccess.GetUserID()))
            {
#if DEBUG
                sb.Append($"[{((AMConfigurationResults) result).ConfigResultsID}] ");
#endif
                if (PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
                {
                    sb.AppendLine(PX.Objects.AM.Messages.GetLocal(PX.Objects.AM.Messages.ConfiguraitonIncompleteSitePortal,
                        ((InventoryItem)result).InventoryCD.TrimIfNotNullEmpty(),
                        UomHelper.FormatQty(((PortalCardLines)result).Qty.GetValueOrDefault()),
                        ((PortalCardLines)result).UOM.TrimIfNotNullEmpty(),
                        ((INSite)result).SiteCD.TrimIfNotNullEmpty()));
                }
                else
                {
                    sb.AppendLine(PX.Objects.AM.Messages.GetLocal(PX.Objects.AM.Messages.ConfiguraitonIncompletePortal,
                        ((InventoryItem)result).InventoryCD.TrimIfNotNullEmpty(),
                        UomHelper.FormatQty(((PortalCardLines)result).Qty.GetValueOrDefault()),
                        ((PortalCardLines)result).UOM.TrimIfNotNullEmpty()));
                }
            }

            message = sb.ToString();
            return string.IsNullOrWhiteSpace(message);
        }

        public PXAction<PortalCardLine> ConfigureEntry;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel, Tooltip = "Launch configuration entry")]
        [PXUIField(DisplayName = PX.Objects.AM.Messages.Configure, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        public virtual void configureEntry()
        {
            if (Base.DocumentDetails.Current == null)
            {
                return;
            }

            var ext = PXCache<PortalCardLines>.GetExtension<PortalCardLinesExt>(Base.DocumentDetails.Current);
            if (!ext.AMIsConfigurable.GetValueOrDefault())
            {
                throw new PXException(PX.Objects.AM.Messages.NotConfigurableItem);
            }

            Base.Actions.PressSave();
            AMConfigurationResults configuration = ItemConfiguration.SelectWindowed(0, 1);
            if (configuration != null)
            {
                var configGraph = PXGraph.CreateInstance<ConfigurationEntry>();
                configGraph.Results.Current =
                    configGraph.Results.Search<AMConfigurationResults.configResultsID>(configuration.ConfigResultsID);

                PXRedirectHelper.TryRedirect(configGraph, PXRedirectHelper.WindowMode.Popup);
            }
        }

        [PXOverride]
        public virtual decimal CalculatePriceCard(PortalCardLines row, Func<PortalCardLines, decimal> del)
        {
            if (row == null)
            {
                return del?.Invoke(row) ?? 0m;
            }

            var rowExt = PXCache<PortalCardLines>.GetExtension<PortalCardLinesExt>(row);
            if (rowExt != null && rowExt.AMIsConfigurable.GetValueOrDefault())
            {
                // For configured line items only...
                var configResult = PortalConfigurationSelect.GetConfigurationResult(Base, row);
                if (configResult != null)
                {
                    return AMConfigurationPriceAttribute.GetPriceExt<AMConfigurationResults.displayPrice>(ItemConfiguration.Cache, configResult, ConfigCuryType.Document).GetValueOrDefault() + configResult.CurySupplementalPriceTotal.GetValueOrDefault();
                }
            }
            
            // All non configured line items...
            return del?.Invoke(row) ?? 0m;
        }
    }
}
