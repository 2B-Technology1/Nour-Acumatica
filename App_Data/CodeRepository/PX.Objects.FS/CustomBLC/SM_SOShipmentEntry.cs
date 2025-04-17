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
using PX.Objects.CS;
using PX.Objects.SO;
using System;
using System.Linq;

namespace PX.Objects.FS
{
    public class SM_SOShipmentEntry : PXGraphExtension<SOShipmentEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public delegate void CreateShipmentDelegate(CreateShipmentArgs args);

        /// <summary>
        /// Overrides <see cref="SOShipmentEntry.CreateShipment(CreateShipmentArgs)"/>
        /// </summary>
        [PXOverride]
        public virtual void CreateShipment(CreateShipmentArgs args, CreateShipmentDelegate del)
        {
            ValidatePostBatchStatus(PXDBOperation.Update, ID.Batch_PostTo.SO, args.Order.OrderType, args.Order.RefNbr);
            del(args);
        }

		#region Event handlers

		protected virtual void _(PX.Data.Events.RowSelecting<SOShipment> e)
		{
			SOShipment row = (SOShipment)e.Row;

			if (row == null)
				return;

			FSxSOShipment ext = e.Row.GetExtension<FSxSOShipment>();

			using (new PXConnectionScope())
			{
				ext.IsFSRelated = PXSelectJoin<SOOrderType,
										InnerJoin<SOOrderShipment, On<SOOrderType.orderType, Equal<SOOrderShipment.orderType>>>,
										Where<SOOrderShipment.shipmentType, Equal<Required<SOShipment.shipmentType>>,
											And<SOOrderShipment.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>,
											And<FSxSOOrderType.enableFSIntegration, Equal<True>>>>>
									.SelectWindowed(Base, 0, 1, row.ShipmentType, row.ShipmentNbr)?
									.RowCast<SOOrderType>()
									.Any();
			}
		}
		#endregion

		#region Validations
		public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus<SOShipment>(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}
