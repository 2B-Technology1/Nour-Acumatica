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

namespace PX.Objects.AM.Attributes
{
    public class SchdPlanFlg
    {
        public const string DefaultFlag = "";
        public const string Plan = "P";
        public const string Move = "M";
        public const string Queue = "Q";

        //BQL constants declaration
        public class defaultFlag : PX.Data.BQL.BqlString.Constant<defaultFlag>
        {
            public defaultFlag() : base(DefaultFlag) { ;}
        }
        public class plan : PX.Data.BQL.BqlString.Constant<plan>
        {
            public plan() : base(Plan) { ;}
        }
        public class move : PX.Data.BQL.BqlString.Constant<move>
        {
            public move() : base(Move) { ;}
        }
        public class queue : PX.Data.BQL.BqlString.Constant<queue>
        {
            public queue() : base(Queue) { ;}
        }
    }
}
