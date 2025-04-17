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
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.SQLTree;
using PX.Objects.IN;

namespace PX.Objects.Common.Bql
{
	/// <exclude/>
	public class EqualSameCostCenter<Operand> : In<Operand> where Operand : IBqlParameter
	{
		public override void Verify(PXCache cacheBase, object item, List<object> pars, ref bool? result, ref object value)
		{
			result = false;
			List<int> costCenters = null;

			if (_operand1 == null)
				_operand1 = _operand1.createOperand<Operand>();

			if (_operand1 is IBqlParameter parameter1)
			{
				if (pars.Count <= 0) return;
				pars.RemoveAt(0);

				Type fieldType = parameter1.GetReferencedType();
				if (fieldType.IsNested && cacheBase.Graph != null)
				{
					Type cacheType = BqlCommand.GetItemType(fieldType);
					PXCache cache = cacheBase.Graph.Caches[cacheType];
					if (cache.InternalCurrent != null)
					{
						var costCenterID = cache.GetValue(cache.Current, fieldType.Name) as int?;
						if (costCenterID != null)
							costCenters = GetSameCostCenters(cache.Graph, costCenterID);
					}
					else
					{
						result = true;
						return;
					}
				}
			}
			else
			{
				throw new NotImplementedException();
			}

			result = costCenters != null && value is int intValue && costCenters.Contains(intValue);
		}

		public override bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;

			if (_operand1 == null) _operand1 = _operand1.createOperand<Operand>();
			SQLExpression right = null;
			status &= _operand1.AppendExpression(ref right, graph, info, selection);
			if (graph == null || !info.BuildExpression) return status;

			// Optimization for the case when Operand is Required<>
			List<int> costCenters = null;
			if (_operand1 is IBqlParameter parameter1)
			{
				Type fieldType = parameter1.GetReferencedType();
				if (fieldType.IsNested)
				{
					Type cacheType = BqlCommand.GetItemType(fieldType);
					PXCache cache = graph.Caches[cacheType];
					if (cache.Current != null)
					{
						var costCenterID = cache.GetValue(cache.Current, fieldType.Name) as int?;
						if (costCenterID != null)
							costCenters = GetSameCostCenters(cache.Graph, costCenterID);
					}
				}
				else
				{
					throw new NotImplementedException();
				}
			}
			exp = costCenters?.Count > 0
				? exp.In(costCenters.Select(c => new SQLConst(c)))
				: new SQLConst(1).EQ(0);

			return status;
		}


		private List<int> GetSameCostCenters(PXGraph graph, int? costCenterID)
		{
			if (costCenterID == CostCenter.FreeStock)
				return new List<int>() { CostCenter.FreeStock };

			var costCenter = INCostCenter.PK.Find(graph, costCenterID);
			if (costCenter == null)
				return null;

			switch(costCenter.CostLayerType)
			{
				case CostLayerType.Special:
					return GetSameSpecialCostCenters(graph, costCenter);
				case CostLayerType.Project:
					return GetSameProjectCostCenters(graph, costCenter);
				default:
					if (PM.ProjectDefaultAttribute.IsProject(graph, costCenter.ProjectID))
					{
						return GetSameProjectCostCenters(graph, costCenter);
					}
					else
						throw new NotImplementedException();
			}
		}

		private List<int> GetSameSpecialCostCenters(PXGraph graph, INCostCenter costCenter)
		{
			return SelectFrom<INCostCenter>
				.Where<INCostCenter.costLayerType.IsEqual<INCostCenter.costLayerType.FromCurrent>
					.And<INCostCenter.sOOrderType.IsEqual<INCostCenter.sOOrderType.FromCurrent>>
					.And<INCostCenter.sOOrderNbr.IsEqual<INCostCenter.sOOrderNbr.FromCurrent>>
					.And<INCostCenter.sOOrderLineNbr.IsEqual<INCostCenter.sOOrderLineNbr.FromCurrent>>>
				.View.SelectMultiBound(graph, new object[] { costCenter })
				.AsEnumerable().Select(c => (int)((INCostCenter)c).CostCenterID).ToList();
		}

		private List<int> GetSameProjectCostCenters(PXGraph graph, INCostCenter costCenter)
		{
			return SelectFrom<INCostCenter>
				.Where<INCostCenter.costLayerType.IsEqual<INCostCenter.costLayerType.FromCurrent>
					.And<INCostCenter.projectID.IsEqual<INCostCenter.projectID.FromCurrent>>
					.And<INCostCenter.taskID.IsEqual<INCostCenter.taskID.FromCurrent>>>
				.View.SelectMultiBound(graph, new object[] { costCenter })
				.AsEnumerable().Select(c => (int)((INCostCenter)c).CostCenterID).ToList();
		}
	}
}
