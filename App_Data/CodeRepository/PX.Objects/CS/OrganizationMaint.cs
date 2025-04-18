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
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Localization;
using PX.Data.Update.ExchangeService;
using PX.Objects.CM;
using PX.Objects.Common.EntityInUse;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.Relational;
using PX.Objects.CS.DAC;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GraphExtensions.ExtendBAccount;
using PX.Objects.IN;
using PX.SM;

using Branch = PX.Objects.GL.Branch;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.CS
{
	public class OrganizationMaint : OrganizationUnitMaintBase<OrganizationBAccount, Where<Match<Current<AccessInfo.userName>>>>
	{
		public const string canadianCountryCode = "CA";
		protected virtual void OrganizationFilter()
		{ 
			BAccount.WhereAnd<Where<OrganizationBAccount.organizationType.IsNotEqual<OrganizationTypes.group>>>(); 
		}

		#region Repository methods

		public static Organization FinOrganizationByBAccountID(PXGraph graph, int? bAccountID)
		{
			return PXSelectReadonly<Organization,
									Where<Organization.organizationID, Equal<Required<Organization.organizationID>>>>
									.Select(graph, bAccountID);
		}

		public static Organization FindOrganizationByID(PXGraph graph, int? organizationID, bool isReadonly = true)
		{
			return FindOrganizationByIDs(graph,
											organizationID != null ? organizationID.SingleToArray() : null,
											isReadonly)
										.SingleOrDefault();
		}

		public static IEnumerable<Organization> FindOrganizationByIDs(PXGraph graph, int?[] organizationIDs, bool isReadonly = true)
		{
			if (organizationIDs == null || !organizationIDs.Any())
				return new Organization[0];

			if (isReadonly)
			{
				return PXSelectReadonly<Organization,
										Where<Organization.organizationID, In<Required<Organization.organizationID>>>>
										.Select(graph, organizationIDs)
										.RowCast<Organization>();
			}
			else
			{
				return PXSelect<Organization,
								Where<Organization.organizationID, In<Required<Organization.organizationID>>>>
								.Select(graph, organizationIDs)
								.RowCast<Organization>();
			}
		}

		public static Organization FindOrganizationByCD(PXGraph graph, string organizationCD, bool isReadonly = true)
		{
			if (isReadonly)
			{
				return PXSelectReadonly<Organization,
						Where<Organization.organizationCD, Equal<Required<Organization.organizationCD>>>>
					.Select(graph, organizationCD);
			}
			else
			{
				return PXSelect<Organization,
						Where<Organization.organizationCD, Equal<Required<Organization.organizationCD>>>>
					.Select(graph, organizationCD);
			}
		}

		public static Contact GetDefaultContact(PXGraph graph, int? organizationID)
		{
			foreach (PXResult<Organization, BAccountR, Contact> res in
				PXSelectJoin<Organization,
						LeftJoin<BAccountR,
							On<Organization.bAccountID, Equal<BAccountR.bAccountID>>,
						LeftJoin<Contact,
							On<BAccountR.defContactID, Equal<Contact.contactID>>>>,
						Where<Organization.organizationID, Equal<Required<Organization.organizationID>>>>
					.Select(graph, organizationID))
			{
				return (Contact)res;
			}

			return null;
		}

		public static IEnumerable<Contact> GetDefaultContactForCurrentOrganization(PXGraph graph)
		{
			int? organizationID = PXAccess.GetParentOrganizationID(graph.Accessinfo.BranchID);

			Contact contact = GetDefaultContact(graph, organizationID);

			return contact != null ? new Contact[] { contact } : new Contact[] { };
		}

		#endregion

		#region Public Helpers

		public static void RedirectTo(int? organizationID)
		{
			var organizationMaint = CreateInstance<OrganizationMaint>();

			Organization organization = FindOrganizationByID(organizationMaint, organizationID);

			if (organization == null)
				return;

			organizationMaint.BAccount.Current = organizationMaint.BAccount.Search<OrganizationBAccount.bAccountID>(organization.BAccountID);

			throw new PXRedirectRequiredException(organizationMaint, true, string.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		#endregion

		#region Custom Graphs

		protected class SeparateBranchMaint : PXGraph<BranchMaint>
		{
			public PXSelect<GL.Branch> BranchView;
		}

		#endregion

		#region Custom Actions

		public class OrganizationChangeID : PXChangeID<OrganizationBAccount, OrganizationBAccount.acctCD>
		{
			public OrganizationChangeID(PXGraph graph, string name) : base(graph, name) { }

			public OrganizationChangeID(PXGraph graph, Delegate handler) : base(graph, handler) { }

			protected override void Initialize()
			{
				DuplicatedKeyMessage = EP.Messages.BAccountExists;

				_Graph.FieldUpdated.AddHandler<OrganizationBAccount.acctCD>((sender, e) =>
				{
					string oldCD = (string)e.OldValue;
					string newCD = ((OrganizationBAccount)e.Row).AcctCD;
					int? id = ((OrganizationBAccount)e.Row).BAccountID;
					if (oldCD == null || newCD == null) return;

					Organization org = PXSelect<Organization, Where<Organization.organizationCD, Equal<Required<Organization.organizationCD>>>>.Select(_Graph, oldCD);
					if (org?.OrganizationType == OrganizationTypes.WithoutBranches && id > 0)
					{
						ChangeCD<GL.Branch.branchCD>(_Graph.Caches<GL.Branch>(), oldCD, newCD);
						_Graph.Caches<GL.Branch>().Normalize();
					}
					ChangeCD<Organization.organizationCD>(_Graph.Caches<Organization>(), oldCD, newCD);
					_Graph.Caches<Organization>().Normalize();
				});

				base.Initialize();
			}

            [PXButton(CommitChanges = true, Category = CS.ActionCategories.Other)]
            [PXUIField(DisplayName = "Change ID", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
            protected override IEnumerable Handler(PXAdapter adapter)
            {
                return base.Handler(adapter);
            }
        }

		#endregion

		#region Types

		[Serializable]
		public class State : IBqlTable
		{
			public abstract class clearAccessRoleOnChildBranches : PX.Data.BQL.BqlBool.Field<clearAccessRoleOnChildBranches> { }
			[PXBool]
			public bool? ClearAccessRoleOnChildBranches { get; set; }

			public abstract class isBranchTabVisible : PX.Data.BQL.BqlBool.Field<isBranchTabVisible> { }
			[PXBool]
			public bool? IsBranchTabVisible { get; set; }

			public abstract class isDeliverySettingsTabVisible : PX.Data.BQL.BqlBool.Field<isDeliverySettingsTabVisible> { }
			[PXBool]
			public bool? IsDeliverySettingsTabVisible { get; set; }

			public abstract class isGLAccountsTabVisible : PX.Data.BQL.BqlBool.Field<isGLAccountsTabVisible> { }
			[PXBool]
			public bool? IsGLAccountsTabVisible { get; set; }

			public abstract class isRutRotTabVisible : PX.Data.BQL.BqlBool.Field<isRutRotTabVisible> { }
			[PXBool]
			public bool? IsRutRotTabVisible { get; set; }

			public abstract class isCompanyGroupsVisible : PX.Data.BQL.BqlBool.Field<isCompanyGroupsVisible> { }
			[PXBool]
			public bool? IsCompanyGroupsVisible { get; set; }
		}

		#endregion

		#region CTor + Public members

		public PXSelect<Organization, Where<Organization.bAccountID, Equal<Current<OrganizationBAccount.bAccountID>>>> OrganizationView;

		public PXSelect<OrganizationBAccount, Where<OrganizationBAccount.bAccountID, Equal<Current<OrganizationBAccount.bAccountID>>>> CurrentBAccount;

		public PXSelectJoin<GL.Branch,
								LeftJoin<Roles,
									On<GL.Branch.roleName, Equal<Roles.rolename>>>,
								Where<GL.Branch.organizationID, Equal<Current<Organization.organizationID>>>>
								BranchesView;

		public PXSelectJoin<EPEmployee,
								InnerJoin<BAccount2,
									On<EPEmployee.parentBAccountID, Equal<BAccount2.bAccountID>>,
								InnerJoin<GL.BranchAlias,
									On<BAccount2.bAccountID, Equal<GL.BranchAlias.bAccountID>>,
								InnerJoin<Contact,
									On<Contact.contactID, Equal<EPEmployee.defContactID>,
										And<Contact.bAccountID, Equal<EPEmployee.parentBAccountID>>>,
								LeftJoin<Address,
									On<Address.addressID, Equal<EPEmployee.defAddressID>,
										And<Address.bAccountID, Equal<EPEmployee.parentBAccountID>>>>>>>,
								Where<GL.BranchAlias.organizationID, Equal<Current<Organization.organizationID>>>>
								Employees;

		public override PXSelectBase<EPEmployee> EmployeesAccessor => Employees;

		public PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Current<OrganizationBAccount.noteID>>>> Notedocs;


		public PXSelect<CommonSetup> Commonsetup;
		public PXSelect<CurrencyList, Where<CurrencyList.curyID, Equal<Current<Organization.baseCuryID>>>> CompanyCurrency;
		public PXSelect<Currency, Where<Currency.curyID, Equal<Current<CurrencyList.curyID>>>> FinancinalCurrency;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Optional<Currency.curyInfoID>>>> currencyinfo;


		public PXFilter<State> StateView;

		/// <summary>
		/// The obviously declared view which provides cache for SetVisible function in <see cref="OrganizationBAccount_RowSelected"/>.
		/// </summary>
		public PXSelect<BranchAlias> BranchAliasView;

		public PXSelect<OrganizationFinYear> OrganizationYear;
		public PXSelect<OrganizationFinPeriod> OrganizationPeriods;

		public SelectFrom<GroupOrganizationLink>
			.InnerJoin<Organization>
				.On<GroupOrganizationLink.groupID.IsEqual<Organization.organizationID>>
			.Where<GroupOrganizationLink.organizationID.IsEqual<Organization.organizationID.FromCurrent>>.View Groups;
		public PXSelect<FABookYear> FaBookYear;
		public PXSelect<FABookPeriod> FaBookPeriod;

		#region Actions


		public PXAction<OrganizationBAccount> AddLedger;
		public PXAction<OrganizationBAccount> AddBranch;
		public PXAction<OrganizationBAccount> ViewBranch;
		public PXAction<GroupOrganizationLink> ViewGroup;
		public PXAction<GroupOrganizationLink> SetAsPrimary;
		public PXAction<OrganizationBAccount> Activate;
		public PXAction<OrganizationBAccount> Deactivate;

		#endregion

		public OrganizationMaint()
		{
			OrganizationFilter();

			if (!PXAccess.FeatureInstalled<FeaturesSet.multiCompany>())
			{
				Organization anyOrganization = PXSelectReadonly<Organization>.SelectWindowed(this, 0, 1);

				BAccount.Cache.AllowInsert = anyOrganization == null;
			}

			BranchesView.Cache.AllowDelete = false;

			PXUIFieldAttribute.SetEnabled<Organization.organizationType>(OrganizationView.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.branch>());

			PXUIFieldAttribute.SetReadOnly(BranchesView.Cache, null, true);

			PXDimensionAttribute.SuppressAutoNumbering<Branch.branchCD>(BranchesView.Cache, true);
			PXDimensionAttribute.SuppressAutoNumbering<Organization.organizationCD>(OrganizationView.Cache, true);

			ActionsMenu.AddMenuAction(ChangeID);

			if (EntityInUseHelper.IsEntityInUse<CurrencyInUse>())
			{
				PXUIFieldAttribute.SetEnabled<CurrencyList.decimalPlaces>(CompanyCurrency.Cache, null, false);
			}

			Init();
		}

		protected virtual void Init()
		{
		}

		protected virtual BranchValidator GetBranchValidator()
		{
			return new BranchValidator(this);
		}

		protected BranchMaint BranchMaint;
		public virtual BranchMaint GetBranchMaint()
		{
			if (BranchMaint != null)
			{
				return BranchMaint;
			}
			else
			{
				BranchMaint = CreateInstance<BranchMaint>();

				return BranchMaint;
			}
		}

		#endregion

		#region Cache Attached Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Group ID")]
		[PXSelector(typeof(Search<Organization.organizationID,
			Where<Match<Organization, Current<AccessInfo.userName>>
				.And<Organization.organizationType.IsEqual<OrganizationTypes.group>>
				.And<Organization.baseCuryID.IsEqual<Organization.baseCuryID.FromCurrent>>>>),
			typeof(Organization.organizationCD),
			typeof(Organization.organizationName),
			SubstituteKey = typeof(Organization.organizationCD))]
		protected virtual void GroupOrganizationLink_GroupID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(Organization.organizationID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void GroupOrganizationLink_OrganizationID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Group Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		protected virtual void Organization_OrganizationName_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Base Currency ID")]
		[PXSelector(typeof(Search<CurrencyList.curyID>), DescriptionField = typeof(CurrencyList.description))]
		protected virtual void Company_BaseCuryID_CacheAttached(PXCache sender) { }

		#region Currency
		#region CuryID

		[PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CM.Currency.curyID>), CacheGlobal = true)]
		protected virtual void Currency_CuryID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RealGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RealGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RealGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RealGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RealLossAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RealLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RealLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RealLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RevalGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RevalGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalLossAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RevalLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RevalLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region TranslationGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_TranslationGainAcctID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region TranslationGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_TranslationGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region TranslationLossAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_TranslationLossAcctID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region TranslationLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_TranslationLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_UnrealizedGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_UnrealizedGainSubID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region UnrealizedLossAcctID

		[PXUIField(Required = false)]
		[PXDBInt()]
		protected virtual void Currency_UnrealizedLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_UnrealizedLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RoundingGainAcctID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region RoundingGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RoundingGainSubID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region RoundingLossAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RoundingLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RoundingLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region CuryInfoID
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = BatchModule.CM)]
		protected virtual void Currency_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region CuryInfoBaseID
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = BatchModule.CM)]
		protected virtual void Currency_CuryInfoBaseID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion
		#endregion

		#region Events Handlers

		#region OrganizationBAccount

		protected virtual void OrganizationBAccount_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Company rec = Company.Select();
			Company.Cache.SetStatus(rec, PXEntryStatus.Updated);

			var orgBAccount = e.Row as OrganizationBAccount;

			if (orgBAccount == null)
				return;

			OrganizationView.Insert(new Organization()
			{
				OrganizationCD = orgBAccount.AcctCD,
				OrganizationName = orgBAccount.AcctName,
				NoteID = orgBAccount.NoteID
			});
			OrganizationView.Cache.IsDirty = false;

			if (OrganizationView.Current != null)
			{
				(orgBAccount.Type, orgBAccount.IsBranch) = GetBAccountComplexType(OrganizationView.Current);
			}
		}

		protected virtual void OrganizationBAccount_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var orgBAccount = e.Row as OrganizationBAccount;

			if (orgBAccount == null)
				return;

			CanBeOrganizationDeleted(OrganizationView.Current);

			ResetVisibilityRestrictions(orgBAccount.BAccountID, orgBAccount.AcctCD, out bool cancelled);
			if (cancelled)
			{
				e.Cancel = true;
				return;
			}

			var org = PXAccess.GetOrganizationByBAccountID(orgBAccount.BAccountID);
			CheckIfCompanyInGroupsRelatedToCustomerVendor(org.OrganizationID, out cancelled);
			if (cancelled)
			{
				e.Cancel = true;
				return;
			}
		}

		protected virtual void OrganizationBAccount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Groups.Select().ForEach(link => Groups.Delete(link));

			DeleteOrganizationFixedAssetCalendar(OrganizationView.Current);
			OrganizationView.Delete(OrganizationView.Current);
		}

		#endregion

		#region Organization
		protected virtual void OrganizationBAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var orgBAccount = e.Row as OrganizationBAccount;

			if (orgBAccount == null)
			{
				// TODO: Redesign to persist before actions and eliminate this code
				createLedger.SetEnabled(false);
				AddBranch.SetEnabled(false);
				Activate.SetEnabled(false);
				Deactivate.SetEnabled(false);
				return;
			}

			if (orgBAccount.AcctCD?.Trim() != OrganizationView.Current?.OrganizationCD?.Trim())
			{
				OrganizationView.Current = OrganizationView.Select();
			}

			Ledger actualLedger = PXSelectJoin<Ledger,
				InnerJoin<OrganizationLedgerLink,
					On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>,
				InnerJoin<Organization,
					On<Organization.organizationID, Equal<OrganizationLedgerLink.organizationID>>>>,
				Where<Ledger.balanceType, Equal<LedgerBalanceType.actual>,
					And<Organization.organizationID, Equal<Current<Organization.organizationID>>>>>.Select(this);

			// TODO: Redesign to persist before actions and eliminate this code (except existing actual ledger check)
			bool isPersistedOrganization = !this.IsPrimaryObjectInserted();
			createLedger.SetEnabled(actualLedger == null && isPersistedOrganization);
			AddBranch.SetEnabled(isPersistedOrganization);

			State state = StateView.Current;

			Organization org = OrganizationView.Current;

			PXUIFieldAttribute.SetVisible<Organization.fileTaxesByBranches>(OrganizationView.Cache, null, org != null && org.OrganizationType == OrganizationTypes.WithBranchesBalancing);

			PXUIFieldAttribute.SetVisible<BranchAlias.branchCD>(BranchAliasView.Cache, null, org != null && org.OrganizationType != OrganizationTypes.WithoutBranches);

			if (org != null)
			{
				state.IsBranchTabVisible = org.OrganizationType != OrganizationTypes.WithoutBranches &&
											OrganizationView.Cache.GetValueOriginal<Organization.organizationType>(org) as string != OrganizationTypes.WithoutBranches;

				state.IsDeliverySettingsTabVisible = org.OrganizationType == OrganizationTypes.WithoutBranches;
				state.IsGLAccountsTabVisible = PXAccess.FeatureInstalled<FeaturesSet.subAccount>()
												&& org.OrganizationType == OrganizationTypes.WithoutBranches;
				state.IsRutRotTabVisible = PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>() && org.OrganizationType == OrganizationTypes.WithoutBranches;
				state.IsCompanyGroupsVisible = PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>() && org.OrganizationType != OrganizationTypes.Group;
				Deactivate.SetVisible(org.OrganizationType != OrganizationTypes.Group);
				Activate.SetVisible(org.OrganizationType != OrganizationTypes.Group);
				Deactivate.SetEnabled(org.Status == OrganizationStatus.Active && isPersistedOrganization);
				Activate.SetEnabled(org.Status == OrganizationStatus.Inactive && isPersistedOrganization);
			}
			else
			{
				state.IsBranchTabVisible = false;
				state.IsDeliverySettingsTabVisible = false;
				state.IsGLAccountsTabVisible = false;
				state.IsRutRotTabVisible = false;
				state.IsCompanyGroupsVisible = false;
			}
		}

		protected virtual void Organization_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			var existingOrgs = PXSelectReadonly<Organization>.SelectWindowed(this, 0, 2).Any();
			var status = cache.GetStatus(e.Row);
			var enable = (status == PXEntryStatus.Inserted) && (!existingOrgs || PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>());
			PXUIFieldAttribute.SetEnabled<Organization.baseCuryID>(cache, e.Row, enable);
		}

		protected virtual void Organization_OrganizationType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var organization = e.Row as Organization;
			var orgBAccount = PXCache<OrganizationBAccount>.CreateCopy(BAccount.Current);

			if (organization != null && orgBAccount != null)
			{
				string origOrganizationType = (string)sender.GetValueOriginal<Organization.organizationType>(organization);

				if (organization.OrganizationType != origOrganizationType)
				{
					(orgBAccount.Type, orgBAccount.IsBranch) = GetBAccountComplexType(organization);
					BAccount.Update(orgBAccount);
				}

				if (organization.OrganizationType != OrganizationTypes.WithBranchesBalancing)
				{
					organization.FileTaxesByBranches = false;
					organization.Reporting1099ByBranches = false;
				}
			}
		}

		protected virtual void Organization_OrganizationType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Organization organization = e.Row as Organization;

			if (organization == null)
				return;

			VerifyOrganizationType((string)e.NewValue,
									organization.OrganizationType,
									organization);
		}

		protected virtual void _(Events.FieldUpdated<Organization, Organization.reporting1099ByBranches> e)
		{
			if (e.Row.Reporting1099ByBranches == true)
			{
				e.Row.Reporting1099 = false;
			}
		}

		#endregion

		#region Groups
		protected virtual void _(Events.RowDeleting<GroupOrganizationLink> e)
		{
			if (e.Row == null) return;

			GroupOrganizationLink group = e.Row;
			CheckIfTheLastCompanyInGroup(group.GroupID, out bool cancelled);

			e.Cancel = cancelled;
		}
		#endregion

		protected virtual void Organization_Active_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Organization item = e.Row as Organization;

			if (item == null)
				return;

			GL.Branch[] branch = BranchMaint.GetChildBranches(this, OrganizationView.Current.OrganizationID).ToArray();

			if (branch.Any())
			{
				GetBranchValidator().ValidateActiveField(branch.Select(b => b.BranchID).ToArray(), (bool?)e.NewValue, item, skipActivateValidation: true);
			}
		}

		protected virtual void Address_CountryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var organization = this.Caches<Organization>().Current as Organization;
			
			if (e.NewValue.ToString() == canadianCountryCode && PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>())
			{
				sender.Graph.Caches<Organization>().RaiseExceptionHandling<Organization.organizationLocalizationCode>(organization, organization.OrganizationLocalizationCode,
					new PXSetPropertyException(Messages.CanadianLocalizationBoxWarning, PXErrorLevel.Warning));
			}
		}

		protected virtual void Organization_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				var rec =
				(PXResult<Organization, BAccount>)
				PXSelectJoin<Organization,
						InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Organization.bAccountID>>>,
						Where<Organization.organizationID, Equal<Current<Organization.organizationID>>>>
					.SelectSingleBound(this, new[] { e.Row });
				if (rec != null)
				{
					Organization org = rec;
					BAccount acct = rec;
					if (org.OrganizationCD != acct.AcctCD)
					{
						throw new PXException(PX.Objects.CS.Messages.CantAutoNumber);
					}
				}
			}
		}
		protected virtual void OrganizationBAccount_AcctName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			OrganizationBAccount organizationBAccount = e.Row as OrganizationBAccount;

			if (organizationBAccount == null)
				return;

			if (OrganizationView.Current != null && OrganizationView.Current.OrganizationCD == organizationBAccount.AcctCD)
			{
				OrganizationView.Current.OrganizationName = organizationBAccount.AcctName;
				OrganizationView.Cache.Update(OrganizationView.Current);
			}
		}

		public virtual void CommonSetup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation.Command() == PXDBOperation.Delete)
				return;

			PXDefaultAttribute.SetPersistingCheck<CommonSetup.weightUOM>(sender, e.Row,
				PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CommonSetup.volumeUOM>(sender, e.Row,
				PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
		}

		public virtual void CommonSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CommonSetup commonsetup = e.Row as CommonSetup;
			if (commonsetup == null) return;

			bool weightEnabled = true;
			bool volumeEnabled = true;
			if (!String.IsNullOrEmpty(commonsetup.WeightUOM))
			{
				InventoryItem itemWeight = PXSelect<InventoryItem, Where<InventoryItem.weightUOM, IsNotNull,
					And<InventoryItem.baseItemWeight, Greater<decimal0>>>>.SelectWindowed(this, 0, 1);
				weightEnabled = (itemWeight == null);
			}

			if (!String.IsNullOrEmpty(commonsetup.VolumeUOM))
			{
				InventoryItem itemVolume = PXSelect<InventoryItem, Where<InventoryItem.volumeUOM, IsNotNull,
					And<InventoryItem.baseItemVolume, Greater<decimal0>>>>.SelectWindowed(this, 0, 1);
				volumeEnabled = (itemVolume == null);
			}
			PXUIFieldAttribute.SetEnabled<CommonSetup.weightUOM>(sender, commonsetup, weightEnabled);
			PXUIFieldAttribute.SetEnabled<CommonSetup.volumeUOM>(sender, commonsetup, volumeEnabled);

			if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>() && commonsetup.DecPlQty == 0m)
			{
				sender.RaiseExceptionHandling<CommonSetup.decPlQty>(commonsetup, commonsetup.DecPlQty,
					new PXSetPropertyException(Messages.LowQtyDecimalPrecision, PXErrorLevel.Warning));
			}
			else
			{
				string warning = PXUIFieldAttribute.GetWarning<CommonSetup.decPlQty>(sender, commonsetup);
				if (!string.IsNullOrEmpty(warning) && warning == PXLocalizer.Localize(Messages.LowQtyDecimalPrecision))
				{
				sender.RaiseExceptionHandling<CommonSetup.decPlQty>(commonsetup, commonsetup.DecPlQty, null);
			}
			}

		}

		[PXDBDefault(typeof(Organization.organizationID))]
		[PXDBInt(IsKey = true, BqlTable = typeof(GL.FinPeriods.TableDefinition.FinYear))]
		[PXParent(typeof(Select<
			Organization,
			Where<Organization.organizationID, Equal<Current<OrganizationFinYear.organizationID>>>>))]
		protected virtual void OrganizationFinYear_OrganizationID_CacheAttached(PXCache sender) { }

		[PXDBDefault(typeof(Organization.organizationID))]
		[PXDBInt(IsKey = true, BqlTable = typeof(GL.FinPeriods.TableDefinition.FinPeriod))]
		protected virtual void OrganizationFinPeriod_OrganizationID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(Organization.organizationID))]
		protected virtual void _(Events.CacheAttached<FABookYear.organizationID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(Organization.organizationID))]
		protected virtual void _(Events.CacheAttached<FABookPeriod.organizationID> e) { }

		protected void CreateOrganizationCalendar(Organization organization, PXEntryStatus orgStatus)
		{
			if (orgStatus != PXEntryStatus.Inserted ||
				PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>() || organization.OrganizationType == OrganizationTypes.Group) return;

			#region Generate financial calendar

			PXCache<OrganizationFinYear> orgYearCache = this.Caches<OrganizationFinYear>();
			PXCache<OrganizationFinPeriod> orgPeriodCache = this.Caches<OrganizationFinPeriod>();

			// Preventing the re-insertion of the year and periods after the previous failed Persist()
			orgYearCache.Clear();
			orgPeriodCache.Clear();

			IEnumerable<MasterFinYear> masterYears = PXSelect<MasterFinYear>.Select(this).RowCast<MasterFinYear>();
			IEnumerable<MasterFinPeriod> masterPeriods = PXSelect<MasterFinPeriod>.Select(this).RowCast<MasterFinPeriod>();

			// TODO: Share code fragment with MasterFinPeriodMaint.SynchronizeBaseAndOrganizationPeriods()
			foreach (MasterFinYear masterYear in masterYears)
			{
				OrganizationFinYear insertedOrgYear = (OrganizationFinYear)orgYearCache.Insert(new OrganizationFinYear
				{
					OrganizationID = organization.OrganizationID,
					Year = masterYear.Year,
					FinPeriods = masterYear.FinPeriods,
					StartMasterFinPeriodID = FinPeriodUtils.GetFirstFinPeriodIDOfYear(masterYear),
					StartDate = masterYear.StartDate,
					EndDate = masterYear.EndDate
				});

				if (insertedOrgYear == null)
				{
					throw new PXException(Messages.FailedToGenerateFinYear, masterYear.Year, organization.OrganizationCD?.Trim());
				}
			}

			bool isCentralizedManagement = PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>();
			foreach (MasterFinPeriod masterPeriod in masterPeriods)
			{
				OrganizationFinPeriod insertedOrgPeriod = (OrganizationFinPeriod)orgPeriodCache.Insert(new OrganizationFinPeriod
				{
					OrganizationID = organization.OrganizationID,
					FinPeriodID = masterPeriod.FinPeriodID,
					MasterFinPeriodID = masterPeriod.FinPeriodID,
					FinYear = masterPeriod.FinYear,
					PeriodNbr = masterPeriod.PeriodNbr,
					Custom = masterPeriod.Custom,
					DateLocked = masterPeriod.DateLocked,
					StartDate = masterPeriod.StartDate,
					EndDate = masterPeriod.EndDate,

					Status = isCentralizedManagement ? masterPeriod.Status : FinPeriod.status.Inactive,
					ARClosed = isCentralizedManagement ? masterPeriod.ARClosed : false,
					APClosed = isCentralizedManagement ? masterPeriod.APClosed : false,
					FAClosed = isCentralizedManagement ? masterPeriod.FAClosed : false,
					CAClosed = isCentralizedManagement ? masterPeriod.CAClosed : false,
					INClosed = isCentralizedManagement ? masterPeriod.INClosed : false,

					Descr = masterPeriod.Descr,
				});

				PXDBLocalizableStringAttribute.CopyTranslations<MasterFinPeriod.descr, OrganizationFinPeriod.descr>(
					this.Caches<MasterFinPeriod>(),
					masterPeriod,
					orgPeriodCache,
					insertedOrgPeriod);

				if (insertedOrgPeriod == null)
				{
					throw new PXException(
						Messages.FailedToGenerateFinPeriod, 
						FinPeriodIDFormattingAttribute.FormatForDisplay(masterPeriod.FinPeriodID), 
						organization.OrganizationCD?.Trim());
				}
			}
			#endregion

			#region Generate fixed asset book calendar
			PXCache<FABookYear> faBookYearCache = this.Caches<FABookYear>();
			PXCache<FABookPeriod> faBookPeriodCache = this.Caches<FABookPeriod>();

			// Preventing the re-insertion of the year and periods after the previous failed Persist()
			faBookYearCache.Clear();
			faBookPeriodCache.Clear();

			foreach (FABook postingBook in SelectFrom<FABook>.Where<FABook.updateGL.IsEqual<True>>.View.Select(this))
			{
				IEnumerable<FABookYear> faBookMasterYears = SelectFrom<FABookYear>
					.Where<FABookYear.organizationID.IsEqual<FinPeriod.organizationID.masterValue>
						.And<FABookYear.bookID.IsEqual<@P.AsInt>>>
					.View
					.Select(this, postingBook.BookID)
					.RowCast<FABookYear>();

				IEnumerable<FABookPeriod> faBookMasterPeriods = SelectFrom<FABookPeriod>
					.Where<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID.masterValue>
						.And<FABookPeriod.bookID.IsEqual<@P.AsInt>>>
					.View
					.Select(this, postingBook.BookID)
					.RowCast<FABookPeriod>();

				foreach (FABookYear faBookMasterYear in faBookMasterYears)
				{
					FABookYear insertedYear = (FABookYear)faBookYearCache.Insert(new FABookYear
					{
						OrganizationID = organization.OrganizationID,
						BookID = postingBook.BookID,
						Year = faBookMasterYear.Year,
						FinPeriods = faBookMasterYear.FinPeriods,
						StartMasterFinPeriodID = FinPeriodUtils.GetFirstFinPeriodIDOfYear(faBookMasterYear),
						StartDate = faBookMasterYear.StartDate,
						EndDate = faBookMasterYear.EndDate
					});

					if (insertedYear == null)
					{
						throw new PXException(
							Messages.FailedToGenerateFABookYear, 
							faBookMasterYear.Year, 
							postingBook.BookCode?.Trim(), 
							organization.OrganizationCD?.Trim());
				}
			}

				foreach (FABookPeriod faBookMasterPeriod in faBookMasterPeriods)
				{
					FABookPeriod insertedPeriod = (FABookPeriod)faBookPeriodCache.Insert(new FABookPeriod
					{
						OrganizationID = organization.OrganizationID,
						BookID = postingBook.BookID,
						FinPeriodID = faBookMasterPeriod.FinPeriodID,
						MasterFinPeriodID = faBookMasterPeriod.FinPeriodID,
						FinYear = faBookMasterPeriod.FinYear,
						PeriodNbr = faBookMasterPeriod.PeriodNbr,
						Custom = faBookMasterPeriod.Custom,
						DateLocked = faBookMasterPeriod.DateLocked,
						StartDate = faBookMasterPeriod.StartDate,
						EndDate = faBookMasterPeriod.EndDate,

						Descr = faBookMasterPeriod.Descr,
					});

					if (insertedPeriod == null)
					{
						throw new PXException(
							Messages.FailedToGenerateFABookPeriod, 
							FinPeriodIDFormattingAttribute.FormatForDisplay(faBookMasterPeriod.FinPeriodID),
							postingBook.BookCode?.Trim(),
							organization.OrganizationCD?.Trim());
				}
			}
		}
			#endregion
		}

		protected virtual void DeleteOrganizationFixedAssetCalendar(Organization organization)
		{
			PXCache<FABookYear> faBookYearCache = this.Caches<FABookYear>();

			foreach (FABookYear fa in SelectFrom<FABookYear>
					.Where<FABookYear.organizationID.IsEqual<@P.AsInt>>
					.View
					.Select(this, organization.OrganizationID))
			{
				faBookYearCache.Delete(fa);
			}
		}

		public override void Persist()
		{
			Organization organization = OrganizationView.Select();
			PXEntryStatus orgBAccountStatus = BAccount.Cache.GetStatus(BAccount.Current);
			PXEntryStatus orgStatus = OrganizationView.Cache.GetStatus(OrganizationView.Current);

			Organization origOrganization = (Organization)OrganizationView.Cache.GetOriginal(organization);
			bool requestRelogin = Accessinfo.BranchID == null;

			if (organization != null)
			{
				if (!IsCompanyBaseCurrencySimilarToActualLedger(organization.BaseCuryID) || !IsCompanyBaseCurrencySimilarToGroup(organization.BaseCuryID)) return;
				CreateOrganizationCalendar(organization, orgStatus);

				if (organization.OrganizationType != OrganizationTypes.WithoutBranches 
					&& origOrganization?.RoleName != organization.RoleName)
				{
					if (organization.RoleName != null)
					{
						if (!IsImport)
						{
							BAccount.Ask(string.Empty,
								PXMessages.LocalizeFormatNoPrefix(GL.Messages.TheAccessRoleWillBeAssignedToAllBranchesOfTheCompany, organization.RoleName, BAccount.Current.AcctCD.Trim()),
								MessageButtons.OK);
						}
						
					}
					else
					{
						if (BAccount.Cache.GetStatus(BAccount.Current) != PXEntryStatus.Inserted)
						{
							if (!IsImport)
							{
								WebDialogResult dialogResult = BAccount.Ask(string.Empty,
									PXMessages.LocalizeFormatNoPrefix(GL.Messages.RemoveTheAccessRoleFromTheSettingsOfAllBranchesOfTheCompany,
										BAccount.Current.AcctCD.Trim()),
									MessageButtons.YesNo);

								StateView.Current.ClearAccessRoleOnChildBranches = dialogResult == WebDialogResult.Yes;
							}
							else
							{
								StateView.Current.ClearAccessRoleOnChildBranches = true;
							}
						}
					}
				}

				VerifyOrganizationType(organization.OrganizationType, origOrganization?.OrganizationType, organization);
				int? organizationID = PXAccess.GetParentOrganizationID(Accessinfo.BranchID);
				if (organizationID == organization.OrganizationID && organization.Active == false &&
				    origOrganization.Active == true)
					requestRelogin = true;
			}

			List<Organization> deletedOrganizations = OrganizationView.Cache.Deleted.Cast<Organization>().ToList();

			foreach (Organization deletedOrganization in deletedOrganizations)
			{
				CanBeOrganizationDeleted(deletedOrganization);
			}

			bool resetPageCache = false;

			if (!IsImport && !IsExport && !IsContractBasedAPI && !IsMobile)
			{
				resetPageCache = BAccount.Cache.Inserted.Any_() || BAccount.Cache.Deleted.Any_();
				if (!resetPageCache)
				{
					foreach (object updated in BAccount.Cache.Updated)
					{
						if (BAccount.Cache.IsValueUpdated<string, OrganizationBAccount.acctName>(updated, StringComparer.CurrentCulture))
						{
							resetPageCache = true;
							break;
						}
					}

					foreach (object updated in OrganizationView.Cache.Updated)
					{
						if (OrganizationView.Cache.IsValueUpdated<bool?, Organization.overrideThemeVariables>(updated)
							|| OrganizationView.Cache.IsValueUpdated<string, Organization.backgroundColor>(updated, StringComparer.OrdinalIgnoreCase)
						    || OrganizationView.Cache.IsValueUpdated<string, Organization.primaryColor>(updated, StringComparer.OrdinalIgnoreCase))
						{
							resetPageCache = true;
						}
						else if (OrganizationView.Cache.IsValueUpdated<string, Organization.roleName>(updated, StringComparer.OrdinalIgnoreCase)) {
							resetPageCache = true;
							requestRelogin = true;
							break;
						}
					}


				}
			}

			using (var tranScope = new PXTransactionScope())
			{
				base.Persist();

				try
				{
					ProcessOrganizationTypeChanging(orgBAccountStatus, origOrganization, organization);

					int? organizationID = PXAccess.GetParentOrganizationID(Accessinfo.BranchID);

					foreach (Organization deletedOrganization in deletedOrganizations)
					{
						ProcessOrganizationBAccountDeletion(deletedOrganization);

						requestRelogin = organizationID == deletedOrganization.OrganizationID;
					}

					SyncLedgerBaseCuryID();
				}
				finally
				{
					BranchesView.Cache.Clear();
				}

				ProcessPublicableToBranchesFieldsChanging(origOrganization, organization, orgStatus);

				tranScope.Complete();
			}

			if (deletedOrganizations.Any())
			{
				Company rec = Company.Select();
				PXResult<Organization> max = null;

				foreach (PXResult<Organization> org in PXSelectGroupBy<Organization,
					Where<Organization.active, Equal<True>>,
					Aggregate<GroupBy<Organization.baseCuryID, Count>>>.Select(this))
				{
					if (max == null || max.RowCount < org.RowCount) max = org;
				}

				rec.BaseCuryID = (max == null) ? null : ((Organization)max).BaseCuryID;

				Company.Cache.SetStatus(rec, PXEntryStatus.Updated);
				base.Persist();
			}
			//using (PXTransactionScope tran = new PXTransactionScope())
			//{
			//	this.Caches[typeof(Organization)].Clear();
			//	bool clearRoleNames = true;
			//	foreach (Organization org in PXSelect<Organization>.Select(this))
			//	{
			//		if (org.RoleName != null) clearRoleNames = false;
			//	}
			//	using (PXReadDeletedScope rds = new PXReadDeletedScope())
			//	{
			//		if (clearRoleNames)
			//		{
			//			PXDatabase.Update<Organization>(new PXDataFieldAssign<Organization.roleName>(null));
			//		}
			//	}
			//	tran.Complete();
			//}

			SelectTimeStamp();
			OrganizationView.Cache.Clear();
			OrganizationView.Cache.ClearQueryCacheObsolete();
			OrganizationView.Current = OrganizationView.Select();

			if (requestRelogin)
			{
				UserBranchSlotControl.Reset();
				PXLogin.SetBranchID(CreateInstance<PX.SM.SMAccessPersonalMaint>().GetDefaultBranchId());
			}

			if (resetPageCache) // to refresh branch combo
			{
				PXPageCacheUtils.InvalidateCachedPages();
				PXDatabase.SelectTimeStamp(); //clear db slots
				if(!this.UnattendedMode) throw new PXRedirectRequiredException(this, "Organization", true);
			}

			if (resetPageCache && requestRelogin)
			{
				// otherwise page will be refreshed on the next request by PXPage.NeedRefresh
				if (System.Web.HttpContext.Current == null)
				{
					throw new PXRedirectRequiredException(this, "Organization", true);
				}
			}
		}

		protected void SyncLedgerBaseCuryID()
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>()) return;

			GeneralLedgerMaint ledgerMaint = CreateInstance<GeneralLedgerMaint>();
			string organizationBaseCuryID = this.Company.Current.BaseCuryID;

			foreach (Ledger ledger in ledgerMaint.LedgerRecords.Select())
			{
				if (ledger.BalanceType == LedgerBalanceType.Actual && ledger.BaseCuryID != organizationBaseCuryID)
				{
					ledger.BaseCuryID = organizationBaseCuryID;
					ledgerMaint.LedgerRecords.Update(ledger);
				}
			}

			ledgerMaint.Actions.PressSave();
		}

		protected virtual void CanBeOrganizationDeleted(Organization organization)
		{
			CheckBranchesForDeletion(organization);
		}

		// TODO: Rework to RIC on Delete engine
		protected virtual void CheckBranchesForDeletion(Organization organization)
		{
			GL.Branch[] childBranches = PXSelectReadonly<GL.Branch,
														Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>>>
														.Select(this, organization.OrganizationID)
														.RowCast<GL.Branch>()
														.ToArray();

			string origOrgType = (string)OrganizationView.Cache.GetValueOriginal<Organization.organizationType>(organization);

			if (origOrgType == OrganizationTypes.WithoutBranches)
			{
				BranchValidator branchValidator = new BranchValidator(this);

				branchValidator.CanBeBranchesDeleted(childBranches, isOrganizationWithoutBranchesDeletion:true);
			}
			else
			{
				if (childBranches.Any())
				{
					throw new PXException(GL.Messages.CompanyCannotBeDeletedBecauseBranchOrBranchesExistForThisCompany,
						organization.OrganizationCD.Trim(),
						childBranches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage());
				}
			}
		}

		private void ProcessOrganizationBAccountDeletion(Organization organization)
		{
			string origOrgType = (string)OrganizationView.Cache.GetValueOriginal<Organization.organizationType>(organization);

			if (origOrgType == OrganizationTypes.WithoutBranches)
			{
				GL.Branch childBranch = BranchMaint.GetChildBranches(this, organization.OrganizationID).SingleOrDefault();

				if (childBranch != null)
				{
					if (childBranch.BAccountID != organization.BAccountID)
					{
						BranchMaint branchMaint = GetBranchMaint();
						branchMaint.Clear(PXClearOption.ClearAll);

						branchMaint.BAccount.Current =
							branchMaint.BAccount.Search<BranchMaint.BranchBAccount.bAccountID>(childBranch.BAccountID);

						branchMaint.BAccount.Delete(branchMaint.BAccount.Current);

						branchMaint.Actions.PressSave();
					}
					else
					{
						BranchesView.Delete(childBranch);
						BranchesView.Cache.Persist(PXDBOperation.Delete);
					}
				}
			}
		}

		private void ProcessOrganizationTypeChanging(PXEntryStatus orgBAccountStatus, Organization origOrganization, Organization organization)
		{
			if (organization == null)
				return;

			if (orgBAccountStatus == PXEntryStatus.Inserted ||
			    orgBAccountStatus == PXEntryStatus.Updated && BranchMaint.GetChildBranch(this, organization.OrganizationID) == null)
			{
				if (organization.OrganizationType == OrganizationTypes.WithoutBranches)
				{
					CreateSingleBranchRecord(organization);
				}
			}
			else if (orgBAccountStatus == PXEntryStatus.Updated)
			{
				if (origOrganization?.OrganizationType == OrganizationTypes.WithoutBranches
				    && organization.OrganizationType != OrganizationTypes.WithoutBranches)
				{
					MakeBranchSeparate(BAccount.Current);
				}
				else if (origOrganization?.OrganizationType != OrganizationTypes.WithoutBranches
				         && organization.OrganizationType == OrganizationTypes.WithoutBranches)
				{
					MergeToSingleBAccount(organization);
				}
			}
		}

		protected virtual void CreateSingleBranchRecord(Organization organization)
		{
			var branch = new GL.Branch()
			{
				OrganizationID = organization.OrganizationID,
				BAccountID = organization.BAccountID,
				BranchCD = organization.OrganizationCD,
				BaseCuryID = organization.BaseCuryID
			};

			branch = BranchesView.Cache.Update(branch) as GL.Branch;			

			BranchesView.Cache.Persist(PXDBOperation.Insert);
		}

		protected virtual void MakeBranchSeparate(OrganizationBAccount orgBAccount)
		{
			string newAcctCD = GetNewBranchCD();

			BAccount newBAccount = CreateSeparateBAccountForBranch(newAcctCD, orgBAccount.AcctName);

			MapBranchToNewBAccountAndChangeBranchCD(orgBAccount.AcctCD, newBAccount.AcctCD, newBAccount.BAccountID);

			AssignNewBranchToEmployees(orgBAccount.BAccountID, newBAccount.BAccountID, newBAccount.AcctCD);
		}

		protected virtual void MergeToSingleBAccount(Organization organization)
		{
			GL.Branch branch = BranchMaint.GetSeparateChildBranches(this, organization).Single();

			MapBranchToNewBAccountAndChangeBranchCD(branch.BranchCD, organization.OrganizationCD, organization.BAccountID);

			AssignNewBranchToEmployees(branch.BAccountID, organization.BAccountID, organization.OrganizationCD);

			DeleteBranchBAccount(branch.BAccountID);
		}

		protected virtual void AssignNewBranchToEmployees(int? oldBAccountID, int? newBAccountID, string branchCD)
		{
			IEnumerable<EPEmployee> employees = PXSelectReadonly<EPEmployee, 
														Where<EPEmployee.parentBAccountID, Equal<Required<EPEmployee.parentBAccountID>>>>
														.Select(this, oldBAccountID)
														.RowCast<EPEmployee>();

			var employeeMaint = CreateInstance<EmployeeMaint>();

			GL.Branch branch = PXSelectReadonly<GL.Branch, 
												Where<GL.Branch.branchCD, Equal<Required<GL.Branch.branchCD>>>>
										.Select(this, branchCD);

			foreach (var employee in employees)
			{
				employeeMaint.Clear();
				employeeMaint.Employee.Current = employee;
				Address defAddress = employeeMaint.Address.Select();
				Contact defContact = employeeMaint.Contact.Select();

				var employeeCopy = PXCache<EPEmployee>.CreateCopy(employee);
				defAddress = PXCache<Address>.CreateCopy(defAddress);
				defContact = PXCache<Contact>.CreateCopy(defContact);				

				employeeCopy.ParentBAccountID = newBAccountID;
				employeeMaint.Employee.Cache.SetStatus(employeeCopy, PXEntryStatus.Updated);											
				defAddress.BAccountID = newBAccountID;								
				defContact.BAccountID = newBAccountID;
				employeeMaint.Address.Update(defAddress);
				employeeMaint.Contact.Update(defContact);
				employeeMaint.Actions.PressSave();
			}
		}

		protected virtual BAccount CreateSeparateBAccountForBranch(string acctCD, string acctName)
		{
			var baccountMaint = CreateInstance<SeparateBAccountMaint>();

			var baccount = new BAccount()
			{
				AcctCD = acctCD,
				AcctName = acctName,
				Type = BAccountType.BranchType
			};

			baccountMaint.BAccount.Insert(baccount);

			var defContactAddress = baccountMaint.GetExtension<SeparateBAccountMaint.DefContactAddressExt>();
			CopyGeneralInfoToBranch(baccountMaint, defContactAddress.DefContact.Current, defContactAddress.DefAddress.Current);

			var defLocationExt = baccountMaint.GetExtension<SeparateBAccountMaint.DefLocationExt>();
			CopyLocationDataToBranch(baccountMaint, defLocationExt.DefLocation.Current, defLocationExt.DefLocationContact.Current);

			baccountMaint.Actions.PressSave();

			return baccountMaint.BAccount.Current;
		}

		protected virtual void DeleteBranchBAccount(int? baccountID)
		{
			var baccountMaint = CreateInstance<SeparateBAccountMaint>();

			baccountMaint.BAccount.Current = baccountMaint.BAccount.Search<CR.BAccount.bAccountID>(baccountID);

			baccountMaint.BAccount.Delete(baccountMaint.BAccount.Current);

			baccountMaint.Actions.PressSave();
		}

		protected virtual void MapBranchToNewBAccountAndChangeBranchCD(string oldBranchCD, string newBranchCD, int? newBAccountID)
		{
			SeparateBranchMaint branchMaint = CreateInstance<SeparateBranchMaint>();

			GL.Branch branch = PXSelect<GL.Branch,
										Where<GL.Branch.branchCD, Equal<Required<GL.Branch.branchCD>>>>
										.Select(branchMaint, oldBranchCD);

			branch.BAccountID = newBAccountID;
			branchMaint.BranchView.Update(branch);

			PXChangeID<GL.Branch, GL.Branch.branchCD>.ChangeCD(branchMaint.BranchView.Cache, branch.BranchCD, newBranchCD);

			branchMaint.Actions.PressSave();
		}

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			int res = base.Persist(cacheType, operation);
			if (cacheType == typeof(OrganizationBAccount) && operation == PXDBOperation.Update)
			{
				foreach (PXResult<NoteDoc, UploadFile> rec in PXSelectJoin<NoteDoc, InnerJoin<UploadFile, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
					Where<NoteDoc.noteID, Equal<Current<OrganizationBAccount.noteID>>>>.Select(this))
				{
					UploadFile file = (UploadFile)rec;
					if (file.IsPublic != true)
					{
						this.SelectTimeStamp();
						file.IsPublic = true;
						file = (UploadFile)this.Caches[typeof(UploadFile)].Update(file);
						this.Caches[typeof(UploadFile)].PersistUpdated(file);
					}
				}
			}
			return res;
		}		

		protected virtual void OrganizationBAccount_OrganizationAcctCD_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			//PXDBChildIdentity() hack
			if (e.Table != null && e.Operation == PXDBOperation.Update)
			{
				e.IsRestriction = false;
				e.Cancel = true;
			}
		}

		protected virtual IEnumerable commonsetup()
		{
			PXCache cache = Commonsetup.Cache;
			PXResultset<CommonSetup> ret = PXSelect<CommonSetup>.SelectSingleBound(this, null);

			if (ret.Count == 0)
			{
				CommonSetup setup = (CommonSetup)cache.Insert(new CommonSetup());
				cache.IsDirty = false;
				ret.Add(new PXResult<CommonSetup>(setup));
			}
			else if (cache.Current == null)
			{
				cache.SetStatus((CommonSetup)ret, PXEntryStatus.Notchanged);
			}

			return ret;
		}
		#endregion

		#region Events Company
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(IsNull<Current<AccessInfo.baseCuryID>, Current<Company.baseCuryID>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
		protected virtual void Organization_BaseCuryID_CacheAttached(PXCache sender)
		{
			}

		protected virtual void Organization_BaseCuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			string newBaseCury = (string)e.NewValue;
			if (!string.IsNullOrEmpty(newBaseCury))
			{
				if (!IsCompanyBaseCurrencySimilarToGroup(newBaseCury))
				{
					e.Cancel = true;
					return;
				}

				if (!IsCompanyBaseCurrencySimilarToActualLedger(newBaseCury))
				{
					e.Cancel = true;
					return;
				}

				if (!IsCompanyBaseCurrencySimilarToActualLedger(newBaseCury))
				{
					e.Cancel = true;
					return;
				}

				Currency currency = PXSelect<
					Currency,
					Where<Currency.curyID, Equal<Required<CurrencyList.curyID>>>>
					.SelectSingleBound(this, null, e.NewValue);

				if (currency?.CuryInfoID == null)
				{
					CurrencyList bc = (CurrencyList)PXSelectorAttribute.Select<Organization.baseCuryID>(sender, e.Row, e.NewValue);

					if (bc != null)
					{
						// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Justification]
						bc.IsActive = true;
						// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Justification]
						bc.IsFinancial = true;
						// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
						bc = CompanyCurrency.Update(bc);
						if (currency == null)
						{
							FieldDefaulting.AddHandler<CurrencyInfo.curyID>((cache, args) => { args.NewValue = bc.CuryID; });
							// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
							Currency finRow = (Currency)FinancinalCurrency.Insert(new Currency { CuryID = bc.CuryID });

							CurrencyInfo info = (CurrencyInfo)currencyinfo.View.SelectSingleBound(new object[] { finRow });
							info.BaseCalc = true;
							info.IsReadOnly = true;
							info.BaseCuryID = finRow.CuryID;

							info = (CurrencyInfo)currencyinfo.View.SelectSingle(finRow.CuryInfoBaseID);
							info.BaseCalc = false;
							info.IsReadOnly = true;
							info.BaseCuryID = finRow.CuryID;
						}
						else
						{
							// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
							currency.CuryInfoID = currencyinfo.Insert(new CurrencyInfo
							{
								CuryID = bc.CuryID,
								BaseCuryID = bc.CuryID,
								BaseCalc = true,
								IsReadOnly = true,
							}).CuryInfoID;

							// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
							currency.CuryInfoBaseID = currencyinfo.Insert(new CurrencyInfo
							{
								CuryID = bc.CuryID,
								BaseCuryID = bc.CuryID,
								BaseCalc = false,
								IsReadOnly = true,
							}).CuryInfoID;
							// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
							FinancinalCurrency.Update(currency);
						}
						e.Cancel = true;

					}
				}
			}
		}

		protected virtual void Organization_BaseCuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CompanyCurrency.View.RequestRefresh();

			if (e.Row == null) return;
			var org = (Organization)e.Row;

			var orgs = PXSelect<Organization>.SelectMultiBound(this, null);
			if (orgs.Count == 1)
			{
				var companyCache = this.Caches[typeof(Company)];
				var comp = (Company)companyCache.Current;
				comp.BaseCuryID = org.BaseCuryID;
				companyCache.Update(comp);
			}
		}


		#endregion

		#region Action Handlers

		[PXButton]
		[PXUIField(DisplayName = "Add Ledger", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		public virtual IEnumerable addLedger(PXAdapter adapter)
		{
			GeneralLedgerMaint.RedirectTo(null);

			return adapter.Get();
		}

		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "Add Branch", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		public virtual IEnumerable addBranch(PXAdapter adapter)
		{
			BranchMaint graph = CreateInstance<BranchMaint>();
			graph.BAccount.Insert(new BranchMaint.BranchBAccount { OrganizationID = OrganizationView.Current.OrganizationID });
			graph.BAccount.Cache.IsDirty = false;
			graph.Caches<RedirectBranchParameters>().Insert(new RedirectBranchParameters { OrganizationID = OrganizationView.Current.OrganizationID });
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable viewBranch(PXAdapter adapter)
		{
			var branch = BranchesView.Current;

			if (branch != null)
			{
				BranchMaint.RedirectTo(branch.BAccountID);
			}

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable viewGroup(PXAdapter adapter)
		{
			GroupOrganizationLink link = Groups.Current;

			if (link != null)
			{
				CompanyGroupsMaint.RedirectTo(link.GroupID);
			}

			return adapter.Get();
		}

		[PXUIField(DisplayName = "setAsPrimary")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry, OnClosingPopup = PXSpecialButtonType.Refresh)]
		public IEnumerable setAsPrimary(PXAdapter adapter)
		{
			GroupOrganizationLink newLink = Groups.Current;			

			if (newLink != null
				&& newLink.PrimaryGroup != true)
			{
				GroupOrganizationLink oldLink = Groups.Select<GroupOrganizationLink>().Where(l => l.PrimaryGroup == true).FirstOrDefault();

				if (oldLink != null)
				{
					oldLink.PrimaryGroup = false;
					Groups.Cache.Update(oldLink);
				}

				newLink.PrimaryGroup = true;
				Groups.Cache.Update(newLink);
				Groups.View.RequestRefresh();
			}

			return adapter.Get();
		}

		#endregion

		#region Service

		protected virtual void VerifyOrganizationType(string newOrgType, string oldOrgType, Organization organization)
		{
			if (OrganizationView.Cache.GetStatus(organization) == PXEntryStatus.Inserted)
				return;

			string errorMessage = null;

			if (oldOrgType != OrganizationTypes.WithoutBranches
				&& newOrgType == OrganizationTypes.WithoutBranches)
			{
				if (BranchMaint.MoreThenOneBranchExist(this, organization.OrganizationID))
				{
					errorMessage = GL.Messages.TheCompanyTypeCannotBeChangedToBecauseMoreThanOneBranchExistsForTheCompany;
				}
			}
			else if (oldOrgType == OrganizationTypes.WithBranchesNotBalancing && newOrgType == OrganizationTypes.WithBranchesBalancing)
			{
				if (BranchMaint.MoreThenOneBranchExist(this, organization.OrganizationID))
				{
					if (GLUtility.RelatedForOrganizationGLHistoryExists(this, organization.OrganizationID))
					{
						errorMessage = GL.Messages.TheCompanyTypeCannotBeChangedToBecauseDataHasBeenPostedForTheCompany;
					}
				}
			}
			else
			{
				OrganizationBAccount baccount = SelectFrom<OrganizationBAccount>
					.Where<OrganizationBAccount.bAccountID.IsEqual<@P.AsInt>>
					.View
					.Select(this, organization.BAccountID);

				if(oldOrgType == OrganizationTypes.WithoutBranches
					&& oldOrgType != newOrgType
					&& baccount != null
					&& (baccount.Type == BAccountType.VendorType
						|| baccount.Type == BAccountType.CustomerType
						|| baccount.Type == BAccountType.CombinedType))
				{
					errorMessage = GL.Messages.TheCompanyTypeCannotBeChangedToBecauseTheCompanyIsExtendedToCustomerOrVendor;
				}
			}

			if (errorMessage != null)
			{
				string localizedOrgType =
					PXStringListAttribute.GetLocalizedLabel<Organization.organizationType>(OrganizationView.Cache,
																									organization,
																									newOrgType);

				throw new PXSetPropertyException(errorMessage, localizedOrgType);
			}
		}

		public virtual bool ShouldInvokeOrganizationBranchSync(PXEntryStatus status)
		{
			return false;
		}

		public virtual void OnOrganizationBranchSync(BranchMaint branchMaint, Organization organization, BranchMaint.BranchBAccount branchBaccountCopy)
		{
		}

		protected virtual void ProcessPublicableToBranchesFieldsChanging(Organization origOrganization, Organization organization, PXEntryStatus status)
		{
			if (organization == null)
				return;
			bool organizationWithoutBranches = (origOrganization?.OrganizationType == OrganizationTypes.WithoutBranches);
			bool forceCopy = organization?.OrganizationType == OrganizationTypes.WithoutBranches &&
			                 (origOrganization?.OrganizationType != OrganizationTypes.WithoutBranches || status == PXEntryStatus.Inserted);

			bool shouldAdjustRoleName = origOrganization?.RoleName != organization.RoleName 
											&& (organization.RoleName == null && StateView.Current.ClearAccessRoleOnChildBranches == true
													|| organization.RoleName != null
													|| organization.OrganizationType == OrganizationTypes.WithoutBranches);

			bool shouldAdjustActive = origOrganization?.Active != organization.Active &&
			                          (organization.Active != true || organization.OrganizationType == OrganizationTypes.WithoutBranches);

			bool shouldAdjustLedgerID = origOrganization?.ActualLedgerID != organization.ActualLedgerID;

			bool shouldAdjustBaseCuryID = origOrganization?.BaseCuryID != organization.BaseCuryID;

			bool shouldAdjustCountryID = organization.OrganizationType == OrganizationTypes.WithoutBranches && origOrganization?.CountryID != organization.CountryID;

			bool shouldInvokeSyncEx = ShouldInvokeOrganizationBranchSync(status);

			bool shouldLogoNameReport = origOrganization?.LogoNameReport != organization.LogoNameReport;
			bool shouldLogoName = origOrganization?.LogoName != organization.LogoName;

			if (forceCopy || shouldAdjustRoleName || shouldAdjustActive || shouldAdjustLedgerID || shouldAdjustCountryID || shouldLogoNameReport || shouldInvokeSyncEx || shouldAdjustBaseCuryID)
			{
				if (shouldAdjustRoleName || forceCopy)
					{
						PXUpdate<
								Set<Branch.roleName, Required<Branch.roleName>>, Branch,
								Where<Branch.organizationID, Equal<Required<Branch.organizationID>>>>
							.Update(this, organization.RoleName, organization.OrganizationID);
					}

					if (shouldAdjustActive || forceCopy)
					{
						PXUpdate<
								Set<Branch.active, Required<Branch.active>>, Branch,
								Where<Branch.organizationID, Equal<Required<Branch.organizationID>>>>
							.Update(this, organization.Active, organization.OrganizationID);
						
					}

					if (shouldAdjustLedgerID || forceCopy)
					{
						PXUpdate<
								Set<Branch.ledgerID, Required<Branch.ledgerID>>, Branch,
								Where<Branch.organizationID, Equal<Required<Branch.organizationID>>>>
							.Update(this, organization.ActualLedgerID, organization.OrganizationID);
					}

					if (shouldAdjustBaseCuryID || forceCopy)
					{
						PXUpdate<
								Set<Branch.baseCuryID, Required<Branch.baseCuryID>>, Branch,
								Where<Branch.organizationID, Equal<Required<Branch.organizationID>>>>
							.Update(this, organization.BaseCuryID, organization.OrganizationID);
					}

					if (shouldAdjustCountryID || forceCopy)
					{
						PXUpdate<
								Set<Branch.countryID, Required<Branch.countryID>>, Branch,
								Where<Branch.organizationID, Equal<Required<Branch.organizationID>>>>
							.Update(this, organization.CountryID, organization.OrganizationID);
					}
					
					if (forceCopy || shouldLogoNameReport)
					{
						PXUpdate<
								Set<Branch.organizationLogoNameReport, Required<Branch.organizationLogoNameReport>>, Branch,
								Where<Branch.organizationID, Equal<Required<Branch.organizationID>>>>
							.Update(this, organization.LogoNameReport, organization.OrganizationID);
					}
					if ((organizationWithoutBranches && shouldLogoNameReport) || forceCopy)
					{
						PXUpdate<
                  		Set<Branch.logoNameReport, Required<Branch.logoNameReport>>, Branch,
                  		Where<Branch.organizationID, Equal<Required<Branch.organizationID>>>>
                  	.Update(this, organization.LogoNameReport, organization.OrganizationID);
					}
					if ((organizationWithoutBranches && shouldLogoName) || forceCopy)
					{
						PXUpdate<
								Set<Branch.logoName, Required<Branch.logoName>>, Branch,
								Where<Branch.organizationID, Equal<Required<Branch.organizationID>>>>
							.Update(this, organization.LogoName, organization.OrganizationID);
					}
				
			}
			BAccount.Cache.Clear();
			BAccount.Cache.ClearQueryCacheObsolete();
			BAccount.Current = BAccount.Search<OrganizationBAccount.acctCD>(organization.OrganizationCD);
			base.ClearRoleNameInBranches();
			base.RefreshBranch();
			PXAccess.ResetOrganizationBranchSlot();

			StateView.Current.ClearAccessRoleOnChildBranches = false;
		}

		#region Copying from Org to Branch

		protected virtual void CopyGeneralInfoToBranch(PXGraph graph, Contact destContact, Address destAddress)
		{
			var defContactAddress = this.GetExtension<DefContactAddressExt>();

			CopyContactData(graph, defContactAddress.DefContact.SelectSingle(), destContact);

			int? oldAddressID = destAddress.AddressID;
			int? oldAddressdBAccountID = destAddress.BAccountID;
			Guid? oldNoteID = destAddress.NoteID;
			var timeStamp = destAddress.tstamp;

			PXCache<Address>.RestoreCopy(destAddress, PXCache<Address>.CreateCopy(defContactAddress.DefAddress.Select()));

			destAddress.AddressID = oldAddressID;
			destAddress.BAccountID = oldAddressdBAccountID;
			destAddress.NoteID = oldNoteID;
			destAddress.tstamp = timeStamp;

			graph.Caches<Address>().Update(destAddress);
		}

		protected virtual void CopyLocationDataToBranch(PXGraph graph, CRLocation destLocation, Contact destLocationContact)
		{
			var defLocationExt = this.GetExtension<DefLocationExt>();

			CopyContactData(graph, defLocationExt.DefLocationContact.SelectSingle(), destLocationContact);

			int? oldLocationID = destLocation.LocationID;
			string oldLocationCD = destLocation.LocationCD;
			Guid? oldNoteID = destLocation.NoteID;
			int? oldBAccountID = destLocation.BAccountID;
			int? oldDefAddressID = destLocation.DefAddressID;
			int? oldDefContactID = destLocation.DefContactID;
			int? oldVAPAccountLocationID = destLocation.VAPAccountLocationID;
			int? oldCARAccountLocationID = destLocation.CARAccountLocationID;
			int? oldVPaymentInfoLocationID = destLocation.VPaymentInfoLocationID;
			var timeStamp = destLocation.tstamp;

			PXCache<CRLocation>.RestoreCopy(destLocation, PXCache<CRLocation>.CreateCopy(defLocationExt.DefLocation.Current ?? defLocationExt.DefLocation.SelectSingle()));

			destLocation.LocationID = oldLocationID;
			destLocation.LocationCD = oldLocationCD;
			destLocation.NoteID = oldNoteID;
			destLocation.BAccountID = oldBAccountID;
			destLocation.DefAddressID = oldDefAddressID;
			destLocation.DefContactID = oldDefContactID;
			destLocation.VAPAccountLocationID = oldVAPAccountLocationID;
			destLocation.CARAccountLocationID= oldCARAccountLocationID;
			destLocation.VPaymentInfoLocationID = oldVPaymentInfoLocationID;
			destLocation.tstamp = timeStamp;

			graph.Caches<CRLocation>().Update(destLocation);
		}

		protected virtual void CopyContactData(PXGraph graph, Contact contactSrc, Contact contactDest)
		{
			int? oldContactID = contactDest.ContactID;
			int? oldContactBAccountID = contactDest.BAccountID;
			int? oldDefAddress = contactDest.DefAddressID;
			Guid? oldNoteID = contactDest.NoteID;
			var timeStamp = contactDest.tstamp;

			PXCache<Contact>.RestoreCopy(contactDest, PXCache<Contact>.CreateCopy(contactSrc));

			contactDest.ContactID = oldContactID;
			contactDest.BAccountID = oldContactBAccountID;
			contactDest.DefAddressID = oldDefAddress;
			contactDest.NoteID = oldNoteID;
			contactDest.tstamp = timeStamp;

			graph.Caches<Contact>().Update(contactDest);
		}

		protected virtual void CopyRutRot(Organization organization, BranchMaint.BranchBAccount branchBAccount)
		{
			
		}

		#endregion

		protected virtual string GetBAccountType(Organization organization)
		{
			return organization.OrganizationType == OrganizationTypes.WithoutBranches
					? BAccountType.BranchType
					: BAccountType.OrganizationType;
		}

		protected virtual (string, bool) GetBAccountComplexType(Organization organization)
		{
			string baccountType = GetBAccountType(organization);
			return (baccountType, baccountType == BAccountType.BranchType);
		}

		protected virtual string GetNewBranchCD()
		{
			string currentCD = PXMessages.LocalizeFormatNoPrefix(GL.Messages.NewBranchNameTemplate, string.Empty);
			int currentNumber = 0;
			do
			{
				BAccountR bAccount;
				using (new PXReadDeletedScope()) // to prevent duplicate with deleted business account (DeletedDatabaseRecord = 1)
				{
					bAccount = PXSelectReadonly<BAccountR,
						Where<BAccountR.acctCD, Equal<Required<BAccountR.acctCD>>>>
						.Select(this, currentCD);
				}

				if (bAccount == null)
				{
					return currentCD;
				}
				else
				{
					currentNumber++;

					if (currentNumber == int.MaxValue)
						throw new PXException(GL.Messages.TheDefaultBranchNameCannotBeAssigned);

					currentCD = PXMessages.LocalizeFormatNoPrefix(GL.Messages.NewBranchNameTemplate, currentNumber);
				}

			} while (true);
		}

		protected virtual bool IsCompanyBaseCurrencySimilarToActualLedger(string baseCuryToCompare)
		{
			Organization organization = OrganizationView.Select();
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>() && organization.BaseCuryID != null)
			{
				foreach (OrganizationLedgerLink link in SelectFrom<OrganizationLedgerLink>
					.Where<OrganizationLedgerLink.organizationID.IsEqual<@P.AsInt>>
					.View.SelectSingleBound(this, null, new object[] { organization.OrganizationID }))
				{
					Ledger curLedger = GeneralLedgerMaint.FindLedgerByID(this, link.LedgerID);
					if (curLedger.BalanceType == LedgerBalanceType.Actual && curLedger.BaseCuryID != baseCuryToCompare)
					{
						OrganizationView.Cache.RaiseExceptionHandling<Organization.baseCuryID>(
							organization,
							baseCuryToCompare,
							new PXSetPropertyException(
								Messages.LedgerBaseCurrencyDifferFromCompany,
								PXErrorLevel.Error,
								organization.OrganizationCD,
								curLedger.LedgerCD));
						return false;
					}
				}
			}
			return true;
		}

		protected virtual void ResetVisibilityRestrictions(int? bAccountID, string bAccountCD, out bool cancelled)
		{
			AR.Customer relatedCustomer = SelectFrom<AR.Customer>
				.Where<AR.Customer.cOrgBAccountID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectSingleBound(this, null, new object[] { bAccountID });

			AP.Vendor relatedVendor = SelectFrom<AP.Vendor>
				.Where<AP.Vendor.vOrgBAccountID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectSingleBound(this, null, new object[] { bAccountID });

			if ((relatedCustomer != null) || (relatedVendor != null))
			{
				if (OrganizationView.Ask(PXMessages.LocalizeFormatNoPrefixNLA(Messages.CustomerVendorLink, bAccountCD), MessageButtons.OKCancel) == WebDialogResult.OK)
				{
					if (relatedCustomer != null)
					{
						PXUpdate<
								Set<AR.Customer.cOrgBAccountID, Zero>,
								AR.Customer,
								Where<AR.Customer.cOrgBAccountID.IsEqual<@P.AsInt>>>
							.Update(this, bAccountID);
					}

					if (relatedVendor != null)
					{
						PXUpdate<
								Set<AP.Vendor.vOrgBAccountID, Zero>,
								AP.Vendor,
								Where<AP.Vendor.vOrgBAccountID.IsEqual<@P.AsInt>>>
							.Update(this, bAccountID);
					}
				}
				else
				{
					cancelled = true;
				}
			}

			cancelled = false;
		}

		protected virtual void CheckIfTheLastCompanyInGroup(int? groupID, out bool cancelled)
		{
			cancelled = false;
			if (groupID == null) return;

			var group = PXAccess.GetOrganizationByID(groupID);
			var currentOrg = PXAccess.GetOrganizationByBAccountID(BAccount.Current.BAccountID);

			if (group == null || currentOrg == null) return;

			GroupOrganizationLink orgsInGroup = SelectFrom<GroupOrganizationLink>
				.Where<GroupOrganizationLink.groupID.IsEqual<@P.AsInt>
				.And<GroupOrganizationLink.organizationID.IsNotEqual<@P.AsInt>>>
				.View.SelectSingleBound(this, null, new object[] { group.OrganizationID, currentOrg.OrganizationID });

			if (orgsInGroup != null) return;

			AR.Customer relatedCustomer = SelectFrom<AR.Customer>
				.Where<AR.Customer.cOrgBAccountID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectSingleBound(this, null, new object[] { group.BAccountID });

			AP.Vendor relatedVendor = SelectFrom<AP.Vendor>
				.Where<AP.Vendor.vOrgBAccountID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectSingleBound(this, null, new object[] { group.BAccountID });

			if ((relatedCustomer != null) || (relatedVendor != null))
			{
				if (OrganizationView.Ask(
					PXMessages.LocalizeFormatNoPrefixNLA(Messages.CustomerVendorWillBeInvisible, group.OrganizationCD),
					MessageButtons.OKCancel) == WebDialogResult.Cancel)
				{
					cancelled = true;
				}
			}
		}

		protected virtual void CheckIfCompanyInGroupsRelatedToCustomerVendor(int? organizationID, out bool cancelled)
		{
			cancelled = false;
			if (organizationID == null) return;

			AR.Customer relatedCustomer = SelectFrom<AR.Customer>
				.InnerJoin<Organization>
					.On<Organization.bAccountID.IsEqual<AR.Customer.cOrgBAccountID>>
				.InnerJoin<GroupOrganizationLink>
					.On<GroupOrganizationLink.groupID.IsEqual<Organization.organizationID>>
				.Where<GroupOrganizationLink.organizationID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectSingleBound(this, null, new object[] { organizationID });

			AP.Vendor relatedVendor = (relatedCustomer != null) ? null :
				SelectFrom<AP.Vendor>
					.InnerJoin<Organization>
						.On<Organization.bAccountID.IsEqual<AP.Vendor.vOrgBAccountID>>
					.InnerJoin<GroupOrganizationLink>
						.On<GroupOrganizationLink.groupID.IsEqual<Organization.organizationID>>
					.Where<GroupOrganizationLink.organizationID.IsEqual<@P.AsInt>>
					.View.ReadOnly.SelectSingleBound(this, null, new object[] { organizationID });

			if ((relatedCustomer != null) || (relatedVendor != null))
			{
				var organization = PXAccess.GetOrganizationByID(organizationID);

				if (OrganizationView.Ask(
					PXMessages.LocalizeFormatNoPrefixNLA(Messages.CustomerVendorWillBeInvisibleWhenOrganizationDeleted, organization.OrganizationCD),
					MessageButtons.OKCancel) == WebDialogResult.Cancel)
				{
					cancelled = true;
				}
			}
		}
		#endregion

		protected override int? BaccountIDForNewEmployee()
		{
			if (OrganizationView.Current == null)
				return null;

			string origOrgType = (string)OrganizationView.Cache.GetValueOriginal<Organization.organizationType>(OrganizationView.Current);

			return origOrgType == OrganizationTypes.WithoutBranches
			       && OrganizationView.Current.OrganizationType == origOrgType
				? BAccount.Current.BAccountID
				: null;
		}

		protected virtual bool IsCompanyBaseCurrencySimilarToGroup(string baseCuryToCompare)
		{
			Organization organization = OrganizationView.Select();
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>() && organization.BaseCuryID != null)
			{
				foreach (GroupOrganizationLink link in Groups.Select())
				{
					PXAccess.MasterCollection.Organization group = PXAccess.GetOrganizationByID(link.GroupID);
					if (group.BaseCuryID != baseCuryToCompare)
					{
						OrganizationView.Cache.RaiseExceptionHandling<Organization.baseCuryID>(
							organization,
							baseCuryToCompare,
							new PXSetPropertyException(
								Messages.CompanyGroupBaseCurrencyDifferFromCompany,
								PXErrorLevel.Error,
								organization.OrganizationCD,
								group.OrganizationCD));
						return false;
					}
				}
			}
			return true;
		}
		#region CREATE LEDGER

		[Serializable]
		public class LedgerCreateParameters : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			[PXInt]
			public virtual int? OrganizationID { get; set; }
			#endregion

			#region LedgerCD
			public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }

			[PXString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
			[PXDefault]
			[PXUIField(DisplayName = "Ledger ID")]
			public virtual string LedgerCD { get; set; }
			#endregion

			#region BaseCuryID
			public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
			/// <summary>
			/// Base <see cref="Currency"/> of the Ledger.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="Currency.CuryID"/> field.
			/// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// </value>
			[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(Currency.curyID))]
			public virtual String BaseCuryID { get; set; }
			#endregion

			#region Descr
			public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

			[PXDBLocalizableString(60, IsUnicode = true, NonDB = true)]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = Messages.Description)]
			public virtual String Descr { get; set; }
			#endregion
		}

		public PXFilter<LedgerCreateParameters> CreateLedgerView;

		#region CreateLedgerSmartPanel
		public PXAction<OrganizationBAccount> createLedger;

		[PXUIField(DisplayName = "Create Ledger", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(Category = CS.ActionCategories.Other, DisplayOnMainToolbar = true)]
		public virtual IEnumerable CreateLedger(PXAdapter adapter)
		{
			if (OrganizationView.Current == null) return adapter.Get();

			if (CreateLedgerView.AskExtFullyValid(
				(graph, viewName) =>
				{
					CreateLedgerView.Current.OrganizationID = OrganizationView.Current.OrganizationID;
					CreateLedgerView.Current.BaseCuryID = OrganizationView.Current.BaseCuryID;
					CreateLedgerView.Current.Descr = String.Format(Messages.ActualLedgerDescription, OrganizationView.Current.OrganizationCD.Trim());
					PXDBLocalizableStringAttribute.SetTranslationsFromMessageFormatNLA<LedgerCreateParameters.descr>(
						CreateLedgerView.Cache,
						CreateLedgerView.Current,
						Messages.ActualLedgerDescription,
						OrganizationView.Current.OrganizationCD.Trim());
				},
				DialogAnswerType.Positive))
			{
				Save.Press();
				CreateLeadgerProc(CreateLedgerView.Current);
				throw new PXRefreshException();
			}

			return adapter.Get();
		}
		#endregion

		protected virtual void LedgerCreateParameters_LedgerCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Ledger ledger = PXSelectReadonly<Ledger, Where<Ledger.ledgerCD, Equal<Required<Ledger.ledgerCD>>>>.Select(this, e.NewValue);
			if (ledger != null)
			{
				throw new PXSetPropertyException(GL.Messages.LedgerAlreadyExists, e.NewValue);
			}
		}

		public static void CreateLeadgerProc(LedgerCreateParameters ledgerParamters)
		{
			GeneralLedgerMaint ledgerMaint;
			ledgerMaint = CreateInstance<GeneralLedgerMaint>();

			var ledger = ledgerMaint.LedgerRecords.Insert(new Ledger()
			{
				LedgerCD = ledgerParamters.LedgerCD,
				BalanceType = LedgerBalanceType.Actual,
				Descr = ledgerParamters.Descr,
				BaseCuryID = ledgerParamters.BaseCuryID
			});

			PXDBLocalizableStringAttribute.CopyTranslations<LedgerCreateParameters.descr, Ledger.descr>(ledgerMaint, ledgerParamters, ledger);

			ledgerMaint.Caches<OrganizationLedgerLink>().Insert(new OrganizationLedgerLink()
			{
				OrganizationID = ledgerParamters.OrganizationID
			});

			ledgerMaint.Save.Press();
		}

		#endregion

		public OrganizationChangeID ChangeID;
		#region Extensions

		#region Details

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefContactAddressExt : DefContactAddressExt<OrganizationMaint, OrganizationBAccount, OrganizationBAccount.acctName>
			.WithPersistentAddressValidation
		{
			#region ctor

			public override void Initialize()
			{
				base.Initialize();

				Base.ActionsMenu.AddMenuAction(ValidateAddresses);
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefLocationExt : DefLocationExt<OrganizationMaint, DefContactAddressExt, LocationDetailsExt, OrganizationBAccount, OrganizationBAccount.bAccountID, OrganizationBAccount.defLocationID>
			.WithUIExtension
		{
			#region Actions

			[PXUIField(DisplayName = CR.Messages.SetDefault, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
			[PXButton()]
			public new void SetDefaultLocation()
			{
				OrganizationBAccount acct = Base.BAccount.Current;
				if (this.LocationDetailsExtension.Locations.Current != null && acct != null && this.LocationDetailsExtension.Locations.Current.LocationID != acct.DefLocationID)
				{
					acct.DefLocationID = this.LocationDetailsExtension.Locations.Current.LocationID;
					Base.BAccount.Update(acct);
				}
			}

			#endregion

			#region Events
			
			[PXDBInt()]
			[PXDBChildIdentity(typeof(CRLocation.locationID))]
			[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
			[PXSelector(typeof(Search<CRLocation.locationID,
					Where<CRLocation.bAccountID, Equal<Current<OrganizationBAccount.bAccountID>>>>),
				DescriptionField = typeof(CRLocation.locationCD),
				DirtyRead = true)]
			protected override void _(Events.CacheAttached<OrganizationBAccount.defLocationID> e) { }

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationDetailsExt : LocationDetailsExt<OrganizationMaint, OrganizationBAccount, OrganizationBAccount.bAccountID>
		{
			#region Events

			protected virtual void _(Events.RowPersisted<Branch> e)
			{
				if (e.TranStatus == PXTranStatus.Open && (e.Operation & PXDBOperation.Delete) == PXDBOperation.Delete)
				{
					PXUpdate<
							Set<CRLocation.cBranchID, Null>,
							CRLocation,
							Where<CRLocation.cBranchID, Equal<Required<CRLocation.cBranchID>>>>
						.Update(Base, e.Row.BranchID);

					PXUpdate<
							Set<CRLocation.vBranchID, Null>,
							CRLocation,
							Where<CRLocation.vBranchID, Equal<Required<CRLocation.vBranchID>>>>
						.Update(Base, e.Row.BranchID);
				}
			}

			#endregion
		}

		#endregion

		#region Address Lookup Extension

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class OrganizationMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<OrganizationMaint, OrganizationBAccount, Address>
		{
			protected override string AddressView => nameof(DefContactAddressExt.DefAddress);
			protected override string ViewOnMap => nameof(DefContactAddressExt.ViewMainOnMap);
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class OrganizationMaintDefLocationAddressLookupExtension : CR.Extensions.AddressLookupExtension<OrganizationMaint, OrganizationBAccount, Address>
		{
			protected override string AddressView => nameof(DefLocationExt.DefLocationAddress);
			protected override string ViewOnMap => nameof(DefLocationExt.ViewDefLocationAddressOnMap);
		}

		#endregion

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class OrganizationLedgerLinkMaint : OrganizationLedgerLinkMaintBase<OrganizationMaint, OrganizationBAccount>
		{
			public PXAction<OrganizationBAccount> ViewLedger;

			public PXSelectJoin<OrganizationLedgerLink,
								LeftJoin<Ledger,
									On<OrganizationLedgerLink.ledgerID, Equal<Ledger.ledgerID>>>,
								Where<OrganizationLedgerLink.organizationID, Equal<Current<Organization.organizationID>>>>
								OrganizationLedgerLinkWithLedgerSelect;

			public override PXSelectBase<OrganizationLedgerLink> OrganizationLedgerLinkSelect => OrganizationLedgerLinkWithLedgerSelect;
			public override PXSelectBase<Organization> OrganizationViewBase => Base.OrganizationView;

			public PXSelect<Ledger> LedgerView;

			public override PXSelectBase<Ledger> LedgerViewBase => LedgerView;

			protected override Organization GetUpdatingOrganization(int? organizationID)
			{
				return Base.OrganizationView.Current;
			}

			protected override Type VisibleField => typeof(OrganizationLedgerLink.ledgerID);

			//Overridden because PXDBDefault is not compatible with PXDimesionSelector
			[PXDBInt(IsKey = true)]
			[PXDBDefault(typeof(Organization.organizationID))]
			[PXParent(typeof(Select<Organization, Where<Organization.organizationID, Equal<Current<OrganizationLedgerLink.organizationID>>>>))]
			protected virtual void OrganizationLedgerLink_OrganizationID_CacheAttached(PXCache sender)
			{
			}

			[PXMergeAttributes(Method = MergeMethod.Merge)]
			[PXSelector(
				typeof(Search<Ledger.ledgerID,
					Where<Ledger.balanceType, Equal<LedgerBalanceType.actual>,
						And<Ledger.baseCuryID, Equal<Current<Organization.baseCuryID>>,
							Or<Ledger.balanceType, NotEqual<LedgerBalanceType.actual>,
								Or<Ledger.baseCuryID, IsNull>>>>>),
				SubstituteKey = typeof(Ledger.ledgerCD),
				DescriptionField = typeof(Ledger.descr))]
			protected virtual void OrganizationLedgerLink_LedgerID_CacheAttached(PXCache sender)
			{
			}

			[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
			[PXButton]
			public virtual IEnumerable viewLedger(PXAdapter adapter)
			{
				OrganizationLedgerLink link = OrganizationLedgerLinkSelect.Current;

				if (link != null)
				{
					GeneralLedgerMaint.RedirectTo(link.LedgerID);
				}

				return adapter.Get();
			}
		}

		/// <exclude/>
		public class ExtendToCustomer : OrganizationUnitExtendToCustomer<OrganizationMaint, OrganizationBAccount>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.interBranch>();

			protected override SourceAccountMapping GetSourceAccountMapping()
			{
				return new SourceAccountMapping(typeof(OrganizationBAccount));
			}

			public override void Initialize()
			{
				base.Initialize();
				viewCustomer.SetCategory(CS.ActionCategories.Other);
				extendToCustomer.SetCategory(CS.ActionCategories.CompanyManagement);
			}
		}

		/// <exclude/>
		public class ExtendToVendor : OrganizationUnitExtendToVendor<OrganizationMaint, OrganizationBAccount>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.interBranch>();

			protected override SourceAccountMapping GetSourceAccountMapping()
			{
				return new SourceAccountMapping(typeof(OrganizationBAccount));
			}

			public override void Initialize()
			{
				base.Initialize();
				viewVendor.SetCategory(CS.ActionCategories.Other);
				extendToVendor.SetCategory(CS.ActionCategories.CompanyManagement);
			}
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class OrganizationSharedContactOverrideGraphExt : SharedChildOverrideGraphExt<OrganizationMaint, OrganizationSharedContactOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defContactID),
					IsOverrideRelated = typeof(CRLocation.overrideContact)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(OrganizationBAccount))
				{
					RelatedID = typeof(OrganizationBAccount.bAccountID),
					ChildID = typeof(OrganizationBAccount.defContactID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Contact))
				{
					ChildID = typeof(Contact.contactID),
					RelatedID = typeof(Contact.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class OrganizationSharedAddressOverrideGraphExt : SharedChildOverrideGraphExt<OrganizationMaint, OrganizationSharedAddressOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defAddressID),
					IsOverrideRelated = typeof(CRLocation.overrideAddress)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(OrganizationBAccount))
				{
					RelatedID = typeof(OrganizationBAccount.bAccountID),
					ChildID = typeof(OrganizationBAccount.defAddressID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Address))
				{
					ChildID = typeof(Address.addressID),
					RelatedID = typeof(Address.bAccountID),
				};
			}

			#endregion
		}

		#endregion
		#region Company Activation/Deactivation
		[PXUIField(DisplayName = Messages.Activate, MapViewRights = PXCacheRights.Select)]
		[PXButton(CommitChanges = true, Category = CS.ActionCategories.CompanyManagement)]
		public virtual IEnumerable activate(PXAdapter adapter)
		{
			Organization org = OrganizationView.Current;
			if (org == null) adapter.Get();
			Save.Press();

			PXLongOperation.StartOperation(this, delegate ()
			{
				ActivateCompanyProcess(org);
			});

			return adapter.Get();
		}

		public static void ActivateCompanyProcess(Organization organization)
		{
			OrganizationMaint orgMaint = PXGraph.CreateInstance<OrganizationMaint>();
			orgMaint.OrganizationView.Current = orgMaint.OrganizationView.Search<Organization.organizationID>(organization.OrganizationID);
			orgMaint.BAccount.Current = orgMaint.BAccount.Search<OrganizationBAccount.bAccountID>(organization.BAccountID);

			if (orgMaint.OrganizationView.Current != null && orgMaint.BAccount.Current != null)
			{
				using (var tranScope = new PXTransactionScope())
				{
					organization.Status = OrganizationStatus.Active;
					organization.Active = true;
					organization = orgMaint.OrganizationView.Update(organization);

					if (organization != null && !PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>())
					{
						orgMaint.SynchronizeMasterAndOrganizationPeriods(organization);
						orgMaint.SynchronizeMasterAndOrganizationFAPeriods(organization);
					}

					orgMaint.Save.Press();
					tranScope.Complete();
				}
			}
		}

		protected virtual void SynchronizeMasterAndOrganizationPeriods(Organization organization)
		{
			PXCache<MasterFinPeriod> masterPeriodCache = this.Caches<MasterFinPeriod>();

			foreach (MasterFinYear problemMasterYear in PXSelectJoin<
				MasterFinYear,
				LeftJoin<OrganizationFinYear,
					On<MasterFinYear.year, Equal<OrganizationFinYear.year>,
					And<OrganizationFinYear.organizationID, Equal<Required<OrganizationFinYear.organizationID>>>>>,
				Where<OrganizationFinYear.year, IsNull>>
				.Select(this, organization.OrganizationID))
			{
				OrganizationFinYear insertedOrgYear = (OrganizationFinYear)OrganizationYear.Insert(new OrganizationFinYear
				{
					OrganizationID = organization.OrganizationID,
					Year = problemMasterYear.Year,
					FinPeriods = problemMasterYear.FinPeriods,
					StartMasterFinPeriodID = GL.FinPeriods.FinPeriodUtils.GetFirstFinPeriodIDOfYear(problemMasterYear),
					StartDate = problemMasterYear.StartDate,
					EndDate = problemMasterYear.EndDate
				});
				if (insertedOrgYear == null)
				{
					throw new PXException(GL.Messages.CannotInsertOrganizationYear, problemMasterYear.Year, organization.OrganizationCD);
				}
			}

			IFinPeriodRepository finPeriodRepository = this.GetService<IFinPeriodRepository>();
			FinPeriod minFinPeriod = finPeriodRepository.FindFirstPeriod(organization.OrganizationID);
			FinPeriod maxFinPeriod = finPeriodRepository.FindLastPeriod(organization.OrganizationID);
			FinPeriod prevFinPeriod = null;

			foreach (MasterFinPeriod masterPeriod in PXSelectJoin<
				MasterFinPeriod,
				LeftJoin<OrganizationFinPeriod,
					On<MasterFinPeriod.finPeriodID, Equal<OrganizationFinPeriod.masterFinPeriodID>,
					And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>,
				Where<OrganizationFinPeriod.finPeriodID, IsNull>>
				.Select(this, organization.OrganizationID))
			{
				OrganizationFinPeriod missingOrgPeriod = (OrganizationFinPeriod)OrganizationPeriods.Insert(new OrganizationFinPeriod
				{
					OrganizationID = organization.OrganizationID,
					FinPeriodID = masterPeriod.FinPeriodID,
					MasterFinPeriodID = masterPeriod.FinPeriodID,
					FinYear = masterPeriod.FinYear,
					PeriodNbr = masterPeriod.PeriodNbr,
					Custom = masterPeriod.Custom,
					DateLocked = masterPeriod.DateLocked,
					StartDate = masterPeriod.StartDate,
					EndDate = masterPeriod.EndDate,
					Descr = masterPeriod.Descr
				});
				if (missingOrgPeriod == null)
				{
					throw new PXException(
						GL.Messages.CannotInsertOrganizationPeriod,
						FinPeriodIDFormattingAttribute.FormatForError(missingOrgPeriod.FinPeriodID),
						organization.OrganizationCD);
				}

				PXDBLocalizableStringAttribute.CopyTranslations<MasterFinPeriod.descr, OrganizationFinPeriod.descr>(
					masterPeriodCache,
					masterPeriod,
					OrganizationPeriods.Cache,
					missingOrgPeriod);

				if (PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
				{
					missingOrgPeriod.Status = masterPeriod.Status;
					missingOrgPeriod.ARClosed = masterPeriod.ARClosed;
					missingOrgPeriod.APClosed = masterPeriod.APClosed;
					missingOrgPeriod.FAClosed = masterPeriod.FAClosed;
					missingOrgPeriod.CAClosed = masterPeriod.CAClosed;
					missingOrgPeriod.INClosed = masterPeriod.INClosed;
					missingOrgPeriod.PRClosed = masterPeriod.PRClosed;
				}
				else
				{
					// compare with the very first and very last periods
					if (maxFinPeriod == null || string.CompareOrdinal(missingOrgPeriod.FinPeriodID, maxFinPeriod?.FinPeriodID) > 0)
					{
						// insert into the end
						missingOrgPeriod.Status = FinPeriod.status.Inactive;
						missingOrgPeriod.ARClosed = false;
						missingOrgPeriod.APClosed = false;
						missingOrgPeriod.FAClosed = false;
						missingOrgPeriod.CAClosed = false;
						missingOrgPeriod.INClosed = false;
						missingOrgPeriod.PRClosed = false;
					}
					else if (string.CompareOrdinal(missingOrgPeriod.FinPeriodID, minFinPeriod.FinPeriodID) < 0)
					{
						// insert into the beginning
						missingOrgPeriod.Status = minFinPeriod.Status;
						missingOrgPeriod.ARClosed = minFinPeriod.ARClosed;
						missingOrgPeriod.APClosed = minFinPeriod.APClosed;
						missingOrgPeriod.FAClosed = minFinPeriod.FAClosed;
						missingOrgPeriod.CAClosed = minFinPeriod.CAClosed;
						missingOrgPeriod.INClosed = minFinPeriod.INClosed;
						missingOrgPeriod.PRClosed = minFinPeriod.PRClosed;
					}
					else
					{
						// insert into the gap
						prevFinPeriod = (prevFinPeriod != null) ?
							prevFinPeriod
							: finPeriodRepository.FindPrevPeriod(organization.OrganizationID, missingOrgPeriod.FinPeriodID);
						missingOrgPeriod.Status = prevFinPeriod.Status;
						missingOrgPeriod.ARClosed = prevFinPeriod.ARClosed;
						missingOrgPeriod.APClosed = prevFinPeriod.APClosed;
						missingOrgPeriod.FAClosed = prevFinPeriod.FAClosed;
						missingOrgPeriod.CAClosed = prevFinPeriod.CAClosed;
						missingOrgPeriod.INClosed = prevFinPeriod.INClosed;
						missingOrgPeriod.PRClosed = prevFinPeriod.PRClosed;
					}
				}
				missingOrgPeriod = (OrganizationFinPeriod)OrganizationPeriods.Update(missingOrgPeriod);
				if (missingOrgPeriod == null)
				{
					throw new PXException(
						GL.Messages.CannotInsertOrganizationPeriod,
						FinPeriodIDFormattingAttribute.FormatForError(missingOrgPeriod.FinPeriodID),
						organization.OrganizationCD);
				}
			}
		}

		protected virtual void SynchronizeMasterAndOrganizationFAPeriods(Organization organization)
		{
			foreach (FABookYear problemMasterYear in PXSelectJoin<
				FABookYear,
				LeftJoin<FABook, On<FABook.bookID, Equal<FABookYear.bookID>>,
				LeftJoin<FABookYearAlias,
					On< FABookYear.year, Equal<FABookYearAlias.year>,
					And<FABookYear.bookID, Equal<FABookYearAlias.bookID>,
					And<FABookYearAlias.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>>>,
				Where<FABookYear.organizationID, Equal<GL.FinPeriods.TableDefinition.FinPeriod.organizationID.masterValue>,
					And<FABook.updateGL, Equal<True>,
					And<FABookYearAlias.year, IsNull>>>>
				.Select(this, organization.OrganizationID))
			{
				FABookYear insertedOrgYear = (FABookYear)FaBookYear.Insert(new FABookYear
				{
					OrganizationID = organization.OrganizationID,
					Year = problemMasterYear.Year,
					BookID = problemMasterYear.BookID,
					FinPeriods = problemMasterYear.FinPeriods,
					StartDate = problemMasterYear.StartDate,
					EndDate = problemMasterYear.EndDate,
					StartMasterFinPeriodID = problemMasterYear.StartMasterFinPeriodID ?? this.GetService<IFinPeriodRepository>().FindNearestOrganizationFinYear(organization.OrganizationID, problemMasterYear.Year).StartMasterFinPeriodID
				});
				if (insertedOrgYear == null)
				{
					throw new PXException(FA.Messages.CannotInsertFAOrganizationYear, problemMasterYear.Year, organization.OrganizationCD);
				}
			}

			foreach (FABookPeriod masterPeriod in PXSelectJoin<
				FABookPeriod,
				LeftJoin<FABook, On<FABook.bookID, Equal<FABookPeriod.bookID>>,
				LeftJoin<FABookPeriodAlias,
					On<FABookPeriod.finPeriodID, Equal<FABookPeriodAlias.finPeriodID>,
					And<FABookPeriod.bookID, Equal<FABookPeriodAlias.bookID>,
					And<FABookPeriodAlias.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>>>,
				Where<FABookPeriod.organizationID, Equal<GL.FinPeriods.TableDefinition.FinPeriod.organizationID.masterValue>,
					And<FABook.updateGL, Equal<True>,
					And<FABookPeriodAlias.finPeriodID, IsNull>>>>
				.Select(this, organization.OrganizationID))
			{
				FABookPeriod missingFAOrgPeriod = (FABookPeriod)FaBookPeriod.Insert(new FABookPeriod
				{
					OrganizationID = organization.OrganizationID,
					BookID = masterPeriod.BookID,
					FinPeriodID = masterPeriod.FinPeriodID,
					MasterFinPeriodID = masterPeriod.FinPeriodID,
					FinYear = masterPeriod.FinYear,
					PeriodNbr = masterPeriod.PeriodNbr,
					StartDate = masterPeriod.StartDate,
					EndDate = masterPeriod.EndDate,
					Active = masterPeriod.Active,
					Descr = masterPeriod.Descr
				});

				PXDBLocalizableStringAttribute.CopyTranslations<FABookPeriod.descr, FABookPeriodAlias.descr>(
					FaBookPeriod.Cache,
					masterPeriod,
					FaBookPeriod.Cache,
					missingFAOrgPeriod);

				if (missingFAOrgPeriod == null)
				{
					throw new PXException(
						FA.Messages.CannotInsertFAOrganizationPeriod,
						FinPeriodIDFormattingAttribute.FormatForError(missingFAOrgPeriod.FinPeriodID),
						organization.OrganizationCD);
				}
			}
		}

		[PXUIField(DisplayName = Messages.Deactivate, MapViewRights = PXCacheRights.Select)]
		[PXButton(CommitChanges = true, Category = CS.ActionCategories.CompanyManagement)]
		public virtual IEnumerable deactivate(PXAdapter adapter)
		{
			Organization org = OrganizationView.Current;
			if (org == null) adapter.Get();
			Save.Press();

			using (var tranScope = new PXTransactionScope())
			{
				org.Status = OrganizationStatus.Inactive;
				org.Active = false;
				org = OrganizationView.Update(org);
				Save.Press();

				tranScope.Complete();
			}
			return adapter.Get();
		}
		#endregion
	}
}
