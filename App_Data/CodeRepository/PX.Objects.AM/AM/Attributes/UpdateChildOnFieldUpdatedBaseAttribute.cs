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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Auto update child row field on change of parent field attached to this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class UpdateChildOnFieldUpdatedBaseAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        protected Type _ChildType;
        protected Type _childUpdateField;
        protected PXGraph _Graph;

        /// <param name="childType">Type of child row</param>
        /// <param name="childUpdateField">Field in child formula used to trigger formula</param>
        protected UpdateChildOnFieldUpdatedBaseAttribute(Type childType, Type childUpdateField)
        {
            _ChildType = childType;
            _childUpdateField = childUpdateField;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            _Graph = sender.Graph;
        }

        protected virtual PXCache ChildCache => _Graph?.Caches[_ChildType];

        protected virtual object[] SelectChildren(object parentRow)
        {
            var childCache = ChildCache;
            return childCache == null || parentRow == null ? null : PXParentAttribute.SelectChildren(ChildCache, parentRow, _BqlTable) ?? new object[0];
        }

        protected virtual object GetChildValue(object childRow)
        {
            return ChildCache?.GetValue(childRow, _childUpdateField.Name);
        }

        protected virtual void SetChildFieldValue(object childRow, object newValue)
        {
            ChildCache?.SetValueExt(childRow, _childUpdateField.Name, newValue);
        }

        protected virtual object UpdateChildRow(object childRow)
        {
            return ChildCache?.Update(childRow);
        }

        protected virtual void UpdateAllChildRows(object parentRow, object newValue)
        {
            Common.Cache.AddCacheView(_Graph, _ChildType);
            foreach (var child in SelectChildren(parentRow))
            {
                SetChildFieldValue(child, newValue);
                UpdateChildRow(child);
            }
        }

        public abstract void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e);
    }
}