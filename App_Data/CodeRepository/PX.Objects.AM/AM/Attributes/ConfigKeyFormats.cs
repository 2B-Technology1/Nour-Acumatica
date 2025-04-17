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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Configuration Key Formats
    /// </summary>
    public class ConfigKeyFormats
    {
        public const string NoKey = "X";
        public const string Formula = "F";
        public const string NumberSequence = "N";

        public static class Desc
        {
            public static string NoKey => Messages.GetLocal(Messages.NoKey);
            public static string Formula => Messages.GetLocal(Messages.Formula);
            public static string NumberSequence => Messages.GetLocal(Messages.NumberSequence);
        }

        //BQL constants declaration
        public class noKey : PX.Data.BQL.BqlString.Constant<noKey>
        {
            public noKey() : base(NoKey) { }
        }
        public class formula : PX.Data.BQL.BqlString.Constant<formula>
        {
            public formula() : base(Formula) { }
        }
        public class numberSequence : PX.Data.BQL.BqlString.Constant<numberSequence>
        {
            public numberSequence() : base(NumberSequence) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { NoKey, Formula, NumberSequence },
                    new string[] { Messages.NoKey, Messages.Formula, Messages.NumberSequence }) { }
        }
    }
}