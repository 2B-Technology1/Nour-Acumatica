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

using PX.Common;
using PX.Data;
using System;

namespace PX.Objects.Common.Attributes
{
	/// <exclude/>
	public class PXDBRestrictionBoolAttribute : PXBoolAttribute, IPXRowPersistedSubscriber
	{
		protected string _RelatedDatabaseFieldName;
		protected Type _RelatedBqlField;

		public PXDBRestrictionBoolAttribute(Type relatedBqlField)
		{
			RelatedBqlField = relatedBqlField;
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);

			bool insert = (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert;
			bool update = (e.Operation & PXDBOperation.Command) == PXDBOperation.Update;

			if (insert)
			{
				e.ExcludeFromInsertUpdate();
			}
			else if (update)
			{
				e.DataType = PXDbType.Bit;
				e.DataLength = 1;
				e.BqlTable = _BqlTable;
				var table = e.Table == null ? _BqlTable : e.Table;
				e.Expr = new PX.Data.SQLTree.Column(_RelatedDatabaseFieldName, new PX.Data.SQLTree.SimpleTable(table), e.DataType);
				e.IsRestriction = true;
				e.Value = e.DataValue = sender.GetValue(e.Row, _FieldName) ?? sender.GetValue(e.Row, RelatedBqlField.Name);
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Row != null && e.TranStatus.IsIn(PXTranStatus.Completed, PXTranStatus.Aborted))
				sender.SetValue(e.Row, _FieldName, null);
		}

		protected virtual Type RelatedBqlField
		{
			get => _RelatedBqlField;
			set
			{
				_RelatedBqlField = value;

				_RelatedDatabaseFieldName = char.ToUpper(value.Name[0]) + value.Name.Substring(1);

				if (value.IsNested)
				{
					if (value.DeclaringType.IsDefined(typeof(PXTableAttribute), true))
					{
						BqlTable = value.DeclaringType;
					}
					else
					{
						BqlTable = BqlCommand.GetItemType(value);
					}
				}
			}
		}
	}
}
