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
using PX.SM;

namespace PX.Objects.CR
{
	public class AssignBAccountMassProcess : CRBaseAssignProcess<AssignBAccountMassProcess, BAccount, CRSetup.defaultBAccountAssignmentMapID>
	{
		[PXViewName(Messages.MatchingRecords)]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<BAccountParent,
				Where<BAccountParent.bAccountID, Equal<Current<BAccount.parentBAccountID>>>>))]
		[PXFilterable]
		public PXProcessingJoin<BAccount, 
			LeftJoin<Contact,
				On<Contact.bAccountID, Equal<BAccount.bAccountID>,
				And<Contact.contactID, Equal<BAccount.defContactID>>>,
			LeftJoin<Address,
				On<Address.bAccountID, Equal<BAccount.bAccountID>,
				And<Address.addressID, Equal<BAccount.defAddressID>>>,
			LeftJoin<BAccountParent,
				On<BAccountParent.bAccountID, Equal<BAccount.parentBAccountID>>,
			LeftJoin<Location,
				On<Location.bAccountID, Equal<BAccount.bAccountID>, 
				And<Location.locationID, Equal<BAccount.defLocationID>>>,
			LeftJoin<State,
				On<State.countryID, Equal<Address.countryID>,
				And<State.stateID, Equal<Address.state>>>>>>>>,
			Where2<
				Where<
					BAccount.type, Equal<BAccountType.prospectType>,
					Or<BAccount.type, Equal<BAccountType.customerType>,
					Or<BAccount.type, Equal<BAccountType.combinedType>,
					Or<BAccount.type, Equal<BAccountType.vendorType>>>>>,
				And<Match<Current<AccessInfo.userName>>>>,
			OrderBy<
				Asc<BAccount.acctName>>>
			Items;
	}
}
