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
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CA
{
    public class CADrCr : DrCr
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { CADebit, CACredit },
                new string[] { Messages.CADebit, Messages.CACredit })
            { }
        }

        public const string CADebit = Debit;
        public const string CACredit = Credit;

        public class cADebit : debit { }

        public class cACredit : credit { }

        public static decimal DebitAmt(string drCr, decimal curyTranAmt)
        {
            switch (drCr)
            {
                case Credit:
                    return (decimal)0.0;
                case Debit:
                    return curyTranAmt;
                default:
                    return (decimal)0.0;
            }
        }

        public static decimal CreditAmt(string drCr, decimal curyTranAmt)
        {
            switch (drCr)
            {
                case Credit:
                    return curyTranAmt;
                case Debit:
                    return (decimal)0.0;
                default:
                    return (decimal)0.0;
            }
        }
    }
}
