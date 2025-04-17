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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	public class WorkCodeMatchCountryAttribute : PMWorkCodeAttribute
	{
		public WorkCodeMatchCountryAttribute(Type countryIDField)
		{
			if (_SelAttrIndex != -1)
			{
				_Attributes.RemoveAt(_SelAttrIndex);
				_SelAttrIndex = -1;
			}

			Type search = BqlTemplate.OfCommand<SearchFor<PMWorkCode.workCodeID>
				.Where<PRxPMWorkCode.countryID.IsEqual<BqlPlaceholder.A.AsField.FromCurrent>>>
				.Replace<BqlPlaceholder.A>(countryIDField)
				.ToType();

			PXSelectorAttribute select = new PXSelectorAttribute(search, DescriptionField = typeof(PMWorkCode.description));
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}
}
