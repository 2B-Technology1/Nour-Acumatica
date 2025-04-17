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
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN.RelatedItems
{
    public abstract class ValidateRequiredRelatedItems<TGraph, TSubstitutableDocument, TSubstitutableLine> : PXGraphExtension<TGraph> 
        where TGraph : PXGraph
        where TSubstitutableDocument : class, IBqlTable, ISubstitutableDocument, new()
        where TSubstitutableLine : class, IBqlTable, ISubstitutableLine, new()
    {
        protected virtual bool IsMassProcessing => PXLongOperation.GetCustomInfoForCurrentThread(PXProcessing.ProcessingKey) != null;

        public virtual bool Validate(TSubstitutableLine substitutableLine)
        {
            if (SubstitutionRequired(substitutableLine))
            {
                ThrowError();
                return false;
            }
            return true;
        }

        protected virtual bool SubstitutionRequired(TSubstitutableLine substitutableLine) => substitutableLine.SubstitutionRequired == true;

        public abstract void ThrowError();
    }
}
