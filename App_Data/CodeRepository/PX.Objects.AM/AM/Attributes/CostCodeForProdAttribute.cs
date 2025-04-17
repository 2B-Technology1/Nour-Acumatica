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

namespace PX.Objects.AM.Attributes
{
    [PXDBInt]
    [PXUIField(DisplayName = "Cost Code", FieldClass = COSTCODE)]
    public class CostCodeForProdAttribute : CostCodeAttribute
    {
        public CostCodeForProdAttribute(Type account, Type task, string budgetType) : base(account, task, budgetType, null) { }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var graph = sender.Graph;
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }
            Visible = ProjectHelper.IsPMVisible(sender.Graph);
        }
    }
}
