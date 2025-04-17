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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.PJ.Common.Descriptor.Attributes
{
    /// <summary>
    /// Attribute used for clearing values associated with <see cref="PMTask"/> when deleting a <see cref="PMTask"/>.
    /// Unlike <see cref="PXParentAttribute"/> not require DataView of the child entity in related Graph.
    /// Should be set on <see cref="PMTask"/> properties as use <see cref="PMTask.taskID"/> property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ClearRelatedEntitiesAttribute : PXEventSubscriberAttribute, IPXRowPersistedSubscriber
    {
        private readonly Type[] entityFieldTypes;

        public ClearRelatedEntitiesAttribute(params Type[] entityFieldTypes)
        {
            this.entityFieldTypes = entityFieldTypes;
        }

        public void RowPersisted(PXCache cache, PXRowPersistedEventArgs args)
        {
	        if (args.TranStatus == PXTranStatus.Open
				&& args.Row is PMTask task && cache.Deleted.ToArray<PMTask>().Contains(task))
            {
                entityFieldTypes.ForEach(type => ClearEntity(task.TaskID, cache, type));
            }
        }

        private static void ClearEntity(int? taskId, PXCache cache, Type entityFieldType)
        {
            var view = GetView(cache, entityFieldType);
            var entities = view.SelectMulti(taskId);
            entities.ForEach(x => UpdateEntity(x, view.Cache, entityFieldType));
            view.Cache.Persist(PXDBOperation.Update);
        }

        private static void UpdateEntity(object entity, PXCache cache, Type entityFieldType)
        {
            cache.SetValue(entity, entityFieldType.Name, null);
            cache.Update(entity);
        }

        private static PXView GetView(PXCache cache, Type entityFieldType)
        {
            var command = BqlCommand.CreateInstance(typeof(Select<,>), entityFieldType.DeclaringType,
                typeof(Where<,>), entityFieldType, typeof(Equal<>), typeof(Required<>), entityFieldType);
            return new PXView(cache.Graph, false, command);
        }
    }
}
