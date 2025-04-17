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

using System;
using System.Collections.Generic;
using PX.CCProcessingBase;
using PX.Data;

namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public class DocDetailsDataReader : IDocDetailsDataReader
	{
		private PXGraph _graph;
		private string _docType;
		private string _refNbr;

		public DocDetailsDataReader(PXGraph graph, string docType, string refNbr)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
			}
			if (string.IsNullOrEmpty(docType))
			{
				throw new ArgumentNullException(nameof(docType));
			}
			if (string.IsNullOrEmpty(refNbr))
			{
				throw new ArgumentNullException(nameof(refNbr));
			}

			_graph = graph;
			_docType = docType;
			_refNbr = refNbr;
		}
				
		void IDocDetailsDataReader.ReadData(List<DocDetailInfo> aData)
		{
			PXSelectBase<ARAdjust> sel = new PXSelect<ARAdjust, Where<ARAdjust.adjgDocType,
														Equal<Required<ARAdjust.adjgDocType>>,
														And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
														And<ARAdjust.voided, Equal<False>>>>>(_graph);
			foreach (ARAdjust it in sel.Select(_docType, _refNbr))
			{
				DocDetailInfo item = new DocDetailInfo();
				item.ItemID = String.Format("{0}{1}", it.AdjdDocType, it.AdjdRefNbr);
				item.ItemName = String.Format("{0} {1}", it.AdjdDocType, it.AdjdRefNbr);
				item.ItemDescription = String.Format("Payment of invoice {0}{1} - {2}", it.AdjdDocType, it.AdjdRefNbr, it.CuryAdjgAmt);
				item.Quantity = 1;
				item.IsTaxable = null;
				item.Price = it.CuryAdjgAmt.Value;
				aData.Add(item);
			}
		}
	}
}
