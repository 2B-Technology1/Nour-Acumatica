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
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Data.PXGenericInqGrph;
using static PX.SM.EMailAccount;

namespace PX.Objects.AM
{
	public class ConfigurationEntryForAPI : ConfigurationEntryBase
	{
		public PXInsert<AMConfigurationResults> Insert;
		public PXDelete<AMConfigurationResults> Delete;

		#region Events

		[PXUIField(DisplayName = "Config Results ID", Visible = true, Enabled = true)]
		[PXSelector(typeof(Search<AMConfigurationResults.configResultsID>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.configResultsID> e)
		{ }

		[Inventory(Enabled = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.inventoryID> e)
		{ }

		[PXUIField(DisplayName = "Configuration ID", Enabled = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.configurationID> e)
		{ }

		[PXUIField(DisplayName = "Conf. Revision", Visible = true, Enabled = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.revision> e)
		{ }

		[PXUIField(DisplayName = "Opportunity Quote ID", Enabled = true, Visible = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.opportunityQuoteID> e)
		{ }

		[PXUIField(DisplayName = "Opportunity Line Nbr", Enabled = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.opportunityLineNbr> e)
		{ }

		[PXUIField(DisplayName = "Prod Order Type", Visible = true, Enabled = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.prodOrderType> e)
		{ }

		[PXUIField(DisplayName = "Prod Order Nbr", Visible = true, Enabled = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.prodOrderNbr> e)
		{ }

		[PXUIField(DisplayName = "Test Configuration", Visible = true, Enabled = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.isConfigurationTesting> e)
		{ }

		[Site(Enabled = true, Visible = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.siteID> e)
		{ }

		[LocationActive(typeof(Where<Location.bAccountID, Equal<Optional<AMConfigurationResults.customerID>>,
			And<MatchWithBranch<Location.cBranchID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, Enabled = true, Visible = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.customerLocationID> e)
		{ }

		[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Enabled = true, Visible = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<AMConfigurationResults.customerID> e)
		{ }

		protected virtual void _(Events.RowSelected<AMConfigurationResults> e)
		{
			//disable opportunity fields if SO is referenced
			PXUIFieldAttribute.SetEnabled<AMConfigurationResults.opportunityQuoteID>(e.Cache, e.Row, e.Row?.IsSalesReferenced != true && e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<AMConfigurationResults.opportunityLineNbr>(e.Cache, e.Row, e.Row?.IsSalesReferenced != true && e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);

			//disable SO fields if opportunity is referenced
			PXUIFieldAttribute.SetEnabled<AMConfigurationResults.ordTypeRef>(e.Cache, e.Row, e.Row?.IsOpportunityReferenced != true && e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<AMConfigurationResults.ordNbrRef>(e.Cache, e.Row, e.Row?.IsOpportunityReferenced != true && e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<AMConfigurationResults.ordLineRef>(e.Cache, e.Row, e.Row?.IsOpportunityReferenced != true && e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);

			//production reference fields always disabled
			PXUIFieldAttribute.SetEnabled<AMConfigurationResults.prodOrderType>(e.Cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<AMConfigurationResults.prodOrderNbr>(e.Cache, e.Row, false);

			//disable customer and location if any order is referenced
			PXUIFieldAttribute.SetEnabled<AMConfigurationResults.customerID>(e.Cache, e.Row, e.Row?.IsSalesReferenced != true && e.Row?.IsOpportunityReferenced != true && e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<AMConfigurationResults.customerLocationID>(e.Cache, e.Row, e.Row?.IsSalesReferenced != true && e.Row?.IsOpportunityReferenced != true && e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);

		}

		protected virtual void _(Events.FieldUpdated<AMConfigurationResults.ordNbrRef> e)
		{
			var row = (AMConfigurationResults)e.Row;
			if (row == null)
				return;

			var order = SOOrder.PK.Find(this, row.OrdTypeRef, row.OrdNbrRef);
			if(order != null)
			{
				row.CuryInfoID = order.CuryInfoID;
				row.CustomerID = order.CustomerID;
				row.CustomerLocationID = order.CustomerLocationID;
			}
		}

		protected virtual void _(Events.FieldUpdated<AMConfigurationResults.opportunityQuoteID> e)
		{
			var row = (AMConfigurationResults)e.Row;
			if (row == null)
				return;

			var order = CROpportunity.PK.Find(this, row.OpportunityQuoteID.ToString());
			if (order != null)
			{
				row.CuryInfoID = order.CuryInfoID;
				row.CustomerID = order.BAccountID;
				row.CustomerLocationID = order.LocationID;
			}
		}

		protected virtual void _(Events.FieldUpdated<AMConfigurationResults.inventoryID> e)
		{
			var row = (AMConfigurationResults)e.Row;
			if (row == null)
				return;

			if(Results.TryGetDefaultConfigurationID(row.InventoryID, out string configID))
			{
				row.ConfigurationID = configID;
				e.Cache.SetDefaultExt<AMConfigurationDefault.revision>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<AMConfigurationResults.siteID> e)
		{
			var row = (AMConfigurationResults)e.Row;
			if (row == null)
				return;

			if (Results.TryGetDefaultConfigurationID(row.InventoryID, row.SiteID, out string configID))
			{
				row.ConfigurationID = configID;
				e.Cache.SetDefaultExt<AMConfigurationDefault.revision>(e.Row);
			}
		}

		#endregion

		public ConfigurationEntryForAPI()
		{
			Results.AllowInsert = Results.AllowUpdate = Results.AllowSelect = Results.AllowDelete = true;
			Attributes.AllowUpdate = true;
			CurrentFeatures.AllowUpdate = true;
		}
	}
}
