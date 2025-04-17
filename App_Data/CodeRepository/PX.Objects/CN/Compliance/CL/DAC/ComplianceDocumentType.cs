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

using PX.Common;
using PX.Data.BQL;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [PXLocalizable]
    public class ComplianceDocumentType
    {
        public const string Certificate = "Certificate";
        public const string Insurance = "Insurance";
        public const string LienWaiver = "Lien Waiver";
        public const string Notice = "Notice";
        public const string Other = "Other";
        public const string Status = "Status";

        public class status : BqlString.Constant<status>
        {
            public status()
                : base(Status)
            {
            }
        }

        public class certificate : BqlString.Constant<certificate>
        {
            public certificate()
                : base(Certificate)
            {
            }
        }

        public class other : BqlString.Constant<other>
        {
            public other()
                : base(Other)
            {
            }
        }

        public class lienWaiver : BqlString.Constant<lienWaiver>
        {
            public lienWaiver()
                : base(LienWaiver)
            {
            }
        }

        public class insurance : BqlString.Constant<insurance>
        {
            public insurance()
                : base(Insurance)
            {
            }
        }

        public class notice : BqlString.Constant<notice>
        {
            public notice()
                : base(Notice)
            {
            }
        }
    }
}