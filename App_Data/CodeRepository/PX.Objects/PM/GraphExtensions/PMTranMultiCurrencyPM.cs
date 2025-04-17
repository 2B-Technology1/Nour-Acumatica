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
using PX.Objects.CM.Extensions;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;

namespace PX.Objects.PM
{
	public abstract class PMTranMultiCurrencyPM<TGraph> : PMTranMultiCurrency<TGraph>
				where TGraph : PXGraph
	{
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CurrencyInfo(typeof(CurrencyInfo.curyInfoID))]
		protected void _(Events.CacheAttached<PMTran.projectCuryInfoID> e)
		{
		}
		protected override void _(Events.FieldSelecting<Document, Document.curyID> e)
		{
		}

		protected void _(Events.FieldSelecting<PMTran, PMTran.tranCuryID> e)
		{
			if (Base.Accessinfo.CuryViewState)
				e.ReturnValue = GetCurrencyInfo(e.Row?.BaseCuryInfoID)?.BaseCuryID;
		}

		protected override string Module => BatchModule.PM;

		protected void _(Events.RowInserting<PMTran> e)
		{
			DocumentRowInserting<PMTran.projectCuryInfoID, PMTran.projectCuryID>(e.Cache, e.Row);
		}

		protected void _(Events.RowUpdating<PMTran> e)
		{
			DocumentRowUpdating<PMTran.projectCuryInfoID, PMTran.projectCuryID>(e.Cache, e.NewRow);
		}

		protected virtual void _(Events.FieldSelecting<PMTran, PMTran.projectCuryID> e)
		{
			e.ReturnValue = GetCurrencyInfo(e.Row?.ProjectCuryInfoID)?.BaseCuryID;
		}

		protected virtual void _(Events.FieldSelecting<PMTran, PMTran.projectCuryRate> e)
		{
			e.ReturnValue = GetCurrencyInfo(e.Row?.ProjectCuryInfoID)?.SampleCuryRate;
		}

		protected virtual void _(Events.FieldSelecting<PMTran, PMTran.baseCuryRate> e)
		{
			e.ReturnValue = GetCurrencyInfo(e.Row?.BaseCuryInfoID)?.SampleCuryRate;
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.date> e)
		{
			DateFieldUpdated<PMTran.projectCuryInfoID, PMTran.date>(e.Cache, e.Row);
		}
	}
}
