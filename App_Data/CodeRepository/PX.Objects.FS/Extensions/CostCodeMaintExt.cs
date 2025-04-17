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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.PM;
using System.Collections.Generic;

namespace PX.Objects.FS
{
	public class CostCodeMaintExt : PXGraphExtension<CostCodeMaint>
	{
		public PXSetup<FSSetup> FSSetup;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
		}

		protected virtual void _(Events.RowSelected<FSSrvOrdType> e)
		{
			var fsSrvOrdTypeRow = e.Row;
			var cache = e.Cache;
			if (fsSrvOrdTypeRow != null)
			{
				bool enable = FSSetup.Current != null
								&& FSSetup.Current.EnableEmpTimeCardIntegration == true
								&& fsSrvOrdTypeRow.Behavior != FSSrvOrdType.behavior.Values.Quote;

				bool activateEarningType = enable && fsSrvOrdTypeRow.CreateTimeActivitiesFromAppointment == true;
				bool requireEarningType = activateEarningType && fsSrvOrdTypeRow.Behavior != FSSrvOrdType.behavior.Values.Quote;

				PXUIFieldAttribute.SetRequired<FSSrvOrdType.dfltEarningType>(cache, requireEarningType);
				PXDefaultAttribute.SetPersistingCheck<FSSrvOrdType.dfltEarningType>(cache, fsSrvOrdTypeRow, requireEarningType ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}

		protected virtual void _(Events.RowPersisting<PMCostCode> e)
		{
			if (e.Operation == PXDBOperation.Delete && e.Row != null)
			{
				FSSrvOrdType ordType = SelectFrom<FSSrvOrdType>
					.Where<FSSrvOrdType.active.IsEqual<True>
						.And<FSSrvOrdType.dfltCostCodeID.IsEqual<P.AsInt>>>
					.View
					.SelectSingleBound(Base, null, e.Row.CostCodeID);

				if (ordType != null)
				{
					throw new PXException(TX.Error.EntityCannotBeDeletedBecauseOfOneRefRecord,
						EntityHelper.GetFriendlyEntityName(typeof(PMCostCode), e.Row),
						EntityHelper.GetFriendlyEntityName(typeof(FSSrvOrdType), ordType));
				}

				PXUpdate<
					Set<FSSrvOrdType.dfltCostCodeID, Null>,
				FSSrvOrdType,
				Where<
					FSSrvOrdType.dfltCostCodeID, Equal<Required<FSSrvOrdType.dfltCostCodeID>>>>
				.Update(Base, e.Row.CostCodeID);
			}
		}
	}
}
