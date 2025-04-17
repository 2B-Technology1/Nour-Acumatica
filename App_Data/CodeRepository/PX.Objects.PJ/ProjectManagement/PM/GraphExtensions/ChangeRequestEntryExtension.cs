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

using PX.Objects.PJ.ProjectManagement.PM.CacheExtensions;
using PX.Objects.PJ.ProjectManagement.PM.Services;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;

namespace PX.Objects.PJ.ProjectManagement.PM.GraphExtensions
{
    public class ChangeRequestEntryExtension : PXGraphExtension<ChangeRequestEntry>
    {
        private ConversionServiceBase conversionService;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public virtual void _(Events.RowPersisting<PMChangeRequest> args)
        {
            if (args.Row is PMChangeRequest changeRequest)
            {
                if (args.Operation == PXDBOperation.Delete)
                {
                    conversionService?.UpdateConvertedEntity(changeRequest);
                }
                else
                {
                    conversionService?.ProcessConvertedChangeRequestIfRequired(changeRequest);
                }
            }
        }

        public virtual void _(Events.RowSelected<PMChangeRequest> args)
        {
            if (args.Row is PMChangeRequest changeRequest)
            {
                InitializeConversionService();
                conversionService?.SetFieldReadonly(changeRequest);
                SetProjectFieldEnabled(changeRequest, args.Cache);
            }
        }

        private static void SetProjectFieldEnabled(PMChangeRequest changeRequest, PXCache cache)
        {
            if (cache.GetEnabled<PMChangeRequest.projectID>(changeRequest))
            {
                var isEditable = IsChangeRequestCreatedManually(changeRequest);
                PXUIFieldAttribute.SetEnabled<PMChangeRequest.projectID>(cache, changeRequest, isEditable);
            }
        }

        private static bool IsChangeRequestCreatedManually(PMChangeRequest changeRequest)
        {
            var extension = PXCache<PMChangeRequest>.GetExtension<PmChangeRequestExtension>(changeRequest);
            return extension?.ConvertedFrom == null;
        }

        private void InitializeConversionService()
        {
            var extension = Base.Document.Current?.GetExtension<PmChangeRequestExtension>();
            switch (extension?.ConvertedFrom)
            {
                case nameof(RequestForInformation):
                    conversionService = new RequestForInformationConversionService(Base);
                    break;
                case nameof(ProjectIssue):
                    conversionService = new ProjectIssueConversionService(Base);
                    break;
            }
        }
    }
}