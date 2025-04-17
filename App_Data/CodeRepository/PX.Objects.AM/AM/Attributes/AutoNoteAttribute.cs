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
using System.Collections.Generic;
using PX.Data;
using PX.Objects.Common;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// A variation of <see cref="PXNoteAttribute"/> to auto create a <see cref="Note"/> record.
    /// Standard <see cref="PXNoteAttribute"/> does not provide a way to auto create the Note record (did back in version 4.X).
    /// </summary>
    public class AutoNoteAttribute : PXNoteAttribute, IPXRowInsertedSubscriber
    {
        /// <summary>
        /// Controls if the NoteID value will be a lookup before insert.
        /// When True there is no check.
        /// </summary>
        public bool SkipNoteRecordCheck;

        public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (SkipNoteRecordCheck)
            {
                EnsureNewNoteID(sender, e.Row, null);
                return;
            }

            EnsureNewNoteID(sender, e.Row, null);
        }

        /// <summary>
        /// Copy of EnsureNoteID but remove the SelectSingle for performance.
        /// We are using this to avoid the NoteID lookup and just insert.
        /// Use only when inserting new DAC object and as a result a new noteid
        /// </summary>
        protected Guid EnsureNewNoteID(PXCache sender, object row, string externalKey)
        {
            Guid? id = (Guid?)sender.GetValue(row, _FieldOrdinal);

            PXView view = GetView(sender.Graph);
            //if (id.HasValue && null != view.SelectSingle(id.Value)) // when note already exists
            //    return id.Value;

            Note note = new Note
            {
                NoteID = id,
                NoteText = string.Empty,
                EntityType = GetEntityType(sender, id),
                GraphType = GetGraphType(sender.Graph),
                ExternalKey = externalKey
            };

            note = (Note)view.Cache.Insert(note);
            if (!id.HasValue)
            {
                id = note.NoteID;
                sender.SetValue(row, _FieldOrdinal, id);
            }

            //Status of row on insert is "NotChanged", working around it
            if (sender.Locate(row) != null)
            {
                sender.Graph.EnsureRowPersistence(row);
            }
            sender.IsDirty = true;
            return id.Value;
        }
    }

    /// <summary>
    /// Assist in the change of value of SkipNoteRecordCheck during runtime
    /// </summary>
    public class AutoNoteSkipNoteRecordCheckScope : OverrideAttributePropertyScope<AutoNoteAttribute, bool>
    {
        public AutoNoteSkipNoteRecordCheckScope(PXCache cache, bool ensureNewNoteIDOnlyValue, params System.Type[] fields)
            : base(cache, (IEnumerable<System.Type>)fields, (System.Action<AutoNoteAttribute, bool>)((attribute, ensureNewNoteIDOnly) => attribute.SkipNoteRecordCheck = ensureNewNoteIDOnly), (Func<AutoNoteAttribute, bool>)(attribute => attribute.SkipNoteRecordCheck), ensureNewNoteIDOnlyValue)
        {
        }
    }
}