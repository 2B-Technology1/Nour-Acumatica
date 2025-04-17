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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using static PX.SM.EMailAccount;

namespace PX.Objects.AM.GraphExtensions
{
	/// <summary>
	/// Manufacturing extension to SOLineSplitPlanIDAttribute.
	/// Implements "SO to Production" allocation
	/// </summary>
	public class SOLineSplitPlanAM : PXGraphExtension<SOLineSplitPlan, SOOrderEntry>
	{
		public static bool IsActive() => !Common.IsPortal && PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();

		public const string SoToProductionPlanType = INPlanConstants.PlanM8;

		/// <summary>
		/// Overrides <see cref="SOLineSplitPlan.InitPlanRequired"/>
		/// </summary>
		[PXOverride]
		public virtual bool InitPlanRequired(SOLineSplit row, SOLineSplit oldRow,
			Func<SOLineSplit, SOLineSplit, bool> base_InitPlanRequired)
		{
			return base_InitPlanRequired(row, oldRow)
				|| !Base.Caches<SOLineSplit>().ObjectsEqual<SOLineSplit.aMProdCreate>(row, oldRow);
		}

		/// <summary>
		/// Overrides <see cref="SOLineSplitPlan.IsLineLinked"/>
		/// </summary>
		[PXOverride]
		public virtual bool IsLineLinked(SOLineSplit soLineSplit,
			Func<SOLineSplit, bool> base_IsLineLinked)
		{
			return base_IsLineLinked(soLineSplit) || PXCache<SOLineSplit>.GetExtension<SOLineSplitExt>(soLineSplit)?.AMProdOrdID != null;
		}

		/// <summary>
		/// Overrides <see cref="SOLineSplitPlan.CalcPlanType(INItemPlan, SOLineSplit, SOOrderType, bool)"/>
		/// </summary>
		[PXOverride]
		public virtual string CalcPlanType(INItemPlan plan, SOLineSplit splitRow, SOOrderType ordertype, bool isOrderOnHold,
			Func<INItemPlan, SOLineSplit, SOOrderType, bool, string> base_CalcPlanType)
		{
			var planType = base_CalcPlanType(plan, splitRow, ordertype, isOrderOnHold);
			if (splitRow?.IsAllocated == false && splitRow?.AMProdCreate == true)
			{
				planType = SoToProductionPlanType;
			}

			return planType;
		}

		/// <summary>
		/// Overrides <see cref="SOLineSplitPlan.DefaultValues"/>
		/// </summary>
		[PXOverride]
		public virtual INItemPlan DefaultValues(INItemPlan planRow, SOLineSplit splitRow,
			Func<INItemPlan, SOLineSplit, INItemPlan> base_DefaultValues)
		{
			var splitRowExt = PXCache<SOLineSplit>.GetExtension<SOLineSplitExt>(splitRow);
			var isProductionLinked = splitRowExt != null && splitRow.AMProdCreate.GetValueOrDefault() && !string.IsNullOrWhiteSpace(splitRowExt.AMProdOrdID);

			var planRowReturn = base_DefaultValues(planRow, splitRow);

			if (planRowReturn == null || splitRowExt == null)
			{
				return planRowReturn;
			}

			if (INPlanTypeHelper.IsMfgPlanType(planRowReturn.PlanType) || isProductionLinked)
			{
				//It is possible during production creation the order gets marked as linked row however...
				//  this doesn't give the plan type enough time to set as M8 due to IsLineLinked(SOLineSplit) reporting back a linked row
				planRowReturn.PlanType = SoToProductionPlanType;

				planRowReturn.FixedSource = INReplenishmentSource.Manufactured;
				planRowReturn.PlanQty = splitRow.BaseQty.GetValueOrDefault() - splitRow.BaseReceivedQty.GetValueOrDefault() - splitRow.BaseShippedQty.GetValueOrDefault();

				if (planRowReturn.PlanQty.GetValueOrDefault() <= 0)
				{
					return null;
				}
				if (isProductionLinked && planRowReturn.SupplyPlanID == null)
				{
					AMProdItemSplit itemSplit = SelectFrom<AMProdItemSplit>
					.Where<AMProdItemSplit.orderType.IsEqual<@P.AsString>
						.And<AMProdItemSplit.prodOrdID.IsEqual<@P.AsString>>>
					.View.Select(Base, splitRowExt.AMOrderType, splitRowExt.AMProdOrdID)
					.ToFirstTable<AMProdItemSplit>().OrderByDescending(d => d.PlanID).FirstOrDefault();
					if (itemSplit?.PlanID != null)
					{
						planRowReturn.SupplyPlanID = itemSplit.PlanID;
					}
				}
			}

			return planRowReturn;
		}
	}
}
