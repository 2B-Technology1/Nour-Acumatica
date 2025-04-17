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
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.CR
{
	/// <summary>
	/// The attribute used in FK of <see cref="SalesTerritory.salesTerritoryID"/> fields.
	/// </summary>
	[PXDBString(15, IsUnicode = true)]
	[PXUIField(DisplayName = "Sales Territory ID", FieldClass = FeaturesSet.salesTerritoryManagement.FieldClass)]
	[PXSelector(typeof(Search2<
				SalesTerritory.salesTerritoryID,
			LeftJoin<Country,
				On<Country.countryID, Equal<SalesTerritory.countryID>>>>),
		fieldList: new []
		{
			typeof(SalesTerritory.salesTerritoryID),
			typeof(SalesTerritory.name),
			typeof(SalesTerritory.salesTerritoryType),
			typeof(Country.countryID),
			typeof(Country.description)
		},
		DescriptionField = typeof(SalesTerritory.name))]
	[PXRestrictor(typeof(Where<SalesTerritory.isActive, Equal<True>>), CS.MessagesNoPrefix.SalesTerritoryInactive, ShowWarning = true)]
	public class SalesTerritoryFieldAttribute : AcctSubAttribute { }
}
