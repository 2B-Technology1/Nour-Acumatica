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
using System;

namespace PX.Objects.IN.Matrix.Attributes
{
	public class TemplateUnitAttribute : INUnitAttribute
	{
		public TemplateUnitAttribute(Type inventoryID, Type templateID)
			: base(VerifyingMode.InventoryUnitConversion)
		{
			InventoryType = inventoryID;

			var searchType = BqlTemplate.OfCommand<
				SearchFor<INUnit.fromUnit>
				.Where<INUnit.unitType.IsEqual<INUnitType.inventoryItem>
					.And<INUnit.inventoryID.IsEqual<BqlPlaceholder.A.AsField.AsOptional.NoDefault>
						.Or<BqlPlaceholder.A.AsField.AsOptional.NoDefault.IsNull.
							And<INUnit.inventoryID.IsEqual<BqlPlaceholder.B.AsField.AsOptional.NoDefault>>>>>>
				.Replace<BqlPlaceholder.A>(inventoryID)
				.Replace<BqlPlaceholder.B>(templateID).ToType();

			Init(searchType, searchType);
		}
	}
}
