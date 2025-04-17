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
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.SM;
using System.Linq;
using System.Web.Compilation;
using PX.Objects.CR.Extensions.Cache;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.EP;
using PX.Objects.PO;
using PX.Objects.SO;

namespace PX.Objects.CR.Extensions
{
	/// <summary>
	/// Represents the Relations grid
	/// </summary>
	public abstract class CRRelationDetailsExt<TGraph, TMaster, TNoteField> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TMaster : class, IBqlTable, new()
		where TNoteField : IBqlField
	{
		#region State

		public PXCache MasterCache => Base.Caches[BqlCommand.GetItemType(typeof(TNoteField))];

		public EntityHelper EntityHelperInstance;

		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			var command = new Select2<
					CRRelation,
				LeftJoin<Contact,
					On<Contact.contactID, Equal<CRRelation.contactID>>,
				LeftJoin<BAccount,
					On<BAccount.bAccountID, Equal<CRRelation.entityID>>,
				LeftJoin<Users,
					On<Users.pKID, Equal<Contact.userID>>>>>,
				Where<
					CRRelation.refNoteID, Equal<Current<TNoteField>>>>();

			dbView = new PXView(Base, false, command);

			EntityHelperInstance = new EntityHelper(Base);
		}

		#endregion

		#region Views

		[PXCopyPasteHiddenView]
		[PXHidden]
		public SelectFrom<Contact>.View Contact_Dummy;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public SelectFrom<BAccount>.View BAccount_Dummy;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public SelectFrom<CRRelation>
			.LeftJoin<Contact>
				.On<True.IsEqual<False>>
			.LeftJoin<BAccount>
				.On<True.IsEqual<False>>
			.LeftJoin<Users>
				.On<True.IsEqual<False>>
			.OrderBy<
				CRRelation.createdDateTime.Asc>
			.View Relations;

		protected PXView dbView;

		public virtual IEnumerable relations()
		{
			var ret = new PXDelegateResult()
			{
				IsResultSorted = false,
				IsResultTruncated = false,
				IsResultFiltered = true
			};

			foreach (PXResult<CRRelation, Contact, BAccount, Users> result in dbView.SelectMulti())
			{
				var relation = result.GetItem<CRRelation>();
				var contact = result.GetItem<Contact>();
				var businessAccount = result.GetItem<BAccount>();
				var user = result.GetItem<Users>();

				var relatedEntity = GetRelatedEntityObject(relation);

				FillUnboundDataForRelation(relation, contact, businessAccount, user, relatedEntity);

				if (!MeetFilter(relation, relatedEntity, PXView.Filters))
					continue;

				ret.Add(result);
			}

			return ret;
		}

		protected Type GetRelatedEntityType(string typeName) => PXBuildManager.GetType(typeName, false);

		#endregion

		#region Actions

		public PXAction<TMaster> RelationsViewTargetDetails;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable relationsViewTargetDetails(PXAdapter adapter)
		{
			var row = this.Relations.Current;

			if (row == null)
				return adapter.Get();

			new EntityHelper(Base).NavigateToRow(row.TargetType, row.TargetNoteID, PXRedirectHelper.WindowMode.New);

			return adapter.Get();
		}

		public PXAction<TMaster> RelationsViewEntityDetails;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable relationsViewEntityDetails(PXAdapter adapter)
		{
			var row = BAccount.PK.Find(Base, this.Relations.Current?.EntityID);

			if (row == null)
				return adapter.Get();

			PXRedirectHelper.TryRedirect(Base, row, PXRedirectHelper.WindowMode.New);

			return adapter.Get();
		}

		public PXAction<TMaster> RelationsViewContactDetails;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable relationsViewContactDetails(PXAdapter adapter)
		{
			var row = Contact.PK.Find(Base, this.Relations.Current?.ContactID);

			if (row == null)
				return adapter.Get();

			PXRedirectHelper.TryRedirect(Base, row, PXRedirectHelper.WindowMode.New);

			return adapter.Get();
		}

		#endregion

		#region Events

		#region Field-level

		#region CRRelationDetail Declaration

		[CRRelationDetail(statusField: typeof(APInvoice.status), descriptionField: typeof(APInvoice.docDesc), ownerField: typeof(APInvoice.employeeID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<APInvoice.refNbr> e) { }

		[CRRelationDetail(statusField: typeof(ARInvoice.status), descriptionField: typeof(ARInvoice.docDesc), ownerField: typeof(ARInvoice.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<ARInvoice.refNbr> e) { }

		[CRRelationDetail(statusField: typeof(BAccount.status), descriptionField: null, ownerField: typeof(BAccount.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<BAccount.bAccountID> e) { }

		[CRRelationDetail(statusField: typeof(CRCampaign.status), descriptionField: typeof(CRCampaign.description), ownerField: typeof(CRCampaign.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRCampaign.campaignID> e) { }

		[CRRelationDetail(statusField: typeof(CRCase.status), descriptionField: typeof(CRCase.subject), ownerField: typeof(CRCase.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRCase.caseCD> e) { }

		[CRRelationDetail(statusField: typeof(Contact.status), descriptionField: null, ownerField: typeof(Contact.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<Contact.contactID> e) { }

		[CRRelationDetail(statusField: typeof(Customer.status), descriptionField: null, ownerField: typeof(Customer.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<Customer.bAccountID> e) { }

		[CRRelationDetail(statusField: typeof(CREmployee.vStatus), descriptionField: null, ownerField: null)]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CREmployee.bAccountID> e) { }

		[CRRelationDetail(statusField: typeof(EPExpenseClaimDetails.status), descriptionField: null, ownerField: null)]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<EPExpenseClaimDetails.claimDetailCD> e) { }

		[CRRelationDetail(statusField: typeof(CRLead.status), descriptionField: null, ownerField: typeof(CRLead.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRLead.contactID> e) { }

		[CRRelationDetail(statusField: typeof(CROpportunity.status), descriptionField: typeof(CROpportunity.subject), ownerField: typeof(CROpportunity.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CROpportunity.opportunityID> e) { }

		[CRRelationDetail(statusField: typeof(POOrder.status), descriptionField: typeof(POOrder.orderDesc), ownerField: typeof(POOrder.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<POOrder.orderNbr> e) { }

		[CRRelationDetail(statusField: typeof(SOOrder.status), descriptionField: typeof(SOOrder.orderDesc), ownerField: typeof(SOOrder.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<SOOrder.orderNbr> e) { }

		[CRRelationDetail(statusField: typeof(CRQuote.status), descriptionField: typeof(CRQuote.subject), ownerField: typeof(CRQuote.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRQuote.quoteID> e) { }

		[CRRelationDetail(statusField: typeof(Vendor.vStatus), descriptionField: null, ownerField: typeof(Vendor.ownerID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<Vendor.bAccountID> e) { }

		#endregion

		[StatusField(typeof(CRRelation.targetType))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRRelation.status> e) { }

		protected virtual void _(Events.FieldDefaulting<CRRelation, CRRelation.refEntityType> e)
		{
			e.NewValue = MasterCache.GetItemType().FullName;
		}

		protected virtual void _(Events.FieldDefaulting<CRRelation, CRRelation.refNoteID> e)
		{
			e.NewValue = MasterCache.GetValue(MasterCache.Current, typeof(TNoteField).Name);
		}

		protected virtual void _(Events.FieldUpdated<CRRelation, CRRelation.isPrimary> e)
		{
			if (e.Row.IsPrimary == true && !String.IsNullOrEmpty(e.Row.TargetType))
			{
				ClearOtherPrimaryRelations(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<CRRelation, CRRelation.targetType> e)
		{
			if (e.Row.IsPrimary == true && !String.IsNullOrEmpty(e.Row.TargetType))
			{
				ClearOtherPrimaryRelations(e.Row);
			}
		}

		#endregion

		#region Row-level

		protected virtual void _(Events.RowDeleted<TMaster> e)
		{
			if (e.Row == null)
				return;

			Relations.SelectMain().ForEach(relation => Relations.Delete(relation));
		}

		protected virtual void _(Events.RowInserted<CRRelation> e)
		{
			FillUnboundDataForRelation(e.Cache.Graph, e.Row);
		}

		protected virtual void _(Events.RowUpdated<CRRelation> e)
		{
			if (!e.Cache.ObjectsEqual<CRRelation.targetNoteID>(e.Row, e.OldRow))
			{
				Type cachetype = e.Row.TargetType != null
					? GraphHelper.GetType(e.Row.TargetType)
					: null;

				if (cachetype != null &&
					!typeof(BAccount).IsAssignableFrom(cachetype) &&
					!typeof(Contact).IsAssignableFrom(cachetype))
				{
					e.Row.DocNoteID = e.Row.TargetNoteID;
				}
				else
				{
					e.Row.DocNoteID = null;
				}
			}

			if (e.Cache.ObjectsEqual<CRRelation.contactID>(e.Row, e.OldRow)
				&& !e.Cache.ObjectsEqual<CRRelation.entityID>(e.Row, e.OldRow))
			{
				e.Row.ContactID = null;
			}

			FillUnboundDataForRelation(e.Cache.Graph, e.Row);
		}

		// TODO: check performance first
		//protected virtual void _(Events.RowSelecting<CRRelation> e)
		//{
		//	FillRow(e.Cache.Graph, e.Row);
		//}

		protected virtual void _(Events.RowSelected<CRRelation> e)
		{
			if (e.Row == null)
				return;

			// I hope it can be deleted
			//FillRow(e.Cache.Graph, e.Row);

			var enableContactID = BAccount.PK.Find(Base, e.Row.EntityID)?.Type != BAccountType.EmployeeType;

			PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, nameof(CRRelation.contactID), enableContactID);
		}

		protected virtual void _(Events.RowPersisting<CRRelation> e)
		{
			if (e.Row.ContactID == null && e.Row.TargetType == CRTargetEntityType.Contact)
			{
				e.Cache.RaiseExceptionHandling<CRRelation.contactID>(e.Row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"{nameof(CRRelation.ContactID)}"));
			}
			else if (e.Row.EntityID == null && e.Row.TargetType.IsIn(CRTargetEntityType.BAccount, CRTargetEntityType.Customer, CRTargetEntityType.Vendor))
			{
				e.Cache.RaiseExceptionHandling<CRRelation.entityID>(e.Row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"{nameof(CRRelation.EntityID)}"));
			}
			else if (e.Row.TargetNoteID == null && e.Row.TargetType.IsNotIn(CRTargetEntityType.Contact, CRTargetEntityType.BAccount, CRTargetEntityType.Customer, CRTargetEntityType.Vendor))
			{
				e.Cache.RaiseExceptionHandling<CRRelation.targetNoteID>(e.Row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"{nameof(CRRelation.TargetNoteID)}"));
			}
		}

		#endregion

		#endregion

		#region Methods

		public virtual BAccount GetBusinessAccount(CRRelation relation)
		{
			return BAccount.PK.Find(Base, relation?.EntityID);
		}

		public virtual Contact GetContact(CRRelation relation)
		{
			return Contact.PK.Find(Base, relation?.ContactID);
		}

		public virtual Users GetUser(Contact contact)
		{
			return Users.PK.Find(Base, contact?.UserID);
		}

		public virtual void FillUnboundDataForRelation(PXGraph graph, CRRelation relation)
		{
			var contact = GetContact(relation);
			var businessAccount = GetBusinessAccount(relation);
			var users = GetUser(contact);

			var relatedEntity = GetRelatedEntityObject(relation);

			FillUnboundDataForRelation(relation, contact, businessAccount, users, relatedEntity);
		}

		public virtual void FillUnboundDataForRelation(CRRelation relation, Contact contact, BAccount businessAccount, Users user, (object Entity, Type EntityType) relatedEntity)
		{
			CRRelation.FillUnboundData(relation, contact, businessAccount, user);


			if (relatedEntity.Entity == null)
				return;

			var field = Base.Caches[relatedEntity.EntityType].GetField_WithAttribute<CRRelationDetail>();

			if (field.Attribute == null)
				return;

			(relation.Status, relation.Description, relation.OwnerID) = field.Attribute.GetValues(Base.Caches[relatedEntity.EntityType], relatedEntity.Entity);
		}

		public virtual bool MeetFilter(CRRelation relation, (object Entity, Type EntityType) relatedEntity, PXView.PXFilterRowCollection filters)
		{
			var viewForResultMapping = new PXView(Base, false, BqlCommand.CreateInstance(GetFilterView(relatedEntity.EntityType)));

			IEnumerable getItemRecord()
			{
				yield return (PXResult)viewForResultMapping.CreateResult(new[] { relation, relatedEntity.Entity });
			}

			var viewForFiltering = new PXView(Base, false, BqlCommand.CreateInstance(GetFilterView(relatedEntity.EntityType)), (PXSelectDelegate)getItemRecord);

			int startRow = 0;
			int totalRows = 0;
			var result = viewForFiltering.Select(null, null, null, null, null, filters, ref startRow, 0, ref totalRows);

			return result.Count > 0;
		}

		public virtual Type GetFilterView(Type relatedEntityType)
		{
			return BqlCommand.Compose(typeof(Select2<,>), typeof(CRRelation), typeof(LeftJoin<,>), relatedEntityType ?? typeof(CRRelation), typeof(On<True, Equal<False>>));
		}

		public virtual (object, Type) GetRelatedEntityObject(CRRelation relation)
		{
			if (relation.TargetType == null)
				return (null, null);

			var etityType = GetRelatedEntityType(relation.TargetType);

			var entity = EntityHelperInstance.GetEntityRow(etityType, relation.TargetNoteID);

			return (entity, etityType);
		}

		public virtual void ClearOtherPrimaryRelations(CRRelation relation)
		{
			foreach (CRRelation rel in this.Relations
						.SelectMain()
						.ToList()
						.Where(item => item.IsPrimary == true && item.RelationID != relation.RelationID))
			{
				rel.IsPrimary = false;

				this.Relations.Update(rel);
			}

			this.Relations.View.RequestRefresh();
		}

		#endregion
	}
}
