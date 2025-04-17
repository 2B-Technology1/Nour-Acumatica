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
using PX.Objects.CR;
using PX.Objects.CS;
using static PX.Objects.PO.POCreate;

namespace PX.Objects.FS
{
    public class FSxPOCreateFilter : PXCacheExtension<POCreateFilter>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

		#region SrvOrdType
		public abstract class srvOrdType : PX.Data.IBqlField { }
        [PXString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorSrvOrdType]
        [PX.Data.EP.PXFieldDescription]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string SrvOrdType { get; set; }
		#endregion
		#region ServiceOrderRefNbr
		public abstract class serviceOrderRefNbr : PX.Data.IBqlField { }
        [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Service Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search2<FSServiceOrder.refNbr,
                               LeftJoin<BAccountSelectorBase,
                                    On<BAccountSelectorBase.bAccountID, Equal<FSServiceOrder.customerID>>,
                               LeftJoin<Location,
                                    On<Location.locationID, Equal<FSServiceOrder.locationID>>>>,
                               Where<
                                    FSServiceOrder.srvOrdType, Equal<Current<FSxPOCreateFilter.srvOrdType>>>,
                               OrderBy<
                                    Desc<FSServiceOrder.refNbr>>>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string ServiceOrderRefNbr { get; set; }
		#endregion
		#region AppointmentRefNbr
		public abstract class appointmentRefNbr : PX.Data.IBqlField { }
        [PXString(20, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AppointmentRefNbr { get; set; }
        #endregion
    }
}
