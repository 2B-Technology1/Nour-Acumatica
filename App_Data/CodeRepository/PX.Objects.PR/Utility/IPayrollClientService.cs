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

using PX.CS.Contracts.Interfaces;
using PX.Objects.CR;
using PX.Payroll.Data;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public interface IPayrollClientTaxService
	{
		IEnumerable<PRPayrollCalculation> Calculate(IEnumerable<PRPayroll> payrolls);
		PRLocationCodeDescription GetLocationCode(IAddressBase address);
		Dictionary<int?, PRLocationCodeDescription> GetLocationCodes(IEnumerable<Address> addresses);
		IEnumerable<Payroll.Data.PRTaxType> GetTaxTypes(string taxLocationCode, string taxMunicipalCode, string taxSchoolCode, bool includeRailroadTaxes);
		IEnumerable<Payroll.Data.PRTaxType> GetAllLocationTaxTypes(IEnumerable<Address> addresses, bool includeRailroadTaxes);
		IEnumerable<Payroll.Data.PRTaxType> GetSpecificTaxTypes(string typeName, string locationSearch);
		Dictionary<string, SymmetryToAatrixTaxMapping> GetAatrixTaxMapping(IEnumerable<Payroll.Data.PRTaxType> uniqueTaxes);
		byte[] GetTaxMappingFile();
	}
}
