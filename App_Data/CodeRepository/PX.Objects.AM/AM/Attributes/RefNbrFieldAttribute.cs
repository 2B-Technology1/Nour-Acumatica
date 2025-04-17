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
using PX.Objects.Common;

namespace PX.Objects.AM.Attributes
{
    [PXDBString(30, IsUnicode = true)]
    [PXUIField(DisplayName = "Ref Nbr")]
    public class RefNbrFieldAttribute : PXEntityAttribute
    {
        protected Type RefNoteIDField;

        public RefNbrFieldAttribute(Type refNoteIDField) : base()
        {
            RefNoteIDField = refNoteIDField;
        }

        public static string FormatFieldNbr(params string[] vals)
        {
            return vals == null ? null : string.Join(", ", vals);
        }

        public static string GetKeyString(PXGraph graph, Guid? refNoteId)
        {
            if (refNoteId == null)
            {
                return null;
            }

            var helper = new EntityHelper(graph);
            var entity = helper.GetEntityRow(refNoteId);
            if (entity == null)
            {
                return null;
            }

            var keys = helper.GetEntityRowKeys(entity.GetType(), entity);
            if (keys == null || keys.Length == 0)
            {
                return null;
            }

            return string.Join(", ", keys);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            PXButtonDelegate del = delegate (PXAdapter adapter)
            {
                PXCache cache = adapter.View.Graph.Caches[sender.GetItemType()];
                if (cache.Current != null)
                {
                    object val = cache.GetValueExt(cache.Current, RefNoteIDField.Name);

                    EntityHelper helper = new EntityHelper(sender.Graph);
                    var state = val as PXRefNoteBaseAttribute.PXLinkState;
                    if (state != null)
                    {
                        helper.NavigateToRow(state.target.FullName, state.keys, PXRedirectHelper.WindowMode.NewWindow);
                    }
                    else
                    {
                        helper.NavigateToRow((Guid?)cache.GetValue(cache.Current, RefNoteIDField.Name), PXRedirectHelper.WindowMode.NewWindow);
                    }
                }

                return adapter.Get();
            };

            string ActionName = sender.GetItemType().Name + "$" + _FieldName + "$Link";
            sender.Graph.Actions[ActionName] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(sender.GetItemType()), new object[] { sender.Graph, ActionName, del, new PXEventSubscriberAttribute[] { new PXUIFieldAttribute { MapEnableRights = PXCacheRights.Select } } });
        }
    }
}
