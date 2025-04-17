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
using PX.Objects.EP;

namespace PX.Objects.PM.GraphExtensions
{
	public class EmployeeMaintExt : PXGraphExtension<EmployeeMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		protected virtual void _(Events.RowSelected<EPEmployee> e)
		{
			ValidateEmployee(e.Cache, e.Row);
		}

		protected virtual void _(Events.RowUpdated<EPEmployee> e)
		{
			ValidateEmployee(e.Cache, e.Row);
		}

		protected virtual void ValidateEmployee(PXCache cache, EPEmployee employee)
		{
			if (employee == null)
				return;

			bool isCuryRateTypeRequired = employee.AllowOverrideCury == true;

			PXDefaultAttribute.SetPersistingCheck<EPEmployee.curyRateTypeID>(cache, employee, isCuryRateTypeRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}
	}
}
