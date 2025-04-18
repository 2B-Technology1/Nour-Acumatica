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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PX.Common;
using PX.Data;
using System.Collections;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR.CRCaseMaint_Extensions;
using PX.Objects.CR.Extensions;
using PX.Objects.CT;
using PX.Objects.CR.Workflows;
using PX.Objects.GL;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
    public class CRCaseMaint : PXGraph<CRCaseMaint, CRCase>
	{
		#region Selects

		//TODO: need review
		[PXHidden]
		public PXSelect<BAccount>
			bAccountBasic;
        [PXHidden]
        public PXSelect<BAccountR>
            bAccountRBasic;

        [PXHidden]
        public PXSelect<Contact>
            Contacts;

        [PXHidden]
        public PXSelect<Contract>
            Contracts;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<Company>
			company;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<CRSetup>
			Setup;
		

		[PXViewName(Messages.Case)]
		public PXSelectJoin<CRCase,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRCase.customerID>>>,
				Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>
			Case;

		[PXHidden]
		[PXCopyPasteHiddenFields(typeof(CRCase.description))]
		public PXSelect<CRCase,
			Where<CRCase.caseCD, Equal<Current<CRCase.caseCD>>>>
			CaseCurrent;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<CRCase.noteID>>>>
			CaseActivityStatistics;

		[PXHidden]
		public PXSetup<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Optional<CRCase.caseClassID>>>> Class;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<CRCase>
			Answers;
		
		[PXHidden]
		public PXSelect<CRActivity>
			ActivitiesSelect;

		[PXViewName(Messages.CaseReferences)]
		[PXViewDetailsButton(typeof(CRCase), 
			typeof(Select<CRCase, Where<CRCase.caseCD, Equal<Current<CRCaseReference.childCaseCD>>>>))]
		public PXSelectJoin<CRCaseReference,
            LeftJoin<CRCaseRelated, On<CRCaseRelated.caseCD, Equal<CRCaseReference.childCaseCD>>>>
			CaseRefs;

	    [PXHidden] 
        public PXSelect<CRCaseRelated, Where<CRCaseRelated.caseCD, Equal<Current<CRCaseReference.childCaseCD>>>>
	        CaseRelated;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.OwnerUser)]
		public PXSelectReadonly2<
				Users,
			InnerJoin<Contact,
				On<Contact.userID, Equal<Users.pKID>>>,
			Where<Contact.contactID, Equal<Current<CRCase.ownerID>>>> OwnerUser;
		#endregion

		#region Ctors

		public CRCaseMaint()
		{
			if (string.IsNullOrEmpty(Setup.Current.CaseNumberingID))
			{
				throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);
			}

			PXUIFieldAttribute.SetRequired<CRCase.caseClassID>(Case.Cache, true);

			this.EnsureCachePersistence(typeof(CRPMTimeActivity));
			var bAccountCache = Caches[typeof(BAccount)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);
		}

		#endregion

		#region Data Handlers

		protected virtual IEnumerable caseRefs()
		{
			var currentCaseCd = Case.Current.With(_ => _.CaseCD);
			if (currentCaseCd == null) yield break;

			var ht = new HybridDictionary();
			foreach (CRCaseReference item in
				PXSelect<CRCaseReference,
					Where<CRCaseReference.parentCaseCD, Equal<Required<CRCaseReference.parentCaseCD>>>>.
				Select(this, currentCaseCd))
			{
				var childCaseCd = item.ChildCaseCD ?? string.Empty;
				if (ht.Contains(childCaseCd)) continue;

				ht.Add(childCaseCd, item);
                var relCase = SelectCase(childCaseCd);
				if (relCase == null)
					continue;

              /*  PXUIFieldAttribute.SetEnabled<CRCaseRelated.status>(CaseRelated.Cache, relCase, false);
                PXUIFieldAttribute.SetEnabled<CRCaseRelated.ownerID>(CaseRelated.Cache, relCase, false);
                PXUIFieldAttribute.SetEnabled<CRCaseRelated.workgroupID>(CaseRelated.Cache, relCase, false);*/

                yield return new PXResult<CRCaseReference, CRCaseRelated>(item, relCase);
			}

			var cache = CaseRefs.Cache;
			var oldIsDirty = cache.IsDirty;

			foreach (CRCaseReference item in 
				PXSelect<CRCaseReference,
					Where<CRCaseReference.childCaseCD, Equal<Required<CRCaseReference.childCaseCD>>>>.
				Select(this, currentCaseCd))
			{
				var parentCaseCd = item.ParentCaseCD ?? string.Empty;
				if (ht.Contains(parentCaseCd)) continue;
				var relCase = SelectCase(parentCaseCd);
				if(relCase == null)
					continue;

				ht.Add(parentCaseCd, item);
				cache.Delete(item);
				var newItem = (CRCaseReference)cache.CreateInstance();
				newItem.ParentCaseCD = currentCaseCd;
				newItem.ChildCaseCD = parentCaseCd;

			    switch (item.RelationType)
			    {
                    case CaseRelationTypeAttribute._DEPENDS_ON_VALUE:
			            newItem.RelationType = CaseRelationTypeAttribute._BLOCKS_VALUE;
                        break;
                    case CaseRelationTypeAttribute._DUBLICATE_OF_VALUE:
			            newItem.RelationType = CaseRelationTypeAttribute._DUBLICATE_OF_VALUE;
                        break;
                    case CaseRelationTypeAttribute._RELATED_VALUE:
			            newItem.RelationType = CaseRelationTypeAttribute._RELATED_VALUE;
                        break;
                    default:
			            newItem.RelationType = CaseRelationTypeAttribute._DEPENDS_ON_VALUE;
			            break;
			    }
				
				newItem = (CRCaseReference)cache.Insert(newItem);
				cache.IsDirty = oldIsDirty;
                yield return new PXResult<CRCaseReference, CRCaseRelated>(newItem, relCase);
			}
		}

		/*public virtual IEnumerable casereferencesdependson()
		{
			var idsHashtable = new Hashtable();
			foreach (RelatedCase item in CaseReferencesDependsOn.Cache.Cached)
			{
				idsHashtable.Add(item.CaseID, item);
				var status = CaseReferencesDependsOn.Cache.GetStatus(item);
				if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated || status == PXEntryStatus.Notchanged)
				{
					var @case = (CRCase)PXSelect<CRCase, Where<CRCase.caseID, Equal<Required<CRCase.caseID>>>>.Select(this, item.CaseID);
					yield return new PXResult<RelatedCase, CRCase>(item, @case);
				}
			}
			foreach (PXResult<CRCaseReference, CRCase> item in
				PXSelectJoin<CRCaseReference,
				InnerJoin<CRCase, On<CRCaseReference.childCaseID, Equal<CRCase.caseID>>>,
				Where<CRCaseReference.parentCaseID, Equal<Current<CRCase.caseID>>>>.
				Select(this))
			{
				var @case = (CRCase)item;
				if (idsHashtable.ContainsKey(@case.CaseID)) continue;

				yield return new PXResult<RelatedCase, CRCase>(
					new RelatedCase
						{
							CaseID = @case.CaseID,
							RelationType = CaseRelationTypeAttribute._DEPENDS_ON_VALUE
						},
					@case);
			}

			foreach (PXResult<CRCaseReference, CRCase> item in
				PXSelectJoin<CRCaseReference,
				InnerJoin<CRCase, On<CRCaseReference.parentCaseID, Equal<CRCase.caseID>>>,
				Where<CRCaseReference.childCaseID, Equal<Current<CRCase.caseID>>>>.
				Select(this))
			{
				var @case = (CRCase)item;
				if (idsHashtable.ContainsKey(@case.CaseID)) continue;

				yield return new PXResult<RelatedCase, CRCase>(
					new RelatedCase
						{
							CaseID = @case.CaseID,
							RelationType = CaseRelationTypeAttribute._BLOCKS_VALUE
						},
					@case);
			}
		}*/

		/*public override void Persist()
		{
			CorrectRelatedCaseRecords();

			base.Persist();
		}*/

		#endregion

		#region Actions

		public new PXSave<CRCase> Save;
		public new PXCancel<CRCase> Cancel;
		public new PXInsert<CRCase> Insert;
		public new PXCopyPasteAction<CRCase> CopyPaste;
		public new PXDelete<CRCase> Delete;
		public new PXFirst<CRCase> First;
		public new PXPrevious<CRCase> Previous;
		public new PXNext<CRCase> Next;
		public new PXLast<CRCase> Last;

		public PXAction<CRCase> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			List<CRCase> list = new List<CRCase>(adapter.Get<CRCase>());
			Save.Press();

			PXLongOperation.StartOperation(this, delegate
				{
					CRCaseMaint graph = PXGraph.CreateInstance<CRCaseMaint>();

					foreach (CRCase @case in list)
					{
						if (@case == null || @case.Released == true) continue;
						graph.CheckBillingSettings(@case);
						graph.ReleaseCase(@case);
					}
				});

			return adapter.Get();
		}

		public PXMenuAction<CRCase> Action;
		public PXMenuInquiry<CRCase> Inquiry;

		public PXAction<CRCase> takeCase;
		[PXUIField(DisplayName = "Take Case", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(PopupVisible = true)]
		public virtual IEnumerable TakeCase(PXAdapter adapter)
		{
		    foreach (CRCase curCase in adapter.Get<CRCase>())
		    {
		        var caseCur = (CRCase)Case.Cache.CreateCopy(curCase);	        
		        caseCur.OwnerID = EmployeeMaint.GetCurrentOwnerID(this);
		        if (caseCur.WorkgroupID != null)
		        {
		            EPCompanyTreeMember member = PXSelect<EPCompanyTreeMember,
		                Where<EPCompanyTreeMember.contactID, Equal<Current<AccessInfo.contactID>>,
		                    And<EPCompanyTreeMember.workGroupID, Equal<Required<CRCase.workgroupID>>>>>.
		                Select(this, caseCur.WorkgroupID);

		            if (member == null)
		            {
		                caseCur.WorkgroupID = null;
		            }
		        }
		        if (caseCur.OwnerID != Case.Current.OwnerID || caseCur.WorkgroupID != Case.Current.WorkgroupID)
		        {
		            caseCur = Case.Update(caseCur);		           
		        }

			    if (this.IsContractBasedAPI)
				    this.Save.Press();

		        yield return caseCur;
		    }
		}

		public PXAction<CRCase> assign;
		[PXUIField(DisplayName = Messages.Assign, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable Assign(PXAdapter adapter)
		{
			if (!Setup.Current.DefaultCaseAssignmentMapID.HasValue)
			{
				throw new PXSetPropertyException(Messages.AssignNotSetupCase, Messages.CRSetup);
			}

			var processor = new EPAssignmentProcessor<CRCase>(this);
			processor.Assign(CaseCurrent.Current, Setup.Current.DefaultCaseAssignmentMapID);

			CaseCurrent.Update(CaseCurrent.Current);
			
			if (this.IsContractBasedAPI)
				this.Save.Press();

			return adapter.Get();
		}

		public PXAction<CRCase> viewInvoice;
		[PXUIField(DisplayName = Messages.ViewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			//Direct Billing:
			if (CaseCurrent.Current != null && !string.IsNullOrEmpty(CaseCurrent.Current.ARRefNbr))
			{
				ARInvoiceEntry target = PXGraph.CreateInstance<ARInvoiceEntry>();
				target.Clear();
				target.Document.Current = target.Document.Search<ARInvoice.refNbr>(CaseCurrent.Current.ARRefNbr);
				throw new PXRedirectRequiredException(target, "ViewInvoice");
			}
			
			//Contract Billing:
			if (CaseCurrent.Current != null && CaseCurrent.Current.ContractID != null)
			{
				PMTran usageTran = PXSelect<PMTran, Where<PMTran.origRefID, Equal<Current<CRCase.noteID>>>>.Select(this);
				if (usageTran != null && !string.IsNullOrEmpty(usageTran.ARRefNbr))
				{
					ARInvoiceEntry target = PXGraph.CreateInstance<ARInvoiceEntry>();
					target.Clear();
					target.Document.Current = target.Document.Search<ARInvoice.refNbr>(usageTran.ARRefNbr);
					throw new PXRedirectRequiredException(target, "ViewInvoice");
				}
			}

			

			return adapter.Get();
		}

        public PXAction<CRCase> addNewContact;
        [PXUIField(DisplayName = Messages.AddNewContact, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(DisplayOnMainToolbar = false)]
        public virtual IEnumerable AddNewContact(PXAdapter adapter)
        {
            if (Case.Current != null && Case.Current.CustomerID != null)
            {
                ContactMaint target = PXGraph.CreateInstance<ContactMaint>();
                target.Clear();
                Contact maincontact = target.Contact.Insert();
                maincontact.BAccountID = Case.Current.CustomerID;

				CRContactClass ocls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>
					.SelectSingleBound(this, new object[] { maincontact });
				if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
				{
					maincontact.WorkgroupID = Case.Current.WorkgroupID;
					maincontact.OwnerID = Case.Current.OwnerID;
				}

                maincontact = target.Contact.Update(maincontact);
                throw new PXRedirectRequiredException(target, true, "Contact") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            return adapter.Get();
        }
		#endregion

		#region Event Handlers

		#region CacheAttached

		[CRMBAccount(bAccountTypes: new[]
		{
			typeof(BAccountType.prospectType),
			typeof(BAccountType.customerType),
			typeof(BAccountType.combinedType),
			typeof(BAccountType.vendorType),
		})]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		[PopupMessage]
		[PXRestrictor(typeof(Where<
			CRCaseClass.requireCustomer.FromCurrent.IsEqual<False>
			.Or<BAccount.type.IsEqual<BAccountType.customerType>>
			.Or<BAccount.type.IsEqual<BAccountType.combinedType>>>), Messages.CustomerRequired, typeof(BAccount.acctCD))]
		[PXRestrictorWithErase(typeof(Where<
			CRCase.caseClassID.FromCurrent.IsNull
			.Or<CRCaseClass.allowEmployeeAsContact.FromCurrent.IsEqual<True>>
			.Or<BAccount.type.IsNull>
			.Or<BAccount.type.IsNotEqual<BAccountType.branchType>>
		>),
			Messages.NonBranchRequired,
			typeof(BAccount.acctName),
			typeof(Current<CRCaseClass.caseClassID>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRCase.customerID> e) { }

		[PXRestrictor(typeof(Where<
			CRCaseClass.requireCustomer.FromCurrent.IsEqual<False>
			.Or<BAccount.type.IsEqual<BAccountType.customerType>>
			.Or<BAccount.type.IsEqual<BAccountType.combinedType>>>), Messages.CustomerRequired, typeof(BAccount.acctCD))]
		[PXRestrictorWithErase(typeof(Where<
			CRCase.caseClassID.FromCurrent.IsNull
			.Or<CRCaseClass.allowEmployeeAsContact.FromCurrent.IsEqual<True>>
			.Or<BAccount.type.IsNull>
			.Or<BAccount.isBranch.IsNotEqual<True>>
		>),
			Messages.NonEmployeeRequired,
			typeof(Contact.displayName),
			typeof(Current<CRCaseClass.caseClassID>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRCase.contactID> e) { }

		[CRCaseBillableTime]
		[PXDBInt]
		[PXUIField(DisplayName = "Billable Time", Enabled = false)]
		protected virtual void _(Events.CacheAttached<CRCase.timeBillable> e) { }

        [PXDBInt]
        [PXUIField(DisplayName = "Contract")]
        [PXSelector(typeof(Search2<Contract.contractID,
                LeftJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>>,
            Where<Contract.baseType, Equal<CTPRType.contract>,
                And<Where<Current<CRCase.customerID>, IsNull,
                        Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
                            And<Current<CRCase.locationID>, IsNull>>,
                        Or2<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
                            And<Current<CRCase.locationID>, IsNull>>,
                        Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
                            And<Contract.locationID, Equal<Current<CRCase.locationID>>>>,
                        Or<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
                            And<ContractBillingSchedule.locationID, Equal<Current<CRCase.locationID>>>>>>>>>>>,
            OrderBy<Desc<Contract.contractCD>>>),
            DescriptionField = typeof(Contract.description),
            SubstituteKey = typeof(Contract.contractCD), Filterable = true)]
        [PXRestrictor(typeof(Where<Contract.status, Equal<Contract.status.active>, Or<Contract.status, Equal<Contract.status.inUpgrade>>>), Messages.ContractIsNotActive)]
        [PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), Messages.ContractExpired)]
        [PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), Messages.ContractActivationDateInFuture, typeof(Contract.startDate))]
        [PXFormula(typeof(Default<CRCase.customerID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void _(Events.CacheAttached<CRCase.contractID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDatetimeScalar(typeof(Search<CRActivityStatistics.lastActivityDate, Where<CRActivityStatistics.noteID, Equal<CRCase.noteID>>>), PreserveTime = true, UseTimeZone = true)]
		protected virtual void _(Events.CacheAttached<CRCase.lastActivity> e) { }

		[PXDBString(15, IsKey = true)]
		[PXDBDefault(typeof(CRCase.caseCD))]
		[PXUIField(Visible = false)]
		protected virtual void _(Events.CacheAttached<CRCaseReference.parentCaseCD> e) { }

		#endregion

		#region CRCase

		protected virtual void _(Events.RowSelected<CRCase> e)
		{
			var caseRow = e.Row as CRCase;
			if (caseRow == null) return;

			var caseClass = CRCase.FK.Class.FindParent(this, e.Row);

			var perItemBilling = false;
			var denyOverrideBillable = false;
			if (caseClass != null)
			{
				denyOverrideBillable = caseClass.AllowOverrideBillable != true;
				perItemBilling = caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity;
			}

			if (caseRow.IsBillable != true)
				caseRow.ManualBillableTimes = false;

			var isNotReleased = caseRow.Released != true;
			if (isNotReleased)
			{
				PXUIFieldAttribute.SetEnabled<CRCase.manualBillableTimes>(e.Cache, caseRow, caseRow.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<CRCase.isBillable>(e.Cache, caseRow, !perItemBilling && !denyOverrideBillable);
				var canModifyBillableTimes = caseRow.IsBillable == true && caseRow.ManualBillableTimes == true;
				PXUIFieldAttribute.SetEnabled<CRCase.timeBillable>(e.Cache, caseRow, canModifyBillableTimes);
				PXUIFieldAttribute.SetEnabled<CRCase.overtimeBillable>(e.Cache, caseRow, canModifyBillableTimes);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, caseRow, false);
			}
			PXUIFieldAttribute.SetEnabled<CRCase.caseCD>(e.Cache, caseRow, true);

			RecalcDetails(e.Cache, caseRow);

			release.SetEnabled(isNotReleased && caseRow.IsBillable == true && !perItemBilling);
			Guid? userID = EmployeeMaint.GetCurrentEmployeeID(this);
			takeCase.SetEnabled(userID != null && caseRow.OwnerID != EmployeeMaint.GetCurrentOwnerID(this) && isNotReleased);

			PXUIFieldAttribute.SetRequired<CRCase.customerID>(e.Cache, (caseRow.IsBillable == true || (caseClass != null && caseClass.RequireCustomer == true)));
			PXUIFieldAttribute.SetRequired<CRCase.contractID>(e.Cache, (caseClass != null && PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>() && caseClass.RequireContract == true));
			PXUIFieldAttribute.SetRequired<CRCase.contactID>(e.Cache, (caseClass != null && caseClass.RequireContact == true));

			var bAccount = PXSelectorAttribute.Select<CRCase.customerID>(e.Cache, e.Row, e.Row.CustomerID) as BAccount;
			PXUIFieldAttribute.SetEnabled<CRCase.customerID>(e.Cache, e.Row, e.Row.ContactID == null || bAccount == null || bAccount.Type != BAccountType.OrganizationType);
		}

		protected virtual void _(Events.RowPersisting<CRCase> e)
		{
			var caseRow = (CRCase) e.Row;
			var caseClass = PXSelectorAttribute.Select<CRCase.caseClassID>(e.Cache, e.Row) as CRCaseClass;

			PXDefaultAttribute.SetPersistingCheck<CRCase.customerID>(e.Cache, caseRow, (caseRow.IsBillable == true || (caseClass != null && caseClass.RequireCustomer == true)) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CRCase.contractID>(e.Cache, caseRow, (caseClass != null && PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>() && caseClass.RequireContract == true) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CRCase.contactID>(e.Cache, caseRow, (caseClass != null && caseClass.RequireContact == true) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			var bAccount = PXSelectorAttribute.Select<CRCase.customerID>(e.Cache, e.Row, e.Row.CustomerID) as BAccount;

			if (bAccount != null && bAccount.Type == BAccountType.BranchType)
			{
				if (caseClass != null && caseClass.AllowEmployeeAsContact == false)
					throw new PXException(Messages.EmployeeCaseClassMismatch);
			}
		}

		protected virtual void _(Events.RowDeleted<CRCase> e)
		{
			var caseRow = (CRCase)e.Row;
			var caseCd = caseRow?.CaseCD;
			if (caseRow == null || String.IsNullOrEmpty(caseCd))
				return;

			var referenceCache = CaseRefs.Cache;
			var ht = new HybridDictionary();
			foreach (CRCaseReference item in
				PXSelect<CRCaseReference,
					Where<CRCaseReference.parentCaseCD, Equal<Required<CRCaseReference.parentCaseCD>>>>.
				Select(this, caseCd))
			{
				var childCaseCd = item.ChildCaseCD ?? string.Empty;
				if (ht.Contains(childCaseCd)) continue;

				ht.Add(childCaseCd, item);
				referenceCache.Delete(item);
			}

			foreach (CRCaseReference item in
				PXSelect<CRCaseReference,
					Where<CRCaseReference.childCaseCD, Equal<Required<CRCaseReference.childCaseCD>>>>.
				Select(this, caseCd))
			{
				var parentCaseCd = item.ParentCaseCD ?? string.Empty;
				if (ht.Contains(parentCaseCd)) continue;

				ht.Add(parentCaseCd, item);
				referenceCache.Delete(item);
			}
		}

		protected virtual void _(Events.FieldDefaulting<CRCase, CRCase.sLAETA> e)
		{
			CRCase row = e.Row as CRCase;
			if (row == null || row.CreatedDateTime == null) return;

			if (row.ClassID != null && row.Severity != null)
			{
				var severity = (CRClassSeverityTime)PXSelect<CRClassSeverityTime,
														Where<CRClassSeverityTime.caseClassID, Equal<Required<CRClassSeverityTime.caseClassID>>,
														And<CRClassSeverityTime.severity, Equal<Required<CRClassSeverityTime.severity>>>>>.
														Select(this, row.ClassID, row.Severity);
				if (severity != null && severity.TimeReaction != null)
				{
					e.NewValue = ((DateTime)row.CreatedDateTime).AddMinutes((int)severity.TimeReaction);
					e.Cancel = true;
				}
			}

			if (row.Severity != null && row.ContractID != null)
			{
				var template = (Contract)PXSelect<Contract, Where<Contract.contractID, Equal<Required<CRCase.contractID>>>>.Select(this, row.ContractID);
				if (template == null) return;
				
				var sla = (ContractSLAMapping)PXSelect<ContractSLAMapping,
												  Where<ContractSLAMapping.severity, Equal<Required<CRCase.severity>>,
												  And<ContractSLAMapping.contractID, Equal<Required<CRCase.contractID>>>>>.
												  Select(this, row.Severity, template.TemplateID);
				if (sla != null && sla.Period != null)
				{
					e.NewValue = ((DateTime)row.CreatedDateTime).AddMinutes((int)sla.Period);
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.RowUpdated<CRCase> e)
		{
			var row = e.Row as CRCase;
			var oldRow = e.OldRow as CRCase;
			if (row == null || oldRow == null) return;

			if (row.OwnerID == null)
			{
				row.AssignDate = null;
			}
			else if (oldRow.OwnerID == null)
			{
				row.AssignDate = PXTimeZoneInfo.Now;
			}
		}

		protected virtual void _(Events.FieldDefaulting<CRCase, CRCase.contractID> e)
		{
			CRCase row = e.Row as CRCase;
			if (row == null || row.CustomerID == null) return;

			List<object> contracts = PXSelectorAttribute.SelectAll<CRCase.contractID>(e.Cache, e.Row);
			if (contracts.Exists(contract => PXResult.Unwrap<Contract>(contract).ContractID == row.ContractID))
			{
				e.NewValue = row.ContractID;
			}
			else if (contracts.Count == 1)
			{
				e.NewValue = PXResult.Unwrap<Contract>(contracts[0]).ContractID;
			}
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldUpdating<CRCase, CRCase.contractID> e)
		{
			CRCase crcase = (CRCase)e.Row;
			Contract contract = PXResult.Unwrap<Contract>(PXSelectorAttribute.Select<CRCase.contractID>(e.Cache, e.Row, e.NewValue));
			if (crcase == null || contract == null) return;

			int daysLeft;
			if (Accessinfo.BusinessDate != null
				&& ContractMaint.IsInGracePeriod(contract, (DateTime)Accessinfo.BusinessDate, out daysLeft))
			{
				e.Cache.RaiseExceptionHandling<CRCase.contractID>(crcase, e.NewValue, new PXSetPropertyException(Messages.ContractInGracePeriod, PXErrorLevel.Warning, daysLeft));
			}
		}

		protected virtual void _(Events.FieldUpdated<CRCase, CRCase.caseClassID> e)
		{
			if (e.Row == null || e.OldValue == null)
				return;

			var oldCaseClass = PXSelectorAttribute.Select<CRCase.caseClassID>(e.Cache, e.Row, e.OldValue) as CRCaseClass;

			if (oldCaseClass == null || oldCaseClass.AllowEmployeeAsContact != true)
				return;

			if (e.Row.ContactID == null)
			{
				e.Row.CustomerID = null;
			}
		}

		#endregion

		#region CRCaseReference

		protected virtual void _(Events.RowInserted<CRCaseReference> e)
		{
			var row = e.Row as CRCaseReference;
			if (row == null || row.ChildCaseCD == null || row.ParentCaseCD == null) return;

			var alternativeRecord = (CRCaseReference)PXSelect<CRCaseReference,
				Where<CRCaseReference.parentCaseCD, Equal<Required<CRCaseReference.parentCaseCD>>,
					And<CRCaseReference.childCaseCD, Equal<Required<CRCaseReference.childCaseCD>>>>>.
				Select(this, row.ChildCaseCD, row.ParentCaseCD);
			if (alternativeRecord != null)
				e.Cache.Delete(alternativeRecord);
		}

		protected virtual void _(Events.FieldUpdating<CRCaseReference, CRCaseReference.relationType> e) { }

		protected virtual void _(Events.FieldVerifying<CRCaseReference, CRCaseReference.childCaseCD> e)
		{
			var row = e.Row as CRCaseReference;
			if (row == null || e.NewValue == null) return;

			if (object.Equals(row.ParentCaseCD, e.NewValue))
			{
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.CaseCannotDependUponItself);
			}
		}

		#endregion

		#endregion

		#region Private Methods

		private CRCaseRelated SelectCase(object caseCd)
		{
			if (caseCd == null) return null;

            return (CRCaseRelated)PXSelect<CRCaseRelated,
                Where<CRCaseRelated.caseCD, Equal<Required<CRCase.caseCD>>>>.
				Select(this, caseCd);
		}
        
		protected virtual void ReleaseCase(CRCase item)
		{
            RegisterEntry registerEntry = (RegisterEntry)PXGraph.CreateInstance(typeof(RegisterEntry));

            PXSelectBase<EPActivityApprove> select = new PXSelect<EPActivityApprove,
                Where<EPActivityApprove.refNoteID, Equal<Required<EPActivityApprove.refNoteID>>>>(this);

            List<EPActivityApprove> list = new List<EPActivityApprove>();
            foreach (EPActivityApprove activity in select.Select(item.NoteID))
            {
                list.Add(activity);

				if ((activity.TimeSpent.GetValueOrDefault() != 0 || activity.TimeBillable.GetValueOrDefault() != 0) && activity.ApproverID != null && activity.ApprovalStatus != ActivityStatusListAttribute.Completed && activity.ApprovalStatus != ActivityStatusListAttribute.Canceled)
                {
                    throw new PXException(Messages.OneOrMoreActivitiesAreNotApproved);
                }
            }

			bool tranAdded = false;

            if (item.ContractID != null)
            {
                //Contract Billing:

	            using (PXTransactionScope ts = new PXTransactionScope())
	            {
		            RecordContractUsage(item);

                    if (!EmployeeActivitiesRelease.RecordCostTrans(registerEntry, list, out tranAdded))
                    {
                        throw new PXException(Messages.FailedRecordCost);
                    }
	                this.TimeStamp = registerEntry.TimeStamp;
                    //foreach (EPActivity activity in PXSelect<EPActivity, Where<EPActivity.refNoteID, Equal<Required<CRCase.noteID>>>>.Select(this, item.NoteID))
                    //{
                    //    activity.tstamp = registerEntry.TimeStamp;
                    //}


                    item.Released = true;
                                        string saveResolution = item.Resolution;
										Case.Update(item);
                                        item.Resolution = saveResolution;
										this.Save.Press();
                    ts.Complete();
                }
            }
            else
            {
                //Direct Billing:
                Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, item.CustomerID);
                if (customer == null)
                {
                    throw new PXException(Messages.CustomerNotFound);
                }
			
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                    invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
                    invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

                    ARInvoice invoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].CreateInstance();
                    invoice.DocType = ARDocType.Invoice;
                    invoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].Insert(invoice);
					ARInvoice copy = (ARInvoice)invoiceEntry.Document.Cache.CreateCopy(invoiceEntry.Document.Current);
					copy.CustomerID = item.CustomerID;
					copy.CustomerLocationID = item.LocationID;
					copy.DocDate = Accessinfo.BusinessDate;
					copy.DocDesc = item.Subject;
					invoice = invoiceEntry.Document.Update(copy);

					invoiceEntry.Caches[typeof(ARInvoice)].SetValueExt<ARInvoice.hold>(invoice, false);                    
					invoiceEntry.customer.Current.CreditRule = customer.CreditRule;
					invoice = invoiceEntry.Document.Update(invoice);
                    foreach (ARTran tran in GenerateARTrans(item))
                    {
                        invoiceEntry.Transactions.Insert(tran);
                    }
					ARInvoice oldInvoice = (ARInvoice)invoiceEntry.Caches[typeof(ARInvoice)].CreateCopy(invoice);
					invoice.CuryOrigDocAmt = invoice.CuryDocBal;
                    invoice.OrigDocAmt = invoice.DocBal;
					invoiceEntry.Caches[typeof(ARInvoice)].RaiseRowUpdated(invoice, oldInvoice);
					invoiceEntry.Caches[typeof(ARInvoice)].SetValue<ARInvoice.curyOrigDocAmt>(invoice, invoice.CuryDocBal);

                    invoiceEntry.Actions.PressSave();

                    item.Released = true;                    
                    item.ARRefNbr = invoiceEntry.Document.Current.RefNbr;
                    string saveResolution = item.Resolution;
                    Case.Update(item);
                    item.Resolution = saveResolution;
                    this.Save.Press();
					
                    if (!EmployeeActivitiesRelease.RecordCostTrans(registerEntry, list, out tranAdded))
                    {
                        throw new PXException(Messages.FailedRecordCost);
                    }


                    ts.Complete();
                }
            }

			if (tranAdded)//there can be no cost transactions at all - they were created when a timecard was released.
            {
                EPSetup setup = PXSelect<EPSetup>.Select(registerEntry);

                if (setup != null && setup.AutomaticReleasePM == true)
                {
                    PX.Objects.PM.RegisterRelease.Release(registerEntry.Document.Current);
                }
            }
		}
        
        protected virtual void RecordContractUsage(CRCase item)
        {
            RegisterEntry registerEntry = CreateInstance<RegisterEntry>();
            registerEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            registerEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.
            registerEntry.Document.Cache.Insert();
            registerEntry.Document.Current.Description = item.Subject;
            registerEntry.Document.Current.Released = true;
			registerEntry.Document.Current.Status = PMRegister.status.Released;
			registerEntry.EnsureCachePersistence(typeof(CRPMTimeActivity));

	        foreach (PMTran tran in GeneratePMTrans(item))
            {
				registerEntry.Transactions.Insert(tran);
				UsageMaint.AddUsage(registerEntry.ContractDetails.Cache, tran.ProjectID, tran.InventoryID, tran.BillableQty ?? 0m, tran.UOM);
			}

            item.Released = true;
            string saveResolution = item.Resolution;
            Case.Update(item);
            item.Resolution = saveResolution;

            registerEntry.Save.Press();
        }
        
		public override object GetValueExt(string viewName, object data, string fieldName)
		{
			object ret = base.GetValueExt(viewName, data, fieldName);
			if (String.Equals(viewName, "CaseCurrent", StringComparison.OrdinalIgnoreCase) && String.Equals(fieldName, "CustomerID", StringComparison.OrdinalIgnoreCase) && ret is PXFieldState && !String.IsNullOrEmpty(((PXFieldState)ret).Error))
			{
				((PXFieldState)ret).Error = null;
			}
			return ret;
		}
        
        protected virtual List<PMTran> GeneratePMTrans(CRCase @case)
		{
            Contract contract = PXSelect<Contract,
				Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.
				Select(this, @case.ContractID);

            CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, @case.CaseClassID);

			List<PMTran> result = new List<PMTran>();

            DateTime startDate = Accessinfo.BusinessDate ?? (DateTime)@case.CreatedDateTime;
			DateTime endDate = startDate.Add(new TimeSpan(0, (@case.TimeBillable ?? 0), 0));

			PXResultset<CRPMTimeActivity> list = PXSelect<CRPMTimeActivity,
				Where<CRPMTimeActivity.refNoteID, Equal<Required<CRPMTimeActivity.refNoteID>>>,
				OrderBy<Desc<CRPMTimeActivity.createdDateTime>>>.
				Select(this, @case.NoteID);

			#region For Case without activities
			if (list.Count > 0)
			{
				startDate = (DateTime)((CRPMTimeActivity)list[0]).StartDate;
				endDate = startDate;
			}
			#endregion

			PXCache cache = null;
			foreach (CRPMTimeActivity activity in list)
			{
				if (cache == null) cache = Caches[activity.GetType()];
				if (activity.ClassID == CRActivityClass.Activity && activity.IsBillable == true)
				{
					if (activity.StartDate != null && (DateTime)activity.StartDate < startDate)
					{
						startDate = (DateTime)activity.StartDate;
					}

					if (activity.EndDate != null && (DateTime)activity.EndDate > endDate)
					{
						endDate = (DateTime)activity.EndDate;
					}
					activity.Billed = true;
				}
				activity.Released = true;				
				cache.Update(activity);
			}

            if (contract.CaseItemID != null)
			{
				InventoryItem item = PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                    Select(this, contract.CaseItemID);

				PMTran newTran = new PMTran();
				newTran.ProjectID = contract.ContractID;
                newTran.InventoryID = contract.CaseItemID;
				newTran.AccountGroupID = contract.ContractAccountGroup;
				newTran.OrigRefID = @case.NoteID;
				newTran.BAccountID = @case.CustomerID;
				newTran.LocationID = @case.LocationID;
				newTran.Description = @case.Subject;
				newTran.StartDate = startDate;
				newTran.EndDate = endDate;
			    newTran.Date = endDate;
				newTran.Qty = 1;
				newTran.BillableQty = 1;
				newTran.UOM = item.SalesUnit;
				newTran.Released = true;
				newTran.Allocated = true;
				newTran.BillingID = contract.BillingID;
				newTran.IsQtyOnly = true;
				newTran.CaseCD = @case.CaseCD;
				result.Add(newTran);
			}

			#region Record Labor Usage
            if (caseClass.LabourItemID != null)
			{
				int totalBillableMinutes = (@case.TimeBillable ?? 0);

                if (caseClass.OvertimeItemID != null)
				{
					totalBillableMinutes -= (@case.OvertimeBillable ?? 0);
				}

				if (totalBillableMinutes > 0)
				{
					if (caseClass.PerItemBilling == BillingTypeListAttribute.PerCase && caseClass.RoundingInMinutes > 1)
					{
						decimal fraction = Convert.ToDecimal(totalBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
						int points = Convert.ToInt32(Math.Ceiling(fraction));
						totalBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
					}

					if (caseClass.PerItemBilling == BillingTypeListAttribute.PerCase && caseClass.MinBillTimeInMinutes > 0)
					{
						totalBillableMinutes = Math.Max(totalBillableMinutes, (int)caseClass.MinBillTimeInMinutes);
					}

					InventoryItem item = PXSelect<InventoryItem,
						Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                        Select(this, caseClass.LabourItemID);
					PMTran newLabourTran = new PMTran();
					newLabourTran.ProjectID = contract.ContractID;
                    newLabourTran.InventoryID = caseClass.LabourItemID;
					newLabourTran.AccountGroupID = contract.ContractAccountGroup;
					newLabourTran.OrigRefID = @case.NoteID;
					newLabourTran.BAccountID = @case.CustomerID;
					newLabourTran.LocationID = @case.LocationID;
					newLabourTran.Description = @case.Subject;
					newLabourTran.StartDate = startDate;
					newLabourTran.EndDate = endDate;
                    newLabourTran.Date = endDate;
					newLabourTran.UOM = item.SalesUnit;
					newLabourTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalBillableMinutes).TotalHours);
					newLabourTran.BillableQty = newLabourTran.Qty;
					newLabourTran.Released = true;
					newLabourTran.Allocated = true;
					newLabourTran.BillingID = contract.BillingID;
					newLabourTran.IsQtyOnly = true;
					newLabourTran.CaseCD = @case.CaseCD;
					result.Add(newLabourTran);
				}
			}
			#endregion

			#region Record Overtime Usage

            if (caseClass.OvertimeItemID.HasValue)
			{
				InventoryItem item = PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                    Select(this, caseClass.OvertimeItemID);
				int totalOvertimeBillableMinutes = (@case.OvertimeBillable ?? 0);

				if (totalOvertimeBillableMinutes > 0)
				{
					if (caseClass.PerItemBilling == BillingTypeListAttribute.PerCase && caseClass.RoundingInMinutes > 1)
					{
						decimal fraction = Convert.ToDecimal(totalOvertimeBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
						int points = Convert.ToInt32(Math.Ceiling(fraction));
						totalOvertimeBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
					}

					PMTran newOvertimeTran = new PMTran();
					newOvertimeTran.ProjectID = contract.ContractID;
                    newOvertimeTran.InventoryID = caseClass.OvertimeItemID;
					newOvertimeTran.AccountGroupID = contract.ContractAccountGroup;
					newOvertimeTran.OrigRefID = @case.NoteID;
					newOvertimeTran.BAccountID = @case.CustomerID;
					newOvertimeTran.LocationID = @case.LocationID;
					newOvertimeTran.Description = @case.Subject;
					newOvertimeTran.StartDate = startDate;
					newOvertimeTran.EndDate = endDate;
                    newOvertimeTran.Date = endDate;
					newOvertimeTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalOvertimeBillableMinutes).TotalHours);
					newOvertimeTran.BillableQty = newOvertimeTran.Qty;
					newOvertimeTran.UOM = item.SalesUnit;
					newOvertimeTran.Released = true;
					newOvertimeTran.Allocated = true;
					newOvertimeTran.BillingID = contract.BillingID;
					newOvertimeTran.IsQtyOnly = true;
					newOvertimeTran.CaseCD = @case.CaseCD;
					result.Add(newOvertimeTran);
				}
			}

			#endregion


			return result;
		}

        protected virtual List<ARTran> GenerateARTrans(CRCase c)
        {
            CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, c.CaseClassID);

            List<ARTran> result = new List<ARTran>();

            DateTime startDate = (DateTime)c.CreatedDateTime;
            DateTime endDate = startDate.Add(new TimeSpan(0, (c.TimeBillable ?? 0), 0));

            PXResultset<CRPMTimeActivity> list = PXSelect<CRPMTimeActivity,
                Where<CRPMTimeActivity.refNoteID, Equal<Required<CRPMTimeActivity.refNoteID>>>,
                OrderBy<Desc<CRPMTimeActivity.createdDateTime>>>.
                Select(this, c.NoteID);

            #region For Case without activities
            if (list.Count > 0)
            {
                startDate = (DateTime)((CRPMTimeActivity)list[0]).StartDate;
                endDate = startDate;
            }
            #endregion

            PXCache cache = null;
            foreach (CRPMTimeActivity activity in list)
            {
                if (cache == null) cache = Caches[activity.GetType()];
                cache.Current = activity;
                if (activity.ClassID == CRActivityClass.Activity && activity.IsBillable == true)
                {
                    if (activity.StartDate != null && (DateTime)activity.StartDate < startDate)
                    {
                        startDate = (DateTime)activity.StartDate;
                    }

                    if (activity.EndDate != null && (DateTime)activity.EndDate > endDate)
                    {
                        endDate = (DateTime)activity.EndDate;
                    }
                    activity.Billed = true;
					cache.Update(activity);
				}
            }
            
            #region Record Labor Usage
            if (caseClass.LabourItemID != null)
            {
                int totalBillableMinutes = (c.TimeBillable ?? 0);

                if (caseClass.OvertimeItemID != null)
                {
                    totalBillableMinutes -= (c.OvertimeBillable ?? 0);
                }

                if (totalBillableMinutes > 0)
                {
                    if (caseClass.RoundingInMinutes > 1)
                    {
                        decimal fraction = Convert.ToDecimal(totalBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
                        int points = Convert.ToInt32(Math.Ceiling(fraction));
                        totalBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
                    }

                    if (caseClass.MinBillTimeInMinutes > 0)
                    {
                        totalBillableMinutes = Math.Max(totalBillableMinutes, (int)caseClass.MinBillTimeInMinutes);
                    }

                    InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, caseClass.LabourItemID);
                    ARTran newLabourTran = new ARTran();
                    newLabourTran.InventoryID = caseClass.LabourItemID;
                    newLabourTran.TranDesc = c.Subject;
                    newLabourTran.UOM = item.SalesUnit;
                    newLabourTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalBillableMinutes).TotalHours);
					newLabourTran.CaseCD = c.CaseCD;
                    newLabourTran.ManualPrice = false;
                    
                    result.Add(newLabourTran);
                }
            }
            #endregion

            #region Record Overtime Usage

            if (caseClass.OvertimeItemID.HasValue)
            {
                InventoryItem item = PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
                    Select(this, caseClass.OvertimeItemID);
                int totalOvertimeBillableMinutes = (c.OvertimeBillable ?? 0);

                if (totalOvertimeBillableMinutes > 0)
                {
                    if (caseClass.RoundingInMinutes > 1)
                    {
                        decimal fraction = Convert.ToDecimal(totalOvertimeBillableMinutes) / Convert.ToDecimal(caseClass.RoundingInMinutes);
                        int points = Convert.ToInt32(Math.Ceiling(fraction));
                        totalOvertimeBillableMinutes = points * (caseClass.RoundingInMinutes ?? 0);
                    }

                    ARTran newOvertimeTran = new ARTran();
                    newOvertimeTran.InventoryID = caseClass.OvertimeItemID;
                    newOvertimeTran.TranDesc = c.Subject;
                    newOvertimeTran.UOM = item.SalesUnit;
                    newOvertimeTran.Qty = Convert.ToDecimal(TimeSpan.FromMinutes(totalOvertimeBillableMinutes).TotalHours);
					newOvertimeTran.CaseCD = c.CaseCD;
                    newOvertimeTran.ManualPrice = true;
                    
                    result.Add(newOvertimeTran);
                }
            }

            #endregion


            return result;
        }

		private void RecalcDetails(PXCache sender, CRCase row)
		{
			using (new PXConnectionScope())
			{
				CRPMTimeActivity firstResponseActivity = null;

				CRCase cached = row;
				if (row.InitResponse == null)
				{
					cached = (CRCase)sender.Locate(row);
				}

				var activitiesExt = this.GetExtension<CRCaseMaint_ActivityDetailsExt>();

				var select = activitiesExt.Activities.View.BqlSelect
					.WhereNew<Where<CRActivity.refNoteID, Equal<Current<CRCase.noteID>>,
						And2<Where<CRActivity.isPrivate, IsNull, Or<CRActivity.isPrivate, Equal<False>>>,
						And<CRActivity.ownerID, IsNotNull,
						And2<Where<CRActivity.incoming, IsNull, Or<CRActivity.incoming, Equal<False>>>,
						And<Where<CRActivity.isExternal, IsNull, Or<CRActivity.isExternal, Equal<False>>>>>>>>>()
					.OrderByNew<OrderBy<Asc<CRPMTimeActivity.startDate>>>();

				PXView view = new PXView(this, true, select);
				PXResult<CRPMTimeActivity, CRReminder> res = (PXResult<CRPMTimeActivity, CRReminder>)view.SelectSingleBound(new object[] { cached });

				firstResponseActivity = res;

				if (firstResponseActivity != null && firstResponseActivity.StartDate != null)
				{
					TimeSpan createDate = new TimeSpan(row.CreatedDateTime.Value.Ticks);
					TimeSpan action = new TimeSpan(firstResponseActivity.StartDate.Value.Ticks);
					sender.SetValue<CRCase.initResponse>(row, ((int)action.TotalMinutes - (int)createDate.TotalMinutes));
					sender.RaiseFieldUpdated<CRCase.initResponse>(row, null);
				}
			}
		}

		private bool VerifyField<TField>(object row, object newValue)
			where TField : IBqlField
		{
			if (row == null) return true;

			var result = false;
			var cache = Caches[row.GetType()];
			try
			{
				result = cache.RaiseFieldVerifying<TField>(row, ref newValue);
			}
			catch (StackOverflowException) { throw; }
			catch (OutOfMemoryException) { throw; }
			catch (Exception) { }

			return result;
		}

		private void CheckBillingSettings(CRCase @case)
		{

			CRPMTimeActivity activity = PXSelectReadonly<CRPMTimeActivity,
				Where<CRPMTimeActivity.isBillable, Equal<True>,
					And2<Where<CRPMTimeActivity.uistatus, IsNull,
						Or<CRPMTimeActivity.uistatus, Equal<ActivityStatusAttribute.open>>>,
					And<Where<CRPMTimeActivity.refNoteID, Equal<Current<CRCase.noteID>>>>>>>.SelectSingleBound(this, new object[] { @case });
			if (activity != null)
			{
				throw new PXException(Messages.CloseCaseWithHoldActivities);
			}

			CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.Select(this, @case.CaseClassID);


			if (caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity)
			{
				throw new PXException(Messages.OnlyBillByActivity);
			}

		    if (@case.IsBillable == true)
		    {
		        if (@case.ContractID == null)
		        {
		            if (caseClass.LabourItemID == null)
		                throw new PXException(Messages.CaseClassDetailsIsNotSet);
					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, caseClass.LabourItemID);
			        if (item != null && item.SalesAcctID == null)
			        {
						throw new PXException(Messages.SalesAccountIsNotSetForLaborItem);
			        }
		        }
		        else
		        {
		            Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Current<CRCase.contractID>>>>.SelectSingleBound(this, new object[] {@case});

		            if (caseClass.LabourItemID == null && contract.CaseItemID == null)
		            {
		                throw new PXException(Messages.CaseClassDetailsIsNotSet);
		            }
		        }
		    }

		}
		#endregion
	}
}
