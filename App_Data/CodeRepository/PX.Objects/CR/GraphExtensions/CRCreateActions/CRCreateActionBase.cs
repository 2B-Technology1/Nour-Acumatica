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
using PX.Data.MassProcess;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.CS.Contracts.Interfaces;
using PX.Objects.GDPR;
using PX.Common;
using System.Reactive.Disposables;
using PX.CS;
using System.Collections;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Wizard;
using System.Runtime.ExceptionServices;
using PX.Objects.IN;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[PXInternalUseOnly]
	public abstract class CRCreateActionBase<TGraph, TMain, TTargetGraph, TTarget, TFilter, TConversionOptions> : CRCreateActionBaseInit<TGraph, TMain>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
		where TTargetGraph : PXGraph, new()
		where TTarget : class, IBqlTable, INotable, new()
		where TFilter : class, IBqlTable, IClassIdFilter, new()
		where TConversionOptions : ConversionOptions<TTargetGraph, TTarget>
	{
		#region State

		protected virtual string TargetType => CRTargetEntityType.Contact;

		private CRPopupValidator.Generic<TFilter> _popupValidator;
		public virtual CRPopupValidator.Generic<TFilter> PopupValidator =>
			_popupValidator ??= CRPopupValidator.Create(FilterInfo, AdditionalFilters);

		protected virtual ICRValidationFilter[] AdditionalFilters => null;

		public bool NeedToUse { get; set; }

		public virtual void ClearAnswers(bool clearCurrent = false)
		{
			this.FilterInfo.ClearAnswers(clearCurrent);
		}

		#endregion

		public override void Initialize()
		{
			NeedToUse = true;

			base.Initialize();
		}

		#region Views

		protected abstract CRValidationFilter<TFilter> FilterInfo { get; }
		protected virtual PXSelectBase<CRPMTimeActivity> Activities => null;

		#endregion

		#region Attributes

		public virtual void _(Events.FieldVerifying<PopupAttributes, PopupAttributes.displayName> e)
		{
			if (e.Row?.Value == null)
			{
				throw new PXSetPropertyException(Messages.FillReqiredAttributes, PXErrorLevel.Error);
			}
		}

		// specified attributes
		public virtual IEnumerable<PopupAttributes> GetFilledAttributes()
		{
			PXCache cache = Base.Caches[typeof(PopupAttributes)];

			foreach (var field in GetPreparedAttributes())
			{
				var item = (PopupAttributes)cache.Locate(field);

				if (item == null)
					cache.Hold(field);

				yield return item ?? field;
			}
		}

		// attributes from class (and prepared entity)
		protected virtual IEnumerable<PopupAttributes> GetPreparedAttributes()
		{
			var master = GetAttributesForMasterEntity().Where(a => a.Value != null).ToList();

			return CRAttribute
				.EntityAttributes(typeof(TTarget), FilterInfo.Current?.ClassID)
				.Where(a => a.Required)
				.Select(a => (entity: a, master: master.FirstOrDefault(_a => _a.AttributeID == a.ID)))
				.Select((a, i) => new PopupAttributes
				{
					Selected = false,
					CacheName = typeof(TTarget).FullName,
					Name = a.entity.ID + "_Attributes",
					DisplayName = a.entity.Description,
					AttributeID = a.entity.ID,
					Value = a.master?.Value ?? a.entity.DefaultValue,
					Order = i,
					Required = true
				});
		}

		protected virtual IEnumerable<CSAnswers> GetAttributesForMasterEntity()
		{
			// view from base lead
			return Base
				.Views[nameof(ContactMaint.Answers)]
				.SelectMulti()
				.RowCast<CSAnswers>();
		}

		protected virtual object GetMasterEntity()
		{
			return null;
		}

		public virtual void _(Events.FieldSelecting<PopupAttributes, PopupAttributes.value> e)
		{
			var row = e.Row as PopupAttributes;
			if (row == null || !typeof(TTarget).FullName.Equals(row.CacheName))
				return;

			PXDBAttributeAttribute.Activate(Base.Caches[typeof(TTarget)]);

			e.ReturnState = PXMassProcessHelper.InitValueFieldState(Base.Caches[typeof(TTarget)], e.Row as FieldValue);
			e.Cancel = true;
		}
		#endregion

		#region UDF Attributes
		public virtual void _(Events.FieldVerifying<PopupUDFAttributes, PopupUDFAttributes.displayName> e)
		{
			if (e.Row?.Value == null || string.IsNullOrEmpty(e.Row?.Value.ToString()))
			{
				throw new PXSetPropertyException(Messages.FillReqiredUDFAttributes, PXErrorLevel.Error);
			}
		}
		public virtual void _(Events.FieldSelecting<PopupUDFAttributes, PopupUDFAttributes.value> e)
		{
			var row = e.Row;
			string screenID = PXSiteMap.Provider.FindSiteMapNode(typeof(TTargetGraph))?.ScreenID;
			if (row == null || screenID == null || !screenID.Equals(row.ScreenID))
				return;
			PXDBAttributeAttribute.Activate(Base.Caches[typeof(TTarget)]);
			var state = UDFHelper.GetGraphUDFFieldState(typeof(TTargetGraph), row.AttributeID);
			if (state != null)
			{
				state.Required = true;
				if (!string.IsNullOrEmpty(row.Value))
				{
					state.Value = row.Value;
				}
				e.ReturnState = state;
				e.Cache.IsDirty = false;
			}
		}
		#endregion

		#region Actions

		public virtual void ValidateForImport(params CRPopupValidator[] validators)
		{
			if (!Base.IsContractBasedAPI && Base.IsDirty)
				Base.Actions.PressSave();

			AdjustFilterForContactBasedAPI(FilterInfo.Current);
			PopupValidator.Validate(validators);
		}

		public virtual void CheckWizardState()
		{
			if (WizardScope.IsScoped is false)
				return;

			switch (PopupValidator.Filter.View.Answer)
			{
				case WizardResult.Back:
				{
					ClearAnswers();
					throw new CRWizardBackException();
				}
				case WizardResult.Abort:
				{
					ClearAnswers();
					throw new CRWizardAbortException();
				}
			}
		}

		public virtual WebDialogResult AskExt(params CRPopupValidator[] validators)
		{
			try
			{
				return PopupValidator.AskExt(
					(graph, view) =>
					{
						if (Base.IsDirty)
						{
							Base.Actions[nameof(PXGraph<TGraph, TMain>.Save)]?.Press();
						}

						if (Base.IsDirty)
						{
							// smth went wrong, graph was not persisted properly => go away
							throw new PXSetPropertyException(String.Empty);
						}

						PopupValidator.Reset(validators);
					},
					reset: WizardScope.IsScoped is false);
			}
			catch (PXSetPropertyException)
			{
				return WebDialogResult.Abort;
			}
		}

		public virtual bool AskExtConvert(out bool redirect, params CRPopupValidator[] validators)
		{
			return AskExtConvert(throwOnException: true, out redirect, validators);
		}

		// you can use currents
		public virtual bool AskExtConvert(bool throwOnException, out bool redirect, params CRPopupValidator[] validators)
		{
			// no need to ask for cb, just return true if data is valid
			if (Base.IsContractBasedAPI || Base.IsImport)
			{
				ValidateForImport(validators);
				redirect = false;
				return true;
			}

			CheckWizardState();

			if (AskWasAborted())
				return redirect = false;

			var result = AskExt(validators);

			bool success = PopupValidator.TryValidate(validators);
			if (result != WebDialogResult.Abort && throwOnException && success is false)
				throw new PXActionInterruptException(Messages.ValidationFailed);

			redirect = result == WebDialogResult.Yes;

			return success && result.IsPositive();
		}

		public bool AskWasAborted()
		{
			return PopupValidator.Filter.View.Answer == WebDialogResult.Abort;
		}

		internal virtual void AdjustFilterForContactBasedAPI(TFilter filter)
		{
		}

		public TMain GetMain(Document doc) => (TMain)Documents.Cache.GetMain(doc);
		public TMain GetMainCurrent() => GetMain(Documents.Current);

		public virtual ConversionResult<TTarget> Convert(TConversionOptions options = null)
		{
			return TryConvert(options).ThrowIfHasException();
		}

		public virtual ConversionResult<TTarget> TryConvert(TConversionOptions options = null)
		{
			TTargetGraph graph = null;
			TTarget entity = null;
			Exception exception = null;
			try
			{
				graph = CreateTargetGraph();

				entity = CreateMaster(graph, options);

				ReverseDocumentUpdate(graph, entity);

				OnBeforePersist(graph, entity);

				if (options?.DoNotPersistAfterConvert != true)
					graph.Actions.PressSave();

				if (options?.DoNotCancelAfterConvert != true)
				{
					using (options.PreserveCachedRecords())
					{
						Base.Actions.PressCancel();
						Documents.View.Clear();
						Documents.Current = Documents.Search<Document.noteID>(Documents.Current.NoteID);
					}
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			return new ConversionResult<TTarget>
			{
				Graph = graph,
				Entity = entity,
				Exception = exception,
			};
		}

		protected virtual TTargetGraph CreateTargetGraph()
		{
			var graph = PXGraph.CreateInstance<TTargetGraph>();
			graph.Caches<TMain>().Current = GetMainCurrent();
			return graph;
		}

		protected abstract TTarget CreateMaster(TTargetGraph graph, TConversionOptions options);

		public void Redirect(ConversionResult<TTarget> result)
		{
			if (result == null)
				throw new ArgumentNullException(nameof(result));

			var graph = result.Graph ?? CreateTargetGraph();
			graph.Caches<TTarget>().Current = result.Entity;

			if (result.Converted)
			graph.Actions.PressCancel();

			throw new PXRedirectRequiredException(graph, typeof(TTarget).Name);
		}

		protected virtual void FillAttributes(CRAttributeList<TTarget> answers, TTarget entity)
		{
			answers.CopyAllAttributes(entity, GetMainCurrent());

			foreach (var answer in GetFilledAttributes()
				?.Where(a => a.Value != null)
				?? Enumerable.Empty<PopupAttributes>())
			{
				answers.Update(new CSAnswers
				{
					AttributeID = answer.AttributeID,
					Value = answer.Value,
					RefNoteID = entity.NoteID,
					IsActive = true,
				});
			}
		}

		protected virtual void FillUDF(PXCache sourceUDFPopupCache, object src_row, PXCache dst_cache, TTarget dst_row, string classID)
		{
			UDFHelper.CopyAttributes(Base.Caches<TMain>(), src_row, dst_cache, dst_row, classID);

			UDFHelper.FillfromPopupUDF(
				Base.Caches<TTarget>(),
				sourceUDFPopupCache,
				typeof(TTargetGraph),
				dst_row);
		}

		protected virtual void FillNotesAndAttachments(PXGraph graph, object src_row, PXCache dst_cache, TTarget dst_row)
		{
			bool isCustomerManagementFeatureEnabled = PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
			CRSetup setup = PXSetupOptional<CRSetup>.Select(graph);

			PXNoteAttribute.CopyNoteAndFiles(
				graph.Caches<TMain>(), src_row, dst_cache, dst_row,
				setup?.CopyNotes == true && isCustomerManagementFeatureEnabled,
				setup?.CopyFiles == true && isCustomerManagementFeatureEnabled
			);
		}

		protected virtual void FillRelations(PXGraph graph, TTarget target)
		{
			var entity = Documents.Current;
			var relationsCache = graph.Caches[typeof(CRRelation)];
			var relation = (CRRelation)relationsCache.CreateInstance();

			relation.RefNoteID = target.NoteID;
			relation.RefEntityType = target.GetType().FullName;
			relation.Role = CRRoleTypeList.Source;
			relation.TargetType = TargetType;
			relation.TargetNoteID = entity.NoteID;
			relation.ContactID = entity.RefContactID;

			// otherwise value would be rewriten from cache
			PXDBDefaultAttribute.SetDefaultForInsert<CRRelation.refNoteID>(relationsCache, relation, false);

			relationsCache.Insert(relation);
		}

		protected virtual void ReverseDocumentUpdate(TTargetGraph graph, TTarget entity) { }

		protected virtual void OnBeforePersist(TTargetGraph graph, TTarget entity) { }

		protected virtual TTarget MapFromDocument(Document source, TTarget target)
		{
			return target;
		}

		#endregion

		#region UDF helper functions 
		public virtual IEnumerable<PopupUDFAttributes> GetRequiredUDFFields()
		{
			return UDFHelper.GetRequiredUDFFields(Base.Caches<TMain>(), GetMasterEntity(), typeof(TTargetGraph), FilterInfo?.Current?.ClassID);
		}
		#endregion


	}

	/// <exclude/>
	[PXInternalUseOnly]
	public abstract class CRCreateActionBaseInit<TGraph, TMain> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Views

		#region Document Mapping
		protected class DocumentMapping : IBqlMapping
		{
			public Type Extension => typeof(Document);
			protected Type _table;
			public Type Table => _table;

			public DocumentMapping(Type table)
			{
				_table = table;
			}
			public Type ParentBAccountID = typeof(Document.parentBAccountID);
			public Type WorkgroupID = typeof(Document.workgroupID);
			public Type OverrideSalesTerritory = typeof(Document.overrideSalesTerritory);
			public Type SalesTerritoryID = typeof(Document.salesTerritoryID);
			public Type OwnerID = typeof(Document.ownerID);
			public Type BAccountID = typeof(Document.bAccountID);
			public Type ContactID = typeof(Document.contactID);
			public Type RefContactID = typeof(Document.refContactID);
			public Type ClassID = typeof(Document.classID);
			public Type NoteID = typeof(Document.noteID);
			public Type Source = typeof(Document.source);
			public Type CampaignID = typeof(Document.campaignID);
			public Type OverrideRefContact = typeof(Document.overrideRefContact);
			public Type Description = typeof(Document.description);
			public Type Location = typeof(Document.locationID);
			public Type TaxZoneID = typeof(Document.taxZoneID);
			public Type QualificationDate = typeof(Document.qualificationDate);
			public Type IsActive = typeof(Document.isActive);
		}
		protected virtual DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(TMain));
		}
		#endregion

		#region Document Contact Mapping
		protected class DocumentContactMapping : IBqlMapping
		{
			public Type Extension => typeof(DocumentContact);
			protected Type _table;
			public Type Table => _table;

			public DocumentContactMapping(Type table)
			{
				_table = table;
			}
			public Type FullName = typeof(DocumentContact.fullName);
			public Type Title = typeof(DocumentContact.title);
			public Type FirstName = typeof(DocumentContact.firstName);
			public Type LastName = typeof(DocumentContact.lastName);
			public Type Salutation = typeof(DocumentContact.salutation);
			public Type Attention = typeof(DocumentContact.attention);
			public Type Email = typeof(DocumentContact.email);
			public Type Phone1 = typeof(DocumentContact.phone1);
			public Type Phone1Type = typeof(DocumentContact.phone1Type);
			public Type Phone2 = typeof(DocumentContact.phone2);
			public Type Phone2Type = typeof(DocumentContact.phone2Type);
			public Type Phone3 = typeof(DocumentContact.phone3);
			public Type Phone3Type = typeof(DocumentContact.phone3Type);
			public Type Fax = typeof(DocumentContact.fax);
			public Type FaxType = typeof(DocumentContact.faxType);
			public Type OverrideContact = typeof(DocumentContact.overrideContact);

			public Type ConsentAgreement = typeof(DocumentContact.consentAgreement);
			public Type ConsentDate = typeof(DocumentContact.consentDate);
			public Type ConsentExpirationDate = typeof(DocumentContact.consentExpirationDate);
		}
		protected abstract DocumentContactMapping GetDocumentContactMapping();
		#endregion

		#region Document Address Mapping
		protected class DocumentAddressMapping : IBqlMapping
		{
			public Type Extension => typeof(DocumentAddress);
			protected Type _table;
			public Type Table => _table;

			public DocumentAddressMapping(Type table)
			{
				_table = table;
			}
			public Type OverrideAddress = typeof(DocumentAddress.overrideAddress);
			public Type AddressLine1 = typeof(DocumentAddress.addressLine1);
			public Type AddressLine2 = typeof(DocumentAddress.addressLine2);
			public Type AddressLine3 = typeof(DocumentAddress.addressLine3);
			public Type City = typeof(DocumentAddress.city);
			public Type CountryID = typeof(DocumentAddress.countryID);
			public Type State = typeof(DocumentAddress.state);
			public Type PostalCode = typeof(DocumentAddress.postalCode);
			public Type IsValidated = typeof(DocumentAddress.isValidated);
		}
		protected abstract DocumentAddressMapping GetDocumentAddressMapping();
		#endregion

		public PXSelectExtension<Document> Documents;
		public PXSelectExtension<DocumentContact> Contacts;
		public PXSelectExtension<DocumentAddress> Addresses;

		#endregion

		#region Initialization

		protected virtual IPersonalContact MapContact(DocumentContact source, IPersonalContact target)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target.FullName = source.FullName;
			target.Title = source.Title;
			target.FirstName = source.FirstName;
			target.LastName = source.LastName;
			target.Salutation = source.Salutation;
			target.Attention = source.Attention;
			target.Email = source.Email;
			target.WebSite = source.WebSite;
			target.Phone1 = source.Phone1;
			target.Phone1Type = source.Phone1Type;
			target.Phone2 = source.Phone2;
			target.Phone2Type = source.Phone2Type;
			target.Phone3 = source.Phone3;
			target.Phone3Type = source.Phone3Type;
			target.Fax = source.Fax;
			target.FaxType = source.FaxType;

			return target;
		}

		protected virtual IConsentable MapConsentable(DocumentContact source, IConsentable target)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target.ConsentAgreement = source.ConsentAgreement;
			target.ConsentDate = source.ConsentDate;
			target.ConsentExpirationDate = source.ConsentExpirationDate;

			return target;
		}

		protected virtual IAddressBase MapAddress(DocumentAddress source, IAddressBase target)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			target.AddressLine1 = source.AddressLine1;
			target.AddressLine2 = source.AddressLine2;
			target.City = source.City;
			target.CountryID = source.CountryID;
			target.State = source.State;
			target.PostalCode = source.PostalCode;

			return target;
		}

		#endregion
	}
}
