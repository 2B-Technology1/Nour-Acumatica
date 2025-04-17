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
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    #region PXProjection
    [Serializable]
    [PXBreakInheritance]
    [PXProjection(typeof(Select<SOOrderType>))]
    #endregion
    public class SOOrderTypeQuickProcess : SOOrderType
    {
        #region OrderType
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        #endregion
        #region Behavior
        public new abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
        #endregion
        #region AllowQuickProcess
        public new abstract class allowQuickProcess : PX.Data.BQL.BqlBool.Field<allowQuickProcess> { }
        #endregion
    }
}
