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
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.CT;

namespace SP.Objects.CR
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class ContractTemplateExt : PXCacheExtension<ContractTemplate>
    {
        [PXDimensionSelector(ContractTemplateAttribute.DimensionName, typeof(Search<ContractTemplate.contractCD, Where<ContractTemplate.isTemplate, Equal<boolTrue>, And<ContractTemplate.baseType, Equal<Contract.ContractBaseType>>>>), typeof(ContractTemplate.contractCD), DescriptionField = typeof(ContractTemplate.description))]
        [PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Contract Template", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFieldDescription]
        public virtual String ContractCD
        {
            get;
            set;
        }
    }
}
