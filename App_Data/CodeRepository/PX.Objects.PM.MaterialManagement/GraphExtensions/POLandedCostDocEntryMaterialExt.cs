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
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.PO.LandedCosts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM.MaterialManagement
{
	public class POLandedCostDocEntryMaterialExt : PXGraphExtension<POLandedCostDocEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
		}

		[PXOverride]
		public virtual LandedCostINAdjustmentFactory GetInAdjustmentFactory(PXGraph graph,
			Func<PXGraph, LandedCostINAdjustmentFactory> baseMethod)
		{
			return new PMLandedCostINAdjustmentFactory(graph);
		}
	}

	public class PMLandedCostINAdjustmentFactory : LandedCostINAdjustmentFactory
	{
		public PMLandedCostINAdjustmentFactory(PXGraph graph):base(graph)
		{
		
		}

		protected override INTran[] CreateTransactions(POLandedCostDoc doc, POLandedCostDetail landedCostDetail, IEnumerable<LandedCostAllocationService.POLandedCostReceiptLineAdjustment> pOLinesToProcess)
		{
			INTran[] result =  base.CreateTransactions(doc, landedCostDetail, pOLinesToProcess);

			Dictionary<string, POReceiptLine> lookup = new Dictionary<string, POReceiptLine>();
			foreach (LandedCostAllocationService.POLandedCostReceiptLineAdjustment poreceiptline in pOLinesToProcess)
			{
				string key = string.Format("{0}.{1}", poreceiptline.ReceiptLine.ReceiptNbr, poreceiptline.ReceiptLine.LineNbr);
				if (!lookup.ContainsKey(key))
				{
					lookup.Add(key, poreceiptline.ReceiptLine);
				}
			}

			foreach (INTran tran in result)
			{
				string key = string.Format("{0}.{1}", tran.POReceiptNbr, tran.POReceiptLineNbr);

				POReceiptLine line;
				if (lookup.TryGetValue(key, out line))
				{
					tran.ProjectID = line.ProjectID;
					tran.TaskID = line.TaskID;
					tran.CostCodeID = line.CostCodeID;
				}
			}

			return result;
		}
	}
}
