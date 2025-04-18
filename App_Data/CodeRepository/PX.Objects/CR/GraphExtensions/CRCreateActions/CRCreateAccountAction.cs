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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Data;
using PX.Objects.IN;
using System.Reactive.Disposables;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.BusinessAccountMaint_Extensions;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public abstract partial class CRCreateAccountAction<TGraph, TMain>
		: CRCreateActionBase<
			TGraph,
			TMain,
			BusinessAccountMaint,
			BAccount,
			AccountsFilter,
			AccountConversionOptions>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Ctor

		protected override ICRValidationFilter[] AdditionalFilters => new ICRValidationFilter[] { AccountInfoAttributes, AccountInfoUDF };

		#endregion

		#region Views
		
		public SelectFrom<
				BAccount>
			.Where<
				BAccount.bAccountID.IsEqual<Document.bAccountID.FromCurrent.NoDefault>>
			.View
			ExistingAccount;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<AccountsFilter> AccountInfo;
		protected override CRValidationFilter<AccountsFilter> FilterInfo => AccountInfo;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupAttributes> AccountInfoAttributes;
		protected virtual IEnumerable accountInfoAttributes()
		{
			return GetFilledAttributes();
		}

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupUDFAttributes> AccountInfoUDF;
		protected virtual IEnumerable<PopupUDFAttributes> accountInfoUDF()
		{
			return base.GetRequiredUDFFields();
		}

		protected override IEnumerable<CSAnswers> GetAttributesForMasterEntity()
		{
			return ExistingAccount.SelectSingle() is BAccount account
				? PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<BAccount.noteID>>>>
						.Select(Base, account.NoteID).FirstTableItems
				: base.GetAttributesForMasterEntity();
		}

		protected override object GetMasterEntity()
		{
			return ExistingAccount.SelectSingle();
		}
		#endregion

		#region Events

		protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.bAccountID> e)
		{
			var existing = ExistingAccount.SelectSingle();
			if (existing?.AcctCD != null)
			{
				e.NewValue = existing.AcctCD;
				return;
			}

			if (IsDimensionAutonumbered(Base, CustomerAttribute.DimensionName))
			{
				e.NewValue = GetDimensionAutonumberingNewValue(Base, CustomerAttribute.DimensionName);
			}
		}

		protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.accountName> e)
		{
			// HACK: for existing account it displayed empty for somereason
			if (ExistingAccount.SelectSingle()?.AcctName is string name)
			{
				e.NewValue = name;
				return;
			}

			var docContact = Contacts.Current ?? Contacts.SelectSingle();

			e.NewValue = docContact?.FullName;
		}

		protected virtual void _(Events.FieldVerifying<AccountsFilter, AccountsFilter.bAccountID> e)
		{
			if (ExistingAccount.SelectSingle() != null)
				return;

			BAccount existing = PXSelect<
					BAccount,
				Where<
					BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>
				.SelectSingleBound(Base, null, e.NewValue);

			if (existing != null)
			{
				AccountInfo.Cache.RaiseExceptionHandling<AccountsFilter.bAccountID>(e.Row, e.NewValue, new PXSetPropertyException(Messages.BAccountAlreadyExists, e.NewValue));
			}
			else
			{
				AccountInfo.Cache.RaiseExceptionHandling<AccountsFilter.bAccountID>(e.Row, e.NewValue, null);
			}
		}

		protected virtual void _(Events.FieldUpdated<AccountsFilter, AccountsFilter.accountClass> e)
		{
			Base.Caches<PopupAttributes>().Clear();
		}

		protected virtual void _(Events.RowSelected<AccountsFilter> e)
		{
			var existing = ExistingAccount.SelectSingle();

			e.Cache.AdjustUI(e.Row)
				.ForAllFields(_ => _.Enabled = existing == null)
				.For<AccountsFilter.bAccountID>(_ => _.Enabled = existing == null && !IsDimensionAutonumbered(Base, CustomerAttribute.DimensionName));
		}

		protected virtual void _(Events.RowSelected<Document> e)
		{
			var existing = ExistingAccount.SelectSingle();

			CreateBAccount.SetEnabled(existing == null);
			CreateBAccount.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.customerModule>());

			if (existing != null)
				AccountInfoAttributes.AllowUpdate = AccountInfoUDF.AllowUpdate = false;
		}

		#endregion

		protected virtual bool IsDimensionAutonumbered(PXGraph graph, string dimension)
		{
			return PXSelect<
					Segment, 
				Where<
					Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>
				.Select(graph, dimension)
				.RowCast<Segment>()
				.All(segment => segment.AutoNumber == true);
		}

		protected virtual string GetDimensionAutonumberingNewValue(PXGraph graph, string dimension)
		{
			Numbering numbering = (PXResult<Dimension, Numbering>) PXSelectJoin<
						Dimension,
					LeftJoin<Numbering, 
						On<Dimension.numberingID, Equal<Numbering.numberingID>>>,
					Where<
						Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>,
						And<Numbering.userNumbering, NotEqual<True>>>>
				.SelectSingleBound(graph, null, dimension);

			return numbering?.NewSymbol ?? Messages.New;
		}

		#region Actions

		public PXAction<TMain> CreateBAccount;
		[PXUIField(DisplayName = Messages.CreateAccount, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable createBAccount(PXAdapter adapter)
		{
			if (AskExtConvert(out bool redirect))
			{
				if (Base.IsDirty)
					Base.Actions.PressSave();

				var processingGraph = Base.CloneGraphState();
				PXLongOperation.StartOperation(Base, () =>
				{
					var extension = processingGraph.GetProcessingExtension<CRCreateAccountAction<TGraph, TMain>>();

					var result = extension.Convert();

					if (redirect)
						extension.Redirect(result);
				});
			}
			return adapter.Get();
		}

		public PXAction<TMain> CreateBAccountRedirect;
		[PXUIField(DisplayName = Messages.CreateAccountRedirect, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable createBAccountRedirect(PXAdapter adapter)
		{
			var graph = CreateTargetGraph();
			var entity = CreateMaster(graph, null);

			Redirect(new ConversionResult<BAccount>()
				{
					Graph = graph,
					Entity = entity,
					Converted = false
				}
			);

			return adapter.Get();
		}

		public override ConversionResult<BAccount> Convert(AccountConversionOptions options = null)
		{
			// do nothing if account already exist
			if (ExistingAccount.SelectSingle() is BAccount account)
			{
				//PXTrace.WriteVerbose($"Using existing account: {account.AcctCD}.");
				return new ConversionResult<BAccount>
				{
					Converted = false,
					Entity = account,
				};
			}

			return base.Convert(options);
		}

		protected override BAccount CreateMaster(BusinessAccountMaint graph, AccountConversionOptions _)
		{
			var param = AccountInfo.Current;
			var document = Documents.Current;
			var docContact = Contacts.Current ?? Contacts.SelectSingle();
			var docAddress = Addresses.Current ?? Addresses.SelectSingle();

			object cd = param.BAccountID;
			graph.BAccount.Cache.RaiseFieldUpdating<BAccount.acctCD>(null, ref cd);

			BAccount account = graph.BAccount.Insert(new BAccount
			{
				AcctCD = (string)cd,
				AcctName = param.AccountName,
				Type = BAccountType.ProspectType,
				ParentBAccountID = document.ParentBAccountID,
				CampaignSourceID = document.CampaignID,
				OverrideSalesTerritory = document.OverrideSalesTerritory,
			});

			account.ClassID = param.AccountClass; // In case of (param.AccountClass == null) constructor fills ClassID with default value, so we have to set this directly.

			if (account.OverrideSalesTerritory is true)
			{
				account.SalesTerritoryID = document.SalesTerritoryID;
			}

			CRCustomerClass ocls = PXSelect<
						CRCustomerClass,
					Where<
						CRCustomerClass.cRCustomerClassID, Equal<Required<CRCustomerClass.cRCustomerClassID>>>>
				.SelectSingleBound(graph, null, account.ClassID);

			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				account.WorkgroupID = document.WorkgroupID;
				account.OwnerID = document.OwnerID;
			}

			account = graph.BAccount.Update(account);

			var defContactAddress = graph.GetExtension<BusinessAccountMaint.DefContactAddressExt>();

			if (param.LinkContactToAccount == true)
			{
				// in case of opportunity
				Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<CROpportunity.contactID>>>>.Select(graph, document.RefContactID);
				if (contact != null)
				{
					graph.Answers.CopyAttributes(account, contact);
					contact.BAccountID = account.BAccountID;
					defContactAddress.DefContact.Update(contact);
				}
			}


			var defContact = defContactAddress.DefContact.SelectSingle()
				?? throw new InvalidOperationException("Cannot get Contact for Business Account."); // just to ensure
			MapContact(docContact, account, ref defContact);
			MapConsentable(docContact, defContact);
			defContact = defContactAddress.DefContact.Update(defContact);

			var defAddress = defContactAddress.DefAddress.SelectSingle()
				?? throw new InvalidOperationException("Cannot get Address for Business Account."); // just to ensure
			MapAddress(docAddress, account, ref defAddress);
			defAddress = defContactAddress.DefAddress.Update(defAddress);

			var locationDetails = graph.GetExtension<BusinessAccountMaint.DefLocationExt>();
			CR.Standalone.Location location = locationDetails.DefLocation.Select();
			location.DefAddressID = defAddress.AddressID;
			location.CTaxZoneID = document.TaxZoneID;
			locationDetails.DefLocation.Update(location);

			account = graph.BAccount.Update(account);

			ReverseDocumentUpdate(graph, account);

			FillRelations(graph, account);

			FillAttributes(graph.Answers, account);

			FillUDF(AccountInfoUDF.Cache, GetMain(document), graph.BAccount.Cache, account, account.ClassID);

			TransferActivities(graph, account);

			FillNotesAndAttachments(graph, Documents.Cache.GetMain(document), graph.CurrentBAccount.Cache, account);

			return account;
		}

		protected override void ReverseDocumentUpdate(BusinessAccountMaint graph, BAccount entity)
		{
			var document = Documents.Current;
			Documents.Cache.SetValue<Document.bAccountID>(document, entity.BAccountID);
			Documents.Cache.SetValue<Document.locationID>(document, entity.DefLocationID);

			graph.Caches<TMain>().Update(GetMain(document));
		}


		protected virtual void MapContact(DocumentContact docContact, BAccount account, ref Contact contact)
		{
			base.MapContact(docContact, contact);
			contact.Title = null;
			contact.FirstName = null;
			contact.LastName = null;
			contact.ContactType = ContactTypesAttribute.BAccountProperty;
			contact.FullName = account.AcctName;
			contact.ContactID = account.DefContactID;
			contact.BAccountID = account.BAccountID;
		}

		protected virtual void MapAddress(DocumentAddress docAddress, BAccount account, ref Address address)
		{
			base.MapAddress(docAddress, address);
		}

		protected virtual void TransferActivities(BusinessAccountMaint graph, BAccount account)
		{
			foreach (CRPMTimeActivity activity in Activities.Select())
			{
				activity.BAccountID = account.BAccountID;
				graph.GetExtension<BusinessAccountMaint_ActivityDetailsExt>().Activities.Update(activity);
			}
		}

		#endregion
	}
}
