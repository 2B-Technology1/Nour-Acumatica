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
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO.GraphExtensions.APReleaseProcessExt;
using System;
using System.Collections.Generic;

namespace PX.Objects.PM.GraphExtensions
{
	public class TaxExpenseAllocationExt : PXGraphExtension<PX.Objects.PO.GraphExtensions.APReleaseProcessExt.TaxExpenseAllocationExt, UpdatePOOnRelease, APReleaseProcess.MultiCurrency, APReleaseProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
		}

		[PXOverride]
		public virtual void CalculateTaxExpenseAllocation(APRegister apdoc, List<INRegister> inDocs, Action<APRegister, List<INRegister>> baseCalculateTaxExpenseAllocation)
		{
			using (new SkipDefaultingFromLocationScope())
			{
				baseCalculateTaxExpenseAllocation(apdoc, inDocs);
			}
		}
	}
}
