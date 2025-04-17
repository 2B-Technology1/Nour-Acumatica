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

namespace PX.Objects.IN
{
	public class INMovementClassMaint : PXGraph<INMovementClassMaint>
	{
		public PXSelect<INMovementClass> MovementClasses;
        public PXSavePerRow<INMovementClass> Save;
        public PXCancel<INMovementClass> Cancel;
        		
		protected virtual void INMovementClass_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            //decimal total;
		}

		public virtual void INMovementClass_CountsPerYear_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            if (e.NewValue != null)
            {
                if ( (((short)e.NewValue) < 0) || (((short)e.NewValue) > 365) )
                {
					throw new PXSetPropertyException(Messages.ThisValueShouldBeBetweenP0AndP1, PXErrorLevel.Error, 0, 365);
				}
            }
        }
	}


}
