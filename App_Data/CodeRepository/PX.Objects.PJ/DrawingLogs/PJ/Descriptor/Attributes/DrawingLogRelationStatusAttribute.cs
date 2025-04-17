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

using System.Linq;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Data;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    public class DrawingLogRelationStatusAttribute : PXStringListAttribute
    {
        public DrawingLogRelationStatusAttribute()
        {
            var requestForInformationStatusAttribute = new RequestForInformationStatusAttribute();
            var projectIssueStatusAttribute = new ProjectIssueStatusAttribute();
            _AllowedLabels = GetLabels(requestForInformationStatusAttribute, projectIssueStatusAttribute);
            _AllowedValues = GetValues(requestForInformationStatusAttribute, projectIssueStatusAttribute);
        }

        private static string[] GetLabels(PXStringListAttribute requestForInformationStatusAttribute,
            PXStringListAttribute projectIssueStatusAttribute)
        {
            var labels = requestForInformationStatusAttribute.ValueLabelDic.Select(x => x.Value).ToList();
            labels.AddRange(projectIssueStatusAttribute.ValueLabelDic.Select(x => x.Value));
            return labels.Distinct().ToArray();
        }

        private static string[] GetValues(PXStringListAttribute requestForInformationStatusAttribute,
            PXStringListAttribute projectIssueStatusAttribute)
        {
            var values = requestForInformationStatusAttribute.ValueLabelDic.Select(x => x.Key).ToList();
            values.AddRange(projectIssueStatusAttribute.ValueLabelDic.Select(x => x.Key));
            return values.Distinct().ToArray();
        }
    }
}
