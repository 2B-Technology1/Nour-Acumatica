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
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class FSLogTypeAction
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(
                new[]
                {
                    Pair(FSLogActionFilter.type.Values.Service, TX.Type_Log.SERVICE),
                    Pair(FSLogActionFilter.type.Values.Travel, TX.Type_Log.TRAVEL),
                    Pair(FSLogActionFilter.type.Values.Staff, TX.Type_Log.STAFF),
                    Pair(FSLogActionFilter.type.Values.ServBasedAssignment, TX.Type_Log.SERV_BASED_ASSIGMENT),
                })
            { }
        }

        public class STListAttribute : PXStringListAttribute
        {
            public STListAttribute() : base(
                new[]
                {
                    Pair(FSLogActionFilter.type.Values.Service, TX.Type_Log.SERVICE),
                    Pair(FSLogActionFilter.type.Values.Travel, TX.Type_Log.TRAVEL),
                })
            { }
        }

        public class Service : Data.BQL.BqlString.Constant<Service>
        {
            public Service() : base(FSLogActionFilter.type.Values.Service) {; }
        }

        public class Travel : Data.BQL.BqlString.Constant<Travel>
        {
            public Travel() : base(FSLogActionFilter.type.Values.Travel) {; }
        }

        public class StaffAssignment : Data.BQL.BqlString.Constant<StaffAssignment>
        {
            public StaffAssignment() : base(FSLogActionFilter.type.Values.Staff) {; }
        }

        public class SrvBasedOnAssignment : Data.BQL.BqlString.Constant<SrvBasedOnAssignment>
        {
            public SrvBasedOnAssignment() : base(FSLogActionFilter.type.Values.ServBasedAssignment) {; }
        }
    }
}
