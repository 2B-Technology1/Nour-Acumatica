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

using System.Collections.Generic;
using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PJ.PhotoLogs.PJ.Services;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Services;
using PX.Objects.CS;

namespace PX.Objects.PJ.PhotoLogs.PJ.Graphs
{
    public class PhotoLogSetupMaint : StatusSetupMaintBase<PhotoLogSetupMaint, PhotoLog, PhotoLogStatus>
    {
        public SelectFrom<PhotoLogSetup>.View PhotoLogSetup;

        public SelectFrom<PhotoLogStatus>.View PhotoLogStatuses;

        [PXViewName(PX.Objects.CR.Messages.Attributes)]
        public SelectFrom<CSAttributeGroup>
            .InnerJoin<CSAttribute>.On<CSAttribute.attributeID.IsEqual<CSAttributeGroup.attributeID>>
            .Where<CSAttributeGroup.entityType.IsEqual<Photo.typeName>
                .And<CSAttributeGroup.entityClassID.IsEqual<Photo.photoClassId>>>
            .OrderBy<Asc<CSAttributeGroup.sortOrder>>.View Attributes;

        public SelectFrom<CSAnswers>.
            RightJoin<Photo>.On<CSAnswers.refNoteID.IsEqual<Photo.noteID>>.View Answers;

        public PXSave<PhotoLogSetup> Save;
        public PXCancel<PhotoLogSetup> Cancel;

        private readonly CommonAttributesService commonAttributesService;

        public PhotoLogSetupMaint()
        {
	        commonAttributesService = new CommonAttributesService(this, Attributes);
        }

        [InjectDependency]
        public IPhotoLogDataProvider PhotoLogDataProvider
        {
            get;
            set;
        }

        protected override PXSelectBase<PhotoLogStatus> Statuses => PhotoLogStatuses;

        protected override string DocumentName => PhotoLogMessages.PhotoLogs;

        public virtual void _(Events.RowInserting<CSAttributeGroup> args)
        {
            commonAttributesService.InitializeInsertedAttribute<Photo>(args.Row, Constants.PhotoClassId);
        }

        public virtual void _(Events.RowDeleting<CSAttributeGroup> args)
        {
            commonAttributesService.DeleteAnswersIfRequired<Photo>(args);
        }

        public virtual void _(Events.FieldSelecting<CSAttributeGroup, CSAttributeGroup.defaultValue> args)
        {
            if (args.ReturnState == null)
            {
                return;
            }
            args.ReturnState = commonAttributesService.GetNewReturnState(args.ReturnState, args.Row);
        }

        protected override IEnumerable<PhotoLog> GetDocuments(int? statusId)
        {
            return PhotoLogDataProvider.GetPhotoLogs(statusId);
        }

        protected override PhotoLogStatus GetDefaultStatus()
        {
            return PhotoLogDataProvider.GetDefaultStatus();
        }
    }
}