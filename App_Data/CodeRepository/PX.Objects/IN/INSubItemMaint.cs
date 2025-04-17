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
using PX.SM;
using PX.Data;


namespace PX.Objects.IN
{
    public class INSubItemMaint : PXGraph<INSubItemMaint>
    {
        public PXCancel<INSubItem> Cancel;
        public PXSavePerRow<INSubItem, INSubItem.subItemID> Save;

        [PXImport(typeof(INSubItem))]
        [PXFilterable]
        public PXSelectOrderBy<INSubItem,OrderBy<Asc<INSubItem.subItemCD>>> SubItemRecords;

        [IN.SubItemRaw(IsKey = true, DisplayName = "Subitem")]
        [PXDefault()]
        protected virtual void INSubItem_SubItemCD_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        protected virtual void INSubItem_Descr_CacheAttached(PXCache sender)
        {
        }

        protected virtual void INSubItem_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            INSubItem row = e.Row as INSubItem; 
            if (row != null)
            {

                INSiteStatusByCostCenter status = PXSelect<INSiteStatusByCostCenter,
					Where<INSiteStatusByCostCenter.subItemID, Equal<Required<INSubItem.subItemID>>>>
					.SelectWindowed(this, 0, 1, row.SubItemID);
                if( status != null)
                {
                    throw new PXSetPropertyException(Messages.SubitemDeleteError);
                }

                INItemXRef itemRef = PXSelect<INItemXRef, Where<INItemXRef.subItemID, Equal<Required<INSubItem.subItemID>>>>.SelectWindowed(this, 0, 1, row.SubItemID);    
                if(itemRef != null)
                {
                    throw new PXSetPropertyException(Messages.SubitemDeleteError);
                }
            }
        }

    }
}
