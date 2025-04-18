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
using PX.Data.Update.ExchangeService;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.Objects.PR;
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		[PXHidden]
		public class PRCalculationEngineUtils : PXGraph<PRCalculationEngineUtils>
		{
			#region Public methods
			public virtual void CreateTaxDetail(
				PXGraph graph,
				PRTaxCode taxCode,
				PRPaymentTax paymentTax,
				IEnumerable<PREarningDetail> earnings,
				out TaxEarningDetailsSplits applicableTaxAmountsPerEarning)
			{
				if (paymentTax.TaxAmount == 0)
				{
					applicableTaxAmountsPerEarning = new TaxEarningDetailsSplits();
					return;
				}

				List<PREarningDetail> earningList = earnings.ToList();
				applicableTaxAmountsPerEarning = SplitTaxAmountsPerEarning(graph, paymentTax, earningList, out UnmatchedSplit unmatched);

				var matchingTaxDetailView = new SelectFrom<PRTaxDetail>
							.Where<PRTaxDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
								.And<PRTaxDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
								.And<PRTaxDetail.taxID.IsEqual<P.AsInt>>>.View(graph);
				matchingTaxDetailView.Select(taxCode.TaxID).ForEach(x => matchingTaxDetailView.Delete(x));

				var setupView = new SelectFrom<PRSetup>.View(graph);
				PRSetup preferences = setupView.SelectSingle();
				DetailSplitType splitType = GetExpenseSplitSettings(
					setupView.Cache,
					preferences,
					typeof(PRSetup.taxExpenseAcctDefault),
					typeof(PRSetup.taxExpenseSubMask),
					PRTaxExpenseAcctSubDefault.MaskEarningType,
					PRTaxExpenseAcctSubDefault.MaskLaborItem);

				int? paymentBranch = PXParentAttribute.SelectParent<PRPayment>(graph.Caches[typeof(PRPaymentTax)], paymentTax)?.BranchID;

				if (taxCode.TaxCategory == TaxCategory.EmployeeWithholding ||
					(preferences.ProjectCostAssignment != ProjectCostAssignmentType.WageLaborBurdenAssigned 
						&& splitType.SplitByProjectTask == false 
						&& splitType.SplitByEarningType == false
						&& splitType.SplitByLaborItem == false))
				{
					CreateDetailSplitByBranch<PRTaxDetail, int?>(matchingTaxDetailView.Cache, taxCode.TaxID, applicableTaxAmountsPerEarning, earningList, unmatched, paymentBranch);
				}
				else
				{
					if (preferences.ProjectCostAssignment == ProjectCostAssignmentType.WageLaborBurdenAssigned || splitType.SplitByProjectTask)
					{
						if (splitType.SplitByEarningType)
						{
							CreateDetailSplitByProjectTaskAndEarningType<PRTaxDetail, int?>(matchingTaxDetailView.Cache, taxCode.TaxID, applicableTaxAmountsPerEarning, earningList, unmatched);
						}
						else if (splitType.SplitByLaborItem)
						{
							CreateDetailSplitByProjectTaskAndLaborItem<PRTaxDetail, int?, PRSetup.taxExpenseAlternateAcctDefault, PRSetup.taxExpenseAlternateSubMask>(matchingTaxDetailView.Cache, taxCode.TaxID, applicableTaxAmountsPerEarning, earningList, unmatched, paymentBranch);
						}
						else
						{
							CreateDetailSplitByProjectTask<PRTaxDetail, int?, PRSetup.taxExpenseAlternateAcctDefault, PRSetup.taxExpenseAlternateSubMask>(matchingTaxDetailView.Cache, taxCode.TaxID, applicableTaxAmountsPerEarning, earningList, unmatched, paymentBranch);
						}

					}
					else
					{
						if (splitType.SplitByEarningType)
						{
							CreateDetailSplitByEarningType<PRTaxDetail, int?>(matchingTaxDetailView.Cache, taxCode.TaxID, applicableTaxAmountsPerEarning, earningList, unmatched);
						}
						else if (splitType.SplitByLaborItem)
						{
							CreateDetailSplitByLaborItem<PRTaxDetail, int?, PRSetup.taxExpenseAlternateAcctDefault, PRSetup.taxExpenseAlternateSubMask>(matchingTaxDetailView.Cache, taxCode.TaxID, applicableTaxAmountsPerEarning, earningList, unmatched, paymentBranch);
						}
					}
				}
			}

			public virtual void CreateDeductionDetail(PXGraph graph, PXCache deductionDetailViewCache, PRPaymentDeduct deduction, IEnumerable<PREarningDetail> earnings)
			{
				if (deduction?.IsActive != true || (deduction?.DedAmount).GetValueOrDefault() == 0)
				{
					return;
				}

				List<PREarningDetail> earningList = earnings.Where(x => x.IsFringeRateEarning != true).ToList();
				Dictionary<int?, decimal?> applicableAmountsPerEarning = SplitDedBenAmountsPerEarning(
					graph,
					ContributionType.EmployeeDeduction,
					deduction,
					earningList,
					out HashSet<UnmatchedBenefitSplit> unmatchedSplits);

				int? paymentBranch = PXParentAttribute.SelectParent<PRPayment>(graph.Caches[typeof(PRPaymentDeduct)], deduction)?.BranchID;
				CreateDetailSplitByBranch<PRDeductionDetail, int?>(deductionDetailViewCache, deduction.CodeID, applicableAmountsPerEarning, earningList, unmatchedSplits, paymentBranch);
			}

			public virtual void CreateBenefitDetail(PXGraph graph, PXCache benefitDetailViewCache, PRPaymentDeduct deduction, IEnumerable<PREarningDetail> earnings)
			{
				decimal? benefitAmount = deduction?.CntAmount;
				if (deduction?.IsActive != true || deduction?.NoFinancialTransaction == true || benefitAmount.GetValueOrDefault() == 0)
				{
					return;
				}

				var setupView = new SelectFrom<PRSetup>.View(graph);
				PRSetup preferences = setupView.SelectSingle();
				DetailSplitType splitType = GetExpenseSplitSettings(
					setupView.Cache,
					preferences,
					typeof(PRSetup.benefitExpenseAcctDefault),
					typeof(PRSetup.benefitExpenseSubMask),
					PRBenefitExpenseAcctSubDefault.MaskEarningType,
					PRBenefitExpenseAcctSubDefault.MaskLaborItem);

				List<PREarningDetail> earningList = earnings.Where(x => x.IsFringeRateEarning != true).ToList();
				Dictionary<int?, decimal?> applicableAmountsPerEarning = SplitDedBenAmountsPerEarning(
					graph,
					ContributionType.EmployerContribution,
					deduction,
					earningList,
					out HashSet<UnmatchedBenefitSplit> unmatchedSplits);
				int? paymentBranch = PXParentAttribute.SelectParent<PRPayment>(graph.Caches[typeof(PRPaymentDeduct)], deduction)?.BranchID;

				if (preferences.ProjectCostAssignment != ProjectCostAssignmentType.WageLaborBurdenAssigned 
					&& !splitType.SplitByProjectTask 
					&& !splitType.SplitByEarningType
					&& !splitType.SplitByLaborItem)
				{
					CreateDetailSplitByBranch<PRBenefitDetail, int?>(benefitDetailViewCache, deduction.CodeID, applicableAmountsPerEarning, earningList, unmatchedSplits, paymentBranch);
				}
				else
				{
					if (preferences.ProjectCostAssignment == ProjectCostAssignmentType.WageLaborBurdenAssigned || splitType.SplitByProjectTask)
					{
						if (splitType.SplitByEarningType)
						{
							CreateDetailSplitByProjectTaskAndEarningType<PRBenefitDetail, int?>(benefitDetailViewCache, deduction.CodeID, applicableAmountsPerEarning, earningList, unmatchedSplits);
						}
						else if (splitType.SplitByLaborItem)
						{
							CreateDetailSplitByProjectTaskAndLaborItem<PRBenefitDetail, int?, PRSetup.benefitExpenseAlternateAcctDefault, PRSetup.benefitExpenseAlternateSubMask>(benefitDetailViewCache, deduction.CodeID, applicableAmountsPerEarning, earningList, unmatchedSplits, paymentBranch);

						}
						else
						{
							CreateDetailSplitByProjectTask<PRBenefitDetail, int?, PRSetup.benefitExpenseAlternateAcctDefault, PRSetup.benefitExpenseAlternateSubMask>(benefitDetailViewCache, deduction.CodeID, applicableAmountsPerEarning, earningList, unmatchedSplits, paymentBranch);
						}
					}
					else
					{
						if (splitType.SplitByEarningType)
						{
							CreateDetailSplitByEarningType<PRBenefitDetail, int?>(benefitDetailViewCache, deduction.CodeID, applicableAmountsPerEarning, earningList, unmatchedSplits);
						}
						else if (splitType.SplitByLaborItem)
						{
							CreateDetailSplitByLaborItem<PRBenefitDetail, int?, PRSetup.benefitExpenseAlternateAcctDefault, PRSetup.benefitExpenseAlternateSubMask>(benefitDetailViewCache, deduction.CodeID, applicableAmountsPerEarning, earningList, unmatchedSplits, paymentBranch);
						}
					}
				}
			}

			public virtual DetailSplitType GetExpenseSplitSettings(
				PXCache setupCache,
				PRSetup setup,
				Type expenseAcctDefault,
				Type expenseSubMask,
				string maskEarningType,
				string maskLaborItem)
			{
				bool splitByProjectTask = setup.ProjectCostAssignment == ProjectCostAssignmentType.WageLaborBurdenAssigned;
				bool splitByEarningType = false;
				bool splitByLaborItem = false;

				string acctDefault = (string)setupCache.GetValue(setup, expenseAcctDefault.Name);
				if (acctDefault == maskEarningType)
				{
					splitByEarningType = true;
				}
				else if (acctDefault == maskLaborItem)
				{
					splitByLaborItem = true;
				}
				else if (acctDefault == GLAccountSubSource.Project || acctDefault == GLAccountSubSource.Task)
				{
					splitByProjectTask = true;
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
				{
					PRSubAccountMaskAttribute subMaskAttribute = setupCache.GetAttributesOfType<PRSubAccountMaskAttribute>(setup, expenseSubMask.Name).FirstOrDefault();
					if (subMaskAttribute != null)
					{
						string subMask = (string)setupCache.GetValue(setup, expenseSubMask.Name);
						PRDimensionMaskAttribute dimensionMaskAttribute = subMaskAttribute.GetAttribute<PRDimensionMaskAttribute>();
						if (dimensionMaskAttribute != null)
						{
							List<string> maskValues = dimensionMaskAttribute.GetSegmentMaskValues(subMask).ToList();
							if (maskValues.Contains(maskEarningType))
							{
								splitByEarningType = true;
							}
							else if (maskValues.Contains(maskLaborItem))
							{
								splitByLaborItem = true;
							}

							if (maskValues.Contains(GLAccountSubSource.Project) || maskValues.Contains(GLAccountSubSource.Task))
							{
								splitByProjectTask = true;
							}
						}
					}
				}

				return new DetailSplitType()
				{
					SplitByProjectTask = splitByProjectTask,
					SplitByEarningType = splitByEarningType,
					SplitByLaborItem = splitByLaborItem
				};
			}

			public virtual DedBenEarningDetailsSplits SplitDedBenAmountsPerEarning(
				PXGraph graph,
				PRPaymentDeduct deduction,
				List<PREarningDetail> earnings)
			{
				Dictionary<int?, decimal?> splitDeductions = SplitDedBenAmountsPerEarning(graph, ContributionType.EmployeeDeduction, deduction, earnings, out HashSet<UnmatchedBenefitSplit> _);
				Dictionary<int?, decimal?> splitBenefits = SplitDedBenAmountsPerEarning(graph, ContributionType.EmployerContribution, deduction, earnings, out HashSet<UnmatchedBenefitSplit> _);

				DedBenEarningDetailsSplits combineSplits = new DedBenEarningDetailsSplits();
				foreach (int? earningRecordID in splitDeductions.Keys.Intersect(splitBenefits.Keys))
				{
					DedBenAmount combined = new DedBenAmount();
					if (!splitDeductions.TryGetValue(earningRecordID, out combined.DeductionAmount))
					{
						combined.DeductionAmount = 0;
					}
					if (!splitBenefits.TryGetValue(earningRecordID, out combined.BenefitAmount))
					{
						combined.BenefitAmount = 0;
					}
					combineSplits[earningRecordID] = combined;
				}

				return combineSplits;
			}
			#endregion Public methods

			#region Helper methods
			protected virtual Dictionary<int?, decimal?> SplitDedBenAmountsPerEarning(
				PXGraph graph,
				string contributionType,
				PRPaymentDeduct deduction,
				List<PREarningDetail> earnings,
				out HashSet<UnmatchedBenefitSplit> unmatchedSplits)
			{
				Dictionary<int?, decimal?> applicableAmountPerEarning = new Dictionary<int?, decimal?>();

				Dictionary<int?, TaxEarningDetailsSplits> taxesSplitByEarning = new Dictionary<int?, TaxEarningDetailsSplits>();
				Dictionary<int?, DedBenEarningDetailsSplits> taxableDedBenSplitByEarning = new Dictionary<int?, DedBenEarningDetailsSplits>();
				PRDeductCode deductCode = PXSelectorAttribute.Select<PRPaymentDeduct.codeID>(graph.Caches[typeof(PRPaymentDeduct)], deduction) as PRDeductCode;
				if (GetCalcType(graph.Caches[typeof(PRDeductCode)], contributionType, deductCode) == DedCntCalculationMethod.PercentOfCustom)
				{
					// For Percent of Custom calculation, the taxes and taxable deductions and benefits need to be split by earning already.
					// There's no risk of infinite recursion as Percent of Custom can't be taxable.
					foreach (PRPaymentDeduct taxableDeductionInPayment in SelectFrom<PRPaymentDeduct>
						.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentDeduct.codeID>>
						.Where<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>
							.And<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
							.And<PRDeductCode.affectsTaxes.IsEqual<True>>
							.And<PRDeductCode.isActive.IsEqual<True>>>.View.Select(graph).FirstTableItems)

					{
						taxableDedBenSplitByEarning[taxableDeductionInPayment.CodeID] = SplitDedBenAmountsPerEarning(graph, taxableDeductionInPayment, earnings);
					}

					IEnumerable<PRPaymentTax> taxesInPayment = SelectFrom<PRPaymentTax>
						.Where<PRPaymentTax.docType.IsEqual<PRPayment.docType.FromCurrent>
							.And<PRPaymentTax.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View.Select(graph).FirstTableItems;
					foreach (PRPaymentTax paymentTax in taxesInPayment)
					{
						taxesSplitByEarning[paymentTax.TaxID] = SplitTaxAmountsPerEarning(graph, paymentTax, earnings, out UnmatchedSplit _);
					}
				}

				PRPaymentDeduct unmatchedEmployeeDeduct = null;
				List<PRPaymentProjectPackageDeduct> unmatchedProjectPackages = new List<PRPaymentProjectPackageDeduct>();
				List<PRPaymentFringeBenefit> unmatchedFringeAmountsInBenefitCode = new List<PRPaymentFringeBenefit>();
				List<PRPaymentUnionPackageDeduct> unmatchedUnionPackages = new List<PRPaymentUnionPackageDeduct>();
				List<PRPaymentWCPremium> unmatchedWCPackages = new List<PRPaymentWCPremium>();

				switch (deduction.Source)
				{
					case DeductionSourceListAttribute.EmployeeSettings:
						applicableAmountPerEarning = SplitEmployeeSettingDedBen(graph, contributionType, deduction, earnings, taxesSplitByEarning, taxableDedBenSplitByEarning);
						if (!applicableAmountPerEarning.Any())
						{
							unmatchedEmployeeDeduct = deduction;
						}
						break;
					case DeductionSourceListAttribute.CertifiedProject:
						applicableAmountPerEarning = SplitProjectDedBen(graph, contributionType, deduction, taxesSplitByEarning, taxableDedBenSplitByEarning, out unmatchedProjectPackages, out unmatchedFringeAmountsInBenefitCode);
						break;
					case DeductionSourceListAttribute.Union:
						applicableAmountPerEarning = SplitUnionDedBen(graph, contributionType, deduction, taxesSplitByEarning, taxableDedBenSplitByEarning, out unmatchedUnionPackages);
						break;
					case DeductionSourceListAttribute.WorkCode:
						applicableAmountPerEarning = SplitWorkCodeDedBen(graph, contributionType, deduction, taxesSplitByEarning, taxableDedBenSplitByEarning, out unmatchedWCPackages);
						break;
				}

				decimal? leftoverAmount;
				if (contributionType == ContributionType.EmployeeDeduction)
				{
					leftoverAmount = deduction.DedAmount - unmatchedProjectPackages.Sum(x => x.DeductionAmount.GetValueOrDefault())
						- unmatchedUnionPackages.Sum(x => x.DeductionAmount.GetValueOrDefault()) - unmatchedWCPackages.Sum(x => x.DeductionAmount.GetValueOrDefault());
				}
				else
				{
					leftoverAmount = deduction.CntAmount - unmatchedProjectPackages.Sum(x => x.BenefitAmount.GetValueOrDefault()) - unmatchedFringeAmountsInBenefitCode.Sum(x => x.FringeAmountInBenefit.GetValueOrDefault())
						- unmatchedUnionPackages.Sum(x => x.BenefitAmount.GetValueOrDefault()) - unmatchedWCPackages.Sum(x => x.Amount.GetValueOrDefault());
				}
				HandleRounding(applicableAmountPerEarning, leftoverAmount);			

				unmatchedSplits = SplitUnmatchedDedBenAmounts(graph, contributionType, unmatchedEmployeeDeduct, unmatchedProjectPackages, unmatchedFringeAmountsInBenefitCode, unmatchedUnionPackages, unmatchedWCPackages);
				return applicableAmountPerEarning;
			}

			protected virtual Dictionary<int?, decimal?> SplitEmployeeSettingDedBen(
				PXGraph graph,
				string contributionType,
				PRPaymentDeduct paymentDeduct,
				List<PREarningDetail> earnings,
				Dictionary<int?, TaxEarningDetailsSplits> taxesSplitByEarning,
				Dictionary<int?, DedBenEarningDetailsSplits> taxableDedBenSplitByEarning)
			{
				Dictionary<int?, decimal?> applicableAmountPerEarning = new Dictionary<int?, decimal?>();
				PRDeductCode deductCode = (PRDeductCode)PXSelectorAttribute.Select<PRPaymentDeduct.codeID>(graph.Caches[typeof(PRPaymentDeduct)], paymentDeduct);

				decimal? paymentDeductAmount = contributionType == ContributionType.EmployeeDeduction ? paymentDeduct.DedAmount : paymentDeduct.CntAmount;
				decimal totalApplicableHours = GetDedBenApplicableHours(graph, deductCode, contributionType, earnings);
				decimal totalApplicableAmount = earnings.Sum(x => GetDedBenApplicableAmount(graph, deductCode, contributionType, x, taxesSplitByEarning, taxableDedBenSplitByEarning));

				foreach (PREarningDetail earning in earnings)
				{
					string calcType = contributionType == ContributionType.EmployeeDeduction ? deductCode.DedCalcType : deductCode.CntCalcType;
					switch (calcType)
					{
						case DedCntCalculationMethod.FixedAmount:
							applicableAmountPerEarning[earning.RecordID] = paymentDeductAmount / earnings.Count();
							break;
						case DedCntCalculationMethod.AmountPerHour:
							if (totalApplicableHours != 0)
							{
								decimal applicableHours = GetDedBenApplicableHours(graph, deductCode, contributionType, earning);
								applicableAmountPerEarning[earning.RecordID] = applicableHours / totalApplicableHours * paymentDeductAmount;
							}
							break;
						default:
							if (totalApplicableAmount != 0)
							{
								decimal applicableAmount = GetDedBenApplicableAmount(graph, deductCode, contributionType, earning, taxesSplitByEarning, taxableDedBenSplitByEarning);
								applicableAmountPerEarning[earning.RecordID] = applicableAmount / totalApplicableAmount * paymentDeductAmount;
							}
							break;
					}
				}

				return applicableAmountPerEarning;
			}

			protected virtual Dictionary<int?, decimal?> SplitProjectDedBen(
				PXGraph graph,
				string contributionType,
				PRPaymentDeduct paymentDeduct,
				Dictionary<int?, TaxEarningDetailsSplits> taxesSplitByEarning,
				Dictionary<int?, DedBenEarningDetailsSplits> taxableDedBenSplitByEarning,
				out List<PRPaymentProjectPackageDeduct> unmatchedPackages,
				out List<PRPaymentFringeBenefit> unmatchedFringeAmountsInBenefitCode)
			{
				Dictionary<int?, decimal?> nominalAmountPerEarning = new Dictionary<int?, decimal?>();
				List<PXResult<PRPaymentProjectPackageDeduct>> paymentPackages = SelectFrom<PRPaymentProjectPackageDeduct>
					.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentProjectPackageDeduct.deductCodeID>>
					.Where<PRPaymentProjectPackageDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>
						.And<PRPaymentProjectPackageDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View.Select(graph).ToList();
				List<PXResult<PRPaymentProjectPackageDeduct>> paymentPackagesForDeduct = paymentPackages.Where(x => ((PRPaymentProjectPackageDeduct)x).DeductCodeID == paymentDeduct.CodeID).ToList();
				Dictionary<int?, PRPaymentProjectPackageDeduct> unmatchedMap = paymentPackagesForDeduct.ToDictionary(k => ((PRPaymentProjectPackageDeduct)k).RecordID, v => (PRPaymentProjectPackageDeduct)v);

				if (paymentPackages.Any())
				{
					// Certified project deduction or deduction summary record has been modified; calculate split taking into account existing
					// PRDeductionAndBenefitProjectPackage records.
					IEnumerable<PRDeductionAndBenefitProjectPackage> packages = SelectFrom<PRDeductionAndBenefitProjectPackage>
						.Where<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.IsEqual<P.AsInt>
							.And<PRDeductionAndBenefitProjectPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>.View.Select(graph, paymentDeduct.CodeID).FirstTableItems;
					IEnumerable<PREarningDetail> earnings = SelectFrom<PREarningDetail>
						.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
							.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
							.And<PREarningDetail.isFringeRateEarning.IsNotEqual<True>>>.View.Select(graph).FirstTableItems;
					foreach (PREarningDetail earningDetail in earnings)
					{
						decimal? nominalAmount = 0m;
						foreach (PXResult<PRPaymentProjectPackageDeduct, PRDeductCode> result in paymentPackagesForDeduct)
						{
							PRPaymentProjectPackageDeduct paymentPackage = result;
							PRDeductCode deductCode = result;
							PRDeductionAndBenefitProjectPackage packageDeduct = packages
								.Where(x => x.ProjectID == paymentPackage.ProjectID && x.LaborItemID == paymentPackage.LaborItemID && x.DeductionAndBenefitCodeID == paymentPackage.DeductCodeID && IsEarningApplicableToProjectDeduction(graph, earningDetail, x))
								.OrderByDescending(x => x.EffectiveDate)
								.FirstOrDefault();
							if (packageDeduct != null)
							{
								decimal? packageAmount = contributionType == ContributionType.EmployeeDeduction ? packageDeduct.DeductionAmount : packageDeduct.BenefitAmount;
								decimal? packageRate = contributionType == ContributionType.EmployeeDeduction ? packageDeduct.DeductionRate : packageDeduct.BenefitRate;
								string calcType = contributionType == ContributionType.EmployeeDeduction ? deductCode.DedCalcType : deductCode.CntCalcType;
								switch (calcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										int numberOfApplicableLines = earnings.Where(x => IsEarningApplicableToProjectDeduction(graph, x, packageDeduct)).Distinct(x => ((PREarningDetail)x).RecordID).Count();
										nominalAmount = packageAmount / numberOfApplicableLines;
										break;
									case DedCntCalculationMethod.PercentOfGross:
									case DedCntCalculationMethod.PercentOfCustom:
										decimal applicableAmount = GetDedBenApplicableAmount(graph, deductCode, contributionType, earningDetail, taxesSplitByEarning, taxableDedBenSplitByEarning);
										nominalAmount = applicableAmount * packageRate / 100;
										break;
									case DedCntCalculationMethod.AmountPerHour:
										decimal applicableHours = GetDedBenApplicableHours(graph, deductCode, contributionType, earningDetail);
										nominalAmount = applicableHours * packageAmount;
										break;
									case DedCntCalculationMethod.PercentOfNet:
										throw new PXException(Messages.PercentOfNetInCertifiedProject);
								}

								unmatchedMap.Remove(paymentPackage.RecordID);
							}
						}
						if (nominalAmount != 0)
						{
							nominalAmountPerEarning[earningDetail.RecordID] = nominalAmount;
						}
					}
				}
				else
				{
					List<PXResult<PREarningDetail>> projectEarnings = new ProjectDeductionQuery(graph).Select(paymentDeduct.CodeID).ToList();
					foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode>> resultGroup in projectEarnings
						.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode>)x)
						.GroupBy(x => ((PREarningDetail)x).RecordID))
					{
						PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitProjectPackage)x).EffectiveDate).First();
						PREarningDetail earningDetail = result;
						PRDeductionAndBenefitProjectPackage packageDeduct = result;
						PRDeductCode deductCode = result;

						decimal? packageAmount = contributionType == ContributionType.EmployeeDeduction ? packageDeduct.DeductionAmount : packageDeduct.BenefitAmount;
						decimal? packageRate = contributionType == ContributionType.EmployeeDeduction ? packageDeduct.DeductionRate : packageDeduct.BenefitRate;
						string calcType = contributionType == ContributionType.EmployeeDeduction ? deductCode.DedCalcType : deductCode.CntCalcType;
						switch (calcType)
						{
							case DedCntCalculationMethod.FixedAmount:
								int numberOfApplicableLines = projectEarnings.Where(x => IsEarningApplicableToProjectDeduction(graph, x, packageDeduct)).Distinct(x => ((PREarningDetail)x).RecordID).Count();
								nominalAmountPerEarning[earningDetail.RecordID] = packageAmount / numberOfApplicableLines;
								break;
							case DedCntCalculationMethod.PercentOfGross:
							case DedCntCalculationMethod.PercentOfCustom:
								decimal applicableAmount = GetDedBenApplicableAmount(graph, deductCode, contributionType, earningDetail, taxesSplitByEarning, taxableDedBenSplitByEarning);
								nominalAmountPerEarning[earningDetail.RecordID] = applicableAmount * packageRate / 100;
								break;
							case DedCntCalculationMethod.AmountPerHour:
								decimal applicableHours = GetDedBenApplicableHours(graph, deductCode, contributionType, earningDetail);
								nominalAmountPerEarning[earningDetail.RecordID] = applicableHours * packageAmount;
								break;
							case DedCntCalculationMethod.PercentOfNet:
								throw new PXException(Messages.PercentOfNetInCertifiedProject);
						}
					}
				}

				if (contributionType == ContributionType.EmployerContribution)
				{
					IEnumerable<PRPaymentFringeBenefit> fringeBenefits = SelectFrom<PRPaymentFringeBenefit>
								.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PRPaymentFringeBenefit.projectID>>
								.Where<PRPaymentFringeBenefit.docType.IsEqual<PRPayment.docType.FromCurrent>
									.And<PRPaymentFringeBenefit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
									.And<PMProjectExtension.benefitCodeReceivingFringeRate.IsEqual<P.AsInt>>
									.And<PRPaymentFringeBenefit.fringeAmountInBenefit.IsGreater<decimal0>>>.View.Select(graph, paymentDeduct.CodeID).FirstTableItems;
					Dictionary<PaymentFringeBenefitKey, PRPaymentFringeBenefit> unmatchedFringeAmountsInBenefitCodeMap = fringeBenefits.ToDictionary(k => new PaymentFringeBenefitKey(k), v => v);
					if (fringeBenefits.Any())
					{
						IEnumerable<PREarningDetail> fringeApplicableEarnings = new FringeBenefitApplicableEarningsQuery(graph).Select().FirstTableItems
							.Distinct(((PXCache<PREarningDetail>)Caches[typeof(PREarningDetail)]).GetComparer());
						Dictionary<PaymentFringeBenefitKey, decimal> splitFringeApplicableEarningTotals = fringeApplicableEarnings
							.GroupBy(x => new PaymentFringeBenefitKey(x.ProjectID, x.LabourItemID, x.ProjectTaskID))
							.ToDictionary(k => k.Key, v => v.Sum(x => x.Amount.GetValueOrDefault()));

						foreach (PREarningDetail fringeApplicableEarning in fringeApplicableEarnings)
						{
							PRPaymentFringeBenefit applicableFringeBenefit = fringeBenefits.FirstOrDefault(x => x.ProjectID == fringeApplicableEarning.ProjectID &&
								x.LaborItemID == fringeApplicableEarning.LabourItemID &&
								x.ProjectTaskID == fringeApplicableEarning.ProjectTaskID);
							PaymentFringeBenefitKey key = new PaymentFringeBenefitKey(fringeApplicableEarning.ProjectID, fringeApplicableEarning.LabourItemID, fringeApplicableEarning.ProjectTaskID);
							if (applicableFringeBenefit == null || splitFringeApplicableEarningTotals[key] == 0)
							{
								continue;
							}

							unmatchedFringeAmountsInBenefitCodeMap.Remove(key);
							if (!nominalAmountPerEarning.TryGetValue(fringeApplicableEarning.RecordID, out decimal? nominal))
							{
								nominal = 0;
							}
							nominalAmountPerEarning[fringeApplicableEarning.RecordID] = nominal + applicableFringeBenefit.FringeAmountInBenefit * fringeApplicableEarning.Amount / splitFringeApplicableEarningTotals[key];
						}
					}

					unmatchedFringeAmountsInBenefitCode = unmatchedFringeAmountsInBenefitCodeMap.Values.ToList();
				}
				else
				{
					unmatchedFringeAmountsInBenefitCode = new List<PRPaymentFringeBenefit>();
				}

				unmatchedPackages = unmatchedMap.Values.ToList();
				decimal? totalNominalAmount = nominalAmountPerEarning.Values.Sum();

				decimal? totalApplicableAmount;
				if (contributionType == ContributionType.EmployerContribution)
				{
					totalApplicableAmount = paymentDeduct.CntAmount - unmatchedPackages.Sum(x => x.BenefitAmount.GetValueOrDefault())
						- unmatchedFringeAmountsInBenefitCode.Sum(x => x.FringeAmountInBenefit);
				}
				else
				{
					totalApplicableAmount = paymentDeduct.DedAmount - unmatchedPackages.Sum(x => x.DeductionAmount.GetValueOrDefault());
				}

				Dictionary<int?, decimal?> applicableDedBenAmountPerEarning = new Dictionary<int?, decimal?>();
				if (totalNominalAmount != 0)
				{
					foreach (KeyValuePair<int?, decimal?> nominal in nominalAmountPerEarning)
					{
						applicableDedBenAmountPerEarning[nominal.Key] = nominal.Value * totalApplicableAmount / totalNominalAmount;
					}
				}

				return applicableDedBenAmountPerEarning;
			}

			protected virtual Dictionary<int?, decimal?> SplitUnionDedBen(
				PXGraph graph,
				string contributionType,
				PRPaymentDeduct paymentDeduct,
				Dictionary<int?, TaxEarningDetailsSplits> taxesSplitByEarning,
				Dictionary<int?, DedBenEarningDetailsSplits> taxableDedBenSplitByEarning,
				out List<PRPaymentUnionPackageDeduct> unmatchedPackages)
			{
				Dictionary<int?, decimal?> nominalAmountPerEarning = new Dictionary<int?, decimal?>();
				List<PXResult<PRPaymentUnionPackageDeduct>> paymentPackages = SelectFrom<PRPaymentUnionPackageDeduct>
					.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentUnionPackageDeduct.deductCodeID>>
					.Where<PRPaymentUnionPackageDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>
						.And<PRPaymentUnionPackageDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View.Select(graph).ToList();
				List<PXResult<PRPaymentUnionPackageDeduct>> paymentPackagesForDeduct = paymentPackages.Where(x => ((PRPaymentUnionPackageDeduct)x).DeductCodeID == paymentDeduct.CodeID).ToList();
				Dictionary<int?, PRPaymentUnionPackageDeduct> unmatchedMap = paymentPackagesForDeduct.ToDictionary(k => ((PRPaymentUnionPackageDeduct)k).RecordID, v => (PRPaymentUnionPackageDeduct)v);

				if (paymentPackages.Any())
				{
					// Union deduction or deduction summary record has been modified; calculate split taking into account existing
					// PRDeductionAndBenefitUnionPackage records.
					IEnumerable<PRDeductionAndBenefitUnionPackage> packages = SelectFrom<PRDeductionAndBenefitUnionPackage>
						.Where<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<P.AsInt>
							.And<PRDeductionAndBenefitUnionPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>.View.Select(graph, paymentDeduct.CodeID).FirstTableItems;
					IEnumerable<PREarningDetail> earnings = SelectFrom<PREarningDetail>
						.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
							.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
							.And<PREarningDetail.isFringeRateEarning.IsNotEqual<True>>>.View.Select(graph).FirstTableItems;
					foreach (PREarningDetail earningDetail in earnings)
					{
						decimal? nominalAmount = 0m;
						foreach (PXResult<PRPaymentUnionPackageDeduct, PRDeductCode> result in paymentPackagesForDeduct)
						{
							PRPaymentUnionPackageDeduct paymentPackage = result;
							PRDeductCode deductCode = result;
							PRDeductionAndBenefitUnionPackage packageDeduct = packages
								.Where(x => x.UnionID == paymentPackage.UnionID && x.LaborItemID == paymentPackage.LaborItemID && x.DeductionAndBenefitCodeID == paymentPackage.DeductCodeID && IsEarningApplicableToUnionDeduction(graph, earningDetail, x))
								.OrderByDescending(x => x.EffectiveDate)
								.FirstOrDefault();
							if (packageDeduct != null)
							{
								decimal? packageAmount = contributionType == ContributionType.EmployeeDeduction ? packageDeduct.DeductionAmount : packageDeduct.BenefitAmount;
								decimal? packageRate = contributionType == ContributionType.EmployeeDeduction ? packageDeduct.DeductionRate : packageDeduct.BenefitRate;
								string calcType = contributionType == ContributionType.EmployeeDeduction ? deductCode.DedCalcType : deductCode.CntCalcType;
								switch (calcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										int numberOfApplicableLines = earnings.Where(x => IsEarningApplicableToUnionDeduction(graph, x, packageDeduct)).Distinct(x => ((PREarningDetail)x).RecordID).Count();
										nominalAmount = packageAmount / numberOfApplicableLines;
										break;
									case DedCntCalculationMethod.PercentOfGross:
									case DedCntCalculationMethod.PercentOfCustom:
										decimal applicableAmount = GetDedBenApplicableAmount(graph, deductCode, contributionType, earningDetail, taxesSplitByEarning, taxableDedBenSplitByEarning);
										nominalAmount = applicableAmount * packageRate / 100;
										break;
									case DedCntCalculationMethod.AmountPerHour:
										decimal applicableHours = GetDedBenApplicableHours(graph, deductCode, contributionType, earningDetail);
										nominalAmount = applicableHours * packageAmount;
										break;
									case DedCntCalculationMethod.PercentOfNet:
										throw new PXException(Messages.PercentOfNetInUnion);
								}

								unmatchedMap.Remove(paymentPackage.RecordID);
							}
						}
						if (nominalAmount != 0)
						{
							nominalAmountPerEarning[earningDetail.RecordID] = nominalAmount;
						}
					}
				}
				else
				{
					List<PXResult<PREarningDetail>> unionEarnings = new UnionDeductionQuery(graph).Select(paymentDeduct.CodeID).ToList();
					foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode>> resultGroup in unionEarnings
						.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode>)x)
						.GroupBy(x => ((PREarningDetail)x).RecordID))
					{
						PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitUnionPackage)x).EffectiveDate).First();
						PREarningDetail earningDetail = result;
						PRDeductionAndBenefitUnionPackage packageDeduct = result;
						PRDeductCode deductCode = result;

						decimal? packageAmount = contributionType == ContributionType.EmployeeDeduction ? packageDeduct.DeductionAmount : packageDeduct.BenefitAmount;
						decimal? packageRate = contributionType == ContributionType.EmployeeDeduction ? packageDeduct.DeductionRate : packageDeduct.BenefitRate;
						string calcType = contributionType == ContributionType.EmployeeDeduction ? deductCode.DedCalcType : deductCode.CntCalcType;
						switch (calcType)
						{
							case DedCntCalculationMethod.FixedAmount:
								int numberOfApplicableLines = unionEarnings.Where(x => IsEarningApplicableToUnionDeduction(graph, x, packageDeduct)).Distinct(x => ((PREarningDetail)x).RecordID).Count();
								nominalAmountPerEarning[earningDetail.RecordID] = packageAmount / numberOfApplicableLines;
								break;
							case DedCntCalculationMethod.PercentOfGross:
							case DedCntCalculationMethod.PercentOfCustom:
								decimal applicableAmount = GetDedBenApplicableAmount(graph, deductCode, contributionType, earningDetail, taxesSplitByEarning, taxableDedBenSplitByEarning);
								nominalAmountPerEarning[earningDetail.RecordID] = applicableAmount * packageRate / 100;
								break;
							case DedCntCalculationMethod.AmountPerHour:
								decimal applicableHours = GetDedBenApplicableHours(graph, deductCode, contributionType, earningDetail);
								nominalAmountPerEarning[earningDetail.RecordID] = applicableHours * packageAmount;
								break;
							case DedCntCalculationMethod.PercentOfNet:
								throw new PXException(Messages.PercentOfNetInUnion);
						}
					}
				}

				unmatchedPackages = unmatchedMap.Values.ToList();
				decimal? totalNominalAmount = nominalAmountPerEarning.Values.Sum();
				decimal? totalApplicableAmount = (contributionType == ContributionType.EmployeeDeduction ? paymentDeduct.DedAmount : paymentDeduct.CntAmount)
					- unmatchedPackages.Sum(x => contributionType == ContributionType.EmployeeDeduction ? x.DeductionAmount : x.BenefitAmount);
				Dictionary<int?, decimal?> applicableDedBenAmountPerEarning = new Dictionary<int?, decimal?>();
				if (totalNominalAmount != 0)
				{
					foreach (KeyValuePair<int?, decimal?> nominal in nominalAmountPerEarning)
					{
						applicableDedBenAmountPerEarning[nominal.Key] = nominal.Value * totalApplicableAmount / totalNominalAmount;
					}
				}

				return applicableDedBenAmountPerEarning;
			}

			protected virtual Dictionary<int?, decimal?> SplitWorkCodeDedBen(
				PXGraph graph,
				string contributionType,
				PRPaymentDeduct paymentDeduct,
				Dictionary<int?, TaxEarningDetailsSplits> taxesSplitByEarning,
				Dictionary<int?, DedBenEarningDetailsSplits> taxableDedBenSplitByEarning,
				out List<PRPaymentWCPremium> unmatched)
			{
				Dictionary<int?, decimal?> nominalWorkCodeAmountPerEarning = new Dictionary<int?, decimal?>();
				var query = new SelectFrom<PREarningDetail>
					.InnerJoin<PRLocation>.On<PREarningDetail.locationID.IsEqual<PRLocation.locationID>>
					.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
					.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<P.AsInt>
						.And<PRDeductCode.isWorkersCompensation.IsEqual<True>>
						.And<PRDeductCode.state.IsEqual<Address.state>>>
					.InnerJoin<PRWorkCompensationBenefitRate>.On<PRWorkCompensationBenefitRate.deductCodeID.IsEqual<PRDeductCode.codeID>
						.And<PRWorkCompensationBenefitRate.workCodeID.IsEqual<PREarningDetail.workCodeID>>>
					.InnerJoin<PMWorkCode>.On<PMWorkCode.workCodeID.IsEqual<PREarningDetail.workCodeID>>
					.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
					.Where<PMWorkCode.isActive.IsEqual<True>
						.And<PRDeductCode.isActive.IsEqual<True>>
						.And<PRDeductCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>
						.And<PRxPMWorkCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>
						.And<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>>
						.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
						.And<PREarningType.isWCCCalculation.IsEqual<True>>
						.And<PRWorkCompensationBenefitRate.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>
					.OrderBy<PREarningDetail.date.Asc, EPEarningType.isOvertime.Asc>.View(graph);

				PRDeductCode deductCode = PRDeductCode.PK.Find(graph, paymentDeduct.CodeID);
				List<PRPaymentWCPremium> wcPremiums = SelectFrom<PRPaymentWCPremium>
					.Where<PRPaymentWCPremium.docType.IsEqual<PRPayment.docType.FromCurrent>
						.And<PRPaymentWCPremium.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
						.And<PRPaymentWCPremium.deductCodeID.IsEqual<P.AsInt>>
						.And<PRPaymentWCPremium.contribType.IsEqual<P.AsString>
							.Or<PRPaymentWCPremium.contribType.IsEqual<ContributionTypeListAttribute.bothDeductionAndContribution>>>>.View
					.Select(graph, paymentDeduct.CodeID, contributionType).FirstTableItems.ToList();
				Dictionary<WCPremiumKey, PRPaymentWCPremium> unmatchedMap =
					wcPremiums.ToDictionary(k => new WCPremiumKey(k.WorkCodeID, deductCode.State, k.BranchID),  v => v);
				Dictionary<WCPremiumKey, decimal> premiumRemainingAmounts = wcPremiums.ToDictionary(
					k => new WCPremiumKey(k.WorkCodeID, deductCode.State, k.BranchID),
					v => (contributionType == ContributionType.EmployeeDeduction ? v.DeductionAmount : v.Amount).GetValueOrDefault());

				foreach (IGrouping<int?, PXResult<PREarningDetail, PRLocation, Address, PRDeductCode, PRWorkCompensationBenefitRate, PMWorkCode, EPEarningType>> group in query
					.Select(paymentDeduct.CodeID)
					.Select(x => (PXResult<PREarningDetail, PRLocation, Address, PRDeductCode, PRWorkCompensationBenefitRate, PMWorkCode, EPEarningType>)x)
					.GroupBy(x => ((PREarningDetail)x).RecordID))
				{
					PXResult<PREarningDetail, PRLocation, Address, PRDeductCode, PRWorkCompensationBenefitRate, PMWorkCode, EPEarningType> effectiveResult =
						group.OrderByDescending(x => ((PRWorkCompensationBenefitRate)x).EffectiveDate).First();

					PREarningDetail earning = effectiveResult;
					PRWorkCompensationBenefitRate effectiveRate = GetApplicableWCRate(graph, earning.WorkCodeID, deductCode.CodeID, earning.BranchID);
					if (effectiveRate == null)
					{
						continue;
					}

					decimal? packageRate = contributionType == ContributionType.EmployeeDeduction ? effectiveRate.DeductionRate : effectiveRate.Rate;
					string calcType = contributionType == ContributionType.EmployeeDeduction ? deductCode.DedCalcType : deductCode.CntCalcType;
					switch (calcType)
					{
						case DedCntCalculationMethod.PercentOfGross:
						case DedCntCalculationMethod.PercentOfCustom:
							decimal applicableAmount = GetDedBenApplicableAmount(graph, deductCode, contributionType, earning, taxesSplitByEarning, taxableDedBenSplitByEarning);
							nominalWorkCodeAmountPerEarning[group.Key] = applicableAmount * packageRate / 100;
							break;
						case DedCntCalculationMethod.AmountPerHour:
							decimal applicableHours = GetDedBenApplicableHours(graph, deductCode, contributionType, earning);
							nominalWorkCodeAmountPerEarning[group.Key] = applicableHours * packageRate;
							break;
					}

					// Nominal amounts per WCPremiumKey grouping should not go over the reported PRPaymentWCPremium amount
					WCPremiumKey premiumKey = new WCPremiumKey(effectiveRate.WorkCodeID, deductCode.State, earning.BranchID);
					if (premiumRemainingAmounts.ContainsKey(premiumKey))
					{
						nominalWorkCodeAmountPerEarning[group.Key] = Math.Min(
							nominalWorkCodeAmountPerEarning[group.Key].GetValueOrDefault(),
							premiumRemainingAmounts[premiumKey]);
						nominalWorkCodeAmountPerEarning[group.Key] = Math.Max(
							nominalWorkCodeAmountPerEarning[group.Key].GetValueOrDefault(),
							0m);
						premiumRemainingAmounts[premiumKey] -= nominalWorkCodeAmountPerEarning[group.Key].GetValueOrDefault();
					}

					unmatchedMap.Remove(premiumKey);
				}

				unmatched = unmatchedMap.Values.ToList();
				decimal? totalNominalAmount = nominalWorkCodeAmountPerEarning.Values.Sum();
				decimal? totalApplicableAmount = (contributionType == ContributionType.EmployeeDeduction ? paymentDeduct.DedAmount : paymentDeduct.CntAmount)
					- unmatched.Sum(x => contributionType == ContributionType.EmployeeDeduction ? x.DeductionAmount : x.Amount);
				Dictionary<int?, decimal?> applicableDedBenAmountPerEarning = new Dictionary<int?, decimal?>();
				if (totalNominalAmount != 0)
				{
					foreach (KeyValuePair<int?, decimal?> nominal in nominalWorkCodeAmountPerEarning)
					{
						applicableDedBenAmountPerEarning[nominal.Key] = nominal.Value * totalApplicableAmount / totalNominalAmount;
					}
				}

				return applicableDedBenAmountPerEarning;
			}

			protected virtual HashSet<UnmatchedBenefitSplit> SplitUnmatchedDedBenAmounts(
				PXGraph graph,
				string contributionType,
				PRPaymentDeduct unmatchedEmployeeDeduct,
				List<PRPaymentProjectPackageDeduct> unmatchedProjectPackages,
				List<PRPaymentFringeBenefit> unmatchedFringeAmountsInBenefitCode,
				List<PRPaymentUnionPackageDeduct> unmatchedUnionPackages,
				List<PRPaymentWCPremium> unmatchedWCPackages)
			{
				HashSet<UnmatchedBenefitSplit> splits = new HashSet<UnmatchedBenefitSplit>();

				if (unmatchedEmployeeDeduct != null)
				{
					decimal? amount = contributionType == ContributionType.EmployeeDeduction ? unmatchedEmployeeDeduct.DedAmount : unmatchedEmployeeDeduct.CntAmount;
					UpdateUnmatchedSplit(splits, null, null, amount);
				}

				unmatchedWCPackages.ForEach(x => UpdateUnmatchedSplit(splits, null, null, contributionType == ContributionType.EmployeeDeduction ? x.DeductionAmount : x.Amount));
				unmatchedProjectPackages.ForEach(x => UpdateUnmatchedSplit(splits, x.ProjectID, x.LaborItemID, contributionType == ContributionType.EmployeeDeduction ? x.DeductionAmount : x.BenefitAmount));
				unmatchedUnionPackages.ForEach(x => UpdateUnmatchedSplit(splits, null, x.LaborItemID, contributionType == ContributionType.EmployeeDeduction ? x.DeductionAmount : x.BenefitAmount));
				if (contributionType == ContributionType.EmployerContribution)
				{
					unmatchedFringeAmountsInBenefitCode.ForEach(x => UpdateUnmatchedSplit(splits, x.ProjectID, x.LaborItemID, x.FringeAmountInBenefit));
				}

				return splits;
			}

			protected virtual void UpdateUnmatchedSplit(HashSet<UnmatchedBenefitSplit> splits, int? projectID, int? laborItemID, decimal? amountToAdd)
			{
				if (!splits.TryGetValue(new UnmatchedBenefitSplit(projectID, laborItemID), out UnmatchedBenefitSplit split))
				{
					split = new UnmatchedBenefitSplit(projectID, laborItemID);
					splits.Add(split);
				}
				split.Amount += amountToAdd.GetValueOrDefault();
			}

			protected virtual TaxEarningDetailsSplits SplitTaxAmountsPerEarning(
				PXGraph graph,
				PRPaymentTax paymentTax,
				List<PREarningDetail> earnings,
				out UnmatchedSplit unmatchedTaxAmount)
			{
				unmatchedTaxAmount = null;
				decimal totalEarnings = earnings.Sum(x => x.Amount.GetValueOrDefault());

				TaxEarningDetailsSplits amountsPerEarning = new TaxEarningDetailsSplits();
				foreach (PREarningDetail earning in earnings)
				{
					if (totalEarnings != 0)
					{
						amountsPerEarning[earning.RecordID] = Math.Round((paymentTax.TaxAmount * earning.Amount / totalEarnings).GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);
					}
				}

				if (amountsPerEarning.Any())
				{
					HandleRounding(amountsPerEarning, paymentTax.TaxAmount);
				}
				else if (paymentTax.TaxAmount != 0)
				{
					unmatchedTaxAmount = new UnmatchedSplit()
					{
						Amount = paymentTax.TaxAmount.GetValueOrDefault()
					};
				}

				return amountsPerEarning;
			}

			protected virtual void CreateDetailSplitByBranch<TDetail, TKey>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				HashSet<UnmatchedBenefitSplit> unmatchedSplits,
				int? paymentBranch)
				where TDetail : class, IPaycheckDetail<TKey>, new()
			{
				IEnumerable<ExpenseDetailSplitGrouping> earningGroups = earnings.SplitExpenseDetails(ExpenseDetailSplitType.Branch);
				DistributeNegativeSplits(earningGroups, applicableAmountsPerEarning);
				foreach (ExpenseDetailSplitGrouping earningGroup in earningGroups)
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(
						codeID,
						applicableAmountsPerEarning,
						earningGroup,
						paymentBranch,
						unmatchedSplits,
						true,
						x => true);
					if (newDetail != null)
					{
						detailViewCache.Update(newDetail);
					}
				}

				CreateDetailForUnmatchedSplits<TDetail, TKey>(detailViewCache, codeID, unmatchedSplits);
			}

			protected virtual void CreateDetailSplitByProjectTask<TDetail, TKey, TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				HashSet<UnmatchedBenefitSplit> unmatchedSplits,
				int? paymentBranch)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
				where TExpenseAlternateAcctDefault : IBqlField
				where TExpenseAlternateSubMask : IBqlField
			{
				ExpenseDetailSplitType flags = ExpenseDetailSplitType.ProjectTask;
				flags = CheckForAlternateAccountSubSource<TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(detailViewCache.Graph, flags);
				IEnumerable<ExpenseDetailSplitGrouping> projectTaskPairs = earnings.SplitExpenseDetails(flags);
				DistributeNegativeSplits(projectTaskPairs, applicableAmountsPerEarning);
				foreach (ExpenseDetailSplitGrouping projectTaskPair in projectTaskPairs)
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(
						codeID,
						applicableAmountsPerEarning,
						projectTaskPair,
						paymentBranch,
						unmatchedSplits,
						projectTaskPair.Key.ProjectTaskID == null,
						x => (x as UnmatchedBenefitSplit)?.ProjectID == projectTaskPair.Key.ProjectID);
					if (newDetail != null)
					{
						newDetail.ProjectID = projectTaskPair.Key.ProjectID;
						newDetail.ProjectTaskID = projectTaskPair.Key.ProjectTaskID;
						newDetail.CostCodeID = projectTaskPair.Key.CostCodeID;
						newDetail.EarningTypeCD = projectTaskPair.Key.EarningTypeCD;
						detailViewCache.Update(newDetail);
					}
				}

				foreach (IGrouping<int?, UnmatchedBenefitSplit> splitGroup in unmatchedSplits.Where(x => !x.Handled).GroupBy(x => x.ProjectID))
				{
					var newDetail = new TDetail();
					newDetail.ParentKeyID = codeID;
					newDetail.Amount = splitGroup.Sum(x => x.Amount);
					newDetail.ProjectID = splitGroup.Key;
					if (newDetail.Amount != 0)
					{
						detailViewCache.Update(newDetail);
					}
				}
			}

			protected virtual void CreateDetailSplitByProjectTaskAndEarningType<TDetail, TKey>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				HashSet<UnmatchedBenefitSplit> unmatchedSplits)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
			{
				IEnumerable<ExpenseDetailSplitGrouping> earningGroups = earnings.SplitExpenseDetails(ExpenseDetailSplitType.ProjectTask | ExpenseDetailSplitType.EarningType);
				DistributeNegativeSplits(earningGroups, applicableAmountsPerEarning);
				foreach (ExpenseDetailSplitGrouping earningGroup in earningGroups)
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(codeID, applicableAmountsPerEarning, earningGroup);
					if (newDetail != null)
					{
						newDetail.ProjectID = earningGroup.Key.ProjectID;
						newDetail.ProjectTaskID = earningGroup.Key.ProjectTaskID;
						newDetail.EarningTypeCD = earningGroup.Key.EarningTypeCD;
						newDetail.CostCodeID = earningGroup.Key.CostCodeID;
						detailViewCache.Update(newDetail);
					}
				}

				foreach (IGrouping<int?, UnmatchedBenefitSplit> splitGroup in unmatchedSplits.GroupBy(x => x.ProjectID))
				{
					var newDetail = new TDetail();
					newDetail.ParentKeyID = codeID;
					newDetail.Amount = splitGroup.Sum(x => x.Amount);
					newDetail.ProjectID = splitGroup.Key;
					if (newDetail.Amount != 0)
					{
						detailViewCache.Update(newDetail);
					}
				}
			}

			protected virtual void CreateDetailSplitByProjectTaskAndLaborItem<TDetail, TKey, TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				HashSet<UnmatchedBenefitSplit> unmatchedSplits,
				int? paymentBranch)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
				where TExpenseAlternateAcctDefault : IBqlField
				where TExpenseAlternateSubMask : IBqlField
			{
				ExpenseDetailSplitType flags = ExpenseDetailSplitType.ProjectTask | ExpenseDetailSplitType.LaborItem;
				flags = CheckForAlternateAccountSubSource<TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(detailViewCache.Graph, flags);
				IEnumerable<ExpenseDetailSplitGrouping> earningGroups = earnings.SplitExpenseDetails(flags);
				DistributeNegativeSplits(earningGroups, applicableAmountsPerEarning);
				foreach (ExpenseDetailSplitGrouping earningGroup in earningGroups)
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(
						codeID,
						applicableAmountsPerEarning,
						earningGroup,
						paymentBranch,
						unmatchedSplits,
						earningGroup.Key.ProjectTaskID == null,
						x => (x as UnmatchedBenefitSplit)?.ProjectID == earningGroup.Key.ProjectID &&
							(x as UnmatchedBenefitSplit)?.LaborItemID == earningGroup.Key.LaborItemID);
					if (newDetail != null)
					{
						newDetail.ProjectID = earningGroup.Key.ProjectID;
						newDetail.ProjectTaskID = earningGroup.Key.ProjectTaskID;
						newDetail.LabourItemID = earningGroup.Key.LaborItemID;
						newDetail.CostCodeID = earningGroup.Key.CostCodeID;
						newDetail.EarningTypeCD = earningGroup.Key.EarningTypeCD;
						detailViewCache.Update(newDetail);
					}
				}

				foreach (IGrouping<(int? projectID, int? laborItemID), UnmatchedBenefitSplit> splitGroup in unmatchedSplits.Where(x => !x.Handled).GroupBy(x => (x.ProjectID, x.LaborItemID)))
				{
					var newDetail = new TDetail();
					newDetail.ParentKeyID = codeID;
					newDetail.Amount = splitGroup.Sum(x => x.Amount);
					newDetail.ProjectID = splitGroup.Key.projectID;
					newDetail.LabourItemID = splitGroup.Key.laborItemID;
					if (newDetail.Amount != 0)
					{
						detailViewCache.Update(newDetail);
					}
				}
			}

			protected virtual ExpenseDetailSplitType CheckForAlternateAccountSubSource<TExpenseAcctDefault, TExpenseSubMask>(PXGraph graph, ExpenseDetailSplitType flags)
				where TExpenseAcctDefault : IBqlField
				where TExpenseSubMask : IBqlField
			{
				if (PRAccountSubHelper.IsVisiblePerSetup<TExpenseAcctDefault, TExpenseSubMask>(graph, GLAccountSubSource.EarningType))
				{
					flags |= ExpenseDetailSplitType.EarningType;
				}

				return flags;
			}

			protected virtual void CreateDetailSplitByEarningType<TDetail, TKey>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				HashSet<UnmatchedBenefitSplit> unmatchedSplits)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
			{
				IEnumerable<ExpenseDetailSplitGrouping> earningGroups = earnings.SplitExpenseDetails(ExpenseDetailSplitType.EarningType);
				DistributeNegativeSplits(earningGroups, applicableAmountsPerEarning);
				foreach (ExpenseDetailSplitGrouping earningGroup in earningGroups)
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(codeID, applicableAmountsPerEarning, earningGroup);
					if (newDetail != null)
					{
						newDetail.EarningTypeCD = earningGroup.Key.EarningTypeCD;
						detailViewCache.Update(newDetail);
					}
				}

				CreateDetailForUnmatchedSplits<TDetail, TKey>(detailViewCache, codeID, unmatchedSplits);
			}

			protected virtual void CreateDetailSplitByLaborItem<TDetail, TKey, TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				HashSet<UnmatchedBenefitSplit> unmatchedSplits,
				int? paymentBranch)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
				where TExpenseAlternateAcctDefault : IBqlField
				where TExpenseAlternateSubMask : IBqlField
			{
				ExpenseDetailSplitType flags = ExpenseDetailSplitType.LaborItem;
				flags = CheckForAlternateAccountSubSource<TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(detailViewCache.Graph, flags);
				IEnumerable<ExpenseDetailSplitGrouping> earningGroups = earnings.SplitExpenseDetails(flags);
				DistributeNegativeSplits(earningGroups, applicableAmountsPerEarning);
				foreach (ExpenseDetailSplitGrouping earningGroup in earningGroups)
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(
						codeID,
						applicableAmountsPerEarning,
						earningGroup,
						paymentBranch,
						unmatchedSplits,
						true,
						x => (x as UnmatchedBenefitSplit)?.LaborItemID == earningGroup.Key.LaborItemID);
					if (newDetail != null)
					{
						newDetail.LabourItemID = earningGroup.Key.LaborItemID;
						newDetail.EarningTypeCD = earningGroup.Key.EarningTypeCD;
						detailViewCache.Update(newDetail);
					}
				}

				foreach (IGrouping<int?, UnmatchedBenefitSplit> splitGroup in unmatchedSplits.Where(x => !x.Handled).GroupBy(x => x.LaborItemID))
				{
					var newDetail = new TDetail();
					newDetail.ParentKeyID = codeID;
					newDetail.Amount = splitGroup.Sum(x => x.Amount);
					newDetail.LabourItemID = splitGroup.Key;
					if (newDetail.Amount != 0)
					{
						detailViewCache.Update(newDetail);
					}
				}
			}

			public virtual void CreateDetailSplitByBranch<TDetail, TKey>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				UnmatchedSplit unmatched,
				int? paymentBranch)
				where TDetail : class, IPaycheckDetail<TKey>, new()
			{
				IEnumerable<UnmatchedSplit> unmatchedEnumerable = unmatched == null ? new UnmatchedSplit[0] : new[] { unmatched };
				foreach (ExpenseDetailSplitGrouping earningGroup in earnings.SplitExpenseDetails(ExpenseDetailSplitType.Branch))
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(
						codeID,
						applicableAmountsPerEarning,
						earningGroup,
						paymentBranch,
						unmatchedEnumerable,
						true,
						x => true);
					if (newDetail != null)
					{
						detailViewCache.Update(newDetail);
					}
				}

				CreateDetailForUnmatchedSplits<TDetail, TKey>(detailViewCache, codeID, unmatchedEnumerable);
			}

			public virtual void CreateDetailSplitByProjectTask<TDetail, TKey, TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				UnmatchedSplit unmatched,
				int? paymentBranch)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
				where TExpenseAlternateAcctDefault : IBqlField
				where TExpenseAlternateSubMask : IBqlField
			{
				IEnumerable<UnmatchedSplit> unmatchedEnumerable = unmatched == null ? new UnmatchedSplit[0] : new[] { unmatched };
				ExpenseDetailSplitType flags = ExpenseDetailSplitType.ProjectTask;
				flags = CheckForAlternateAccountSubSource<TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(detailViewCache.Graph, flags);
				foreach (ExpenseDetailSplitGrouping earningGroup in earnings.SplitExpenseDetails(flags))
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(
						codeID,
						applicableAmountsPerEarning,
						earningGroup,
						paymentBranch,
						unmatchedEnumerable,
						(earningGroup.Key.ProjectID == null || ProjectDefaultAttribute.IsNonProject(earningGroup.Key.ProjectID))
							&& earningGroup.Key.ProjectTaskID == null,
						x => true);
					if (newDetail != null)
					{
						newDetail.ProjectID = earningGroup.Key.ProjectID;
						newDetail.ProjectTaskID = earningGroup.Key.ProjectTaskID;
						newDetail.CostCodeID = earningGroup.Key.CostCodeID;
						newDetail.EarningTypeCD = earningGroup.Key.EarningTypeCD;
						detailViewCache.Update(newDetail);
					}
				}

				CreateDetailForUnmatchedSplits<TDetail, TKey>(detailViewCache, codeID, unmatchedEnumerable);
			}

			public virtual void CreateDetailSplitByProjectTaskAndEarningType<TDetail, TKey>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				UnmatchedSplit unmatched)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
			{
				IEnumerable<UnmatchedSplit> unmatchedEnumerable = unmatched == null ? new UnmatchedSplit[0] : new[] { unmatched };
				foreach (ExpenseDetailSplitGrouping earningGroup in earnings.SplitExpenseDetails(ExpenseDetailSplitType.ProjectTask | ExpenseDetailSplitType.EarningType))
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(codeID, applicableAmountsPerEarning, earningGroup);
					if (newDetail != null)
					{
						newDetail.ProjectID = earningGroup.Key.ProjectID;
						newDetail.ProjectTaskID = earningGroup.Key.ProjectTaskID;
						newDetail.EarningTypeCD = earningGroup.Key.EarningTypeCD;
						newDetail.CostCodeID = earningGroup.Key.CostCodeID;
						detailViewCache.Update(newDetail);
					}
				}

				CreateDetailForUnmatchedSplits<TDetail, TKey>(detailViewCache, codeID, unmatchedEnumerable);
			}

			public virtual void CreateDetailSplitByProjectTaskAndLaborItem<TDetail, TKey, TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				UnmatchedSplit unmatched,
				int? paymentBranch)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
				where TExpenseAlternateAcctDefault : IBqlField
				where TExpenseAlternateSubMask : IBqlField
			{
				IEnumerable<UnmatchedSplit> unmatchedEnumerable = unmatched == null ? new UnmatchedSplit[0] : new[] { unmatched };
				ExpenseDetailSplitType flags = ExpenseDetailSplitType.ProjectTask | ExpenseDetailSplitType.LaborItem;
				flags = CheckForAlternateAccountSubSource<TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(detailViewCache.Graph, flags);
				foreach (ExpenseDetailSplitGrouping earningGroup in earnings.SplitExpenseDetails(flags))
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(
						codeID,
						applicableAmountsPerEarning,
						earningGroup,
						paymentBranch,
						unmatchedEnumerable,
						(earningGroup.Key.ProjectID == null || ProjectDefaultAttribute.IsNonProject(earningGroup.Key.ProjectID))
							&& earningGroup.Key.ProjectTaskID == null && earningGroup.Key.LaborItemID == null,
						x => true);
					if (newDetail != null)
					{
						newDetail.ProjectID = earningGroup.Key.ProjectID;
						newDetail.ProjectTaskID = earningGroup.Key.ProjectTaskID;
						newDetail.LabourItemID = earningGroup.Key.LaborItemID;
						newDetail.CostCodeID = earningGroup.Key.CostCodeID;
						newDetail.EarningTypeCD = earningGroup.Key.EarningTypeCD;
						detailViewCache.Update(newDetail);
					}
				}

				CreateDetailForUnmatchedSplits<TDetail, TKey>(detailViewCache, codeID, unmatchedEnumerable);
			}

			public virtual void CreateDetailSplitByEarningType<TDetail, TKey>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				UnmatchedSplit unmatched)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
			{
				IEnumerable<UnmatchedSplit> unmatchedEnumerable = unmatched == null ? new UnmatchedSplit[0] : new[] { unmatched };
				foreach (ExpenseDetailSplitGrouping earningGroup in earnings.SplitExpenseDetails(ExpenseDetailSplitType.EarningType))
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(codeID, applicableAmountsPerEarning, earningGroup);
					if (newDetail != null)
					{
						newDetail.EarningTypeCD = earningGroup.Key.EarningTypeCD;
						detailViewCache.Update(newDetail);
					}
				}

				CreateDetailForUnmatchedSplits<TDetail, TKey>(detailViewCache, codeID, unmatchedEnumerable);
			}

			public virtual void CreateDetailSplitByLaborItem<TDetail, TKey, TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(
				PXCache detailViewCache,
				TKey codeID,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				List<PREarningDetail> earnings,
				UnmatchedSplit unmatched,
				int? paymentBranch)
				where TDetail : class, IPaycheckExpenseDetail<TKey>, new()
				where TExpenseAlternateAcctDefault : IBqlField
				where TExpenseAlternateSubMask : IBqlField
			{
				IEnumerable<UnmatchedSplit> unmatchedEnumerable = unmatched == null ? new UnmatchedSplit[0] : new[] { unmatched };
				ExpenseDetailSplitType flags = ExpenseDetailSplitType.LaborItem;
				flags = CheckForAlternateAccountSubSource<TExpenseAlternateAcctDefault, TExpenseAlternateSubMask>(detailViewCache.Graph, flags);
				foreach (ExpenseDetailSplitGrouping earningGroup in earnings.SplitExpenseDetails(flags))
				{
					TDetail newDetail = CreateDetailSplit<TDetail, TKey>(
						codeID,
						applicableAmountsPerEarning,
						earningGroup,
						paymentBranch,
						unmatchedEnumerable,
						earningGroup.Key.LaborItemID == null,
						x => true);
					if (newDetail != null)
					{
						newDetail.LabourItemID = earningGroup.Key.LaborItemID;
						newDetail.EarningTypeCD = earningGroup.Key.EarningTypeCD;
						detailViewCache.Update(newDetail);
					}
				}

				CreateDetailForUnmatchedSplits<TDetail, TKey>(detailViewCache, codeID, unmatchedEnumerable);
			}

			protected virtual TDetail CreateDetailSplit<TDetail, TKey>(
				TKey id,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				ExpenseDetailSplitGrouping earningGroup,
				int? paymentBranch,
				IEnumerable<UnmatchedSplit> unmatchedSplits,
				bool applyUnmatchedSplits,
				Func<UnmatchedSplit, bool> unmatchedFilter)
				where TDetail : class, IPaycheckDetail<TKey>, new()
			{
				decimal? applicableAmount = earningGroup.Sum(x => GetApplicableAmountAsDecimal(x, applicableAmountsPerEarning));
				if (applyUnmatchedSplits && earningGroup.Key.BranchID == paymentBranch)
				{
					applicableAmount += unmatchedSplits.Where(unmatchedFilter).Where(x => !x.Handled).Sum(x =>
					{
						x.Handled = true;
						return x.Amount;
					});
				}

				if (applicableAmount != 0)
				{
					TDetail newDetail = new TDetail();
					newDetail.ParentKeyID = id;
					newDetail.BranchID = earningGroup.Key.BranchID;
					newDetail.Amount = applicableAmount;
					return newDetail;
				}
				return null;
			}

			protected virtual TDetail CreateDetailSplit<TDetail, TKey>(
				TKey id,
				Dictionary<int?, decimal?> applicableAmountsPerEarning,
				ExpenseDetailSplitGrouping earningGroup)
				where TDetail : class, IPaycheckDetail<TKey>, new()
			{
				return CreateDetailSplit<TDetail, TKey>(
					id,
					applicableAmountsPerEarning,
					earningGroup,
					null,
					null,
					false,
					x => false);
			}

			protected virtual void CreateDetailForUnmatchedSplits<TDetail, TKey>(PXCache detailViewCache, TKey id, IEnumerable<UnmatchedSplit> unmatchedSplits)
				where TDetail : class, IPaycheckDetail<TKey>, new()
			{
				var unmatchedDetail = new TDetail();
				unmatchedDetail.ParentKeyID = id;
				unmatchedDetail.Amount = unmatchedSplits.Where(x => !x.Handled).Sum(x => x.Amount);
				if (unmatchedDetail.Amount != 0)
				{
					detailViewCache.Update(unmatchedDetail);
				}
			}

			protected virtual void DistributeNegativeSplits(IEnumerable<IEnumerable<PREarningDetail>> earningSplits, Dictionary<int?, decimal?> applicableAmountsPerEarning)
			{
				var positiveEarnings = new List<IEnumerable<PREarningDetail>>();
				var negativeEarnings = new List<IEnumerable<PREarningDetail>>();

				foreach (IEnumerable<PREarningDetail> earningSplit in earningSplits)
				{
					decimal applicableAmount = earningSplit.Sum(x => GetApplicableAmountAsDecimal(x, applicableAmountsPerEarning));

					if (applicableAmount < 0)
					{
						negativeEarnings.Add(earningSplit);
					}
					else if (applicableAmount > 0)
					{
						positiveEarnings.Add(earningSplit);
					}
				}

				decimal totalPositiveApplicableAmounts = positiveEarnings.Sum(x => x.Sum(y => GetApplicableAmountAsDecimal(y, applicableAmountsPerEarning)));
				decimal totalNegativeApplicableAmounts = negativeEarnings.Sum(x => x.Sum(y => GetApplicableAmountAsDecimal(y, applicableAmountsPerEarning)));

				if (totalPositiveApplicableAmounts == 0 || totalNegativeApplicableAmounts == 0)
				{
					return;
				}

				// Add applicable amounts from negative splits to applicable amounts from positive splits by pro-rating from total applicable amounts
				foreach (PREarningDetail positiveApplicableEarning in positiveEarnings.SelectMany(x => x))
				{
					int? key = positiveApplicableEarning.RecordID;
					if (applicableAmountsPerEarning.ContainsKey(key))
					{
						applicableAmountsPerEarning[key] += totalNegativeApplicableAmounts * applicableAmountsPerEarning[key] / totalPositiveApplicableAmounts;
					}
				}

				// Set applicable amounts from negative splits to 0
				foreach (PREarningDetail negativeApplicableEarning in negativeEarnings.SelectMany(x => x))
				{
					int? key = negativeApplicableEarning.RecordID;
					if (applicableAmountsPerEarning.ContainsKey(key))
					{
						applicableAmountsPerEarning[key] = 0;
					}
				}

				HandleRounding(applicableAmountsPerEarning, totalPositiveApplicableAmounts + totalNegativeApplicableAmounts);

				// Recurse to handle any splits that were porentially made negative by adjustment
				DistributeNegativeSplits(earningSplits, applicableAmountsPerEarning);
			}

			protected virtual decimal GetApplicableAmountAsDecimal(PREarningDetail earning, Dictionary<int?, decimal?> applicableAmountsPerEarning)
			{
				if (applicableAmountsPerEarning.TryGetValue(earning.RecordID, out decimal? amount))
				{
					return amount.GetValueOrDefault();
				}
				return 0m;
			}

			public virtual void HandleRounding<TKey>(Dictionary<TKey, decimal?> amounts, decimal? leftoverAmount)
			{
				amounts.ToList().ForEach(kvp => amounts[kvp.Key] = Math.Round(kvp.Value.GetValueOrDefault(), 2, MidpointRounding.AwayFromZero));
				decimal? assignedAmount = amounts.Sum(x => x.Value);
				decimal roundedLeftoverAmount = Math.Round(leftoverAmount.GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);
				if (roundedLeftoverAmount != assignedAmount && amounts.Count > 0)
				{
					amounts[amounts.OrderByDescending(x => x.Value).First().Key] += roundedLeftoverAmount - assignedAmount;
				}
			}

			protected virtual bool IsEarningApplicableToProjectDeduction(PXGraph graph, PREarningDetail earningDetail, PRDeductionAndBenefitProjectPackage package)
			{
				var packageQuery = new SelectFrom<PRDeductionAndBenefitProjectPackage>
					.Where<PRDeductionAndBenefitProjectPackage.projectID.IsEqual<P.AsInt>
						.And<PRDeductionAndBenefitProjectPackage.laborItemID.IsNotNull>
						.And<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.IsEqual<P.AsInt>>
						.And<PRDeductionAndBenefitProjectPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>.View(graph);
				return earningDetail.ProjectID == package.ProjectID &&
					(earningDetail.LabourItemID == package.LaborItemID ||
					(package.LaborItemID == null && !packageQuery.Select(earningDetail.ProjectID, package.DeductionAndBenefitCodeID)
						.Any(x => ((PRDeductionAndBenefitProjectPackage)x).LaborItemID == earningDetail.LabourItemID)));
			}

			protected virtual bool IsEarningApplicableToUnionDeduction(PXGraph graph, PREarningDetail earningDetail, PRDeductionAndBenefitUnionPackage package)
			{
				var packageQuery = new SelectFrom<PRDeductionAndBenefitUnionPackage>
					.Where<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<P.AsString>
						.And<PRDeductionAndBenefitUnionPackage.laborItemID.IsNotNull>
						.And<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<P.AsInt>>
						.And<PRDeductionAndBenefitUnionPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>.View(graph);
				return earningDetail.UnionID == package.UnionID &&
					(earningDetail.LabourItemID == package.LaborItemID ||
					(package.LaborItemID == null && !packageQuery.Select(earningDetail.UnionID, package.DeductionAndBenefitCodeID)
						.Any(x => ((PRDeductionAndBenefitUnionPackage)x).LaborItemID == earningDetail.LabourItemID)));
			}

			public virtual bool IsDeductCodeApplicableToEarning(PXGraph graph, PRDeductCode deductCode, string contributionType, PREarningDetail earning)
			{
				PXCache cache = graph.Caches[typeof(PRDeductCode)];

				string calcType = GetCalcType(cache, contributionType, deductCode);
				string applicableEarnings = GetApplicableEarningSetting(cache, deductCode, contributionType);
				if (calcType != DedCntCalculationMethod.PercentOfCustom && applicableEarnings != DedBenApplicableEarningsAttribute.TotalEarnings &&
					applicableEarnings != DedBenApplicableEarningsAttribute.TotalEarningsWithOTMult)
				{
					string earningTypeCategory = graph.Caches[typeof(EPEarningType)].GetValueExt<PREarningType.earningTypeCategory>(
						PXSelectorAttribute.Select<PREarningDetail.typeCD>(graph.Caches[typeof(PREarningDetail)], earning)) as string;
					switch (applicableEarnings)
					{
						case DedBenApplicableEarningsAttribute.RegularEarnings:
							if (earningTypeCategory != EarningTypeCategory.Wage)
							{
								return false;
							}
							break;
						case DedBenApplicableEarningsAttribute.RegularAndOTEarnings:
						case DedBenApplicableEarningsAttribute.RegularAndOTEarningsWithOTMult:
						case DedBenApplicableEarningsAttribute.StraightTimeEarnings:
							if (earningTypeCategory != EarningTypeCategory.Wage && earningTypeCategory != EarningTypeCategory.Overtime)
							{
								return false;
							}
							break;
						case DedBenApplicableEarningsAttribute.StraightTimeEarningsWithPTO:
						case DedBenApplicableEarningsAttribute.RegularAndOTEarningsWithPTO:
							if (earningTypeCategory != EarningTypeCategory.Wage && earningTypeCategory != EarningTypeCategory.Overtime 
								&& earningTypeCategory != EarningTypeCategory.TimeOff)
							{
								return false;
							}
							break;
					}
				}

				if (calcType != DedCntCalculationMethod.PercentOfCustom)
				{
					return true;
				}
				if (deductCode.EarningsIncreasingWageIncludeType == SubjectToTaxes.All)
				{
					return true;
				}
				if (deductCode.EarningsIncreasingWageIncludeType == SubjectToTaxes.None)
				{
					return false;
				}

				bool isEarningTypeSpecified = new SelectFrom<PRDeductCodeEarningIncreasingWage>
					.Where<PRDeductCodeEarningIncreasingWage.deductCodeID.IsEqual<P.AsInt>
						.And<PRDeductCodeEarningIncreasingWage.applicableTypeCD.IsEqual<P.AsString>>>.View(graph).Select(deductCode.CodeID, earning.TypeCD).Any();
				return isEarningTypeSpecified ^ deductCode.EarningsIncreasingWageIncludeType == SubjectToTaxes.AllButList;
			}

			public virtual decimal GetDedBenApplicableAmount(
				PXGraph graph,
				PRDeductCode deductCode,
				string contributionType,
				IEnumerable<PREarningDetail> earnings,
				Dictionary<int?, TaxEarningDetailsSplits> taxesSplitByEarning,
				Dictionary<int?, DedBenEarningDetailsSplits> dedBenSplitByEarning)
			{
				PXCache cache = graph.Caches[typeof(PRDeductCode)];
				string calcType = GetCalcType(cache, contributionType, deductCode);
				PRCalculationEngine calculationEngine = graph as PRCalculationEngine;
				decimal amountFromEarnings = 0;
				if (calcType == DedCntCalculationMethod.PercentOfNet)
				{
					if (calculationEngine != null)
					{
						if (contributionType == ContributionType.EmployeeDeduction && deductCode.IsGarnishment == true)
						{
							amountFromEarnings = calculationEngine.PaymentsToProcess[calculationEngine.Payments.Current].NetIncomeForGarnishmentCalc;
						}
						else
						{
							amountFromEarnings = calculationEngine.PaymentsToProcess[calculationEngine.Payments.Current].NetIncomeAccumulator;
						}
					}
				}
				else
				{
					amountFromEarnings = earnings.Sum(x => GetDedBenApplicableAmount(graph, deductCode, contributionType, x, taxesSplitByEarning, dedBenSplitByEarning));
				}
				
				decimal amountFromPayableBenefits = 0;
				if (calculationEngine != null && deductCode.DedCalcType != DedCntCalculationMethod.PercentOfCustom)
				{
					amountFromPayableBenefits = calculationEngine.PaymentsToProcess[calculationEngine.Payments.Current].PayableBenefitContributingAmount;
				}
				return amountFromEarnings + amountFromPayableBenefits;
			}

			public virtual decimal GetDedBenApplicableAmount(
				PXGraph graph,
				PRDeductCode deductCode,
				string contributionType,
				PREarningDetail earning,
				Dictionary<int?, TaxEarningDetailsSplits> taxesSplitByEarning,
				Dictionary<int?, DedBenEarningDetailsSplits> dedBenSplitByEarning)
			{
				decimal earningPortion = IsDeductCodeApplicableToEarning(graph, deductCode, contributionType, earning) ? earning.Amount.GetValueOrDefault() : 0;

				PXCache cache = graph.Caches[typeof(PRDeductCode)];
				string calcType = GetCalcType(cache, contributionType, deductCode);
				string applicableEarningSetting = GetApplicableEarningSetting(cache, deductCode, contributionType);
				if (earningPortion != 0
					&& (applicableEarningSetting == DedBenApplicableEarningsAttribute.StraightTimeEarnings || applicableEarningSetting == DedBenApplicableEarningsAttribute.StraightTimeEarningsWithPTO))
				{
					bool includeTimeOff = applicableEarningSetting == DedBenApplicableEarningsAttribute.StraightTimeEarningsWithPTO;
					earningPortion = GetStraightTimeEarningAmount(graph, earning, calcType, includeTimeOff);
				}

				if (calcType != DedCntCalculationMethod.PercentOfCustom)
				{
					return earningPortion;
				}

				decimal totalApplicableWage = earningPortion;
				List<int?> benefitsIncreasingWage = GetApplicableWageList<PRDeductCode, PRDeductCode.codeID, PRDeductCodeBenefitIncreasingWage.applicableBenefitCodeID>(
					graph,
					typeof(PRDeductCodeBenefitIncreasingWage.deductCodeID),
					deductCode.BenefitsIncreasingWageIncludeType,
					deductCode.CodeID)
					.Select(x => x.CodeID).ToList();
				List<int?> taxesIncreasingWage = GetApplicableWageList<PRTaxCode, PRTaxCode.taxID, PRDeductCodeTaxIncreasingWage.applicableTaxID>(
					graph,
					typeof(PRDeductCodeTaxIncreasingWage.deductCodeID),
					deductCode.TaxesIncreasingWageIncludeType,
					deductCode.CodeID)
					.Where(x => x.TaxCategory == TaxCategory.EmployerTax).Select(x => x.TaxID).ToList();
				List<int?> deductionsDecreasingWage = GetApplicableWageList<PRDeductCode, PRDeductCode.codeID, PRDeductCodeDeductionDecreasingWage.applicableDeductionCodeID>(
					graph,
					typeof(PRDeductCodeDeductionDecreasingWage.deductCodeID),
					deductCode.DeductionsDecreasingWageIncludeType,
					deductCode.CodeID)
					.Select(x => x.CodeID).ToList();
				List<int?> taxesDecreasingWage = GetApplicableWageList<PRTaxCode, PRTaxCode.taxID, PRDeductCodeTaxDecreasingWage.applicableTaxID>(
					graph,
					typeof(PRDeductCodeTaxDecreasingWage.deductCodeID),
					deductCode.TaxesDecreasingWageIncludeType,
					deductCode.CodeID)
					.Where(x => x.TaxCategory == TaxCategory.EmployeeWithholding).Select(x => x.TaxID).ToList();

				totalApplicableWage += dedBenSplitByEarning.Where(kvp => benefitsIncreasingWage.Contains(kvp.Key) && kvp.Value.ContainsKey(earning.RecordID))
					.Sum(kvp => kvp.Value[earning.RecordID].BenefitAmount.GetValueOrDefault());
				totalApplicableWage += taxesSplitByEarning.Where(kvp => taxesIncreasingWage.Contains(kvp.Key) && kvp.Value.ContainsKey(earning.RecordID))
					.Sum(kvp => kvp.Value[earning.RecordID].GetValueOrDefault());
				totalApplicableWage -= dedBenSplitByEarning.Where(kvp => deductionsDecreasingWage.Contains(kvp.Key) && kvp.Value.ContainsKey(earning.RecordID))
					.Sum(kvp => kvp.Value[earning.RecordID].DeductionAmount.GetValueOrDefault());
				totalApplicableWage -= taxesSplitByEarning.Where(kvp => taxesDecreasingWage.Contains(kvp.Key) && kvp.Value.ContainsKey(earning.RecordID))
					.Sum(kvp => kvp.Value[earning.RecordID].GetValueOrDefault());

				return totalApplicableWage;
			}

			public virtual decimal GetDedBenApplicableHours(PXGraph graph, PRDeductCode deductCode, string contributionType, IEnumerable<PREarningDetail> earnings)
			{
				return earnings.Sum(x => GetDedBenApplicableHours(graph, deductCode, contributionType, x));
			}

			public virtual decimal GetDedBenApplicableHours(PXGraph graph, PRDeductCode deductCode, string contributionType, PREarningDetail earning)
			{
				return IsDeductCodeApplicableToEarning(graph, deductCode, contributionType, earning) ? earning.Hours.GetValueOrDefault() : 0;
			}

			protected virtual string GetApplicableEarningSetting(PXCache cache, PRDeductCode deductCode, string contributionType)
			{
				string calcType = GetCalcType(cache, contributionType, deductCode);
				if (calcType == DedCntCalculationMethod.PercentOfGross || calcType == DedCntCalculationMethod.AmountPerHour || calcType == DedCntCalculationMethod.PercentOfCustom)
				{
					string applicableEarningsField = contributionType == ContributionType.EmployeeDeduction ? nameof(PRDeductCode.DedApplicableEarnings) : nameof(PRDeductCode.CntApplicableEarnings);
					return cache.GetValue(deductCode, applicableEarningsField) as string;
				}

				return DedBenApplicableEarningsAttribute.TotalEarnings;
			}

			protected virtual decimal GetStraightTimeEarningAmount(PXGraph graph, PREarningDetail earning, string calculationType, bool includeTimeOff)
			{
				EPEarningType epEarningType = PXSelectorAttribute.Select<PREarningDetail.typeCD>(graph.Caches[typeof(PREarningDetail)], earning) as EPEarningType;
				string earningTypeCategory = graph.Caches[typeof(EPEarningType)].GetValueExt<PREarningType.earningTypeCategory>(epEarningType) as string;
				switch (earningTypeCategory)
				{
					case EarningTypeCategory.Wage:
						return earning.Amount.GetValueOrDefault();
					case EarningTypeCategory.Overtime:
						return epEarningType.OvertimeMultiplier != 0 ? (earning.Amount / epEarningType.OvertimeMultiplier).GetValueOrDefault() : 0;
					case EarningTypeCategory.TimeOff:
						return calculationType == DedCntCalculationMethod.PercentOfCustom || includeTimeOff ? earning.Amount.GetValueOrDefault() : 0;
					default:
						return calculationType == DedCntCalculationMethod.PercentOfCustom ? earning.Amount.GetValueOrDefault() : 0;
				}
			}

			protected virtual List<TTable> GetApplicableWageList<TTable, TRefIDField, TReferenceField>(
				PXGraph graph,
				Type applicableWageTableDeductCodeField,
				string inclusionType,
				int? deductCodeID)
				where TTable : class, IBqlTable, new()
				where TRefIDField : BqlInt.Field<TRefIDField>
				where TReferenceField : IBqlField
			{
				if (inclusionType == SubjectToTaxes.None)
				{
					return new List<TTable>();
				}

				BqlCommand command = null;
				switch (inclusionType)
				{
					case SubjectToTaxes.All:
						command = BqlCommand.CreateInstance(typeof(Select<TTable>));
						break;
					case SubjectToTaxes.AllButList:
						command = BqlTemplate.OfCommand<SelectFrom<TTable>
							.Where<BqlPlaceholder.A.AsField.IsNotInSubselect<SearchFor<TReferenceField>
								.Where<BqlPlaceholder.B.AsField.IsEqual<TRefIDField>
									.And<BqlPlaceholder.C.AsField.IsEqual<P.AsInt>>>>>>
							.Replace<BqlPlaceholder.A>(typeof(TRefIDField))
							.Replace<BqlPlaceholder.B>(typeof(TReferenceField))
							.Replace<BqlPlaceholder.C>(applicableWageTableDeductCodeField)
							.ToCommand();
						break;
					case SubjectToTaxes.NoneButList:
						command = BqlTemplate.OfCommand<SelectFrom<TTable>
							.Where<BqlPlaceholder.A.AsField.IsInSubselect<SearchFor<TReferenceField>
								.Where<BqlPlaceholder.B.AsField.IsEqual<TRefIDField>
									.And<BqlPlaceholder.C.AsField.IsEqual<P.AsInt>>>>>>
							.Replace<BqlPlaceholder.A>(typeof(TRefIDField))
							.Replace<BqlPlaceholder.B>(typeof(TReferenceField))
							.Replace<BqlPlaceholder.C>(applicableWageTableDeductCodeField)
							.ToCommand();
						break;
				}

				PXView query = new PXView(graph, false, command);
				return query.SelectMulti(deductCodeID).Select(x => (TTable)x).ToList();
			}

			private string GetCalcType(PXCache cache, string contributionType, PRDeductCode deductCode)
			{
				string calculationMethodField = contributionType == ContributionType.EmployeeDeduction ? nameof(PRDeductCode.DedCalcType) : nameof(PRDeductCode.CntCalcType);
				return cache.GetValue(deductCode, calculationMethodField) as string;
			}
			#endregion Helper methods

			#region Helper classes
			public class UnmatchedSplit
			{
				public decimal Amount { get; set; } = 0m;
				public bool Handled { get; set; } = false;
			}

			protected class UnmatchedBenefitSplit : UnmatchedSplit, IEquatable<UnmatchedBenefitSplit>
			{
				public int? ProjectID { get; private set; }
				public int? LaborItemID { get; private set; }

				public UnmatchedBenefitSplit(int? projectID, int? laborItemID)
				{
					ProjectID = projectID;
					LaborItemID = laborItemID;
				}

				public bool Equals(UnmatchedBenefitSplit other)
				{
					return other.ProjectID == ProjectID && other.LaborItemID == LaborItemID;
				}
			}
			#endregion Helper classes

			#region Obsolete 2020R2
			[Obsolete]
			protected virtual decimal GetStraightTimeEarningAmount(PXGraph graph, PREarningDetail earning)
			{
				return GetStraightTimeEarningAmount(graph, earning, DedCntCalculationMethod.PercentOfGross, false);
			}

			[Obsolete]
			protected virtual HashSet<UnmatchedBenefitSplit> SplitUnmatchedDedBenAmounts(
				PXGraph graph,
				string contributionType,
				List<PRPaymentProjectPackageDeduct> unmatchedProjectPackages,
				List<PRPaymentFringeBenefit> unmatchedFringeAmountsInBenefitCode,
				List<PRPaymentUnionPackageDeduct> unmatchedUnionPackages,
				List<PRPaymentWCPremium> unmatchedWCPackages)
			{
				return SplitUnmatchedDedBenAmounts(graph, contributionType, null, unmatchedProjectPackages, unmatchedFringeAmountsInBenefitCode, unmatchedUnionPackages, unmatchedWCPackages);
			}
			#endregion
		}
	}

	public struct DetailSplitType
	{
		public bool SplitByProjectTask;
		public bool SplitByEarningType;
		public bool SplitByLaborItem;
	}
}
