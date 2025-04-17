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
using System.Linq;

namespace PX.Objects.CM.Extensions
{
	public class SlaveCuryIDAttribute : PXAggregateAttribute, IPXRowPersistingSubscriber
	{
		private readonly Type SourceField;

		public SlaveCuryIDAttribute(Type sourceField)
		{
			SourceField = sourceField;
			_Attributes.Add(new PXDBStringAttribute(5) { IsUnicode = true, InputMask = ">LLLLL" });
			_Attributes.Add(new PXUIFieldAttribute { DisplayName = "Currency" });
			_Attributes.Add(new PXSelectorAttribute(typeof(Currency.curyID)));
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			long? newkey = CurrencyInfoAttribute.GetPersistedCuryInfoID(sender, (long?)sender.GetValue(e.Row, SourceField.Name));
			CurrencyInfo currencyInfo = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, newkey);
			if (currencyInfo?.CuryID != null)
				sender.SetValue(e.Row, FieldName, currencyInfo.CuryID);
		}
	}
}
