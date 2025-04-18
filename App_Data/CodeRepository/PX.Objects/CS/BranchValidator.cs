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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Extensions;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.PM;

namespace PX.Objects.CS
{
	public class BranchValidator
	{
		protected int EntityCountInErrorMessage = 10;

		protected readonly PXGraph Graph;

		public BranchValidator(PXGraph graph)
		{
			Graph = graph;
		}

		public virtual void CanBeBranchesDeletedSeparately(IReadOnlyCollection<Branch> branches)
		{
			foreach (var branch in branches)
			{
				Graph.Caches[typeof(Branch)].ClearQueryCacheObsolete();

				Organization organization = OrganizationMaint.FindOrganizationByID(Graph, branch.OrganizationID);

				if (organization != null
				    && organization.OrganizationType == OrganizationTypes.WithoutBranches)
				{
					throw new PXException(Messages.TheBranchCannotBeDeletedBecauseItBelongsToTheCompanyOfTheWithoutBranchesType,
											branch.BranchCD.Trim(),
											organization.OrganizationCD.Trim());
				}
			}

			CanBeBranchesDeleted(branches);
		}

		public virtual void CanBeBranchesDeleted(IReadOnlyCollection<Branch> branches, bool isOrganizationWithoutBranchesDeletion = false)
		{
			int?[] baccountIDs = branches.Select(b => b.BAccountID).ToArray();
			int?[] branchIDs = branches.Select(b => b.BranchID).ToArray();

			using (new PXReadBranchRestrictedScope())
			{
				CheckRelatedCashAccountsDontExist(branchIDs);

				CheckRelatedEmployeesDoNotExist(baccountIDs);

				CheckRelatedGLHistoryDoesNotExist(branchIDs);

				CheckRelatedGLTranDoesNotExist(branchIDs);

				string warehouseMessage = null;
				string fixedAssetMessage = null;

				if (isOrganizationWithoutBranchesDeletion)
				{
					warehouseMessage = GL.Messages.CompanyCannotDeletedBecauseRelatedWarehousesExist;
					fixedAssetMessage = GL.Messages.CompanyCannotDeletedBecauseRelatedFixedAssetsExist;
				}
				else
				{
					warehouseMessage = GL.Messages.BranchCannotDeletedBecauseRelatedWarehousesExist;
					fixedAssetMessage = GL.Messages.BranchCannotDeletedBecauseRelatedFixedAssetsExist;
				}

				CheckRelatedWarehousesDontExist(branchIDs, warehouseMessage);

				CheckRelatedFixedAssetsDontExist(branchIDs, fixedAssetMessage);

				CheckRelatedProformaDoesNotExist(branchIDs, PM.Messages.BranchCannotDeleteReferencedByProforma);

				CheckProjectOrTemplateDoesNotExist(branchIDs);

				if (isOrganizationWithoutBranchesDeletion == false)
				{
					CheckRelatedBillingRulesDontExist(branchIDs, GL.Messages.BranchCannotDeletedBecauseRelatedBillingRulesExist);
				}

				CheckExtendedBranch(baccountIDs);
			}
		}

		public virtual void CheckRelatedGLTranDoesNotExist(int?[] branchIDs)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			GLTran tran = PXSelectReadonly<GLTran,
											Where<GLTran.branchID, In<Required<GLTran.branchID>>>>
											.SelectSingleBound(Graph, null, branchIDs);

			if (tran != null)
			{
				Branch branch = BranchMaint.FindBranchByID(Graph, tran.BranchID);

				throw new PXException(Messages.TheBranchOrBranchesCannotBeDeletedBecauseTheRelatedTransactionHasBeenPosted, 
										branch.BranchCD.Trim(), 
										tran.ToString());
			}
		}

		public virtual void CheckRelatedCashAccountsDontExist(int?[] branchIDs)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			CA.CashAccount[] cashAccounts = PXSelectReadonly<CA.CashAccount,
															Where<CA.CashAccount.branchID, In<Required<CA.CashAccount.branchID>>,
																And<CA.CashAccount.restrictVisibilityWithBranch, Equal<boolTrue>>>>
															.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchIDs)
															.RowCast<CA.CashAccount>()
															.ToArray();
			
			if (cashAccounts.Any())
			{
				IEnumerable<Branch> branches = BranchMaint.FindBranchesByID(Graph, branchIDs).ToArray();

				throw new PXException(Messages.CashAccountsForBranch,
										branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
										cashAccounts.Select(a => a.CashAccountCD.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}

		public virtual void CheckRelatedEmployeesDoNotExist(int?[] branchBAccountIDs)
		{
			if (branchBAccountIDs == null || branchBAccountIDs.IsEmpty())
				return;

			EPEmployee[] employees = PXSelectReadonly<EPEmployee,
														Where<EPEmployee.parentBAccountID, In<Required<EPEmployee.parentBAccountID>>>>
														.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchBAccountIDs)
														.RowCast<EPEmployee>()
														.ToArray();

			if (employees.Any())
			{
				IEnumerable<Branch> branches = PXSelectReadonly<Branch,
																Where<Branch.bAccountID, In<Required<Branch.bAccountID>>>>
																.Select(Graph, employees.Take(EntityCountInErrorMessage).Select(e => e.ParentBAccountID).ToArray())
																.RowCast<Branch>();

				throw new PXException(Messages.TheBranchOrBranchesCannotBeDeletedBecauseTheFollowingEmployeesAreAssigned,
										branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
										employees.Select(e => e.AcctCD.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}

		public virtual void CheckRelatedGLHistoryDoesNotExist(int?[] branchIDs)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			GLHistory history = GLUtility.GetRelatedToBranchGLHistory(Graph, branchIDs);

			if (history != null)
			{
				Branch branch = BranchMaint.FindBranchByID(Graph, history.BranchID);

				if (branch != null)
				{
					throw new PXException(Messages.BranchCanNotBeDeletedBecausePostedGLTransExist,
						branch.BranchCD.Trim());
				}
			}
		}

		public virtual void CheckProjectOrTemplateDoesNotExist(int?[] branchIDs)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			PM.PMProject[] projects = PXSelectReadonly<PM.PMProject,
															Where<PM.PMProject.defaultBranchID, In<Required<PM.PMProject.defaultBranchID>>,
																And<Where<PM.PMProject.baseType, Equal<CT.CTPRType.project>, 
																	Or<PM.PMProject.baseType, Equal<CT.CTPRType.projectTemplate>>>>>>
															.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchIDs)
															.RowCast<PM.PMProject>()
															.ToArray();

			if (projects.Any())
			{
				IEnumerable<Branch> branches = BranchMaint.FindBranchesByID(Graph, branchIDs).ToArray();

				throw new PXException(PM.Messages.BranchCanNotBeDeletedBecauseUsedInProjects,
										branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
										projects.Select(a => a.ContractCD.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}

		public virtual void CheckRelatedProformaDoesNotExist(int?[] branchIDs, string message)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			List<PM.PMProforma> proforma = PXSelectReadonly<PM.PMProforma,
				Where<PM.PMProforma.status, NotEqual<ProformaStatus.closed>,
				And<Where<PM.PMProforma.branchID, In<Required<PM.PMProforma.branchID>>>>>>
				.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchIDs)
				.RowCast<PM.PMProforma>().Select(_ => (PM.PMProforma)_).ToList();

			if (proforma.Any())
			{				
				throw new PXSetPropertyException(message, BranchMaint.FindBranchByID(Graph, branchIDs?[0]).BranchCD.Trim(), String.Join(", ", proforma.Select(x => x.RefNbr).ToArray()));
			}
		}

		public virtual void ValidateActiveField(int?[] branchIDs, bool? newValue, Organization organization) =>
			ValidateActiveField(branchIDs, newValue, organization, false);

		public virtual void ValidateActiveField(int?[] branchIDs, bool? newValue, Organization organization, bool skipActivateValidation = false)
		{
			if (newValue != true)
			{
				using (new PXReadBranchRestrictedScope())
				{
					string warehouseErrorMessaage = null;
					string fixedAssetsErrorMessaage = null;

					if (organization?.OrganizationType == OrganizationTypes.WithoutBranches)
					{
						warehouseErrorMessaage = GL.Messages.CompanyCannotBeSetAsInactiveBecauseRelatedWarehousesExist;
						fixedAssetsErrorMessaage = GL.Messages.CompanyCannotBeSetAsInactiveBecauseRelatedFixedAssetsExist;
					}
					else
					{
						warehouseErrorMessaage = GL.Messages.BranchCannotBeSetAsInactiveBecauseRelatedWarehousesExist;
						fixedAssetsErrorMessaage = GL.Messages.BranchCannotBeSetAsInactiveBecauseRelatedFixedAssetsExist;
					}

					CheckRelatedWarehousesDontExist(branchIDs, warehouseErrorMessaage);

					CheckRelatedFixedAssetsDontExist(branchIDs, fixedAssetsErrorMessaage);

					if (organization?.OrganizationType == OrganizationTypes.WithBranchesBalancing ||
						organization?.OrganizationType == OrganizationTypes.WithBranchesNotBalancing)
					{
						CheckRelatedBillingRulesDontExist(branchIDs, GL.Messages.BranchCannotBeSetAsInactiveBecauseRelatedBillingRulesExist);
					}

					CheckRelatedProformaDoesNotExist(branchIDs, PM.Messages.BranchCannotDeactiveReferencedByProforma);
				}
			}

			if (newValue == true && organization != null && organization.Active != true && skipActivateValidation == false)
			{
				throw new PXSetPropertyException(GL.Messages.BranchCannotBeActivatedBecauseItsParentCompanyIsInactive);
			}
		}

		public virtual void CheckRelatedFixedAssetsDontExist(int?[] branchIDs, string exceptionMessage)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			FixedAsset[] assets = PXSelectReadonly<FixedAsset,
													Where<FixedAsset.branchID, In<Required<FixedAsset.branchID>>,
													And<FixedAsset.status, NotEqual<FixedAssetStatus.reversed>,
													And<FixedAsset.status, NotEqual<FixedAssetStatus.disposed>>>>>
													.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchIDs)
													.RowCast<FixedAsset>()
													.ToArray();

			if (assets.Any())
			{
				IEnumerable<Branch> branches =
					BranchMaint.FindBranchesByID(Graph, assets.Take(EntityCountInErrorMessage).Select(a => a.BranchID).ToArray());

				throw new PXSetPropertyException(exceptionMessage,
												branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
												assets.Select(s => s.AssetCD.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}

		public virtual void CheckRelatedWarehousesDontExist(int?[] branchIDs, string exceptionMessage)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			INSite[] sites = PXSelectReadonly<INSite,
												Where<INSite.branchID, In<Required<INSite.branchID>>, And<INSite.active, Equal<True>>>>
												.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchIDs)
												.RowCast<INSite>()
												.ToArray();

			if (sites.Any())
			{
				IEnumerable<Branch> branches =
					BranchMaint.FindBranchesByID(Graph, sites.Take(EntityCountInErrorMessage).Select(a => a.BranchID).ToArray());

				throw new PXSetPropertyException(exceptionMessage,
													branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
													sites.Select(s => s.SiteCD.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}

		public virtual void CheckRelatedBillingRulesDontExist(int?[] branchIDs, string exceptionMessage)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			PMBillingRule[] rules = PXSelectReadonly<PMBillingRule,
												Where<PMBillingRule.targetBranchID, In<Required<PMBillingRule.targetBranchID>>>>
												.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchIDs)
												.RowCast<PMBillingRule>()
												.ToArray();

			if (rules.Any())
			{
				IEnumerable<Branch> branches =
					BranchMaint.FindBranchesByID(Graph, rules.Take(EntityCountInErrorMessage).Select(a => a.TargetBranchID).ToArray());

				throw new PXSetPropertyException(exceptionMessage,
													branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
													rules.Select(s => s.BillingID.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}

		public virtual void CheckExtendedBranch(int?[] branchBAccountIDs)
		{
			if (branchBAccountIDs == null || branchBAccountIDs.IsEmpty()) return;

			BAccount baccount = SelectFrom<BAccount>
				.Where<BAccount.bAccountID.IsIn<@P.AsInt>
					.And<BAccount.isBranch.IsEqual<True>>
					.And<BAccount.type.IsEqual<BAccountType.customerType>
						.Or<BAccount.type.IsEqual<BAccountType.vendorType>
						.Or<BAccount.type.IsEqual<BAccountType.combinedType>>>>>
				.View
				.ReadOnly
				.SelectSingleBound(Graph, null, new object[] { branchBAccountIDs });
			
			if(baccount != null)
			{
				throw new PXSetPropertyException(Messages.ExtendedBranchCannotBeDeleted, baccount.AcctCD.Trim());
			}
		}
	}
}
