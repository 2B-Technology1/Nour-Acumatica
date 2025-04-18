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
using PX.Objects.CR;
using System;
using PX.SM;
using PX.Objects.CS;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.PM;
using PX.Objects.CA;
using CROpportunityRevision = PX.Objects.CR.Standalone.CROpportunityRevision;
using PX.Data.BQL.Fluent;

namespace PX.Objects.GDPR
{
	#region CR

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			Contact,
		Where<
			Contact.contactID, Equal<Current<Contact.contactID>>>>))]
	[PXPersonalDataTable(typeof(
		Select2<
			Contact,
		InnerJoin<Location,
			On<Contact.contactID, Equal<Location.defContactID>>>,
		Where<
			Location.bAccountID, Equal<Current<BAccount.bAccountID>>, 
			And<Location.locationID, Equal<Current<BAccount.defLocationID>>>>>))]
	[PXPersonalDataTable(typeof(
		Select2<
			Contact,
		InnerJoin<CR.Standalone.CRLead,
			On<CR.Standalone.CRLead.contactID, Equal<Contact.contactID>>>,
		Where<
			CR.Standalone.CRLead.refContactID, Equal<Current<Contact.contactID>>>>))]
	[PXPersonalDataTable(typeof(
		Select<
			Contact,
		Where<
			Contact.contactType, Equal<ContactTypesAttribute.lead>,
			And<Contact.bAccountID, Equal<Current<BAccount.bAccountID>>>>>))]
	public sealed class ContactExt : PXCacheExtension<Contact>, IPseudonymizable, IPostPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
		
		public List<PXDataFieldParam> InterruptPseudonimyzationHandler(List<PXDataFieldParam> restricts)
		{
			List<PXDataFieldParam> assigns = new List<PXDataFieldParam>();

			assigns.Add(new PXDataFieldAssign<Contact.noCall>(true));
			assigns.Add(new PXDataFieldAssign<Contact.noEMail>(true));
			assigns.Add(new PXDataFieldAssign<Contact.noFax>(true));
			assigns.Add(new PXDataFieldAssign<Contact.noMail>(true));
			assigns.Add(new PXDataFieldAssign<Contact.noMarketing>(true));
			assigns.Add(new PXDataFieldAssign<Contact.noMassMail>(true));

			assigns.Add(new PXDataFieldAssign<Contact.isActive>(false));
			assigns.Add(new PXDataFieldAssign<Contact.status>(ContactStatus.Inactive));	// as the WF is so far from here, desided to make it this way. Rare scenario
			return assigns;
		}
	}

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			CRContact,
		Where<
			CRContact.contactID, Equal<Current<CRContact.contactID>>>>))]
	public sealed class CRContactExt : PXCacheExtension<CRContact>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	[PXPersonalDataTable(typeof(
		Select<
			CROpportunityRevision,
		Where<
			CROpportunityRevision.opportunityContactID, Equal<Current<CRContact.contactID>>>>))]
	[PXPersonalDataTable(typeof(
		Select<
			CROpportunityRevision,
		Where<
			CROpportunityRevision.shipContactID, Equal<Current<CRContact.contactID>>>>))]
	[PXPersonalDataTable(typeof(
		Select<
			CROpportunityRevision,
		Where<
			CROpportunityRevision.billContactID, Equal<Current<CRContact.contactID>>>>))]
	public sealed class CROpportunityRevisionExt : PXCacheExtension<CROpportunityRevision>
	{
		[PXPersonalDataTableAnchor]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public string OpportunityID
		{
			get => Base.OpportunityID;
			set => Base.OpportunityID = value;
		}

		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		// Acuminator disable once PX1030 PXDefaultIncorrectUse [Justification] just a warning
		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			BAccount,
		Where<
			BAccount.defContactID, Equal<Current<Contact.contactID>>>>))]
	public sealed class BAccountExt : PXCacheExtension<BAccount>, IPseudonymizable, IPostPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion

		public List<PXDataFieldParam> InterruptPseudonimyzationHandler(List<PXDataFieldParam> restricts)
		{
			List<PXDataFieldParam> assigns = new List<PXDataFieldParam>();

			assigns.Add(new PXDataFieldAssign<BAccount.status>(CustomerStatus.Inactive));
			assigns.Add(new PXDataFieldAssign<BAccount.vStatus>(VendorStatus.Inactive));

			return assigns;
		}
	}

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			Address,
		Where<
			Address.addressID, Equal<Current<BAccount.defAddressID>>>>))]
	[PXPersonalDataTable(typeof(
		Select2<
			Address,
		InnerJoin<Location,
			On<Address.addressID, Equal<Location.defAddressID>>>,
		Where<
			Location.bAccountID, Equal<Current<BAccount.bAccountID>>,
			And<Location.locationID, Equal<Current<BAccount.defLocationID>>>>>))]
	public sealed class AddressExt : PXCacheExtension<Address>, IPseudonymizable
	{
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
	}

	[Serializable]
	[PXPersonalDataTable(typeof(
		SelectFrom<CRAddress>
			.InnerJoin<CROpportunityRevision>
				.On<CROpportunityRevision.opportunityAddressID.IsEqual<CRAddress.addressID>>
			.Where<CROpportunityRevision.opportunityContactID.IsEqual<CRContact.contactID.FromCurrent>>
	))]
	[PXPersonalDataTable(typeof(
		SelectFrom<CRAddress>
			.InnerJoin<CROpportunityRevision>
				.On<CROpportunityRevision.shipAddressID.IsEqual<CRAddress.addressID>>
			.Where<CROpportunityRevision.shipContactID.IsEqual<CRContact.contactID.FromCurrent>>
	))]
	[PXPersonalDataTable(typeof(
		SelectFrom<CRAddress>
			.InnerJoin<CROpportunityRevision>
				.On<CROpportunityRevision.billAddressID.IsEqual<CRAddress.addressID>>
			.Where<CROpportunityRevision.billContactID.IsEqual<CRContact.contactID.FromCurrent>>
	))]
	public sealed class CRAddressExt : PXCacheExtension<CRAddress>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			Location,
		Where<
			Location.bAccountID, Equal<Current<BAccount.bAccountID>>,
			And<Location.locationID, Equal<Current<BAccount.defLocationID>>>>>))]
	public sealed class LocationExt : PXCacheExtension<Location>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	[Serializable]
	// already pseudonymized by LocationExt above
	public sealed class StandaloneLocationExt : PXCacheExtension<CR.Standalone.Location>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select2<
			CSAnswers,
		InnerJoin<CSAttribute,
			On<CSAttribute.attributeID, Equal<CSAnswers.attributeID>,
			And<CSAttribute.containsPersonalData, Equal<True>>>>,
		Where <
			CSAnswers.refNoteID, Equal<Current<BAccount.noteID>>>>))]
	[PXPersonalDataTable(typeof(
		Select2<
			CSAnswers,
		InnerJoin<CSAttribute,
			On<CSAttribute.attributeID, Equal<CSAnswers.attributeID>,
			And<CSAttribute.containsPersonalData, Equal<True>>>>,
		Where<
			CSAnswers.refNoteID, Equal<Current<Contact.noteID>>>>))]
	public sealed class CSAnswersExt : PXCacheExtension<CSAnswers>, IPseudonymizable, INotable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
		
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuidNotNull]
		public Guid? NoteID { get; set; }
		#endregion
	}

	#endregion
	
	#region AP

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			APContact,
		Where<
			APContact.vendorID, Equal<Current<BAccount.bAccountID>>>>))]
	[PXPersonalDataTable(typeof(
		Select5<
			APContact,
		InnerJoin<APPayment,
			On<APPayment.remitContactID, Equal<APContact.contactID>>>,
		Where<
			APPayment.vendorID, Equal<Current<BAccount.bAccountID>>>,
		Aggregate<
			GroupBy<APContact.noteID>>>))] // remit
	public sealed class APContactExt : PXCacheExtension<APContact>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}
	
	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			APAddress,
		Where<
			APAddress.vendorID, Equal<Current<BAccount.bAccountID>>>>))]
	[PXPersonalDataTable(typeof(
		Select5<
			APAddress,
		InnerJoin<APPayment,
			On<APPayment.remitAddressID, Equal<APAddress.addressID>>>,
		Where<
			APPayment.vendorID, Equal<Current<BAccount.bAccountID>>>,
		Aggregate<
			GroupBy<APAddress.noteID>>>))] // remit
	public sealed class APAddressExt : PXCacheExtension<APAddress>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	#endregion

	#region AR

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			ARContact,
		Where<
			ARContact.customerID, Equal<Current<BAccount.bAccountID>>>>))]
	public sealed class ARContactExt : PXCacheExtension<ARContact>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}
	
	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			ARAddress,
		Where<
			ARAddress.customerID, Equal<Current<BAccount.bAccountID>>>>))]
	public sealed class ARAddressExt : PXCacheExtension<ARAddress>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	#endregion
	
	#region PM

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			PMContact,
		Where<
			PMContact.customerID, Equal<Current<BAccount.bAccountID>>>>))]
	public sealed class PMContactExt : PXCacheExtension<PMContact>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}
	
	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			PMAddress,
		Where<
			PMAddress.customerID, Equal<Current<BAccount.bAccountID>>>>))]
	public sealed class PMAddressExt : PXCacheExtension<PMAddress>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	#endregion

	#region PO

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			POContact,
		Where<
			POContact.bAccountID, Equal<Current<BAccount.bAccountID>>>>))]
	[PXPersonalDataTable(typeof(
		Select5<
			POContact,
		InnerJoin<POOrder,
			On<POOrder.shipContactID, Equal<POContact.contactID>,
			And<
				Where<POOrder.shipDestType, Equal<POShippingDestination.customer>,
				Or<POOrder.shipDestType, Equal<POShippingDestination.vendor>>>>>>,
		Where<
			POOrder.shipToBAccountID, Equal<Current<BAccount.bAccountID>>,
			And<POContact.isDefaultContact, Equal<False>>>,
		Aggregate<
			GroupBy<POContact.noteID>>>))] // shipping
	[PXPersonalDataTable(typeof(
		Select5<
			POContact,
		InnerJoin<POOrder,
			On<POOrder.remitContactID, Equal<POContact.contactID>,
			And<POOrder.shipDestType, Equal<POShippingDestination.vendor>>>>,
		Where<
			POOrder.vendorID, Equal<Current<BAccount.bAccountID>>>,
		Aggregate<
			GroupBy<POContact.noteID>>>))] // remit
	public sealed class POContactExt : PXCacheExtension<POContact>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}
	
	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			POAddress,
		Where<
			POAddress.bAccountID, Equal<Current<BAccount.bAccountID>>>>))]
	[PXPersonalDataTable(typeof(
		Select5<
			POAddress,
		InnerJoin<POOrder,
			On<POOrder.shipAddressID, Equal<POAddress.addressID>,
			And<
				Where<POOrder.shipDestType, Equal<POShippingDestination.customer>,
				Or<POOrder.shipDestType, Equal<POShippingDestination.vendor>>>>>>,
		Where<
			POOrder.vendorID, Equal<Current<BAccount.bAccountID>>,
			And<POAddress.isDefaultAddress, Equal<False>>>,
		Aggregate<
			GroupBy<POAddress.noteID>>>))] // shipping
	[PXPersonalDataTable(typeof(
		Select5<
			POAddress,
		InnerJoin<POOrder,
			On<POOrder.remitAddressID, Equal<POAddress.addressID>,
			And<POOrder.shipDestType, Equal<POShippingDestination.vendor>>>>,
		Where<
			POOrder.vendorID, Equal<Current<BAccount.bAccountID>>>,
		Aggregate<
			GroupBy<POAddress.noteID>>>))] // remit
	public sealed class POAddressExt : PXCacheExtension<POAddress>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	#endregion

	#region SO

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			SOContact,
		Where<
			SOContact.customerID, Equal<Current<BAccount.bAccountID>>>>))]
	public sealed class SOContactExt : PXCacheExtension<SOContact>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}
	
	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			SOAddress,
		Where<
			SOAddress.customerID, Equal<Current<BAccount.bAccountID>>>>))]
	public sealed class SOAddressExt : PXCacheExtension<SOAddress>, IPseudonymizable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
	}

	#endregion

	#region Other

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			Users,
		Where<
			Users.pKID, Equal<Current<Contact.userID>>>>))]
	public sealed class UsersExt : PXCacheExtension<Users>, IPseudonymizable, IPostPseudonymizable
	{
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }

		public List<PXDataFieldParam> InterruptPseudonimyzationHandler(List<PXDataFieldParam> restricts)
		{
			List<PXDataFieldParam> assigns = new List<PXDataFieldParam>();

			assigns.Add(new PXDataFieldAssign<Users.isApproved>(false));

			return assigns;
		}
	}

	[Serializable]
	[PXPersonalDataTable(typeof(
		Select<
			LoginTrace,
		Where<
			LoginTrace.username, Equal<Current<Users.username>>>>))]
	public sealed class LoginTraceExt : PXCacheExtension<LoginTrace>, IPseudonymizable, INotable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
		
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuidNotNull]
		public Guid? NoteID { get; set; }
		#endregion
	}
	
	[Serializable]
	[PXPersonalDataTable(typeof(
		Select2<
			CustomerPaymentMethodDetail,
		InnerJoin<CustomerPaymentMethod, 
			On<CustomerPaymentMethod.pMInstanceID, Equal<CustomerPaymentMethodDetail.pMInstanceID>>,
		InnerJoin<PaymentMethod,
			On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
			And<PaymentMethod.containsPersonalData, Equal<True>>>>>,
		Where<
			CustomerPaymentMethod.bAccountID, Equal<Current<BAccount.bAccountID>>>>))]
	public sealed class CustomerPaymentMethodDetailExt : PXCacheExtension<CustomerPaymentMethodDetail>, IPseudonymizable, INotable
	{
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		public int? PseudonymizationStatus { get; set; }
		#endregion
		
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuidNotNull]
		public Guid? NoteID { get; set; }
		#endregion
	}

	#endregion
}
