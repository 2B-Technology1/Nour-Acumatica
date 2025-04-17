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
    /// Manufacturing order type selector
    /// </summary>
    public class AMOrderTypeSelectorAttribute : PXSelectorAttribute
    {
        public AMOrderTypeSelectorAttribute() : base(typeof(Search<AMOrderType.orderType>))
        {
            _DescriptionField = typeof(AMOrderType.descr);
        }

        /// <summary>
        /// Selector by given order type function
        /// </summary>
        /// <param name="functionRestrictor"></param>
        public AMOrderTypeSelectorAttribute(Type functionRestrictor) : base(GetQueryCommandByFunction(functionRestrictor))
        {
            _DescriptionField = typeof(AMOrderType.descr);
        }

        public AMOrderTypeSelectorAttribute(params Type[] fieldList) : base(typeof(Search<AMOrderType.orderType>), fieldList)
        {
            //Leave out description field here
        }

        /// <summary>
        /// Selector by given order type function
        /// </summary>
        /// <param name="functionRestrictor"></param>
        /// <param name="fieldList"></param>
        public AMOrderTypeSelectorAttribute(Type functionRestrictor, params Type[] fieldList) : base(GetQueryCommandByFunction(functionRestrictor), fieldList)
        {
            _DescriptionField = typeof(AMOrderType.descr);
        }

        /// <summary>
        /// Build query by restricting selector to a given order type function
        /// </summary>
        /// <param name="functionRestrictor">OrderTypeFunction BQL Constant</param>
        /// <returns>Composed Bql Command</returns>
        private static Type GetQueryCommandByFunction(Type functionRestrictor)
        {
            if (functionRestrictor == null)
            {
                throw new PXArgumentException("functionRestrictor");
            }

            Type where = BqlCommand.Compose(typeof(Where<,>), typeof(AMOrderType.function), typeof(Equal<>), functionRestrictor);

            return BqlCommand.Compose(typeof(Search<,>), typeof(AMOrderType.orderType), where);
        }
    }
}
