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
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	public class UpdateLeadMassProcess : CRBaseWorkflowUpdateProcess<UpdateLeadMassProcess, LeadMaint, CRLead, PXMassUpdatableFieldAttribute, CRLead.classID>
	{

		[PXMassUpdatableField]
		[PXDefault(typeof(Search<CRLeadClass.defaultSource, Where<CRLeadClass.classID, Equal<Current<CRLead.classID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRLead.source> e) { }

		[PXMassUpdatableField]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRLead.campaignID> e) { }

		[PXViewName(Messages.MatchingRecords)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(CRLead))]
		[PXViewDetailsButton(typeof(CRLead),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<CRLead.bAccountID>>>>),
			ActionName = "Items_BAccount_ViewDetails")]
		[PXViewDetailsButton(typeof(CRLead),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<CRLead.parentBAccountID>>>>),
				ActionName = "Items_BAccountParent_ViewDetails")]
		public PXFilteredProcessingJoin<CRLead, CRWorkflowMassActionFilter,
			LeftJoin<Address, 
				On<Address.addressID, Equal<CRLead.defAddressID>>,
			LeftJoin<BAccount, 
				On<BAccount.bAccountID, Equal<CRLead.bAccountID>>,
			LeftJoin<BAccountParent, 
				On<BAccountParent.bAccountID, Equal<CRLead.parentBAccountID>>,
			LeftJoin<State,
				On<State.countryID, Equal<Address.countryID>,
				And<State.stateID, Equal<Address.state>>>,
			LeftJoin<CRActivityStatistics, 
				On<CRLead.noteID, Equal<CRActivityStatistics.noteID>>>>>>>,
			Where<
				CRLead.contactType.IsEqual<ContactTypesAttribute.lead>
				.And<Brackets<BAccount.bAccountID.IsNull.Or<Match<BAccount, AccessInfo.userName.FromCurrent>>>>
				.And<Brackets<
						CRWorkflowMassActionFilter.operation.FromCurrent.IsEqual<CRWorkflowMassActionOperation.updateSettings>
							.And<CRLead.isActive.IsEqual<True>>>
					.Or<WorkflowAction.IsEnabled<CRLead, CRWorkflowMassActionFilter.action>>>>,
			OrderBy<
				Asc<CRLead.displayName>>>
			Items;

		protected override PXFilteredProcessing<CRLead, CRWorkflowMassActionFilter> ProcessingView => Items;
	}
}
