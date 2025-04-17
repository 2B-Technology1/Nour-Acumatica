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

using PX.Objects.PJ.Common.CacheExtensions;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.DrawingLogs.CR.CacheExtensions;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.Services;
using PX.Objects.PJ.Submittals.PJ.Services;
using PX.Data;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.CR;
using PX.Objects.CS;
using System;
using System.Collections;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;

namespace PX.Objects.PJ.Common.GraphExtensions
{
    public class CrEmailActivityMaintExt : PXGraphExtension<CREmailActivityMaint>
	{
		public PXAction<CRSMEmail> Attach;

		public PXSelect<NoteDoc> AttachedFiles;

		public BaseEmailFileAttachService EmailFileAttachService;

		public delegate IEnumerable SendDelegate(PXAdapter adapter);

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
		}

		public override void Initialize()
		{
			if (IsRequestsForInformationEmail())
			{
				EmailFileAttachService = new RequestForInformationEmailFileAttachService(Base);
			}
			else if (IsDrawingLogEmail())
			{
				EmailFileAttachService = new DrawingLogEmailFileAttachService(Base);
			}
			else if(IsSubmittalEmail())
			{
                EmailFileAttachService = new SubmittalEmailAttachService(Base);
			}
		}

		public IEnumerable attachedFiles()
		{
			return EmailFileAttachService?.GetAttachedFiles();
		}

		[PXUIField(DisplayName = "Attach")]
		[PXButton(Tooltip = RequestForInformationMessages.AttachButtonTooltip)]
		public virtual void attach()
		{
			if (AttachedFiles.AskExt((graph, viewName) => Base.Persist()).IsPositive())
			{
				EmailFileAttachService.MaintainFilesReferences(AttachedFiles);
				Base.Persist();
			}
			Base.Clear();
		}

		public bool IsRequestsForInformationEmail() => typeof(RequestForInformation).IsAssignableFrom(GetParentEntityType(Base.Message.Current));

		public bool IsDrawingLogEmail() => typeof(DrawingLog).IsAssignableFrom(GetParentEntityType(Base.Message.Current));

		public bool IsSubmittalEmail() => typeof(PJSubmittal).IsAssignableFrom(GetParentEntityType(Base.Message.Current));

		public bool IsAttachActionVisible() => EmailFileAttachService != null && Base.Send.GetVisible();

		protected virtual void _(Events.RowSelected<CRSMEmail> args)
		{
			Attach.SetVisible(IsAttachActionVisible());
			Attach.SetEnabled(Base.Send.GetVisible());
		}

        private Type GetParentEntityType(CRSMEmail email)
        {
            if (email == null)
            {
                return null;
            }
            var refNoteId = email.RefNoteID;
            return new EntityHelper(Base).GetEntityRowType(refNoteId);
        }

		private static void SetVisible<TField>(PXCache cache, bool isVisible)
			where TField : IBqlField
		{
			PXUIFieldAttribute.SetVisible<TField>(cache, null, isVisible);
		}

		protected void _(Events.RowSelected<NoteDoc> args)
		{
			bool isDrawingLogEmail = IsDrawingLogEmail();
			bool isRfiEmail = IsRequestsForInformationEmail();
			bool isDraingLogVisible = isDrawingLogEmail || isRfiEmail;
			PXCache cache = args.Cache;

			SetVisible<NoteDocRequestForInformationExt.fileSource>(cache, isRfiEmail || IsSubmittalEmail());
			SetVisible<NoteDocDrawingLogExt.drawingLogCd>(cache, isDraingLogVisible);
			SetVisible<NoteDocDrawingLogExt.number>(cache, isDraingLogVisible);
			SetVisible<NoteDocDrawingLogExt.revision>(cache, isDraingLogVisible);

			if (!isDrawingLogEmail)
			{
				PXUIFieldAttribute.SetVisibility<NoteDocRequestForInformationExt.fileSource>(cache, null, PXUIVisibility.Invisible);
			}

			var file = args.Row;
			if (file != null)
			{
				EmailFileAttachService?.FillRequiredFields(file);
			}
		}

		private CrSmEmailExt GetEmailExtension()
		{
			return PXCache<CRSMEmail>.GetExtension<CrSmEmailExt>(Base.Message.Current);
		}
	}
}