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
    public class VendorShipmentStatus
    {
        public const string Open = "N";
        public const string Hold = "H";
        public const string Completed = "C";
        public const string Cancelled = "L";
        public const string Confirmed = "F";

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(
                new[]
                {
                    Pair(Open, PX.Objects.SO.Messages.Open),
                    Pair(Hold, PX.Objects.SO.Messages.Hold),
                    Pair(Completed, PX.Objects.SO.Messages.Completed),
                    Pair(Cancelled, PX.Objects.SO.Messages.Cancelled),
                    Pair(Confirmed, PX.Objects.SO.Messages.Confirmed)
                })
            { }
        }

        public class open : PX.Data.BQL.BqlString.Constant<open>
        {
            public open() : base(Open) {; }
        }

        public class hold : PX.Data.BQL.BqlString.Constant<hold>
        {
            public hold() : base(Hold) {; }
        }

        public class completed : PX.Data.BQL.BqlString.Constant<completed>
        {
            public completed() : base(Completed) {; }
        }

        public class cancelled : PX.Data.BQL.BqlString.Constant<cancelled>
        {
            public cancelled() : base(Cancelled) {; }
        }

        public class confirmed : PX.Data.BQL.BqlString.Constant<confirmed>
        {
            public confirmed() : base(Confirmed) {; }
        }
    }
}