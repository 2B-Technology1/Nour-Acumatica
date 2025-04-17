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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    [PXProjection(typeof(Select4<AMClockTran,
                        Where<AMClockTran.status, Equal<ClockTranStatus.clockedIn>>,
                        Aggregate<
                        GroupBy<AMClockTran.orderType,
                        GroupBy<AMClockTran.prodOrdID,
                        GroupBy<AMClockTran.operationID,
                        GroupBy<AMClockTran.employeeID,
                         GroupBy<AMClockTran.operationID>>>>>>>))]
    [PXHidden]
    public partial class ClockedInByOperation : IBqlTable
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.manufacturing>();
        }

        #region Order Type
        [AMOrderTypeField(IsKey = true, BqlField = typeof(AMClockTran.orderType))]
        public virtual string OrderType { get; set; }
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
        #region ProdOrdID
        [ProductionNbr(IsKey = true, BqlField = typeof(AMClockTran.prodOrdID))]
        public virtual String ProdOrdID { get; set; }
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }
        
        #endregion
        #region OperationID
        [OperationIDField(IsKey = true, BqlField = typeof(AMClockTran.operationID))]
        public virtual int? OperationID { get; set; }
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }
        #endregion

        #region EmployeeID
        [PXDBInt(IsKey = true, BqlField = typeof(AMClockTran.employeeID))]
        [PXUIField(DisplayName = "Employee ID")]
        public virtual Int32? EmployeeID { get; set; }
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
        #endregion
        #region Active

        protected bool? _Active = false;
        [PXExistance()]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active
        {
            get
            {
                return this._Active;
            }
            set
            {
                this._Active = value;
            }
        }
        #endregion
    }
}
