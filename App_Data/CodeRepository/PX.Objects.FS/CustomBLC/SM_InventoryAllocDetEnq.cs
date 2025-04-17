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
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    public class SM_InventoryAllocDetEnq : PXGraphExtension<InventoryAllocDetEnq>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region DacServiceOrder
        [PXHidden]
        public class FSServiceOrder : IBqlTable
        {
            #region SrvOrdType
            [PXDBString(4, IsKey = true, IsFixed = true, InputMask = ">AAAA")]
            public virtual String SrvOrdType { get; set; }
            public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
            #endregion
            #region RefNbr
            [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            public virtual String RefNbr { get; set; }
            public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
            #endregion
            #region NoteID
            [PXDBGuid(IsImmutable = true)]
            public virtual Guid? NoteID { get; set; }
            public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
            #endregion
        }
        #endregion

        public delegate InventoryAllocDetEnq.ItemPlanWithExtraInfo[] UnwrapAndGroupDelegate(PXResultset<InventoryAllocDetEnq.INItemPlan> records);

        [PXOverride]
        public virtual InventoryAllocDetEnq.ItemPlanWithExtraInfo[] UnwrapAndGroup(PXResultset<InventoryAllocDetEnq.INItemPlan> records, UnwrapAndGroupDelegate baseMethod)
        {
            InventoryAllocDetEnq.ItemPlanWithExtraInfo[] result = baseMethod(records);

            foreach (InventoryAllocDetEnq.ItemPlanWithExtraInfo item in result)
            {
                if (item.RefEntity == null && item.ItemPlan.RefEntityType == typeof(FS.FSServiceOrder).FullName)
                {
                    FS.FSServiceOrder fsServiceOrderRow = PXSelect<FS.FSServiceOrder, Where<FS.FSServiceOrder.noteID, Equal<Required<FS.FSServiceOrder.noteID>>>>.Select(Base, item.ItemPlan.RefNoteID);
                    if (fsServiceOrderRow != null)
                    item.RefEntity = new FSServiceOrder { SrvOrdType = fsServiceOrderRow.SrvOrdType, RefNbr = fsServiceOrderRow.RefNbr, NoteID = fsServiceOrderRow.NoteID };
                }
            }

            return result;
        }
    }
}
