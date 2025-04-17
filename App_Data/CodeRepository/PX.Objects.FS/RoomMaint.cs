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
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class RoomMaint : PXGraph<RoomMaint, FSRoom>
    {
        #region Selects
        public PXSelect<FSRoom,
               Where<
                   FSRoom.branchLocationID, Equal<Optional<FSRoom.branchLocationID>>>> RoomRecords;
        #endregion

        #region Cache Attached
        #region FSRoom_BranchLocationID
        [PXDefault]
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Branch Location ID")]
        [PXSelector(typeof(FSBranchLocation.branchLocationID), SubstituteKey = typeof(FSBranchLocation.branchLocationCD), DescriptionField = typeof(FSBranchLocation.descr))]
        protected virtual void FSRoom_BranchLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRoom_RoomID
        [PXDefault]
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">AAAAAAAAAA")]
        [PXUIField(DisplayName = "Room ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSRoom.roomID, Where<FSRoom.branchLocationID, Equal<Current<FSRoom.branchLocationID>>>>))]
        protected virtual void FSRoom_RoomID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }
}