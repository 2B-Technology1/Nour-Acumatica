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
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using System;
using PX.Objects.AP.Standalone;


namespace PX.Objects.DR
{
	public class DraftScheduleMaintMultipleBaseCurrencies : PXGraphExtension<DraftScheduleMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		public override void Initialize()
		{
			base.Initialize();
			PXUIFieldAttribute.SetVisible<DRSchedule.baseCuryID>(Base.Schedule.Cache, null, !PXAccess.FeatureInstalled<FeaturesSet.aSC606>());
			PXUIFieldAttribute.SetVisible<DRScheduleMultipleBaseCurrencies.baseCuryIDASC606>(Base.Schedule.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.aSC606>());
		}

		protected virtual void _(Events.FieldUpdated<DRSchedule.bAccountID> e)
		{
			if (e.Row == null) return;
			var schedule = (DRSchedule)e.Row;
			var baccount = (BAccount)PXSelectorAttribute.Select<DRSchedule.bAccountID>(e.Cache, schedule);
			var baBaseCuryID = VisibilityRestriction.IsNotEmpty(baccount?.COrgBAccountID) ||
									VisibilityRestriction.IsNotEmpty(baccount?.VOrgBAccountID) ?
									baccount.BaseCuryID : null;

			e.Cache.RaiseExceptionHandling<DRSchedule.baseCuryID>(schedule, null, null);
			e.Cache.SetValuePending<DRSchedule.baseCuryID>(schedule, PXCache.NotSetValue);
			e.Cache.SetValue<DRSchedule.baseCuryID>(schedule, baBaseCuryID ?? Base.Accessinfo.BaseCuryID);
		}

		protected virtual void _(Events.FieldUpdated<DRSchedule.baseCuryID> e)
		{
			var schedule = (DRSchedule)e.Row;
			CurrencyInfo origCurrencyInfo = Base.CurrencyInfo.Select();

			Base.CurrencyInfo.Cache.Clear();
			CurrencyInfo info = (CurrencyInfo)Base.CurrencyInfo.Cache.CreateInstance();
			if (schedule.RefNbr == null)
			{
				info.CuryID = (string)e.NewValue;
				info.BaseCuryID = (string)e.NewValue;
			}
			else
			{
				info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)origCurrencyInfo);
			}
			info.CuryInfoID = null;
			info.IsReadOnly = false;
			info = PXCache<CurrencyInfo>.CreateCopy(Base.CurrencyInfo.Insert(info));

			schedule.CuryInfoID = info.CuryInfoID;
			schedule.CuryID = info.CuryID;
		}

		protected virtual void _(Events.FieldDefaulting<CurrencyInfo.baseCuryID> e)
		{
			e.NewValue = Base.Schedule?.Current?.BaseCuryID ?? Base.Accessinfo.BaseCuryID;
			e.Cancel = true;
		}

		protected virtual void _(Events.RowSelected<DRSchedule> e)
		{
			if (e.Row == null) return;
			var schedule = (DRSchedule)e.Row;
			var baseCuryEnabled = CanBaseCurrencyBeChanged(e.Cache, schedule) && !Base.Components.Any();
			PXUIFieldAttribute.SetEnabled<DRSchedule.baseCuryID>(e.Cache, schedule, baseCuryEnabled);
			PXUIFieldAttribute.SetEnabled<DRScheduleMultipleBaseCurrencies.baseCuryIDASC606>(e.Cache, schedule, baseCuryEnabled);
			
			
			if (e.Row.BaseCuryID == null)
			{
				Exception ex = new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat,
					PXUIFieldAttribute.GetDisplayName<DRScheduleMultipleBaseCurrencies.baseCuryIDASC606>(e.Cache));
				e.Cache.RaiseExceptionHandling<DRSchedule.baseCuryID>(e.Row, null, ex);
			}
			else
			{
				e.Cache.RaiseExceptionHandling<DRSchedule.baseCuryID>(e.Row, null, null);
			}
			
			PXFieldState state = e.Cache.GetValueExt<DRSchedule.baseCuryID>(e.Row) as PXFieldState;

			Base.Components.AllowInsert =
			Base.Components.AllowUpdate =
			Base.Components.AllowDelete = state != null && state.Error == null;
		}

		internal static bool CanBaseCurrencyBeChanged(PXCache cache, DRSchedule schedule)
		{
			if (schedule.RefNbr != null) return false;

			var baccount = (BAccount)PXSelectorAttribute.Select<DRSchedule.bAccountID>(cache, schedule);
			var baIsRestricted = VisibilityRestriction.IsNotEmpty(baccount?.COrgBAccountID) || VisibilityRestriction.IsNotEmpty(baccount?.VOrgBAccountID);
			var canBeChanged = baccount != null && !baIsRestricted;
			return canBeChanged;
		}
	}
}
