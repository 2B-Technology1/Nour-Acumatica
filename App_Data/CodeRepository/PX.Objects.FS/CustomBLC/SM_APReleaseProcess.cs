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
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.PO.GraphExtensions.APReleaseProcessExt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class SM_APReleaseProcess : PXGraphExtension<UpdatePOOnRelease, APReleaseProcess.MultiCurrency, APReleaseProcess>
    {
		#region Views

		PXSelect<FSSODet> FsSODet;

		PXSelect<FSAppointmentDet> FsAppointmentDet;

		#endregion

		public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

		[PXHidden]
        public PXSelect<FSServiceOrder> serviceOrderView;

        #region Overrides
        [PXOverride]
        public void VerifyStockItemLineHasReceipt(APRegister arRegisterRow, Action<APRegister> del)
        {
            if (arRegisterRow.CreatedByScreenID != ID.ScreenID.INVOICE_BY_APPOINTMENT
                    && arRegisterRow.CreatedByScreenID != ID.ScreenID.INVOICE_BY_SERVICE_ORDER)
            {
                if (del != null)
                {
                    del(arRegisterRow);
                }
            }
        }

        public delegate APRegister OnBeforeReleaseDelegate(APRegister apdoc);

        [PXOverride]
        public virtual APRegister OnBeforeRelease(APRegister apdoc, OnBeforeReleaseDelegate del)
        {
            ValidatePostBatchStatus(PXDBOperation.Update, ID.Batch_PostTo.AP, apdoc.DocType, apdoc.RefNbr);

            if (del != null)
            {
                return del(apdoc);
            }

            return null;
        }
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowPersisted<POOrder> e)
        {
            if (e.TranStatus == PXTranStatus.Open && e.Operation == PXDBOperation.Update)
            {
                POOrder poOrderRow = (POOrder)e.Row;
                string poOrderOldStatus = (string)e.Cache.GetValueOriginal<POOrder.status>(poOrderRow);

                if (poOrderOldStatus != poOrderRow.Status)
                {
                    FSPOReceiptProcess.UpdateSrvOrdLinePOStatus(e.Cache.Graph, poOrderRow);
                }
            }
        }

		/// <summary>
		/// Extends <see cref="APReleaseProcess.InvoiceTransactionsReleased(InvoiceTransactionsReleasedArgs)"/>
		/// </summary>
		[PXOverride]
		public virtual void InvoiceTransactionsReleased(InvoiceTransactionsReleasedArgs args)
		{
			PXCache soLineCache = Base.Caches<FSSODet>(); 
			PXCache apptLineCache = Base.Caches<FSAppointmentDet>();
			PXCache soCache = Base.Caches<FSServiceOrder>();
			PXCache apptCache = Base.Caches<FSAppointment>();

			foreach (APTran apRow in Base.APTran_TranType_RefNbr.Cache.Updated)
			{
				var updateRow = SelectFrom<FSSODet>
								.LeftJoin<FSAppointmentDet>.On<FSAppointmentDet.sODetID.IsEqual<FSSODet.sODetID>>
								.Where<FSSODet.poType.IsEqual<@P.AsString>
									.And<FSSODet.poNbr.IsEqual<@P.AsString>>
									.And<FSSODet.poLineNbr.IsEqual<@P.AsInt>>
									.And<FSSODet.curyUnitCost.IsNotEqual<@P.AsDecimal>>
									.And<Brackets<FSSODet.lineType.IsEqual<FSLineType.NonStockItem>
										.Or<FSSODet.lineType.IsEqual<FSLineType.Service>>>>>
								.View
								.Select(Base, apRow.POOrderType, apRow.PONbr, apRow.POLineNbr, apRow.CuryUnitCost)
								.FirstOrDefault();

				if (updateRow != null)
				{
					var soLine = updateRow.GetItem<FSSODet>();
					var apLine = updateRow.GetItem<FSAppointmentDet>();

					if (soLine != null)
					{
						FSSODet soLineCopy = (FSSODet)soLineCache.CreateCopy(soLine);
						soLineCopy.CuryUnitCost = apRow.CuryUnitCost;
						soLineCopy.UnitCost = apRow.UnitCost;
						soLineCache.Update(soLineCopy);

						if (apLine != null)
						{
							FSAppointmentDet apptLineCopy = (FSAppointmentDet)apptLineCache.CreateCopy(apLine);
							apptLineCopy.CuryUnitCost = apRow.CuryUnitCost;
							apptLineCopy.UnitCost = apRow.UnitCost;
							apptLineCache.Update(apptLineCopy);
						}
					}
				}
			}
			soLineCache.Persist(PXDBOperation.Update);
			apptLineCache.Persist(PXDBOperation.Update);
			soCache.Persist(PXDBOperation.Update);
			apptCache.Persist(PXDBOperation.Update);
						
		}

		#endregion

		#region Cache Attached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXCheckUnique))]
		protected virtual void _(Events.CacheAttached<FSSODet.lineNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXCheckUnique))]
		protected virtual void _(Events.CacheAttached<FSAppointmentDet.lineNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXUIVerifyAttribute))]
		[PXRemoveBaseAttribute(typeof(FSDBTimeSpanLongAllowNegativeAttribute))]
		protected virtual void _(Events.CacheAttached<FSAppointmentDet.actualDuration> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXUIVerifyAttribute))]
		protected virtual void _(Events.CacheAttached<FSAppointmentDet.actualQty> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXUIVerifyAttribute))]
		protected virtual void _(Events.CacheAttached<FSAppointmentDet.billableQty> e)
		{
		}
		#endregion

		#region Validations
		public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus<APRegister>(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}
