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
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARCustomerCreditHoldProcessVisibilityRestriction : PXGraphExtension<ARCustomerCreditHoldProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public delegate PXResultset<Customer> GetCustomersToProcessDelegate(ARCustomerCreditHoldProcess.CreditHoldParameters header);

		[PXOverride]
		public PXResultset<Customer> GetCustomersToProcess(ARCustomerCreditHoldProcess.CreditHoldParameters header,
			GetCustomersToProcessDelegate baseMethod)
		{
			switch (header.Action)
			{
				case ARCustomerCreditHoldProcess.CreditHoldParameters.ActionApplyCreditHold:

					return PXSelectJoin<Customer,
						InnerJoin<ARDunningLetter, On<Customer.bAccountID, Equal<ARDunningLetter.bAccountID>,
							And<ARDunningLetter.lastLevel, Equal<True>,
							And<ARDunningLetter.released, Equal<True>,
							And<ARDunningLetter.voided, NotEqual<True>>>>>>,
						Where<ARDunningLetter.dunningLetterDate,
							Between<Required<ARDunningLetter.dunningLetterDate>, Required<ARDunningLetter.dunningLetterDate>>,
							And<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>,
						OrderBy<Asc<ARDunningLetter.bAccountID>>>.Select(Base, header.BeginDate, header.EndDate);

				case ARCustomerCreditHoldProcess.CreditHoldParameters.ActionReleaseCreditHold:

					PXSelectBase<Customer> select = new PXSelectJoin<Customer,
						LeftJoin<ARDunningLetter, On<Customer.bAccountID, Equal<ARDunningLetter.bAccountID>,
							And<ARDunningLetter.lastLevel, Equal<True>,
							And<ARDunningLetter.released, Equal<True>,
							And<ARDunningLetter.voided, NotEqual<True>>>>>>,
						Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>(Base);
					if (PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>())
					{
						select.WhereAnd<Where<Customer.bAccountID, Equal<Customer.sharedCreditCustomerID>>>();
					}
					return select.Select();

				default:
					return new PXResultset<Customer>();
			}
		}
	}
}