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
using System.Linq;

using PX.Common;
using PX.Data;

namespace PX.Objects.Common
{
	public class LabelListAttribute : PXStringListAttribute
	{
		public LabelListAttribute(Type labelProviderType)
		{
			if (!typeof(ILabelProvider).IsAssignableFrom(labelProviderType))
			{
				throw new PXException(
					Messages.TypeMustImplementLabelProvider, 
					labelProviderType.Name);
			}

			try
			{
				ILabelProvider labelProvider =
					Activator.CreateInstance(labelProviderType) as ILabelProvider;

				List<string> values = new List<string>();
				List<string> labels = new List<string>();

				labelProvider.ValueLabelPairs.ForEach(pair =>
				{
					values.Add(pair.Value);
					labels.Add(pair.Label);
				});

				_AllowedValues = values.ToArray();
				_AllowedLabels = labels.ToArray();
				_NeutralAllowedLabels = _AllowedLabels;
			}
			catch (MissingMethodException exception)
			{
				throw new PXException(
					exception,
					Messages.LabelProviderMustHaveParameterlessConstructor,
					labelProviderType.Name);
			}
		}

		public LabelListAttribute(IEnumerable<ValueLabelPair> valueLabelPairs)
			: base(
				  valueLabelPairs.Select(pair => pair.Value).ToArray(),
				  valueLabelPairs.Select(pair => pair.Label).ToArray())
		{ }
	}
}
