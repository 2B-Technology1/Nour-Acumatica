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
using System.Text;

using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.AR
{
	public class ARPriceClassMaint : PXGraph<ARPriceClassMaint>
	{
        public PXSavePerRow<ARPriceClass> Save;
		public PXCancel<ARPriceClass> Cancel;
		public PXSelect<ARPriceClass> Records;


		protected virtual void ARPriceClass_CustPriceClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARPriceClass row = e.Row as ARPriceClass;
			if (row != null)
			{
				if ( ARPriceClass.EmptyPriceClass == e.NewValue.ToString())
				{
					e.Cancel = true;
					if (sender.RaiseExceptionHandling<ARPriceClass.priceClassID>(e.Row, null, new PXSetPropertyException(Messages.ReservedWord, ARPriceClass.EmptyPriceClass)))
					{
						throw new PXSetPropertyException(typeof(ARPriceClass.priceClassID).Name, null, Messages.ReservedWord, ARPriceClass.EmptyPriceClass);
					}
				}
			}
		}


		protected virtual void ARPriceClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			ARPriceClass row = e.Row as ARPriceClass;
			if (row != null)
			{
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(DiscountCustomerPriceClass.customerPriceClassID));

				/* TODO: add customer(location) ref. */
			}
		}
	}
}