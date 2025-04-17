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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common.Abstractions;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote
{
    public static class ComplianceReferenceTypeHelper
    {
        public static string GetValueByKey(Type type, string key)
        {
            return GetTypes().Single(x => x.Type == type && x.Code == key).DisplayValue;
        }

        public static string GetKeyByValue(Type type, string value)
        {
            return GetTypes().Single(x => x.Type == type && x.DisplayValue == value).Code;
        }

        private static IEnumerable<ComplianceReferenceType> GetTypes()
        {
            var types = new List<ComplianceReferenceType>();
            FillTypes<POOrderType.ListAttribute, POOrder>(types);
            FillTypes<ARInvoiceType.ListAttribute, ARInvoice>(types);
            FillTypes<APInvoiceType.ListAttribute, APInvoice>(types);
            FillTypes<APPaymentType.ListAttribute, APPayment>(types);
            FillTypes<ARPaymentType.ListAttribute, ARPayment>(types);
            FillTypes<BatchModule.ListAttribute, PMRegister>(types);
            return types;
        }

        private static void FillTypes<T, TE>(List<ComplianceReferenceType> types)
            where T : PXStringListAttribute
        {
            types.AddRange(Activator.CreateInstance<T>().ValueLabelDic
                .Select(pair => GetComplianceReferenceType(typeof(TE), pair)));
        }

        private static ComplianceReferenceType GetComplianceReferenceType(Type type, KeyValuePair<string, string> pair)
        {
            return new ComplianceReferenceType
            {
                Type = type,
                Code = pair.Key,
                DisplayValue = pair.Value
            };
        }

        public static DocumentKey ConvertToDocumentKey<T>(string clDisplayName)
        {
            var keys = (clDisplayName).Split(',');
            var type = keys[0].Trim();
            var refNumber = keys[1].Trim();

            return new DocumentKey
            (
                ComplianceReferenceTypeHelper.GetKeyByValue(typeof(T), type),
                refNumber
            );
        }
    }
}