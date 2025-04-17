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
    /// Manufacturing Cross (X) Reference Type
    /// </summary>
    public class XRefType
    {
        /// <summary>
        /// Purchase X Reference type = P
        /// </summary>
        public const string Purchase = "P";

        /// <summary>
        /// Manufacture X Reference type = M
        /// </summary>
        public const string Manufacture = "M";

        /// <summary>
        /// Transfer X Reference type = T
        /// </summary>
        public const string Transfer = "T";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Purchase => Messages.GetLocal(Messages.PurchasedXRef);
            public static string Manufacture => Messages.GetLocal(Messages.ManufactureXRef);
            public static string Transfer => Messages.GetLocal(Messages.TransferXRef);
        }

        public class purchase : PX.Data.BQL.BqlString.Constant<purchase>
        {
            public purchase() : base(Purchase) {}
        }

        public class manufacture : PX.Data.BQL.BqlString.Constant<manufacture>
        {
            public manufacture() : base(Manufacture) {}
        }

        public class transfer : PX.Data.BQL.BqlString.Constant<transfer>
        {
            public transfer() : base(Transfer) {}
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                       new string[] { 
                           Manufacture,
                           Purchase,
                           Transfer},
                       new string[] {
                           Messages.PurchasedXRef,
                           Messages.PurchasedXRef,
                           Messages.TransferXRef})
            { }
        }
    }
}