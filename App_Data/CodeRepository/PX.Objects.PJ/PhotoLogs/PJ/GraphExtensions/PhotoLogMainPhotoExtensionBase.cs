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

using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.Services;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.PhotoLogs.PJ.GraphExtensions
{
    public abstract class PhotoLogMainPhotoExtensionBase<TGraph> : PXGraphExtension<TGraph>
        where TGraph : PXGraph
    {
        public SelectFrom<Photo>.View MainPhotoPhotos;

        [InjectDependency]
        public IPhotoLogDataProvider PhotoLogDataProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public IPhotoConfirmationService PhotoConfirmationService
        {
            get;
            set;
        }

        public virtual void _(Events.FieldUpdated<Photo, Photo.isMainPhoto> args)
        {
            if ((bool) args.NewValue)
            {
                var mainPhoto = PhotoLogDataProvider.GetMainPhoto(args.Row.PhotoLogId);
                if (mainPhoto != null)
                {
                    UnmarkPhoto(mainPhoto, args.Row);
                }
            }
        }

        protected void UnmarkPhoto(Photo mainPhoto, Photo currentPhoto)
        {
            var photo = PhotoConfirmationService.IsMarkConfirmed(MainPhotoPhotos)
                ? mainPhoto
                : currentPhoto;
            photo.IsMainPhoto = false;
            MainPhotoPhotos.Cache.Update(photo);
        }
    }
}