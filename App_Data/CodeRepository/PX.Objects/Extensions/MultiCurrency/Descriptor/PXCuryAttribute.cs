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

using CommonServiceLocator;
using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.CM.Extensions
{
	/// <summary>
	/// Extends <see cref="PXDecimalAttribute"/> by defaulting the precision property.
	/// Precision is taken from given Currency.
	/// </summary>
	/// <remarks>This is a NON-DB attribute. Use it for calculated fields that are not storred in database.</remarks>
	public class PXCuryAttribute : PXDecimalAttribute
	{
		protected readonly Type sourceCuryID;

		protected Dictionary<long, string> _matches;

		public PXCuryAttribute(Type CuryIDType)
			: base(CurrencyCollection.CombiteSearchPrecision(CuryIDType))
		{
			sourceCuryID = CuryIDType;
		}

		public override void CacheAttached(PXCache sender)
		{
			sender.SetAltered(_FieldName, true);
			base.CacheAttached(sender);
		}

		protected override void _ensurePrecision(PXCache sender, object row)
		{
			_Precision = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(sender.Graph).CuryDecimalPlaces(GetCuryID(sender, row));
		}

		private string GetCuryID(PXCache sender, object row)
		{
			if (sourceCuryID.DeclaringType == null)
				return null;

			bool sameType = sourceCuryID.DeclaringType.IsAssignableFrom(sender.GetItemType());

			PXCache cache = sameType
				? sender
				: sender.Graph.Caches[sourceCuryID.DeclaringType];

			object source = sameType
				? row
				: cache.Current;

			return (string)cache.GetValue(source, sourceCuryID.Name);
		}
	}
}
