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
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Payroll.Data;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class PRAcaDedBenCodeMaint : PXGraphExtension<PRDedBenCodeMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollUS>();
		}

		protected virtual void _(Events.RowUpdating<PRDeductCode> e)
		{
			PRDeductCode row = e.NewRow;
			if (row == null)
			{
				return;
			}

			if (row.IsWorkersCompensation == true)
			{
				PRAcaDeductCode acaExt = PXCache<PRDeductCode>.GetExtension<PRAcaDeductCode>(row);
				acaExt.AcaApplicable = false;
			}
		}

		protected virtual void _(Events.RowSelected<PRDeductCode> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PRAcaDeductCode.acaApplicable>(e.Cache, e.Row, e.Row.CountryID == LocationConstants.USCountryCode);
		}
	}
}