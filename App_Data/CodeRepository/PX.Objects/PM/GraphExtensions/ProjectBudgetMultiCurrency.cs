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
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using System;
using System.Collections.Generic;

namespace PX.Objects.PM
{
	public abstract class ProjectBudgetMultiCurrency<TGraph> : MultiCurrencyGraph<TGraph, PMProject> where TGraph : PXGraph
	{
		protected override string Module => BatchModule.PM;

		protected override CurySourceMapping GetCurySourceMapping()
		{
			return new CurySourceMapping(typeof(PMProject));
		}

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(PMProject))
			{
				CuryInfoID = typeof(PMProject.curyInfoID)
			};
		}

		protected override bool ShouldMainCurrencyInfoBeReadonly()
		{
			return true;
		}

		protected override bool AllowOverrideCury()
		{
			return false;
		}

		protected override void CuryRowInserting(PXCache sender, PXRowInsertingEventArgs e, List<CuryField> fields, Dictionary<Type, string> topCuryInfoIDs)
		{
			if (!IsAccumulator(e.Row))
			{
				base.CuryRowInserting(sender, e, fields, topCuryInfoIDs);
			}
		}

		protected override void CuryRowInserted(PXCache sender, PXRowInsertedEventArgs e, List<CuryField> fields)
		{
			if (!IsAccumulator(e.Row))
			{
				recalculateRowBaseValues(sender, e.Row, fields);
			}
		}

		protected virtual bool IsAccumulator(object row)
		{
			return row is PMBudgetAccum;
		}
	}
}
