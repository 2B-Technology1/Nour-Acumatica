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
using PX.CS;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR.MassProcess;

namespace PX.Objects.CR.Extensions.Cache
{
	[PXInternalUseOnly]
	public static class CacheExtensions
	{
		public static IEnumerable<string> GetFields_ContactInfo(this PXCache cache)
		{
			return GetFields_WithAttribute<PXContactInfoFieldAttribute>(cache);
		}

		public static IEnumerable<string> GetFields_MassUpdatable(this PXCache cache)
		{
			return GetFields_WithAttribute<PXMassUpdatableFieldAttribute>(cache);
		}

		public static IEnumerable<string> GetFields_MassMergable(this PXCache cache)
		{
			return GetFields_WithAttribute<PXMassMergableFieldAttribute>(cache);
		}

		public static IEnumerable<string> GetFields_DeduplicationSearch(this PXCache cache)
		{
			return GetFields_WithAttribute<PXDeduplicationSearchFieldAttribute>(cache);
		}

		public static IEnumerable<string> GetFields_DeduplicationSearch(this PXCache cache, string validationType)
		{
			return GetFields_WithAttribute<PXDeduplicationSearchFieldAttribute>(cache, attribute => attribute.ValidationTypes.Any(type => type == validationType));
		}

		public static IEnumerable<string> GetFields_Udf(this PXCache cache)
		{
			return UDFHelper
				.GetUDFFields(cache.Graph)
				.Select(attr => KeyValueHelper.AttributePrefix + attr.AttributeID);
		}

		public static (string FieldName, TAttribute Attribute) GetField_WithAttribute<TAttribute>(this PXCache cache)
		{
			return cache
				.Fields
				.Where(field => cache
					.GetAttributesReadonly(field)
					.OfType<TAttribute>()
					.Any())
				.Select(_ => (_, cache
					.GetAttributesReadonly(_)
					.OfType<TAttribute>()
					.First()))
				.FirstOrDefault();
		}

		public static IEnumerable<string> GetFields_WithAttribute<TAttribute>(this PXCache cache)
		{
			return cache
				.Fields
				.Where(field => cache
					.GetAttributesReadonly(field)
					.OfType<TAttribute>()
					.Any());
		}

		public static IEnumerable<string> GetFields_WithAttribute<TAttribute>(this PXCache cache, Func<TAttribute, bool> predicate)
		{
			return cache
				.Fields
				.Where(field => cache
					.GetAttributesReadonly(field)
					.OfType<TAttribute>()
					.Any(predicate));
		}

		public static TAttribute GetFieldAttribute<TAttribute>(this PXCache cache, string field)
		{
			return cache
				.GetAttributesReadonly(field)
				.OfType<TAttribute>()
				.FirstOrDefault();
		}
	}
}
