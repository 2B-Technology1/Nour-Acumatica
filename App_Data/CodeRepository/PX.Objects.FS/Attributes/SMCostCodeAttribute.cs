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
using PX.Objects.PM;
using System;

namespace PX.Objects.FS
{
    public class SMCostCodeAttribute : CostCodeAttribute, IPXRowPersistingSubscriber
    {
        private Type _SkipRowPersistingValidation;

        public SMCostCodeAttribute(Type account, Type task) : base(account, task)
        {

        }

        public SMCostCodeAttribute(Type skipRowPersistingValidation, Type account, Type task) : base(account, task)
        {
            _SkipRowPersistingValidation = skipRowPersistingValidation;
        }

        public new void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            bool? skipRowPersistingValidation = _SkipRowPersistingValidation != null ? (bool?)sender.GetValue(e.Row, _SkipRowPersistingValidation.Name) : null;

            if (skipRowPersistingValidation == false)
            {
                base.RowPersisting(sender, e);
            }
        }
    }
}
