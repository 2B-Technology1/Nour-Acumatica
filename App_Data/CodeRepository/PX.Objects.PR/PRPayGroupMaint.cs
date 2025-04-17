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

namespace PX.Objects.PR
{
	public class PRPayGroupMaint : PXGraph<PRPayGroupMaint>
	{
		public PXSavePerRow<PRPayGroup> Save;
		public PXCancel<PRPayGroup> Cancel;

		public PXSelect<PRPayGroup> PayGroup;

		#region Data Members for deleting by PXParentAttribute
		public PXSelect<PRPayGroupYearSetup> YearSetup;
		public PXSelect<PRPayGroupYear> Years;
		public PXSelect<PRPayGroupPeriodSetup> PeriodSetup;
		#endregion

		#region Calendar Button

		public PXAction<PRPayGroup> ShowCalendar;
		[PXUIField(DisplayName = FA.Messages.Calendar, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual void showCalendar()
		{
			if (string.IsNullOrWhiteSpace(PayGroup.Current?.PayGroupID))
				return;

			PRPayGroupYearSetupMaint graph = CreateInstance<PRPayGroupYearSetupMaint>();
			graph.FiscalYearSetup.Current = graph.FiscalYearSetup.Search<PRPayGroupYearSetup.payGroupID>(PayGroup.Current.PayGroupID);
			if (graph.FiscalYearSetup.Current == null)
			{
				PRPayGroupYearSetup calendar = new PRPayGroupYearSetup { PayGroupID = PayGroup.Current.PayGroupID };
				graph.FiscalYearSetup.Cache.SetDefaultExt<PRPayGroupYearSetup.periodType>(calendar);
				graph.FiscalYearSetup.Cache.Insert(calendar);
				graph.FiscalYearSetup.Cache.IsDirty = false;
			}
			throw new PXRedirectRequiredException(graph, FA.Messages.Calendar);
		}

		#endregion

		#region PayGroup Event Handlers

		protected virtual void PRPayGroup_IsDefault_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PRPayGroup payGroup = (PRPayGroup)e.Row;
			if (payGroup == null || !(payGroup.IsDefault ?? false)) return;

			foreach (PRPayGroup other in PXSelect<PRPayGroup, Where<PRPayGroup.payGroupID, NotEqual<Current<PRPayGroup.payGroupID>>>>.SelectMultiBound(this, new object[] { payGroup }))
			{
				other.IsDefault = false;
				PayGroup.Update(other);
			}
			PayGroup.View.RequestRefresh();
		}

		protected virtual void _(Events.RowPersisted<PRPayGroup> e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
				MatchWithPayGroupHelper.ClearUserPayGroupIDsSlot();
		}

		public void _(Events.RowSelected<PRPayGroup> e)
		{
			PXCache cache = e.Cache;
			PRPayGroup row = e.Row;

			if (row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PRPayGroup.payGroupID>(cache, row, row.PayGroupID == null);
		}

		#endregion
	}
}
