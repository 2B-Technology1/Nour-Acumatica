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
    /// Manufacturing Configuration Calculate Options
    /// </summary>
    public class CalcOptions
    {
        public const string OnCompletion = "C";
        public const string AfterSelection = "S";

        public static class Desc
        {
            public static string OnCompletion => Messages.GetLocal(Messages.OnCompletion);
            public static string AfterSelection => Messages.GetLocal(Messages.AfterSelection);
        }

        //BQL constants declaration
        public class onCompletion : PX.Data.BQL.BqlString.Constant<onCompletion>
        {
            public onCompletion() : base(OnCompletion) { ;}
        }
        public class afterSelection : PX.Data.BQL.BqlString.Constant<afterSelection>
        {
            public afterSelection() : base(AfterSelection) { ;}
        }
        
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { OnCompletion, AfterSelection },
                    new string[] { Messages.OnCompletion, Messages.AfterSelection }){ }
        }
    }
}