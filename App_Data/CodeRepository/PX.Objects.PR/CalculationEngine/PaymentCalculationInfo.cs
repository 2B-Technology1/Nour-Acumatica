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

using PX.Api;
using PX.Data;
using PX.Objects.PR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		// For these classes, Key is PREarningDetail.RecordID
		public class TaxEarningDetailsSplits : Dictionary<int?, decimal?> { }
		public class DedBenEarningDetailsSplits : Dictionary<int?, DedBenAmount> { }
		public class PTOAccrualSplits : Dictionary<int?, decimal?> { }

		public class PaymentCalculationInfo
		{
			public PaymentCalculationInfo(PRPayment payment)
			{
				Payment = payment;
			}

			public PRPayment Payment { get; set; }

			public decimal NetIncomeAccumulator
			{
				get
				{
					return GrossWage - TaxAmount - WCDeductionAmount - NonWCDeductionAmount;
				}
				set
				{
					// No-op, left to avoid breaking changes in 2020R2
				}
			}
			public decimal NetIncomeForGarnishmentCalc => GrossWage - TaxAmount - WCDeductionAmount;

			public decimal GrossWage { get; set; } = 0m;
			public decimal TaxAmount { get; set; } = 0m;
			public decimal WCDeductionAmount { get; set; } = 0m;
			public decimal NonWCDeductionAmount { get; set; } = 0m;

			public decimal PayableBenefitContributingAmount { get; set; } = 0m;

			public Dictionary<int?, DedBenAmount> NominalTaxableDedBenAmounts { get; set; } = new Dictionary<int?, DedBenAmount>();

			public Dictionary<ProjectDedBenPackageKey, PackageDedBenCalculation> NominalProjectPackageAmounts { get; set; } 
				= new Dictionary<ProjectDedBenPackageKey, PackageDedBenCalculation>();

			public Dictionary<UnionDedBenPackageKey, PackageDedBenCalculation> NominalUnionPackageAmounts { get; set; }
				= new Dictionary<UnionDedBenPackageKey, PackageDedBenCalculation>();

			public Dictionary<FringeBenefitDecreasingRateKey, PRPaymentFringeBenefitDecreasingRate> FringeRateReducingBenefits { get; set; }
				= new Dictionary<FringeBenefitDecreasingRateKey, PRPaymentFringeBenefitDecreasingRate>();

			public Dictionary<FringeEarningDecreasingRateKey, PRPaymentFringeEarningDecreasingRate> FringeRateReducingEarnings { get; set; }
				= new Dictionary<FringeEarningDecreasingRateKey, PRPaymentFringeEarningDecreasingRate>();

			public List<FringeSourceEarning> FringeRates { get; set; } = new List<FringeSourceEarning>();

			public Dictionary<int?, FringeAmountInfo> FringeAmountsPerProject { get; set; } = new Dictionary<int?, FringeAmountInfo>();

			// Key is TaxID
			public Dictionary<int?, TaxEarningDetailsSplits> TaxesSplitByEarning { get; set; } = new Dictionary<int?, TaxEarningDetailsSplits>();

			// Key is CodeID
			public Dictionary<int?, DedBenEarningDetailsSplits> TaxableDeductionsAndBenefitsSplitByEarning { get; set; } = new Dictionary<int?, DedBenEarningDetailsSplits>();

			// Key is BankID
			public Dictionary<string, PTOAccrualSplits> PTOAccrualMoneySplitByEarning { get; set; } = new Dictionary<string, PTOAccrualSplits>();
		}

		public class PaymentCalculationInfoCollection : IEnumerable<PaymentCalculationInfo>
		{
			private Dictionary<string, PaymentCalculationInfo> _PaymentInfoList = new Dictionary<string, PaymentCalculationInfo>();
			private PXGraph _Graph;

			public PaymentCalculationInfoCollection(PXGraph graph)
			{
				_Graph = graph;
			}

			public PaymentCalculationInfo this[PRPayment key]
			{
				get
				{
					return this[key.PaymentDocAndRef];
				}
			}

			public PaymentCalculationInfo this[string paymentRef]
			{
				get
				{
					try
					{
						return _PaymentInfoList[paymentRef];
					}
					catch (KeyNotFoundException)
					{
						throw new PXException(Messages.CalculationEngineError, paymentRef);
					}
				}
			}

			public void Add(PRPayment key)
			{
				_PaymentInfoList[key.PaymentDocAndRef] = new PaymentCalculationInfo(key);
			}

			public IEnumerable<PRPayment> GetAllPayments()
			{
				return _PaymentInfoList.Values.Select(x => x.Payment);
			}

			public void UpdatePayment(PRPayment payment)
			{
				_Graph.Caches[typeof(PRPayment)].Update(payment);
				this[payment].Payment = payment;
			}

			public IEnumerator<PaymentCalculationInfo> GetEnumerator()
			{
				return _PaymentInfoList.Values.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _PaymentInfoList.Values.GetEnumerator();
			}
		}
	}
}
