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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	public class CashForecastEntry : PXGraph<CashForecastEntry>
	{
		#region Internal type definitions
		[Serializable]
		public partial class Filter : PX.Data.IBqlTable
        {
            #region StartDate
            public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
			protected DateTime? _StartDate;
            [PXDBDate]
            [PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
			public virtual DateTime? StartDate
			{
				get
				{
					return this._StartDate;
				}
				set
				{
					this._StartDate = value;
				}
			}
			#endregion
		}

		[CashAccount(DisplayName = "Cash Account",
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(CashAccount.descr), Visible= false)]
        [PXDefault(typeof(CashAccount.cashAccountID))]
        protected virtual void CashForecastTran_CashAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<CashAccount.active, Equal<True>>), CA.Messages.CashAccountInactive, typeof(CashAccount.cashAccountCD))]
		protected virtual void CashAccount_CashAccountCD_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region Buttons

		public PXSave<CashAccount> Save;
        public PXCancel<CashAccount> Cancel;
        #endregion
        #region Ctor + Selects
        public PXFilter<Filter> filter;
        [PXReadOnlyView]
        public PXSelect<CashAccount> filterCashAccounts;
        [PXImport(typeof(Filter))]
		public PXSelect<CashForecastTran,
							Where<CashForecastTran.tranDate, GreaterEqual<Current<Filter.startDate>>,
									And<CashForecastTran.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>>,
									OrderBy<Asc<CashForecastTran.tranDate>>> cashForecastTrans;
		public PXSetup<CASetup> casetup;

		public CashForecastEntry()
		{
			CASetup setup = casetup.Current;
		}

		#endregion
		#region Event Handlers
        protected virtual void CashAccount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CashAccount cashAccount = (CashAccount)e.Row;

			if (cashAccount != null && cashAccount.Active != true)
			{
				string errorMsg = string.Format(CA.Messages.CashAccountInactive, cashAccount.CashAccountCD);
				this.cashForecastTrans.Cache.AllowInsert = false;
				this.cashForecastTrans.Cache.AllowUpdate = false;
				this.cashForecastTrans.Cache.AllowDelete = false;
				sender.RaiseExceptionHandling<CashAccount.cashAccountCD>(cashAccount, cashAccount.CashAccountCD, new PXSetPropertyException<CashAccount.cashAccountCD>(errorMsg, PXErrorLevel.Error));
			}
			else
			{
				bool enableEdit = cashAccount?.CashAccountID != null;
				this.cashForecastTrans.Cache.AllowInsert = enableEdit;
				this.cashForecastTrans.Cache.AllowUpdate = enableEdit;
			}
        }

        protected virtual void CashForecastTran_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CashForecastTran row = (CashForecastTran)e.Row;
			sender.SetDefaultExt<CashForecastTran.curyID>(e.Row);
		}

		protected virtual void CashForecastTran_TranDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Filter filter = this.filter.Current;

            if (filter?.StartDate != null)
			{
				e.NewValue = filter.StartDate;
				e.Cancel = true;
			}
		}
		#endregion
	}
}
