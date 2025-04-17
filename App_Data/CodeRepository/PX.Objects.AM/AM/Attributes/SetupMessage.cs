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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Setup message attribute
    /// </summary>
    public class SetupMessage
    {
        public const string AllowMsg = "A";
        public const string WarningMsg = "W";
        public const string ErrorMsg = "E";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            public static string AllowMsg => Messages.GetLocal(Messages.Setting_Allow);
            public static string WarningMsg => Messages.GetLocal(Messages.Setting_Warn);
            public static string ErrorMsg => Messages.GetLocal(Messages.Setting_NotAllow);
        }

        //BQL constants declaration
        public class allowMsg : PX.Data.BQL.BqlString.Constant<allowMsg>
        {
            public allowMsg() : base(AllowMsg) { ;}
        }

        public class warningMsg : PX.Data.BQL.BqlString.Constant<warningMsg>
        {
            public warningMsg() : base(WarningMsg) { ;}
        }

        public class errorMsg : PX.Data.BQL.BqlString.Constant<errorMsg>
        {
            public errorMsg() : base(ErrorMsg) { ;}
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { AllowMsg, WarningMsg, ErrorMsg },
                    new string[] { Messages.Setting_Allow, Messages.Setting_Warn, Messages.Setting_NotAllow }) { }
        }

        public class BackFlushListAttribute : PXStringListAttribute
        {
            public BackFlushListAttribute()
                : base(
                    new string[] { AllowMsg, ErrorMsg },
                    new string[] { Messages.Setting_Allow, Messages.Setting_NotAllow })  { }
        }
    }
}