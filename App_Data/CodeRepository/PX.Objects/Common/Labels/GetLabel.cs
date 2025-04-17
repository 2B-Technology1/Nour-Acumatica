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

using PX.Data;

namespace PX.Objects.Common
{
	public static class GetLabel
	{
		/// <summary>
		/// For the specified BQL constant declared inside an <see cref="ILabelProvider"/>-implementing
		/// class, fetches its value and returns the corresponding label.
		/// </summary>
		/// <typeparam name="TConstant">
		/// A BQL constant type declared inside an <see cref="ILabelProvider"/>-implementing class.
		/// </typeparam>
		/// <example><code>
		/// string value = GetLabel.For&lt;ARDocType.refund&gt;();
		/// </code></example>
		public static string For<TConstant>() where TConstant : IConstant<string>, IBqlOperand, new()
		{
			Type constantType = typeof(TConstant);
			Type encompassingType = constantType.DeclaringType;

			if (encompassingType == null ||
				!typeof(ILabelProvider).IsAssignableFrom(encompassingType))
			{
				throw new PXException(
					Messages.ConstantMustBeDeclaredInsideLabelProvider,
					constantType.Name);
			}

			try
			{
				ILabelProvider labelProvider =
					Activator.CreateInstance(encompassingType) as ILabelProvider;

				return labelProvider.GetLabel(
					new TConstant().Value as string);
			}
			catch (MissingMethodException exception)
			{
				throw new PXException(
					exception,
					Messages.LabelProviderMustHaveParameterlessConstructor,
					encompassingType.Name);
			}
		}

		/// <summary>
		/// For the specified string value, returns the corresponding label as defined by 
		/// the specified <see cref="ILabelProvider"/>-implementing class.
		/// </summary>
		/// <typeparam name="TLabelProvider">The type of the label provider class, e.g. <see cref="ARDocType"/>.</typeparam>
		/// <param name="value">The string value for which the label should be obtained.</param>
		/// <example><code>
		/// string value = GetLabel.For&lt;ARDocType&gt;("INV"); // returns "Invoice"
		/// </code></example>
		public static string For<TLabelProvider>(string value) where TLabelProvider : ILabelProvider, new()
			=> new TLabelProvider().GetLabel(value);

		/// <summary>
		/// For the specified BQL field, returns the label defined for the field value 
		/// by an item-level instance of <see cref="PXStringListAttribute"/> residing 
		/// on the field.
		/// </summary>
		/// <remarks>
		/// This method can be used when the string list is changed dynamically
		/// for individual records.
		/// </remarks>
		public static string For<TField>(PXCache cache, IBqlTable record) where TField : IBqlField
		{
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (record == null) throw new ArgumentNullException(nameof(record));

			string fieldValue = cache.GetValue<TField>(record) as string;

			if (fieldValue == null)
			{
				throw new PXException(Messages.FieldIsNotOfStringType, typeof(TField).FullName);
			}

			PXStringListAttribute stringListAttribute = cache
				.GetAttributesReadonly<TField>(record)
				.OfType<PXStringListAttribute>()
				.SingleOrDefault();

			if (stringListAttribute == null)
			{
				throw new PXException(
					Messages.FieldDoesNotHaveItemOrCacheLevelAttribute, 
					typeof(TField).FullName,
					nameof(PXStringListAttribute));
			}

			string label;

			if (!stringListAttribute.ValueLabelDic.TryGetValue(fieldValue, out label))
			{
				throw new PXException(
					Messages.StringListAttributeDoesNotDefineLabelForValue, 
					fieldValue);
			}

			return label;
		}

		/// <summary>
		/// For the specified BQL field and its value, returns the label defined for that 
		/// value by a cache-level instance of <see cref="PXStringListAttribute"/>
		/// residing on the field.
		/// </summary>
		/// <remarks>
		/// This method can be used when the string list is changed dynamically
		/// at the cache level.
		/// </remarks>
		public static string For<TField>(PXCache cache, string fieldValue) where TField : IBqlField
		{
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));

			PXStringListAttribute stringListAttribute = cache
				.GetAttributesReadonly<TField>()
				.OfType<PXStringListAttribute>()
				.SingleOrDefault();

			if (stringListAttribute == null)
			{
				throw new PXException(
					Messages.FieldDoesNotHaveCacheLevelAttribute,
					typeof(TField).FullName,
					nameof(PXStringListAttribute));
			}

			string label;

			if (!stringListAttribute.ValueLabelDic.TryGetValue(fieldValue, out label))
			{
				throw new PXException(
					Messages.StringListAttributeDoesNotDefineLabelForValue,
					fieldValue);
			}

			return label;
		}
	}
}
