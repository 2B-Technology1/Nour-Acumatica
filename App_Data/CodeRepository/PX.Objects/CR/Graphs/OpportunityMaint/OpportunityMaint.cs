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

using Autofac;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Discount;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using PX.Objects.Extensions.Discount;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.MultiCurrency.CR;
using PX.Objects.Extensions.SalesPrice;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.IN.Attributes;
using PX.Objects.SO;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;
using PX.Data.WorkflowAPI;
using PX.Objects.CR.Extensions.CROpportunityContactAddress;
using PX.Objects.CR.Extensions.CRCreateSalesOrder;
using PX.Objects.CR.Extensions.CRCreateInvoice;
using PX.CS;
using PX.Objects.CR.OpportunityMaint_Extensions;
using static PX.Data.BQL.BqlPlaceholder;
using System.Web.Caching;

namespace PX.Objects.CR
{
    public class OpportunityMaint : PXGraph<OpportunityMaint>, PXImportAttribute.IPXPrepareItems, IGraphWithInitialization
	{
		#region Filters

        #region CreateQuotesFilter
        [Serializable]
	    public partial class CreateQuotesFilter : IBqlTable
	    {
            #region QuoteType
            public abstract class quoteType : PX.Data.BQL.BqlString.Field<quoteType> { }
            [PXString(1, IsFixed = true)]
            [PXUIField(DisplayName = "Quote Type")]
            [CRQuoteType()]
            [PXUnboundDefault(CRQuoteTypeAttribute.Distribution)]
            public virtual string QuoteType { get; set; }
            #endregion

            #region AddProductsFromOpportunity
            public abstract class addProductsFromOpportunity : PX.Data.BQL.BqlBool.Field<addProductsFromOpportunity> { }
	        [PXBool()]
            [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
	        [PXUIField(DisplayName = "Add Details from Opportunity")]
	        public virtual bool? AddProductsFromOpportunity { get; set; }
            #endregion

            #region MakeNewQuotePrimary
            public abstract class makeNewQuotePrimary : PX.Data.BQL.BqlBool.Field<makeNewQuotePrimary> { }
	        [PXBool()]
            [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
	        [PXUIField(DisplayName = "Set New Quote as Primary")]
	        public virtual bool? MakeNewQuotePrimary { get; set; }
            #endregion

            #region RecalculatePrices
            public abstract class recalculatePrices : PX.Data.BQL.BqlBool.Field<recalculatePrices> { }
	        [PXBool()]
	        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Recalculate Prices")]
	        public virtual bool? RecalculatePrices { get; set; }
            #endregion

            #region Override Manual Prices
            public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
	        [PXBool()]
	        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
	        [PXUIField(DisplayName = "Override Manual Prices")]
	        public virtual bool? OverrideManualPrices { get; set; }
            #endregion

            #region Recalculate Discounts
	        public abstract class recalculateDiscounts : PX.Data.BQL.BqlBool.Field<recalculateDiscounts> { }
            [PXBool()]
	        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
	        [PXUIField(DisplayName = "Recalculate Discounts")]
            public virtual bool? RecalculateDiscounts { get; set; }
            #endregion

	        #region Override Manual Discounts
	        public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
	        [PXBool()]
	        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
	        [PXUIField(DisplayName = "Override Manual Line Discounts")]
	        public virtual bool? OverrideManualDiscounts { get; set; }
	        #endregion

			#region OverrideManualDocGroupDiscounts
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Override Manual Group and Document Discounts")]
			public virtual Boolean? OverrideManualDocGroupDiscounts { get; set; }
			public abstract class overrideManualDocGroupDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDocGroupDiscounts> { }
			#endregion
        }
        #endregion

        #region CopyQuoteFilter
        [Serializable]
        public partial class CopyQuoteFilter : IBqlTable
        {
            #region OpportunityID
            public abstract class opportunityId : PX.Data.BQL.BqlString.Field<opportunityId> { }

            [PXString()]
            [PXUIField(DisplayName = "Opportunity ID", Visible = false)]
            [PXSelector(typeof(Search2<CROpportunity.opportunityID,
                LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
                    LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>>>,
                Where<CROpportunity.isActive.IsEqual<True>>,
                OrderBy<Desc<CROpportunity.opportunityID>>>),
            new[] { typeof(CROpportunity.opportunityID),
                typeof(CROpportunity.subject),
                typeof(CROpportunity.status),
                typeof(CROpportunity.stageID),
                typeof(CROpportunity.classID),
                typeof(BAccount.acctName),
                typeof(Contact.displayName),
                typeof(CROpportunity.subject),
                typeof(CROpportunity.externalRef),
                typeof(CROpportunity.closeDate) },
            Filterable = true)]
            [PXDefault(typeof(CROpportunity.opportunityID), PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual string OpportunityID { get; set; }
            #endregion

            #region Description
            public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
            protected string _Description;
            [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
            [PXString(60, IsUnicode = true)]
            [PXUIField(DisplayName = "Description", Required = true)]
            public virtual string Description
            {
                get { return _Description; }
                set { _Description = value; }
            }
            #endregion

            #region RecalculatePrices
            public abstract class recalculatePrices : PX.Data.BQL.BqlBool.Field<recalculatePrices> { }
            [PXBool()]
            [PXUIField(DisplayName = "Recalculate Prices")]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual bool? RecalculatePrices { get; set; }
            #endregion

            #region OverrideManualPrices
            public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
            [PXBool()]
            [PXUIField(DisplayName = "Override Manual Prices", Enabled = false)]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual bool? OverrideManualPrices { get; set; }
            #endregion

            #region RecalculateDiscounts
            public abstract class recalculateDiscounts : PX.Data.BQL.BqlBool.Field<recalculateDiscounts> { }
            [PXBool()]
            [PXUIField(DisplayName = "Recalculate Discounts")]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual bool? RecalculateDiscounts { get; set; }
            #endregion

            #region OverrideManualDiscounts
            public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
            [PXBool()]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Override Manual Line Discounts", Enabled = false)]
            public virtual bool? OverrideManualDiscounts { get; set; }
            #endregion

			#region OverrideManualDocGroupDiscounts
			[PXBool()]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Override Manual Group and Document Discounts")]
			public virtual Boolean? OverrideManualDocGroupDiscounts { get; set; }
			public abstract class overrideManualDocGroupDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDocGroupDiscounts> { }
			#endregion
        }
        #endregion
		
		#endregion

		#region Selects / Views

        [PXHidden()]
        public PXSelect<BAccount> BAccounts;

		//TODO: need review
		[PXHidden]
		public PXSelect<BAccount>
			bAccountBasic;

        [PXHidden]
        public PXSelect<BAccountR>
            bAccountRBasic;

        [PXHidden]
		public PXSetupOptional<SOSetup>
			sosetup;

		[PXHidden]
		public PXSetup<CRSetup>
			Setup;

		[PXCopyPasteHiddenFields(typeof(CROpportunity.stageID), typeof(CROpportunity.resolution))]
		[PXViewName(Messages.Opportunity)]
		public PXSelectJoin<CROpportunity,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>>,
				Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>
			Opportunity;

		[PXHidden]
        public PXSelect<CROpportunityRevision>
            OpportunityRevision;

        [PXHidden]
		public PXSelect<CRLead> Leads;

		[PXCopyPasteHiddenFields(typeof(CROpportunity.stageID), typeof(CROpportunity.resolution))]
		public PXSelect<CROpportunity,
			Where<CROpportunity.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
			OpportunityCurrent;

		[PXHidden]
		public PXSelectReadonly<CRQuote, Where<CRQuote.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>> PrimaryQuoteQuery;

		public PXSelect<CROpportunityProbability,
			Where<CROpportunityProbability.stageCode, Equal<Current<CROpportunity.stageID>>>>
			ProbabilityCurrent;

		[PXHidden]
		public PXSelect<Address>
			Address;

		[PXHidden]
		public PXSetup<Contact, Where<Contact.contactID, Equal<Optional<CROpportunity.contactID>>>> Contacts;

		[PXHidden]
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<CROpportunity.bAccountID>>>> customer;


		[PXViewName(Messages.Answers)]
		public CRAttributeSourceList<CROpportunity, CROpportunity.contactID>
			Answers;

        public PXSetup<CROpportunityClass, Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunity.classID>>>> OpportunityClass;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics>
			ActivityStatistics;

		public virtual IEnumerable activityStatistics()
		{
			var opp = ActivityOpportunityStatistics.SelectSingle();
			var qt = ActivityQuoteStatistics.SelectSingle();
			var lastIn = opp?.LastIncomingActivityDate > qt?.LastIncomingActivityDate ? opp : qt;
			var lastOut = opp?.LastOutgoingActivityDate > qt?.LastOutgoingActivityDate ? opp : qt;

			yield return (opp != null && qt != null)
				? new CRActivityStatistics()
				{
					LastActivityDate = lastIn.LastActivityDate > lastOut.LastActivityDate ? lastIn.LastActivityDate : lastOut.LastActivityDate,
					LastIncomingActivityDate = lastIn.LastIncomingActivityDate,
					LastIncomingActivityNoteID = lastIn.LastIncomingActivityNoteID,
					LastOutgoingActivityDate = lastOut.LastOutgoingActivityDate,
					LastOutgoingActivityNoteID = lastOut.LastOutgoingActivityNoteID
				}
				: opp ?? qt;
		}

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<CROpportunity.noteID>>>>
			ActivityOpportunityStatistics;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<CRQuote.noteID>>>>
			ActivityQuoteStatistics;

	    [PXCopyPasteHiddenView]	    
	    [PXFilterable]
	    public PXSelect<CRQuote, Where<CRQuote.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
            Quotes;

	    [PXViewName(Messages.CopyQuote)]
		[PXCopyPasteHiddenView]
        public PXFilter<CopyQuoteFilter> CopyQuoteInfo;

		[PXViewName(Messages.OpportunityProducts)]
		[PXImport(typeof(CROpportunity))]
		public PXOrderedSelect<CROpportunity, CROpportunityProducts,
			Where<CROpportunityProducts.quoteID, Equal<Current<CROpportunity.defQuoteID>>>,
			OrderBy<Asc<CROpportunityProducts.sortOrder>>>
			Products;
		
	    public PXSelect<CROpportunityTax,
			Where<CROpportunityTax.quoteID, Equal<Current<CROpportunity.quoteNoteID>>,
			  And<CROpportunityTax.lineNbr, Less<intMax>>>,
            OrderBy<Asc<CROpportunityTax.taxID>>> TaxLines;

		 [PXViewName(Messages.OpportunityTax)]
		 public PXSelectJoin<CRTaxTran,
			 InnerJoin<Tax, On<Tax.taxID, Equal<CRTaxTran.taxID>>>,
			 Where<CRTaxTran.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
			 OrderBy<Asc<CRTaxTran.lineNbr, Asc<CRTaxTran.taxID>>>> Taxes;

	    public PXSetup<Location, 
            Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>,
              And<Location.locationID, Equal<Optional<CROpportunity.locationID>>>>> location;

	    [PXViewName(Messages.CreateQuote)]
		[PXCopyPasteHiddenView]
	    public PXFilter<CreateQuotesFilter> QuoteInfo;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupUDFAttributes> CreateQuoteInfoUDF;

		protected virtual IEnumerable createQuoteInfoUDF()
		{
			CreateQuotesFilter quoteFilter = (CreateQuotesFilter)this.Caches<CreateQuotesFilter>()?.Current;
			if (quoteFilter.QuoteType == CRQuoteTypeAttribute.Project)
			{
				return UDFHelper.GetRequiredUDFFields(this.Caches<CROpportunity>(), null, typeof(PM.PMQuoteMaint));
			}
			else
			{
				return UDFHelper.GetRequiredUDFFields(this.Caches<CROpportunity>(), null, typeof(QuoteMaint));
			}
		}

        [PXViewName(Messages.OpportunityContact)]
        public PXSelect<CRContact, Where<CRContact.contactID, Equal<Current<CROpportunity.opportunityContactID>>>> Opportunity_Contact;

        [PXViewName(Messages.OpportunityAddress)]
        public PXSelect<CRAddress, Where<CRAddress.addressID, Equal<Current<CROpportunity.opportunityAddressID>>>> Opportunity_Address;

        [PXViewName(Messages.ShippingContact)]
        public PXSelect<CRShippingContact, Where<CRShippingContact.contactID, Equal<Current<CROpportunity.shipContactID>>>> Shipping_Contact;

        [PXViewName(Messages.ShippingAddress)]
        public PXSelect<CRShippingAddress, Where<CRShippingAddress.addressID, Equal<Current<CROpportunity.shipAddressID>>>> Shipping_Address;

        [PXViewName(Messages.BillToContact)]
        public PXSelect<CRBillingContact, Where<CRBillingContact.contactID, Equal<Current<CROpportunity.billContactID>>>> Billing_Contact;

        [PXViewName(Messages.BillToAddress)]
        public PXSelect<CRBillingAddress, Where<CRBillingAddress.addressID, Equal<Current<CROpportunity.billAddressID>>>> Billing_Address;


        [PXHidden]
        public PXSelectJoin<Contact,
            LeftJoin<Address, On<Contact.defAddressID, Equal<Address.addressID>>>,  
            Where<Contact.contactID, Equal<Current<CROpportunity.contactID>>>> CurrentContact;       
		
        [PXHidden]
        public PXSelect<SOBillingContact> CurrentSOBillingContact;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<
			CROpportunityProducts,
			InnerJoin<InventoryItem,
				On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>,
			InnerJoin<INSite, On<INSite.siteID, Equal<CROpportunityProducts.siteID>>>>,
			Where<CROpportunityProducts.quoteID, Equal<Required<CROpportunity.quoteNoteID>>,
				And<Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>,
				And<Where<INSite.siteCD, Equal<Required<INSite.siteCD>>>>>>>>
			ProductsByQuoteIDAndInventoryCD;

		public PXSelect<INItemSiteSettings, Where<INItemSiteSettings.inventoryID, Equal<Required<INItemSiteSettings.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Required<INItemSiteSettings.siteID>>>>> initemsettings;

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }
		#endregion

		#region Ctors

		public OpportunityMaint()
		{
			var crsetup = Setup.Current;

			if (string.IsNullOrEmpty(Setup.Current.OpportunityNumberingID))
			{
				throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);
			}

			//Have to be remnoved for multicurrency.
			this.Views.Caches.Remove(typeof(CRQuote));

		    actionsFolder.MenuAutoOpen = true;

			Caches[typeof(SOOrder)].AllowUpdate = Caches[typeof(SOOrder)].AllowInsert = Caches[typeof(SOOrder)].AllowDelete = false;
			Caches[typeof(ARInvoice)].AllowUpdate = Caches[typeof(ARInvoice)].AllowInsert = Caches[typeof(ARInvoice)].AllowDelete = false;

			PXUIFieldAttribute.SetVisible<Contact.languageID>(OpportunityCurrent.Cache, null, PXDBLocalizableStringAttribute.HasMultipleLocales);
		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<CROpportunity>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(CROpportunityProducts), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<CROpportunityProducts.quoteID>(((OpportunityMaint)graph).Opportunity.Current?.QuoteNoteID)
						};
					}));
			}
		}

		#endregion

		#region Actions

		public PXSave<CROpportunity> Save;
		public PXCancel<CROpportunity> Cancel;
		public PXInsert<CROpportunity> Insert;
		public PXCopyPasteAction<CROpportunity> CopyPaste;
		public PXDelete<CROpportunity> Delete;
		public PXFirst<CROpportunity> First;
		public PXPrevious<CROpportunity> Previous;
		public PXNext<CROpportunity> Next;
		public PXLast<CROpportunity> Last;

		public PXAction<CROpportunity> createQuote;
		[PXUIField(DisplayName = Messages.CreateQuote, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
		public virtual IEnumerable CreateQuote(PXAdapter adapter)
		{
			foreach (CROpportunity opportunity in adapter.Get<CROpportunity>())
			{
				if (QuoteInfo.View.Answer == WebDialogResult.None)
				{
					QuoteInfo.Cache.Clear();
					QuoteInfo.Cache.Insert();
				}

				WebDialogResult result = this.IsImport ? WebDialogResult.No : QuoteInfo.AskExt();

				CreateQuoteInfoUDF.Validate();

				if (result == WebDialogResult.Cancel)
					yield return opportunity;

				Opportunity.Current = opportunity;
				Actions.PressSave();

				if (QuoteInfo.Current.QuoteType == CRQuoteTypeAttribute.Project)
				{
					if (!PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() && OpportunityCurrent.Current.CuryID != (Accessinfo.BaseCuryID ?? new PXSetup<PX.Objects.GL.Company>(this).Current?.BaseCuryID))
					throw new PXException(Messages.CannotCreateProjectQuoteBecauseOfCury);
				}

				var clone = this.CloneGraphState();
				if ((QuoteInfo.Current.QuoteType == CRQuoteTypeAttribute.Distribution))
					PXLongOperation.StartOperation(this, () => clone.CreateNewQuote(opportunity, clone.QuoteInfo.Current, result));
				else
					PXLongOperation.StartOperation(this, () => clone.CreateNewProjectQuote(opportunity, clone.QuoteInfo.Current, result));

				yield return opportunity;
			}
		}

	    public PXAction<CROpportunity> actionsFolder;

        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
        protected virtual IEnumerable ActionsFolder(PXAdapter adapter)
        {
            return adapter.Get();
        }

		public PXAction<CROpportunity> updateClosingDate;
		[PXUIField(Visible = false)]
		[PXButton]
		public virtual IEnumerable UpdateClosingDate(PXAdapter adapter)
		{
			var opportunity = Opportunity.Current;
			if (opportunity != null)
			{
				opportunity.ClosingDate = Accessinfo.BusinessDate;
				Opportunity.Cache.Update(opportunity);
				Save.Press();
			}
			return adapter.Get();
		}

        public PXAction<CROpportunity> viewMainOnMap;

        [PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ViewMainOnMap()
        {
            var address = Opportunity_Address.SelectSingle();
            if (address != null)
            {
                BAccountUtility.ViewOnMap(address);
            }
        }

        public PXAction<CROpportunity> ViewShippingOnMap;

        [PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void viewShippingOnMap()
        {
            var address = Shipping_Address.SelectSingle();
            if (address != null)
            {
                BAccountUtility.ViewOnMap(address);
            }
        }

        public PXAction<CROpportunity> ViewBillingOnMap;
        [PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void viewBillingOnMap()
        {
	        var address = Billing_Address.SelectSingle();
	        if (address != null)
	        {
		        BAccountUtility.ViewOnMap(address);
	        }
        }

		public PXAction<CROpportunity> primaryQuote;
		[PXUIField(DisplayName = Messages.MarkAsPrimary)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
		public virtual IEnumerable PrimaryQuote(PXAdapter adapter)
		{
			foreach (CROpportunity opp in adapter.Get<CROpportunity>())
			{
				if (Quotes.Current?.IsPrimary != true)
				{
                    var selectExistingPrimary = new PXSelect<CRQuote, Where<CRQuote.quoteID, Equal<Required<CRQuote.quoteID>>>>(this);

                    CRQuote primary = selectExistingPrimary.Select(opp.DefQuoteID);
                    if (primary != null && primary.QuoteID != Quotes.Current.QuoteID && primary.Status == PM.PMQuoteStatusAttribute.Closed)
                    {
                        throw new PXException(PM.Messages.QuoteIsClosed, opp.OpportunityID, primary.QuoteNbr);
                    }


					var quoteID = Quotes.Current.QuoteID;
					var opportunityID = this.Opportunity.Current.OpportunityID;
                    this.Persist();
					PXDatabase.Update<Standalone.CROpportunity>(
						new PXDataFieldAssign<Standalone.CROpportunity.defQuoteID>(quoteID),
						new PXDataFieldRestrict<Standalone.CROpportunity.opportunityID>(PXDbType.VarChar, 255, opportunityID, PXComp.EQ)
						);
					this.Cancel.Press();
					CROpportunity rec = this.Opportunity.Search<CROpportunity.opportunityID>(opportunityID);
                    yield return rec;
                }
                yield return opp;
            }
        }

        public virtual void CreateNewQuote(CROpportunity opportunity, CreateQuotesFilter param, WebDialogResult result)
	    {
            QuoteMaint graph = CreateInstance<QuoteMaint>();
		    graph.SelectTimeStamp();

			CreateNewSalesQuote(graph, opportunity, param, result);

			if (result == WebDialogResult.Yes)
            {
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
            }
			else if (result == WebDialogResult.No)
			{
				graph.Actions.PressSave();
			}
        }

		protected virtual void CreateNewSalesQuote(QuoteMaint graph, CROpportunity opportunity, CreateQuotesFilter param, WebDialogResult result)
		{
			graph.Opportunity.Current = graph.Opportunity.SelectSingle(opportunity.OpportunityID);
	        var quote = (CRQuote)graph.Quote.Cache.CreateInstance();

			quote.OpportunityID = opportunity.OpportunityID;

			quote.OpportunityAddressID = opportunity.OpportunityAddressID;
			quote.OpportunityContactID = opportunity.OpportunityContactID;
			quote.ShipAddressID = opportunity.ShipAddressID;
			quote.ShipContactID = opportunity.ShipContactID;
			quote.BillAddressID = opportunity.BillAddressID;
			quote.BillContactID = opportunity.BillContactID;

			quote = graph.Quote.Insert(quote);

			quote.LocationID = opportunity.LocationID;
			quote.ContactID = opportunity.ContactID;
			quote.BAccountID = opportunity.BAccountID;

			quote = graph.Quote.Update(quote);

			foreach (string field in Opportunity.Cache.Fields)
			{
				if (graph.Quote.Cache.Keys.Contains(field)
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.quoteID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.status))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.approved))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.rejected))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.opportunityAddressID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.opportunityContactID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.shipAddressID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.shipContactID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.billAddressID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.billContactID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.createdByID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.createdByScreenID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.createdDateTime))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.lastModifiedByID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.lastModifiedByScreenID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.lastModifiedDateTime))
					)
					continue;

				graph.Quote.Cache.SetValue(quote, field,
					Opportunity.Cache.GetValue(opportunity, field));
			}
			graph.Quote.Cache.SetDefaultExt<CRQuote.termsID>(quote);
			quote.DocumentDate = this.Accessinfo.BusinessDate;

			if (IsSingleQuote(quote.OpportunityID))
			{
				quote.QuoteID = quote.NoteID = opportunity.QuoteNoteID;
			}
			else
			{
				object quoteID;
				graph.Quote.Cache.RaiseFieldDefaulting<CRQuote.noteID>(quote, out quoteID);
				quote.QuoteID = quote.NoteID = (Guid?)quoteID;
			}

			CurrencyInfo info =
			    PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CROpportunity.curyInfoID>>>>
				    .Select(this);

		    info.CuryInfoID = null;

		    info = (CurrencyInfo)graph.Caches<CurrencyInfo>().Insert(info);
		    quote.CuryInfoID = info.CuryInfoID;
		    quote.Subject = opportunity.Subject;
		    quote.DocumentDate = this.Accessinfo.BusinessDate;
		    quote.IsPrimary = param.MakeNewQuotePrimary;
			quote.TermsID = opportunity.TermsID;

			if (param.MakeNewQuotePrimary == true)
				quote.DefQuoteID = quote.QuoteID;

			if (param.AddProductsFromOpportunity == true && !IsSingleQuote(quote.OpportunityID))
		    {
				Products.View.CloneView(graph, quote.QuoteID, info);
		    }
		    else
		    {			    
				graph.Quote.Cache.SetDefaultExt<CRQuote.curyDiscTot>(quote);
				graph.Quote.Cache.SetDefaultExt<CRQuote.curyLineDocDiscountTotal>(quote);
				graph.Quote.Cache.SetDefaultExt<CRQuote.curyProductsAmount>(quote);
			}



		    string note = PXNoteAttribute.GetNote(Opportunity.Cache, opportunity);
	        Guid[] files = PXNoteAttribute.GetFileNotes(Opportunity.Cache, opportunity);

	        PXNoteAttribute.SetNote(graph.Quote.Cache, quote, note);
	        PXNoteAttribute.SetFileNotes(graph.Quote.Cache, quote, files);

	        if (param.AddProductsFromOpportunity == true && !IsSingleQuote(quote.OpportunityID))
	        {
	            var DiscountExt = this.GetExtension<Discount>();
				TaxLines.View.CloneView(graph, quote.QuoteID, info);
				Taxes.View.CloneView(graph, quote.QuoteID, info, nameof(CRTaxTran.RecordID));
				Views[nameof(DiscountExt.DiscountDetails)].CloneView(graph, quote.QuoteID, info);
			}
	        

	        if (opportunity.AllowOverrideContactAddress == true)
	        {
				Opportunity_Contact.View.CloneView(graph, quote.QuoteID, info, nameof(CRContact.ContactID));
	            quote.OpportunityContactID = graph.Quote_Contact.Current.ContactID;
				Opportunity_Address.View.CloneView( graph, quote.QuoteID, info, nameof(CRAddress.AddressID));
	            quote.OpportunityAddressID = graph.Quote_Address.Current.AddressID;
	        }
			if (Shipping_Contact.SelectSingle() is CRContact shippingContact && shippingContact.OverrideContact is true)
			{
				Shipping_Contact.View.CloneView( graph, quote.QuoteID, info, nameof(CRShippingContact.ContactID));
				quote.ShipContactID = shippingContact.ContactID;
			}
			if (Shipping_Address.SelectSingle() is CRAddress shippingAddress && shippingAddress.OverrideAddress is true)
			{
				Shipping_Address.View.CloneView( graph, quote.QuoteID, info, nameof(CRShippingAddress.AddressID));
				quote.ShipAddressID = shippingAddress.AddressID;
			}
			if (Billing_Contact.SelectSingle() is CRContact billingContact && billingContact.OverrideContact is true)
			{
				Billing_Contact.View.CloneView( graph, quote.QuoteID, info, nameof(CRBillingContact.ContactID));
				quote.BillContactID = billingContact.ContactID;
			}
			if (Billing_Address.SelectSingle() is CRAddress billingAddress && billingAddress.OverrideAddress is true)
			{
				Billing_Address.View.CloneView( graph, quote.QuoteID, info, nameof(CRBillingAddress.AddressID));
				quote.BillAddressID = billingAddress.AddressID;
			}
			UDFHelper.CopyAttributes(this.Caches<CROpportunity>(), opportunity, graph.Quote.Cache, quote, null);
			UDFHelper.FillfromPopupUDF(
						this.Caches<CRQuote>(),
						CreateQuoteInfoUDF.Cache,
						graph.GetType(),
						quote);

			graph.Quote.Update(quote);
		    var Discount = graph.GetExtension<QuoteMaint.Discount>();
			Discount.recalcdiscountsfilter.Current.OverrideManualDocGroupDiscounts = param.OverrideManualDocGroupDiscounts == true;
		    Discount.recalcdiscountsfilter.Current.OverrideManualDiscounts = param.OverrideManualDiscounts == true;
		    Discount.recalcdiscountsfilter.Current.OverrideManualPrices = param.OverrideManualPrices == true;
		    Discount.recalcdiscountsfilter.Current.RecalcDiscounts = param.RecalculateDiscounts == true;
		    Discount.recalcdiscountsfilter.Current.RecalcUnitPrices = param.RecalculatePrices == true;				
			graph.Actions[nameof(Discount.RecalculateDiscountsAction)].Press();
			Discount.RefreshTotalsAndFreeItems(Discount.Details.Cache);
		}

		public void CreateNewProjectQuote(CROpportunity opportunity, CreateQuotesFilter param, WebDialogResult result)
		{
			var graph = CreateInstance<PM.PMQuoteMaint>();
			graph.SelectTimeStamp();

			CreateNewProjectQuote(graph, opportunity, param, result);

			if (result == WebDialogResult.Yes)
            {
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
            }
			else if (result == WebDialogResult.No)
			{
				graph.Actions.PressSave();
			}
        }

		protected virtual void CreateNewProjectQuote(PM.PMQuoteMaint graph, CROpportunity opportunity, CreateQuotesFilter param, WebDialogResult result)
		{
			graph.Opportunity.Current = graph.Opportunity.SelectSingle(opportunity.OpportunityID);
			var quote = (PM.PMQuote)graph.Quote.Cache.CreateInstance();

			if (IsSingleQuote(opportunity.OpportunityID))
			{
				quote.QuoteID = quote.NoteID = opportunity.QuoteNoteID;
			}
			else
			{
				object quoteID;
				graph.Quote.Cache.RaiseFieldDefaulting<PM.PMQuote.noteID>(quote, out quoteID);
				quote.QuoteID = quote.NoteID = (Guid?)quoteID;
			}

			quote = graph.Quote.Insert(quote);

			quote.LocationID = opportunity.LocationID;
			quote.ContactID = opportunity.ContactID;
			quote.BAccountID = opportunity.BAccountID;
			quote.OpportunityID = opportunity.OpportunityID;
			quote.OpportunityAddressID = opportunity.OpportunityAddressID;
			quote.OpportunityContactID = opportunity.OpportunityContactID;
			quote.ShipAddressID = opportunity.ShipAddressID;
			quote.ShipContactID = opportunity.ShipContactID;

			quote = graph.Quote.Update(quote);

			foreach (string field in Opportunity.Cache.Fields)
			{
				if (graph.Quote.Cache.Keys.Contains(field)
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.quoteProjectID))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.manualTotalEntry))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyAmount))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyDiscTot))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyExtPriceTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyProductsAmount))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyTaxTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyLineTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyVatExemptTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyVatTaxableTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.status))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.approved))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.rejected))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.noteID))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.quoteID))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.opportunityAddressID))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.opportunityContactID))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.shipAddressID))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.shipContactID)))
					continue;

				graph.Quote.Cache.SetValue(quote, field, Opportunity.Cache.GetValue(opportunity, field));
			}
			graph.Quote.Cache.SetDefaultExt<PM.PMQuote.termsID>(quote);
			quote.DocumentDate = this.Accessinfo.BusinessDate;

			CurrencyInfo info =
				PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CROpportunity.curyInfoID>>>>
					.Select(this);

			info.CuryInfoID = null;

			info = (CurrencyInfo)graph.Caches<CurrencyInfo>().Insert(info);
			quote.CuryInfoID = info.CuryInfoID;
			quote.Subject = opportunity.Subject;
			quote.DocumentDate = this.Accessinfo.BusinessDate;
			quote.IsPrimary = param.MakeNewQuotePrimary;
			quote.TermsID = opportunity.TermsID;

			if (param.MakeNewQuotePrimary == true)
				quote.DefQuoteID = quote.QuoteID;

			graph.Quote.Cache.SetDefaultExt<CRQuote.curyDiscTot>(quote);
			graph.Quote.Cache.SetDefaultExt<CRQuote.curyLineDocDiscountTotal>(quote);
			graph.Quote.Cache.SetDefaultExt<CRQuote.curyProductsAmount>(quote);

			string note = PXNoteAttribute.GetNote(Opportunity.Cache, opportunity);
			Guid[] files = PXNoteAttribute.GetFileNotes(Opportunity.Cache, opportunity);

			PXNoteAttribute.SetNote(graph.Quote.Cache, quote, note);
			PXNoteAttribute.SetFileNotes(graph.Quote.Cache, quote, files);

			if (opportunity.AllowOverrideContactAddress == true)
			{
				Opportunity_Contact.View.CloneView(graph, quote.QuoteID, info, nameof(CRContact.ContactID));
				quote.OpportunityContactID = graph.Quote_Contact.Current.ContactID;
				Opportunity_Address.View.CloneView( graph, quote.QuoteID, info, nameof(CRAddress.AddressID));
				quote.OpportunityAddressID = graph.Quote_Address.Current.AddressID;
			}
			if (Shipping_Contact.SelectSingle() is CRContact shippingContact && shippingContact.OverrideContact is true)
			{
				Shipping_Contact.View.CloneView(graph, quote.QuoteID, info, nameof(CRShippingContact.ContactID));
				quote.ShipContactID = shippingContact.ContactID;
			}
			if (Shipping_Address.SelectSingle() is CRAddress shippingAddress && shippingAddress.OverrideAddress is true)
			{
				Shipping_Address.View.CloneView(graph, quote.QuoteID, info, nameof(CRShippingAddress.AddressID));
				quote.ShipAddressID = shippingAddress.AddressID;
			}
			UDFHelper.CopyAttributes(this.Caches<CROpportunity>(), opportunity, graph.Quote.Cache, quote, null);
			UDFHelper.FillfromPopupUDF(
						this.Caches<PM.PMQuote>(),
						CreateQuoteInfoUDF.Cache,
						graph.GetType(),
						quote);

			graph.Quote.Update(quote);

			foreach(var product in graph.Products.Select())
			{
				graph.Products.Delete(product);
			}
			foreach (var tax in graph.TaxLines.Select())
			{
				graph.TaxLines.Delete(tax);
			}
			foreach (var discount in graph._DiscountDetails.Select())
			{
				graph._DiscountDetails.Delete(discount);
			}

			graph.Quote.Cache.SetDefaultExt<PM.PMQuote.curyAmount>(quote);
			graph.Quote.Cache.SetDefaultExt<PM.PMQuote.curyCostTotal>(quote);
			graph.Quote.Cache.SetDefaultExt<PM.PMQuote.curyTaxTotal>(quote);
		}

        public PXAction<CROpportunity> copyQuote;
        [PXUIField(DisplayName = Messages.CopyQuote, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual IEnumerable CopyQuote(PXAdapter adapter)
        {
            CRQuote currentQuote = Quotes.Cache.Current as CRQuote;
            if (currentQuote == null)
                return adapter.Get();

            foreach (CROpportunity opportunity in adapter.Get<CROpportunity>())
            {
                if (CopyQuoteInfo.View.Answer == WebDialogResult.None)
                {
                    CopyQuoteInfo.Cache.Clear();
                    CopyQuoteFilter filterdata = CopyQuoteInfo.Cache.Insert() as CopyQuoteFilter;
                    if (filterdata != null)
                    {
                        filterdata.Description = currentQuote.Subject + Messages.QuoteCopy;
                    }
                }

                if (CopyQuoteInfo.AskExt() != WebDialogResult.Yes)
                    return adapter.Get();

				switch (currentQuote.QuoteType)
				{
					case CRQuoteTypeAttribute.Distribution:
                QuoteMaint.CopyQuoteFilter quoteFilterData = new QuoteMaint.CopyQuoteFilter()
		{
                    OpportunityID = CopyQuoteInfo.Current.OpportunityID,
                    Description = CopyQuoteInfo.Current.Description,
                    RecalculatePrices = CopyQuoteInfo.Current.RecalculatePrices,
                    RecalculateDiscounts = CopyQuoteInfo.Current.RecalculateDiscounts,
                    OverrideManualPrices = CopyQuoteInfo.Current.OverrideManualPrices,
							OverrideManualDiscounts = CopyQuoteInfo.Current.OverrideManualDiscounts,
							OverrideManualDocGroupDiscounts = CopyQuoteInfo.Current.OverrideManualDocGroupDiscounts
                };
	            
				PXLongOperation.StartOperation(this, () => PXGraph.CreateInstance<QuoteMaint>().CopyToQuote(currentQuote, quoteFilterData));
						break;

					case CRQuoteTypeAttribute.Project:
						PM.PMQuoteMaint.CopyQuoteFilter pmQuoteFilterData = new PM.PMQuoteMaint.CopyQuoteFilter()
						{
							Description = CopyQuoteInfo.Current.Description,
							RecalculatePrices = CopyQuoteInfo.Current.RecalculatePrices,
							RecalculateDiscounts = CopyQuoteInfo.Current.RecalculateDiscounts,
							OverrideManualPrices = CopyQuoteInfo.Current.OverrideManualPrices,
							OverrideManualDiscounts = CopyQuoteInfo.Current.OverrideManualDiscounts,
							OverrideManualDocGroupDiscounts = CopyQuoteInfo.Current.OverrideManualDocGroupDiscounts
						};

						var pmGraph = PXGraph.CreateInstance<PM.PMQuoteMaint>();
						var pmQuote = pmGraph.Quote.Search<PM.PMQuote.quoteID>(currentQuote.QuoteID);
						PXLongOperation.StartOperation(this, () => pmGraph.CopyToQuote(pmQuote, pmQuoteFilterData));
						break;

					default:
						throw new PXException(Messages.UnsupportedQuoteType);
				}
            }

            return adapter.Get();
		}


		public PXAction<CROpportunity> ViewQuote;

		[PXUIField(DisplayName = Messages.ViewQuote, Visible = false)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
		public virtual IEnumerable viewQuote(PXAdapter adapter)
		{
			if (this.Quotes.Current != null)
		{
				var quote = this.Quotes.Current;
				this.Persist();
				
				switch (quote.QuoteType)
				{
					case CRQuoteTypeAttribute.Distribution:
						var quoteMaint = PXGraph.CreateInstance<QuoteMaint>();
						quoteMaint.Quote.Current = quoteMaint.Quote.Search<CRQuote.quoteNbr>(quote.QuoteNbr, quote.OpportunityID);
						if (quoteMaint.Quote.Current != null)
							PXRedirectHelper.TryRedirect(quoteMaint, PXRedirectHelper.WindowMode.InlineWindow);
						break;

					case CRQuoteTypeAttribute.Project:
						var pmQuoteMaint = PXGraph.CreateInstance<PM.PMQuoteMaint>();
						pmQuoteMaint.Quote.Current = pmQuoteMaint.Quote.Search<PM.PMQuote.quoteNbr>(quote.QuoteNbr);
						if (pmQuoteMaint.Quote.Current != null)
							PXRedirectHelper.TryRedirect(pmQuoteMaint, PXRedirectHelper.WindowMode.InlineWindow);
						break;

					default:
						throw new PXException(Messages.UnsupportedQuoteType);
				}
			}
			return adapter.Get();
		}

		public PXAction<CROpportunity> ViewProject;
		[PXUIField(DisplayName = Messages.ViewQuote, Visible = false)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
		public virtual IEnumerable viewProject(PXAdapter adapter)
		{
            int? projectID = this.Quotes.Current?.QuoteProjectID;

            if (projectID != null)
			{
				this.Persist();
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(projectID, PXRedirectHelper.WindowMode.InlineWindow);
			}
			return adapter.Get();
		}

		public PXAction<CROpportunity> validateAddresses;
        [PXUIField(DisplayName = CR.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select/*, FieldClass = CS.Messages.ValidateAddress*/)]
        [PXButton()]
        public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
        {
            foreach (CROpportunity current in adapter.Get<CROpportunity>())
            {
                if (current != null)
                {
                    CRAddress address = this.Opportunity_Address.Select();
					if (address != null && current.AllowOverrideContactAddress == true && address.IsValidated == false)
                        {
						PXAddressValidator.Validate<CRAddress>(this, address, true, true);
                    }

                    CRShippingAddress shipAddress = this.Shipping_Address.Select();
					if (shipAddress != null && current.BAccountID == null && current.ContactID == null && shipAddress.IsDefaultAddress is true)
                    {
						shipAddress.IsValidated = address.IsValidated;
						Shipping_Address.Cache.MarkUpdated(shipAddress);
                    }

					if (shipAddress != null && shipAddress.IsDefaultAddress is false && shipAddress.IsValidated == false)
                    {
						PXAddressValidator.Validate<CRShippingAddress>(this, shipAddress, true, true);
                    }

					CRBillingAddress billAddress = this.Billing_Address.Select();
					if (billAddress != null && current.BAccountID == null && current.ContactID == null && billAddress.IsDefaultAddress is true)
					{
						billAddress.IsValidated = address.IsValidated;
						Billing_Address.Cache.MarkUpdated(billAddress);
					}

					if (billAddress != null && billAddress.IsDefaultAddress is false && billAddress.IsValidated == false)
					{
						PXAddressValidator.Validate<CRBillingAddress>(this, billAddress, true, true);
					}
				}
                yield return current;
            }
        }

		#endregion

		#region Workflow Actions

		public PXAction<CROpportunity> Open;
		[PXButton, PXUIField(DisplayName = "Open", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable open(PXAdapter adapter) => adapter.Get<CROpportunity>();

		public PXAction<CROpportunity> OpenFromNew;
		[PXButton, PXUIField(DisplayName = "Open", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable openFromNew(PXAdapter adapter) => adapter.Get<CROpportunity>();

		public PXAction<CROpportunity> CloseAsWon;
		[PXButton, PXUIField(DisplayName = "Close as Won", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable closeAsWon(PXAdapter adapter)
		{
			foreach (CROpportunity current in adapter.Get<CROpportunity>())
			{
				if (current != null)
				{
					CROpportunity.Events.Select(ev => ev.OpportunityWon).FireOn(this, current);
					CROpportunity.Events.Select(ev => ev.OpportunityClosed).FireOn(this, current);
				}
				yield return current;
			}
		}

		public PXAction<CROpportunity> CloseAsLost;
		[PXButton, PXUIField(DisplayName = "Close as Lost", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable closeAsLost(PXAdapter adapter)
		{
			foreach (CROpportunity current in adapter.Get<CROpportunity>())
			{
				if (current != null)
				{
					CROpportunity.Events.Select(ev => ev.OpportunityLost).FireOn(this, current);
					CROpportunity.Events.Select(ev => ev.OpportunityClosed).FireOn(this, current);
				}
				yield return current;
			}
		}

		#endregion

		#region Event Handlers

		#region CROpportunity

		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CROpportunity.bAccountID> e) { }

		[PXDefault(typeof(Coalesce<
			Search<Customer.curyID, Where<Customer.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>, 
			Search<Branch.baseCuryID, Where<Branch.branchID, Equal<Current<CROpportunity.branchID>>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CROpportunity.curyID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(
			typeof(Search<Location.taxRegistrationID,
				Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>,
					And<Location.locationID, Equal<Current<CROpportunity.locationID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<CROpportunity.taxRegistrationID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(
			typeof(Search<Location.cAvalaraExemptionNumber,
				Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>,
					And<Location.locationID, Equal<Current<CROpportunity.locationID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<CROpportunity.externalTaxExemptionNumber> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(
			TXAvalaraCustomerUsageType.Default,
			typeof(Search<Location.cAvalaraCustomerUsageType,
				Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>,
					And<Location.locationID, Equal<Current<CROpportunity.locationID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<CROpportunity.avalaraCustomerUsageType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Default<CROpportunity.branchID>))]
		[PXFormula(typeof(Default<CROpportunity.locationID>))]
		protected virtual void _(Events.CacheAttached<CROpportunity.taxZoneID> e) { }

		protected virtual void CROpportunity_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CROpportunity;
			e.NewValue = GetDefaultTaxZone(row);
		}

		public virtual string GetDefaultTaxZone(CROpportunity row)
		{
			string result = null;

			if (row == null)
				return result;

			var customerLocation = (Location)PXSelect<Location,
				Where<Location.bAccountID, Equal<Required<CROpportunity.bAccountID>>,
					And<Location.locationID, Equal<Required<CROpportunity.locationID>>>>>.
				Select(this, row.BAccountID, row.LocationID);
			if (customerLocation != null)
			{
				if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
				{
					result = customerLocation.CTaxZoneID;
				}
				else
				{
					var address = (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(this, customerLocation.DefAddressID);
					if (address != null )
					{
						result = TaxBuilderEngine.GetTaxZoneByAddress(this, address);
					}
				}
			}
			if (customerLocation == null && result == null)
			{
				var branchLocation = (Location)PXSelectJoin<Location,
					InnerJoin<Branch, On<Branch.branchID, Equal<Current<CROpportunity.branchID>>>,
					InnerJoin<BAccount, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>>,
						Where<Location.locationID, Equal<BAccount.defLocationID>>>.Select(this);
				if (branchLocation != null && branchLocation.VTaxZoneID != null)
					result = branchLocation.VTaxZoneID;
				else
					result = row.TaxZoneID;
			}
			return result;
		}


		protected virtual void CROpportunity_ClassID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Setup.Current.DefaultOpportunityClassID;
		}

		protected virtual void CROpportunity_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CROpportunity;
			if (row == null || row.BAccountID == null) return;

			var baccount = (BAccount)PXSelect<BAccount, 
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(this, row.BAccountID);

			if (baccount != null)
			{
				e.NewValue = baccount.DefLocationID;
				e.Cancel = true;
			}
		}

		protected virtual void CROpportunity_DefQuoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = (CROpportunity)e.Row;
			if (row == null) return;
			e.NewValue = row.QuoteNoteID;
			e.Cancel = true;
		}

        protected virtual void CROpportunity_ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CROpportunity row = (CROpportunity)e.Row;
			if (row != null && row.CampaignSourceID != null)
			{
				CRCampaign campaign =
					(CRCampaign)PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(this.Opportunity.Cache, this.Opportunity.Current);
				var project =
					PXSelectorAttribute.Select<CROpportunity.projectID>(this.Opportunity.Cache, this.Opportunity.Current);
				if (campaign != null && campaign.ProjectID != null && project != null)
				{
					e.NewValue = campaign.ProjectID;
					e.Cancel = true;
				}
			}
		}
		protected virtual void CROpportunity_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateProductsTasks();
		}
	    protected virtual void CROpportunity_CampaignSourceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
	        CROpportunity row = (CROpportunity)e.Row;
	        if (row != null && row.ProjectID != null)
	        {
	            PXResultset<CRCampaign> result = 
                    PXSelect<CRCampaign, 
                    Where<CRCampaign.projectID, Equal<Required<CROpportunity.projectID>>>>
	                .SelectWindowed(this,0,2, row.ProjectID);

	            if (result.Count == 1)
	            {
	                CRCampaign rec = result[0];
	                e.NewValue = rec.CampaignID;
	            }
	        }
	    }
	    protected virtual void CROpportunity_CampaignSourceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
			UpdateProductsTasks();
		}
		protected virtual void UpdateProductsTasks()
        {
			if (this.Opportunity.Current?.CampaignSourceID != null)
			{
				CRCampaign campaign = (CRCampaign)PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(this.Opportunity.Cache, this.Opportunity.Current);
				if (campaign != null && campaign.ProjectID == this.Opportunity.Current.ProjectID)
				{
					foreach (var product in Products.Select())
					{
						Products.Cache.SetDefaultExt<CROpportunityProducts.projectID>(product);
						Products.Cache.SetDefaultExt<CROpportunityProducts.taskID>(product);
					}
				}
			}
		}

		protected virtual void UpdateProductsCostCodes(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = this.Opportunity.Current;
			if (row == null) return;

			foreach (CROpportunityProducts prod in Products.Select())
			{
				bool isUpdated = prod.ProjectID != row.ProjectID;

				try
				{
					PM.PMProject project;
					if (PXAccess.FeatureInstalled<FeaturesSet.costCodes>() && PM.ProjectDefaultAttribute.IsProject(this, row.ProjectID, out project))
					{
						if (project.BudgetLevel == PM.BudgetLevels.Task)
						{
							int CostCodeID = PM.CostCodeAttribute.GetDefaultCostCode();
							isUpdated = isUpdated || prod.CostCodeID != CostCodeID;
							prod.CostCodeID = CostCodeID;
						}
					}

					prod.ProjectID = row.ProjectID;

					if (isUpdated)
					{
						Products.Update(prod);
					}
				}
				catch (PXException ex)
				{
					PXFieldState projectIDState = (PXFieldState)sender.GetStateExt<CROpportunity.projectID>(row);
					Products.Cache.RaiseExceptionHandling<CROpportunityProducts.projectID>(prod, projectIDState.Value, ex);
				}
			}
		}

		protected virtual void CROpportunity_CloseDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CROpportunity row = e.Row as CROpportunity;
			if (row == null) return;

			if(PrimaryQuoteQuery.SelectSingle() == null)
			{
                Opportunity.Cache.SetValueExt<CROpportunity.documentDate>(row, row.CloseDate);
			}
		}
			
        protected virtual void CROpportunity_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXNoteAttribute.SetTextFilesActivitiesRequired<CROpportunityProducts.noteID>(Products.Cache, null);

			CROpportunity row = e.Row as CROpportunity;
            if (row == null) return;

			CRQuote primaryQt = PrimaryQuoteQuery.SelectSingle();

			if (row.PrimaryQuoteType == null)
				row.PrimaryQuoteType = primaryQt?.QuoteType ?? CRQuoteTypeAttribute.Distribution;

			if (primaryQt?.IsDisabled == true)
			{
				sender.RaiseExceptionHandling<CROpportunity.opportunityID>(row, row.OpportunityID,
					new PXSetPropertyException(Messages.QuoteSubmittedReadonly, PXErrorLevel.Warning));
			}

			foreach (var type in new[]
			{
				typeof(CROpportunityDiscountDetail),
				typeof(CROpportunityProducts),
				typeof(CRTaxTran),
			})
			{
				Caches[type].AllowInsert = Caches[type].AllowUpdate = Caches[type].AllowDelete = primaryQt?.IsDisabled != true;
			}

			var isSalesQuote = row.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution;

			if (!isSalesQuote)
			{
				row.ManualTotalEntry = false;
			}

			sender.Adjust<SharedRecordAttribute>(row)
				.For<CROpportunity.billContactID>(a => a.Required = isSalesQuote)
				.SameFor<CROpportunity.billAddressID>();

			Billing_Contact.Cache.Adjust<PXUIFieldAttribute>()
				.ForAllFields(a => a.Visible = isSalesQuote);

			Billing_Address.Cache.Adjust<PXUIFieldAttribute>()
				.ForAllFields(a => a.Visible = isSalesQuote);

			PXUIFieldAttribute.SetEnabled<CROpportunity.manualTotalEntry>(sender, row, primaryQt?.IsDisabled != true && row.PrimaryQuoteType != CRQuoteTypeAttribute.Project);

			PXUIFieldAttribute.SetEnabled<CROpportunity.classID>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyID>(sender, row, primaryQt?.IsDisabled != true && row.PrimaryQuoteType != CRQuoteTypeAttribute.Project);
			PXUIFieldAttribute.SetEnabled<CROpportunity.bAccountID>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.locationID>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyAmount>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyDiscTot>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.branchID>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.taxZoneID>(sender, row, primaryQt?.IsDisabled != true);
			
			PXUIFieldAttribute.SetEnabled<CROpportunity.taxCalcMode>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetVisible<CROpportunity.taxCalcMode>(sender, row, row.PrimaryQuoteType != CRQuoteTypeAttribute.Project);

			PXUIFieldAttribute.SetEnabled<CROpportunity.allowOverrideBillingContactAddress>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.allowOverrideShippingContactAddress>(sender, row, primaryQt?.IsDisabled != true);

			Caches[typeof(CRContact)].AllowUpdate = row.AllowOverrideContactAddress == true;
			PXUIFieldAttribute.SetEnabled(Caches[typeof(CRContact)], null, Caches[typeof(CRContact)].AllowUpdate);

			Caches[typeof(CRAddress)].AllowUpdate = row.AllowOverrideContactAddress == true;
			PXUIFieldAttribute.SetEnabled(Caches[typeof(CRAddress)], null, Caches[typeof(CRAddress)].AllowUpdate);

			if (row.BAccountID != null && PrimaryQuoteQuery.Current != null && PrimaryQuoteQuery.Current.Status != CRQuoteStatusAttribute.Draft)
			{
				PXUIFieldAttribute.SetEnabled<CROpportunity.bAccountID>(sender, row, false);
			}
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyAmount>(sender, e.Row, row.ManualTotalEntry == true);
			bool isCustomerDiscountsFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>();
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyDiscTot>(sender, e.Row, row.ManualTotalEntry == true || isCustomerDiscountsFeatureInstalled == false);
			PXUIFieldAttribute.SetEnabled<CROpportunity.opportunityID>(sender, e.Row, true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.allowOverrideContactAddress>(sender, row, !(row.BAccountID == null && row.ContactID == null));

			PXUIFieldAttribute.SetEnabled<CROpportunity.projectID>(sender, e.Row, row.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution);

			PXUIFieldAttribute.SetVisible<CROpportunity.curyID>(sender, row, IsMultyCurrency);

            decimal? curyWgtAmount = null;
			var oppProbability = row.StageID.
				With(_ => (CROpportunityProbability)PXSelect<CROpportunityProbability,
					Where<CROpportunityProbability.stageCode, Equal<Required<CROpportunityProbability.stageCode>>>>.
				Select(this, _));
			if (oppProbability != null && oppProbability.Probability != null)
                curyWgtAmount = oppProbability.Probability * (this.Accessinfo.CuryViewState ? row.ProductsAmount : row.CuryProductsAmount) / 100;
			row.CuryWgtAmount = curyWgtAmount;



			bool hasQuotes = primaryQt != null;

			createQuote
				.SetEnabled(Opportunity.Current.IsActive == true);

			primaryQuote
				.SetEnabled(hasQuotes && Opportunity.Current.IsActive == true);

			copyQuote
				.SetEnabled(hasQuotes && Opportunity.Current.IsActive == true);

			if (!UnattendedMode)
            {
                CRShippingAddress shipAddress = this.Shipping_Address.Select();
                CRBillingAddress billAddress = this.Billing_Address.Select();
                CRAddress contactAddress = this.Opportunity_Address.Select();
                bool enableAddressValidation = (shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
												|| (billAddress != null && billAddress.IsDefaultAddress == false && billAddress.IsValidated == false)
												|| (contactAddress != null && (contactAddress.IsDefaultAddress == false || row.BAccountID == null && row.ContactID == null) && contactAddress.IsValidated == false);
                validateAddresses
					.SetEnabled(enableAddressValidation);
            }

            PXUIFieldAttribute.SetVisible<CROpportunityProducts.curyUnitCost>(this.Products.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>() && PXAccess.FeatureInstalled<FeaturesSet.inventory>());
            PXUIFieldAttribute.SetVisible<CROpportunityProducts.vendorID>(this.Products.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.inventory>());
            PXUIFieldAttribute.SetVisible<CROpportunityProducts.pOCreate>(this.Products.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.inventory>());
		}

		protected virtual void CROpportunityRevision_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
			//Suppress update revision for opportunity, should be done by main DAC
			CROpportunityRevision row = (CROpportunityRevision)e.Row;
			if (row != null && this.Opportunity.Current != null &&
			    row.OpportunityID == this.Opportunity.Current.OpportunityID &&
			    row.NoteID == this.Opportunity.Current.DefQuoteID)
				e.Cancel = true;
		}

		protected virtual void CROpportunity_BAccountID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CROpportunity row = e.Row as CROpportunity;
			if (row == null) return;

			if (row.BAccountID < 0)
				e.ReturnValue = "";
		}

		protected virtual void CROpportunity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CROpportunity;
			if (row == null) return;

			object revisionNoteID;
			this.Caches[typeof(CROpportunity)].RaiseFieldDefaulting<CROpportunity.quoteNoteID>(row, out revisionNoteID);
			if (revisionNoteID != null)
				row.DefQuoteID = (Guid?)revisionNoteID;

			object newContactId = row.ContactID;
			if (newContactId != null && !VerifyField<CROpportunity.contactID>(row, newContactId))
				row.ContactID = null;

			if (row.ContactID != null)
			{
				object newCustomerId = row.BAccountID;
				if (newCustomerId == null)
					FillDefaultBAccountID(cache, row);
			}

			object newLocationId = row.LocationID;
			if (newLocationId == null || !VerifyField<CROpportunity.locationID>(row, newLocationId))
			{
				cache.SetDefaultExt<CROpportunity.locationID>(row);
			}

			if (row.ContactID == null)
				cache.SetDefaultExt<CROpportunity.contactID>(row);

			if (row.TaxZoneID == null) 
				cache.SetDefaultExt<CROpportunity.taxZoneID>(row);
		}


		protected virtual void CROpportunity_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
            var oldRow = e.OldRow as CROpportunity;
            var row = e.Row as CROpportunity;
            if (oldRow == null || row == null) return;

            if (row.ContactID != null && row.ContactID != oldRow.ContactID)
            {
                object newCustomerId = row.BAccountID;
                if (newCustomerId == null)
                    FillDefaultBAccountID(sender, row);
            }

            var customerChanged = row.BAccountID != oldRow.BAccountID;
			object newLocationId = row.LocationID;
			bool locationChanged = !sender.ObjectsEqual<CROpportunity.locationID>(e.Row, e.OldRow);
			if ((locationChanged || customerChanged) && (newLocationId == null || !VerifyField<CROpportunity.locationID>(row, newLocationId)))
			{
				sender.SetDefaultExt<CROpportunity.locationID>(row);
			}

		    bool campaignChanged = !sender.ObjectsEqual<CROpportunity.campaignSourceID>(e.Row, e.OldRow);
		    bool projectChanged = !sender.ObjectsEqual<CROpportunity.projectID>(e.Row, e.OldRow);

		    if (campaignChanged)
		    {
		        CRCampaign campaign =
		            (CRCampaign) PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(
                        this.Opportunity.Cache,
		                this.Opportunity.Current);

		        if (!projectChanged || campaign.ProjectID != row.ProjectID)
		            sender.SetDefaultExt<CROpportunity.projectID>(row);
		        projectChanged = sender.ObjectsEqual<CROpportunity.projectID>(e.Row, e.OldRow);
            }
		    else if(projectChanged)
		    {
		        CRCampaign campaign =
		            (CRCampaign)PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(this.Opportunity.Cache, this.Opportunity.Current);
		        if (campaign == null || campaign.ProjectID != row.ProjectID)
		        {
		            sender.SetDefaultExt<CROpportunity.campaignSourceID>(row);
                }		        
		    }
		    
			if (!projectChanged && customerChanged)
			{
				var project =
					PXSelectorAttribute.Select<CROpportunity.projectID>(this.Opportunity.Cache, this.Opportunity.Current);
				if (project == null && row.ProjectID != null)
				{
					row.ProjectID = null;
					sender.SetDefaultExt<CROpportunity.projectID>(row);
				}
			}

			var closeDateChanged = row.CloseDate != oldRow.CloseDate;
			
			if (locationChanged || closeDateChanged || projectChanged || customerChanged)
			{
				var productsCache = Products.Cache;
				foreach (CROpportunityProducts line in SelectProducts(row.QuoteNoteID))
				{
					var lineCopy = (CROpportunityProducts)productsCache.CreateCopy(line);
					lineCopy.ProjectID = row.ProjectID;
					lineCopy.CustomerID = row.BAccountID;
					productsCache.Update(lineCopy);
				}

				sender.SetDefaultExt<CROpportunity.taxCalcMode>(row);
			}

			if (locationChanged)
			{
				sender.SetDefaultExt<CROpportunity.taxZoneID>(row);
				sender.SetDefaultExt<CROpportunity.taxRegistrationID>(row);
				sender.SetDefaultExt<CROpportunity.externalTaxExemptionNumber>(row);
				sender.SetDefaultExt<CROpportunity.avalaraCustomerUsageType>(row);
			}

			if (row.OwnerID == null)
			{
				row.AssignDate = null;
			}
			else if (oldRow.OwnerID == null)
			{
				row.AssignDate = PXTimeZoneInfo.Now;
			}

		    if (IsExternalTax(Opportunity.Current.TaxZoneID) && (!sender.ObjectsEqual<CROpportunity.contactID, CROpportunity.taxZoneID, CROpportunity.branchID, CROpportunity.locationID,
                                                       CROpportunity.curyAmount, CROpportunity.shipAddressID>(e.Row, e.OldRow) || 
               (PrimaryQuoteQuery.SelectSingle() == null && !sender.ObjectsEqual<CROpportunity.closeDate>(e.Row, e.OldRow))))
		    {
                row.IsTaxValid = false;
		    }
        }

		protected virtual void CROpportunity_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (CROpportunity)e.Row;
			if (row == null) return;

			if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.BAccountID != null)
			{
				PXDefaultAttribute.SetPersistingCheck<CROpportunity.locationID>(sender, e.Row, PXPersistingCheck.NullOrBlank);				
			}
		}

		protected virtual void CROpportunity_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			CROpportunity row = e.Row as CROpportunity;
			if (row == null) return;

			if (e.Operation == PXDBOperation.Delete && e.TranStatus == PXTranStatus.Open)
			{
				var quoteIDs = new List<Guid?>();
				quoteIDs.Add(row.QuoteNoteID);
				foreach (CRQuote quote in this.Quotes.View.SelectMultiBound(new object[] {row}))
				{
					PXDatabase.Delete<EP.EPApproval>(new PXDataFieldRestrict<EP.EPApproval.refNoteID>(quote.NoteID));
					quoteIDs.Add(quote.QuoteID);
				}
				foreach (var quoteId in quoteIDs)
				{
					PXDatabase.Delete<Standalone.CROpportunityRevision>(new PXDataFieldRestrict<Standalone.CROpportunityRevision.noteID>(quoteId));
					PXDatabase.Delete<Standalone.CRQuote>(new PXDataFieldRestrict<Standalone.CRQuote.quoteID>(quoteId));
				}
			}
		}
		#endregion

		#region CROpportunityProducts

		protected virtual void CROpportunityProducts_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CROpportunityProducts;
			if (row == null) return;

			bool autoFreeItem = row.ManualDisc != true && row.IsFree == true;

			if (autoFreeItem)
			{
				PXUIFieldAttribute.SetEnabled<CROpportunityProducts.taxCategoryID>(sender, e.Row);
				PXUIFieldAttribute.SetEnabled<CROpportunityProducts.descr>(sender, e.Row);
			}

            PXUIFieldAttribute.SetEnabled<CROpportunityProducts.vendorID>(sender, row, row.POCreate == true);
			PXUIFieldAttribute.SetEnabled<CROpportunityProducts.skipLineDiscounts>(sender, row, this.IsCopyPasteContext);
        }
		protected virtual void CROpportunityProducts_TaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Opportunity.Current.CampaignSourceID != null)
			{
				CRCampaign campaign =
					(CRCampaign)PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(this.Opportunity.Cache, this.Opportunity.Current);
				if (campaign != null && campaign.ProjectID == this.Opportunity.Current.ProjectID)
				{
					e.NewValue = campaign.ProjectTaskID;
					e.Cancel = true;
				}
			}
		}		
        protected virtual void CROpportunityProducts_VendorID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
                return;

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;

            if (crOpportunityProductsRow.POCreate == false || crOpportunityProductsRow.InventoryID == null)
            {
                e.Cancel = true;
            }
        }
        protected virtual void CROpportunityProducts_POCreate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
                return;

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;

            sender.SetDefaultExt<CROpportunityProducts.vendorID>(crOpportunityProductsRow);
        }
		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts, CROpportunityProducts.pOCreate> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.InventoryID != null && e.Row.SiteID != null)
			{
				bool dropShipmentsEnabled = PXAccess.FeatureInstalled<FeaturesSet.dropShipments>();
				bool soToPOLinkEnabled = PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>();
				INItemSiteSettings itemSettings = initemsettings.SelectSingle(e.Row.InventoryID, e.Row.SiteID);

				if (itemSettings.ReplenishmentSource == INReplenishmentSource.DropShipToOrder && dropShipmentsEnabled
					|| itemSettings.ReplenishmentSource == INReplenishmentSource.PurchaseToOrder && soToPOLinkEnabled)
				{
					e.NewValue = true;
					e.Cancel = true;
					return;
				}

				INItemSite inItemSite = PXSelect<INItemSite,
							Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
								And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>
								.Select(e.Cache.Graph, e.Row.InventoryID, e.Row.SiteID);

				if (inItemSite != null)
				{
					INReplenishmentClass inReplenishmentClass = INReplenishmentClass.PK.Find(e.Cache.Graph, inItemSite.ReplenishmentClassID);

					if (inItemSite.ReplenishmentSource == INReplenishmentSource.DropShipToOrder &&
						inReplenishmentClass?.ReplenishmentSource == INReplenishmentSource.Purchased &&
						PXAccess.FeatureInstalled<FeaturesSet.dropShipments>())
					{
						e.NewValue = true;
						return;
					}
				}
			}
		}
		protected virtual void CROpportunityProducts_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
		    if (IsExternalTax(Opportunity.Current.TaxZoneID))
		    {
		        Opportunity.Current.IsTaxValid = false;
		        Opportunity.Update(Opportunity.Current);
            }
		}
		protected virtual void CROpportunityProducts_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
		    if (IsExternalTax(Opportunity.Current.TaxZoneID))
		    {
		        Opportunity.Current.IsTaxValid = false;
		        Opportunity.Update(Opportunity.Current);
            }
        }
	    protected virtual void CROpportunityProducts_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
	    {
	        if (!IsExternalTax(Opportunity.Current.TaxZoneID)) return;
	        
	        Opportunity.Cache.SetValue(Opportunity.Current, typeof(CROpportunity.isTaxValid).Name, false);
	    }
        protected virtual void CROpportunityProducts_IsFree_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CROpportunityProducts row = e.Row as CROpportunityProducts;
            if (row == null) return;

            if (row.InventoryID != null && row.IsFree == false)
            {
                Caches[typeof(CROpportunityProducts)].SetDefaultExt<CROpportunityProducts.curyUnitPrice>(row);
            }
        }

        [PopupMessage]
        [PXRestrictor(typeof(Where<
            InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
            And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
            And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>>>), IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<CROpportunityProducts.inventoryID> e) { }

        protected virtual void CROpportunityProducts_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
                return;

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;

            sender.SetValueExt<CROpportunityProducts.pOCreate>(crOpportunityProductsRow, false);

			sender.SetDefaultExt<CROpportunityProducts.pOCreate>(e.Row);
        }

        [PXUIField(DisplayName = "Manual Price", Visible = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CROpportunityProducts_ManualPrice_CacheAttached(PXCache e)
        {
        }

		protected virtual void _(Events.FieldDefaulting<ARTran, CROpportunityProducts.costCodeID> e)
		{
			PM.PMProject project;
			if (PM.CostCodeAttribute.UseCostCode() && PM.ProjectDefaultAttribute.IsProject(this, e.Row.ProjectID, out project))
			{
				if (project.BudgetLevel == PM.BudgetLevels.Task)
				{
					e.NewValue = PM.CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictSiteByBranch(typeof(CROpportunity.branchID), ShowWarning = true)]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.siteID> e)
		{
		}
		#endregion

		#region CreateQuoteFilter

		protected virtual void CreateQuotesFilter_AddProductsFromOpportunity_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
	    {
	        var row = e.Row as CreateQuotesFilter;
	        if (row == null) return;

            e.NewValue = Products.SelectSingle() != null && row.QuoteType == CRQuoteTypeAttribute.Distribution && Opportunity.Current.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution;
            e.Cancel = true;
	    }

		protected virtual void CreateQuotesFilter_QuoteType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CreateQuotesFilter;
			if (row == null) return;
			e.NewValue = (SalesQuotesInstalled) ? CRQuoteTypeAttribute.Distribution : CRQuoteTypeAttribute.Project;
            e.Cancel = true;
	    }

        protected virtual void CreateQuotesFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
	    {
	        CreateQuotesFilter row = e.Row as CreateQuotesFilter;
	        if (row == null) return;

            if (!row.RecalculatePrices.GetValueOrDefault())
	        {
	            QuoteInfo.Cache.SetValue<CreateQuotesFilter.overrideManualPrices>(row, false);
	        }
	        if (!row.RecalculateDiscounts.GetValueOrDefault())
	        {
	            QuoteInfo.Cache.SetValue<CreateQuotesFilter.overrideManualDiscounts>(row, false);
                QuoteInfo.Cache.SetValue<CreateQuotesFilter.overrideManualDocGroupDiscounts>(row, false);
	        }
	        if (row.QuoteType == CRQuoteTypeAttribute.Project)
	        {
	            QuoteInfo.Cache.SetValue<CreateQuotesFilter.addProductsFromOpportunity>(row, false);
	        }
	    }

        protected virtual void CreateQuotesFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	    {
	        CreateQuotesFilter row = e.Row as CreateQuotesFilter;
	        if (row == null) return;

	        bool hasProducts = Products.SelectSingle() != null;
	        bool hasQuotes = Quotes.SelectSingle() != null;
			var isDistributionQuote = row.QuoteType == CRQuoteTypeAttribute.Distribution;
			var isProjectQuote = row.QuoteType == CRQuoteTypeAttribute.Project;
			var currentQuoteIsDistribution = Opportunity.Current.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution;

			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.quoteType>(sender, row, ProjectQuotesInstalled && SalesQuotesInstalled);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.addProductsFromOpportunity>(sender, row, hasProducts && hasQuotes && isDistributionQuote && currentQuoteIsDistribution);
	        PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.makeNewQuotePrimary>(sender, row, hasQuotes);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.recalculatePrices>(sender, row, isDistributionQuote);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.overrideManualPrices>(sender, row, row.RecalculatePrices == true && isDistributionQuote);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.recalculateDiscounts>(sender, row, isDistributionQuote);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.overrideManualDiscounts>(sender, row, row.RecalculateDiscounts == true && isDistributionQuote);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalculateDiscounts == true);

			if (row.QuoteType == CRQuoteTypeAttribute.Project)
			{
				if (!PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() && OpportunityCurrent.Current.CuryID != (Accessinfo.BaseCuryID ?? new PXSetup<PX.Objects.GL.Company>(this).Current?.BaseCuryID))
				sender.RaiseExceptionHandling<CreateQuotesFilter.quoteType>(row, row.QuoteType,
					new PXSetPropertyException(Messages.CannotCreateProjectQuoteBecauseOfCury, PXErrorLevel.Error));

				if (Opportunity.Current.ManualTotalEntry == true)
					sender.RaiseExceptionHandling<CreateQuotesFilter.quoteType>(row, row.QuoteType,
						new PXSetPropertyException(Messages.ManualAmountWillBeCleared, PXErrorLevel.Warning));
			}
			else    
			{
				sender.RaiseExceptionHandling<CreateQuotesFilter.quoteType>(row, row.QuoteType, null);
			}
        }
        #endregion

        #region QuoteFilter
        protected virtual void CopyQuoteFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
	    {
	        CopyQuoteFilter row = e.Row as CopyQuoteFilter;
	        if (row == null) return;

	        if (row.RecalculatePrices != true)
	        {
	            CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualPrices>(row, false);
	        }
	        if (row.RecalculateDiscounts != true)
	        {
	            CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualDiscounts>(row, false);
				CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualDocGroupDiscounts>(row, false);
	        }
	    }

	    protected virtual void CopyQuoteFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	    {
	        CopyQuoteFilter row = e.Row as CopyQuoteFilter;
	        if (row == null) return;

			PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.recalculatePrices>(sender, row, Quotes.Current?.QuoteType == CRQuoteTypeAttribute.Distribution);
	        PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualPrices>(sender, row, row.RecalculatePrices == true);
			PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.recalculateDiscounts>(sender, row, Quotes.Current?.QuoteType == CRQuoteTypeAttribute.Distribution);
	        PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualDiscounts>(sender, row, row.RecalculateDiscounts == true);
			PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalculateDiscounts == true);
	    }
		#endregion

		#region CRContact / CRAddress

		public virtual void _(Events.RowSelected<CRShippingContact> e)
		{
			SetDisabledItemIfQuoteDisabled(e.Cache, e.Row);
		}

		public virtual void _(Events.RowSelected<CRShippingAddress> e)
		{
			SetDisabledItemIfQuoteDisabled(e.Cache, e.Row);
		}

		public virtual void _(Events.RowSelected<CRBillingContact> e)
		{
			SetDisabledItemIfQuoteDisabled(e.Cache, e.Row);
		}

		public virtual void _(Events.RowSelected<CRBillingAddress> e)
		{
			SetDisabledItemIfQuoteDisabled(e.Cache, e.Row);
		}

		private void SetDisabledItemIfQuoteDisabled(PXCache cache, object row)
		{
			if (row == null)
				return;

			var disabled = PrimaryQuoteQuery.SelectSingle()?.IsDisabled == true;
			if (disabled)
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
			}
		}

		#endregion

		#region UDF Attributes

		public virtual void _(Events.FieldSelecting<PopupUDFAttributes, PopupUDFAttributes.value> e)
		{
			var row = e.Row;
			if (row == null)
				return;
			CreateQuotesFilter quoteFilter = (CreateQuotesFilter)this.Caches<CreateQuotesFilter>()?.Current;
			PXFieldState state;
			Type targetGrapthType = typeof(QuoteMaint);
			if (quoteFilter?.QuoteType == CRQuoteTypeAttribute.Project)
			{
				targetGrapthType = typeof(PM.PMQuoteMaint);
			}
			string screenID = PXSiteMap.Provider.FindSiteMapNode(targetGrapthType)?.ScreenID;
			if (row == null || screenID == null || !screenID.Equals(row.ScreenID))
				return;
			state = UDFHelper.GetGraphUDFFieldState(targetGrapthType, row.AttributeID);

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

		#endregion

		#region CacheAttached

		[CRMBAccount(bAccountTypes: new[]
		{
			typeof(BAccountType.prospectType),
			typeof(BAccountType.customerType),
			typeof(BAccountType.combinedType),
			typeof(BAccountType.vendorType),
		})]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), Messages.BAccountCD)]
		protected virtual void _(Events.CacheAttached<BAccount.acctCD> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), Messages.BAccountName)]
		protected virtual void _(Events.CacheAttached<BAccount.acctName> e) { }

		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>>),
	        DisplayName = "Location",
	        DescriptionField = typeof(Location.descr),
	        BqlField = typeof(Standalone.CROpportunityRevision.locationID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
	    public virtual void CROpportunity_LocationID_CacheAttached(PXCache sender) { }

		[PXDefault(false)]
        [PXDBCalced(typeof(True), typeof(Boolean))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void BAccountR_ViewInCrm_CacheAttached(PXCache sender) { }

	    [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.opportunityContactID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void CRQuote_OpportunityContactID_CacheAttached(PXCache sender) { }

	    [PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void CROpportunityRevision_OpportunityContactID_CacheAttached(PXCache sender) { }

	    [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.opportunityAddressID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void CRQuote_OpportunityAddressID_CacheAttached(PXCache sender) { }

	    [PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void CROpportunityRevision_OpportunityAddressID_CacheAttached(PXCache sender) { }

		[PXDBUShort()]
		[PXLineNbr(typeof(CROpportunity))]
		protected virtual void CROpportunityDiscountDetail_LineNbr_CacheAttached(PXCache e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		protected virtual void _(Events.CacheAttached<CRShippingAddress.latitude> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		protected virtual void _(Events.CacheAttached<CRShippingAddress.longitude> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Business Account")]
		protected virtual void _(Events.CacheAttached<CRQuote.bAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Contact")]
		protected virtual void _(Events.CacheAttached<CRQuote.contactID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Location")]
		protected virtual void _(Events.CacheAttached<CRQuote.locationID> e) { }
		#endregion

		#region Private Methods

		private bool VerifyField<TField>(object row, object newValue)
			where TField : IBqlField
		{
			if (row == null) return true;

			var result = false;
			var cache = Caches[row.GetType()];
			try
			{
				result = cache.RaiseFieldVerifying<TField>(row, ref newValue);
			}
			catch (StackOverflowException) { throw; }
			catch (OutOfMemoryException) { throw; }
			catch (Exception) { }

			return result;
		}

		private void FillDefaultBAccountID(PXCache cache, CROpportunity row)
		{
			if (row == null) return;

			if (row.ContactID != null)
			{
				var contact = (Contact)PXSelectReadonly<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
					Select(this, row.ContactID);
				if (contact != null)
				{
					row.BAccountID = contact.BAccountID;
					row.ParentBAccountID = contact.ParentBAccountID;
					cache.SetDefaultExt<CROpportunity.locationID>(row);
				}
			}

		}

		private bool IsMultyCurrency
		{
			get { return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>(); }
		}

		private IEnumerable SelectProducts(object quoteId)
		{
			if (quoteId == null)
				return new CROpportunityProducts[0];

			return PXSelect<CROpportunityProducts,
				Where<CROpportunityProducts.quoteID, Equal<Required<CROpportunity.quoteNoteID>>>>.
				Select(this, quoteId).
				RowCast<CROpportunityProducts>();
		}

        private Contact FillFromOpportunityContact(Contact Contact)
        {
            CRContact _CRContact = Opportunity_Contact.SelectSingle();

            Contact.FullName = _CRContact.FullName;
            Contact.Title = _CRContact.Title;
            Contact.FirstName = _CRContact.FirstName;
            Contact.LastName = _CRContact.LastName;
            Contact.Salutation = _CRContact.Salutation;
            Contact.Attention = _CRContact.Attention;
            Contact.EMail = _CRContact.Email;
            Contact.WebSite = _CRContact.WebSite;
            Contact.Phone1 = _CRContact.Phone1;
            Contact.Phone1Type = _CRContact.Phone1Type;
            Contact.Phone2 = _CRContact.Phone2;
            Contact.Phone2Type = _CRContact.Phone2Type;
            Contact.Phone3 = _CRContact.Phone3;
            Contact.Phone3Type = _CRContact.Phone3Type;
            Contact.Fax = _CRContact.Fax;
            Contact.FaxType = _CRContact.FaxType;
            return Contact;
        }
        private Address FillFromOpportunityAddress(Address Address)
        {
            CRAddress _CRAddress = Opportunity_Address.SelectSingle();

            Address.AddressLine1 = _CRAddress.AddressLine1;
            Address.AddressLine2 = _CRAddress.AddressLine2;
            Address.City = _CRAddress.City;
            Address.CountryID = _CRAddress.CountryID;
            Address.State = _CRAddress.State;
            Address.PostalCode = _CRAddress.PostalCode;
            return Address;
        }

		private bool IsSingleQuote(string opportunityId)
		{
			var quote = PXSelect<CRQuote, Where<CRQuote.opportunityID, Equal<Optional<CRQuote.opportunityID>>>>.SelectSingleBound(this, null, opportunityId);
			return (quote.Count == 0);
		}

        #endregion

		#region External Tax Provider

		public virtual bool IsExternalTax(string taxZoneID)
		{
			return false;
		}

		public virtual CROpportunity CalculateExternalTax(CROpportunity opportunity)
		{
			return opportunity;
		}

		#endregion

		#region Entity Event Handlers
		public PXWorkflowEventHandler<CROpportunity> OnOpportunityCreatedFromLead;
		public PXWorkflowEventHandler<CROpportunity> OnOpportunityLost;
		public PXWorkflowEventHandler<CROpportunity> OnOpportunityWon;
		public PXWorkflowEventHandler<CROpportunity> OnOpportunityClosed;
		#endregion

		public override void Persist()
		{
		    base.Persist();
			this.Quotes.Cache.Clear();
			this.Quotes.View.Clear();
			this.Quotes.Cache.ClearQueryCache();
			this.Quotes.View.RequestRefresh();
		}

		#region Implementation of IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (nameof(this.Products).Equals(viewName, StringComparison.OrdinalIgnoreCase))
			{
				// if Insert New Rows specified - just set available value
				if (PX.Common.PXExecutionContext.Current.Bag
						.TryGetValue(PXImportAttribute._DONT_UPDATE_EXIST_RECORDS, out object dontUpdateObj)
					&& dontUpdateObj is true)
				{
					keys[nameof(CROpportunityProducts.lineNbr)] = Opportunity.Current.ProductCntr + 1;
				}
				// Only if "Inventory ID" and "site ID" columns imported
				else if (values.Contains(nameof(CROpportunityProducts.inventoryID)) && values.Contains(nameof(CROpportunityProducts.siteID)))
				{
					Guid? quoteId = Opportunity.Current.QuoteNoteID;
					string inventoryCD = (string)values[nameof(CROpportunityProducts.inventoryID)];
					string siteCD = (string)values[nameof(CROpportunityProducts.siteID)];
					CROpportunityProducts product = null;

					// Find first product already added with same inventory code and site ID
					// and use its keys to update
					if (quoteId != null && !String.IsNullOrEmpty(inventoryCD) && !String.IsNullOrEmpty(siteCD))
					{
						product = ProductsByQuoteIDAndInventoryCD.SelectSingle(quoteId, inventoryCD, siteCD);
					}
					if (product != null)
					{
						keys[nameof(CROpportunityProducts.quoteID)] = product.QuoteID;
						keys[nameof(CROpportunityProducts.lineNbr)] = product.LineNbr;
					}
					else
					{
						keys[nameof(CROpportunityProducts.quoteID)] = null;
						keys[nameof(CROpportunityProducts.lineNbr)] = null;
					}
				}
			}

			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
		}

		#endregion

        #region Extensions
        /// <exclude/>
        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class MultiCurrency : CRMultiCurrencyGraph<OpportunityMaint, CROpportunity>
	    {	       
			#region Initialization

            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CROpportunity)) { DocumentDate = typeof(CROpportunityRevision.documentDate) };
	        }

			#endregion

			#region Overrides

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.Opportunity,
					Base.Products,
					Base.TaxLines,
					Base.Taxes,
					Base.GetExtension<Discount>().DiscountDetails
				};
			}

			protected override BAccount GetRelatedBAccount() => CROpportunity.FK.BusinessAccount.Dirty.FindParent(Base, Base.Opportunity.Current);

			protected override Type BAccountField => typeof(CROpportunity.bAccountID);

			protected override PXView DetailsView => Base.Products.View;

			protected override CurySource CurrentSourceSelect()
			{
				var primaryQt = Base.PrimaryQuoteQuery.SelectSingle();
				if (primaryQt == null)
				{
					return base.CurrentSourceSelect();
				}

				var result = new CurySource();
				if (primaryQt.QuoteType != CRQuoteTypeAttribute.Project)
				{
					result = base.CurrentSourceSelect();
					if (primaryQt.Status != CRQuoteStatusAttribute.Draft)
					{
						result.AllowOverrideCury = false;
						result.AllowOverrideRate = false;
					}
					return result;
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() && Base.customer.Current != null)
				{
					result.CuryID = Base.customer.Current.CuryID;
					result.CuryRateTypeID = Base.customer.Current.CuryRateTypeID;
				}

				if (primaryQt.Status == CRQuoteStatusAttribute.Draft)
				{
					if (Base.customer.Current != null)
					{
						result.AllowOverrideCury = Base.customer.Current.AllowOverrideCury;
						result.AllowOverrideRate = Base.customer.Current.AllowOverrideRate;
					}
					else
					{
						result.AllowOverrideCury = true;
						result.AllowOverrideRate = true;
					}
				}
				else
				{
					result.AllowOverrideCury = false;
					result.AllowOverrideRate = false;
				}

				return result;
			}

			#endregion
		}

        /// <exclude/>
        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class SalesPrice : SalesPriceGraph<OpportunityMaint, CROpportunity>
	    {
			#region Initialization

			protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CROpportunity));
            }
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyExtPrice), Descr = typeof(CROpportunityProducts.descr)};
            }
            protected override PriceClassSourceMapping GetPriceClassSourceMapping()
            {
                return new PriceClassSourceMapping(typeof(Location)) {PriceClassID = typeof(Location.cPriceClassID)};
            }

            #endregion
        }

        /// <exclude/>
        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class Discount : DiscountGraph<OpportunityMaint, CROpportunity>
        {
            #region Initialization

            public override void Initialize()
            {
                base.Initialize();
                if(this.Discounts == null)
                    this.Discounts = new PXSelectExtension<PX.Objects.Extensions.Discount.Discount>(this.DiscountDetails);
			}
            protected override DocumentMapping GetDocumentMapping()
            {
				return new DocumentMapping(typeof(CROpportunity)){ CuryDiscTot = typeof(CROpportunity.curyLineDocDiscountTotal), DocumentDate = typeof(CROpportunity.documentDate) };
			}
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyAmount), Quantity = typeof(CROpportunityProducts.quantity)};
            }
            protected override DiscountMapping GetDiscountMapping()
            {
                return new DiscountMapping(typeof(CROpportunityDiscountDetail));
            }

            #endregion

            #region Views

            [PXViewName(Messages.DiscountDetails)]
            public PXSelect<CROpportunityDiscountDetail,
				Where<CROpportunityDiscountDetail.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
				OrderBy<Asc<CROpportunityDiscountDetail.lineNbr>>>
                      DiscountDetails;

            #endregion

            #region Events

            [PXSelector(typeof(Search<ARDiscount.discountID, 
                Where<ARDiscount.type, NotEqual<DiscountType.LineDiscount>, 
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouse>, 
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomer>,
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomerPrice>, 
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventory>, 
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventoryPrice>>>>>>>>))]
            [PXMergeAttributes]
            public override void Discount_DiscountID_CacheAttached(PXCache sender)
            {
            }
            
			[PXMergeAttributes]
            [CurrencyInfo(typeof(CROpportunity.curyInfoID))]
            public override void Discount_CuryInfoID_CacheAttached(PXCache sender)
            {
            }

            protected virtual void Discount_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
            {
                if (Base.IsExternalTax(Base.Opportunity.Current.TaxZoneID))
                {
                    Base.Opportunity.Current.IsTaxValid = false;
                }
            }
            protected virtual void Discount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
            {
                if (Base.IsExternalTax(Base.Opportunity.Current.TaxZoneID))
                {
                    Base.Opportunity.Current.IsTaxValid = false;
                }
            }
            protected virtual void Discount_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
            {
                if (Base.IsExternalTax(Base.Opportunity.Current.TaxZoneID))
                {
                    Base.Opportunity.Current.IsTaxValid = false;
                }
            }


            protected virtual void _(Events.RowSelected<PX.Objects.Extensions.Discount.Document> e)
            {
                var opportunity = e.Cache.GetMain(e.Row) as CROpportunity;
                CRQuote primaryQt = Base.PrimaryQuoteQuery.SelectSingle();

                this.recalculatePrices.SetEnabled(opportunity.IsActive == true
                    && opportunity.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution
                    && primaryQt?.IsDisabled != true);
            }

            #endregion

            #region Actions

            public PXAction<CROpportunity> recalculatePrices;

            [PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
            [PXButton]
            public virtual IEnumerable RecalculatePrices(PXAdapter adapter)
            {
                List<CROpportunity> opportunities = new List<CROpportunity>(adapter.Get<CROpportunity>());
                foreach (CROpportunity opportunity in opportunities)
                {
                    if (recalcdiscountsfilter.View.Answer == WebDialogResult.None)
                    {
                        recalcdiscountsfilter.Cache.Clear();
                        RecalcDiscountsParamFilter filterdata = recalcdiscountsfilter.Cache.Insert() as RecalcDiscountsParamFilter;
                        if (filterdata != null)
                        {
                            filterdata.RecalcUnitPrices = true;
                            filterdata.RecalcDiscounts = true;
                            filterdata.OverrideManualPrices = false;
                            filterdata.OverrideManualDiscounts = false;
							filterdata.OverrideManualDocGroupDiscounts = false;
                        }
                    }

                    if (recalcdiscountsfilter.AskExt() != WebDialogResult.OK)
                        return opportunities;

                    RecalculateDiscountsAction(adapter);
                }

                return opportunities;
            }

			#endregion

			#region Overrides

			protected override bool AddDocumentDiscount => true;

			protected override void DefaultDiscountAccountAndSubAccount(PX.Objects.Extensions.Discount.Detail det)
			{
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class SalesTax : TaxGraph<OpportunityMaint, CROpportunity>
	    {
            #region Initialization

            protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }

			protected override PXView DocumentDetailsView => Base.Products.View;

			protected override DocumentMapping GetDocumentMapping()
            {
	            return new DocumentMapping(typeof(CROpportunity))
	            {
                    BranchID = typeof(CROpportunity.branchID),
                    CuryID = typeof(CROpportunity.curyID),
                    CuryInfoID = typeof(CROpportunity.curyInfoID),
                    DocumentDate = typeof(CROpportunity.documentDate),
                    //FinPeriodID = null,
                    TaxZoneID = typeof(CROpportunity.taxZoneID),
                    TermsID = typeof(CROpportunity.termsID),
                    CuryLinetotal = typeof(CROpportunity.curyLineTotal),
                    CuryDiscountLineTotal = typeof(CROpportunity.curyLineDiscountTotal),
                    CuryExtPriceTotal = typeof(CROpportunity.curyExtPriceTotal),
                    CuryDocBal = typeof(CROpportunity.curyProductsAmount),
                    CuryTaxTotal = typeof(CROpportunity.curyTaxTotal),
                    CuryDiscTot = typeof(CROpportunity.curyDiscTot),
                    //CuryDiscAmt = null,
                    //CuryOrigWhTaxAmt = null,
                    //CuryTaxRoundDiff = null,
                    //TaxRoundDiff = null,
                    //IsTaxSaved = null,
                    TaxCalcMode = typeof(CROpportunity.taxCalcMode)
				};
            }
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts))
                {
                    CuryInfoID = typeof(CROpportunityProducts.curyInfoID),
                    TaxCategoryID = typeof(CROpportunityProducts.taxCategoryID),
                    GroupDiscountRate = typeof(CROpportunityProducts.groupDiscountRate),
                    DocumentDiscountRate = typeof(CROpportunityProducts.documentDiscountRate),
                    CuryTranAmt = typeof(CROpportunityProducts.curyAmount),
                    CuryTranDiscount = typeof(CROpportunityProducts.curyDiscAmt),
                    CuryTranExtPrice = typeof(CROpportunityProducts.curyExtPrice)
                };
            }

	        protected override TaxDetailMapping GetTaxDetailMapping()
	        {
	            return new TaxDetailMapping(typeof(CROpportunityTax), typeof(CROpportunityTax.taxID));
	        }
            protected override TaxTotalMapping GetTaxTotalMapping()
            {
                return new TaxTotalMapping(typeof(CRTaxTran), typeof(CRTaxTran.taxID));
            }	       

            #endregion

            #region Events
			protected virtual void _(Events.FieldUpdated<CROpportunity, CROpportunity.curyDiscTot> e)
			{
				if (e.Row != null && e.Row.ManualTotalEntry == false)
				{
					bool isCustomerDiscountsFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>();
					if (isCustomerDiscountsFeatureInstalled == false)
					{
						CalcTotals(null, false);
					}
				}
			}

	        protected virtual void _(Events.FieldUpdated<CROpportunity, CROpportunity.manualTotalEntry> e)
	        {
	            if (e.Row != null && e.Row.ManualTotalEntry == false)
	            {
	                CalcTotals(null, false);
	            }
	        }

		protected virtual void Document_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	        {
	            var row = sender.GetExtension<PX.Objects.Extensions.SalesTax.Document>(e.Row);
                if(row == null)
                    return;

                if(row.TaxCalc == null)
                    row.TaxCalc = TaxCalc.Calc;	            
	        }

            #endregion

            #region Overrides

            protected override void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
            {
                base.CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);
                CROpportunity doc = (CROpportunity )this.Documents.Cache.GetMain<PX.Objects.Extensions.SalesTax.Document>(this.Documents.Current);
	            bool manualEntry = doc.ManualTotalEntry == true;
				decimal CuryManualAmount = (decimal)(doc.CuryAmount ?? 0m);
                decimal CuryManualDiscTot = (decimal)(doc.CuryDiscTot ?? 0m);
                decimal CuryLineTotal = (decimal)(ParentGetValue<CROpportunity.curyLineTotal>() ?? 0m);
	            decimal CuryDiscAmtTotal = (decimal)(ParentGetValue<CROpportunity.curyLineDiscountTotal>() ?? 0m);
	            decimal CuryExtPriceTotal = (decimal)(ParentGetValue<CROpportunity.curyExtPriceTotal>() ?? 0m);
				decimal CuryDiscTotal = (decimal)(ParentGetValue<CROpportunity.curyDiscTot>() ?? 0m);

				decimal CuryDocTotal =
	                manualEntry 
					? CuryManualAmount - CuryManualDiscTot
                    : CuryLineTotal - CuryDiscTotal + CuryTaxTotal - CuryInclTaxTotal;

                if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<CROpportunity.curyProductsAmount>() ?? 0m)) == false)
                {
                    ParentSetValue<CROpportunity.curyProductsAmount>(CuryDocTotal);
                }
            }

            protected override string GetExtCostLabel(PXCache sender, object row)
            {
                return ((PXDecimalState)sender.GetValueExt<CROpportunityProducts.curyExtPrice>(row)).DisplayName;
            }

            protected override void SetExtCostExt(PXCache sender, object child, decimal? value)
            {
                var row = child as PX.Data.PXResult<PX.Objects.Extensions.SalesTax.Detail>;
                if (row != null)
                {
                    var det = PXResult.Unwrap<PX.Objects.Extensions.SalesTax.Detail>(row);
                    var line = (CROpportunityProducts)det.Base;
                    line.CuryExtPrice = value;
                    sender.Update(row);
                }
            }

            protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
            {
                Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
                var currents = new[]
	            {
		            row != null && row is PX.Objects.Extensions.SalesTax.Detail ? Details.Cache.GetMain((PX.Objects.Extensions.SalesTax.Detail)row):null,
					((OpportunityMaint)graph).Opportunity.Current
				};

				IComparer<Tax> taxComparer = GetTaxByCalculationLevelComparer();
				taxComparer.ThrowOnNull(nameof(taxComparer));

                foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
                    LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
                        And<TaxRev.outdated, Equal<boolFalse>,
                            And<TaxRev.taxType, Equal<TaxType.sales>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                            And<Tax.taxType, NotEqual<CSTaxType.use>,
                            And<Tax.reverseTax, Equal<boolFalse>,
                            And<Current<CROpportunity.documentDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
                    Where>
                    .SelectMultiBound(graph, currents, parameters))
                {
                    tail[((Tax)record).TaxID] = record;
                }
                List<object> ret = new List<object>();
                switch (taxchk)
                {
                    case PXTaxCheck.Line:
                        foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
                            Where<CROpportunityTax.quoteID, Equal<Current<CROpportunity.quoteNoteID>>,
                                And<CROpportunityTax.lineNbr, Equal<Current<CROpportunityProducts.lineNbr>>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0) && taxComparer.Compare((PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcLine:
                        foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
                            Where<CROpportunityTax.quoteID, Equal<Current<CROpportunity.quoteNoteID>>,
                                And<CROpportunityTax.lineNbr, Less<intMax>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && ((CROpportunityTax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
                                    && taxComparer.Compare((PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcTotals:
                        foreach (CRTaxTran record in PXSelect<CRTaxTran,
                            Where<CRTaxTran.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
                            OrderBy<Asc<CRTaxTran.lineNbr, Asc<CRTaxTran.taxID>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            if (record.TaxID != null && tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && taxComparer.Compare((PXResult<CRTaxTran, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<CRTaxTran, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    default:
                        return ret;
                }
            }

            protected override List<Object> SelectDocumentLines(PXGraph graph, object row)
            {
                var res = PXSelect<CROpportunityProducts,
                            Where<CROpportunityProducts.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>>
                            .SelectMultiBound(graph, new object[] { row })
                            .RowCast<CROpportunityProducts>()
                            .Select(_ => (object)_)
                            .ToList();
                return res;
            }

            #endregion

            #region CRTaxTran
            protected virtual void CRTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
            {
                if (e.Row == null)
                    return;

                PXUIFieldAttribute.SetEnabled<CRTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
            }
            #endregion

            #region CROpportunityTax
            protected virtual void CRTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
            {
                CRTaxTran row = e.Row as CRTaxTran;
                if (row == null) return;

                if (e.Operation == PXDBOperation.Delete)
                {
                    CROpportunityTax tax = (CROpportunityTax)Base.TaxLines.Cache.Locate(FindCROpportunityTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Deleted ||
                         Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.InsertedDeleted)
                        e.Cancel = true;
                }
                if (e.Operation == PXDBOperation.Update)
                {
                    CROpportunityTax tax = (CROpportunityTax)Base.TaxLines.Cache.Locate(FindCROpportunityTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Updated)
                        e.Cancel = true;
                }
            }
            protected virtual CROpportunityTax FindCROpportunityTax(CRTaxTran tran)
            {
                var list = PXSelect<CROpportunityTax,
                    Where<CROpportunityTax.quoteID, Equal<Required<CRTaxTran.quoteID>>,
                        And<CROpportunityTax.lineNbr, Equal<Required<CRTaxTran.lineNbr>>,
                        And<CROpportunityTax.taxID, Equal<Required<CRTaxTran.taxID>>>>>>
                        .SelectSingleBound(Base, new object[] { }, tran.QuoteID, tran.LineNbr, tran.TaxID)
                        .RowCast<CROpportunityTax>();

                return list.FirstOrDefault();
            }
            #endregion
        }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ContactAddress : CROpportunityContactAddressExt<OpportunityMaint>
		{
			#region Overrides

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<Extensions.CROpportunityContactAddress.DocumentAddress>(Base.Opportunity_Address);
				Contacts = new PXSelectExtension<Extensions.CROpportunityContactAddress.DocumentContact>(Base.Opportunity_Contact);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CROpportunity))
				{
					DocumentAddressID = typeof(CROpportunity.opportunityAddressID),
					DocumentContactID = typeof(CROpportunity.opportunityContactID),
					ShipAddressID = typeof(CROpportunity.shipAddressID),
					ShipContactID = typeof(CROpportunity.shipContactID),
					BillAddressID = typeof(CROpportunity.billAddressID),
					BillContactID = typeof(CROpportunity.billContactID),
				};
			}
			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRContact)) { EMail = typeof(CRContact.email) };
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(CRAddress));
			}
			protected override PXCache GetContactCache()
			{
				return Base.Opportunity_Contact.Cache;
			}
			protected override PXCache GetAddressCache()
			{
				return Base.Opportunity_Address.Cache;
			}

			protected override PXCache GetBillingContactCache()
			{
				return Base.Opportunity.Current?.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution
					? Base.Billing_Contact.Cache : null;
			}

			protected override PXCache GetBillingAddressCache()
			{
				return Base.Opportunity.Current?.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution
					? Base.Billing_Address.Cache : null;
			}

			protected override PXCache GetShippingContactCache()
			{
				return Base.Shipping_Contact.Cache;
			}
			protected override PXCache GetShippingAddressCache()
			{
				return Base.Shipping_Address.Cache;
			}
			protected override IPersonalContact SelectContact()
			{
				return Base.Opportunity_Contact.SelectSingle();
			}

			protected override IPersonalContact SelectBillingContact()
			{
				return Base.Billing_Contact.SelectSingle();
			}

			protected override IPersonalContact SelectShippingContact()
			{
				return Base.Shipping_Contact.SelectSingle();
			}

			protected override IAddress SelectAddress()
			{
				return Base.Opportunity_Address.SelectSingle();
			}

			protected override IAddress SelectBillingAddress()
			{
				return Base.Billing_Address.SelectSingle();
			}

			protected override IAddress SelectShippingAddress()
			{
				return Base.Shipping_Address.SelectSingle();
			}

			protected override IPersonalContact GetEtalonContact()
			{
				return SafeGetEtalon(Base.Opportunity_Contact.Cache) as IPersonalContact;
			}

			protected override IPersonalContact GetEtalonShippingContact()
			{
				return SafeGetEtalon(Base.Shipping_Contact.Cache) as IPersonalContact;
			}

			protected override IPersonalContact GetEtalonBillingContact()
			{
				return SafeGetEtalon(Base.Billing_Contact.Cache) as IPersonalContact;
			}

			protected override IAddress GetEtalonAddress()
			{
				return SafeGetEtalon(Base.Opportunity_Address.Cache) as IAddress;
			}

			protected override IAddress GetEtalonShippingAddress()
			{
				return SafeGetEtalon(Base.Shipping_Address.Cache) as IAddress;
			}

			protected override IAddress GetEtalonBillingAddress()
			{
				return SafeGetEtalon(Base.Billing_Address.Cache) as IAddress;
			}

			#endregion

			#region Events

			protected override void _(Events.FieldUpdated<Extensions.CROpportunityContactAddress.Document, Extensions.CROpportunityContactAddress.Document.bAccountID> e)
			{
				if (e.Row == null)
				{
					base._(e);
					return;
				}

				var opportunity = e.Row.Base as CROpportunity;

				Base.Opportunity.Cache.SetDefaultExt<CROpportunity.locationID>(opportunity);
				Base.Opportunity.Cache.SetDefaultExt<CROpportunity.taxCalcMode>(opportunity);

				foreach (var opportunityRevision in PXSelect<CROpportunityRevision, Where<CROpportunityRevision.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
																.Select(Base).RowCast<CROpportunityRevision>())
				{
					Base.OpportunityRevision.Cache.SetValueExt<CROpportunityRevision.bAccountID>(opportunityRevision, opportunity.BAccountID);
					Base.OpportunityRevision.Cache.SetDefaultExt<CROpportunityRevision.locationID>(opportunityRevision);
				}

				base._(e);

				var allowOverrideContactAddress = (opportunity.AllowOverrideContactAddress == true) || (opportunity.BAccountID == null && opportunity.ContactID == null);
				Base.Opportunity.Cache.SetValueExt<CROpportunity.allowOverrideContactAddress>(opportunity, allowOverrideContactAddress);
			}

			protected override void _(Events.FieldUpdated<Extensions.CROpportunityContactAddress.Document, Extensions.CROpportunityContactAddress.Document.contactID> e)
			{
				base._(e);

				if (e.Row == null) return;

				var opportunityRevisionsquery = new PXSelectJoin<CROpportunityRevision, InnerJoin<CRQuote, On<CRQuote.quoteID, Equal<CROpportunityRevision.noteID>>>,
					Where<CROpportunityRevision.opportunityID, Equal<Current<CROpportunity.opportunityID>>, And<CRQuote.quoteID, Equal<CRQuote.defQuoteID>>>>(Base);

				CROpportunityRevision opportunityRevision = opportunityRevisionsquery.SelectSingle();
				if (opportunityRevision != null)
				{
					Base.OpportunityRevision.Cache.SetValueExt<CROpportunityRevision.contactID>(opportunityRevision, e.Row.ContactID);
				}

				if (Documents.Cache.GetStatus(e.Row) == PXEntryStatus.Updated)
				{
					var allowOverrideContactAddress = (e.Row.AllowOverrideContactAddress == true) || (e.Row.BAccountID == null && e.Row.ContactID == null);
					Documents.Cache.SetValueExt<Extensions.CROpportunityContactAddress.Document.allowOverrideContactAddress>(e.Row, allowOverrideContactAddress);
				}
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefaultOpportunityOwner : CRDefaultDocumentOwner<
			OpportunityMaint, CROpportunity,
			CROpportunity.classID, CROpportunity.ownerID, CROpportunity.workgroupID>
		{ }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CreateBothAccountAndContactFromOpportunityGraphExt : CRCreateBothContactAndAccountAction<OpportunityMaint, CROpportunity, CreateAccountFromOpportunityGraphExt, CreateContactFromOpportunityGraphExt> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CreateAccountFromOpportunityGraphExt : CRCreateAccountAction<OpportunityMaint, CROpportunity>
		{
			#region Initialization

			protected override string TargetType => CRTargetEntityType.CROpportunity;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.Opportunity_Address);
				Contacts = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentContact>(Base.Opportunity_Contact);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CROpportunity)) { RefContactID = typeof(CROpportunity.contactID) };
			}
			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRContact));
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(CRAddress));
			}

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.GetExtension<OpportunityMaint_ActivityDetailsExt>().Activities;

			#endregion

			#region Events

			protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.accountClass> e)
			{
				if (ExistingAccount.SelectSingle() is BAccount existingAccount)
				{
					e.NewValue = existingAccount.ClassID;
					e.Cancel = true;
					return;
				}

				CROpportunity opportunity = Base.Opportunity.Current;
				if (opportunity == null) return;

				CROpportunityClass cls = PXSelect<
						CROpportunityClass,
					Where<
						CROpportunityClass.cROpportunityClassID,
						Equal<Required<CROpportunity.classID>>>>
					.Select(Base, opportunity.ClassID);

				if (cls?.TargetBAccountClassID != null)
				{
					e.NewValue = cls.TargetBAccountClassID;
				}
				else
				{
					e.NewValue = Base.Setup.Current?.DefaultCustomerClassID;
				}

				e.Cancel = true;
			}

			protected override void _(Events.RowSelected<AccountsFilter> e)
			{
				base._(e);

				AccountsFilter row = e.Row as AccountsFilter;
				if (row == null)
					return;

				CROpportunity opportunity = Base.Opportunity.Current;
				if (opportunity.ContactID != null)
				{
					PXUIFieldAttribute.SetVisible<AccountsFilter.linkContactToAccount>(e.Cache, row, true);
					Contact contact = Base.CurrentContact.Current ?? Base.CurrentContact.SelectSingle();
					if (contact == null)
					{
						PXUIFieldAttribute.SetEnabled<AccountsFilter.linkContactToAccount>(e.Cache, row, false);
					}
					else
					{
						if (contact.BAccountID != null)
						{
							PXUIFieldAttribute.SetWarning<AccountsFilter.linkContactToAccount>(e.Cache, row, Messages.AccountContactValidation);
						}
						else
						{
							PXUIFieldAttribute.SetEnabled<AccountsFilter.linkContactToAccount>(e.Cache, row, true);
						}
					}
				}
			}

			protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.linkContactToAccount> e)
			{
				AccountsFilter row = e.Row as AccountsFilter;
				if (row == null)
					return;

				CROpportunity opportunity = Base.Opportunity.Current;
				if (opportunity.ContactID != null)
				{
					Contact contact = Base.CurrentContact.Current ?? Base.CurrentContact.SelectSingle();
					if (contact == null)
					{
						e.NewValue = false;
					}
					else
					{
						if (contact.BAccountID != null)
						{
							e.NewValue = false;
						}
						else
						{
							e.NewValue = true;
						}
					}
				}
				else
				{
					e.NewValue = false;
				}

				e.Cancel = true;
			}

			#endregion

		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CreateContactFromOpportunityGraphExt : CRCreateContactAction<OpportunityMaint, CROpportunity>
		{
			#region Initialization

			protected override string TargetType => CRTargetEntityType.CROpportunity;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentAddress>(Base.Opportunity_Address);
				Contacts = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentContact>(Base.Opportunity_Contact);
				ContactMethod = new PXSelectExtension<CR.Extensions.CRCreateActions.DocumentContactMethod>(Base.Opportunity_Contact);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CROpportunity)) { RefContactID = typeof(CROpportunity.contactID) };
			}
			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRContact));
			}
			protected override DocumentContactMethodMapping GetDocumentContactMethodMapping()
			{
				return new DocumentContactMethodMapping(typeof(CRContact));
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(CRAddress));
			}

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.GetExtension<OpportunityMaint_ActivityDetailsExt>().Activities;

			#endregion

			#region Events

			protected virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.contactClass> e)
			{
				if (ExistingContact.SelectSingle() is Contact existingContact)
				{
					e.NewValue = existingContact.ClassID;
					e.Cancel = true;
					return;
				}

				CROpportunity opportunity = Base.Opportunity.Current;
				if (opportunity == null) return;

				CROpportunityClass cls = PXSelect<
						CROpportunityClass,
					Where<
						CROpportunityClass.cROpportunityClassID,
						Equal<Required<CROpportunity.classID>>>>
					.Select(Base, opportunity.ClassID);

				if (cls?.TargetContactClassID != null)
				{
					e.NewValue = cls.TargetContactClassID;
				}
				else
				{
					e.NewValue = Base.Setup.Current?.DefaultContactClassID;
				}

				e.Cancel = true;
			}

			public virtual void _(Events.RowSelected<ContactFilter> e)
			{
				bool isBAccountSelected = (Base?.Opportunity?.Current?.BAccountID != null);
				PXUIFieldAttribute.SetReadOnly<ContactFilter.fullName>(e.Cache, e.Row, isBAccountSelected);
			}

			#endregion

			#region Overrides

			protected override void MapContactMethod(DocumentContactMethod source, Contact target)
			{
			}

			protected override object GetDefaultFieldValueFromCache<TExistingField, TField>()
			{
				if (typeof(TExistingField) == typeof(Contact.fullName)
					|| (Base?.Opportunity?.Current?.BAccountID == null)
					|| (Base?.Opportunity?.Current?.AllowOverrideContactAddress == true))
				{
					return base.GetDefaultFieldValueFromCache<TExistingField, TField>();
				}
				return null;
			}

			#endregion
		}

		/// <exclude/>
		public class CRCreateSalesOrderExt : CRCreateSalesOrder<OpportunityMaint.Discount, OpportunityMaint, CROpportunity>
		{
			#region Initialization

			public static bool IsActive() => IsExtensionActive();

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CROpportunity))
				{
					QuoteID = typeof(CROpportunity.quoteNoteID)
				};
			}

			#endregion

			#region Events

			public virtual void _(Events.RowSelected<CROpportunity> e)
			{
				CROpportunity row = e.Row as CROpportunity;
				if (row == null) return;

				CRQuote primaryQt = Base.PrimaryQuoteQuery.SelectSingle();

				bool hasProducts = Base.Products.SelectSingle() != null;

				var products = Base.Products.View.SelectMultiBound(new object[] { row }).RowCast<CROpportunityProducts>();

				bool allProductsHasNoInventoryID = products.Any(_ => _.InventoryID == null) && !products.Any(_ => _.InventoryID != null);

				bool hasQuotes = primaryQt != null;

				CreateSalesOrder
					.SetEnabled(hasProducts && !allProductsHasNoInventoryID
					&& (
						(!hasQuotes
							|| (primaryQt.Status == CRQuoteStatusAttribute.Approved
								|| primaryQt.Status == CRQuoteStatusAttribute.Sent
								|| primaryQt.Status == CRQuoteStatusAttribute.Accepted
								|| primaryQt.Status == CRQuoteStatusAttribute.Draft
								)
							)
						)
					&& (!hasQuotes || primaryQt.QuoteType == CRQuoteTypeAttribute.Distribution)
					&& e.Row.BAccountID != null);
			}

			#endregion

			#region Overrides

			public override CRQuote GetQuoteForWorkflowProcessing()
			{
				return Base.PrimaryQuoteQuery.SelectSingle();
			}
			#endregion
		}


		/// <exclude/>
		public class CRCreateInvoiceExt : CRCreateInvoice<OpportunityMaint.Discount, OpportunityMaint, CROpportunity>
		{
			#region Initialization

			public static bool IsActive() => IsExtensionActive();

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CROpportunity))
				{
					QuoteID = typeof(CROpportunity.quoteNoteID)
				};
			}

			#endregion

			#region Events

			public virtual void _(Events.RowSelected<CROpportunity> e)
			{
				CROpportunity row = e.Row as CROpportunity;
				if (row == null) return;

				CRQuote primaryQt = Base.PrimaryQuoteQuery.SelectSingle();

				bool hasProducts = Base.Products.SelectSingle() != null;

				var products = Base.Products.View.SelectMultiBound(new object[] { row }).RowCast<CROpportunityProducts>();

				bool allProductsHasNoInventoryID = products.Any(_ => _.InventoryID == null) && !products.Any(_ => _.InventoryID != null);

				bool hasQuotes = primaryQt != null;

				CreateInvoice
					.SetEnabled(hasProducts && !allProductsHasNoInventoryID
					&& (
						(!hasQuotes
							|| (primaryQt.Status == CRQuoteStatusAttribute.Approved
								|| primaryQt.Status == CRQuoteStatusAttribute.Sent
								|| primaryQt.Status == CRQuoteStatusAttribute.Accepted
								|| primaryQt.Status == CRQuoteStatusAttribute.Draft
								)
							)
						)
					&& (!hasQuotes || primaryQt.QuoteType == CRQuoteTypeAttribute.Distribution)
					&& e.Row.BAccountID != null);
			}

			#endregion

			#region Overrides

			public override CRQuote GetQuoteForWorkflowProcessing()
			{
				return Base.PrimaryQuoteQuery.SelectSingle();
			}

			#endregion
		}

		#region Address Lookup Extension

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class OpportunityMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<OpportunityMaint, CROpportunity, CRAddress>
		{
			protected override string AddressView => nameof(Base.Opportunity_Address);
			protected override string ViewOnMap => nameof(Base.viewMainOnMap);
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class OpportunityMaintShippingAddressLookupExtension : CR.Extensions.AddressLookupExtension<OpportunityMaint, CROpportunity, CRShippingAddress>
		{
			protected override string AddressView => nameof(Base.Shipping_Address);
			protected override string ViewOnMap => nameof(Base.ViewShippingOnMap);
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class OpportunityMaintBillingAddressLookupExtension : CR.Extensions.AddressLookupExtension<OpportunityMaint, CROpportunity, CRBillingAddress>
		{
			protected override string AddressView => nameof(Base.Billing_Address);
			protected override string ViewOnMap => nameof(Base.ViewBillingOnMap);

			public override void _(Events.RowSelected<CROpportunity> e)
			{
				base._(e);

				if (e.Row == null) return;

				bool isSalesQuote = e.Row.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution;

				Base.ViewBillingOnMap.SetVisible(Base.ViewBillingOnMap.GetVisible() && isSalesQuote);

				var billingLookup = Base.Actions[ActionName];

				billingLookup.SetVisible(billingLookup.GetVisible() && isSalesQuote);
			}
		}

		#endregion

		/// <exclude/>
		public class ExtensionSort
			: SortExtensionsBy<ExtensionOrderFor<OpportunityMaint>
				.FilledWith<
					ContactAddress,
					MultiCurrency,
					SalesPrice,
					Discount,
					SalesTax
				>>
		{ }

		#endregion

		public static bool ProjectQuotesInstalled { get { return PXAccess.FeatureInstalled<FeaturesSet.projectQuotes>(); } }
		public static bool SalesQuotesInstalled { get { return PXAccess.FeatureInstalled<FeaturesSet.salesQuotes>(); } }
	}
}
