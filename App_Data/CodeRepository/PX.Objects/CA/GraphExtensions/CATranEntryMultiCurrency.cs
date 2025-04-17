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

namespace PX.Objects.CA.MultiCurrency
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class CATranEntryMultiCurrency : CAMultiCurrencyGraph<CATranEntry, CAAdj>
    {
        #region SettingUp
        protected override CurySourceMapping GetCurySourceMapping()
        {
            return new CurySourceMapping(typeof(CashAccount))
            {
                CuryID = typeof(CashAccount.curyID),
                CuryRateTypeID = typeof(CashAccount.curyRateTypeID)
            };
        }
        protected override DocumentMapping GetDocumentMapping()
        {
            return new DocumentMapping(typeof(CAAdj))
            {
                CuryID = typeof(CAAdj.curyID),
                CuryInfoID = typeof(CAAdj.curyInfoID),
                BAccountID = typeof(CAAdj.cashAccountID),
                DocumentDate = typeof(CAAdj.tranDate),
                BranchID = typeof(CAAdj.branchID)
            };
        }
        protected override PXSelectBase[] GetChildren()
        {
            return new PXSelectBase[]
            {
                Base.CAAdjRecords,
                Base.CASplitRecords,
                Base.Tax_Rows,
                Base.Taxes
            };
        }
        protected int? AccountProcessing;

		protected string DocumentStatus => Base.CAAdjRecords.Current?.Status;

		protected override string Module => GL.BatchModule.CA;

		protected override CurySource CurrentSourceSelect()
        {
            return CurySource.Select(AccountProcessing);
        }
        #endregion
        #region CAAdj

        protected void _(Events.FieldUpdated<CAAdj, CAAdj.cashAccountID> e)
        {
            SourceFieldUpdated<CAAdj.curyInfoID, CAAdj.curyID, CAAdj.tranDate>(e.Cache, e.Row);
        }

        protected void _(Events.FieldUpdated<CAAdj, CAAdj.tranDate> e)
        {
            CAAdj adj = e.Row as CAAdj;
            if (adj == null) return;

            DateFieldUpdated<CAAdj.curyInfoID, CAAdj.tranDate>(e.Cache, e.Row);

        }

		protected bool ShouldBeDisabledDueToDocStatus()
		{
			switch (DocumentStatus)
			{
				case CATransferStatus.Released:
				case CATransferStatus.Pending:
					return true;
				default: return false;
			}
		}

		protected override bool AllowOverrideRate(PXCache sender, CurrencyInfo info, CurySource source)
		{
			return !ShouldBeDisabledDueToDocStatus() && base.AllowOverrideRate(sender, info, source);
		}
        #endregion
        #region CASplit
        protected void _(Events.RowInserting<CASplit> e, PXRowInserting baseMethod)
		{
            UpdateNewTranDetailCuryTranAmtOrCuryUnitPrice(e.Cache, new CASplit(), e.Row);
            baseMethod(e.Cache, e.Args);
        }

        protected void _(Events.RowUpdating<CASplit> e, PXRowUpdating baseMethod)
        {
            UpdateNewTranDetailCuryTranAmtOrCuryUnitPrice(e.Cache, e.Row, e.NewRow);
            baseMethod(e.Cache, e.Args);
        }
        #endregion
    }
}