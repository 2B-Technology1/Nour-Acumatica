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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.SM;

namespace PX.Objects.PM
{
	public class TemplateMaint : PXGraph<TemplateMaint, PMProject>, PXImportAttribute.IPXPrepareItems
	{
		#region DAC Attributes Override
		#region PMProject

		[PXDimensionSelector(
					ProjectAttribute.DimensionNameTemplate,
					typeof(SelectFrom<PMProject>
						.Where<
							PMProject.baseType.IsEqual<CTPRType.projectTemplate>
							.And<MatchUser>>
						.SearchFor<PMProject.contractCD>),
					typeof(PMProject.contractCD),
					typeof(PMProject.contractCD),
					typeof(PMProject.description),
					typeof(PMProject.status),
					DescriptionField = typeof(PMProject.description))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Template ID", Visibility = PXUIVisibility.SelectorVisible)]
		[ProjectCDRestrictor]
		protected virtual void _(Events.CacheAttached<PMProject.contractCD> e)
		{
		}

		[Project(Visibility = PXUIVisibility.Invisible, Visible = false)]
		protected virtual void _(Events.CacheAttached<PMProject.templateID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(CTPRType.ProjectTemplate)]
		protected virtual void _(Events.CacheAttached<PMProject.baseType> e)
		{
		}

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Visibility = PXUIVisibility.Invisible, Visible = false)]
		protected virtual void _(Events.CacheAttached<PMProject.nonProject> e)
		{
		}

		[PXDBString(1, IsFixed = true)]
		[ProjectStatus.TemplStatusList]
		[PXDefault(ProjectStatus.OnHold)]
		[PXUIField(DisplayName = "Status", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<PMProject.status> e)
		{
		}

		[PXDBDate]
		protected virtual void _(Events.CacheAttached<PMProject.startDate> e)
		{
		}

		[PXDBLong]
		protected virtual void _(Events.CacheAttached<PMProject.curyInfoID> e)
		{
		}

		[PXDBLong]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void PMRevenueBudget_CuryInfoID_CacheAttached(PXCache sender)
		{
			CuryField.SubscribeSimpleCopying(sender);
		}

		[PXDBLong]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void PMCostBudget_CuryInfoID_CacheAttached(PXCache sender)
		{
			CuryField.SubscribeSimpleCopying(sender);
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<PMProject.billingCuryID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Project Currency", Required = true, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		protected virtual void _(Events.CacheAttached<PMProject.curyID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<PMProject.baseCuryID> e)
		{ }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Application Nbr. Format", FieldClass = nameof(FeaturesSet.Construction))]
		protected virtual void _(Events.CacheAttached<PMProject.lastProformaNumber> e) { }
		#endregion

		#region PMTask
		[PXDBInt(IsKey = true)]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXDBDefault(typeof(PMProject.contractID))]
		protected virtual void _(Events.CacheAttached<PMTask.projectID> e)
		{
		}

		[PXDBString(1, IsFixed = true)]
		[PXDefault(ProjectTaskStatus.Active)]
		[PXUIField(Visibility = PXUIVisibility.Invisible, Visible = false)]
		protected virtual void _(Events.CacheAttached<PMTask.status> e)
		{
		}

		[Customer(DescriptionField = typeof(Customer.acctName), Visibility = PXUIVisibility.Invisible, Visible = false)]
		protected virtual void _(Events.CacheAttached<PMTask.customerID> e)
		{
		}

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Automatically Include in Project")]
		protected virtual void _(Events.CacheAttached<PMTask.autoIncludeInPrj> e)
		{
		}

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bill Separately", Visible = false)]
		protected virtual void _(Events.CacheAttached<PMTask.billSeparately> e)
		{
		}
		#endregion

		#region EPEquipmentRate
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Equipment ID")]
		[PXSelector(typeof(EPEquipment.equipmentID), DescriptionField = typeof(EPEquipment.description), SubstituteKey = typeof(EPEquipment.equipmentCD))]
		protected virtual void _(Events.CacheAttached<EPEquipmentRate.equipmentID> e)
		{
		}

		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<EPEquipmentRate.projectID>>>>))]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXDBInt(IsKey = true)]
		protected virtual void _(Events.CacheAttached<EPEquipmentRate.projectID> e)
		{
		}
		#endregion

		#region EPEmployeeContract
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.Visible)]
		[EP.PXEPEmployeeSelector()]
		[PXCheckUnique(Where = typeof(Where<EPEmployeeContract.contractID, Equal<Current<EPEmployeeContract.contractID>>>))]
		protected virtual void _(Events.CacheAttached<EPEmployeeContract.employeeID> e)
		{
		}

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<EPEmployeeContract.contractID>>>>))]
		[PXCheckUnique(Where = typeof(Where<EPEmployeeContract.employeeID, Equal<Current<EPEmployeeContract.employeeID>>>))]
		protected virtual void _(Events.CacheAttached<EPEmployeeContract.contractID> e)
		{
		}
		#endregion

		[PXDBString(1, IsFixed = true)]
		[BillingType.ListForProject()]
		[PXUIField(DisplayName = "Billing Period")]
		protected virtual void _(Events.CacheAttached<ContractBillingSchedule.type> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXBool]
		[PXDefault(false)]
		protected virtual void _(Events.CacheAttached<PMCostCode.isProjectOverride> e)
		{
		}

		#region NotificationSource
		[PXDBGuid(IsKey = true)]
		[PXSelector(typeof(Search<NotificationSetup.setupID,
			Where<NotificationSetup.sourceCD, Equal<PMNotificationSource.project>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
		[PXUIField(DisplayName = "Mailing ID")]
		protected virtual void _(Events.CacheAttached<NotificationSource.setupID> e)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		protected virtual void _(Events.CacheAttached<NotificationSource.classID> e)
		{
		}
		[GL.Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCheckUnique(typeof(NotificationSource.setupID), IgnoreNulls = false,
			Where = typeof(Where<NotificationSource.refNoteID, Equal<Current<NotificationSource.refNoteID>>>))]
		protected virtual void _(Events.CacheAttached<NotificationSource.nBranchID> e)
		{

		}
		[PXDBString(8, InputMask = "CC.CC.CC.CC")]
		[PXUIField(DisplayName = "Report")]
		[PXDefault(typeof(Search<NotificationSetup.reportID,
			Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SiteMap.screenID,
			Where<SiteMap.url, Like<urlReports>,
				And<Where<SiteMap.screenID, Like<PXModule.pm_>>>>,
			OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
			Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
			DescriptionField = typeof(SiteMap.title))]
		[PXFormula(typeof(Default<NotificationSource.setupID>))]
		protected virtual void _(Events.CacheAttached<NotificationSource.reportID> e)
		{
		}
		#endregion
		
		#region NotificationRecipient
		[PXDBInt]
		[PXDBDefault(typeof(NotificationSource.sourceID))]
		protected virtual void _(Events.CacheAttached<NotificationRecipient.sourceID> e)
		{
		}
		[PXDBString(10)]
		[PXDefault]
		[NotificationContactType.ProjectTemplateList]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckDistinct(typeof(NotificationRecipient.contactID),
			Where = typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>,
			And<NotificationRecipient.refNoteID, Equal<Current<PMProject.noteID>>>>))]
		protected virtual void _(Events.CacheAttached<NotificationRecipient.contactType> e)
		{
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationRecipient.contactType),
			typeof(Search2<Contact.contactID,
				LeftJoin<EPEmployee,
					  On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
					  And<EPEmployee.defContactID, Equal<Contact.contactID>>>>,
				Where<Current<NotificationRecipient.contactType>, Equal<NotificationContactType.employee>,
			  And<EPEmployee.acctCD, IsNotNull>>>)
			, DirtyRead = true)]
		protected virtual void _(Events.CacheAttached<NotificationRecipient.contactID> e)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		protected virtual void _(Events.CacheAttached<NotificationRecipient.classID> e)
		{
		}
		[PXString]
		[PXUIField(DisplayName = "Email", Enabled = false)]
		[PXFormula(typeof(Selector<NotificationRecipient.contactID, Contact.eMail>))]
		protected virtual void _(Events.CacheAttached<NotificationRecipient.email> e)
		{
		}
		#endregion

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXDBInt(IsKey = true)]
		protected virtual void _(Events.CacheAttached<PMRetainageStep.projectID> e) { }
		#endregion

		[InjectDependency]
		public IUnitRateService RateService { get; set; }

		public sealed class ProjectGroupMaskHelperExt : ProjectGroupMaskHelper<TemplateMaint>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		public IProjectGroupMaskHelper ProjectGroupMaskHelper => GetExtension<ProjectGroupMaskHelperExt>();

		#region Views/Selects
		public SelectFrom<PMProject>
			.Where<
				PMProject.baseType.IsEqual<CTPRType.projectTemplate>
				.And<MatchUser>>
			.View Project;

		public PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMProject.contractID>>>> ProjectProperties;

		public PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Current<PMProject.contractID>>>> Billing;
		[PXImport(typeof(PMProject))]
		[PXFilterable]
		public PXSelect<PMTask, Where<PMTask.projectID, Equal<Current<PMProject.contractID>>>> Tasks;
		[PXFilterable]
		public PXSelectJoin<EPEquipmentRate, InnerJoin<EPEquipment, On<EPEquipmentRate.equipmentID, Equal<EPEquipment.equipmentID>>>, Where<EPEquipmentRate.projectID, Equal<Current<PMProject.contractID>>>> EquipmentRates;
		public PXSelect<PMAccountTask, Where<PMAccountTask.projectID, Equal<Current<PMProject.contractID>>>> Accounts;
		public PXSetup<PMSetup> Setup;
		public PXSetup<Company> Company;

		[PXCopyPasteHiddenFields(typeof(PMRevenueBudget.completedPct), typeof(PMRevenueBudget.revisedQty), typeof(PMRevenueBudget.curyRevisedAmount), typeof(PMRevenueBudget.curyAmountToInvoice))]
		[PXImport(typeof(PMProject))]
		[PXFilterable]
		public PXSelect<PMRevenueBudget, Where<PMRevenueBudget.projectID, Equal<Current<PMProject.contractID>>, And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>> RevenueBudget;
				
		[PXCopyPasteHiddenFields(typeof(PMCostBudget.revisedQty), typeof(PMCostBudget.curyRevisedAmount), typeof(PMCostBudget.curyCostToComplete), typeof(PMCostBudget.curyCostAtCompletion), typeof(PMCostBudget.completedPct))]
		[PXImport(typeof(PMProject))]
		[PXFilterable]
		public PXSelect<PMCostBudget, Where<PMCostBudget.projectID, Equal<Current<PMProject.contractID>>, And<PMCostBudget.type, Equal<GL.AccountType.expense>>>> CostBudget;
				
		public PXSelectJoin<EPEmployeeContract,
			InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPEmployeeContract.employeeID>>>,
			Where<EPEmployeeContract.contractID, Equal<Current<PMProject.contractID>>>> EmployeeContract;
		public PXSelectJoin<EPContractRate
				, LeftJoin<IN.InventoryItem, On<IN.InventoryItem.inventoryID, Equal<EPContractRate.labourItemID>>
					, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPContractRate.earningType>>>
					>
				, Where<EPContractRate.employeeID, Equal<Optional<EPEmployeeContract.employeeID>>, And<EPContractRate.contractID, Equal<Optional<PMProject.contractID>>>>
				, OrderBy<Asc<EPContractRate.contractID>>
				> ContractRates;
		
		public PXSelect<PMRetainageStep, Where<PMRetainageStep.projectID, Equal<Current<PMProject.contractID>>>, OrderBy<Asc<PMRetainageStep.thresholdPct>>> RetainageSteps;


		[PXHidden]
		public PXSelect<PMRecurringItem, Where<PMRecurringItem.projectID, Equal<Current<PMTask.projectID>>>> BillingItems;

		[PXViewName(Messages.ProjectAnswers)]
		public TemplateAttributeList<PMProject> Answers;

		[PXHidden]
		public TemplateAttributeList<PMTask> TaskAnswers;

		public PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<PMProject.noteID>>>> Answer;
		public EPDependNoteList<NotificationSource, NotificationSource.refNoteID, PMProject> NotificationSources;
		public PXSelect<NotificationRecipient, Where<NotificationRecipient.sourceID, Equal<Optional<NotificationSource.sourceID>>>> NotificationRecipients;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMCostCode> dummyCostCode;

		public PXFilter<CopyDialogInfo> CopyDialog;
		#endregion

		#region Actions/Buttons

		public PXAction<PMProject> viewTask;
        [PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        public IEnumerable ViewTask(PXAdapter adapter)
        {
            if (Tasks.Current != null && Project.Cache.GetStatus(Project.Current) != PXEntryStatus.Inserted)
            {
                TemplateTaskMaint graph = CreateInstance<TemplateTaskMaint>();
                graph.Task.Current = PMTask.PK.FindDirty(this, Tasks.Current.ProjectID, Tasks.Current.TaskID);

                throw new PXPopupRedirectException(graph, Messages.ProjectTaskEntry + " - " + Messages.ViewTask, true);
            }
            return adapter.Get();
        }

		public PXAction<PMProject> copyTemplate;
		[PXUIField(DisplayName = "Copy", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable CopyTemplate(PXAdapter adapter)
		{
			if (Project.Current != null)
			{
				this.Save.Press();

				IsCopyPaste = true;
				try
				{
					Copy(Project.Current);
				}
				finally
				{
					IsCopyPaste = false;
				}
			}
			return adapter.Get();
		}

		public PXAction<PMProject> updateRetainage;
		[PXUIField(DisplayName = "Update Retainage", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(DisplayOnMainToolbar = false, VisibleOnProcessingResults = false)]
		public virtual IEnumerable UpdateRetainage(PXAdapter adapter)
		{
			if (Project.Current != null)
			{
				bool hasDiscrepency = false;

				foreach (PMRevenueBudget budget in RevenueBudget.Select())
				{
					if (budget.RetainagePct != Project.Current.RetainagePct)
					{
						hasDiscrepency = true;
					}
				}

				if (hasDiscrepency)
				{
					if (Project.Current.RetainageMode == RetainageModes.Contract)
					{
						SyncRetainage();
					}
					else
					{
						WebDialogResult result = Project.Ask(Messages.RetaiangeChangedDialogHeader, Messages.RetaiangeChangedDialogQuestion, MessageButtons.YesNo, MessageIcon.Question);
						if (result == WebDialogResult.Yes)
						{
							SyncRetainage();
						}
					}
				}
			}
			return adapter.Get();
		}

		public virtual void SyncRetainage()
		{
			List<PMRevenueBudget> budgetToUpdate = new List<PMRevenueBudget>();

			foreach (PMRevenueBudget budget in RevenueBudget.Select())
			{
				if (budget.RetainagePct != Project.Current.RetainagePct)
				{
					budgetToUpdate.Add(budget);
				}
			}

			if (budgetToUpdate.Count > 0)
			{
				foreach (PMRevenueBudget budget in budgetToUpdate)
				{
					budget.RetainagePct = Project.Current.RetainagePct;
					RevenueBudget.Update(budget);
				}
			}
		}

		#endregion

		public TemplateMaint()
		{
			if (Setup.Current == null)
			{
				throw new PXException(Messages.SetupNotConfigured);
			}

			this.CopyPaste.SetVisible(false);
		}

		#region Event Handlers
		protected virtual void _(Events.RowInserted<PMProject> e)
		{
			bool BillingCacheIsDirty = Billing.Cache.IsDirty;
			ContractBillingSchedule schedule = new ContractBillingSchedule { ContractID = e.Row.ContractID };
			Billing.Insert(schedule);
			Billing.Cache.IsDirty = BillingCacheIsDirty;

			var select = new PXSelect<NotificationSetup, Where<NotificationSetup.module, Equal<GL.BatchModule.modulePM>>>(this);
			var select2 = new PXSelect<NotificationSetupRecipient, Where<NotificationSetupRecipient.setupID, Equal<Required<NotificationSetupRecipient.setupID>>>>(this);

			bool NotificationSourcesCacheIsDirty = NotificationSources.Cache.IsDirty;
			bool NotificationRecipientsCacheIsDirty = NotificationRecipients.Cache.IsDirty;
			foreach (NotificationSetup setup in select.Select())
			{
				NotificationSource source = new NotificationSource();
				source.SetupID = setup.SetupID;
				source.Active = setup.Active;
				source.EMailAccountID = setup.EMailAccountID;
				source.NotificationID = setup.NotificationID;
				source.ReportID = setup.ReportID;
				source.Format = setup.Format;

				NotificationSources.Insert(source);

				foreach (NotificationSetupRecipient setupRecipient in select2.Select(setup.SetupID))
				{
					NotificationRecipient recipient = new NotificationRecipient();
					recipient.SetupID = setupRecipient.SetupID;
					recipient.Active = setupRecipient.Active;
					recipient.ContactID = setupRecipient.ContactID;
					recipient.AddTo = setupRecipient.AddTo;
					recipient.ContactType = setupRecipient.ContactType;
					recipient.Format = setup.Format;

					NotificationRecipients.Insert(recipient);
				}
			}

			NotificationSources.Cache.IsDirty = NotificationSourcesCacheIsDirty;
			NotificationRecipients.Cache.IsDirty = NotificationRecipientsCacheIsDirty;
		}
		
		protected virtual void PMProject_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			PMProject row = e.Row as PMProject;
			if (row != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.templateID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ContractID);

				if (project != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.ProjectRefError);
				}
			}
		}

		protected virtual void PMProject_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMProject row = e.Row as PMProject;
			if (row != null)
			{
				sender.SetDefaultExt<PMProject.defaultSalesSubID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProject, PMProject.defaultBranchID> e)
		{
			if (e.Row != null && !PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
			{
				e.Cache.SetDefaultExt<PMProject.curyID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProject, PMProject.curyID> e)
		{
			if (e.Row == null) return;
			
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() && e.Row.DefaultBranchID != null)
			{
				GL.Branch branch = SelectFrom<GL.Branch>.Where<GL.Branch.branchID.IsEqual<PMProject.defaultBranchID.FromCurrent>>.View.ReadOnly.Select(this);
				e.NewValue = branch.BaseCuryID;
			}
			else
			{
				e.NewValue = Accessinfo.BaseCuryID;
			}
		}

		protected virtual void EPEmployeeContract_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ContractRates.View.Cache.AllowInsert = e.Row != null;
		}

		protected virtual void EPEmployeeContract_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPEmployeeContract oldRow = (EPEmployeeContract)e.OldRow;
			EPEmployeeContract newRow = (EPEmployeeContract)e.Row;
			if (oldRow == null)
				return;
			EPContractRate.UpdateKeyFields(this, oldRow.ContractID, oldRow.EmployeeID, newRow.ContractID, newRow.EmployeeID);
		}

		protected virtual void _(Events.RowSelected<PMProject> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetVisible<PMProject.curyID>(e.Cache, e.Row, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInGL>(e.Cache, e.Row, Setup.Current.VisibleInGL == true);
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInAP>(e.Cache, e.Row, Setup.Current.VisibleInAP == true);
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInAR>(e.Cache, e.Row, Setup.Current.VisibleInAR == true);
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInSO>(e.Cache, e.Row, Setup.Current.VisibleInSO == true);
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInPO>(e.Cache, e.Row, Setup.Current.VisibleInPO == true);
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInTA>(e.Cache, e.Row, Setup.Current.VisibleInTA == true);
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInEA>(e.Cache, e.Row, Setup.Current.VisibleInEA == true);
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInIN>(e.Cache, e.Row, Setup.Current.VisibleInIN == true);
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInCA>(e.Cache, e.Row, Setup.Current.VisibleInCA == true);
				PXUIFieldAttribute.SetEnabled<PMProject.visibleInCR>(e.Cache, e.Row, Setup.Current.VisibleInCR == true);
				PXUIFieldAttribute.SetEnabled<PMProject.templateID>(e.Cache, e.Row, e.Row.TemplateID == null && e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
							
				PXUIFieldAttribute.SetVisible<PMCostBudget.inventoryID>(CostBudget.Cache, null, e.Row.CostBudgetLevel == BudgetLevels.Item || e.Row.CostBudgetLevel == BudgetLevels.Detail);
				PXUIFieldAttribute.SetVisible<PMCostBudget.costCodeID>(CostBudget.Cache, null, e.Row.CostBudgetLevel == BudgetLevels.CostCode || e.Row.CostBudgetLevel == BudgetLevels.Detail);
				PXUIFieldAttribute.SetVisible<PMCostBudget.revenueInventoryID>(CostBudget.Cache, null, e.Row.BudgetLevel == BudgetLevels.Item || e.Row.BudgetLevel == BudgetLevels.Detail);

				PXUIFieldAttribute.SetVisibility<PMCostBudget.revenueInventoryID>(CostBudget.Cache, null, e.Row.BudgetLevel == BudgetLevels.Item || e.Row.BudgetLevel == BudgetLevels.Detail ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

				PXUIFieldAttribute.SetVisible<PMRevenueBudget.inventoryID>(RevenueBudget.Cache, null, e.Row.BudgetLevel == BudgetLevels.Item || e.Row.BudgetLevel == BudgetLevels.Detail);
				PXUIFieldAttribute.SetVisible<PMRevenueBudget.costCodeID>(RevenueBudget.Cache, null, e.Row.BudgetLevel == BudgetLevels.CostCode || e.Row.BudgetLevel == BudgetLevels.Detail);
				PXUIFieldAttribute.SetRequired<PMRevenueBudget.inventoryID>(RevenueBudget.Cache, e.Row.BudgetLevel == BudgetLevels.Item || e.Row.BudgetLevel == BudgetLevels.Detail);
				PXUIFieldAttribute.SetVisible<PMRevenueBudget.curyPrepaymentAmount>(RevenueBudget.Cache, null, PrepaymentVisible());
				PXUIFieldAttribute.SetVisible<PMRevenueBudget.prepaymentPct>(RevenueBudget.Cache, null, PrepaymentVisible());
				PXUIFieldAttribute.SetVisible<PMRevenueBudget.limitQty>(RevenueBudget.Cache, null, false);
				PXUIFieldAttribute.SetVisible<PMRevenueBudget.maxQty>(RevenueBudget.Cache, null, false);
				PXUIFieldAttribute.SetVisible<PMRevenueBudget.limitAmount>(RevenueBudget.Cache, null, LimitsVisible());
				PXUIFieldAttribute.SetVisible<PMRevenueBudget.curyMaxAmount>(RevenueBudget.Cache, null, LimitsVisible());

				PXUIFieldAttribute.SetVisibility<PMRevenueBudget.curyPrepaymentAmount>(RevenueBudget.Cache, null, PrepaymentVisible() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMRevenueBudget.prepaymentPct>(RevenueBudget.Cache, null, PrepaymentVisible() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMRevenueBudget.limitQty>(RevenueBudget.Cache, null, false ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMRevenueBudget.maxQty>(RevenueBudget.Cache, null, false ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMRevenueBudget.limitAmount>(RevenueBudget.Cache, null, LimitsVisible() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMRevenueBudget.curyMaxAmount>(RevenueBudget.Cache, null, LimitsVisible() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

				bool payByLine = PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>();
				PXUIFieldAttribute.SetVisible<PMProject.retainageMode>(e.Cache, e.Row, payByLine);
				PXUIFieldAttribute.SetVisible<PMRevenueBudget.retainagePct>(RevenueBudget.Cache, null, e.Row.RetainageMode != RetainageModes.Contract);
				PXUIFieldAttribute.SetVisibility<PMRevenueBudget.retainagePct>(RevenueBudget.Cache, null, e.Row.RetainageMode != RetainageModes.Contract ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisible<PMProject.retainageMaxPct>(e.Cache, e.Row, payByLine && e.Row.RetainageMode == RetainageModes.Contract);
				PXUIFieldAttribute.SetVisible<PMRevenueBudget.retainageMaxPct>(RevenueBudget.Cache, null, e.Row.RetainageMode == RetainageModes.Line);
				PXUIFieldAttribute.SetEnabled<PMProject.retainagePct>(e.Cache, e.Row, e.Row.SteppedRetainage != true);
				PXUIFieldAttribute.SetEnabled<PMProject.steppedRetainage>(e.Cache, e.Row, e.Row.RetainageMode != RetainageModes.Line);
				RetainageSteps.AllowSelect = e.Row.SteppedRetainage == true;
				PXUIFieldAttribute.SetEnabled<PMProject.accountingMode>(e.Cache, e.Row, PXAccess.FeatureInstalled<FeaturesSet.materialManagement>());
				PXDefaultAttribute.SetPersistingCheck<PMProject.dropshipExpenseSubMask>(e.Cache, e.Row,
					PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && PXAccess.FeatureInstalled<FeaturesSet.subAccount>() ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				if (!PXAccess.FeatureInstalled<FeaturesSet.inventory>())
				{
					PXStringListAttribute.SetList<PMProject.dropshipExpenseAccountSource>(e.Cache, e.Row,
						new string[] {
							DropshipExpenseAccountSourceOption.PostingClassOrItem,
							DropshipExpenseAccountSourceOption.Project,
							DropshipExpenseAccountSourceOption.Task },
						new string[] {
							Messages.AccountSource_Item,
							Messages.AccountSource_Project,
							Messages.AccountSource_Task });
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProject, PMProject.accountingMode> e)
		{
			if(e.Row is PMProject project && !PXAccess.FeatureInstalled<FeaturesSet.materialManagement>())
			{
				e.NewValue = ProjectAccountingModes.Linked;
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProject, PMProject.retainagePct> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal percent = (decimal)e.NewValue;
				if (percent < 0 || percent > 100)
				{
					throw new PXSetPropertyException<PMProject.retainagePct>(IN.Messages.PercentageValueShouldBeBetween0And100);
				}

			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProject, PMProject.costBudgetLevel> e)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				e.NewValue = BudgetLevels.CostCode;
			}
			else
			{
				e.NewValue = BudgetLevels.Item;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProject, PMProject.steppedRetainage> e)
		{
			if (e.Row != null && e.Row.SteppedRetainage == true && RetainageSteps.Select().Count == 0)
			{
				PMRetainageStep step = RetainageSteps.Insert();
				step.ThresholdPct = 0;
				step.RetainagePct = ((PMProject)e.Row).RetainagePct;
				RetainageSteps.Update(step);
				SyncRetainage();
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProject, PMProject.createProforma> e)
		{			
			if ((bool?)e.NewValue != true && e.Row != null && e.Row.RetainageMode == RetainageModes.Contract)
			{
				throw new PXSetPropertyException<PMProject.createProforma>(Messages.ChangeRetainageMode);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProject, PMProject.retainageMode> e)
		{
			if ((string)e.NewValue == RetainageModes.Contract && e.Row.CreateProforma != true)
			{
				throw new PXSetPropertyException<PMProject.retainageMode>(Messages.CreateProformaRequired);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProject, PMProject.retainageMode> e)
		{
			if (e.Row != null && e.Row.RetainageMode != RetainageModes.Contract)
			{
				e.Row.SteppedRetainage = false;
			}

			if (e.Row.RetainageMode == RetainageModes.Contract)
			{
				SyncRetainage();
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProject, PMProject.retainagePct> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.retainage>())
				updateRetainage.Press();
		}

		protected virtual void _(Events.FieldUpdated<PMProject, PMProject.ownerID> e)
		{
			e.Cache.SetDefaultExt<PMProject.approverID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProject, PMProject.projectGroupID> e)
		{
			ProjectGroupMaskHelper.UpdateProjectMaskFromProjectGroup(e.Row, (string)e.NewValue, e.Cache);
		}

		protected virtual void _(Events.FieldDefaulting<PMProject, PMProject.approverID> e)
		{
			PX.Objects.CR.Standalone.EPEmployee owner = PXSelect<PX.Objects.CR.Standalone.EPEmployee, Where<PX.Objects.CR.Standalone.EPEmployee.defContactID, Equal<Current<PMProject.ownerID>>>>.Select(this);
			if (owner != null)
			{
				e.NewValue = owner.BAccountID;
			}
		}

		#region Revenue Budget
		protected virtual void _(Events.RowSelected<PMRevenueBudget> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMRevenueBudget.limitQty>(e.Cache, e.Row, !string.IsNullOrEmpty(e.Row.UOM));
				PXUIFieldAttribute.SetEnabled<PMRevenueBudget.maxQty>(e.Cache, e.Row, e.Row.LimitQty == true && !string.IsNullOrEmpty(e.Row.UOM));
				PXUIFieldAttribute.SetEnabled<PMRevenueBudget.curyMaxAmount>(e.Cache, e.Row, e.Row.LimitAmount == true);

				if ((e.Row.Qty != 0 || e.Row.RevisedQty != 0) && string.IsNullOrEmpty(e.Row.UOM))
				{
					if (string.IsNullOrEmpty(PXUIFieldAttribute.GetError<PMRevenueBudget.uOM>(e.Cache, e.Row)))
						PXUIFieldAttribute.SetWarning<PMRevenueBudget.uOM>(e.Cache, e.Row, Messages.UomNotDefinedForBudget);
				}
				else
				{
					string errorText = PXUIFieldAttribute.GetError<PMRevenueBudget.uOM>(e.Cache, e.Row);
					if (errorText == PXLocalizer.Localize(Messages.UomNotDefinedForBudget))
					{
						PXUIFieldAttribute.SetWarning<PMRevenueBudget.uOM>(e.Cache, e.Row, null);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMRevenueBudget, PMRevenueBudget.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMRevenueBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMRevenueBudget, PMRevenueBudget.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMRevenueBudget.description>(e.Row);

			if (e.Row.AccountGroupID == null)
				e.Cache.SetDefaultExt<PMRevenueBudget.accountGroupID>(e.Row);

			e.Cache.SetDefaultExt<PMRevenueBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMRevenueBudget.curyUnitRate>(e.Row);
			e.Cache.SetDefaultExt<PMRevenueBudget.taxCategoryID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMRevenueBudget, PMRevenueBudget.uOM> e)
		{
			e.Cache.SetDefaultExt<PMRevenueBudget.curyUnitRate>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMRevenueBudget, PMRevenueBudget.description> e)
		{
			if (e.Row == null || Project.Current == null) return;

			if (Project.Current.BudgetLevel == BudgetLevels.CostCode || Project.Current.BudgetLevel == BudgetLevels.Detail)
			{
				if (e.Row.CostCodeID != null)
				{
					PMCostCode costCode = PXSelectorAttribute.Select<PMRevenueBudget.costCodeID>(e.Cache, e.Row) as PMCostCode;
					if (costCode != null)
					{
						e.NewValue = costCode.Description;
					}
				}
			}
			else if (Project.Current.BudgetLevel == BudgetLevels.Task)
			{
				if (e.Row.ProjectTaskID != null)
				{
					PMTask projectTask = PXSelectorAttribute.Select<PMRevenueBudget.projectTaskID>(e.Cache, e.Row) as PMTask;
					if (projectTask != null)
					{
						e.NewValue = projectTask.Description;
					}
				}
			}
			else if (Project.Current.BudgetLevel == BudgetLevels.Item)
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = PXSelectorAttribute.Select<PMRevenueBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMRevenueBudget, PMRevenueBudget.accountGroupID> e)
		{
			if (e.Row == null) return;

			var select = new PXSelect<PMAccountGroup, Where<PMAccountGroup.type, Equal<GL.AccountType.income>>>(this);

			var resultset = select.SelectWindowed(0, 2);

			if (resultset.Count == 1)
			{
				e.NewValue = ((PMAccountGroup)resultset).GroupID;
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = PXSelectorAttribute.Select<PMRevenueBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
					if (item != null)
					{
						Account account = PXSelectorAttribute.Select<InventoryItem.salesAcctID>(Caches[typeof(InventoryItem)], item) as Account;
						if (account != null && account.AccountGroupID != null)
						{
							e.NewValue = account.AccountGroupID;
						}
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMRevenueBudget, PMRevenueBudget.projectTaskID> e)
		{
			e.Cache.SetDefaultExt<PMRevenueBudget.description>(e.Row);

			if (e.Row != null && e.NewValue != null && (!e.Cache.Graph.IsImportFromExcel || e.Row.ProgressBillingBase == null))
			{
				PMTask task = PMTask.PK.FindDirty(this, e.Row.ProjectID, e.NewValue as int?);

				if (task != null)
				{
					e.Cache.SetValueExt<PMRevenueBudget.progressBillingBase>(e.Row, task.ProgressBillingBase);
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMRevenueBudget, PMRevenueBudget.inventoryID> e)
		{
			e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
		}

		protected virtual void _(Events.FieldDefaulting<PMRevenueBudget, PMRevenueBudget.costCodeID> e)
		{
			if (Project.Current != null)
			{
				if (Project.Current.BudgetLevel != BudgetLevels.CostCode)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMRevenueBudget, PMRevenueBudget.curyUnitRate> e)
		{
			if (Project.Current != null)
			{
				decimal? unitPrice = RateService.CalculateUnitPrice(e.Cache, e.Row.ProjectID, e.Row.ProjectTaskID, e.Row.InventoryID, e.Row.UOM, e.Row.Qty, Project.Current.StartDate, Project.Current.CuryInfoID);
				e.NewValue = unitPrice ?? 0m;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProject, PMProject.dropshipExpenseSubMask> e)
		{
			if (Setup.Current != null)
			{
				e.NewValue = Setup.Current.DropshipExpenseSubMask;
				if (e.NewValue != null)
					e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProject, PMProject.dropshipReceiptProcessing> e)
		{
			if (e.Row != null && (string)e.NewValue == DropshipReceiptProcessingOption.SkipReceipt)
			{
				e.Cache.SetValueExt<PMProject.dropshipExpenseRecording>(e.Row, DropshipExpenseRecordingOption.OnBillRelease);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProject, PMProject.dropshipExpenseRecording> e)
		{
			if (e.Row != null && (string)e.NewValue == DropshipExpenseRecordingOption.OnReceiptRelease && PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				INSetup inSetup = PXSelect<INSetup>.Select(this);
				if (inSetup?.UpdateGL != true)
				{
					throw new PXSetPropertyException<PMProject.dropshipExpenseRecording>(Messages.ProjectDropShipPostExpensesUpdateGLInactive);
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PMRevenueBudget> e)
		{
			if (e.Operation != PXDBOperation.Delete && Project.Current != null)
			{
				if (Project.Current.BudgetLevel == BudgetLevels.Item && string.IsNullOrEmpty(e.Row.Description))
				{
					e.Cache.RaiseExceptionHandling<PMRevenueBudget.description>(e.Row, null, new PXSetPropertyException<PMRevenueBudget.description>(Data.ErrorMessages.FieldIsEmpty, $"[{nameof(PMRevenueBudget.Description)}]"));
					throw new PXRowPersistingException(nameof(PMRevenueBudget.Description), null, ErrorMessages.FieldIsEmpty, nameof(PMRevenueBudget.Description));
				}
			}
			e.Row.CuryInfoID = null;
		}

		protected virtual void _(Events.FieldSelecting<PMRevenueBudget, PMRevenueBudget.prepaymentPct> e)
		{
			if (e.Row != null)
			{
				decimal budgetedAmount = e.Row.CuryAmount.GetValueOrDefault();
				decimal result = 0;

				if (budgetedAmount != 0)
					result = e.Row.CuryPrepaymentAmount.GetValueOrDefault() * 100 / budgetedAmount;
				result = Math.Round(result, PMRevenueBudget.completedPct.Precision);

				PXFieldState fieldState = PXDecimalState.CreateInstance(result, PMRevenueBudget.completedPct.Precision, nameof(PMRevenueBudget.prepaymentPct), false, 0, Decimal.MinValue, Decimal.MaxValue);
				e.ReturnState = fieldState;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMRevenueBudget, PMRevenueBudget.prepaymentPct> e)
		{
			if (e.Row != null)
			{
				decimal budgetedAmount = e.Row.CuryAmount.GetValueOrDefault();
				decimal prepayment = Math.Max(0, (budgetedAmount * e.Row.PrepaymentPct.GetValueOrDefault() / 100m));

				e.Cache.SetValueExt<PMRevenueBudget.curyPrepaymentAmount>(e.Row, prepayment);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMRevenueBudget, PMRevenueBudget.curyPrepaymentAmount> e)
		{
			if (Project.Current?.PrepaymentEnabled == true)
			{
				decimal budgetedAmount = e.Row.CuryAmount.GetValueOrDefault();
				decimal? prepayment = (decimal?)e.NewValue;

				if (prepayment > budgetedAmount)
				{
					e.Cache.RaiseExceptionHandling<PMRevenueBudget.curyPrepaymentAmount>(e.Row, e.NewValue, new PXSetPropertyException<PMRevenueBudget.curyPrepaymentAmount>(Messages.PrepaymentAmointExceedsRevisedAmount, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMRevenueBudget, PMRevenueBudget.curyPrepaymentAmount> e)
		{
			if (e.Row != null)
			{
				e.Row.CuryPrepaymentAvailable = e.Row.CuryPrepaymentAmount.GetValueOrDefault() - e.Row.CuryPrepaymentInvoiced.GetValueOrDefault();
			
			}
		}

		protected virtual void _(Events.FieldVerifying<PMRevenueBudget, PMRevenueBudget.retainagePct> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal percent = (decimal)e.NewValue;
				if (percent < 0 || percent > 100)
				{
					throw new PXSetPropertyException<PMRevenueBudget.retainagePct>(IN.Messages.PercentageValueShouldBeBetween0And100);
				}

			}
		}
		#endregion

		#region Cost Budget
		protected virtual void _(Events.RowSelected<PMCostBudget> e)
		{
			if (e.Row != null)
			{
				if ((e.Row.Qty != 0 || e.Row.RevisedQty != 0) && string.IsNullOrEmpty(e.Row.UOM))
				{
					if (string.IsNullOrEmpty(PXUIFieldAttribute.GetError<PMCostBudget.uOM>(e.Cache, e.Row)))
						PXUIFieldAttribute.SetWarning<PMCostBudget.uOM>(e.Cache, e.Row, Messages.UomNotDefinedForBudget);
				}
				else
				{
					string errorText = PXUIFieldAttribute.GetError<PMCostBudget.uOM>(e.Cache, e.Row);
					if (errorText == PXLocalizer.Localize(Messages.UomNotDefinedForBudget))
					{
						PXUIFieldAttribute.SetWarning<PMCostBudget.uOM>(e.Cache, e.Row, null);
					}
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PMCostBudget> e)
		{
			if (e.Row != null)
			{
				e.Row.CuryInfoID = null;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMCostBudget, PMCostBudget.curyUnitRate> e)
		{
			if (Project.Current != null)
			{
				decimal? unitCost = RateService.CalculateUnitCost(e.Cache, e.Row.ProjectID, e.Row.ProjectTaskID, e.Row.InventoryID, e.Row.UOM, null, Project.Current.StartDate, Project.Current.CuryInfoID);
				e.NewValue = unitCost ?? 0m;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMCostBudget, PMCostBudget.inventoryID> e)
		{
			e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
		}

		protected virtual void _(Events.FieldDefaulting<PMCostBudget, PMCostBudget.costCodeID> e)
		{
			if (Project.Current != null)
			{
				if (Project.Current.BudgetLevel != BudgetLevels.CostCode)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMCostBudget, PMCostBudget.projectTaskID> e)
		{
			e.Cache.SetDefaultExt<PMCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMCostBudget, PMCostBudget.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMCostBudget.description>(e.Row);

			if (e.Row.AccountGroupID == null)
				e.Cache.SetDefaultExt<PMCostBudget.accountGroupID>(e.Row);

			e.Cache.SetDefaultExt<PMCostBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMCostBudget.curyUnitRate>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMCostBudget, PMCostBudget.uOM> e)
		{
			e.Cache.SetDefaultExt<PMCostBudget.curyUnitRate>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMCostBudget, PMCostBudget.revenueTaskID> e)
		{
			var select = new PXSelect<PMRevenueBudget, Where<PMRevenueBudget.projectID, Equal<Current<PMCostBudget.projectID>>,
				And<PMRevenueBudget.projectTaskID, Equal<Current<PMCostBudget.revenueTaskID>>,
				And<PMRevenueBudget.inventoryID, Equal<Current<PMCostBudget.inventoryID>>>>>>(this);

			PMRevenueBudget revenue = select.Select();

			if (revenue == null)
				e.Row.RevenueInventoryID = null;
		}

		protected virtual void _(Events.FieldDefaulting<PMCostBudget, PMCostBudget.accountGroupID> e)
		{
			if (e.Row == null) return;
			if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				InventoryItem item = PXSelectorAttribute.Select<PMCostBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
				if (item != null)
				{
					Account account = PXSelectorAttribute.Select<InventoryItem.cOGSAcctID>(Caches[typeof(InventoryItem)], item) as Account;
					if (account != null && account.AccountGroupID != null)
					{
						e.NewValue = account.AccountGroupID;
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMCostBudget, PMCostBudget.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMCostBudget, PMCostBudget.description> e)
		{
			if (e.Row == null || Project.Current == null) return;

			if (Project.Current.CostBudgetLevel == BudgetLevels.CostCode || Project.Current.CostBudgetLevel == BudgetLevels.Detail)
			{
				if (e.Row.CostCodeID != null)
				{
					PMCostCode costCode = PXSelectorAttribute.Select<PMCostBudget.costCodeID>(e.Cache, e.Row) as PMCostCode;
					if (costCode != null)
					{
						e.NewValue = costCode.Description;
					}
				}
			}
			else if (Project.Current.CostBudgetLevel == BudgetLevels.Task)
			{
				if (e.Row.ProjectTaskID != null)
				{
					PMTask projectTask = PXSelectorAttribute.Select<PMCostBudget.projectTaskID>(e.Cache, e.Row) as PMTask;
					if (projectTask != null)
					{
						e.NewValue = projectTask.Description;
					}
				}
			}
			else if (Project.Current.CostBudgetLevel == BudgetLevels.Item)
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = PXSelectorAttribute.Select<PMCostBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}
		#endregion

		Dictionary<int?, int?> persistedTask = new Dictionary<int?, int?>();
		int? negativeKey = null;
		protected virtual void _(Events.RowPersisting<PMTask> e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				negativeKey = e.Row.TaskID;
			}
		}

		protected virtual void _(Events.RowPersisted<PMTask> e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && negativeKey != null)
			{
				int? newKey = e.Row.TaskID;

				foreach (PMCostBudget budget in CostBudget.Cache.Inserted)
				{
					if (budget.RevenueTaskID != null && budget.RevenueTaskID == negativeKey)
					{
						CostBudget.Cache.SetValue<PMCostBudget.revenueTaskID>(budget, newKey);
						if (!persistedTask.ContainsKey(newKey))
							persistedTask.Add(newKey, negativeKey);
					}
				}

				negativeKey = null;
			}

			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				foreach (PMCostBudget budget in CostBudget.Cache.Inserted)
				{
					if (budget.RevenueTaskID != null && persistedTask.TryGetValue(e.Row.TaskID, out negativeKey))
					{
						CostBudget.Cache.SetValue<PMCostBudget.revenueTaskID>(budget, negativeKey);
					}
				}

				foreach (PMCostBudget budget in CostBudget.Cache.Updated)
				{
					if (budget.RevenueTaskID != null && persistedTask.TryGetValue(e.Row.TaskID, out negativeKey))
					{
						CostBudget.Cache.SetValue<PMCostBudget.revenueTaskID>(budget, negativeKey);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTask, PMTask.isDefault> e)
		{
			if (e.Row.IsDefault == true)
			{
				bool requestRefresh = false;
				foreach (PMTask task in Tasks.Select())
				{
					if (task.IsDefault == true && task.TaskID != e.Row.TaskID)
					{
						Tasks.Cache.SetValue<PMTask.isDefault>(task, false);
						Tasks.Cache.SmartSetStatus(task, PXEntryStatus.Updated);

						requestRefresh = true;
					}
				}

				if (requestRefresh)
				{
					Tasks.View.RequestRefresh();
				}
			}
		}

		#region Notification Events
		protected virtual void _(Events.RowSelected<NotificationSource> e)
		{
			if (e.Row != null)
			{
				NotificationSetup ns = PXSelect<NotificationSetup, Where<NotificationSetup.setupID, Equal<Required<NotificationSetup.setupID>>>>.Select(this, e.Row.SetupID);
				if (ns != null && ns.NotificationCD == ProformaEntry.ProformaNotificationCD)
				{
					PXUIFieldAttribute.SetEnabled<NotificationSource.active>(e.Cache, e.Row, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<NotificationSource.active>(e.Cache, e.Row, true);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<NotificationSource.reportID> e)
		{
			if ((string)e.NewValue == PX.Objects.CN.ProjectAccounting.PM.GraphExtensions.ProformaEntryExt.AIAReport ||
				(string)e.NewValue == PX.Objects.CN.ProjectAccounting.PM.GraphExtensions.ProformaEntryExt.AIAWithQtyReport)
			{
				throw new PXSetPropertyException(Messages.ReportsNotSupported, PX.Objects.CN.ProjectAccounting.PM.GraphExtensions.ProformaEntryExt.AIAReport, PX.Objects.CN.ProjectAccounting.PM.GraphExtensions.ProformaEntryExt.AIAWithQtyReport);
			}
		}
		#endregion

		protected virtual void _(Events.FieldVerifying<CopyDialogInfo, CopyDialogInfo.templateID> e)
		{
			var select = new PXSelect<PMProject, Where<PMProject.contractCD, Equal<Required<PMProject.contractCD>>,
				And<PMProject.baseType, Equal<CTPRType.projectTemplate>>>>(this);

			PMProject duplicate = select.Select(e.NewValue);

			if (duplicate != null)
			{
				throw new PXSetPropertyException<CopyDialogInfo.templateID>(Messages.DuplicateTemplateCD);
			}
		}
		#endregion

		/// <summary>
		/// Creates a new instance of ProjectEntry graph and inserts copies of entities from current graph.
		/// Redirects to target graph on completion.
		/// </summary>
		public virtual void Copy(PMProject project)
		{
			bool isAutonumbered = DimensionMaint.IsAutonumbered(this, ProjectAttribute.DimensionNameTemplate);

			string newContractCD = null;
			if (!isAutonumbered)
			{
				if (CopyDialog.AskExt() == WebDialogResult.Yes && !string.IsNullOrEmpty(CopyDialog.Current.TemplateID))
				{
					newContractCD = CopyDialog.Current.TemplateID;
				}
				else
				{
					return;
				}
			}

			TemplateMaint target = PXGraph.CreateInstance<TemplateMaint>();
			target.SelectTimeStamp();
			target.IsCopyPaste = true;
			target.CopySource = this;

			PMProject newProject = (PMProject)Project.Cache.CreateCopy(project);
			newProject.ContractID = null;
			newProject.ContractCD = newContractCD;
			newProject.Status = null;
			newProject.Hold = null;
			newProject.StartDate = null;
			newProject.ExpireDate = null;
			newProject.BudgetFinalized = null;
			newProject.LastChangeOrderNumber = null;
			newProject.IsActive = null;
			newProject.IsCompleted = null;
			newProject.IsCancelled = null;
			newProject.NoteID = null;
			newProject = target.Project.Insert(newProject);

			target.Billing.Cache.Clear();
			ContractBillingSchedule schedule = (ContractBillingSchedule)Billing.Cache.CreateCopy(Billing.SelectSingle());
			schedule.ContractID = newProject.ContractID;
			schedule.LastDate = null;
			target.Billing.Insert(schedule);

			Dictionary<int, int> taskMap = new Dictionary<int, int>();
			foreach (PMTask task in Tasks.Select())
			{
				PMTask newTask = (PMTask)Tasks.Cache.CreateCopy(task);
				newTask.TaskID = null;
				newTask.ProjectID = newProject.ContractID;
				newTask.IsActive = null;
				newTask.IsCompleted = null;
				newTask.IsCancelled = null;
				newTask.Status = null;
				newTask.StartDate = null;
				newTask.EndDate = null;
				newTask.PlannedStartDate = null;
				newTask.PlannedEndDate = null;
				newTask.CompletedPercent = null;
				newTask.NoteID = null;
				if (task.PlannedStartDate != null && Accessinfo.BusinessDate != null && project.StartDate != null)
				{
					newTask.PlannedStartDate = this.Accessinfo.BusinessDate.Value.AddDays(task.PlannedStartDate.Value.Subtract(project.StartDate.Value).TotalDays);
					newTask.PlannedEndDate = newTask.PlannedStartDate.Value.AddDays(task.PlannedEndDate.Value.Subtract(task.PlannedStartDate.Value).TotalDays);
				}
				newTask = target.Tasks.Insert(newTask);
				taskMap.Add(task.TaskID.Value, newTask.TaskID.Value);
				target.TaskAnswers.CopyAllAttributes(newTask, task);
				OnCopyPasteTaskInserted(newProject, newTask, task);
			}

			OnCopyPasteTasksInserted(target, taskMap);

			foreach (PMRevenueBudget budget in RevenueBudget.Select())
			{
				PMRevenueBudget newBudget = (PMRevenueBudget)RevenueBudget.Cache.CreateCopy(budget);
				newBudget.ProjectID = newProject.ContractID;
				newBudget.ProjectTaskID = taskMap[budget.TaskID.Value];
				newBudget.ActualAmount = 0;
				newBudget.ActualQty = 0;
				newBudget.AmountToInvoice = 0;
				newBudget.ChangeOrderAmount = 0;
				newBudget.ChangeOrderQty = 0;
				newBudget.CuryCommittedOrigAmount = 0;
				newBudget.CommittedOrigAmount = 0;
				newBudget.CommittedOrigQty = 0;
				newBudget.CuryCommittedAmount = 0;
				newBudget.CommittedAmount = 0;
				newBudget.CuryCommittedInvoicedAmount = 0;
				newBudget.CuryCommittedOpenAmount = 0;
				newBudget.CommittedOpenQty = 0;
				newBudget.CommittedQty = 0;
				newBudget.CommittedReceivedQty = 0;
				newBudget.CompletedPct = 0;
				newBudget.CostAtCompletion = 0;
				newBudget.CostToComplete = 0;
				newBudget.InvoicedAmount = 0;
				newBudget.LastCostAtCompletion = 0;
				newBudget.LastCostToComplete = 0;
				newBudget.LastPercentCompleted = 0;
				newBudget.PercentCompleted = 0;
				newBudget.PrepaymentInvoiced = 0;
				newBudget.LineCntr = null;
				newBudget.NoteID = null;
				newBudget = target.RevenueBudget.Insert(newBudget);
				OnCopyPasteRevenueBudgetInserted(newProject, newBudget, budget);
			}

			foreach (PMCostBudget budget in CostBudget.Select())
			{
				PMCostBudget newBudget = (PMCostBudget)CostBudget.Cache.CreateCopy(budget);
				newBudget.ProjectID = newProject.ContractID;
				newBudget.ProjectTaskID = taskMap[budget.TaskID.Value];
				newBudget.ActualAmount = 0;
				newBudget.ActualQty = 0;
				newBudget.AmountToInvoice = 0;
				newBudget.ChangeOrderAmount = 0;
				newBudget.ChangeOrderQty = 0;
				newBudget.CuryCommittedOrigAmount = 0;
				newBudget.CommittedOrigAmount = 0;
				newBudget.CommittedOrigQty = 0;
				newBudget.CuryCommittedAmount = 0;
				newBudget.CommittedAmount = 0;
				newBudget.CuryCommittedInvoicedAmount = 0;
				newBudget.CuryCommittedOpenAmount = 0;
				newBudget.CommittedOpenQty = 0;
				newBudget.CommittedQty = 0;
				newBudget.CommittedReceivedQty = 0;
				newBudget.CompletedPct = 0;
				newBudget.CostAtCompletion = 0;
				newBudget.CostToComplete = 0;
				newBudget.InvoicedAmount = 0;
				newBudget.LastCostAtCompletion = 0;
				newBudget.LastCostToComplete = 0;
				newBudget.LastPercentCompleted = 0;
				newBudget.PercentCompleted = 0;
				newBudget.PrepaymentInvoiced = 0;
				newBudget.LineCntr = null;
				newBudget.NoteID = null;
				newBudget.RevenueTaskID = budget.RevenueTaskID == null ? null : ((int?)taskMap[budget.RevenueTaskID.Value]);
				newBudget = target.CostBudget.Insert(newBudget);
				OnCopyPasteCostBudgetInserted(newProject, newBudget, budget);
			}

			foreach (EPEmployeeContract employee in EmployeeContract.Select())
			{
				EPEmployeeContract newEmployee = (EPEmployeeContract)EmployeeContract.Cache.CreateCopy(employee);
				newEmployee.ContractID = newProject.ContractID;
				newEmployee = target.EmployeeContract.Insert(newEmployee);
				OnCopyPasteEmployeeContractInserted(newProject, newEmployee, employee);
			}

			foreach (EPContractRate rate in ContractRates.Select())
			{
				EPContractRate newRate = (EPContractRate)ContractRates.Cache.CreateCopy(rate);
				newRate.ContractID = newProject.ContractID;
				newRate = target.ContractRates.Insert(newRate);
				OnCopyPasteContractRateInserted(newProject, newRate, rate);
			}

			foreach (EPEquipmentRate rate in EquipmentRates.Select())
			{
				EPEquipmentRate newRate = (EPEquipmentRate)EquipmentRates.Cache.CreateCopy(rate);
				newRate.ProjectID = newProject.ContractID;
				newRate.NoteID = null;
				newRate = target.EquipmentRates.Insert(newRate);
				OnCopyPasteEquipmentRateInserted(newProject, newRate, rate);
			}

			foreach (PMAccountTask account in Accounts.Select())
			{
				PMAccountTask newAccount = (PMAccountTask)Accounts.Cache.CreateCopy(account);
				newAccount.ProjectID = newProject.ContractID;
				newAccount.TaskID = taskMap[account.TaskID.Value];
				newAccount.NoteID = null;
				newAccount = target.Accounts.Insert(newAccount);
				OnCopyPasteAccountTaskInserted(newProject, newAccount, account);
			}

			target.NotificationSources.Cache.Clear();
			target.NotificationRecipients.Cache.Clear();

			foreach (NotificationSource source in NotificationSources.Select())
			{
				int? sourceID = source.SourceID;
				source.SourceID = null;
				source.RefNoteID = null;
				NotificationSource newsource = target.NotificationSources.Insert(source);

				foreach (NotificationRecipient recipient in NotificationRecipients.Select(sourceID))
				{
					if (recipient.ContactType == NotificationContactType.Primary || recipient.ContactType == NotificationContactType.Employee)
					{
						recipient.NotificationID = null;
						recipient.SourceID = newsource.SourceID;
						recipient.RefNoteID = null;

						target.NotificationRecipients.Insert(recipient);
					}
				}
				OnCopyPasteNotificationSourceInserted(newProject, newsource, source);
			}

			target.Views.Caches.Add(typeof(PMRecurringItem));

			foreach (PMRecurringItem detail in PXSelect<PMRecurringItem, Where<PMRecurringItem.projectID, Equal<Required<PMRecurringItem.projectID>>>>.Select(this, project.ContractID))
			{
				PMRecurringItem newDetail = (PMRecurringItem)this.Caches[typeof(PMRecurringItem)].CreateCopy(detail);
				newDetail.ProjectID = newProject.ContractID;
				newDetail.TaskID = taskMap[detail.TaskID.Value];
				newDetail.Used = null;
				newDetail.LastBilledDate = null;
				newDetail.LastBilledQty = null;
				newDetail.NoteID = null;
				newDetail = (PMRecurringItem)target.Caches[typeof(PMRecurringItem)].Insert(newDetail);
				OnCopyPasteRecurringItemInserted(newProject, newDetail, detail);
			}

			target.RetainageSteps.Cache.Clear();

			foreach (PMRetainageStep step in RetainageSteps.Select())
			{
				var newStep = (PMRetainageStep)RetainageSteps.Cache.CreateCopy(step);
				newStep.ProjectID = newProject.ContractID;
				newStep.NoteID = null;
				target.RetainageSteps.Insert(newStep);
			}

			target.Answers.CopyAllAttributes(newProject, project);
			OnCopyPasteCompleted(newProject, project);

			PXRedirectHelper.TryRedirect(target, PXRedirectHelper.WindowMode.Same);
		}

		#region CopyPaste Customization
		protected virtual void OnCopyPasteTasksInserted(TemplateMaint target, Dictionary<int, int> taskMap)
		{
			//this method is used to extend Copy in Customizations.
		}

		protected virtual void OnCopyPasteTaskInserted(PMProject target, PMTask newItem, PMTask sourceItem)
		{
		}

		protected virtual void OnCopyPasteRevenueBudgetInserted(PMProject target, PMRevenueBudget newItem, PMRevenueBudget sourceItem)
		{
		}

		protected virtual void OnCopyPasteCostBudgetInserted(PMProject target, PMCostBudget newItem, PMCostBudget sourceItem)
		{
		}

		protected virtual void OnCopyPasteEmployeeContractInserted(PMProject target, EPEmployeeContract newItem, EPEmployeeContract sourceItem)
		{
		}

		protected virtual void OnCopyPasteContractRateInserted(PMProject target, EPContractRate newItem, EPContractRate sourceItem)
		{
		}

		protected virtual void OnCopyPasteEquipmentRateInserted(PMProject target, EPEquipmentRate newItem, EPEquipmentRate sourceItem)
		{
		}

		protected virtual void OnCopyPasteAccountTaskInserted(PMProject target, PMAccountTask newItem, PMAccountTask sourceItem)
		{
		}

		protected virtual void OnCopyPasteNotificationSourceInserted(PMProject target, NotificationSource newItem, NotificationSource sourceItem)
		{
		}

		protected virtual void OnCopyPasteRecurringItemInserted(PMProject target, PMRecurringItem newItem, PMRecurringItem sourceItem)
		{
		}

		protected virtual void OnCopyPasteCompleted(PMProject target, PMProject source)
		{
		}
		#endregion

		/// <summary>
		/// Returns true both for source as well as target graph during copy-paste procedure. 
		/// </summary>
		public bool IsCopyPaste
		{
			get;
			private set;
		}

		/// <summary>
		/// During Paste of Copied Template this propert holds the reference to the Graph with source data.
		/// </summary>
		public TemplateMaint CopySource
		{
			get;
			private set;
		}

		public virtual bool PrepaymentVisible()
		{
			if (Project.Current != null)
			{
				return Project.Current.PrepaymentEnabled == true;
			}

			return false;
		}

		public virtual bool LimitsVisible()
		{
			if (Project.Current != null)
			{
				return Project.Current.LimitsEnabled == true;
			}

			return false;
		}

		#region PMImport Implementation
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (viewName == nameof(RevenueBudget))
			{
				string accountGroupCD = null;
				if (keys.Contains(nameof(PMRevenueBudget.AccountGroupID)))
				{
					//Import file could be missing the AccountGroupID field and hence the Default value could be set by the DefaultEventHandler

					object keyVal = keys[nameof(PMRevenueBudget.AccountGroupID)];

					if (keyVal is int)
					{
						PMAccountGroup accountGroup = PMAccountGroup.PK.Find(this, (int?)keyVal);
						if (accountGroup != null)
						{
							return accountGroup.Type == GL.AccountType.Income;
						}
					}
					else
					{
						accountGroupCD = (string)keys[nameof(PMRevenueBudget.AccountGroupID)];
					}
				}

				if (!string.IsNullOrEmpty(accountGroupCD))
				{
					PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupCD, Equal<Required<PMAccountGroup.groupCD>>>>.Select(this, accountGroupCD);
					if (accountGroup != null)
					{
						return accountGroup.Type == GL.AccountType.Income;
					}
				}
				else
				{
					return true;
				}

				return false;

			}
			else if (viewName == nameof(CostBudget))
			{
				string accountGroupCD = null;
				if (keys.Contains(nameof(PMCostBudget.AccountGroupID)))
				{
					accountGroupCD = (string)keys[nameof(PMCostBudget.AccountGroupID)];
				}

				if (!string.IsNullOrEmpty(accountGroupCD))
				{
					PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupCD, Equal<Required<PMAccountGroup.groupCD>>>>.Select(this, accountGroupCD);
					if (accountGroup != null)
					{
						return accountGroup.IsExpense == true;
					}
				}
				else
				{
					return true;
				}

				return false;
			}
			

			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return true;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }
		#endregion

		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class CopyDialogInfo : IBqlTable
		{
			#region TemplateID
			public abstract class templateID : PX.Data.BQL.BqlString.Field<templateID>
			{
			}

			[PXString()]
			[PXUIField(DisplayName = "Template ID", Required = true)]
			[PXDimensionAttribute(ProjectAttribute.DimensionNameTemplate)]
			public virtual string TemplateID
			{
				get;
				set;
			}
			#endregion
		}
	}

	public class TemplateAttributeList<T> : CRAttributeList<T>
	{
		public TemplateAttributeList(PXGraph graph) : base(graph) { }

		protected override void ReferenceRowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row;

			if (row == null) return;

			var answersCache = _Graph.Caches[typeof(CSAnswers)];

			foreach (CSAnswers answer in answersCache.Cached)
			{
				if (e.Operation == PXDBOperation.Delete)
				{
					answersCache.Delete(answer);
				}				
			}			
		}

		protected override void RowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Insert && e.Operation != PXDBOperation.Update) return;

			var row = e.Row as CSAnswers;
			if (row == null) return;

			if (!row.RefNoteID.HasValue)
			{
				e.Cancel = true;
				RowPersistDeleted(sender, row);
			}
		}
	}
}
