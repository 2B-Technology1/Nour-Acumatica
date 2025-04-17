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
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.Common;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.CS.GraphExtensions
{
    public class CsAttributeMaintExt : PXGraphExtension<CSAttributeMaint>
    {
        protected virtual void CSAttributeDetail_RowDeleting(PXCache cache, PXRowDeletingEventArgs arguments,
            PXRowDeleting baseHandler)
        {
            if (arguments.Row is CSAttributeDetail attributeDetail)
            {
                baseHandler(cache, arguments);
                if (DoesAnyAttributeRelationExist(null, attributeDetail))
                {
                    arguments.Cancel = true;
                }
            }
        }

        protected virtual void CSAttribute_RowSelected(PXCache cache, PXRowSelectedEventArgs arguments,
            PXRowSelected baseHandler)
        {
            if (arguments.Row is CSAttribute attribute)
            {
                baseHandler(cache, arguments);
                cache.AllowDelete = !DoesAnyAttributeRelationExist(attribute, null);
            }
        }

        protected virtual void CSAttribute_ControlType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs arguments,
            PXFieldUpdated baseHandler)
        {
            if (arguments.Row is CSAttribute attribute)
            {
                baseHandler(cache, arguments);
                cache.AllowDelete = !DoesAnyAttributeRelationExist(attribute, null);
            }
        }

        private bool DoesAnyAttributeRelationExist(CSAttribute attribute, CSAttributeDetail attributeDetail)
        {
            var isGroupExist = DoesAttributeGroupExist(attribute);
            var isAttributeExist = attributeDetail == null || DoesAttributeExist(attributeDetail);
            return isAttributeExist && isGroupExist;
        }

        private bool DoesAttributeGroupExist(CSAttribute attribute)
        {
			return new PXSelect<CSAttributeGroup,
					Where<CSAttributeGroup.attributeID, Equal<Required<CSAttributeGroup.attributeID>>>>(Base)
				.Any(attribute?.AttributeID);
		}

        private bool DoesAttributeExist(CSAttributeDetail attributeDetail)
        {
            return new PXSelect<CSAnswers,
                    Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                        And<CSAnswers.value, Equal<Required<CSAnswers.value>>>>>(Base)
                .Any(attributeDetail?.AttributeID, attributeDetail?.ValueID);
        }

        public static bool IsActive()
        {
	        return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }
	}
}
