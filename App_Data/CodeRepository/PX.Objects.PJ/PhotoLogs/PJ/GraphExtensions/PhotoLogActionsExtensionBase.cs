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
using PX.Objects.PJ.PhotoLogs.PJ.Services;
using PX.Data;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.PM;
using Messages = PX.Objects.PM.Messages;

namespace PX.Objects.PJ.PhotoLogs.PJ.GraphExtensions
{
    public abstract class PhotoLogActionsExtensionBase<TGraph, TPrimaryView> : PXGraphExtension<TGraph>
        where TGraph : PXGraph
        where TPrimaryView : class, IBqlTable, new()
    {
        public PXAction<TPrimaryView> DownloadZip;
        public PXAction<TPrimaryView> EmailPhotoLog;

        protected PhotoLogZipServiceBase PhotoLogZipServiceBase;

        [InjectDependency]
        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        [PXUIField(DisplayName = "Export Photo Logs",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true, Category = Messages.Processing)]
        public virtual IEnumerable downloadZip(PXAdapter adapter)
        {
            PhotoLogZipServiceBase.DownloadPhotoLogZip();
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Email Photo Logs",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true, Category = Messages.Processing)]
        public virtual IEnumerable emailPhotoLog(PXAdapter adapter)
        {
            Base.Persist();
            var photoLogZip = PhotoLogZipServiceBase.GetPhotoLogZip();
            var photoLogEmailActivityService = new PhotoLogEmailActivityService(Base, photoLogZip);
            var graph = photoLogEmailActivityService.GetEmailActivityGraph();
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
            return adapter.Get();
        }
    }
}
