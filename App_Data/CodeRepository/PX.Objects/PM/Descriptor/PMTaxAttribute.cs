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

using PX.Objects.TX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Common;
using static PX.Objects.PM.PMProforma;

namespace PX.Objects.PM
{
	public class PMTaxAttribute : TaxAttribute
	{
		protected int SortOrder = 0;
		public PMTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			CuryLineTotal = typeof(PMProformaLine.curyLineTotal);
			CuryTranAmt = typeof(PMProformaLine.curyLineTotal);
			CuryDocBal = null;
			CuryTaxTotal = typeof(PMProforma.curyTaxTotal);
			CuryTaxInclTotal = typeof(PMProforma.curyTaxInclTotal);
			DocDate = typeof(PMProforma.invoiceDate);
		}

		protected override List<object> SelectDocumentLines(PXGraph graph, object row)
		{
			var result = new List<object>();

			if (row == null)
				return result;

			SelectDetails(graph, row, out var progressiveDetails, out var transactionDetails);

			if (progressiveDetails != null)
				result.AddRange(progressiveDetails);

			if (transactionDetails != null)
				result.AddRange(transactionDetails);

			return result;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					int? linenbr = int.MinValue;

					if (row != null && row.GetType() == typeof(PMProformaProgressLine))
					{
						linenbr = (int?)graph.Caches[typeof(PMProformaProgressLine)].GetValue<PMProformaProgressLine.lineNbr>(row);
					}

					if (row != null && row.GetType() == typeof(PMProformaTransactLine))
					{
						linenbr = (int?)graph.Caches[typeof(PMProformaTransactLine)].GetValue<PMProformaTransactLine.lineNbr>(row);
					}

					foreach (PMTax record in PXSelect<PMTax, 
						Where<PMTax.refNbr, Equal<Current<PMProforma.refNbr>>,
							And<PMTax.revisionID, Equal<Current<PMProforma.revisionID>>>>>.SelectMultiBound(graph, new object[] { row }))
					{
						if (record.LineNbr == linenbr)
						{
							AppendTail<PMTax, Where>(graph, ret, record, row, parameters);
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (PMTax record in PXSelect<PMTax, 
						Where<PMTax.refNbr, Equal<Current<PMProforma.refNbr>>, 
							And<PMTax.revisionID, Equal<Current<PMProforma.revisionID>>>>>.SelectMultiBound(graph, new object[] { row }))
					{
						AppendTail<PMTax, Where>(graph, ret, record, row, parameters);
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (PMTaxTran record in PXSelect<PMTaxTran,
						Where<PMTaxTran.refNbr, Equal<Current<PMProforma.refNbr>>, 
							And<PMTaxTran.revisionID, Equal<Current<PMProforma.revisionID>>>>>.SelectMultiBound(graph, new object[] { row }))
					{
						AppendTail<PMTaxTran, Where>(graph, ret, record, row, parameters);
					}
					return ret;
				default:
					return ret;
			}
		}

		protected virtual void AppendTail<T, W>(PXGraph graph, List<object> ret, T record, object row, params object[] parameters) where T : class, ITaxDetail, PX.Data.IBqlTable, new()
			where W : IBqlWhere, new()
		{
			IComparer<Tax> taxByCalculationLevelComparer = GetTaxByCalculationLevelComparer();
			taxByCalculationLevelComparer.ThrowOnNull(nameof(taxByCalculationLevelComparer));

			PXSelectBase<Tax> select = new PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
				And<TaxRev.outdated, Equal<False>,
				And<TaxRev.taxType, Equal<TaxType.sales>,
				And<Tax.taxType, NotEqual<CSTaxType.withholding>,
				And<Tax.taxType, NotEqual<CSTaxType.use>,
				And<Tax.reverseTax, Equal<False>,
				And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
				Where2<Where<Tax.taxID, Equal<Required<Tax.taxID>>>, And<W>>>(graph);

			List<object> newParams = new List<object>();
			newParams.Add(this.GetDocDate(ParentCache(graph), row));
			newParams.Add(record.TaxID);

			if (parameters != null)
			{
				newParams.AddRange(parameters);
			}

			foreach (PXResult<Tax, TaxRev> line in select.View.SelectMultiBound(new object[] { row }, newParams.ToArray()))
			{
				int idx;
				for (idx = ret.Count;
					(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<T, Tax, TaxRev>)ret[idx - 1], line) > 0;
					idx--) ;

				ret.Insert(idx, new PXResult<T, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
			}
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			decimal CuryLineTotal = 0m;

			SelectDetails(sender, row, out var progressiveDetails, out var transactionDetails);

			if (progressiveDetails != null)
			{
				foreach (object detrow in progressiveDetails)
				{
					CuryLineTotal += GetCuryTranAmt(sender.Graph.Caches[typeof(PMProformaProgressLine)], sender.Graph.Caches[typeof(PMProformaProgressLine)].ObjectsEqual(detrow, row) ? row : detrow) ?? 0m;
				}
			}
			if (transactionDetails != null)
			{
				foreach (object detrow in transactionDetails)
				{
					CuryLineTotal += GetCuryTranAmt(sender.Graph.Caches[typeof(PMProformaTransactLine)], sender.Graph.Caches[typeof(PMProformaTransactLine)].ObjectsEqual(detrow, row) ? row : detrow) ?? 0m;
				}
			}
			return CuryLineTotal;
		}

		protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			try
			{
				forceRetainedTaxesOff = true;
				base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);
			}
			finally
			{
				forceRetainedTaxesOff = false;
			}
		}

		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType="I")
		{
			PMProformaLine line = (PMProformaLine)row;
			if (IsRetainedTaxes(sender.Graph))
			{
				return line.CuryLineTotal - line.CuryRetainage.GetValueOrDefault();
			}

			return line.CuryLineTotal;
		}
		
		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.RowUpdated(sender, e);

			PMProformaLine row = e.Row as PMProformaLine;
			PMProformaLine oldRow = e.OldRow as PMProformaLine;
			
			if (_TaxCalc == TaxCalc.Calc && row != null && oldRow != null)
			{
				if (row.TaxCategoryID == oldRow.TaxCategoryID && 
					row.CuryLineTotal == oldRow.CuryLineTotal &&
					row.CuryRetainage != oldRow.CuryRetainage)
				{
					CalcTaxes(sender, e.Row, PXTaxCheck.Line);
				}				
			}
		}

		protected bool forceRetainedTaxesOff = false;
		protected override bool IsRetainedTaxes(PXGraph graph)
		{
			if (forceRetainedTaxesOff)
				return false;

			PXCache cache = graph.Caches[typeof(ARSetup)];
			ARSetup arsetup = cache.Current as ARSetup;
			
			return
				PXAccess.FeatureInstalled<FeaturesSet.retainage>() &&
				arsetup?.RetainTaxes == true;
		}

		public override int CompareTo(object other)
		{
			return SortOrder.CompareTo(((PMTaxAttribute)other).SortOrder);
		}

		private void SelectDetails(PXGraph graph, object row, out object[] progressiveDetails, out object[] transactionDetails)
		{
			SelectDetails(graph.Caches[row.GetType()], row, out progressiveDetails, out transactionDetails);
		}

		private void SelectDetails(PXCache sender, object row, out object[] progressiveDetails, out object[] transactionDetails)
		{
			object document = PXParentAttribute.SelectParent(sender, row, _ParentType);

			progressiveDetails = PXParentAttribute.SelectChildren(sender.Graph.Caches[typeof(PMProformaProgressLine)], document, _ParentType);
			transactionDetails = PXParentAttribute.SelectChildren(sender.Graph.Caches[typeof(PMProformaTransactLine)], document, _ParentType);
		}
	}
	public class PMRetainedTaxAttribute : PMTaxAttribute
	{
		public PMRetainedTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			CuryLineTotal = typeof(PMProformaLine.curyRetainage);
			CuryTranAmt = typeof(PMProformaLine.curyRetainage);
			CuryDocBal = null;
			CuryTaxTotal = typeof(PMProforma.curyRetainageTaxTotal);
			CuryTaxInclTotal = typeof(PMProforma.curyRetainageTaxInclTotal);
			DocDate = typeof(PMProforma.invoiceDate);
			_CuryTaxableAmt = typeof(PMTax.curyRetainedTaxableAmt).Name;
			_CuryTaxAmt = typeof(PMTax.curyRetainedTaxAmt).Name;
			SortOrder = 1;
		}

		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType="I")
		{
			PMProformaLine line = (PMProformaLine)row;
			if (IsRetainedTaxes(sender.Graph))
			{
				return line.CuryRetainage;
			}
			return 0m;
		}

		protected override List<object> SelectTaxes<WhereType>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return
				IsRetainedTaxes(graph)
					? base.SelectTaxes<WhereType>(graph, row, taxchk, parameters)
					: new List<object>();
		}

		protected override bool IsRetainedTaxes(PXGraph graph)
		{
			//if (forceRetainedTaxesOff)
			//	return false;

			PXCache cache = graph.Caches[typeof(ARSetup)];
			ARSetup arsetup = cache.Current as ARSetup;

			return
				PXAccess.FeatureInstalled<FeaturesSet.retainage>() &&
				arsetup?.RetainTaxes == true;
		}

		protected override void ReDefaultTaxes(PXCache cache, List<object> details) { }

		protected override void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true) { }

		protected override void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			if (IsRetainedTaxes(sender.Graph))
			{
				base.DefaultTaxes(sender, row, DefaultExisting);
			}
		}
	}
}
