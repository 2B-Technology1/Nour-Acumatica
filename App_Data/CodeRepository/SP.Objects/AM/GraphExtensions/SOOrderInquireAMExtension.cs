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
using SP.Objects.IN;
using PX.Objects.SO;
using PX.Objects.IN;
using PX.Objects.AM;

namespace SP.Objects.AM.GraphExtensions
{
	/// <summary>
	/// Manufacturing extension for My Orders (SP700003) - SP.Objects.IN.SOOrderInquire
	/// </summary>
	[Serializable]
	public class SOOrderInquireAMExtension : PortalCardLinesConfigurationBase<SOOrderInquire>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturingProductConfigurator>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.ordTypeRef> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.ordNbrRef> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.ordLineRef> e) { }

		public delegate PortalCardLines InsertOrderLineToCartDelegate(SOLine soLine, InventoryItem inventoryItem);

		[PXOverride]
		public virtual PortalCardLines InsertOrderLineToCart(SOLine soLine, InventoryItem inventoryItem, InsertOrderLineToCartDelegate del)
		{
			var newLine = del?.Invoke(soLine, inventoryItem);
			var newLineExt = PXCache<PortalCardLines>.GetExtension<PortalCardLinesExt>(newLine);
			if(newLineExt?.AMIsConfigurable == true)
			{
				CopyConfiguration(newLine, PortalConfigurationSelect.GetConfigurationResult(Base, newLine), soLine);
			}

			return newLine;
		}

		protected override void InsertConfigurationResult(PXCache sender, PortalCardLines row)
		{
			InsertConfigurationResult(sender, row, Base.GetCurrencyInfo(), Base.currentCustomer.BAccountID, Base.currentCustomer.DefLocationID);
		}

		public delegate bool PersistInsertedPortalCardLinesDelegate(PortalCardLines portalCardLines);

		[PXOverride]
		public virtual bool PersistInsertedPortalCardLines(PortalCardLines portalCardLines, PersistInsertedPortalCardLinesDelegate del)
		{
			if(!ItemConfiguration.IsDirty)
			{
				return del?.Invoke(portalCardLines) == true;
			}

			var ret = false;
			using(var ts = new PXTransactionScope())
			{
				ret = del?.Invoke(portalCardLines) == true;
				if(ret)
				{
					PersistConfigurations();
				}
				ts.Complete();
			}

			SetPersistedConfigurations();
			ItemConfiguration.ClearConfigurationCache();

			return ret;
		}

		protected virtual void CopyConfiguration(PortalCardLines portalCardLine, AMConfigurationResults portalCardConfigResult, SOLine soLine)
		{
			var salesConfiguration = (AMConfigurationResults)PXSelect<AMConfigurationResults,
				Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
				And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>,
				And<AMConfigurationResults.ordLineRef, Equal<Required<AMConfigurationResults.ordLineRef>>>>>>
				.Select(Base, soLine?.OrderType, soLine?.OrderNbr, soLine?.LineNbr);

			if(portalCardConfigResult?.ConfigResultsID == null || salesConfiguration?.ConfigResultsID == null)
			{
				return;
			}

			ConfigurationCopyEngine.UpdateConfigurationFromConfiguration(Base, portalCardConfigResult, salesConfiguration, true, true);
		}
	}
}
