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
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [Serializable]
    [PXCacheName("Compliance Attribute")]
    public class ComplianceAttribute : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? AttributeId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search<ComplianceAttributeType.complianceAttributeTypeID,
                Where<ComplianceAttributeType.type, NotEqual<ComplianceDocumentType.status>>>),
            SubstituteKey = typeof(ComplianceAttributeType.type))]
        [PXDefault]
        public virtual int? Type
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Value")]
        [PXDefault]
        public virtual string Value
        {
            get;
            set;
        }

        public abstract class attributeId : BqlInt.Field<attributeId>
        {
        }

        public abstract class type : BqlInt.Field<type>
        {
        }

        public abstract class value : BqlString.Field<value>
        {
        }
    }
}