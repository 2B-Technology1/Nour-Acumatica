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
using System.Collections;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions
{
	public abstract class SkipNullTextWhenImportLines<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, PXImportAttribute.IPXPrepareItems
	{
		protected virtual string NullText { get => PXMessages.LocalizeNoPrefix(IN.Messages.EmptyLS); }

		protected abstract PXSelectBase LinesView { get; }

		protected abstract IEnumerable<Type> FieldsWithNullText();

		/// Overrides <see cref="PXImportAttribute.IPXPrepareItems.PrepareImportRow(string, IDictionary, IDictionary)" />
		[PXOverride]
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values, Func<string, IDictionary, IDictionary, bool> baseImpl)
		{
			if (viewName.Equals(LinesView.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				foreach (var field in FieldsWithNullText())
					ClearNullText(values, field);
			}

			return baseImpl(viewName, keys, values);
		}

		protected virtual bool ClearNullText(IDictionary values, Type field)
		{
			var fieldName = LinesView.Cache.GetField(field);
			if (values.Contains(fieldName))
			{
				var fieldValue = values[fieldName] as string;
				fieldValue = fieldValue?.Trim();

				if (!string.IsNullOrEmpty(fieldValue) && fieldValue.Equals(NullText, StringComparison.InvariantCultureIgnoreCase))
				{
					values[fieldName] = null;
					return true;
				}
			}
			return false;
		}
	}
}
