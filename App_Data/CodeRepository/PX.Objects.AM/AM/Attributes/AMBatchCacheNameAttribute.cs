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

namespace PX.Objects.AM.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AMBatchCacheNameAttribute : PX.Data.PXCacheNameAttribute
    {
        public AMBatchCacheNameAttribute(string name)
            : base(name)
        {
        }

        public override string GetName(object row)
        {
            var batch = row as AMBatch;
            if (batch == null)
            {
                return base.GetName();
            }

            switch (batch.DocType)
            {
                case AMDocType.Material:
                    return Messages.DocTypeMaterial;
                case AMDocType.Move:
                    return Messages.DocTypeMove;
                case AMDocType.Labor:
                    return Messages.DocTypeLabor;
                case AMDocType.ProdCost:
                    return Messages.DocTypeProdCost;
                case AMDocType.WipAdjust:
                    return Messages.DocTypeWipAdjust;
                case AMDocType.Disassembly:
                    return Messages.Disassembly;
            }
            return "AM Batch";
        }
    }
}