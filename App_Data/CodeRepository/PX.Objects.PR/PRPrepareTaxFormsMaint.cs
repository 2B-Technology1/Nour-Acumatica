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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;
using PX.Objects.TX.DAC.ReportParameters;
using PX.Payroll.GovernmentSlips;
using PX.Web.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace PX.Objects.PR
{
	public class PRPrepareTaxFormsMaint : PXGraph<PRPrepareTaxFormsMaint>
	{
		private const string _stateOfQuebec = "QC";

		private WebDialogResult _ProceedWithNoOriginalTaxFormBatch;

		public override bool IsDirty => false;

		public PXCancel<PRTaxFormsFilter> Cancel;
		public PXFilter<PRTaxFormsFilter> Filter;

		public SelectFrom<PREmployee>
			.OrderBy<PRTaxFormEmployee.hasDiscrepancies.Desc>.ProcessingView.FilteredBy<PRTaxFormsFilter> EmployeesWithPaychecksList;

		public PXFilter<PRTaxFormDiscrepancy> TaxFormDiscrepancies;

		[PXCopyPasteHiddenView]
		public PXSetup<PRSetup> PRSetup;
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class SetupValidation : PRSetupValidation<PRPrepareTaxFormsMaint> { }

		protected IEnumerable employeesWithPaychecksList()
		{
			int[] branchIDs = PRTaxFormGenerator.GetBranchIDsByBusinessAccount(Filter.Current.OrgBAccountID);
			PREmployee[] result = null;

			if (Filter.Current.TaxForm == GovernmentSlipTypes.T4)
			{
				result = SelectFrom<PREmployee>
				.InnerJoin<PRPayment>
					.On<PREmployee.bAccountID.IsEqual<PRPayment.employeeID>>
				.Where<PRPayment.branchID.IsIn<P.AsInt>
					.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
					.And<PRPayment.released.IsEqual<True>
					.And<PRPayment.voided.IsEqual<False>>>
					.And<Where<DatePart<DatePart.year, PRPayment.transactionDate>, Equal<PRTaxFormsFilter.year.FromCurrent>>>>
				.AggregateTo<GroupBy<PREmployee.bAccountID>>.View.Select(this, branchIDs).FirstTableItems.ToArray();
			}
			else if (Filter.Current.TaxForm == GovernmentSlipTypes.RL1)
			{
				PREmployee[] employeeWithBranchInQuebec = EmployeesWithAtLeastOneReleasedPayCheckAndBranchInQuebec(branchIDs);
				PREmployee[] employeesWithWorkLocationInQuebec = EmployeesWithAtLeastOneReleasedPayCheckAndWorkLocInQuebec(branchIDs);

				result = employeeWithBranchInQuebec.Union(employeesWithWorkLocationInQuebec, new PREmployeeEqualityComparer()).ToArray();
			}

			foreach (PREmployee employee in result)
			{
				PRTaxFormEmployee taxFormEmployee = PXCache<PREmployee>.GetExtension<PRTaxFormEmployee>(employee);
				PREmployeeTaxForm[] batchSubmissions = PRTaxFormBatchMaint.GetEmployeeBatches(this, Filter.Current.Year, Filter.Current.OrgBAccountID, employee.BAccountID, Filter.Current.TaxForm);
				PREmployeeTaxForm publishedFromBatch = batchSubmissions.FirstOrDefault(item => item.Published == true);

				string payGroupID = PREmployee.PK.Find(this, employee.BAccountID)?.PayGroupID;
				taxFormEmployee.PayGroupName = payGroupID;
				
				if (publishedFromBatch?.BatchID != null)
				{
					taxFormEmployee.PublishedFrom = publishedFromBatch.BatchID;
					taxFormEmployee.DocType = PRTaxFormBatch.PK.Find(this, publishedFromBatch.BatchID)?.DocType;
				}

				taxFormEmployee.BatchID = string.Join(";", batchSubmissions.Select(item => item.BatchID).ToArray());
				taxFormEmployee.Warning = null;

				if (Filter.Current.Operation == PRTaxFormsFilter.operation.Correct && batchSubmissions.Length == 0)
				{
					if (Filter.Current.DocType == TaxFormBatchType.Amendment)
					{
						taxFormEmployee.Warning = Messages.NoOriginalTaxFormBatchForAmendment;
					}
					else if (Filter.Current.DocType == TaxFormBatchType.Cancellation)
					{
						taxFormEmployee.Warning = Messages.NoOriginalTaxFormBatchForCancellation;
					}
				}

				if (employee.BAccountID == Filter.Current.SelectedEmployeeID)
				{
					employee.Selected = true;
				}
				EmployeesWithPaychecksList.Cache.Update(employee);
			}

			return result;
		}

		protected IEnumerable taxFormDiscrepancies()
		{
			List<PRTaxFormDiscrepancy> discrepancies = new List<PRTaxFormDiscrepancy>();

			PREmployee employee = EmployeesWithPaychecksList.Current;

			if (employee != null)
			{
				PREmployeeTaxFormData taxFormData = SelectFrom<PREmployeeTaxFormData>
					.InnerJoin<PRTaxFormBatch>.On<PREmployeeTaxFormData.batchID.IsEqual<PRTaxFormBatch.batchID>>
					.Where<PREmployeeTaxFormData.employeeID.IsEqual<P.AsInt>
						.And<PREmployeeTaxFormData.formFileType.IsEqual<P.AsString>>
						.And<PRTaxFormBatch.year.IsEqual<P.AsString>>
						.And<PRTaxFormBatch.formType.IsEqual<P.AsString>>>
					.OrderBy<PREmployeeTaxFormData.createdDateTime.Desc>
					.View.Select(this, employee.BAccountID, FormFileType.XML, Filter.Current.Year, Filter.Current.TaxForm).FirstTableItems.FirstOrDefault();

				if (taxFormData != null)
				{
					string oldXmlAsString = taxFormData.FormData;
					// Acuminator disable once PX1084 GraphCreationInDataViewDelegate [needed to generate tax form XML]
					// Acuminator disable once PX1083 SavingChangesInDataViewDelegate [needed to generate tax form XML]
					string newXmlAsString = GetTaxFormXml((int)employee.BAccountID);

					XmlDocument oldXml = new XmlDocument();
					oldXml.LoadXml(oldXmlAsString);
					XmlDocument newXml = new XmlDocument();
					newXml.LoadXml(newXmlAsString);

					bool isT4 = Filter.Current.TaxForm == GovernmentSlipTypes.T4;

					IEnumerable<PropertyInfo> t4properties = null;

					if (isT4)
					{
						t4properties = PRTaxFormGenerator.GetT4Properties();
					}

					XmlNodeList amounts = oldXml.GetElementsByTagName(isT4 ? PRTaxFormGenerator.T4AmountsTag : PRTaxFormGenerator.RL1AmountsTag);

					if (amounts.Any_())
					{
						foreach (XmlElement oldElem in amounts[0].ChildNodes)
						{
							XmlElement newElem = (XmlElement)newXml.GetElementsByTagName(oldElem.Name)[0];
							bool elemIsBoxO = oldElem.Name == RL1BoxNames.RL1BoxO;
							string oldBoxOValue = null;
							string newBoxOValue = null;
							if (elemIsBoxO)
							{
								oldBoxOValue = oldElem.GetElementsByTagName(RL1BoxNames.RL1BoxOAmount)[0].FirstChild.Value;
								newBoxOValue = newElem.GetElementsByTagName(RL1BoxNames.RL1BoxOAmount)[0].FirstChild.Value;
							}
							if (elemIsBoxO && oldBoxOValue != newBoxOValue || !elemIsBoxO && oldElem.FirstChild?.Value != newElem.FirstChild?.Value)
							{
								PRTaxFormDiscrepancy discrepancy = new PRTaxFormDiscrepancy();
								discrepancy.EmployeeID = employee.AcctName;
								discrepancy.Box = isT4
									? t4properties.Where(p => p.GetCustomAttribute<T4XmlNodeAttribute>().NodeName == oldElem.Name).FirstOrDefault().GetCustomAttribute<T4XmlNodeAttribute>().BoxName
									: oldElem.Name;
								discrepancy.OldValue = elemIsBoxO ? oldBoxOValue : oldElem.FirstChild.Value;
								discrepancy.NewValue = elemIsBoxO ? newBoxOValue : newElem.FirstChild.Value;
								discrepancies.Add(discrepancy);
							}
						}
					}
				}
			}

			return discrepancies;
		}

		#region Events Handlers
		protected virtual void _(Events.RowSelected<PRTaxFormsFilter> e)
		{
			PRTaxFormsFilter filter = Filter.Current;

			if (filter.Operation == PRTaxFormsFilter.operation.Prepare)
			{
				EmployeesWithPaychecksList.SetProcessAllCaption(Messages.Prepare);
			}
			else if (filter.Operation == PRTaxFormsFilter.operation.Correct)
			{
				EmployeesWithPaychecksList.SetProcessCaption(Messages.Correct);
				EmployeesWithPaychecksList.SetProcessAllCaption(Messages.CorrectAll);
			}

			bool hasDiscrepanciesIsVisible = filter.Operation == PRTaxFormsFilter.operation.Correct && filter.IsCheckingDiscrepancies == true;

			EmployeesWithPaychecksList.SetProcessEnabled(filter.IncludeUnreleasedPaychecks != true && filter.Operation == PRTaxFormsFilter.operation.Correct);
			EmployeesWithPaychecksList.SetProcessVisible(filter.IncludeUnreleasedPaychecks != true && filter.Operation == PRTaxFormsFilter.operation.Correct);
			EmployeesWithPaychecksList.SetProcessAllEnabled(filter.IncludeUnreleasedPaychecks != true);
			PXUIFieldAttribute.SetVisibility<PREmployee.selected>(EmployeesWithPaychecksList.Cache, null, filter.Operation == PRTaxFormsFilter.operation.Correct ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisible<PREmployee.selected>(EmployeesWithPaychecksList.Cache, null, filter.Operation == PRTaxFormsFilter.operation.Correct);
			PXUIFieldAttribute.SetVisibility<PRTaxFormEmployee.hasDiscrepancies>(EmployeesWithPaychecksList.Cache, null, hasDiscrepanciesIsVisible ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisible<PRTaxFormEmployee.hasDiscrepancies>(EmployeesWithPaychecksList.Cache, null, hasDiscrepanciesIsVisible);

			EmployeesWithPaychecksList.SetProcessDelegate(list => ProcessTaxForms(list, filter));
		}

		protected virtual void _(Events.FieldVerifying<PRTaxFormsFilter, PRTaxFormsFilter.operation> e)
		{
			if (e.NewValue as string == PRTaxFormsFilter.operation.Correct && CheckExistingBatches())
			{
				e.NewValue = e.OldValue;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldVerifying<PRTaxFormsFilter, PRTaxFormsFilter.year> e)
		{
			if (Filter.Current.Operation == PRTaxFormsFilter.operation.Correct)
			{
				Filter.Current.Year = e.NewValue as string;

				if (CheckExistingBatches())
				{
					Filter.Current.Year = null;

					e.NewValue = null;
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PRTaxFormsFilter, PRTaxFormsFilter.orgBAccountID> e)
		{
			if (Filter.Current.Operation == PRTaxFormsFilter.operation.Correct)
			{
				Filter.Current.OrgBAccountID = e.NewValue as int?;

				if (CheckExistingBatches())
				{
					Filter.Current.OrgBAccountID = null;

					e.NewValue = null;
					e.Cancel = true;
				}
			}

			Organization organization = SelectFrom<Organization>.Where<Organization.bAccountID.IsEqual<P.AsInt>>.View.Select(this, e.NewValue as int?);
			if (organization != null)
			{
				if (organization.FileTaxesByBranches == true)
				{
					e.NewValue = e.OldValue;
					e.Cancel = true;
				}
			}
			else
			{
				Branch branch = SelectFrom<Branch>.Where<Branch.bAccountID.IsEqual<P.AsInt>>.View.Select(this, e.NewValue as int?);
				if (branch != null)
				{
					organization = Organization.PK.Find(this, branch.OrganizationID);
					if (organization.FileTaxesByBranches == false)
					{
						e.NewValue = e.OldValue;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PRTaxFormsFilter, PRTaxFormsFilter.operation> e)
		{
			IEnumerable<PREmployee> employees = EmployeesWithPaychecksList.Select().FirstTableItems;

			if (e.NewValue as string == PRTaxFormsFilter.operation.Correct
				&& employees.Any()
				&& (_ProceedWithNoOriginalTaxFormBatch == WebDialogResult.Yes || TaxFormDiscrepancies.View.Ask(null, Messages.ConfirmationHeader, Messages.CheckDiscrepancies, MessageButtons.YesNo, PRPayChecksAndAdjustments.ConfirmCancelButtons, MessageIcon.Question) == WebDialogResult.Yes))
			{
				Filter.Current.IsCheckingDiscrepancies = true;
				CheckDiscrepancies();
			}
			else
			{
				Filter.Current.IsCheckingDiscrepancies = false;
				foreach (PREmployee employee in employees)
				{
					PRTaxFormEmployee taxFormEmployee = PXCache<PREmployee>.GetExtension<PRTaxFormEmployee>(employee);
					taxFormEmployee.HasDiscrepancies = false;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PRTaxFormsFilter, PRTaxFormsFilter.year> e)
		{
			PRTaxFormsFilter filter = Filter.Current;

			if (filter.Operation == PRTaxFormsFilter.operation.Correct && filter.IsCheckingDiscrepancies == true)
			{
				CheckDiscrepancies();
			}
		}

		protected virtual void _(Events.FieldUpdated<PRTaxFormsFilter, PRTaxFormsFilter.orgBAccountID> e)
		{
			PRTaxFormsFilter filter = Filter.Current;

			if (filter.Operation == PRTaxFormsFilter.operation.Correct && filter.IsCheckingDiscrepancies == true)
			{
				CheckDiscrepancies();
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployee, PREmployee.selected> e)
		{
			if (e.NewValue as bool? == false && e.Row?.BAccountID == Filter.Current.SelectedEmployeeID)
			{
				Filter.Current.SelectedEmployeeID = null;
			}
		}

		protected virtual void _(Events.RowSelected<PREmployee> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PRTaxFormEmployee taxFormEmployee = PXCache<PREmployee>.GetExtension<PRTaxFormEmployee>(e.Row);
			PXUIFieldAttribute.SetWarning<PREmployee.acctCD>(e.Cache, e.Row, taxFormEmployee.Warning);
		}
		#endregion Events Handlers

		protected static void ProcessTaxForms(List<PREmployee> list, PRTaxFormsFilter filter)
		{
			try
			{
				filter.IsBatchCreationInProgress = true;
				if (list.Count == 0 ||
					filter.Operation != PRTaxFormsFilter.operation.Prepare && filter.Operation != PRTaxFormsFilter.operation.Correct ||
					filter.Operation == PRTaxFormsFilter.operation.Correct && !list.Any(item => item.Selected == true))
				{
					return;
				}

				PRTaxFormBatchMaint taxFormBatchMaint = PXGraph.CreateInstance<PRTaxFormBatchMaint>();

				PRTaxFormBatch taxFormBatch = new PRTaxFormBatch();
				taxFormBatch.FormType = filter.TaxForm;
				taxFormBatch.DocType = filter.Operation == PRTaxFormsFilter.operation.Prepare ? TaxFormBatchType.Original : filter.DocType;
				taxFormBatch.Year = filter.Year;
				taxFormBatch.OrgBAccountID = filter.OrgBAccountID;

				taxFormBatchMaint.SubmissionDocument.Insert(taxFormBatch);

				int year = int.Parse(filter.Year);
				string[] fileTypes = { FormFileType.PDF, FormFileType.XML };
				bool includeUnreleasedPaychecks = filter.IncludeUnreleasedPaychecks ?? false;
				string reportingTypeCode = taxFormBatch.DocType;

				foreach (PREmployee employee in list)
				{
					try
					{
						PXProcessing.SetCurrentItem(employee);
						if (filter.Operation == PRTaxFormsFilter.operation.Correct && employee.Selected != true)
						{
							PXProcessing.SetProcessed();
							continue;
						}

						PREmployeeTaxForm employeeTaxForm = new PREmployeeTaxForm();
						employeeTaxForm.EmployeeID = employee.BAccountID;
						taxFormBatchMaint.BatchEmployees.Insert(employeeTaxForm);

						TaxFormRequestParameters parameters = PRPrepareTaxFormsMaint.GetTaxFormRequestParameters(filter.TaxForm, fileTypes, year, taxFormBatch.OrgBAccountID, employee.BAccountID, includeUnreleasedPaychecks, reportingTypeCode);

						List<TaxFormFile> taxFormFiles =
							PRTaxFormGenerator.GetTaxFormFiles(parameters);

						foreach (TaxFormFile taxFormFile in taxFormFiles)
						{
							PREmployeeTaxFormData employeeTaxFormPdfData = new PREmployeeTaxFormData();
							employeeTaxFormPdfData.EmployeeID = employee.BAccountID;
							employeeTaxFormPdfData.FormFileType = taxFormFile.FileType;
							employeeTaxFormPdfData.FormData = taxFormFile.Data;
							taxFormBatchMaint.EmployeeTaxFormsData.Insert(employeeTaxFormPdfData);
						}
						PXProcessing.SetProcessed();
					}
					catch (Exception exception)
					{
						PXProcessing.SetError(list.IndexOf(employee), exception.Message);
						throw;
					}
				}

				taxFormBatchMaint.Persist();

				throw new PXRedirectRequiredException(taxFormBatchMaint, false, Messages.BatchForSubmission);
			}

			finally
			{
				filter.IsBatchCreationInProgress = false;
			}
		}

		protected virtual bool CheckExistingBatches()
		{
			foreach (PREmployee employee in EmployeesWithPaychecksList.Select())
			{
				PRTaxFormEmployee taxFormEmployee = PXCache<PREmployee>.GetExtension<PRTaxFormEmployee>(employee);

				if (string.IsNullOrWhiteSpace(taxFormEmployee.BatchID))
				{
					_ProceedWithNoOriginalTaxFormBatch = Filter.View.Ask(null, Messages.ConfirmationHeader, Messages.NoOriginalTaxFormBatchForCorrectAction, MessageButtons.YesNo, PRPayChecksAndAdjustments.ConfirmCancelButtons, MessageIcon.Question);
					if (_ProceedWithNoOriginalTaxFormBatch != WebDialogResult.Yes)
					{
						return true;
					}

					return false;
				}
			}

			return false;
		}

		#region Actions
		public PXAction<PRTaxFormBatch> ViewTaxFormBatch;
		[PXUIField(DisplayName = Messages.BatchForSubmission, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		protected virtual void viewTaxFormBatch()
		{
			PREmployee payrollEmployee = EmployeesWithPaychecksList.Current;
			PRTaxFormEmployee taxFormEmployee = PXCache<PREmployee>.GetExtension<PRTaxFormEmployee>(payrollEmployee);

			PRTaxFormBatchMaint graph = PXGraph.CreateInstance<PRTaxFormBatchMaint>();
			graph.SubmissionDocument.Current = PRTaxFormBatch.PK.Find(this, taxFormEmployee.PublishedFrom);
			throw new PXRedirectRequiredException(graph, true, Messages.BatchForSubmission);
		}

		public PXAction<PRTaxFormBatch> ViewDiscrepancies;
		[PXUIField(DisplayName = Messages.ViewDiscrepancies, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		protected virtual void viewDiscrepancies()
		{
			TaxFormDiscrepancies.View.AskExt();
		}
		#endregion

		public virtual string GetTaxFormPdfAsBase64String(int employeeID)
		{
			if (Filter.Current == null || Filter.Current.IsBatchCreationInProgress == true)
			{
				return null;
			}

			int year = int.Parse(Filter.Current.Year);
			int? orgBaccountID = Filter.Current.OrgBAccountID;
			bool includeUnreleasedPaychecks = Filter.Current.IncludeUnreleasedPaychecks ?? false;
			string reportingTypeCode = Filter.Current.Operation == PRTaxFormsFilter.operation.Prepare ? TaxFormBatchType.Original : Filter.Current.DocType;

			TaxFormRequestParameters parameters = GetTaxFormRequestParameters(Filter.Current.TaxForm, new[] { FormFileType.PDF }, year, orgBaccountID, employeeID, includeUnreleasedPaychecks, reportingTypeCode);

			List<TaxFormFile> taxFormFile = PRTaxFormGenerator.GetTaxFormFiles(parameters);
			return taxFormFile[0].Data;
		}

		public virtual string GetTaxFormXml(int employeeID)
		{
			if (Filter.Current == null || Filter.Current.IsBatchCreationInProgress == true)
			{
				return null;
			}

			string formType = Filter.Current.TaxForm;
			int year = int.Parse(Filter.Current.Year);
			int? orgBaccountID = Filter.Current.OrgBAccountID;
			bool includeUnreleasedPaychecks = Filter.Current.IncludeUnreleasedPaychecks ?? false;
			TaxFormRequestParameters parameters =
				new TaxFormRequestParameters(formType, new[] { FormFileType.XML }, year, orgBaccountID, employeeID, includeUnreleasedPaychecks);
			List<TaxFormFile> taxFormFile = PRTaxFormGenerator.GetTaxFormFiles(parameters);
			return taxFormFile[0].Data;
		}

		public virtual void CheckDiscrepancies()
		{
			IEnumerable<PREmployee> employees = EmployeesWithPaychecksList.Select().FirstTableItems;

			int[] branchIDs = PRTaxFormGenerator.GetBranchIDsByBusinessAccount(Filter.Current.OrgBAccountID);

			foreach (PREmployee employee in employees)
			{
				PRTaxFormEmployee taxFormEmployee = PXCache<PREmployee>.GetExtension<PRTaxFormEmployee>(employee);

				if (!string.IsNullOrWhiteSpace(taxFormEmployee.BatchID))
				{
					PREmployeeTaxForm batchSubmission = PRTaxFormBatchMaint.GetEmployeeBatches(this, Filter.Current.Year, Filter.Current.OrgBAccountID, employee.BAccountID, Filter.Current.TaxForm).FirstOrDefault();

					if (batchSubmission != null)
					{
						PRPayment latestReleasedPayment = SelectFrom<PRPayment>
							.Where<PRPayment.branchID.IsIn<P.AsInt>
								.And<PRPayment.employeeID.IsEqual<P.AsInt>>
								.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
								.And<PRPayment.released.IsEqual<True>>
								.And<PRPayment.voided.IsEqual<False>>
								.And<PRPayment.grossAmount.IsNotEqual<decimal0>>
								.And<Where<DatePart<DatePart.year, PRPayment.transactionDate>, Equal<PRTaxFormsFilter.year.FromCurrent>>>>
							.OrderBy<PRPayment.lastModifiedDateTime.Desc>.View.Select(this, branchIDs, employee.BAccountID).FirstTableItems.FirstOrDefault();

						if (latestReleasedPayment != null)
						{
							taxFormEmployee.HasDiscrepancies = latestReleasedPayment.LastModifiedDateTime > batchSubmission.CreatedDateTime;
							employee.Selected = taxFormEmployee.HasDiscrepancies;
						}
					}
				}
			}
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

		#region Helpers
		public static TaxFormRequestParameters GetTaxFormRequestParameters(string taxForm, string[] fileTypes, int year, int? orgBaccountID, int? employeeID, bool includeUnreleasedPaychecks, string reportingTypeCode)
		{
			switch (taxForm)
			{
				case GovernmentSlipTypes.T4:
				case GovernmentSlipTypes.RL1:
					return new TaxFormRequestParameters(taxForm, fileTypes, year, orgBaccountID, employeeID, includeUnreleasedPaychecks, reportingTypeCode);
				default:
					throw new PXException(Messages.TaxFormIsNotSupported, taxForm);
			}
		}

		public static void CheckCancellationTaxForm(PXView view, PRPayment payment)
		{
			if (payment.DocType == PayrollType.VoidCheck)
			{
				PRPrepareTaxFormsMaint graph = PXGraph.CreateInstance<PRPrepareTaxFormsMaint>();
				PXAccess.MasterCollection.Branch branch = PXAccess.GetBranch(payment.BranchID);

				PXResult<PREmployeeTaxForm, PRTaxFormBatch> existingTaxForm = (PXResult<PREmployeeTaxForm, PRTaxFormBatch>)SelectFrom<PREmployeeTaxForm>
					.InnerJoin<PRTaxFormBatch>.On<PREmployeeTaxForm.FK.TaxFormBatch>
					.Where<PREmployeeTaxForm.employeeID.IsEqual<P.AsInt>
						.And<PRTaxFormBatch.formType.IsEqual<P.AsString>>
						.And<PRTaxFormBatch.orgBAccountID.IsEqual<P.AsInt>
							.Or<PRTaxFormBatch.orgBAccountID.IsEqual<P.AsInt>>>
						.And<PRTaxFormBatch.year.IsEqual<P.AsString>>>.View
					.Select(graph, payment.EmployeeID, GovernmentSlipTypes.T4, branch.BAccountID, branch.Organization.BAccountID, payment.TransactionDate.Value.Year.ToString())
					.FirstOrDefault();

				PRTaxFormBatch taxFormBatch = existingTaxForm;
				PREmployeeTaxForm employeeTaxForm = existingTaxForm;

				if (existingTaxForm != null)
				{
					PXResult<PREmployeeTaxForm, PRTaxFormBatch> cancellationTaxForm = (PXResult<PREmployeeTaxForm, PRTaxFormBatch>)SelectFrom<PREmployeeTaxForm>
						.InnerJoin<PRTaxFormBatch>.On<PREmployeeTaxForm.FK.TaxFormBatch>
						.Where<PREmployeeTaxForm.employeeID.IsEqual<P.AsInt>
							.And<PRTaxFormBatch.formType.IsEqual<P.AsString>>
							.And<PRTaxFormBatch.docType.IsEqual<P.AsString>>
							.And<PRTaxFormBatch.orgBAccountID.IsEqual<P.AsInt>>
							.And<PRTaxFormBatch.year.IsEqual<P.AsString>>>.View
						.Select(graph, employeeTaxForm.EmployeeID, taxFormBatch.FormType, TaxFormBatchType.Cancellation, taxFormBatch.OrgBAccountID, taxFormBatch.Year)
						.FirstOrDefault();

					if (cancellationTaxForm == null && view.Ask(null, Messages.ConfirmationHeader, Messages.CreateCancellationT4, MessageButtons.YesNo, PRPayChecksAndAdjustments.ConfirmCancelButtons, MessageIcon.Question) == WebDialogResult.Yes)
					{
						graph.Filter.Current.Year = payment.TransactionDate.Value.Year.ToString();
						graph.Filter.Current.OrgBAccountID = taxFormBatch.OrgBAccountID;
						graph.Filter.Current.DocType = TaxFormBatchType.Cancellation;
						graph.Filter.Current.Operation = PRPrepareTaxFormsMaint.PRTaxFormsFilter.operation.Correct;
						graph.Filter.Current.TaxForm = GovernmentSlipTypes.T4;
						graph.Filter.Current.SelectedEmployeeID = employeeTaxForm.EmployeeID;

						throw new PXRedirectRequiredException(graph, true, Messages.PrepareCancellationTaxForm);
					}
				}
			}
		}

		public virtual PREmployee[] EmployeesWithAtLeastOneReleasedPayCheckAndBranchInQuebec(int[] branchIDs)
		{
			return SelectFrom<PREmployee>
				.InnerJoin<PRPayment>
					.On<PRPayment.FK.Employee>
				.InnerJoin<PRPaymentEarning>
					.On<PRPaymentEarning.FK.Payment>
				.InnerJoin<PRLocation>
					.On<PRPaymentEarning.FK.Location>
				.InnerJoin<Address>
					.On<PRLocation.FK.Address>
				.Where<PRPayment.branchID.IsIn<P.AsInt>
					.And<PRPayment.released.IsEqual<True>
					.And<PRPayment.voided.IsEqual<False>>>
					.And<Address.state.IsEqual<P.AsString>>
					.And<Where<DatePart<DatePart.year, PRPayment.transactionDate>, Equal<PRTaxFormsFilter.year.FromCurrent>>>>
				.AggregateTo<GroupBy<PREmployee.bAccountID>>.View.Select(this, branchIDs, _stateOfQuebec).FirstTableItems.ToArray();
		}

		public virtual PREmployee[] EmployeesWithAtLeastOneReleasedPayCheckAndWorkLocInQuebec(int[] branchIDs)
		{
			return SelectFrom<PREmployee>
				.InnerJoin<PRPayment>
					.On<PRPayment.FK.Employee>
				.InnerJoin<Branch>
					.On<PRPayment.FK.Branch>
				.InnerJoin<BAccount>
					.On<Branch.FK.BAccount>
				.InnerJoin<Address>
					.On<BAccount.FK.Address>
				.Where<PRPayment.branchID.IsIn<P.AsInt>
					.And<PRPayment.released.IsEqual<True>
					.And<PRPayment.voided.IsEqual<False>>>
					.And<Address.state.IsEqual<P.AsString>>
					.And<Where<DatePart<DatePart.year, PRPayment.transactionDate>, Equal<PRTaxFormsFilter.year.FromCurrent>>>>
				.AggregateTo<GroupBy<PREmployee.bAccountID>>.View.Select(this, branchIDs, _stateOfQuebec).FirstTableItems.ToArray();
		}
		#endregion Helpers

		[PXCacheName(Messages.PRTaxFormsFilter)]
		[Serializable]
		public class PRTaxFormsFilter : IBqlTable
		{
			#region Year
			public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
			[PXString(4)]
			[PXUIField(DisplayName = Messages.Year)]
			[PXSelector(typeof(SelectFrom<PRPayGroupYear>
				.AggregateTo<GroupBy<PRPayGroupYear.year>>
				.OrderBy<PRPayGroupYear.year.Desc>
				.SearchFor<PRPayGroupYear.year>))]
			public string Year { get; set; }
			#endregion
			#region OrgBAccountID
			public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }
			[OrganizationTree(
				treeDataMember: typeof(TaxTreeSelect),
				onlyActive: true,
				SelectionMode = OrganizationTreeAttribute.SelectionModes.Branches)]
			public virtual int? OrgBAccountID { get; set; }
			#endregion
			#region IncludeUnreleasedPaychecks
			public abstract class includeUnreleasedPaychecks : PX.Data.BQL.BqlBool.Field<includeUnreleasedPaychecks> { }
			[PXBool]
			[PXUIField(DisplayName = "Include Data from Unreleased Paychecks")]
			public bool? IncludeUnreleasedPaychecks { get; set; }
			#endregion
			#region Operation
			public abstract class operation : PX.Data.BQL.BqlString.Field<operation>
			{
				public const string Prepare = "PRP";
				public const string Correct = "CRT";

				public class prepare : PX.Data.BQL.BqlString.Constant<prepare>
				{
					public prepare() : base(Prepare) { }
				}

				public class correct : PX.Data.BQL.BqlString.Constant<correct>
				{
					public correct() : base(Correct) { }
				}
			}
			[PXString(3, IsFixed = true)]
			[PXUnboundDefault(operation.Prepare)]
			[PXUIField(DisplayName = "Action")]
			[PXStringList(
				new string[] { operation.Prepare, operation.Correct },
				new string[] { Messages.Prepare, Messages.Correct })]
			public string Operation { get; set; }
			#endregion
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXString(1, IsFixed = true)]
			[PXUnboundDefault(TaxFormBatchType.Amendment)]
			[PXUIField(DisplayName = "Type")]
			[TaxFormBatchType.List]
			[PXUIVisible(typeof(Where<operation.IsEqual<operation.correct>>))]
			public string DocType { get; set; }
			#endregion
			#region IsBatchCreationInProgress
			public abstract class isBatchCreationInProgress : PX.Data.BQL.BqlBool.Field<isBatchCreationInProgress> { }
			[PXBool]
			public bool? IsBatchCreationInProgress { get; set; }
			#endregion
			#region TaxForm
			public abstract class taxForm : PX.Data.BQL.BqlString.Field<taxForm> { }
			/// <summary>
			/// The Tax Form identifier.
			/// </summary>
			[PXString(3, IsFixed = false)]
			[PXUnboundDefault(GovernmentSlipTypes.T4)]
			[PXUIField(DisplayName = "Tax Form")]
			[PXStringList(
				new string[] { GovernmentSlipTypes.T4, GovernmentSlipTypes.RL1 },
				new string[] { Messages.T4Form, Messages.RL1Form })]
			public string TaxForm { get; set; }
			#endregion
			#region IsCheckingDiscrepancies
			public abstract class isCheckingDiscrepancies : PX.Data.BQL.BqlBool.Field<isCheckingDiscrepancies> { }
			/// <summary>
			/// Indicates (if set to <see langword="true" />) that discrepancies between the current tax form and the previous one are currently being checked.
			/// </summary>
			[PXBool]
			public bool? IsCheckingDiscrepancies { get; set; }
			#endregion
			#region SelectedEmployeeID
			public abstract class selectedEmployeeID : PX.Data.BQL.BqlInt.Field<selectedEmployeeID> { }
			/// <summary>
			/// The selected Employee ID.
			/// </summary>
			[PXInt]
			public int? SelectedEmployeeID { get; set; }
			#endregion
		}

		[PXHidden]
		[Serializable]
		public class PRTaxFormDiscrepancy : IBqlTable
		{
			#region EmployeeID
			public abstract class employeeID : PX.Data.BQL.BqlString.Field<employeeID> { }
			[PXString(IsKey = true)]
			[PXUIField(DisplayName = "Employee")]
			public virtual string EmployeeID { get; set; }
			#endregion
			#region Box
			public abstract class box : PX.Data.BQL.BqlString.Field<box> { }
			[PXString(IsKey = true)]
			[PXUIField(DisplayName = "Box")]
			public virtual string Box { get; set; }
			#endregion
			#region OldValue
			public abstract class oldValue : PX.Data.BQL.BqlString.Field<oldValue> { }
			[PXString]
			[PXUIField(DisplayName = "Old Value")]
			public virtual string OldValue { get; set; }
			#endregion
			#region NewValue
			public abstract class newValue : PX.Data.BQL.BqlString.Field<newValue> { }
			[PXString]
			[PXUIField(DisplayName = "New Value")]
			public virtual string NewValue { get; set; }
			#endregion
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		[Serializable]
		[PXHidden]
		public sealed class PRTaxFormEmployee : PXCacheExtension<PREmployee>
		{
			#region PayGroupName
			public abstract class payGroupName : BqlString.Field<payGroupName> { }
			[PXString]
			[PXUIField(DisplayName = "Pay Group")]
			public string PayGroupName { get; set; }
			#endregion
			#region PublishedFrom
			public abstract class publishedFrom : PX.Data.BQL.BqlBool.Field<publishedFrom> { }
			[PXUIField(DisplayName = "Published From", Enabled = false)]
			public string PublishedFrom { get; set; }
			#endregion
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Document Type", Enabled = false)]
			[TaxFormBatchType.List]
			public string DocType { get; set; }
			#endregion
			#region BatchID
			public abstract class batchID : PX.Data.BQL.BqlString.Field<batchID> { }
			[PXUIField(DisplayName = "Batch ID")]
			[PXString]
			public string BatchID { get; set; }
			#endregion
			#region HasDiscrepancies
			public abstract class hasDiscrepancies : PX.Data.BQL.BqlBool.Field<hasDiscrepancies> { }
			[PXBool]
			[PXUIField(DisplayName = "Has Discrepancies", Enabled = false, Visible = false)]
			[PXUnboundDefault(false)]
			public bool? HasDiscrepancies { get; set; }
			#endregion
			#region Warning
			public abstract class warning : PX.Data.BQL.BqlString.Field<warning> { }
			[PXString]
			public string Warning { get; set; }
			#endregion
		}

		public class PREmployeeEqualityComparer : IEqualityComparer<PREmployee>
		{
			public bool Equals(PREmployee x, PREmployee y)
			{
				return x.BAccountID == y.BAccountID;
			}

			public int GetHashCode(PREmployee obj)
			{
				return obj.BAccountID.GetHashCode();
			}
		}
	}
}
