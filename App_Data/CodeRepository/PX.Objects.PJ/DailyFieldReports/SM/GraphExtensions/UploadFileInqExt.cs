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

using System.Collections;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.SM.Services;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.DailyFieldReports.SM.GraphExtensions
{
    public class UploadFileInqExt : PXGraphExtension<UploadFileInq>
    {
        public PXAction<FilesFilter> DeleteFile;

        [InjectDependency]
        public IFilesDataProvider FilesDataProvider
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        [PXUIField(DisplayName = "Delete File", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable deleteFile(PXAdapter adapter)
        {
            var fileHasRelatedHistoryRevision =
                FilesDataProvider.DoesFileHaveRelatedHistoryRevision(Base.Files.Current.FileID);
            if (fileHasRelatedHistoryRevision)
            {
                Base.Files.Ask(DailyFieldReportMessages.TheFileIsReferredToTheDailyFieldReport, MessageButtons.OK);
                return adapter.Get();
            }
            return Base.DeleteFile.Press(adapter);
        }
    }
}
