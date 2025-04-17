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
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;
using PX.Objects.AM.GraphExtensions;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Default the mark for field value based on <see cref="InventoryItem"/> settings for <see cref="InventoryItemExt.AMDefaultMarkFor"/>
    /// (Use the attribute on the Mark for Field)
    /// </summary>
    public class DefaultMarkForAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
    {
        protected int MarkForType;
        protected string InventoryFieldName = "InventoryID";
        protected object ValueIfMarkForMatched = true;
        protected object ValueIfMarkForNotMatched = false;

        public DefaultMarkForAttribute(int markForType)
        {
            MarkForType = markForType;
        }

        public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var inventoryId = GetInventoryId(sender, e.Row);
            if (inventoryId == null)
            {
                e.NewValue = ValueIfMarkForNotMatched;
                return;
            }

            var markForType = GetMarkForType(sender, e.Row, inventoryId);
            if (markForType == null || MarkForType != markForType)
            {
                e.NewValue = ValueIfMarkForNotMatched;
                return;
            }

            e.NewValue = ValueIfMarkForMatched;
        }

        protected virtual int? GetInventoryId(PXCache cache, object data)
        {
            return (int?)cache.GetValue(data, InventoryFieldName);
        }

        protected virtual int? GetMarkForType(PXCache cache, object data, int? inventoryId)
        {
            return GetInventoryItem(cache, data, inventoryId)?.GetExtension<InventoryItemExt>()?.AMDefaultMarkFor;
        }

        protected virtual InventoryItem GetInventoryItem(PXCache cache, object data, int? inventoryId)
        {
            return (InventoryItem)PXSelectorAttribute.Select(cache, data, InventoryFieldName)
                ?? InventoryItem.PK.Find(cache.Graph, inventoryId);
        }
    }
}
