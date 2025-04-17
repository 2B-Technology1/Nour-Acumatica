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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using System.Collections;

namespace PX.Objects.PR
{
	public class PRxEmployeeMaint : PXGraphExtension<EmployeeMaint>
	{
		public SelectFrom<PREmployee>.Where<PREmployee.bAccountID.IsEqual<EPEmployee.bAccountID.FromCurrent>>.View PayrollEmployee;

		public SelectFrom<PREmployee>
			.InnerJoin<BranchWithAddress>.On<PREmployee.parentBAccountID.IsEqual<BranchWithAddress.bAccountID>>
			.Where<MatchWithBranch<BranchWithAddress.branchID>
				.And<MatchWithPayGroup<PREmployee.payGroupID>>
				.And<PREmployee.bAccountID.IsEqual<EPEmployee.bAccountID.FromCurrent>>
				.And<MatchPRCountry<BranchWithAddress.addressCountryID>>>.View FilteredPayrollEmployee;

		public SelectFrom<Contact>.Where<Contact.contactID.IsEqual<EPEmployee.defContactID.FromCurrent>>.View Contact;

		public SelectFrom<Address>
			.InnerJoin<BAccount>.On<Address.addressID.IsEqual<BAccount.defAddressID>>
			.Where<BAccount.bAccountID.IsEqual<EPEmployee.parentBAccountID.FromCurrent>>.View BranchAddress;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		public override void Initialize()
		{
			base.Initialize();
			Base.Action.AddMenuAction(CreateEditPREmployee);
		}

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		public IEnumerable currentEmployee()
		{
			PREmployee payrollEmployee = PayrollEmployee.Select();
			PREmployee filteredPayrollEmployee = FilteredPayrollEmployee.Select();

			bool canCreatePREmployee = false;
			if (payrollEmployee == null)
			{
				canCreatePREmployee = new SelectFrom<BranchWithAddress>
					.Where<BranchWithAddress.bAccountID.IsEqual<EPEmployee.parentBAccountID.FromCurrent>
						.And<MatchPRCountry<BranchWithAddress.addressCountryID>>>.View(Base).SelectSingle() != null;
			}

			CreateEditPREmployee.SetEnabled(canCreatePREmployee || filteredPayrollEmployee != null);
			CreateEditPREmployee.SetCaption(payrollEmployee == null ? Messages.CreatePREmployeeLabel : Messages.EditPREmployeeLabel);
			return null;
		}

		public PXAction<EPEmployee> CreateEditPREmployee;
		[PXButton]
		[PXUIField(DisplayName = Messages.CreatePREmployeeLabel, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		public void createEditPREmployee()
		{
			var employeeSettingsGraph = PXGraph.CreateInstance<PREmployeePayrollSettingsMaint>();
			PREmployee payrollEmployee = PayrollEmployee.SelectSingle();
			if (payrollEmployee == null)
			{
				employeeSettingsGraph.Caches[typeof(EPEmployee)] = Base.Caches[typeof(EPEmployee)];
				employeeSettingsGraph.PayrollEmployee.Extend(Base.Employee.Current);
			}
			else if (FilteredPayrollEmployee.SelectSingle() == null)
			{
				return;
			}
			else
			{
				employeeSettingsGraph.PayrollEmployee.Current = payrollEmployee;
			}

			throw new PXRedirectRequiredException(employeeSettingsGraph, string.Empty);
		}

		#region Event Handlers

		protected virtual void _(Events.RowDeleting<EPEmployee> e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (PayrollEmployee.SelectSingle() != null)
			{
				throw new PXException(Messages.DeleteEmployeePayrollSettings);
			}
		}

		protected virtual void _(Events.RowPersisted<PX.SM.UsersInRoles> e, PXRowPersisted baseHandler)
		{
			baseHandler?.Invoke(e.Cache, e.Args);

			if (e.TranStatus == PXTranStatus.Completed)
			{
				MatchWithPayGroupHelper.ClearUserPayGroupIDsSlot();
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

		protected virtual void _(Events.RowPersisting<EPEmployee> e)
		{
			if (e.Row == null || (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				return;
			}

			if (BranchAddress.SelectSingle()?.CountryID == CountryCodes.Canada)
			{
				Contact contact = Contact.SelectSingle();
				if (contact.DateOfBirth == null)
				{
					throw new PXException(Messages.MandatoryDOB);
				}
			}
		}

		#endregion Event Handlers
	}
}
