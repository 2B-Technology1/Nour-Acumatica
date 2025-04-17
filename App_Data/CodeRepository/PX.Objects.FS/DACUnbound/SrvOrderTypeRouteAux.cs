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

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class SrvOrderTypeRouteAux : IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault(typeof(Coalesce<
            Search2<FSxUserPreferences.dfltSrvOrdType,
            InnerJoin<
                FSSrvOrdType, On<FSSrvOrdType.srvOrdType, Equal<FSxUserPreferences.dfltSrvOrdType>>>,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                And<FSSrvOrdType.behavior, Equal<FSSrvOrdType.behavior.Values.routeAppointment>>>>,
            Search<FSRouteSetup.dfltSrvOrdType>>))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType,
                    Where<FSSrvOrdType.active, Equal<True>, 
                    And<FSSrvOrdType.behavior, Equal<FSSrvOrdType.behavior.Values.routeAppointment>>>>))]
        public virtual string SrvOrdType { get; set; }
        #endregion
    }
}