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

namespace PX.Objects.PR
{
	public class EnsureParentPXFormulaAttribute : PXFormulaAttribute
	{
		private Type[] _ParentFields;
		private Type[] _ChildFields;

		public EnsureParentPXFormulaAttribute(Type formula, Type aggregate, Type[] parentFields, Type[] childFields) : base(formula, aggregate) 
		{
			_ParentFields = parentFields;
			_ChildFields = childFields;
		}

		protected override object EnsureParent(PXCache cache, object Row, object NewRow)
		{
			object parent = base.EnsureParent(cache, Row, NewRow);
			if (parent != null)
			{
				return parent;
			}
			
			if (_ParentFields.Length != _ChildFields.Length)
			{
				return null;
			}

			PXCache parentCache = cache.Graph.Caches[BqlCommand.GetItemType(_ParentFieldType)];
			parent = parentCache.CreateInstance();
			for (int i = 0; i < _ParentFields.Length; i++)
			{
				parentCache.SetValue(parent, _ParentFields[i].Name, cache.GetValue(Row, _ChildFields[i].Name));
			}

			return parentCache.Insert(parent);
		}
	}
}
