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
using PX.Data.BQL.Fluent;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;
using System.Collections;
using System.Linq;

namespace PX.Objects.Localizations.CA.MISCT5018Vendor
{
	public class Attributes
	{
		public class T5018TaxYearSelector : PXCustomSelectorAttribute
		{
			private const int MAX_HISTORIC_YEAR = 4;

			public T5018TaxYearSelector(Type type) :
				base(type)
			{ }

			public virtual IEnumerable GetRecords()
			{
				T5018MasterTable current = _Graph.Caches[nameof(T5018MasterTable)].Current as T5018MasterTable;

				if (current == null || current.OrganizationID == null) yield break;

				FinYear currentYear =
					_Graph.Select<FinYear>().
					Where(
						FY =>
						FY.OrganizationID == current.OrganizationID &&
						FY.StartDate <= _Graph.Accessinfo.BusinessDate.Value &&
						FY.EndDate > _Graph.Accessinfo.BusinessDate.Value
					).SingleOrDefault() ??
					SelectFrom<FinYear>.
					Where<FinYear.organizationID.IsEqual<FinYear.organizationID.AsOptional>>.
					OrderBy<FinYear.endDate.Desc>.View.Select(_Graph, current.OrganizationID).TopFirst;

				if (currentYear == null) yield break;				

				T5018MasterTable earliestRevision = SelectFrom<T5018MasterTable>.
					Where<T5018MasterTable.organizationID.IsEqual<T5018MasterTable.organizationID.AsOptional>>.
					OrderBy<T5018MasterTable.toDate.Asc>.View.ReadOnly.Select(_Graph, current.OrganizationID).TopFirst;

				if (earliestRevision == null)
				{
					foreach (FinYear finYear in
						_Graph.Select<FinYear>().
						Where(
							FY =>
							FY.OrganizationID == current.OrganizationID &&
							FY.EndDate <= currentYear.EndDate
						).OrderByDescending(
							FY => FY.Year
						).Take(MAX_HISTORIC_YEAR + 1))
					{
						yield return finYear;
					}						
				}
				else
				{
					foreach (FinYear finYear in
						_Graph.Select<FinYear>().
						Where(
							FY =>
							FY.OrganizationID == current.OrganizationID &&
							FY.EndDate >= (Int32.Parse(earliestRevision.Year) < (Int32.Parse(currentYear.Year) - MAX_HISTORIC_YEAR) ? earliestRevision.ToDate : currentYear.EndDate.Value.AddYears(-MAX_HISTORIC_YEAR)) &&
							FY.EndDate <= currentYear.EndDate
						))
					{
						yield return finYear;
					}
				}
			}
		}

	}
}
