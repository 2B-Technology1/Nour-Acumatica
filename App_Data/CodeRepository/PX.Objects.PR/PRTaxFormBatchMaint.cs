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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Payroll.GovernmentSlips;
using PX.Web.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRTaxFormBatchMaint : PXGraph<PRTaxFormBatchMaint>
	{
		public SelectFrom<PRTaxFormBatch>.View SubmissionDocument;

		public SelectFrom<PREmployeeTaxForm>
			.InnerJoin<PREmployee>.On<PREmployeeTaxForm.employeeID.IsEqual<PREmployee.bAccountID>>
			.Where<PREmployeeTaxForm.batchID.IsEqual<PRTaxFormBatch.batchID.AsOptional>>
			.View BatchEmployees;

		public SelectFrom<PREmployeeTaxFormData>
			.Where<PREmployeeTaxFormData.batchID.IsEqual<PRTaxFormBatch.batchID.FromCurrent>
			.And<PREmployeeTaxFormData.formFileType.IsEqual<P.AsString>>>
			.View EmployeeTaxFormsData;

		[PXCopyPasteHiddenView]
		public PXSetup<PRSetup> PRSetup;
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class SetupValidation : PRSetupValidation<PRTaxFormBatchMaint> { }

		public PXFilter<EmployeeSlipAlreadyPublished> EmployeeSlipAlreadyPublished;

		#region Toolbar buttons
		public PXCancel<PRTaxFormBatch> Cancel;
		public PXDelete<PRTaxFormBatch> Delete; // Action implemented in the 'delete' handler
		public PXFirst<PRTaxFormBatch> First;
		public PXPrevious<PRTaxFormBatch> Previous;
		public PXNext<PRTaxFormBatch> Next;
		public PXLast<PRTaxFormBatch> Last;
		#endregion

		#region Entity Event Handlers
		public PXWorkflowEventHandler<PRTaxFormBatch> OnTaxFormPublished;
		public PXWorkflowEventHandler<PRTaxFormBatch> OnTaxFormUnpublished;
		#endregion

		public PRTaxFormBatchMaint()
		{
			Action.AddMenuAction(DownloadXml);
			Action.AddMenuAction(PrintAll);
			Action.AddMenuAction(PublishAll);
			Action.AddMenuAction(UnpublishAll);
		}

		protected IEnumerable batchEmployees()
		{
			PXView query = new PXView(this, false, BatchEmployees.View.BqlSelect);
			List<PXResult<PREmployeeTaxForm, PREmployee>> result = new List<PXResult<PREmployeeTaxForm, PREmployee>>();

			foreach (PXResult<PREmployeeTaxForm, PREmployee> record in query.SelectMulti())
			{
				PREmployeeTaxForm employeeTaxForm = record;
				PRTaxFormBatch taxFormBatch = SubmissionDocument.Current;
				employeeTaxForm.PublishedFrom = GetEmployeeBatches(this, taxFormBatch.Year, taxFormBatch.OrgBAccountID, employeeTaxForm.EmployeeID, taxFormBatch.FormType)
					.FirstOrDefault(item => item.Published == true)?.BatchID;

				result.Add(record);
			}

			return result;
		}

		#region Actions
		[PXUIField]
		[PXDeleteButton(ConfirmationMessage = Messages.CurrentBatchWillBeDeleted)]
		protected virtual IEnumerable delete(PXAdapter adapter)
		{
			SubmissionDocument.Delete(SubmissionDocument.Current);
			Actions.PressSave();
			return adapter.Get();
		}

		public PXAction<PRTaxFormBatch> Action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(MenuAutoOpen = true)]
		public virtual void action() { }

		public PXAction<PRTaxFormBatch> DownloadXml;
		[PXUIField(DisplayName = "Download XML", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public virtual IEnumerable downloadXml(PXAdapter adapter)
		{
			PRTaxFormBatch submissionDocument = SubmissionDocument.Current;
			PXLongOperation.StartOperation(this, delegate ()
			{
				PRTaxFormBatchMaint graph = PXGraph.CreateInstance<PRTaxFormBatchMaint>();
				graph.SubmissionDocument.Current = submissionDocument;

				PREmployeeTaxFormData[] publishedTaxFormDataRecords =
					graph.EmployeeTaxFormsData.Select(FormFileType.XML).FirstTableItems.ToArray();
				PX.SM.FileInfo fileInfo;
				if (submissionDocument.FormType == GovernmentSlipTypes.T4)
				{
					fileInfo = PRTaxFormGenerator.GetCombinedT4XmlFile(submissionDocument, publishedTaxFormDataRecords);
				}
				else
				{
					fileInfo = PRTaxFormGenerator.GetCombinedRL1XmlFile(submissionDocument, publishedTaxFormDataRecords);
				}

				graph.SubmissionDocument.Current.DownloadedAt = DateTime.UtcNow;
				graph.SubmissionDocument.Update(graph.SubmissionDocument.Current);
				graph.Actions.PressSave();

				throw new PXRedirectToFileException(fileInfo, true);
			});
			return adapter.Get();
		}

		public PXAction<PRTaxFormBatch> PrintAll;
		[PXUIField(DisplayName = "Print All", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public virtual IEnumerable printAll(PXAdapter adapter)
		{
			PRTaxFormBatch submissionDocument = SubmissionDocument.Current;
			PXLongOperation.StartOperation(this, delegate ()
			{
				PRTaxFormBatchMaint graph = PXGraph.CreateInstance<PRTaxFormBatchMaint>();
				graph.SubmissionDocument.Current = submissionDocument;

				PREmployeeTaxFormData[] publishedTaxFormDataRecords =
					graph.EmployeeTaxFormsData.Select(FormFileType.PDF).FirstTableItems.ToArray();
				PX.SM.FileInfo fileInfo = PRTaxFormGenerator.GetCombinedPdfFile(submissionDocument, publishedTaxFormDataRecords);
				throw new PXRedirectToFileException(fileInfo, true);
			});
			return adapter.Get();
		}

		public PXAction<PRTaxFormBatch> PublishAll;
		[PXUIField(DisplayName = "Publish All", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public virtual IEnumerable publishAll(PXAdapter adapter)
		{
			UpdatePublishedFlagForAll(true);
			return adapter.Get();
		}

		public PXAction<PRTaxFormBatch> UnpublishAll;
		[PXUIField(DisplayName = "Unpublish All", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public virtual IEnumerable unpublishAll(PXAdapter adapter)
		{
			UpdatePublishedFlagForAll(false);
			return adapter.Get();
		}

		public PXAction<PRTaxFormBatch> Publish;
		[PXUIField(DisplayName = "Publish", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public virtual IEnumerable publish(PXAdapter adapter)
		{
			if (UpdatePublishedFlag(BatchEmployees.Current, true))
			{
				PRTaxFormBatch.Events
					.Select(e => e.PublishTaxForm)
					.FireOn(this, SubmissionDocument.Current);
				Actions.PressSave();
			}
			return adapter.Get();
		}

		public PXAction<PRTaxFormBatch> Unpublish;
		[PXUIField(DisplayName = "Unpublish", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public virtual IEnumerable unpublish(PXAdapter adapter)
		{
			if (UpdatePublishedFlag(BatchEmployees.Current, false))
			{
				PRTaxFormBatch.Events
					.Select(e => e.UnPublishTaxForm)
					.FireOn(this, SubmissionDocument.Current);
				Actions.PressSave();
			}
			return adapter.Get();
		}

		public PXAction<PRTaxFormBatch> ViewTaxFormBatch;
		[PXUIField(DisplayName = Messages.BatchForSubmission, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual void viewTaxFormBatch()
		{
			PREmployeeTaxForm employeeTaxForm = BatchEmployees.Current;

			PRTaxFormBatchMaint graph = PXGraph.CreateInstance<PRTaxFormBatchMaint>();
			graph.SubmissionDocument.Current = PRTaxFormBatch.PK.Find(this, employeeTaxForm.PublishedFrom);
			throw new PXRedirectRequiredException(graph, true, Messages.BatchForSubmission);
		}
		#endregion Actions

		#region Events Handlers
		protected virtual void _(Events.RowSelected<PRTaxFormBatch> e)
		{
			if (e.Row == null)
			{
				return;
			}

			Delete.SetEnabled(e.Row.EverPublished == false);
		}
		#endregion Events Handlers

		#region Helpers
		public static PREmployeeTaxForm[] GetEmployeeBatches(PXGraph graph, string year, int? orgBAccountID, int? employeeID, string formType)
		{
			PREmployeeTaxForm[] employeeTaxFormRecords = SelectFrom<PREmployeeTaxForm>
				.InnerJoin<PRTaxFormBatch>
					.On<PREmployeeTaxForm.FK.TaxFormBatch>
				.Where<PRTaxFormBatch.year.IsEqual<P.AsString>
					.And<PRTaxFormBatch.orgBAccountID.IsEqual<P.AsInt>>
					.And<PREmployeeTaxForm.employeeID.IsEqual<P.AsInt>>
					.And<PRTaxFormBatch.formType.IsEqual<P.AsString>>>
				.OrderBy<PREmployeeTaxForm.batchID.Desc>
				.View.Select(graph, year, orgBAccountID, employeeID, formType).FirstTableItems.ToArray();

			return employeeTaxFormRecords;
		}

		public virtual bool UpdatePublishedFlag(PREmployeeTaxForm employeeTaxForm, bool published)
		{
			if (employeeTaxForm == null)
			{
				return false;
			}

			if (published)
			{
				PRTaxFormBatch taxFormBatch = SubmissionDocument.Current;
				PREmployeeTaxForm[] alreadyPublishedRecords = GetEmployeeBatches(this, taxFormBatch.Year, taxFormBatch.OrgBAccountID, employeeTaxForm.EmployeeID, taxFormBatch.FormType)
					.Where(record => record.BatchID != employeeTaxForm.BatchID && record.Published == true).ToArray();

				if (alreadyPublishedRecords.Length > 0)
				{
					EmployeeSlipAlreadyPublished.Current.Message = string.Format(Messages.EmployeeSlipAlreadyPublished, alreadyPublishedRecords[0].BatchID);
					if (EmployeeSlipAlreadyPublished.AskExt() != WebDialogResult.OK)
					{
						return false;
					}

					foreach (PREmployeeTaxForm alreadyPublishedRecord in alreadyPublishedRecords)
					{
						alreadyPublishedRecord.Published = false;
						BatchEmployees.Update(alreadyPublishedRecord);
					}
				}

				employeeTaxForm.EverPublished = true;
				SubmissionDocument.Current.EverPublished = true;
				SubmissionDocument.Update(SubmissionDocument.Current);
			}

			employeeTaxForm.Published = published;
			BatchEmployees.Update(employeeTaxForm);
			return true;
		}

		public virtual void UpdatePublishedFlagForAll(bool published)
		{
			bool updated = false;
			foreach (PREmployeeTaxForm employeeTaxForm in BatchEmployees.Select())
			{
				if (UpdatePublishedFlag(employeeTaxForm, published))
				{
					updated = true;
				}
			}
			if (updated)
			{
				Actions.PressSave();
			}
		}

		public virtual string GetTaxFormPdfAsBase64String(int employeeID)
		{
			if (SubmissionDocument.Current?.BatchID == null)
			{
				return null;
			}

			string pdfTaxFormData = PREmployeeTaxFormData.PK.Find(this, SubmissionDocument.Current.BatchID, employeeID, FormFileType.PDF)?.FormData;
			return pdfTaxFormData;
		}

		public PXAction<PRTaxFormBatch> generatePdfDocument;
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton()]
		public virtual IEnumerable GeneratePdfDocument(PXAdapter adapter)
		{
			if (string.IsNullOrWhiteSpace(adapter.CommandArguments) || !int.TryParse(adapter.CommandArguments.ToString(), out int employeeID))
			{
				return adapter.Get();
			}

			string result = GetTaxFormPdfAsBase64String(employeeID);

			PXWebControl webControl = new PXWebControl();
			webControl.ID = "PdfWebControl";
			PXCallbackManager.GetInstance().AddLargeProperty(webControl, "PdfAsString", result);

			return adapter.Get();
		}
		#endregion Helpers
	}

	[Serializable]
	[PXHidden]
	public class EmployeeSlipAlreadyPublished : IBqlTable
	{
		#region Message
		public abstract class message : PX.Data.BQL.BqlString.Field<message> { }
		[PXString]
		public string Message { get; set; }
		#endregion
	}
}
