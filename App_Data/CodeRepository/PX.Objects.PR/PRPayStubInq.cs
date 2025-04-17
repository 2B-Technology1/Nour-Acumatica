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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.EP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRPayStubInq : PXGraph<PRPayStubInq>
	{
		public PXCancel<PRPayment> Cancel;
		public PXFilter<PayrollDocumentsFilter> Filter;

		public SelectFrom<PRPayment>
			.InnerJoin<EPEmployee>.On<EPEmployee.bAccountID.IsEqual<PRPayment.employeeID>>
			.Where<EPEmployee.userID.IsEqual<AccessInfo.userID.FromCurrent>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>
				.And<PRPayment.paid.IsEqual<True>
				.And<PRPayment.voided.IsEqual<False>>>>>
			.OrderBy<PRPayment.transactionDate.Desc>.View PayChecks;

		public SelectFrom<PRTaxFormBatch>
			.InnerJoin<PREmployeeTaxForm>.On<PREmployeeTaxForm.FK.TaxFormBatch>
			.InnerJoin<EPEmployee>.On<EPEmployee.bAccountID.IsEqual<PREmployeeTaxForm.employeeID>>
			.Where<EPEmployee.userID.IsEqual<AccessInfo.userID.FromCurrent>
				.And<PREmployeeTaxForm.published.IsEqual<True>>>
			.OrderBy<PRTaxFormBatch.year.Desc, PRTaxFormBatch.formType.Asc>.View TaxForms;

		public PRPayStubInq()
		{
			PayChecks.AllowInsert = false;
			PayChecks.AllowUpdate = false;
			PayChecks.AllowDelete = false;

			TaxForms.AllowInsert = false;
			TaxForms.AllowUpdate = false;
			TaxForms.AllowDelete = false;
		}

		[PXRemoveBaseAttribute(typeof(PXNoteAttribute))]
		[PXGuid]
		public void _(Events.CacheAttached<PRPayment.noteID> e) { }

		public PXAction<PRPayment> viewStubReport;
		[PXUIField(DisplayName = Messages.ViewPayStub, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public IEnumerable ViewStubReport(PXAdapter adapter)
		{
			if (PayChecks.Current != null)
			{
				var parameters = new Dictionary<string, string>();
				parameters["DocType"] = PayChecks.Current.DocType;
				parameters["RefNbr"] = PayChecks.Current.RefNbr;

				throw new PXReportRequiredException(parameters, "PR641000", PXBaseRedirectException.WindowMode.New, Messages.PayCheckReport);
			}

			return adapter.Get();
		}

		public PXAction<PRPayment> viewTaxForm;
		[PXUIField(DisplayName = Messages.ViewTaxForm, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public IEnumerable ViewTaxForm(PXAdapter adapter)
		{
			if (TaxForms.Current?.BatchID != null)
			{
				PXResult<PRTaxFormBatch, PREmployeeTaxForm, EPEmployee> result = TaxForms.Select().Select(x => (PXResult<PRTaxFormBatch, PREmployeeTaxForm, EPEmployee>)x)
					.Where(x => ((PRTaxFormBatch)x).BatchID == TaxForms.Current.BatchID).FirstOrDefault();

				PRTaxFormBatch taxFormBatch = result;
				PREmployeeTaxForm employeeTaxForm = result;
				EPEmployee employee = result;
				PREmployeeTaxFormData taxFormData = PREmployeeTaxFormData.PK.Find(this, taxFormBatch.BatchID, employeeTaxForm.EmployeeID, FormFileType.PDF);

				if (taxFormData.FormData != null)
				{
					byte[] formData = Convert.FromBase64String(taxFormData.FormData);
					PX.SM.FileInfo fileInfo = new PX.SM.FileInfo(string.Format("{0}-{1}-{2}.pdf", taxFormBatch.FormType, taxFormBatch.Year, employee.AcctCD), null, formData);
					throw new PXRedirectToFileException(fileInfo, true);
				}
			}

			return adapter.Get();
		}

		protected virtual void _(Events.FieldSelecting<PayrollDocumentsFilter.showTaxFormsTab> e)
		{
			if (e.Row == null)
			{
				return;
			}

			e.ReturnValue = PXAccess.FeatureInstalled<FeaturesSet.payrollCAN>();
		}
	}

	[PXCacheName(Messages.PayrollDocumentsFilter)]
	public class PayrollDocumentsFilter : IBqlTable
	{
		#region ShowTaxFormsTab
		public abstract class showTaxFormsTab : PX.Data.BQL.BqlBool.Field<showTaxFormsTab> { }
		[PXBool]
		[PXUIField(Visible = false)]
		public virtual bool? ShowTaxFormsTab { get; set; }
		#endregion
	}
}
