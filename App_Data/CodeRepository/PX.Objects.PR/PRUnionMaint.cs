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
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Payroll.Data;
using System.Collections;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRUnionMaint : PXGraph<PRUnionMaint, PMUnion>
	{
		public PXSelect<PMUnion> UnionLocal;

		public SelectFrom<PMLaborCostRate>.
			Where<PMLaborCostRate.unionID.
				IsEqual<PMUnion.unionID.FromCurrent>>.View EarningRates;

		public SelectFrom<PRDeductionAndBenefitUnionPackage>.
			InnerJoin<PRDeductCode>.On<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<PRDeductCode.codeID>>.
			Where<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<PMUnion.unionID.FromCurrent>>.View DeductionsAndBenefitsPackage;

		#region Data view delegates
		public IEnumerable deductionsAndBenefitsPackage()
		{
			PXView bqlSelect = new PXView(this, false, DeductionsAndBenefitsPackage.View.BqlSelect);

			foreach (object objResult in bqlSelect.SelectMulti())
			{
				PXResult<PRDeductionAndBenefitUnionPackage, PRDeductCode> result = objResult as PXResult<PRDeductionAndBenefitUnionPackage, PRDeductCode>;
				if (result != null)
				{
					PRDeductionAndBenefitUnionPackage packageDeduct = (PRDeductionAndBenefitUnionPackage)result;
					PRDeductCode deductCode = (PRDeductCode)result;

					if (deductCode.CountryID != PRCountryAttribute.GetPayrollCountry())
					{
						continue;
					}

					if (packageDeduct.DeductionAndBenefitCodeID != null && deductCode.IsActive != true)
					{
						PXUIFieldAttribute.SetEnabled(DeductionsAndBenefitsPackage.Cache, packageDeduct, false);
						DeductionsAndBenefitsPackage.Cache.RaiseExceptionHandling<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID>(
							packageDeduct,
							packageDeduct.DeductionAndBenefitCodeID,
							new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
					}

					yield return result;
				}
			}
		}
		#endregion Data view delegates

		#region Cache Attached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(Search<PMUnion.unionID>), SelectorMode = PXSelectorMode.DisplayMode)]
		protected virtual void _(Events.CacheAttached<PMUnion.unionID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(PMUnion.unionID))]
		[PXParent(typeof(Select<PMUnion, Where<PMUnion.unionID, Equal<Current<PMLaborCostRate.unionID>>>>))]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.unionID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PMLaborCostRateType.Union)]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.type> e ) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCheckUnique(typeof(PMLaborCostRate.unionID), typeof(PMLaborCostRate.inventoryID), ClearOnDuplicate = false)]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.effectiveDate> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(
			SelectFrom<InventoryItem>.
			Where<InventoryItem.inventoryID.IsEqual<PMLaborCostRate.inventoryID.FromCurrent>>.
			SearchFor<InventoryItem.descr>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<PMLaborCostRate.inventoryID>))]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.description> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXDBPriceCostAttribute), nameof(PXDBPriceCostAttribute.MinValue), 0)]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.wageRate> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Deduction Calculation Method")]
		[DeductionCalcMethodDisplay]
		protected virtual void _(Events.CacheAttached<PRDeductCode.dedCalcType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Contribution Calculation Method")]
		[BenefitCalcMethodDisplay]
		protected virtual void _(Events.CacheAttached<PRDeductCode.cntCalcType> e) { }
		#endregion Cache Attached

		#region Events
		protected virtual void _(Events.RowPersisting<PRDeductionAndBenefitUnionPackage> e)
		{
			PRDeductCode deductCode = PXSelectorAttribute.Select<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID>(e.Cache, e.Row) as PRDeductCode;
			if (deductCode != null)
			{
				if ((deductCode.DedCalcType == DedCntCalculationMethod.FixedAmount || deductCode.DedCalcType == DedCntCalculationMethod.AmountPerHour)
					&& deductCode.ContribType != ContributionType.EmployerContribution && e.Row.DeductionAmount == null)
				{
					e.Row.DeductionAmount = 0m;
				}
				else if ((deductCode.DedCalcType == DedCntCalculationMethod.PercentOfGross || deductCode.DedCalcType == DedCntCalculationMethod.PercentOfCustom)
					&& deductCode.ContribType != ContributionType.EmployerContribution && e.Row.DeductionRate == null)
				{
					e.Row.DeductionRate = 0m;
				}

				if ((deductCode.CntCalcType == DedCntCalculationMethod.FixedAmount || deductCode.CntCalcType == DedCntCalculationMethod.AmountPerHour)
					&& deductCode.ContribType != ContributionType.EmployeeDeduction && e.Row.BenefitAmount == null)
				{
					e.Row.BenefitAmount = 0m;
				}
				else if ((deductCode.CntCalcType == DedCntCalculationMethod.PercentOfGross || deductCode.CntCalcType == DedCntCalculationMethod.PercentOfCustom)
					&& deductCode.ContribType != ContributionType.EmployeeDeduction && e.Row.BenefitRate == null)
				{
					e.Row.BenefitRate = 0m;
				}
			}
		}
		#endregion Events
	}
}
