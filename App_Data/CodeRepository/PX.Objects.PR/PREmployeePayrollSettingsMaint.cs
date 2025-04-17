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
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Payroll.Data;
using PX.Payroll.Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PREmployeePayrollSettingsMaint : PXGraph<PREmployeePayrollSettingsMaint>
	{
		private const string IsEndpointImportInProgressKey = "IsEndpointImportInProgress";

		private bool IsEndpointImportInProgress
		{
			get
			{
				return PXContext.GetSlot<bool>(IsEndpointImportInProgressKey);
			}
			set
			{
				PXContext.SetSlot(IsEndpointImportInProgressKey, value);
			}
		}

		public override bool IsDirty
		{
			get
			{
				PXLongRunStatus status = PXLongOperation.GetStatus(this.UID);
				if (status == PXLongRunStatus.Completed || status == PXLongRunStatus.Aborted)
				{
					foreach (KeyValuePair<Type, PXCache> pair in Caches)
					{
						if (Views.Caches.Contains(pair.Key) && pair.Value.IsDirty)
						{
							return true;
						}
					}
				}
				return base.IsDirty;
			}
		}

		public PREmployeePayrollSettingsMaint()
		{
			EmployeeTax.AllowInsert = false;
			EmployeeTax.AllowDelete = false;
		}

		#region Views
		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<Vendor> DummyVendor;
		public PXSetup<PRSetup> PRSetup;
		public class SetupValidation : PRSetupValidation<PREmployeePayrollSettingsMaint> { }

		public SelectFrom<PREmployee>.
			InnerJoin<BranchWithAddress>.
				On<PREmployee.parentBAccountID.IsEqual<BranchWithAddress.bAccountID>>.
			Where<MatchWithBranch<BranchWithAddress.branchID>.
				And<MatchWithPayGroup<PREmployee.payGroupID>>.
				And<MatchPRCountry<PREmployee.countryID>>>.View PayrollEmployee;
		public PXSelect<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployee.bAccountID>>>> CurrentPayrollEmployee;
		public SelectFrom<Contact>.Where<Contact.contactID.IsEqual<PREmployee.defContactID.FromCurrent>>.View Contact;
		public PXSelect<Address, Where<Address.addressID, Equal<Current<PREmployee.defAddressID>>>> Address;
		public PXSelect<PREmployeeEarning, Where<PREmployeeEarning.bAccountID, Equal<Current<PREmployee.bAccountID>>>> EmployeeEarning;
		public SelectFrom<PREmployeeDeduct>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PREmployeeDeduct.codeID>>
			.Where<PREmployeeDeduct.bAccountID.IsEqual<PREmployee.bAccountID.FromCurrent>
				.And<PRDeductCode.countryID.IsEqual<PREmployee.countryID.FromCurrent>>>
			.OrderBy<PREmployeeDeduct.sequence.Asc>.View EmployeeDeduction;
		public PXSelect<PREmployeeDeduct,
			Where<PREmployeeDeduct.bAccountID, Equal<Optional<PREmployee.bAccountID>>,
			And<PREmployeeDeduct.codeID, Equal<Optional<PREmployeeDeduct.codeID>>>>> CurrentDeduction;

		public PREmployeeAttributeValueSelect<
			PREmployeeAttribute,
			SelectFrom<PREmployeeAttribute>
				.Where<PREmployeeAttribute.bAccountID.IsEqual<PREmployee.bAccountID.FromCurrent>
					.And<PREmployeeAttribute.countryID.IsEqual<PREmployee.countryID.FromCurrent>>>
				.OrderBy<PREmployeeAttribute.isFederal.Desc, PREmployeeAttribute.state.Asc, PREmployeeAttribute.sortOrder.Asc, PREmployeeAttribute.description.Asc>,
			PRCompanyTaxAttribute,
			SelectFrom<PRCompanyTaxAttribute>
				.Where<Brackets<PRCompanyTaxAttribute.state.IsEqual<BQLLocationConstants.FederalUS>
						.Or<PRCompanyTaxAttribute.state.IsEqual<BQLLocationConstants.FederalCAN>>>
					.And<PRCompanyTaxAttribute.countryID.IsEqual<PREmployee.countryID.FromCurrent>>>,
			SelectFrom<PREmployeeTax>.
				Where<PREmployeeTax.bAccountID.IsEqual<PREmployee.bAccountID.FromCurrent>>,
			PREmployeeTax.state,
			PREmployee> EmployeeAttributes;

		public SelectFrom<PREmployeeTax>
			.Where<PREmployeeTax.bAccountID.IsEqual<PREmployee.bAccountID.FromCurrent>
				.And<PREmployeeTax.countryID.IsEqual<PREmployee.countryID.FromCurrent>>>.View EmployeeTax;

		public PRAttributeValuesSelect<
				PREmployeeTaxAttribute,
				Select<PREmployeeTaxAttribute,
					Where<PREmployeeTaxAttribute.bAccountID,
						Equal<Current<PREmployee.bAccountID>>,
					And<PREmployeeTaxAttribute.taxID,
						Equal<Current<PREmployeeTax.taxID>>>>,
					OrderBy<Asc<PREmployeeTaxAttribute.sortOrder, Asc<PREmployeeAttribute.description>>>>,
				PRTaxCodeAttribute,
				Select<PRTaxCodeAttribute,
					Where<PRTaxCodeAttribute.taxID,
						Equal<Current<PREmployeeTax.taxID>>>>,
				PRTaxCode,
				Select<PRTaxCode,
					Where<PRTaxCode.taxID,
						Equal<Optional<PREmployeeTax.taxID>>>>,
				Payroll.Data.PRTax,
				Payroll.TaxTypeAttribute> EmployeeTaxAttributes;

		public PXOrderedSelect<PREmployee, PREmployeeDirectDeposit,
			Where<PREmployeeDirectDeposit.bAccountID, Equal<Current<PREmployee.bAccountID>>>,
			OrderBy<Asc<PREmployeeDirectDeposit.sortOrder>>> EmployeeDirectDeposit;

		public SelectFrom<PREmployeePTOBank>
			.Where<PREmployeePTOBank.bAccountID.IsEqual<PREmployee.bAccountID.FromCurrent>>.View PTOBanks;

		public SelectFrom<PRPayment>.
			Where<PRPayment.employeeID.IsEqual<PREmployee.bAccountID.FromCurrent>.
				And<PRPayment.released.IsNotEqual<True>>.And<PRPayment.paid.IsNotEqual<True>>>.View ActiveEmployeePayments;

		public SelectFrom<PRPaymentOvertimeRule>.
			Where<PRPaymentOvertimeRule.paymentDocType.IsEqual<P.AsString>.
				And<PRPaymentOvertimeRule.paymentRefNbr.IsEqual<P.AsString>>>.View PaymentOvertimeRules;

		public SelectFrom<PREmployeeClassPTOBank>
			.InnerJoin<PRPTOBank>.On<PRPTOBank.bankID.IsEqual<PREmployeeClassPTOBank.bankID>>
			.Where<PREmployeeClassPTOBank.employeeClassID.IsEqual<PREmployee.employeeClassID.FromCurrent>
				.And<PREmployee.usePTOBanksFromClass.FromCurrent.IsEqual<True>>
				.And<PREmployeeClassPTOBank.isActive.IsEqual<True>>
				.And<PRPTOBank.isActive.IsEqual<True>>>.View EmployeeClassPTOBanks;

		public SelectFrom<PRPaymentPTOBank>
			.InnerJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PRPaymentPTOBank.refNbr>
				.And<PRPayment.docType.IsEqual<PRPaymentPTOBank.docType>>>
			.LeftJoin<PRPTODetail>.On<PRPTODetail.paymentDocType.IsEqual<PRPayment.docType>
					.And<PRPTODetail.paymentRefNbr.IsEqual<PRPayment.refNbr>>
					.And<PRPTODetail.bankID.IsEqual<PRPaymentPTOBank.bankID>>>
			.Where<PRPayment.employeeID.IsEqual<PREmployee.bAccountID.FromCurrent>
				.And<PRPayment.paid.IsEqual<False>>
				.And<PRPayment.released.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<PRPaymentPTOBank.bankID.IsEqual<P.AsString>>>.View EditablePaymentPTOBanks;
		public SelectFrom<PRPTODetail>.View DummyPTODetailView;

		public SelectFrom<PRPaymentPTOBank>
			.InnerJoin<PRPayment>.On<PRPaymentPTOBank.FK.Payment>
			.Where<PRPayment.employeeID.IsEqual<PREmployee.bAccountID.FromCurrent>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<PRPayment.status.IsNotEqual<PaymentStatus.voided>>>.View NonVoidedPaymentPTOBanks;

		public SelectFrom<EmploymentHistory>.View EmploymentHistory;

		public PXFilter<CreateEditPREmployeeFilter> CreateEditPREmployeeFilter;

		public SelectFrom<PREmployeeWorkLocation>
			.InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREmployeeWorkLocation.locationID>>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
			.Where<PREmployeeWorkLocation.employeeID.IsEqual<PREmployee.bAccountID.FromCurrent>
				.And<PREmployee.locationUseDflt.FromCurrent.IsEqual<False>>
				.And<Address.countryID.IsEqual<PREmployee.countryID.FromCurrent>>>.View WorkLocations;

		public PXSelect<EPEmployeePosition,
			Where<EPEmployeePosition.employeeID, Equal<Current<EPEmployee.bAccountID>>>,
			OrderBy<Desc<EPEmployeePosition.startDate>>> EmployeePositions;

		public SelectFrom<Address>
			.InnerJoin<BAccountR>.On<BAccountR.defAddressID.IsEqual<Address.addressID>>
			.InnerJoin<BAccount>.On<BAccount.parentBAccountID.IsEqual<BAccountR.bAccountID>>
			.Where<BAccount.bAccountID.IsEqual<PREmployee.bAccountID.FromCurrent>>.View BranchAddress;
		#endregion

		#region Data View Delegates

		public IEnumerable pTOBanks()
		{
			PXView viewSelect = new PXView(this, false, PTOBanks.View.BqlSelect);
			IEnumerable<PREmployeeClassPTOBank> employeeClassBanks = EmployeeClassPTOBanks.Select().FirstTableItems;

			IEnumerable<PREmployeePTOBank> records = viewSelect.SelectMulti().Select(x => (PREmployeePTOBank)x);
			IEnumerable<IPTOBank> lastEffectiveBanks = PTOHelper.GetLastEffectiveBanks(records.Where(x => x.StartDate <= Accessinfo.BusinessDate));
			foreach (PREmployeePTOBank record in records)
			{
				bool hasEmployeeClassBank = employeeClassBanks.Any(x => x.BankID == record.BankID);
				PXUIFieldAttribute.SetEnabled<PREmployeePTOBank.isActive>(PTOBanks.Cache, record, !hasEmployeeClassBank);
				PXUIFieldAttribute.SetEnabled<PREmployeePTOBank.useClassDefault>(PTOBanks.Cache, record, hasEmployeeClassBank);
				record.AllowDelete = !hasEmployeeClassBank;

				record.AccumulatedAmount = null;
				record.AccumulatedMoney = null;
				record.UsedAmount = null;
				record.UsedMoney = null;
				record.AvailableAmount = null;
				record.AvailableMoney = null;
				if (record.StartDate != null && lastEffectiveBanks.Contains(record))
				{
					PTOHelper.PTOHistoricalAmounts history = PTOHelper.GetPTOHistory(this, Accessinfo.BusinessDate.Value, record.BAccountID.Value, record);
					record.AccumulatedAmount = history.AccumulatedHours;
					record.AccumulatedMoney = history.AccumulatedMoney;
					record.UsedAmount = history.UsedHours;
					record.UsedMoney = history.UsedMoney;
					record.AvailableAmount = history.AvailableHours;
					record.AvailableMoney = history.AvailableMoney;
				}

				yield return record;
			}
		}

		public IEnumerable employeeDeduction()
		{
			PXView bqlSelect = new PXView(this, false, EmployeeDeduction.View.BqlSelect);
			PXResultset<PRDeductCode> activeDeductCodes = SelectFrom<PRDeductCode>.Where<PRDeductCode.isActive.IsEqual<True>>.View.Select(this);

			foreach (PXResult<PREmployeeDeduct, PRDeductCode> result in bqlSelect.SelectMulti()
				.Select(x => (PXResult<PREmployeeDeduct, PRDeductCode>)x))
			{
				PREmployeeDeduct deduct = result;
				if (deduct != null)
				{
					if (deduct.CodeID != null && !activeDeductCodes.FirstTableItems.Any(x => x.CodeID == deduct.CodeID))
					{
						deduct.IsActive = false;
						PXUIFieldAttribute.SetEnabled(EmployeeDeduction.Cache, deduct, false);
						EmployeeDeduction.Cache.RaiseExceptionHandling<PREmployeeDeduct.codeID>(
							deduct,
							deduct.CodeID,
							new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
					}

					yield return result;
				}
			}
		}

		public IEnumerable employmentHistory()
		{
			int? employeeID = CurrentPayrollEmployee.Current.BAccountID;
			DateTime? effectiveDate = Accessinfo.BusinessDate;
			EmploymentDates employmentDates = EmploymentHistoryHelper.GetEmploymentDates(this, employeeID, effectiveDate);

			yield return new EmploymentHistory
			{
				EmployeeID = employeeID,
				HireDate = employmentDates.ContinuousHireDate,
				TerminationDate = employmentDates.TerminationDate
			};
		}

		public IEnumerable workLocations()
		{
			if (CurrentPayrollEmployee.Current.LocationUseDflt == true)
			{
				IEnumerable<PXResult<PREmployeeClassWorkLocation, PRLocation>> employeeClassWorkLocations = SelectFrom<PREmployeeClassWorkLocation>
					.InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREmployeeClassWorkLocation.locationID>>
					.Where<PREmployeeClassWorkLocation.employeeClassID.IsEqual<PREmployee.employeeClassID.FromCurrent>>.View.Select(this)
					.Select(x => (PXResult<PREmployeeClassWorkLocation, PRLocation>)x);

				return employeeClassWorkLocations.Select(x => new PXResult<PREmployeeWorkLocation, PRLocation>(
					new PREmployeeWorkLocation()
					{
						EmployeeID = CurrentPayrollEmployee.Current.BAccountID,
						IsDefault = ((PREmployeeClassWorkLocation)x).IsDefault,
						LocationID = ((PREmployeeClassWorkLocation)x).LocationID
					},
					x));
			}
			else
			{
				return new PXView(this, false, WorkLocations.View.BqlSelect).SelectMulti();
			}
		}

		#endregion Data View Delegates

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(CountryAttribute))]
		[PRCountry]
		public void _(Events.CacheAttached<Address.countryID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(BAccountCascadeAttribute))]
		public void _(Events.CacheAttached<PREmployee.bAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPEmployeePosition.employeeID>>>>), LeaveChildren = true)]
		public void _(Events.CacheAttached<EPEmployeePosition.employeeID> e) { }
		#endregion

		#region Actions
		public PXSave<PREmployee> Save;
		public PXCancel<PREmployee> Cancel;
		public PXAction<PREmployee> Insert;
		public PXDelete<PREmployee> Delete;
		public PXFirst<PREmployee> First;
		public PXPrevious<PREmployee> Prev;
		public PXNext<PREmployee> Next;
		public PXLast<PREmployee> Last;

		public PXAction<PRBatch> ViewPayCheck;
		[PXUIField(DisplayName = "View Paycheck", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		protected virtual void viewPayCheck()
		{
			var noteID = CacheHelper.GetCurrentValue(this, typeof(PRxEPEmployeePosition.settlementPaycheckRefNoteID));
			var paycheck = SelectFrom<PRPayment>.Where<PRPayment.noteID.IsEqual<P.AsGuid>>.View.Select(this, noteID).TopFirst;
			var paycheckGraph = CreateInstance<PRPayChecksAndAdjustments>();
			paycheckGraph.Document.Current = paycheck;
			PXRedirectHelper.TryRedirect(paycheckGraph);
		}

		#endregion

		#region Buttons

		[PXUIField(DisplayName = ActionsMessages.Insert, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXInsertButton]
		protected virtual IEnumerable insert(PXAdapter adapter)
		{
			if (CreateEditPREmployeeFilter.AskExt() == WebDialogResult.OK)
			{
				if (this.IsImport)
				{
					InsertForImport();
					return adapter.Get();
				}

				var epGraph = PXGraph.CreateInstance<EPEmployeeSelectGraph>();
				EPEmployee employee = epGraph.Employee.SelectSingle(CreateEditPREmployeeFilter.Current.BAccountID);
				if (employee != null)
				{
					var prGraph = PXGraph.CreateInstance<PREmployeePayrollSettingsMaint>();
					prGraph.Caches[typeof(EPEmployee)] = epGraph.Caches[typeof(EPEmployee)];
					prGraph.PayrollEmployee.Extend(employee);
					CreateEditPREmployeeFilter.Current.BAccountID = null;
					throw new PXRedirectRequiredException(prGraph, string.Empty);
				}
			}

			return adapter.Get();
		}

		protected virtual void InsertForImport()
		{
			var epGraph = PXGraph.CreateInstance<EPEmployeeSelectGraph>();
			EPEmployee employee = epGraph.Employee.SelectSingle(CreateEditPREmployeeFilter.Current.BAccountID);
			if (employee != null)
			{
				Caches[typeof(EPEmployee)] = epGraph.Caches[typeof(EPEmployee)];
				PREmployee prEmployee = PayrollEmployee.Extend(employee);
				prEmployee.EmployeeClassID = CreateEditPREmployeeFilter.Current.EmployeeClassID;
				prEmployee.PaymentMethodID = CreateEditPREmployeeFilter.Current.PaymentMethodID;
				prEmployee.CashAccountID = CreateEditPREmployeeFilter.Current.CashAccountID;
				PayrollEmployee.Update(prEmployee);
				Actions.PressSave();
			}
		}

		public PXAction<PREmployee> GarnishmentDetails;
		[PXButton]
		[PXUIField(DisplayName = "Garnishment Details")]
		public virtual void garnishmentDetails()
		{
			CurrentDeduction.AskExt();
		}

		public PXAction<PREmployee> DeletePTOBank;
		[PXButton]
		[PXUIField]
		public virtual void deletePTOBank()
		{
			PTOBanks.Delete(PTOBanks.Current);
		}

		public PXAction<PREmployee> EditEmployee;
		[PXUIField(DisplayName = "Edit Employee Record", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void editEmployee()
		{
			var graph = PXGraph.CreateInstance<EmployeeMaint>();
			graph.Employee.Current = CurrentPayrollEmployee.Current;
			PXRedirectHelper.TryRedirect(graph);
		}

		public PXAction<PREmployee> ImportTaxes;
		[PXUIField(DisplayName = "Import Taxes", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		[PXButton]
		public virtual IEnumerable importTaxes(PXAdapter adapter)
		{
			Address address = Address.Current;
			PREmployee currentPayrollEmployee = CurrentPayrollEmployee.Current;

			PXLongOperation.StartOperation(this, () =>
			{
				PREmployeePayrollSettingsMaint graph = PXGraph.CreateInstance<PREmployeePayrollSettingsMaint>();
				bool isEndpointImportInProgress = IsImport && adapter.ExternalCall;
				IsEndpointImportInProgress = isEndpointImportInProgress;
				graph.Address.Current = address;
				graph.CurrentPayrollEmployee.Current = currentPayrollEmployee;
				graph.ImportTaxesProc(IsImport);

				if (isEndpointImportInProgress)
				{
					graph.Save.Press();
				}
				IsEndpointImportInProgress = false;
			});

			return adapter.Get();
		}
		#endregion

		#region Events
		protected virtual void _(Events.RowSelected<Contact> e)
		{
			Contact row = e.Row;
			if (row == null) return;

			if (PXAccess.FeatureInstalled<FeaturesSet.payrollCAN>())
			{
				PXDefaultAttribute.SetPersistingCheck<Contact.dateOfBirth>(e.Cache, e.Row, PXPersistingCheck.NullOrBlank);
				PXUIFieldAttribute.SetRequired<Contact.dateOfBirth>(e.Cache, true);
			}
		}
		
		protected virtual void _(Events.FieldUpdated<PREmployee.usePTOBanksFromClass> e)
		{
			if (e.Row == null)
			{
				return;
			}
			var row = (PREmployee)e.Row;

			if (row.UsePTOBanksFromClass == false)
			{
				foreach (PREmployeePTOBank bank in PTOBanks.Select())
				{
					bank.UseClassDefault = false;
					PTOBanks.Update(bank);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployee.empType> e)
		{
			PREmployee currentRow = e.Row as PREmployee;

			if (currentRow == null)
			{
				return;
			}

			string employeeType = e.NewValue as string;
			SetOvertimeExemptFields(employeeType, currentRow);
		}

		protected virtual void _(Events.RowInserting<PREmployee> e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (CurrentPayrollEmployee.Current?.UnionID != null)
			{
				e.Row.UnionUseDflt = false;
				e.Row.UnionID = CurrentPayrollEmployee.Current.UnionID;
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployee.paymentMethodID> e)
		{
			if (e.Row == null)
			{
				return;
			}

			WarnOfOpenPaychecks<PREmployee.paymentMethodID>(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PREmployee.cashAccountID> e)
		{
			if (e.Row == null)
			{
				return;
			}

			WarnOfOpenPaychecks<PREmployee.cashAccountID>(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeDirectDeposit.getsRemainder> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row == null || row.GetsRemainder == false) return;

			//Ensure that we don't have more than one row with GetsRemainder checked.
			foreach (PREmployeeDirectDeposit rec in EmployeeDirectDeposit.Select())
			{
				if (rec.GetsRemainder == true && rec != row)
				{
					rec.GetsRemainder = false;
					EmployeeDirectDeposit.Update(rec);
					EmployeeDirectDeposit.View.RequestRefresh();
				}
			}

			WarnOfOpenPaychecks<PREmployeeDirectDeposit.getsRemainder>(e.Cache, e.Row);
		}

		protected virtual void _(Events.RowInserting<PREmployeeDirectDeposit> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row == null)
			{
				return;
			}

			//Ensure that we have exactly one row with GetsRemainder checked.
			if (!EmployeeDirectDeposit.Select().FirstTableItems.Any(x => x.GetsRemainder == true))
			{
				row.GetsRemainder = true;
			}
		}

		protected virtual void _(Events.RowDeleted<PREmployeeDirectDeposit> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row == null)
			{
				return;
			}

			WarnOfOpenPaychecks<PREmployee.paymentMethodID>(PayrollEmployee.Cache, PayrollEmployee.Current);
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeDirectDeposit.amount> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row == null)
			{
				return;
			}

			if (row.Amount != null)
			{
				row.Percent = null;
			}
			WarnOfOpenPaychecks<PREmployeeDirectDeposit.amount>(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeDirectDeposit.percent> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row == null)
			{
				return;
			}

			if (row.Percent != null)
			{
				row.Amount = null;
			}
			WarnOfOpenPaychecks<PREmployeeDirectDeposit.percent>(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldVerifying<PREmployeeDirectDeposit.percent> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row != null)
			{
				var newValue = (decimal?)e.NewValue ?? 0m;
				var oldValue = (decimal?)e.OldValue ?? 0m;
				decimal? total = newValue - oldValue;
				foreach (PREmployeeDirectDeposit ddRow in EmployeeDirectDeposit.Select())
				{
					total += ddRow.Percent ?? 0m;
				}

				if (total > 100)
				{
					PXUIFieldAttribute.SetError<PREmployeeDirectDeposit.percent>(e.Cache, row, Messages.TotalOver100Pct);
					e.NewValue = 0m;
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeDirectDeposit.sortOrder> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row != null)
			{
				WarnOfOpenPaychecks<PREmployeeDirectDeposit.sortOrder>(e.Cache, e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeAttribute, PREmployeeAttribute.value> e)
		{
			if (e.Row != null)
			{
				WarnOfOpenPaychecks<PREmployeeAttribute.value>(e.Cache, e.Row);
			}
		}

		protected virtual void _(Events.FieldVerifying<PREmployeeDirectDeposit.bankName> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row != null)
			{
				// Bank Name shouldn't be only digits
				if (e.NewValue != null && int.TryParse(e.NewValue.ToString(), out int _))
				{
					e.Cancel = true;
					PXUIFieldAttribute.SetError<PREmployeeDirectDeposit.bankName>(e.Cache, row, Messages.InvalidBankName);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PREmployeeDirectDeposit.bankRoutingNbr> e)
		{
			var row = e.Row as PREmployeeDirectDeposit;
			if (row != null)
			{
				if (e.NewValue?.ToString().Length != 9)
				{
					e.Cancel = true;
					PXUIFieldAttribute.SetError<PREmployeeDirectDeposit.bankRoutingNbr>(e.Cache, row, Messages.RoutingNumberRequires9Digits, string.Empty);
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PREmployeeDirectDeposit> e)
		{
			if (e.Row != null && PXAccess.FeatureInstalled<FeaturesSet.payrollUS>())
			{
				if (e.Row.BankRoutingNbr?.ToString().Length != 9)
				{
					PXUIFieldAttribute.SetError<PREmployeeDirectDeposit.bankRoutingNbr>(e.Cache, e.Row, Messages.RoutingNumberRequires9Digits, string.Empty);
					throw new PXException(Messages.RoutingNumberRequires9Digits);
				}
			}
		}

		protected virtual void _(Events.FieldUpdating<PREmployeeDeduct.codeID> e)
		{
			PREmployeeDeduct row = e.Row as PREmployeeDeduct;
			if (row == null)
			{
				return;
			}

			bool oldAffectsTaxes = GetDeductCodeFromSelectorValue(e.OldValue)?.AffectsTaxes == true;
			bool newAffectsTaxes = GetDeductCodeFromSelectorValue(e.NewValue)?.AffectsTaxes == true;

			if (newAffectsTaxes)
			{
				row.Sequence = 0;
			}
			else if (oldAffectsTaxes)
			{
				row.Sequence = null;
			}
		}

		protected virtual void _(Events.RowPersisting<PREmployeeDeduct> e)
		{
			var row = e.Row as PREmployeeDeduct;

			// If row doesn't have a start date, let the DAC generate an error.
			if (row == null || row.StartDate == null)
			{
				return;
			}

			if (row.EndDate < row.StartDate)
			{
				e.Cache.RaiseExceptionHandling<PREmployeeDeduct.codeID>(
					row,
					row.CodeID,
					new PXSetPropertyException(Messages.InconsistentDeductDate, PXErrorLevel.RowError, row.StartDate?.ToString("d"), row.EndDate?.ToString("d")));
			}

			foreach (PREmployeeDeduct deduction in EmployeeDeduction.SearchAll<Asc<PREmployeeDeduct.codeID>>(new object[] { row.CodeID }))
			{
				if (row.LineNbr != deduction.LineNbr &&
					(row.EndDate == null && deduction.EndDate == null ||
					row.StartDate <= deduction.EndDate && row.EndDate >= deduction.StartDate ||
					row.StartDate <= deduction.EndDate && row.EndDate == null ||
					row.EndDate >= deduction.StartDate && deduction.EndDate == null))
				{
					e.Cache.RaiseExceptionHandling<PREmployeeDeduct.codeID>(row,
						row.CodeID,
						new PXSetPropertyException(Messages.DuplicateEmployeeDeduct));
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PREmployeeEarning> e)
		{
			var row = e.Row as PREmployeeEarning;

			// If row doesn't have a start date, let the DAC generate an error.
			if (row == null || row.StartDate == null)
			{
				return;
			}

			if (row.EndDate < row.StartDate)
			{
				e.Cache.RaiseExceptionHandling<PREmployeeEarning.typeCD>(
					row,
					row.TypeCD,
					new PXSetPropertyException(Messages.InconsistentEarningDate, PXErrorLevel.RowError, row.StartDate?.ToString("d"), row.EndDate?.ToString("d")));
			}

			foreach (PREmployeeEarning earning in EmployeeEarning.SearchAll<Asc<PREmployeeEarning.typeCD>>(new object[] { row.TypeCD }))
			{
				if (row.LineNbr != earning.LineNbr &&
					(row.EndDate == null && earning.EndDate == null ||
					row.StartDate <= earning.EndDate && row.EndDate >= earning.StartDate ||
					row.StartDate <= earning.EndDate && row.EndDate == null ||
					row.EndDate >= earning.StartDate && earning.EndDate == null))
				{
					e.Cache.RaiseExceptionHandling<PREmployeeEarning.typeCD>(row,
						row.TypeCD,
						new PXSetPropertyException(Messages.DuplicateEmployeeEarning, PXErrorLevel.RowError, earning.StartDate?.ToString("d"), earning.EndDate?.ToString("d")));
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeePTOBank.bankID> e)
		{
			var row = (PREmployeePTOBank)e.Row;
			if (row?.BankID == null)
			{
				return;
			}

			IPTOBank bank = (PREmployeeClassPTOBank)SelectFrom<PREmployeeClassPTOBank>
				.Where<PREmployeeClassPTOBank.employeeClassID.IsEqual<PREmployee.employeeClassID.FromCurrent>
				.And<PREmployeeClassPTOBank.bankID.IsEqual<P.AsString>>
				.And<PREmployeeClassPTOBank.isActive.IsEqual<True>>>.View.SelectWindowed(this, 0, 1, row.BankID);
			if (bank == null)
			{
				bank = (PRPTOBank)PXSelectorAttribute.Select<PREmployeePTOBank.bankID>(e.Cache, row);
				if (bank == null)
				{
					return;
				}
			}

			row.AccrualMethod = bank.AccrualMethod;
			row.AccrualRate = bank.AccrualRate;
			row.HoursPerYear = bank.HoursPerYear;
			row.AccrualLimit = bank.AccrualLimit;
			row.CarryoverType = bank.CarryoverType;
			row.CarryoverAmount = bank.CarryoverAmount;
			row.FrontLoadingAmount = bank.FrontLoadingAmount;
			row.StartDate = bank.StartDate;
			row.DisbursingType = bank.DisbursingType;
		}

		public void _(Events.FieldVerifying<PREmployeePTOBank.bankID> e)
		{
			if (e.Row == null || e.NewValue == null)
			{
				return;
			}

			if (PXSelectorAttribute.Select<PREmployeePTOBank.bankID>(e.Cache, e.Row, e.NewValue) == null)
			{
				throw new PXSetPropertyException<PREmployeePTOBank.bankID>(ErrorMessages.ValueDoesntExist, nameof(PREmployeePTOBank.bankID), e.NewValue);
			}
		}

		public void _(Events.RowSelected<PREmployeePTOBank> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PREmployeePTOBank.bankID>(e.Cache, e.Row, e.Row.BankID == null);
			PXUIFieldAttribute.SetVisibility<PREmployeePTOBank.disbursingType>(e.Cache, null, CurrentPayrollEmployee.SelectSingle().CountryID == LocationConstants.CanadaCountryCode ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
		}

		protected virtual void _(Events.RowSelecting<PREmployee> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.StdWeeksPerYearUseDflt == null)
				PayrollEmployee.Cache.SetDefaultExt<PREmployee.stdWeeksPerYearUseDflt>(e.Row);

			if (e.Row.HoursPerWeek == null)
				PayrollEmployee.Cache.SetDefaultExt<PREmployee.hoursPerWeek>(e.Row);

			if (e.Row.CalendarIDUseDflt == null)
				PayrollEmployee.Cache.SetDefaultExt<PREmployee.calendarIDUseDflt>(e.Row);
		}

		protected virtual void _(Events.RowUpdated<PREmployee> e)
		{
			PREmployee oldRow = e.OldRow;
			PREmployee newRow = e.Row;

			if (oldRow.ExemptFromOvertimeRules != true && newRow.ExemptFromOvertimeRules == true && ActiveEmployeePayments.Select().Any_())
			{
				foreach (PRPayment payment in ActiveEmployeePayments.Select())
				{
					payment.ApplyOvertimeRules = false;
					ActiveEmployeePayments.Update(payment);

					PaymentOvertimeRules.Select(payment.DocType, payment.RefNbr).
						ForEach(paymentOvertimeRule => PaymentOvertimeRules.Delete(paymentOvertimeRule));
				}
			}

			if (newRow.UsePTOBanksFromClass == true &&
				!e.Cache.ObjectsEqual<PREmployee.usePTOBanksFromClass, PREmployee.employeeClassID>(newRow, oldRow))
			{
				IEnumerable<PREmployeePTOBank> existingEmployeePTOBanks = PTOBanks.Select().FirstTableItems;
				foreach (PREmployeeClassPTOBank employeeClassBank in EmployeeClassPTOBanks.Select())
				{
					PREmployeePTOBank existingBank = existingEmployeePTOBanks.FirstOrDefault(x => x.BankID == employeeClassBank.BankID && x.StartDate == employeeClassBank.StartDate);
					if (existingBank != null)
					{
						existingBank.IsActive = true;
						PTOBanks.Update(existingBank);
					}
					else
					{
						var newBank = new PREmployeePTOBank();
						PTOBanks.SetValueExt<PREmployeePTOBank.bankID>(newBank, employeeClassBank.BankID);
						PTOBanks.SetValueExt<PREmployeePTOBank.useClassDefault>(newBank, true);
						PTOBanks.Cache.SetDefaultExt<PREmployeePTOBank.bAccountID>(newBank);
						PTOBanks.Cache.SetDefaultExt<PREmployeePTOBank.employeeClassID>(newBank);

						newBank = PTOBanks.Insert(newBank);
					}
				}
			}
		}

		protected virtual void _(Events.RowDeleting<PREmployee> e)
		{
			if (e.Row != null)
			{
				// We shouldn't delete the base dacs records when deleting PREmployee
				PXTableAttribute tableAttr = e.Cache.Interceptor as PXTableAttribute;
				tableAttr.BypassOnDelete(typeof(EPEmployee), typeof(Vendor), typeof(BAccount));
				PXNoteAttribute.ForceRetain<PREmployee.noteID>(e.Cache);
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeePTOBank.isActive> e)
		{
			PREmployeePTOBank row = e.Row as PREmployeePTOBank;
			if (row == null)
			{
				return;
			}

			if (!e.NewValue.Equals(true))
			{
				PXCache paymentCache = this.Caches<PRPayment>();
				PXCache paymentPTOBankCache = this.Caches<PRPaymentPTOBank>();
				PXCache ptoDetailCache = this.Caches<PRPTODetail>();
				foreach (PXResult<PRPaymentPTOBank, PRPayment, PRPTODetail> result in EditablePaymentPTOBanks.Select(row.BankID))
				{
					PRPayment payment = result;
					PRPaymentPTOBank paymentPTOBank = result;
					PRPTODetail ptoDetail = result;

					paymentPTOBank.IsActive = false;
					paymentPTOBank.AccrualAmount = 0m;
					paymentPTOBank.AccrualMoney = 0m;
					paymentPTOBankCache.Update(paymentPTOBank);

					ptoDetailCache.Delete(ptoDetail);

					payment.Calculated = false;
					paymentCache.Update(payment);
				}
			}
		}

		protected virtual void _(Events.FieldUpdating<PREmployeePTOBank.bankID> e)
		{
			if (e.Row == null)
			{
				return;
			}

			foreach (PREmployeePTOBank row in PTOBanks.Select().FirstTableItems.Where(x => x.BankID == (string)e.NewValue && x.UseClassDefault == true))
			{
				row.UseClassDefault = false;
				PTOBanks.Cache.MarkUpdated(row);
			}
		}

		protected virtual void _(Events.FieldVerifying<PREmployeePTOBank.useClassDefault> e)
		{
			if (e.Row == null || (bool)e.NewValue == false)
			{
				return;
			}

			var row = (PREmployeePTOBank)e.Row;
			if (PTOBanks.Select().FirstTableItems.Any(x => x.BankID == row.BankID && x.StartDate == row.StartDate && !e.Cache.ObjectsEqual(x, row)))
			{
				e.Cancel = true;
				throw new PXException(Messages.DuplicateBanksWithUseClassDefault);
			}
		}

		public virtual void _(Events.FieldVerifying<PREmployeeWorkLocation.isDefault> e)
		{
			if (!e.ExternalCall)
			{
				return;
			}

			bool? newValueBool = e.NewValue as bool?;
			bool requestRefresh = false;
			if (newValueBool == true)
			{
				WorkLocations.Select().FirstTableItems.Where(x => x.IsDefault == true).ForEach(x =>
				{
					x.IsDefault = false;
					WorkLocations.Update(x);
					requestRefresh = true;
				});
			}
			else if (newValueBool == false && !WorkLocations.Select().FirstTableItems.Any(x => x.IsDefault == true && !x.LocationID.Equals(e.Cache.GetValue<PREmployeeWorkLocation.locationID>(e.Row))))
			{
				e.NewValue = true;
			}

			if (requestRefresh)
			{
				WorkLocations.View.RequestRefresh();
			}
		}

		public virtual void _(Events.RowInserting<PREmployeeWorkLocation> e)
		{
			if (!WorkLocations.Select().FirstTableItems.Any(x => x.IsDefault == true))
			{
				e.Row.IsDefault = true;
			}
		}

		public virtual void _(Events.RowDeleted<PREmployeeWorkLocation> e)
		{
			IEnumerable<PREmployeeWorkLocation> remainingWorkLocations = WorkLocations.Select().FirstTableItems;
			if (!remainingWorkLocations.Any(x => x.IsDefault == true))
			{
				PREmployeeWorkLocation newDefault = remainingWorkLocations.FirstOrDefault();
				if (newDefault != null)
				{
					newDefault.IsDefault = true;
					WorkLocations.Update(newDefault);
					WorkLocations.View.RequestRefresh();
				}
			}
		}

		public virtual void _(Events.RowSelected<PREmployee> e)
		{
			if (e.Row == null)
			{
				return;
			}

			WorkLocations.AllowInsert = e.Row.LocationUseDflt == false;
			WorkLocations.AllowUpdate = e.Row.LocationUseDflt == false;
			WorkLocations.AllowDelete = e.Row.LocationUseDflt == false;

			bool enableLocationUseDflt = e.Row.LocationUseDflt == true ||
				(PXSelectorAttribute.Select<PREmployee.employeeClassID>(e.Cache, e.Row) as PREmployeeClass)?.WorkLocationCount > 0;
			PXUIFieldAttribute.SetEnabled<PREmployee.locationUseDflt>(e.Cache, e.Row, enableLocationUseDflt);
			PXUIFieldAttribute.SetWarning<PREmployee.locationUseDflt>(e.Cache, e.Row, enableLocationUseDflt ? null : Messages.EmployeeClassHasNoWorkLocation);

			ImportTaxesCustomInfo customInfo = PXLongOperation.GetCustomInfo(this.UID) as ImportTaxesCustomInfo;
			if (customInfo?.ClearAttributeCache == true)
			{
				EmployeeTaxAttributes.Cache.Clear();
				EmployeeAttributes.Cache.Clear();
				customInfo.ClearAttributeCache = false;
			}

			if (customInfo?.TaxesToDelete.Any() == true && !EmployeeTax.Cache.Deleted.Any_())
			{
				customInfo.TaxesToDelete.ForEach(x => EmployeeTax.Delete(x));
				customInfo.TaxesToDelete.Clear();
			}

			if (customInfo?.TaxesToAdd.Any() == true && !EmployeeTax.Cache.Inserted.Any_())
			{
				customInfo.TaxesToAdd.ForEach(x => EmployeeTax.Insert(x));
				ValidateTaxAttributes();
				customInfo.TaxesToAdd.Clear();
			}

			bool isSalaried = EmployeeType.IsSalaried(e.Row.EmpType);
			PXUIFieldAttribute.SetEnabled<PREmployee.exemptFromOvertimeRules>(e.Cache, e.Row, !isSalaried);
			PXUIFieldAttribute.SetEnabled<PREmployee.exemptFromOvertimeRulesUseDflt>(e.Cache, e.Row, !isSalaried);
			PXUIFieldAttribute.SetEnabled<PREmployee.workCodeUseDflt>(e.Cache, e.Row, string.IsNullOrEmpty(PXUIFieldAttribute.GetError<PREmployee.workCodeUseDflt>(e.Cache, e.Row)));
		}

		public void _(Events.FieldUpdated<PREmployee, PREmployee.empType> e)
		{
			var newValue = (string)e.NewValue;
			if (newValue == EmployeeType.SalariedExempt)
			{
				e.Row.ExemptFromOvertimeRules = true;
				e.Row.ExemptFromOvertimeRulesUseDflt = false;
			}
			else if (newValue == EmployeeType.SalariedNonExempt)
			{
				e.Row.ExemptFromOvertimeRules = false;
				e.Row.ExemptFromOvertimeRulesUseDflt = false;
			}
		}

		public virtual void _(Events.RowSelected<PREmployeeTax> e)
		{
			if (e.Row == null)
			{
				return;
			}

			SetTaxSettingError<PREmployeeTax.taxID>(e.Cache, e.Row, e.Row.ErrorLevel);
		}

		public virtual void _(Events.RowSelected<Address> e)
		{
			Address row = e.Row as Address;
			if (row == null || CurrentPayrollEmployee.Current == null)
			{
				return;
			}

			bool showWarning = row.CountryID != CurrentPayrollEmployee.Current.CountryID;
			PXUIFieldAttribute.SetWarning<Address.countryID>(e.Cache, row, showWarning ? Messages.EmployeeInDifferentCountry : null);
		}

		public virtual void _(Events.RowPersisting<PREmployeeTaxAttribute> e)
		{
			if (e.Row.ErrorLevel == (int?)PXErrorLevel.RowError && !IsEndpointImportInProgress)
			{
				e.Cache.RaiseExceptionHandling<PREmployeeTaxAttribute.value>(
					e.Row,
					e.Row.Value,
					new PXSetPropertyException(Messages.ValueBlankAndRequired, PXErrorLevel.RowError));
			}
		}

		public virtual void _(Events.RowPersisting<PREmployeeAttribute> e)
		{
			if (e.Row.ErrorLevel == (int?)PXErrorLevel.RowError && !IsEndpointImportInProgress)
			{
				e.Cache.RaiseExceptionHandling<PREmployeeAttribute.value>(
					e.Row,
					e.Row.Value,
					new PXSetPropertyException(Messages.ValueBlankAndRequired, PXErrorLevel.RowError));
			}
		}

		protected virtual void _(Events.RowPersisting<Address> e)
		{
			TaxLocationHelpers.AddressPersisting(e);
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeAttribute.description> e)
		{
			PREmployeeAttribute row = e.Row as PREmployeeAttribute;
			if (!IsImport || row == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(row.SettingName))
			{
				EmployeeAttributes.SetSettingNameForDescription(row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployeeAttribute.state> e)
		{
			PREmployeeAttribute row = e.Row as PREmployeeAttribute;
			if (!IsImport || row == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(row.SettingName))
			{
				EmployeeAttributes.SetSettingNameForDescription(row);
			}
		}

		protected virtual void _(Events.FieldSelecting<PREmployeeAttribute.description> e)
		{
			if (IsImport)
			{
				// Acuminator disable once PX1070 UiPresentationLogicInEventHandlers
				// The import scenario engine needs this field to be enabled for "Import PR Employee Attributes".
				PXUIFieldAttribute.SetEnabled<PREmployeeAttribute.description>(e.Cache, e.Row);
			}
		}

		protected virtual void _(Events.FieldSelecting<PREmployeeAttribute.state> e)
		{
			if (IsImport)
			{
				// Acuminator disable once PX1070 UiPresentationLogicInEventHandlers
				// The import scenario engine needs this field to be enabled for "Import PR Employee Attributes".
				PXUIFieldAttribute.SetEnabled<PREmployeeAttribute.state>(e.Cache, e.Row);
			}
		}

		protected virtual void _(Events.FieldSelecting<PREmployee.employeeClassID> e)
		{
			PREmployee row = e.Row as PREmployee;
			if (row == null || row.EmployeeClassID == null)
			{
				return;
			}

			PREmployeeClass employeeClass = PXSelectorAttribute.Select<PREmployee.employeeClassID>(e.Cache, row) as PREmployeeClass;
			if (employeeClass == null)
			{
				e.ReturnValue = null;
				row.EmployeeClassID = null;
				e.Cache.Update(row);
			}
		}

		protected virtual void _(Events.FieldSelecting<PREmployee.workCodeID> e)
		{
			PREmployee row = e.Row as PREmployee;
			if (row == null || row.WorkCodeID == null)
			{
				return;
			}

			bool updated = false;

			PMWorkCode workCode = PXSelectorAttribute.Select<PREmployee.workCodeID>(e.Cache, row) as PMWorkCode;
			if (workCode == null)
			{
				e.ReturnValue = null;
				row.WorkCodeID = null;
				updated = true;
			}

			if (row.WorkCodeUseDflt == true)
			{
				bool setError = workCode == null && !string.IsNullOrEmpty(row.EmployeeClassID);
				if (setError)
				{
					row.WorkCodeUseDflt = false;
					updated = true;
				}

				string workCodeIDFieldName = PXUIFieldAttribute.GetDisplayName<PREmployee.workCodeID>(e.Cache);
				PXUIFieldAttribute.SetError<PREmployee.workCodeUseDflt>(e.Cache, row, setError ? string.Format(Messages.WorkCodeFromClassNotFound, workCodeIDFieldName) : null);
			}

			if (updated)
			{
				e.Cache.Update(row);
			}
		}

		protected virtual void _(Events.RowInserted<PREmployeeTax> e)
		{
			ValidateTaxAttributes();
			ValidateEmployeeAttributes();
		}

		protected virtual void _(Events.FieldUpdated<PREmployee, PREmployee.employeeClassID> e)
		{
			if (e.Row == null || e.Row.EmployeeClassID == null)
			{
				return;
			}

			if (e.Row.EmpTypeUseDflt == true)
			{
				var newValue = (string)e.NewValue;
				var employeeClass = PREmployeeClass.PK.Find(this, newValue);
				SetOvertimeExemptFields(employeeClass.EmpType, e.Row);
			}

			foreach (PREmployeePTOBank employeeBank in SelectFrom<PREmployeePTOBank>
				.LeftJoin<PREmployeeClassPTOBank>.On<PREmployeeClassPTOBank.bankID.IsEqual<PREmployeePTOBank.bankID>
					.And<PREmployeeClassPTOBank.employeeClassID.IsEqual<P.AsString>>>
				.Where<PREmployeePTOBank.bAccountID.IsEqual<P.AsInt>
					.And<PREmployeePTOBank.useClassDefault.IsEqual<True>>
					.And<PREmployeeClassPTOBank.employeeClassID.IsNull>>.View.Select(this, e.Row.EmployeeClassID, e.Row.BAccountID))
			{
				employeeBank.UseClassDefault = false;
				PTOBanks.Update(employeeBank);
			}
		}

		protected virtual void _(Events.FieldUpdated<PREmployee, PREmployee.empTypeUseDflt> e)
		{
			if (e.Row == null || e.NewValue == null)
			{
				return;
			}

			var newValue = (bool)e.NewValue;
			if (newValue == true)
			{
				var employeeClass = PREmployeeClass.PK.Find(this, e.Row.EmployeeClassID);
				SetOvertimeExemptFields(employeeClass.EmpType, e.Row);
			}
		}

		protected virtual void _(Events.RowSelected<EPEmployeePosition> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var hasRefNote = e.Cache.GetValue<PRxEPEmployeePosition.settlementPaycheckRefNoteID>(e.Row) != null;
			PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, !hasRefNote);
		}

		protected virtual void _(Events.RowDeleting<EPEmployeePosition> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var hasRefNote = e.Cache.GetValue<PRxEPEmployeePosition.settlementPaycheckRefNoteID>(e.Row) != null;
			if (hasRefNote)
			{
				throw new PXException(EP.Messages.HistoryHasFinalPayment);
			}
		}

		protected virtual void _(Events.RowPersisting<PREmployee> e)
		{
			if (e.Row == null || (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				return;
			}

			if (e.Row.CountryID == CountryCodes.Canada)
			{
				Contact contact = Contact.SelectSingle();
				if (contact.DateOfBirth == null)
				{
					throw new PXException(Messages.MandatoryDOB);
				}
			}
		}


		protected virtual void _(Events.FieldUpdated<Address.countryID> e)
		{
			if ((string)e.NewValue != BranchAddress.SelectSingle().CountryID)
			{
				PXUIFieldAttribute.SetWarning<Address.countryID>(e.Cache, e.Row, Messages.EmployeeAndBranchCountriesDifferent);
			}
		}

		public virtual void _(Events.RowInserting<PREmployeePTOBank> e)
		{
			var row = e.Row;
			if (row == null || row.StartDate == null)
			{
				return;
			}

			PXResult<PRPaymentPTOBank, PRPayment> result = NonVoidedPaymentPTOBanks.Select().Select(x => (PXResult<PRPaymentPTOBank, PRPayment>)x).Where(x => ((PRPayment)x).EndDate.Value.Date >= row.StartDate.Value.Date && ((PRPaymentPTOBank)x).BankID == row.BankID).OrderBy(x => ((PRPayment)x).TransactionDate).LastOrDefault();

			if (result != null)
			{
				PRPayment payment = result;
				e.Cache.RaiseExceptionHandling<PREmployeePTOBank.startDate>(e.Row, row.StartDate, new PXSetPropertyException(PXMessages.LocalizeFormat(Messages.EffectiveDateCannotBeChanged, $"{payment.EndDate.Value.Date.ToShortDateString()}")));
				e.Cancel = true;
			}
		}

		public virtual void _(Events.FieldVerifying<PREmployeePTOBank, PREmployeePTOBank.startDate> e)
		{
			var row = e.Row;
			if (row == null || row.StartDate == null || e.NewValue == null)
			{
				return;
			}

			DateTime date = (DateTime)e.NewValue;
			PXResult<PRPaymentPTOBank, PRPayment> result = NonVoidedPaymentPTOBanks.Select().Select(x => (PXResult<PRPaymentPTOBank, PRPayment>)x).Where(x => ((PRPayment)x).EndDate.Value.Date >= date && ((PRPaymentPTOBank)x).BankID == row.BankID).OrderBy(x => ((PRPayment)x).TransactionDate).LastOrDefault();

			if (result != null)
			{
				PRPayment payment = result;
				e.Cache.RaiseExceptionHandling<PREmployeePTOBank.startDate>(e.Row, row.StartDate, new PXSetPropertyException(PXMessages.LocalizeFormat(Messages.EffectiveDateCannotBeChanged, $"{payment.EndDate.Value.Date.ToShortDateString()}")));
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowDeleting<PREmployeePTOBank> e)
		{
			if (e.Row != null)
			{
				PXResult<PRPaymentPTOBank, PRPayment> result = NonVoidedPaymentPTOBanks.Select().Select(x => (PXResult<PRPaymentPTOBank, PRPayment>)x).Where(x => ((PRPayment)x).EndDate.Value.Date >= e.Row.StartDate.Value.Date && ((PRPaymentPTOBank)x).BankID == e.Row.BankID).OrderBy(x => ((PRPayment)x).TransactionDate).LastOrDefault();
				if (result != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.PTOBankCannotBeDeleted);
				}
			}
		}

		protected void _(Events.FieldVerifying<PREmployee, PREmployee.paymentMethodID> e)
		{
			PREmployee row = e.Row;
			if (row == null || e.NewValue == null)
			{
				return;
			}

			PaymentMethod paymentMethod = PaymentMethod.PK.Find(this, e.NewValue as string);
			PRxPaymentMethod paymentMethodExt = paymentMethod.GetExtension<PRxPaymentMethod>();

			if (paymentMethodExt.PRPrintChecks == false && !SelectFrom<PREmployeeDirectDeposit>
				.Where<PREmployeeDirectDeposit.bAccountID.IsEqual<P.AsInt>>.View.Select(this, row.BAccountID).Any())
			{
				throw new PXSetPropertyException<PRPayment.paymentMethodID>(Messages.NoBankAccountForDirectDeposit);
			}
		}

		#endregion Events

		#region Graph Overrides
		public override void Persist()
		{
			bool foundOne = true;
			foreach (PREmployeeDirectDeposit dd in EmployeeDirectDeposit.Select())
			{
				foundOne = false;
				if (dd.GetsRemainder == true)
				{
					foundOne = true;
					break;
				}
			}
			if (!foundOne)
			{
				throw new PXException(Messages.AtLeastOneRemainderDD);
			}


			try
			{
				base.Persist();
			}
			catch (PXOuterException ex)
			{
				throw new PXPrimaryDacOuterException(ex, PayrollEmployee.Cache, typeof(PREmployee));
			}
		}
		#endregion

		#region Helpers
		private void ValidateTaxAttributes()
		{
			foreach (PREmployeeTax taxCodeWithError in GetTaxAttributeErrors().Where(x => x.ErrorLevel != null && x.ErrorLevel != (int?)PXErrorLevel.Undefined))
			{
				SetTaxSettingError<PREmployeeTax.taxID>(EmployeeTax.Cache, taxCodeWithError, taxCodeWithError.ErrorLevel);
			}
		}

		private IEnumerable<PREmployeeTax> GetTaxAttributeErrors()
		{
			PREmployeeTax restoreCurrent = EmployeeTax.Current;
			try
			{
				foreach (PREmployeeTax taxCode in EmployeeTax.Select().FirstTableItems)
				{
					EmployeeTax.Current = taxCode;
					foreach (PREmployeeTaxAttribute taxAttribute in EmployeeTaxAttributes.Select().FirstTableItems)
					{
						// Raising FieldSelecting on PREmployeeTaxAttribute will set error on the attribute and propagate
						// the error/warning to the tax code
						object value = taxAttribute.Value;
						EmployeeTaxAttributes.Cache.RaiseFieldSelecting<PREmployeeTaxAttribute.value>(taxAttribute, ref value, false);
					}

					yield return taxCode;
				}
			}
			finally
			{
				EmployeeTax.Current = restoreCurrent;
			}
		}

		private void SetTaxSettingError<TErrorField>(PXCache cache, IBqlTable row, int? errorLevel) where TErrorField : IBqlField
		{
			(string previousErrorMsg, PXErrorLevel previousErrorLevel) = PXUIFieldAttribute.GetErrorWithLevel<TErrorField>(cache, row);
			bool previousErrorIsRelated = previousErrorMsg == Messages.ValueBlankAndRequired || previousErrorMsg == Messages.NewTaxSetting;

			if (errorLevel == (int?)PXErrorLevel.RowError && !IsEndpointImportInProgress)
			{
				PXUIFieldAttribute.SetError(cache, row, typeof(TErrorField).Name, Messages.ValueBlankAndRequired, cache.GetValue<TErrorField>(row)?.ToString(), false, PXErrorLevel.RowError);
			}
			else if ((errorLevel == (int?)PXErrorLevel.RowWarning || cache.GetStatus(row) == PXEntryStatus.Inserted) &&
				(previousErrorLevel != PXErrorLevel.RowError || previousErrorIsRelated))
			{
				PXUIFieldAttribute.SetError(cache, row, typeof(TErrorField).Name, Messages.NewTaxSetting, cache.GetValue<TErrorField>(row)?.ToString(), false, PXErrorLevel.RowWarning);
			}
			else if (errorLevel == (int?)PXErrorLevel.Undefined && previousErrorIsRelated)
			{
				PXUIFieldAttribute.SetError(cache, row, typeof(TErrorField).Name, "", cache.GetValue<TErrorField>(row)?.ToString(), false, PXErrorLevel.Undefined);
			}
		}

		private void ValidateEmployeeAttributes()
		{
			foreach (PREmployeeAttribute attribute in EmployeeAttributes.Select().FirstTableItems)
			{
				object value = attribute.Value;
				EmployeeAttributes.Cache.RaiseFieldSelecting<PREmployeeAttribute.value>(attribute, ref value, false);
				SetTaxSettingError<PREmployeeAttribute.value>(EmployeeAttributes.Cache, attribute, attribute.ErrorLevel);
				if (attribute.ErrorLevel == (int?)PXErrorLevel.RowError || attribute.IsEncryptionRequired == true && attribute.IsEncrypted != true)
				{
					EmployeeAttributes.Cache.SetStatus(attribute, PXEntryStatus.Modified);
				}
			}
		}

		public void ImportTaxesProc(bool isMassProcess)
		{
			if (CurrentPayrollEmployee.Current == null)
			{
				return;
			}

			List<Address> taxableAddresses = GetTaxableAddresses(out Address taxableResidenceAddress);
			IEnumerable<Address> addressesWithoutLocationCode = taxableAddresses.Where(x => string.IsNullOrEmpty(x.TaxLocationCode));
			if (taxableResidenceAddress != null && TaxLocationHelpers.IsAddressedModified(Address.Cache, taxableResidenceAddress))
			{
				addressesWithoutLocationCode = addressesWithoutLocationCode.Union(new List<Address> { taxableResidenceAddress }, new TaxLocationHelpers.AddressEqualityComparer());
			}

			if (addressesWithoutLocationCode.Any())
			{
				TaxLocationHelpers.UpdateAddressLocationCodes(addressesWithoutLocationCode.ToList());
				Address.Cache.Clear();
				Address.Cache.ClearQueryCache();
				taxableAddresses = GetTaxableAddresses(out Address _);
			}

			HashSet<string> applicableTaxCodes = null;
			if (CurrentPayrollEmployee.Current.CountryID == LocationConstants.USCountryCode)
			{
				var payrollService = new PayrollTaxClient(CurrentPayrollEmployee.Current.CountryID);
				bool.TryParse(
					EmployeeAttributes.Select().FirstTableItems.FirstOrDefault(x => x.SettingName == PRTaxMaintenance.GetIncludeRailroadTaxesSettingName())?.Value,
					out bool includeRailroadTaxes);
				applicableTaxCodes = payrollService.GetAllLocationTaxTypes(taxableAddresses, includeRailroadTaxes)
					.Select(x => x.UniqueTaxID)
					.ToHashSet();
			}
			else
			{
				PRWebServiceRestClient restClient = new PRWebServiceRestClient();
				applicableTaxCodes = restClient.GetTaxList(CurrentPayrollEmployee.Current.CountryID, taxableAddresses.Select(x => new PRLocationCode() { TaxLocationCode = x.TaxLocationCode }))
					.Select(x => x.UniqueTaxID)
					.ToHashSet();
			}
			
			HashSet<int?> taxIDsToAdd = SelectFrom<PRTaxCode>
				.Where<PRTaxCode.countryID.IsEqual<PREmployee.countryID.FromCurrent>>.View.Select(this).FirstTableItems
				.Where(x => applicableTaxCodes.Contains(x.TaxUniqueCode))
				.Select(x => x.TaxID)
				.ToHashSet();

			List<PREmployeeTax> taxesToDelete = new List<PREmployeeTax>();
			foreach (PREmployeeTax employeeTax in EmployeeTax.Select().FirstTableItems)
			{
				if (!taxIDsToAdd.Contains(employeeTax.TaxID))
				{
					taxesToDelete.Add(employeeTax);
				}
				else
				{
					taxIDsToAdd.Remove(employeeTax.TaxID);
				}
			}

			List<PREmployeeTax> taxesToAdd = taxIDsToAdd.Select(x => new PREmployeeTax() { TaxID = x }).ToList();

			if (isMassProcess)
			{
				taxesToAdd.ForEach(x => EmployeeTax.Insert(x));
				taxesToDelete.ForEach(x => EmployeeTax.Delete(x));
			}
			else
			{
				PXLongOperation.SetCustomInfo(new ImportTaxesCustomInfo(taxesToAdd, taxesToDelete));
			}
		}

		protected virtual List<Address> GetTaxableAddresses(out Address taxableResidenceAddress)
		{
			taxableResidenceAddress = null;
			List<Address> taxableAddresses = new List<Address>();
			PXView employeeClassWorkAddressesView = new SelectFrom<Address>
				.InnerJoin<PRLocation>.On<PRLocation.addressID.IsEqual<Address.addressID>>
				.InnerJoin<PREmployeeClassWorkLocation>.On<PREmployeeClassWorkLocation.locationID.IsEqual<PRLocation.locationID>>
				.Where<PREmployeeClassWorkLocation.employeeClassID.IsEqual<PREmployee.employeeClassID.FromCurrent>
					.And<PRLocation.isActive.IsEqual<True>>>.View(this).View;

			if (CurrentPayrollEmployee.Current.CountryID == LocationConstants.USCountryCode)
			{
				taxableResidenceAddress = Address.Current ?? Address.SelectSingle();
				taxableAddresses.Add(taxableResidenceAddress);
				if (CurrentPayrollEmployee.Current.LocationUseDflt == true)
				{
					taxableAddresses.AddRange(employeeClassWorkAddressesView.SelectMulti().Select(x => (Address)(PXResult<Address>)x));
				}
				else
				{
					taxableAddresses.AddRange(WorkLocations.Select().ToList()
						.Where(x => ((PRLocation)x[typeof(PRLocation)]).IsActive == true)
						.Select(x => (Address)x[typeof(Address)]));
				}
			}
			else
			{
				Address primaryWorkAddress;
				if (CurrentPayrollEmployee.Current.LocationUseDflt == true)
				{
					employeeClassWorkAddressesView.WhereAnd<Where<PREmployeeClassWorkLocation.isDefault.IsEqual<True>>>();
					primaryWorkAddress = (PXResult<Address>)employeeClassWorkAddressesView.SelectSingle();
				}
				else
				{
					primaryWorkAddress = (Address)WorkLocations.Select()
						.FirstOrDefault(x => ((PREmployeeWorkLocation)x).IsDefault == true)?[typeof(Address)];
				}

				if (primaryWorkAddress != null)
				{
					taxableAddresses.Add(primaryWorkAddress);
				}
			}

			return taxableAddresses;
		}

		protected virtual void WarnOfOpenPaychecks<TField>(PXCache cache, object row)
			where TField : IBqlField
		{
			var paymentsInfo = new List<string>();
			foreach (PRPayment payment in SelectFrom<PRPayment>
				.Where<PRPayment.employeeID.IsEqual<PREmployee.bAccountID.FromCurrent>
					.And<Brackets<PRPayment.status.IsEqual<PaymentStatus.pendingPayment>
					.Or<PRPayment.status.IsEqual<PaymentStatus.paymentBatchCreated>>>>>.View.ReadOnly.Select(this))
			{
				paymentsInfo.Add(payment.PaymentDocAndRef);
				payment.Calculated = false;
				Caches[typeof(PRPayment)].Update(payment);
			}

			if (paymentsInfo.Any())
			{
				PXUIFieldAttribute.SetWarning<TField>(cache, row, Messages.PaychecksNeedRecalculationSeeTrace);
				PXTrace.WriteWarning(Messages.PaychecksNeedRecalculationFormat, string.Join(", ", paymentsInfo));
			}
		}

		protected virtual PRDeductCode GetDeductCodeFromSelectorValue(object selectorValue)
		{
			if (selectorValue == null)
			{
				return null;
			}
			if (selectorValue is int selectorInt)
			{
				return PRDeductCode.PK.Find(this, selectorInt);
			}
			if (selectorValue is string selectorString)
			{
				return PRDeductCode.UK.Find(this, selectorString);
			}
			return null;
		}

		protected virtual void SetOvertimeExemptFields(string employeeType, PREmployee currentRow)
		{
			if (EmployeeType.IsSalaried(employeeType))
			{
				currentRow.ExemptFromOvertimeRulesUseDflt = false;
				currentRow.ExemptFromOvertimeRules = employeeType == EmployeeType.SalariedExempt;
			}
		}
		#endregion Helpers

		#region Helper classes
		private class ImportTaxesCustomInfo
		{
			public List<PREmployeeTax> TaxesToAdd;
			public List<PREmployeeTax> TaxesToDelete;
			public bool ClearAttributeCache = true;

			public ImportTaxesCustomInfo(List<PREmployeeTax> taxesToAdd, List<PREmployeeTax> taxesToDelete)
			{
				TaxesToAdd = taxesToAdd;
				TaxesToDelete = taxesToDelete;
			}
		}
		#endregion Helper classes

		#region Address Lookup Extension
		/// <exclude/>
		public class PREmployeePayrollSettingsMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<PREmployeePayrollSettingsMaint, PREmployee, Address>
		{
			protected override string AddressView => nameof(Base.Address);
		}
		#endregion

	}

	[PXCacheName(Messages.EmploymentHistory)]
	public class EmploymentHistory : IBqlTable
	{
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Employee ID", Enabled = false)]
		public int? EmployeeID { get; set; }
		#endregion

		#region HireDate
		public abstract class hireDate : PX.Data.BQL.BqlDateTime.Field<hireDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Hire Date", Enabled = false)]
		public DateTime? HireDate { get; set; }
		#endregion

		#region TerminationDate
		public abstract class terminationDate : PX.Data.BQL.BqlDateTime.Field<terminationDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Termination Date", Enabled = false)]
		public DateTime? TerminationDate { get; set; }
		#endregion
	}

	[PXCacheName(Messages.CreateEditPREmployeeFilter)]
	public class CreateEditPREmployeeFilter : IBqlTable
	{
		#region BAccountID
		[PXInt]
		[PXUIField(DisplayName = "Employee ID")]
		[PXDimensionSelector(EmployeeRawAttribute.DimensionName,
				typeof(Search2<CR.Standalone.EPEmployee.bAccountID,
							InnerJoin<BranchWithAddress, On<BranchWithAddress.bAccountID, Equal<EPEmployee.parentBAccountID>>,
							LeftJoin<PREmployee, On<CR.Standalone.EPEmployee.bAccountID, Equal<PREmployee.bAccountID>>>>,
							Where<PREmployee.bAccountID, IsNull,
								And<MatchPRCountry<BranchWithAddress.addressCountryID>>>>),
				typeof(CR.Standalone.EPEmployee.acctCD),
				typeof(CR.Standalone.EPEmployee.bAccountID),
				typeof(CR.Standalone.EPEmployee.acctCD),
				typeof(CR.Standalone.EPEmployee.acctName),
				typeof(CR.Standalone.EPEmployee.departmentID))]
		public virtual int? BAccountID { get; set; }
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }
		#endregion

		#region EmployeeClassID
		public abstract class employeeClassID : BqlString.Field<employeeClassID> { }
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID", Visible = false)]
		[PXSelector(typeof(PREmployeeClass.employeeClassID))]
		public string EmployeeClassID { get; set; }
		#endregion

		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
			Where<PaymentMethod.isActive, Equal<True>,
				And<PRxPaymentMethod.useForPR, Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		public virtual string PaymentMethodID { get; set; }
		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[UnboundCashAccount(typeof(Search2<CashAccount.cashAccountID,
			InnerJoin<PaymentMethodAccount,
				On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
					And<PaymentMethodAccount.paymentMethodID, Equal<Current<paymentMethodID>>,
					And<PRxPaymentMethodAccount.useForPR, Equal<True>>>>>,
			Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Cash Account", DescriptionField = typeof(CashAccount.descr), Visible = false)]
		public virtual int? CashAccountID { get; set; }
		#endregion
	}

	[PXInt]
	public class UnboundCashAccountAttribute : CashAccountBaseAttribute
	{
		public UnboundCashAccountAttribute(Type search) : base(null, search)
		{
		}
	}

	[PXHidden]
	public class EPEmployeeSelectGraph : PXGraph<EPEmployeeSelectGraph>
	{
		public PXSelect<EPEmployee,
					Where<EPEmployee.bAccountID,
						Equal<Required<EPEmployee.bAccountID>>>> Employee;
	}
}
