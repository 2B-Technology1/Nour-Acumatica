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
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AP.CacheExtensions
{
    public sealed class ApInvoiceExt : PXCacheExtension<APInvoice>
    {
        [PXString]
        public string ClDisplayName
        {
            get 
            {
                switch (Base.DocType)
                {
                    case APDocType.Invoice:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.Invoice, Base.RefNbr);
                    case APDocType.DebitAdj:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.DebitAdj, Base.RefNbr);
                    case APDocType.CreditAdj:
                        return string.Format("{0}, {1}", PX.Objects.AP.Messages.CreditAdj, Base.RefNbr);
                }
               
                return string.Format("{0}, {1}", Base.DocType, Base.RefNbr); 
            }
            set { }
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class clDisplayName : IBqlField
        {
        }
    }
}