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
using System;

namespace PX.Objects.AM.Attributes
{
    public class AMRowStatusEventAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowUpdatedSubscriber, IPXRowDeletingSubscriber
    {
        protected Type MasterParentType;
        protected PXGraph Graph;

        public AMRowStatusEventAttribute(Type masterParentType)
        {
            MasterParentType = masterParentType;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            Graph = sender.Graph;
        }

        protected bool IsCurrentMasterParentDeleted => Graph?.Caches[MasterParentType]?.IsCurrentRowDeleted() == true;

        protected int? GetRowStatusValue(PXCache cache, object row)
        {
            return row == null ? null : (int?)cache.GetValue(row, FieldName);
        }

        protected void SetRowStatusValue(PXCache cache, object row, int? value)
        {
            if (row == null)
            {
                return;
            }

            cache.SetValue(row, FieldName, value);
        }

        public void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            var rowStatus = GetRowStatusValue(cache, e.Row);
            if (rowStatus == null || rowStatus != AMRowStatus.Unchanged || cache.IsRowInserted(e.Row))
            {
                return;
            }

            SetRowStatusValue(cache, e.Row, AMRowStatus.Updated);
        }

        public void RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (IsCurrentMasterParentDeleted || cache.GetStatus(e.Row) == PXEntryStatus.InsertedDeleted)
            {
                return;
            }

            var rowStatus = GetRowStatusValue(cache, e.Row);
            if (rowStatus == null || rowStatus == AMRowStatus.Inserted)
            {
                return;
            }

            e.Cancel = true;
            SetRowStatusValue(cache, e.Row, AMRowStatus.Deleted);
            cache.SetStatus(e.Row, PXEntryStatus.Updated);
            // Setting the cache as updated was not enough to enable the save button
            cache.IsDirty = true;
        }

        public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMRowStatus.Inserted;
        }
    }
}