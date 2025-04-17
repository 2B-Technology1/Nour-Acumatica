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
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.CM;

namespace PX.Objects.GL
{
    [Serializable]
    public class GLTranCodeMaint : PXGraph<GLTranCodeMaint>
    {
		#region Type Override
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]		
		[CA.CAAPARTranType.ListByModuleRestricted(typeof(GLTranCode.module))]
		[PXUIField(DisplayName = "Module Tran. Type", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void GLTranCode_TranType_CacheAttached(PXCache sender)
		{
		}				
		#endregion

        [PXImport(typeof(GLTranCode))]
        public PXSelect<GLTranCode> TranCodes;
	
        public PXSavePerRow<GLTranCode> Save;
        public PXCancel<GLTranCode> Cancel;

		public virtual void GLTranCode_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				GLTranCode row = (GLTranCode)e.Row;
				bool isSupported = IsSupported(row);
				if (!isSupported)
				{
					PXErrorLevel level = (row.Active == true) ? PXErrorLevel.Error : PXErrorLevel.Warning;
					sender.RaiseExceptionHandling<GLTranCode.tranType>(row, row.TranType, new PXSetPropertyException(Messages.DocumentTypeIsNotSupportedYet, level));
				}
				else
				{
					sender.RaiseExceptionHandling<GLTranCode.tranType>(row, row.TranType, null);
				}
			}
		}

		public virtual void GLTranCode_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null)
			{
				GLTranCode row = (GLTranCode)e.Row;
				if (row.Active == true)
				{
					if (!IsSupported(row))
					{
						sender.RaiseExceptionHandling<GLTranCode.tranType>(row, row.TranType, new PXSetPropertyException(Messages.DocumentTypeIsNotSupportedYet, PXErrorLevel.Error));
					}
				}
			}
		}

		public virtual void GLTranCode_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (e.Row == null) return;

			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof (GLTranDoc.tranCode));
		}

		protected static bool IsSupported(GLTranCode row)
		{
			bool isSupported = true;
			if ((row.Module == GL.BatchModule.AP &&
					(row.TranType == AP.APPaymentType.Refund ||
						//row.TranType == AP.APPaymentType.Check ||
						row.TranType == AP.APPaymentType.VoidCheck ||
						row.TranType == AP.APPaymentType.VoidQuickCheck))
				|| (row.Module == GL.BatchModule.AR
					&& (row.TranType == AR.ARPaymentType.Refund ||
						row.TranType == AR.ARPaymentType.FinCharge ||						
						row.TranType == AR.ARPaymentType.SmallBalanceWO ||
						row.TranType == AR.ARPaymentType.SmallCreditWO ||						
						row.TranType == AR.ARPaymentType.NoUpdate ||
						row.TranType == AR.ARPaymentType.Undefined ||
						row.TranType == AR.ARPaymentType.VoidPayment||
						row.TranType == AR.ARPaymentType.CashReturn))
				|| (row.Module == GL.BatchModule.CA
					&& (row.TranType == CA.CATranType.CAAdjustmentRGOL ||
						row.TranType == CA.CATranType.CADeposit ||
						row.TranType == CA.CATranType.CAVoidDeposit ||
						row.TranType == CA.CATranType.CATransferExp ||
						row.TranType == CA.CATranType.CATransferOut ||
						row.TranType == CA.CATranType.CATransferIn)))
			{
				isSupported = false;
			}
			return isSupported;
		}
    }
}
