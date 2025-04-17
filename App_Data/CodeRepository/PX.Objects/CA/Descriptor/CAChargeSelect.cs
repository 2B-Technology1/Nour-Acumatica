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

using System.Collections.Generic;
using PX.Data;
using PX.DbServices.Model;
using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.CA
{
	public class CAChargeSelect<DocumentTable, DocDate, FinPeriodID,
			ChargeTable, EntryTypeID, ChargeRefNbr, WhereSelect> : PXSelect<ChargeTable, WhereSelect>
		where DocumentTable : class, ICADocument, IBqlTable, new()
		where DocDate : IBqlField
		where FinPeriodID : IBqlField
		where ChargeTable : class, IBqlTable, AP.IPaymentCharge, new()
		where EntryTypeID : IBqlField
		where ChargeRefNbr : IBqlField
		where WhereSelect : IBqlWhere, new()
	{
		#region Ctor
		public CAChargeSelect(PXGraph graph)
			: base(graph)
		{
			graph.RowUpdated.AddHandler<DocumentTable>(PaymentRowUpdated);
			graph.RowPersisting.AddHandler<ChargeTable>(ChargeTable_RowPersisting);
		}
		#endregion

		#region Implementation
		protected virtual void PaymentRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<DocDate, FinPeriodID>(e.Row, e.OldRow))
			{
				foreach (ChargeTable charge in this.View.SelectMulti())
				{
					this.View.Cache.MarkUpdated(charge);
				}
			}
		}

		protected virtual void ChargeTable_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ChargeTable charge = (ChargeTable)e.Row;

			if (charge.CuryTranAmt == 0m || (charge.CuryTranAmt < 0m && !IsAllowedNegativeSign(charge)))
			{
				Cache.RaiseExceptionHandling(nameof(AP.IPaymentCharge.CuryTranAmt), charge, charge.CuryTranAmt, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
			}
		}
		#endregion

		public void ReverseExpenses(ICADocument oldDoc, ICADocument newDoc)
		{
			ReverseCharges(oldDoc, newDoc, true);
		}

		protected virtual void ReverseCharges(ICADocument oldDoc, ICADocument newDoc, bool reverseSign)
		{
			foreach (PXResult<ChargeTable> paycharge in PXSelect<ChargeTable,
				Where<ChargeRefNbr, Equal<Required<ChargeRefNbr>>>>.Select(this._Graph, oldDoc.RefNbr))
			{
				ChargeTable charge = ReverseCharge((ChargeTable)paycharge, reverseSign);
				this.Insert(charge);
			}
		}

		public virtual ChargeTable ReverseCharge(ChargeTable oldCharge, bool reverseSign)
		{
			ChargeTable charge = PXCache<ChargeTable>.CreateCopy(oldCharge);

			charge.DocType = CATranType.CATransferExp;
			charge.RefNbr = null;
			charge.CuryTranAmt = (reverseSign ? -1 : 1) * charge.CuryTranAmt;
			charge.Released = false;
			charge.CashTranID = null;
			this.Cache.SetValueExt(charge, AcumaticaDb.NoteId, null);
			return charge;
		}

		protected virtual bool IsAllowedNegativeSign(ChargeTable charge) => true;
	}
}