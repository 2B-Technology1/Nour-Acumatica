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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

namespace PX.Objects.AM.GraphExtensions
{
	public class INSiteMaintAMExtension : PXGraphExtension<INSiteMaint>
    {
        public static bool IsActive()
        {
            return Features.ManufacturingOrDRPOrReplenishmentEnabled();
        }

		[PXCopyPasteHiddenView]
		[PXImport(typeof(INSite))]
		public SelectFrom<AMSiteTransfer>
			.Where<AMSiteTransfer.siteID.IsEqual<INSite.siteID.FromCurrent>>.View AMLeadTimes;

		protected virtual void INSite_AMScrapSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (INSite)e.Row;
            var rowExt = row.GetExtension<INSiteExt>();

            if (row == null)
            {
                return;
            }

            rowExt.AMScrapLocationID = null;
        }

		protected virtual void _(Events.FieldVerifying<AMSiteTransfer, AMSiteTransfer.transferSiteID> e)
		{			
			if((int?)e.NewValue == e.Row.SiteID)
			{
				throw new PXSetPropertyException(Messages.WarehouseCannotBeCurrent);
			}
		}

		public delegate void PersistDelegate();
		[PXOverride]
		public void Persist(PersistDelegate handler)
		{
			SyncItemSiteTransfer();
			SyncAMSiteTransfer();
			handler();
		}

		protected virtual void SyncItemSiteTransfer()
		{
			foreach(AMSiteTransfer siteTransfer in AMLeadTimes.Cache.Cached)
			{
				var rowStatus = AMLeadTimes.Cache.GetStatus(siteTransfer);
				if (rowStatus == PXEntryStatus.Notchanged)
					continue;

				foreach (INItemSite itemSite in SelectFrom<INItemSite>.Where<INItemSite.siteID.IsEqual<@P.AsInt>
					.And<INItemSite.replenishmentSourceSiteID.IsEqual<@P.AsInt>>
					.And<Brackets<INItemSiteExt.aMTransferLeadTimeOverride.IsEqual<False>
						.Or<INItemSiteExt.aMTransferLeadTimeOverride.IsNull>>>>.View.Select(Base, siteTransfer.SiteID, siteTransfer.TransferSiteID))
				{
					var itemSiteExt = itemSite.GetExtension<INItemSiteExt>();					
					if(rowStatus == PXEntryStatus.Inserted || rowStatus == PXEntryStatus.Updated)
					{
						itemSiteExt.AMTransferLeadTime = siteTransfer.TransferLeadTime;
					}
					else if (rowStatus == PXEntryStatus.Deleted)
					{
						itemSiteExt.AMTransferLeadTime = 0;
					}
					Base.itemsiterecords.Update(itemSite);
				}
			}
		}

		protected virtual void SyncAMSiteTransfer()
		{
			foreach (AMSiteTransfer siteTransfer in AMLeadTimes.Cache.Cached)
			{
				var rowStatus = AMLeadTimes.Cache.GetStatus(siteTransfer);
				if (rowStatus == PXEntryStatus.Notchanged)
					continue;

				AMSiteTransfer existingRec = AMSiteTransfer.PK.Find(Base, siteTransfer.TransferSiteID, siteTransfer.SiteID);
				
				if (rowStatus == PXEntryStatus.Inserted || rowStatus == PXEntryStatus.Updated)
				{
					if(existingRec == null)
					{
						AMLeadTimes.Cache.Insert(new AMSiteTransfer
						{
							SiteID = siteTransfer.TransferSiteID,
							TransferSiteID = siteTransfer.SiteID,
							TransferLeadTime = siteTransfer.TransferLeadTime
						});
					}
					else if(rowStatus == PXEntryStatus.Deleted)
					{
						existingRec = PXCache<AMSiteTransfer>.CreateCopy(existingRec);
						existingRec.TransferLeadTime = siteTransfer.TransferLeadTime;
						AMLeadTimes.Cache.Update(existingRec);
					}

				}
				else 
				{
					if(existingRec != null)
						AMLeadTimes.Cache.Delete(existingRec);
				}				
			}			
		}

		public override void Initialize()
		{
			AMLeadTimes.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && (PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>() || PXAccess.FeatureInstalled<FeaturesSet.distributionReqPlan>());
		}
	}
}
