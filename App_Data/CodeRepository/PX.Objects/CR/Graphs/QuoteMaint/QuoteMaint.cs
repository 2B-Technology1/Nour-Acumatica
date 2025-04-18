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
using System.Collections.Specialized;
using System.Linq;
using PX.Common;
using PX.Data;
using System.Collections;
using PX.Objects.CS;
using PX.Objects.CM.Extensions;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.IN.Attributes;
using PX.Objects.TX;
using PX.Objects.SO;
using System.Collections.Generic;
using System.Diagnostics;
using PX.Objects.GL;
using PX.Objects.Extensions.MultiCurrency.CR;
using PX.Objects.Extensions.SalesPrice;
using PX.Objects.Extensions.Discount;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.Extensions.ContactAddress;
using Autofac;
using System.Web.Compilation;
using PX.Data.DependencyInjection;
using PX.Objects.Common.Discount;
using PX.Objects.EP;
using PX.Objects.CR.Standalone;
using PX.Objects.CR.DAC;
using PX.LicensePolicy;
using PX.Data.WorkflowAPI;
using PX.Api.Models;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CROpportunityContactAddress;
using PX.Objects.CR.Extensions.CRCreateSalesOrder;
using PX.Objects.CR.Extensions.CRCreateInvoice;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.Objects.CR.QuoteMaint_Extensions;

namespace PX.Objects.CR
{
    public class QuoteMaint : PXGraph<QuoteMaint>, PXImportAttribute.IPXPrepareItems, IGraphWithInitialization
    {
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
                Where<CROpportunity.isActive.IsEqual<True>.
                    And<Where<BAccount.bAccountID.IsNull.Or<MatchUserFor<BAccount>>>>>,
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
            [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual string OpportunityID{ get; set; }
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
            public virtual bool? RecalculatePrices { get; set; }
            #endregion

            #region OverrideManualPrices
            public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
            [PXBool()]
            [PXUIField(DisplayName = "Override Manual Prices", Enabled = false)]
            public virtual bool? OverrideManualPrices { get; set; }
            #endregion

            #region RecalculateDiscounts
            public abstract class recalculateDiscounts : PX.Data.BQL.BqlBool.Field<recalculateDiscounts> { }
            [PXBool()]
            [PXUIField(DisplayName = "Recalculate Discounts")]
            public virtual bool? RecalculateDiscounts { get; set; }
            #endregion

            #region OverrideManualDiscounts
            public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
            [PXBool()]
            [PXUIField(DisplayName = "Override Manual Line Discounts", Enabled = false)]
            public virtual bool? OverrideManualDiscounts { get; set; }
            #endregion

            #region OverrideManualDocGroupDiscounts
            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Override Manual Group and Document Discounts")]
            public virtual Boolean? OverrideManualDocGroupDiscounts { get; set; }
            public abstract class overrideManualDocGroupDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDocGroupDiscounts> { }
            #endregion
        }
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

        [PXViewName(Messages.CRQuote)]
        [PXCopyPasteHiddenFields(typeof(CRQuote.approved), typeof(CRQuote.rejected))]
        public PXSelect<CRQuote,
                Where<CRQuote.opportunityID, Equal<Optional<CRQuote.opportunityID>>, And<CRQuote.quoteType, Equal<CRQuoteTypeAttribute.distribution>>>> Quote;

        [PXHidden]
        [PXCopyPasteHiddenFields(typeof(CRQuote.approved), typeof(CRQuote.rejected))]
        public PXSelect<CRQuote,
                Where<CRQuote.opportunityID, Equal<Current<CRQuote.opportunityID>>,
                    And<CRQuote.quoteNbr, Equal<Current<CRQuote.quoteNbr>>>>>
            QuoteCurrent;


        [PXHidden]
        public PXSelect<Address>
            Address;

		[PXHidden]
		public PXSelect<CROpportunityRevision>
			OpportunityRevision;

        [PXHidden]
        public PXSetup<Contact, Where<Contact.contactID, Equal<Optional<CRQuote.contactID>>>> Contacts;

        [PXHidden]
        public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<CRQuote.bAccountID>>>> customer;

		[PXViewName(Messages.Answers)]
		public CRAttributeSourceList<CRQuote, CRQuote.contactID> Answers;

        [PXViewName(Messages.QuoteProducts)]
        [PXImport(typeof(CRQuote))]
        public PXOrderedSelect<CRQuote, CROpportunityProducts,
                Where<CROpportunityProducts.quoteID, Equal<Current<CRQuote.quoteID>>>,
                OrderBy<Asc<CROpportunityProducts.sortOrder>>>
            Products;

        [PXHidden]
        public PXSelect<CROpportunityRevision> FakeRevisionCache;

        public PXSelect<CROpportunityTax,
            Where<CROpportunityTax.quoteID, Equal<Current<CRQuote.quoteID>>,
                And<CROpportunityTax.lineNbr, Less<intMax>>>,
            OrderBy<Asc<CROpportunityTax.taxID>>> TaxLines;

        [PXViewName(Messages.QuoteTax)]
        public PXSelectJoin<CRTaxTran,
            InnerJoin<Tax, On<Tax.taxID, Equal<CRTaxTran.taxID>>>,
            Where<CRTaxTran.quoteID, Equal<Current<CRQuote.quoteID>>>,
            OrderBy<Asc<CRTaxTran.lineNbr, Asc<CRTaxTran.taxID>>>> Taxes;


        public PXSetup<Location,
            Where<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>,
                And<Location.locationID, Equal<Optional<CRQuote.locationID>>>>> location;


        [PXViewName(Messages.QuoteContact)]
        public PXSelect<CRContact, Where<CRContact.contactID, Equal<Current<CRQuote.opportunityContactID>>>> Quote_Contact;

        [PXViewName(Messages.QuoteAddress)]
        public PXSelect<CRAddress, Where<CRAddress.addressID, Equal<Current<CRQuote.opportunityAddressID>>>> Quote_Address;

        [PXViewName(Messages.ShippingContact)]
        public PXSelect<CRShippingContact, Where<CRShippingContact.contactID, Equal<Current<CRQuote.shipContactID>>>> Shipping_Contact;

        [PXViewName(Messages.ShippingAddress)]
        public PXSelect<CRShippingAddress, Where<CRShippingAddress.addressID, Equal<Current<CRQuote.shipAddressID>>>> Shipping_Address;

		[PXViewName(Messages.BillToContact)]
		public PXSelect<CRBillingContact, Where<CRBillingContact.contactID, Equal<Current<CRQuote.billContactID>>>> Billing_Contact;

		[PXViewName(Messages.BillToAddress)]
		public PXSelect<CRBillingAddress, Where<CRBillingAddress.addressID, Equal<Current<CRQuote.billAddressID>>>> Billing_Address;


		[PXHidden]
        public PXSelectJoin<Contact,
            LeftJoin<Address, On<Contact.defAddressID, Equal<Address.addressID>>>,
            Where<Contact.contactID, Equal<Current<CRQuote.contactID>>>> CurrentContact;

        [PXHidden]
        public PXSelectJoin<Standalone.CROpportunity,
                LeftJoin<CROpportunityRevision,
                    On<CROpportunityRevision.noteID, Equal<Standalone.CROpportunity.defQuoteID>>,
                LeftJoin<Standalone.CRQuote,
                    On<Standalone.CRQuote.quoteID, Equal<Standalone.CROpportunityRevision.noteID>>>>,
                Where<Standalone.CROpportunity.opportunityID, Equal<Optional<CRQuote.opportunityID>>>>
            Opportunity;

        [PXViewName(Messages.Opportunity)]
        public PXSelect<CROpportunity,
                Where<CROpportunity.opportunityID, Equal<Optional<CRQuote.opportunityID>>>> CurrentOpportunity;

        [PXHidden]
        public PXSelect<AP.Vendor> Vendors;

		[PXViewName(Messages.Approval)]
		public EPApprovalAutomation<CRQuote, CRQuote.approved, CRQuote.rejected, CRQuote.hold, CRSetupQuoteApproval> Approval;

        [PXViewName(Messages.CreateAccount)]
        [PXCopyPasteHiddenView]
        public PXFilter<CopyQuoteFilter> CopyQuoteInfo;

        [InjectDependency]
        protected ILicenseLimitsService _licenseLimits { get; set; }

        public override bool ProviderInsert(Type table, params PXDataFieldAssign[] pars)
        {
            if (table == typeof(CROpportunityRevision))
            {
                foreach (var param in pars)
                {
                    var cacheColumn = new Data.SQLTree.Column(Caches[typeof(CROpportunityRevision)].GetBqlField<CROpportunityRevision.noteID>().Name, table.Name);
                    if (param.Column.Equals(cacheColumn))
                    {
                        var noteID = Guid.Parse(param.Value.ToString());
                        var revisions = PXSelect<CROpportunityRevision, Where<CROpportunityRevision.noteID, Equal<Required<CROpportunityRevision.noteID>>>>.SelectSingleBound(this, null, noteID);
                        if (revisions.Count > 0)
                            // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Used inside Acumatica only, not faces to UI]
                            throw new PXDbOperationSwitchRequiredException(table.Name, "Need update instead of insert");
                    }
                }
            }
            return base.ProviderInsert(table, pars);
        }

        public override bool ProviderDelete(Type table, params PXDataFieldRestrict[] pars)
        {
            if (table == typeof(CROpportunityRevision))
            {
                var cacheColumn = new Data.SQLTree.Column(Caches[typeof(CROpportunityRevision)].GetBqlField<CROpportunityRevision.opportunityID>().Name, table.Name);
                foreach (var param in pars)
                {
                    if (param.Column.Equals(cacheColumn))
                    {
                        if (param.Value != null && IsSingleQuote(param.Value.ToString()))
                        {
                            return true;
                        }
                    }
                }


            }
            return base.ProviderDelete(table, pars);
        }

        #endregion

        #region Ctors

        public QuoteMaint()
        {
            if (string.IsNullOrEmpty(Setup.Current.QuoteNumberingID))
            {
                throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);
            }

            this.Views.Caches.Remove(typeof(Standalone.CROpportunity));
            this.Views.Caches.Remove(typeof(CROpportunity));

        }

        void IGraphWithInitialization.Initialize()
        {
            if (_licenseLimits != null)
            {
                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<CRQuote>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(CROpportunityProducts), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                            new PXDataFieldValue<CROpportunityProducts.quoteID>(((QuoteMaint)graph).Quote.Current?.QuoteID)
                        };
                    }));
            }
        }

        #endregion

        #region Actions

        public PXSave<CRQuote> Save;
        public PXAction<CRQuote> cancel;
        public PXInsert<CRQuote> insert;
        public PXCopyPasteAction<CRQuote> CopyPaste;
        public PXDelete<CRQuote> Delete;
        public PXFirst<CRQuote> First;
        public PXPrevious<CRQuote> previous;
        public PXNext<CRQuote> next;
        public PXLast<CRQuote> Last;
        public PXAction<CRQuote> viewOnMap;
        public PXAction<CRQuote> validateAddresses;

        [PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXCancelButton]
        protected virtual IEnumerable Cancel(PXAdapter adapter)
        {
            string oppID = Quote.Current != null ? Quote.Current.OpportunityID : null;
            Quote.Cache.Clear();
            foreach (CRQuote quote in (new PXCancel<CRQuote>(this, "Cancel")).Press(adapter))
            {
                return new object[] { quote };
            }
            return new object[0];
        }

        [PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXPreviousButton]
        protected virtual IEnumerable Previous(PXAdapter adapter)
        {
            foreach (CRQuote loc in (new PXPrevious<CRQuote>(this, "Prev")).Press(adapter))
            {
                if (Quote.Cache.GetStatus(loc) == PXEntryStatus.Inserted)
                {
                    return Last.Press(adapter);
                }
                else
                {
                    return new object[] { loc };
                }
            }
            return new object[0];
        }

        [PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXNextButton]
        protected virtual IEnumerable Next(PXAdapter adapter)
        {
            foreach (CRQuote loc in (new PXNext<CRQuote>(this, "Next")).Press(adapter))
            {
                if (Quote.Cache.GetStatus(loc) == PXEntryStatus.Inserted)
                {
                    return First.Press(adapter);
                }
                else
                {
                    return new object[] { loc };
                }
            }
            return new object[0];
        }

        public PXAction<CRQuote> viewMainOnMap;

        [PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ViewMainOnMap()
        {
            var address = Quote_Address.SelectSingle();
            if (address != null)
            {
                BAccountUtility.ViewOnMap(address);
            }
        }

        public PXAction<CRQuote> ViewShippingOnMap;

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

		public PXAction<CRQuote> ViewBillingOnMap;
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

		public PXAction<CRQuote> primaryQuote;
        [PXUIField(DisplayName = Messages.MarkAsPrimary)]
        [PXButton]
        public virtual IEnumerable PrimaryQuote(PXAdapter adapter)
        {
            IEnumerable<CRQuote> quotes = adapter.Get<CRQuote>().ToArray();

            Save.Press();

            foreach (CRQuote item in quotes)
            {
                Opportunity.Cache.Clear();
                var rec = (PXResult<Standalone.CROpportunity>)
                    this.Opportunity.View.SelectSingleBound(new object[] { item });

                this.Opportunity.Current = rec;
                this.Opportunity.Current.DefQuoteID = item.QuoteID;
                item.DefQuoteID = item.QuoteID;
                Standalone.CROpportunity opudate = Opportunity.Cache.Update(this.Opportunity.Current) as Standalone.CROpportunity;
                this.Views.Caches.Add(typeof(Standalone.CROpportunity));
                CRQuote upitem = Quote.Cache.Update(item) as CRQuote;
                Save.Press();
                yield return upitem;
            }
        }


        public PXAction<CRQuote> copyQuote;
        [PXUIField(DisplayName = Messages.CopyQuote, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable CopyQuote(PXAdapter adapter)
        {
            List<CRQuote> CRQoutes = new List<CRQuote>(adapter.Get().Cast<CRQuote>());
            foreach (CRQuote quote in CRQoutes)
            {
                if (CopyQuoteInfo.View.Answer == WebDialogResult.None)
                {
                    CopyQuoteInfo.Cache.Clear();
                    CopyQuoteFilter filterdata = CopyQuoteInfo.Cache.Insert() as CopyQuoteFilter;
                    filterdata.Description = quote.Subject + Messages.QuoteCopy;
                    filterdata.RecalculatePrices = false;
                    filterdata.RecalculateDiscounts = false;
                    filterdata.OverrideManualPrices = false;
                    filterdata.OverrideManualDiscounts = false;
                    filterdata.OverrideManualDocGroupDiscounts = false;
                    filterdata.OpportunityID = quote.OpportunityID;
                }

                if (CopyQuoteInfo.AskExt() != WebDialogResult.Yes)
                    return CRQoutes;

                Save.Press();
                PXLongOperation.StartOperation(this, () => CopyToQuote(quote, CopyQuoteInfo.Current));
            }
            return CRQoutes;
        }

        public virtual void CopyToQuote(CRQuote currentquote, CopyQuoteFilter param)
        {
	        this.Quote.Current = currentquote;

			QuoteMaint graph = PXGraph.CreateInstance<QuoteMaint>();
	        graph.SelectTimeStamp();

			CopyToQuote(graph, currentquote, param);

			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Same);
		}

		protected virtual void CopyToQuote(QuoteMaint graph, CRQuote currentquote, CopyQuoteFilter param)
		{
			var quote = (CRQuote)graph.Quote.Cache.CreateInstance();
	        graph.Opportunity.Current = graph.Opportunity.SelectSingle(param.OpportunityID);
			quote.OpportunityID = param.OpportunityID;
			quote.ShipAddressID = currentquote.ShipAddressID;
			quote.ShipContactID = currentquote.ShipContactID;
			quote.OpportunityAddressID = currentquote.OpportunityAddressID;
			quote.OpportunityContactID = currentquote.OpportunityContactID;
			quote.BillAddressID = currentquote.BillAddressID;
			quote.BillContactID = currentquote.BillContactID;
            quote = graph.Quote.Insert(quote);
	        CurrencyInfo info =
		        PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CRQuote.curyInfoID>>>>
			        .Select(this);
	        info.CuryInfoID = null;
	        info = (CurrencyInfo)graph.Caches<CurrencyInfo>().Insert(info);

			foreach (string field in Quote.Cache.Fields)
            {
                if (graph.Quote.Cache.Keys.Contains(field)
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.quoteID))
                    || field == graph.Quote.Cache.GetField(typeof(CRQuote.status))
                    || field == graph.Quote.Cache.GetField(typeof(CRQuote.isPrimary))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.expirationDate))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.approved))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.rejected)))
                    continue;

                graph.Quote.Cache.SetValue(quote, field,
                    Quote.Cache.GetValue(currentquote, field));
			}
			UDFHelper.CopyAttributes(Quote.Cache, currentquote, graph.Quote.Cache, quote, null);

            quote.CuryInfoID = info.CuryInfoID;
	        quote.Subject = param.Description;
	        quote.DocumentDate = this.Accessinfo.BusinessDate;	        

			string note = PXNoteAttribute.GetNote(Quote.Cache, currentquote);
            Guid[] files = PXNoteAttribute.GetFileNotes(Quote.Cache, currentquote);

			if (!IsSingleQuote(quote.OpportunityID))
			{
				object quoteID;
				graph.Quote.Cache.RaiseFieldDefaulting<CRQuote.noteID>(quote, out quoteID);
				quote.QuoteID = quote.NoteID = (Guid?)quoteID;
			}

			PXNoteAttribute.SetNote(graph.Quote.Cache, quote, note);
			PXNoteAttribute.SetFileNotes(graph.Quote.Cache, quote, files);

			Products.View.CloneView(graph, quote.QuoteID, info);
			var DiscountExt = this.GetExtension<Discount>();
			Views[nameof(DiscountExt.DiscountDetails)].CloneView(graph, quote.QuoteID, info);
			TaxLines.View.CloneView(graph, quote.QuoteID, info);
			Taxes.View.CloneView(graph, quote.QuoteID, info, nameof(CRTaxTran.RecordID));

			Quote_Contact.View.CloneView(graph, quote.QuoteID, info, nameof(CRContact.ContactID));
			quote.OpportunityContactID = graph.Quote_Contact.Current.ContactID;
			Quote_Address.View.CloneView(graph, quote.QuoteID, info, nameof(CRAddress.AddressID));
			quote.OpportunityAddressID = graph.Quote_Address.Current.AddressID;

			if (graph.Shipping_Contact.Current.OverrideContact is true)
			{
				Shipping_Contact.View.CloneView(graph, quote.QuoteID, info, nameof(CRShippingContact.ContactID));
				quote.ShipContactID = graph.Shipping_Contact.Current.ContactID;
			}
			if (graph.Shipping_Address.Current.OverrideAddress is true)
			{
				Shipping_Address.View.CloneView(graph, quote.QuoteID, info, nameof(CRShippingAddress.AddressID));
				quote.ShipAddressID = graph.Shipping_Address.Current.AddressID;
			}
			if (graph.Billing_Contact.Current.OverrideContact is true)
			{
				Billing_Contact.View.CloneView(graph, quote.QuoteID, info, nameof(CRBillingContact.ContactID));
				quote.BillContactID = graph.Billing_Contact.Current.ContactID;
			}
			if (graph.Billing_Address.Current.OverrideAddress is true)
			{
				Billing_Address.View.CloneView(graph, quote.QuoteID, info, nameof(CRBillingAddress.AddressID));
				quote.BillAddressID = graph.Billing_Address.Current.AddressID;
			}

			graph.Quote.Update(quote);
			var Discount = graph.GetExtension<QuoteMaint.Discount>();
	        Discount.recalcdiscountsfilter.Current.OverrideManualDiscounts = param.OverrideManualDiscounts == true;
			Discount.recalcdiscountsfilter.Current.OverrideManualDocGroupDiscounts = param.OverrideManualDocGroupDiscounts == true;
			Discount.recalcdiscountsfilter.Current.OverrideManualPrices = param.OverrideManualPrices == true;
	        Discount.recalcdiscountsfilter.Current.RecalcDiscounts = param.RecalculateDiscounts == true;
	        Discount.recalcdiscountsfilter.Current.RecalcUnitPrices = param.RecalculatePrices == true;
	        graph.Actions[nameof(Discount.RecalculateDiscountsAction)].Press();
        }

		protected virtual string DefaultReportID => "CR604500";

		protected virtual string DefaultNotificationCD => "CRQUOTE";


		public PXAction<CRQuote> sendQuote;
		[PXUIField(DisplayName = Messages.SendQuote)]
        [PXButton]
		public IEnumerable SendQuote(PXAdapter adapter)
		{
			foreach (CRQuote item in adapter.Get<CRQuote>())
			{
				var parameters = new Dictionary<string, string>();
				parameters[nameof(CRQuote) + "." + nameof(CRQuote.OpportunityID)] = item.OpportunityID;
				parameters[nameof(CRQuote) + "." + nameof(CRQuote.QuoteNbr)] = item.QuoteNbr;

				this.GetExtension<QuoteMaint_ActivityDetailsExt>().SendNotification(CRNotificationSource.BAccount, DefaultNotificationCD, item.BranchID, parameters, adapter.MassProcess);

				yield return item;
			}
		}


		public PXAction<CRQuote> printQuote;
        [PXUIField(DisplayName = "Print Quote")]
        [PXButton]
        public IEnumerable PrintQuote(PXAdapter adapter)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string actualReportID = DefaultReportID;
			PXReportRequiredException ex = null;

            foreach (CRQuote item in adapter.Get<CRQuote>())
            {
				Save.Press();

				parameters[nameof(CRQuote.OpportunityID)] = item.OpportunityID;
				parameters[nameof(CRQuote.QuoteNbr)] = item.QuoteNbr;
				ex = PXReportRequiredException.CombineReport(ex, actualReportID, parameters, OrganizationLocalizationHelper.GetCurrentLocalization(this));

				throw ex;
            }

            return adapter.Get();
        }

        [PXUIField(DisplayName = CR.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select/*, FieldClass = CS.Messages.ValidateAddress*/)]
        [PXButton()]
        public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
        {
            foreach (CRQuote current in adapter.Get<CRQuote>())
            {
                bool needSave = false;
                Save.Press();

                if (current != null)
                {
                    CRAddress address = this.Quote_Address.Select();
                    if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
                    {
                        if (PXAddressValidator.Validate<CRAddress>(this, address, true, true))
                        {
                            needSave = true;
                        }
                    }

                    CRShippingAddress shipAddress = this.Shipping_Address.Select();
                    if (shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
                    {
                        if (PXAddressValidator.Validate<CRShippingAddress>(this, shipAddress, true, true))
                        {
                            needSave = true;
                        }
                    }

					CRBillingAddress billAddress = this.Billing_Address.Select();
					if (billAddress != null && billAddress.IsDefaultAddress == false && billAddress.IsValidated == false)
					{
						if (PXAddressValidator.Validate<CRBillingAddress>(this, billAddress, true, true))
						{
							needSave = true;
						}
					}

					if (needSave)
                    {
                        this.Save.Press();
                    }
                }
                yield return current;
            }
        }

		public override void CopyPasteGetScript(bool isImportSimple, List<Command> script, List<Container> containers)
		{
			script.Where(_ => _.ObjectName.StartsWith(nameof(this.Products))).ForEach(_ => _.Commit = false);
			script.Where(_ => _.ObjectName.StartsWith(nameof(this.Products))).Last().Commit = true;
		}
        #endregion

        #region Work-flow Actions
        public PXAction<CRQuote> requestApproval;
        [PXUIField(Visible = true, DisplayName = Messages.RequestApproval, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable RequestApproval(PXAdapter adapter)
        {
            foreach (CRQuote item in adapter.Get<CRQuote>())
            {
                item.Approved = false;
                item.Rejected = false;
                QuoteCurrent.Cache.Update(item);
                Approval.Assign(item, Approval.GetAssignedMaps(item, QuoteCurrent.Cache));
                Save.Press();
                yield return item;
            }
        }

        public PXAction<CRQuote> editQuote;
        [PXUIField(Visible = true, DisplayName = Messages.Edit, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable EditQuote(PXAdapter adapter)
        {
            foreach (CRQuote item in adapter.Get<CRQuote>())
            {
                Approval.Reset(item);
                Save.Press();
                yield return item;
            }
        }

        public PXAction<CRQuote> approve;
        [PXUIField(Visible = true, DisplayName = Messages.Approve, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<CRQuote> reject;
        [PXUIField(Visible = true, DisplayName = Messages.Reject, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();


        public PXAction<CRQuote> markAsConverted;
        [PXUIField(Visible = true, DisplayName = Messages.MarkAsConverted, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable MarkAsConverted(PXAdapter adapter) => adapter.Get();

        public PXAction<CRQuote> decline;
        [PXUIField(Visible = true, DisplayName = Messages.MarkAsDeclined, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Decline(PXAdapter adapter) => adapter.Get();

        public PXAction<CRQuote> accept;
        [PXUIField(Visible = true, DisplayName = Messages.MarkAsAccepted, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Accept(PXAdapter adapter) => adapter.Get();
        #endregion

        #region Entity Event Handlers
        public PXWorkflowEventHandler<CRQuote, SOOrder> OnSalesOrderCreatedFromQuote;
        public PXWorkflowEventHandler<CRQuote, SOOrder> OnSalesOrderDeleted;

        public PXWorkflowEventHandler<CRQuote, ARInvoice, CRQuote> OnARInvoiceCreatedFromQuote;
        public PXWorkflowEventHandler<CRQuote, ARInvoice> OnARInvoiceDeleted;
        #endregion

        #region Contacts

		[CRMBAccount(bAccountTypes: new[]
		{
			typeof(BAccountType.prospectType),
			typeof(BAccountType.customerType),
			typeof(BAccountType.combinedType),
			typeof(BAccountType.vendorType),
		})]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

        #endregion
		
		#region EPApproval Cache Attached
		[PXDefault(typeof(CRQuote.documentDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender) { }

		[PXDefault(typeof(CRQuote.subject), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender) { }

		[CurrencyInfo(typeof(CRQuote.curyInfoID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender) { }

		[PXDefault(typeof(CRQuote.curyProductsAmount), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender) { }

		[PXDefault(typeof(CRQuote.productsAmount), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender) { }

		[PXDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender) { }

		[PXDefault(typeof(Search<Contact.contactID,
				Where<Contact.contactID, Equal<Current<AccessInfo.contactID>>,
					And<Current<CRQuote.workgroupID>, IsNull>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender) { }
        #endregion

        #region CRShippingAddress Cache Attached

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
        protected virtual void _(Events.CacheAttached<CRShippingAddress.latitude> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
        protected virtual void _(Events.CacheAttached<CRShippingAddress.longitude> e) { }

        #endregion

        #region QuoteFilter
        protected virtual void CopyQuoteFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            CopyQuoteFilter row = e.Row as CopyQuoteFilter;
            if (row == null) return;

            if (!row.RecalculatePrices == true)
            {
                CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualPrices>(row, false);
            }
            if (!row.RecalculateDiscounts == true)
            {
                CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualDiscounts>(row, false);
				CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualDocGroupDiscounts>(row, false);
			}
        }

        protected virtual void CopyQuoteFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CopyQuoteFilter row = e.Row as CopyQuoteFilter;
            if (row == null) return;

            PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualPrices>(sender, row, row.RecalculatePrices == true);
            PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualDiscounts>(sender, row, row.RecalculateDiscounts == true);
			PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalculateDiscounts == true);
		}
        #endregion

        #region QuoteFilter
        protected virtual void RecalcDiscountsParamFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            RecalcDiscountsParamFilter row = e.Row as RecalcDiscountsParamFilter;
            if (row == null) return;

            if (!(row.RecalcUnitPrices == true))
            {
                CopyQuoteInfo.Cache.SetValue<RecalcDiscountsParamFilter.overrideManualPrices>(row, false);
            }
            if (!(row.RecalcDiscounts == true))
            {
                CopyQuoteInfo.Cache.SetValue<RecalcDiscountsParamFilter.overrideManualDiscounts>(row, false);
				CopyQuoteInfo.Cache.SetValue<RecalcDiscountsParamFilter.overrideManualDocGroupDiscounts>(row, false);
			}
        }

        protected virtual void RecalcDiscountsParamFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            RecalcDiscountsParamFilter row = e.Row as RecalcDiscountsParamFilter;
            if (row == null) return;

            PXUIFieldAttribute.SetEnabled<RecalcDiscountsParamFilter.overrideManualPrices>(sender, row, row.RecalcUnitPrices == true);
            PXUIFieldAttribute.SetEnabled<RecalcDiscountsParamFilter.overrideManualDiscounts>(sender, row, row.RecalcDiscounts == true);
			PXUIFieldAttribute.SetEnabled<RecalcDiscountsParamFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalcDiscounts == true);
		}
        #endregion

        #region CRQuote        

        protected virtual void CRQuote_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = e.Row as CRQuote;
            if (row == null) return;

			e.NewValue = GetDefaultTaxZone(row);

			if (sender.GetStatus(e.Row) != PXEntryStatus.Notchanged)
				e.Cancel = true;
		}

		public virtual string GetDefaultTaxZone(CRQuote row) {
			string result = null;
			var customerLocation = (Location)PXSelect<Location,
						Where<Location.bAccountID, Equal<Required<CRQuote.bAccountID>>,
							And<Location.locationID, Equal<Required<CRQuote.locationID>>>>>.
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
					if (address != null)
					{
						result = TaxBuilderEngine.GetTaxZoneByAddress(this, address);
					}
				}
			}
			if (customerLocation == null && result == null)
			{
				var branchLocation = (Location)PXSelectJoin<Location,
					InnerJoin<Branch, On<Branch.branchID, Equal<Current<CRQuote.branchID>>>,
						InnerJoin<BAccount, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>>,
					Where<Location.locationID, Equal<BAccount.defLocationID>>>.Select(this);
				if (branchLocation != null && branchLocation.VTaxZoneID != null)
					result = branchLocation.VTaxZoneID;
				else
					result = row.TaxZoneID;
			}
			return result;
		}

		protected virtual void CRQuote_QuoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var quote = e.Row as CRQuote;
			if (quote == null) return;

			object noteID = quote.NoteID;
			if (noteID == null)
			{
				sender.RaiseFieldDefaulting<CRQuote.noteID>(quote, out noteID);
			}
			e.NewValue = noteID;
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<CRQuote.quoteType, Equal<CRQuoteTypeAttribute.distribution>>), Messages.OnlyDistributionQuotesAvailable)]
		protected virtual void CRQuote_QuoteNbr_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(
            typeof(Search<Location.taxRegistrationID,
                Where<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>,
                    And<Location.locationID, Equal<Current<CRQuote.locationID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void _(Events.CacheAttached<CRQuote.taxRegistrationID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(
            typeof(Search<Location.cAvalaraExemptionNumber,
                Where<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>,
                    And<Location.locationID, Equal<Current<CRQuote.locationID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void _(Events.CacheAttached<CRQuote.externalTaxExemptionNumber> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(
            TXAvalaraCustomerUsageType.Default,
            typeof(Search<Location.cAvalaraCustomerUsageType,
                Where<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>,
                    And<Location.locationID, Equal<Current<CRQuote.locationID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void _(Events.CacheAttached<CRQuote.avalaraCustomerUsageType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Search<BAccount.defLocationID, Where<BAccount.bAccountID, Equal<Current<CRQuote.bAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<CRQuote.locationID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(CRMBAccountAttribute), nameof(CRMBAccountAttribute.Enabled), true)]
		protected virtual void _(Events.CacheAttached<CRQuote.bAccountID> e) { }

		#region CROpportunity
		protected virtual void CRQuote_OpportunityID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = e.Row as CRQuote;
            if (row == null) return;            
            Opportunity.Cache.Current = Opportunity.SelectSingle(row.OpportunityID);
	        if (Opportunity.Cache.Current != null)
	        {
		        row.Subject = ((Standalone.CROpportunity) Opportunity.Cache.Current).Subject;
				if (IsSingleQuote(row.OpportunityID))
				{
					var opportunity = CurrentOpportunity.SelectSingle(row.OpportunityID);
					if (opportunity != null)
					{
						row.QuoteID = opportunity.QuoteNoteID;
						row.IsPrimary = true;
					}
				}
			}
        }

		[PXCustomizeBaseAttribute(typeof(PXSelectorAttribute), nameof(PXSelectorAttribute.DescriptionField), typeof(CROpportunity.subject))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRQuote.opportunityID> e) { }
        #endregion

        protected virtual void CRQuote_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {            
            PXNoteAttribute.SetTextFilesActivitiesRequired<CROpportunityProducts.noteID>(Products.Cache, null);
            
			CRQuote row = e.Row as CRQuote;
            if (row == null) return;

            PXUIFieldAttribute.SetEnabled<CRQuote.allowOverrideContactAddress>(cache, row, !(row.BAccountID == null && row.ContactID == null));
            Caches[typeof(CRContact)].AllowUpdate = row.AllowOverrideContactAddress == true;
            Caches[typeof(CRAddress)].AllowUpdate = row.AllowOverrideContactAddress == true;

            PXUIFieldAttribute.SetEnabled<CRQuote.curyAmount>(cache, row, row.ManualTotalEntry == true);
			bool isCustomerDiscountsFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>();
			PXUIFieldAttribute.SetEnabled<CRQuote.curyDiscTot>(cache, row, row.ManualTotalEntry == true || isCustomerDiscountsFeatureInstalled == false);

			PXUIFieldAttribute.SetEnabled<CRQuote.locationID>(cache, row, row.BAccountID != null);
            PXDefaultAttribute.SetPersistingCheck<CRQuote.locationID>(cache, row, row.BAccountID == null ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

            PXUIFieldAttribute.SetVisible<CRQuote.curyID>(cache, row, IsMultyCurrency);

			var line = String.Format(Messages.QuoteGridProductText,
				this.Quote.Cache.GetValueExt<CRQuote.curyExtPriceTotal>(row),
				this.Quote.Cache.GetValueExt<CRQuote.curyLineDiscountTotal>(row));
			foreach (CROpportunityProducts product in this.Products.Select())
			{
				product.TextForProductsGrid = line;
				 PXEntryStatus oldstatus = this.Products.Cache.GetStatus(product);
                this.Products.Cache.SetStatus(product, PXEntryStatus.Updated);
                this.Products.Cache.SetStatus(product, oldstatus);
            }


			if (row.OpportunityIsActive == false)
			{
				cache.RaiseExceptionHandling<CRQuote.opportunityID>(row, row.OpportunityID, 
					new PXSetPropertyException(Messages.OpportunityIsNotActive, PXErrorLevel.Warning));

				cache.AdjustUI(row)
					.ForAllFields(f => f.Enabled = false)
					.For<CRQuote.quoteID>(f => f.Enabled = true)
					.SameFor<CRQuote.quoteNbr>()
					.SameFor<CRQuote.opportunityID>()
					.SameFor<CRQuote.subject>()
					.SameFor<CRQuote.documentDate>()
					.SameFor<CRQuote.expirationDate>();

				DisableFields<CRContact>();
				DisableFields<CRAddress>();
				DisableFields<CRBillingContact>();
				DisableFields<CRBillingAddress>();
				DisableFields<CRShippingContact>();
				DisableFields<CRShippingAddress>();

				void DisableFields<TTable>() where TTable : class, IBqlTable, new()
				{
					this.Caches<TTable>().AllowUpdate = false;
				}
			}

			if (!UnattendedMode)
			{
				CRShippingAddress shipAddress = this.Shipping_Address.Select();
				CRBillingAddress billAddress = this.Billing_Address.Select();
				CRAddress contactAddress = this.Quote_Address.Select();
				bool enableAddressValidation = ((shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
												|| (billAddress != null && billAddress.IsDefaultAddress == false && billAddress.IsValidated == false)
												|| (contactAddress != null && (contactAddress.IsDefaultAddress == false || row.BAccountID == null && row.ContactID == null) && contactAddress.IsValidated == false));
				this.validateAddresses.SetEnabled(enableAddressValidation);
			}

			VisibilityHandler(cache, row);
		}

        protected virtual void CRQuote_BAccountID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            CRQuote row = e.Row as CRQuote;
            if (row == null) return;

            if (row.BAccountID < 0)
                e.ReturnValue = "";
        }        
                
		protected virtual void CRQuote_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            var row = e.Row as CRQuote;
            if (row == null) return;

			row.NoteID = row.QuoteID;

            object newContactId = row.ContactID;
            if (newContactId != null && !VerifyField<CRQuote.contactID>(row, newContactId))
                row.ContactID = null;

            object newLocationId = row.LocationID;
            if (newLocationId == null || !VerifyField<CRQuote.locationID>(row, newLocationId))
            {
                cache.SetDefaultExt<CRQuote.locationID>(row);
            }

            if (row.ContactID == null)
                cache.SetDefaultExt<CRQuote.contactID>(row);

            if (row.TaxZoneID == null)
                cache.SetDefaultExt<CRQuote.taxZoneID>(row);

            foreach (var product in Products.Select().RowCast<CROpportunityProducts>())                
            {
                Products.Cache.Update(product);
            }

            if (IsFirstQuote(row.OpportunityID))
            {
                CROpportunityRevision firstrevision = PXSelect<CROpportunityRevision,
                    Where<CROpportunityRevision.opportunityID, Equal<Required<CROpportunityRevision.opportunityID>>>>.SelectSingleBound(this, null, new object[] { row.OpportunityID });

				if (firstrevision != null)
				{
					cache.SetValueExt(row, typeof(CRQuote.curyInfoID).Name, firstrevision.CuryInfoID);
					var Discount = this.GetExtension<QuoteMaint.Discount>();
					Discount.RefreshTotalsAndFreeItems(Discount.Details.Cache);
				}
            }
        }


        protected virtual void CRQuote_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var oldRow = e.OldRow as CRQuote;
            var row = e.Row as CRQuote;
            if (oldRow == null || row == null) return;

            if (row.ContactID != null && row.ContactID != oldRow.ContactID)
            {
                object newCustomerId = row.BAccountID;
                if (newCustomerId == null)
                    FillDefaultBAccountID(row);
            }

            var customerChanged = row.BAccountID != oldRow.BAccountID;
            object newLocationId = row.LocationID;
			bool locationChanged = !sender.ObjectsEqual<CRQuote.locationID>(e.Row, e.OldRow);
			if ((locationChanged || customerChanged) && (newLocationId == null || !VerifyField<CRQuote.locationID>(row, newLocationId)))
			{
                sender.SetDefaultExt<CRQuote.locationID>(row);
            }

            if (customerChanged)
                sender.SetDefaultExt<CRQuote.taxZoneID>(row);

            var docDateChanged = row.DocumentDate != oldRow.DocumentDate;
            var projectChanged = row.ProjectID != oldRow.ProjectID;
            if (locationChanged || docDateChanged || projectChanged || customerChanged)
            {
                var productsCache = Products.Cache;
                foreach (CROpportunityProducts line in SelectProducts(row.QuoteID))
                {
                    var lineCopy = (CROpportunityProducts)productsCache.CreateCopy(line);
                    lineCopy.ProjectID = row.ProjectID;
                    lineCopy.CustomerID = row.BAccountID;
                    productsCache.Update(lineCopy);
                }
                sender.SetDefaultExt<CRQuote.taxCalcMode>(row);
            }

            foreach (CROpportunityProducts product in this.Products.Select())
            {
                product.TextForProductsGrid = String.Format(Messages.QuoteGridProductText, row.CuryExtPriceTotal.ToString(), row.CuryLineDiscountTotal.ToString());
                PXEntryStatus oldstatus = this.Products.Cache.GetStatus(product);
                this.Products.Cache.SetStatus(product, PXEntryStatus.Updated);
                this.Products.Cache.SetStatus(product, oldstatus);
            }

            if (locationChanged)
			{
                sender.SetDefaultExt<CRQuote.taxZoneID>(row);
                sender.SetDefaultExt<CRQuote.taxRegistrationID>(row);
                sender.SetDefaultExt<CRQuote.externalTaxExemptionNumber>(row);
                sender.SetDefaultExt<CRQuote.avalaraCustomerUsageType>(row);
            }
        }

        protected virtual void CRQuote_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            CRQuote row = e.Row as CRQuote;
            if (row == null) return;
            
            bool IsPrimary = (bool)sender.GetValue<CRQuote.isPrimary>(row);
            if (IsPrimary && !IsSingleQuote(row.OpportunityID))
            {
                throw new PXException(ErrorMessages.PrimaryQuote);
            }
        }

        protected virtual void CRQuote_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (CRQuote)e.Row;
            if (row == null) return;

            if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.BAccountID != null)
            {
                PXDefaultAttribute.SetPersistingCheck<CRQuote.locationID>(sender, e.Row, PXPersistingCheck.NullOrBlank);
            }
        }

        protected virtual void CRQuote_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            CRQuote row = e.Row as CRQuote;
            if (row == null) return;

	        if (e.Operation == PXDBOperation.Insert && this.Opportunity.Current != null && e.TranStatus == PXTranStatus.Open)
	        {
		        //Persist counter manually, removed from Views.Caches by multicurrenry.
		        PXDatabase.Update<Standalone.CROpportunity>(
			        new PXDataFieldAssign<Standalone.CROpportunity.defQuoteID>(row.IsPrimary == true ? row.QuoteID : this.Opportunity.Current.DefQuoteID),
					new PXDataFieldRestrict<Standalone.CROpportunity.opportunityID>(PXDbType.VarChar, 255, this.Opportunity.Current.OpportunityID, PXComp.EQ)
		        );
	        }
        }

	    protected void SuppressCascadeDeletion(PXView view, object row)
	    {
		    PXCache cache = this.Caches[row.GetType()];
		    foreach (object rec in view.Cache.Deleted)
		    {
			    if (view.Cache.GetStatus(rec) == PXEntryStatus.Deleted)
			    {					
				    bool own = true;
				    foreach (string key in new[]{typeof(CROpportunity.quoteNoteID).Name})
				    {
					    if (!object.Equals(cache.GetValue(row, key), view.Cache.GetValue(rec, key)))
					    {
						    own = false;
						    break;
					    }
				    }
					if(own)
						view.Cache.SetStatus(rec, PXEntryStatus.Notchanged);
			    }
		    }
	    }

		protected virtual void VisibilityHandler(PXCache sender, CRQuote row)
		{
			Standalone.CROpportunity opportunity = PXSelect<Standalone.CROpportunity,
				Where<Standalone.CROpportunity.opportunityID, Equal<Required<Standalone.CROpportunity.opportunityID>>>>.Select(this, row.OpportunityID).FirstOrDefault();

			if (opportunity != null)
			{
				bool allowUpdate = row.IsDisabled != true && opportunity.IsActive == true;

				foreach (var type in new[]
						{
							typeof(CROpportunityDiscountDetail),
							typeof(CROpportunityProducts),
							typeof(CRTaxTran),
						})
				{
					this.Caches[type].AllowInsert
						= this.Caches[type].AllowUpdate
						= this.Caches[type].AllowDelete = allowUpdate;
				}

				this.Caches[typeof(CopyQuoteFilter)].AllowUpdate = true;
				this.Caches[typeof(RecalcDiscountsParamFilter)].AllowUpdate = true;
			}
		}

	    protected virtual void CROpportunityRevision_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
	    {
		    //Suppress update revision for quote, should be done by main DAC
		    CROpportunityRevision row = (CROpportunityRevision)e.Row;
		    if (row != null && this.Quote.Current != null &&
		        row.NoteID == this.Quote.Current.QuoteID)
			    e.Cancel = true;
	    }
		#endregion

		#region CROpportunityProducts
	    
	    [PXDBLong]
	    [CurrencyInfo(typeof(CRQuote.curyInfoID))]
	    protected virtual void CROpportunityProducts_CuryInfoID_CacheAttached(PXCache e)
	    {
	    }

		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(CRQuote.quoteID))]
		[PXParent(typeof(Select<CRQuote,
			 Where<CRQuote.quoteID, Equal<Current<CROpportunityProducts.quoteID>>>>))]
		protected virtual void CROpportunityProducts_QuoteID_CacheAttached(PXCache e)
		{
        }

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts, CROpportunityProducts.vendorID> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.POCreate == false || e.Row.InventoryID == null)
				e.Cancel = true;
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.pOCreate> e)
		{
			if (e.Row == null)
				return;

			e.Cache.SetDefaultExt<CROpportunityProducts.vendorID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.inventoryID> e)
		{
			if (e.Row == null)
				return;

			e.Cache.SetValueExt<CROpportunityProducts.pOCreate>(e.Row, false);
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts.projectID> e)
		{
			e.NewValue = QuoteCurrent.Current.ProjectID;
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts.customerID> e)
		{
			e.NewValue = QuoteCurrent.Current.BAccountID;
		}

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(CRQuote.productCntr))]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Line Nbr.")]
		protected virtual void CROpportunityProducts_LineNbr_CacheAttached(PXCache e)
        {           
        }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price", Visible = false)]
        protected virtual void CROpportunityProducts_ManualPrice_CacheAttached(PXCache e)
        {
        }

		[PXDBInt]
		[PXDBDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void CROpportunityProducts_CustomerID_CacheAttached(PXCache e)
		{
		}

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

			PXUIFieldAttribute.SetEnabled<CROpportunityProducts.skipLineDiscounts>(sender, row, this.IsCopyPasteContext);
        }

        [PopupMessage]
        [PXRestrictor(typeof(Where<
            InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
            And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
            And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>>>), IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<CROpportunityProducts.inventoryID> e) { }

        protected virtual void CROpportunityProducts_IsFree_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CROpportunityProducts row = e.Row as CROpportunityProducts;
            if (row == null) return;

            if (row.InventoryID != null && row.IsFree == false)
            {
                Caches[typeof(CROpportunityProducts)].SetDefaultExt<CROpportunityProducts.curyUnitPrice>(row);
            }
        }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictSiteByBranch(typeof(CRQuote.branchID), ShowWarning = true)]
		[PXDefault(typeof(
				Coalesce<
					Search<Location.cSiteID, Where<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>, And<Location.locationID, Equal<Current<CRQuote.locationID>>>>>,
					Search<InventoryItemCurySettings.dfltSiteID,
						Where<InventoryItemCurySettings.inventoryID, Equal<Current<CROpportunityProducts.inventoryID>>,
							And<InventoryItemCurySettings.curyID, EqualBaseCuryID<Current2<CRQuote.branchID>>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.siteID> e)
		{
		}
        #endregion

        #region CROpportunityDiscountDetail    
        public PXSelect<CROpportunityDiscountDetail,
				Where<CROpportunityDiscountDetail.quoteID, Equal<Current<CRQuote.quoteID>>>,
				OrderBy<Asc<CROpportunityDiscountDetail.lineNbr>>>
			_DiscountDetails;

		[PXDBGuid(IsKey = true)]
	    [PXDBDefault(typeof(CRQuote.quoteID))]
		protected virtual void CROpportunityDiscountDetail_QuoteID_CacheAttached(PXCache sender)
		{
	    }

		[PXDBUShort()]
		[PXLineNbr(typeof(CRQuote))]
		protected virtual void CROpportunityDiscountDetail_LineNbr_CacheAttached(PXCache e)
		{
		}

		#endregion

		#region CROpportunityTax
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(CRQuote.quoteID))]
		[PXParent(typeof(Select<CRQuote,
			Where<CRQuote.quoteID, Equal<Current<CROpportunityTax.quoteID>>>>))]
		protected virtual void CROpportunityTax_QuoteID_CacheAttached(PXCache sender)
		{
        }

	    [PXDBLong]
	    [CurrencyInfo(typeof(CRQuote.curyInfoID))]
	    protected virtual void CROpportunityTax_CuryInfoID_CacheAttached(PXCache e)
	    {
	    }


		#endregion

		#region CRTaxTran
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(CRQuote.quoteID))]
		[PXParent(typeof(Select<CRQuote,
			Where<CRQuote.quoteID, Equal<Current<CRTaxTran.quoteID>>>>))]
		protected virtual void CRTaxTran_QuoteID_CacheAttached(PXCache sender)
		{
        }

	    [PXDBLong]
	    [CurrencyInfo(typeof(CRQuote.curyInfoID))]
	    protected virtual void CRTaxTran_CuryInfoID_CacheAttached(PXCache e)
	    {
	    }

		#endregion

		#region BAccountR

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), Messages.BAccountCD)]
		protected virtual void _(Events.CacheAttached<BAccount.acctCD> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), Messages.BAccountName)]
		protected virtual void _(Events.CacheAttached<BAccount.acctName> e) { }

		[PXBool]
        [PXDefault(false)]
        [PXDBCalced(typeof(True), typeof(Boolean))]
        protected virtual void BAccountR_ViewInCrm_CacheAttached(PXCache sender)
        {
        }

        #endregion

        #region Private Methods

        private BAccount SelectAccount(string acctCD)
        {
            if (string.IsNullOrEmpty(acctCD)) return null;
            return (BAccount)PXSelectReadonly<BAccount,
                    Where<BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>.
                Select(this, acctCD);
        }

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

        private void FillDefaultBAccountID(CRQuote row)
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
                }
             }
        }

        private bool IsMultyCurrency
        {
            get { return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>(); }
        }

        private bool ProrateDiscount
        {
            get
            {
                SOSetup sosetup = PXSelect<SOSetup>.Select(this);

                if (sosetup == null)
                    return true; //default true

                if (sosetup.ProrateDiscounts == null)
                    return true;

                return sosetup.ProrateDiscounts == true;
            }
        }

        private bool IsSingleQuote(string opportunityId)
        {
            var quote = PXSelect<CRQuote, Where<CRQuote.opportunityID, Equal<Optional<CRQuote.opportunityID>>>>.SelectSingleBound(this, null, opportunityId);
            return (quote.Count == 0);
        }

        private bool IsFirstQuote(string opportunityId)
        {
            var quote = PXSelectReadonly<CRQuote, Where<CRQuote.opportunityID, Equal<Required<CRQuote.opportunityID>>>>.SelectSingleBound(this, null, opportunityId);
            return (quote.Count == 0);
        }

        private CRQuote SelectSingleQuote(string opportunityId)
        {
            if (opportunityId == null) return null;

            var opportunity = (CRQuote)PXSelect<CRQuote,
                    Where<CRQuote.opportunityID, Equal<Required<CRQuote.opportunityID>>>>.
                Select(this, opportunityId);
            return opportunity;
        }

        private IEnumerable SelectProducts(object quoteId)
        {
            if (quoteId == null)
                return new CROpportunityProducts[0];

            return PXSelect<CROpportunityProducts,
                    Where<CROpportunityProducts.quoteID, Equal<Required<CRQuote.quoteID>>>>.
                Select(this, quoteId).
                RowCast<CROpportunityProducts>();
        }

        private IEnumerable SelectDiscountDetails(object quoteId)
        {
            if (quoteId == null)
                return new CROpportunityDiscountDetail[0];

            return PXSelect<CROpportunityDiscountDetail,
                    Where<CROpportunityDiscountDetail.quoteID, Equal<Required<CRQuote.quoteID>>>>.
                Select(this, quoteId).
                RowCast<CROpportunityDiscountDetail>();
        }


        private Contact FillFromOpportunityContact(Contact Contact)
        {
            CRContact _CRContact = Quote_Contact.SelectSingle();

            Contact.FullName = _CRContact.FullName;
            Contact.Title = _CRContact.Title;
            Contact.FirstName = _CRContact.FirstName;
            Contact.LastName = _CRContact.LastName;
            Contact.Salutation = _CRContact.Salutation;
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
            CRAddress _CRAddress = Quote_Address.SelectSingle();

            Address.AddressLine1 = _CRAddress.AddressLine1;
            Address.AddressLine2 = _CRAddress.AddressLine2;
            Address.City = _CRAddress.City;
            Address.CountryID = _CRAddress.CountryID;
            Address.State = _CRAddress.State;
            Address.PostalCode = _CRAddress.PostalCode;
            return Address;
        }

        private bool IsDefaultContactAdress()
        {
            CRAddress _CRAddress = Quote_Address.SelectSingle();
            CRContact _CRContact = Quote_Contact.SelectSingle();

            if (_CRContact != null && _CRAddress != null)
            {
                bool IsDirtya = Quote_Address.Cache.IsDirty;
                bool IsDirtyc = Quote_Contact.Cache.IsDirty;

                CRAddress _etalonCRAddress = Quote_Address.Insert();
                CRContact _etalonCRContact = Quote_Contact.Insert();

                Quote_Address.Cache.SetStatus(_etalonCRAddress, PXEntryStatus.Held);
                Quote_Contact.Cache.SetStatus(_etalonCRContact, PXEntryStatus.Held);

                Quote_Address.Cache.IsDirty = IsDirtya;
                Quote_Contact.Cache.IsDirty = IsDirtyc;

                if (_CRContact.FullName != _etalonCRContact.FullName)
                    return false;
                if (_CRContact.Title != _etalonCRContact.Title)
                    return false;
                if (_CRContact.FirstName != _etalonCRContact.FirstName)
                    return false;
                if (_CRContact.LastName != _etalonCRContact.LastName)
                    return false;
                if (_CRContact.Salutation != _etalonCRContact.Salutation)
                    return false;
                if (_CRContact.Email != _etalonCRContact.Email)
                    return false;
                if (_CRContact.Phone1 != _etalonCRContact.Phone1)
                    return false;
                if (_CRContact.Phone1Type != _etalonCRContact.Phone1Type)
                    return false;
                if (_CRContact.Phone2 != _etalonCRContact.Phone2)
                    return false;
                if (_CRContact.Phone2Type != _etalonCRContact.Phone2Type)
                    return false;
                if (_CRContact.Phone3 != _etalonCRContact.Phone3)
                    return false;
                if (_CRContact.Phone3Type != _etalonCRContact.Phone3Type)
                    return false;
                if (_CRContact.Fax != _etalonCRContact.Fax)
                    return false;
                if (_CRContact.FaxType != _etalonCRContact.FaxType)
                    return false;

                if (_CRAddress.AddressLine1 != _etalonCRAddress.AddressLine1)
                    return false;
                if (_CRAddress.AddressLine2 != _CRAddress.AddressLine2)
                    return false;
                if (_CRAddress.City != _CRAddress.City)
                    return false;
                if (_CRAddress.State != _CRAddress.State)
                    return false;
                if (_CRAddress.CountryID != _CRAddress.CountryID)
                    return false;
                if (_CRAddress.PostalCode != _CRAddress.PostalCode)
                    return false;
            }
            return true;
        }

        private bool IsContactAddressNoChanged(Contact _etalonCRContact, Address _etalonCRAddress)
        {
            if (_etalonCRContact == null || _etalonCRAddress == null)
            {
                return false;
            }

            CRAddress _CRAddress = Quote_Address.SelectSingle();
            CRContact _CRContact = Quote_Contact.SelectSingle();

            if (_CRContact != null && _CRAddress != null)
            {
                if (_CRContact.FullName != _etalonCRContact.FullName)
                    return false;
                if (_CRContact.Title != _etalonCRContact.Title)
                    return false;
                if (_CRContact.LastName != _etalonCRContact.LastName)
                    return false;
                if (_CRContact.FirstName != _etalonCRContact.FirstName)
                    return false;
                if (_CRContact.Salutation != _etalonCRContact.Salutation)
                    return false;
                if (_CRContact.Email != _etalonCRContact.EMail)
                    return false;
                if (_CRContact.Phone1 != _etalonCRContact.Phone1)
                    return false;
                if (_CRContact.Phone1Type != _etalonCRContact.Phone1Type)
                    return false;
                if (_CRContact.Phone2 != _etalonCRContact.Phone2)
                    return false;
                if (_CRContact.Phone2Type != _etalonCRContact.Phone2Type)
                    return false;
                if (_CRContact.Phone3 != _etalonCRContact.Phone3)
                    return false;
                if (_CRContact.Phone3Type != _etalonCRContact.Phone3Type)
                    return false;
                if (_CRContact.Fax != _etalonCRContact.Fax)
                    return false;
                if (_CRContact.FaxType != _etalonCRContact.FaxType)
                    return false;

                if (_CRAddress.AddressLine1 != _etalonCRAddress.AddressLine1)
                    return false;
                if (_CRAddress.AddressLine2 != _etalonCRAddress.AddressLine2)
                    return false;
                if (_CRAddress.City != _etalonCRAddress.City)
                    return false;
                if (_CRAddress.State != _etalonCRAddress.State)
                    return false;
                if (_CRAddress.CountryID != _etalonCRAddress.CountryID)
                    return false;
                if (_CRAddress.PostalCode != _etalonCRAddress.PostalCode)
                    return false;
            }
            else
            {
                return false;
            }
            return true;
        }
		#endregion

		#region Avalara Tax

		public virtual bool IsExternalTax(string taxZoneID)
	    {
		    return false;
	    }

	    public virtual CRQuote CalculateExternalTax(CRQuote quote)
	    {
		    return quote;
	    }

		#endregion

		public override void Persist()
        {
	        foreach (CRQuote quote in this.Quote.Cache.Deleted)
	        {
		        if (IsSingleQuote(quote.OpportunityID))
		        {
					//Suppress cascace deleting
			        SuppressCascadeDeletion(this.Products.View, quote);
			        SuppressCascadeDeletion(this.Taxes.View, quote);
			        SuppressCascadeDeletion(this.TaxLines.View, quote);
			        SuppressCascadeDeletion(this._DiscountDetails.View, quote);
				}
	        }

	        base.Persist();
		}

		#region Implementation of IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, "Products", true) == 0)
			{
				if (values.Contains("opportunityID"))
					values["opportunityID"] = Quote.Current.OpportunityID;
				else
					values.Add("opportunityID", Quote.Current.OpportunityID);
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
        public class MultiCurrency : CRMultiCurrencyGraph<QuoteMaint, CRQuote>
        {
            #region Initialization 

            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CRQuote)) { DocumentDate = typeof(CRQuote.documentDate) };
            }

			#endregion

			#region Overrides

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.Quote,
					Base.Products,
					Base.TaxLines,
					Base.Taxes,
					Base.GetExtension<Discount>().DiscountDetails
				};
			}

			protected override BAccount GetRelatedBAccount() => CRQuote.FK.BusinessAccount.Dirty.FindParent(Base, Base.Quote.Current);

			protected override Type BAccountField => typeof(CRQuote.bAccountID);

			protected override PXView DetailsView => Base.Products.View;

			#endregion

			#region Events

			protected override bool AllowOverrideCury()
			{
				if (Base.QuoteCurrent.Current?.OpportunityIsActive is false)
				{
					return false;
				}

				return base.AllowOverrideCury();
			}

			#endregion
        }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class SalesPrice : SalesPriceGraph<QuoteMaint, CRQuote>
        {
            #region Initialization 

            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CRQuote)) { CuryInfoID = typeof(CRQuote.curyInfoID) };
            }
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyExtPrice), Descr = typeof(CROpportunityProducts.descr) };
            }
            protected override PriceClassSourceMapping GetPriceClassSourceMapping()
            {
                return new PriceClassSourceMapping(typeof(Location)) { PriceClassID = typeof(Location.cPriceClassID) };
            }            

            #endregion
        }

        /// <exclude/>
        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class Discount : DiscountGraph<QuoteMaint, CRQuote>
        {
            #region Initialization 

            protected override bool AddDocumentDiscount => true;

            public override void Initialize()
            {
                base.Initialize();
                if (this.Discounts == null)
                    this.Discounts = new PXSelectExtension<PX.Objects.Extensions.Discount.Discount>(this.DiscountDetails);
            }
            protected override DocumentMapping GetDocumentMapping()
            {
				return new DocumentMapping(typeof(CRQuote)) { CuryDiscTot = typeof(CRQuote.curyLineDocDiscountTotal) };
			}
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyAmount), Quantity = typeof(CROpportunityProducts.quantity) };
            }
            protected override DiscountMapping GetDiscountMapping()
            {
                return new DiscountMapping(typeof(CROpportunityDiscountDetail));
            }

            #endregion

            #region Views

            [PXViewName(Messages.DiscountDetails)]
            public PXSelect<CROpportunityDiscountDetail,
                    Where<CROpportunityDiscountDetail.quoteID, Equal<Current<CRQuote.quoteID>>>,
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
            [CurrencyInfo(typeof(CRQuote.curyInfoID))]            
			public override void Discount_CuryInfoID_CacheAttached(PXCache sender)
            {
            }

            protected virtual void Discount_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
            {
                if (Base.IsExternalTax(Base.Quote.Current.TaxZoneID))
                {
                    Base.Quote.Current.IsTaxValid = false;
                }
            }
            protected virtual void Discount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
            {
                if (Base.IsExternalTax(Base.Quote.Current.TaxZoneID))
                {
                    Base.Quote.Current.IsTaxValid = false;
                }
            }
            protected virtual void Discount_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
            {
                if (Base.IsExternalTax(Base.Quote.Current.TaxZoneID))
                {
                    Base.Quote.Current.IsTaxValid = false;
                }
            }

            #endregion

            #region Overrides

            protected override void DefaultDiscountAccountAndSubAccount(PX.Objects.Extensions.Discount.Detail det)
            {
            }

            #endregion

            #region Actions

            public PXAction<CRQuote> graphRecalculateDiscountsAction;
            [PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
            [PXButton]
            public virtual IEnumerable GraphRecalculateDiscountsAction(PXAdapter adapter)
            {
                List<CRQuote> CRQoutes = new List<CRQuote>(adapter.Get().Cast<CRQuote>());
                foreach (CRQuote quote in CRQoutes)
                {
                    if (recalcdiscountsfilter.View.Answer == WebDialogResult.None)
                    {
                        recalcdiscountsfilter.Cache.Clear();
                        RecalcDiscountsParamFilter filterdata = recalcdiscountsfilter.Cache.Insert() as RecalcDiscountsParamFilter;
                        filterdata.RecalcUnitPrices = true;
                        filterdata.RecalcDiscounts = true;
                        filterdata.OverrideManualPrices = false;
                        filterdata.OverrideManualDiscounts = false;
						filterdata.OverrideManualDocGroupDiscounts = false;
					}

                    if (recalcdiscountsfilter.AskExt() != WebDialogResult.OK)
                        return CRQoutes;

                    RecalculateDiscountsAction(adapter);                    
                }
                return CRQoutes;              
            }

            #endregion
        }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class SalesTax : TaxGraph<QuoteMaint, CRQuote>
        {
            #region Initialization 

            protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }

			protected override PXView DocumentDetailsView => Base.Products.View;

			protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CRQuote))
                {
                    BranchID = typeof(CRQuote.branchID),
                    CuryID = typeof(CRQuote.curyID),
                    CuryInfoID = typeof(CRQuote.curyInfoID),
                    DocumentDate = typeof(CRQuote.documentDate),
                    //FinPeriodID = null,
                    TaxZoneID = typeof(CRQuote.taxZoneID),
                    TermsID = typeof(CRQuote.termsID),
                    CuryLinetotal = typeof(CRQuote.curyLineTotal),
                    CuryDiscountLineTotal = typeof(CRQuote.curyLineDiscountTotal),
                    CuryExtPriceTotal = typeof(CRQuote.curyExtPriceTotal),
                    CuryDocBal = typeof(CRQuote.curyProductsAmount),
                    CuryTaxTotal = typeof(CRQuote.curyTaxTotal),
                    CuryDiscTot = typeof(CRQuote.curyDiscTot),
                    //CuryDiscAmt = null,
                    //CuryOrigWhTaxAmt = null,
                    //CuryTaxRoundDiff = null,
                    //TaxRoundDiff = null,
                    //IsTaxSaved = null,
                    TaxCalcMode = typeof(CRQuote.taxCalcMode)
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
            protected virtual void _(Events.FieldUpdated<CRQuote, CRQuote.curyDiscTot> e)
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

            protected virtual void _(Events.FieldUpdated<CRQuote, CRQuote.manualTotalEntry> e)
            {
                if (e.Row != null && e.Row.ManualTotalEntry == false)
                {
                    CalcTotals(null, false);
                }
            }
            protected virtual void Document_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
            {
                var row = sender.GetExtension<PX.Objects.Extensions.SalesTax.Document>(e.Row);
                if (row != null && row.TaxCalc == null)
                    row.TaxCalc = TaxCalc.Calc;
            }

            #endregion

            #region Overrides

            protected override void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
            {
                base.CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);


                CRQuote doc = (CRQuote)this.Documents.Cache.GetMain<PX.Objects.Extensions.SalesTax.Document>(this.Documents.Current);
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

                if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<CRQuote.curyProductsAmount>() ?? 0m)) == false)
                {
                    ParentSetValue<CRQuote.curyProductsAmount>(CuryDocTotal);
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
				IComparer<Tax> taxComparer = GetTaxByCalculationLevelComparer();
				taxComparer.ThrowOnNull(nameof(taxComparer));

				Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();                
	            var currents = new[]
                {
                    row != null && row is PX.Objects.Extensions.SalesTax.Detail ? Details.Cache.GetMain((PX.Objects.Extensions.SalesTax.Detail)row):null,
					((QuoteMaint)graph).Quote.Current
                };                

                foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
                        LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
                            And<TaxRev.outdated, Equal<boolFalse>,
                                And<TaxRev.taxType, Equal<TaxType.sales>,
                                    And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                                        And<Tax.taxType, NotEqual<CSTaxType.use>,
                                            And<Tax.reverseTax, Equal<boolFalse>,
                                                And<Current<CRQuote.documentDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
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
                                Where<CROpportunityTax.quoteID, Equal<Current<CRQuote.quoteID>>,
                                        And <CROpportunityTax.quoteID, Equal<Current<CROpportunityProducts.quoteID>>,
                                            And <CROpportunityTax.lineNbr, Equal<Current<CROpportunityProducts.lineNbr>>>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && taxComparer.Compare((PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcLine:
                        foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
                                Where<CROpportunityTax.quoteID, Equal<Current<CRQuote.quoteID>>,
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
                                Where<CRTaxTran.quoteID, Equal<Current<CRQuote.quoteID>>>,
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
                            Where<CROpportunityProducts.quoteID, Equal<Current<CRQuote.quoteID>>>>
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
		public class ContactAddress : CROpportunityContactAddressExt<QuoteMaint>
		{
			#region Overrides

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<Extensions.CROpportunityContactAddress.DocumentAddress>(Base.Quote_Address);
				Contacts = new PXSelectExtension<Extensions.CROpportunityContactAddress.DocumentContact>(Base.Quote_Contact);
			}
			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRQuote))
				{
					DocumentAddressID = typeof(CRQuote.opportunityAddressID),
					DocumentContactID = typeof(CRQuote.opportunityContactID),
					ShipAddressID = typeof(CRQuote.shipAddressID),
					ShipContactID = typeof(CRQuote.shipContactID),
					BillAddressID = typeof(CRQuote.billAddressID),
					BillContactID = typeof(CRQuote.billContactID),
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
				return Base.Quote_Contact.Cache;
			}

			protected override PXCache GetAddressCache()
			{
				return Base.Quote_Address.Cache;
			}

			protected override PXCache GetShippingContactCache()
			{
				return Base.Shipping_Contact.Cache;
			}

			protected override PXCache GetShippingAddressCache()
			{
				return Base.Shipping_Address.Cache;
			}

			protected override PXCache GetBillingContactCache()
			{
				return Base.Billing_Contact.Cache;
			}

			protected override PXCache GetBillingAddressCache()
			{
				return Base.Billing_Address.Cache;
			}

			protected override IPersonalContact SelectContact()
			{
				return Base.Quote_Contact.SelectSingle();
			}

			protected override IPersonalContact SelectShippingContact()
			{
				return Base.Shipping_Contact.SelectSingle();
			}

			protected override IPersonalContact SelectBillingContact()
			{
				return Base.Billing_Contact.SelectSingle();
			}

			protected override IPersonalContact GetEtalonContact()
			{
				return SafeGetEtalon(Base.Quote_Contact.Cache) as IPersonalContact;
			}

			protected override IPersonalContact GetEtalonShippingContact()
			{
				return SafeGetEtalon(Base.Shipping_Contact.Cache) as IPersonalContact;
			}

			protected override IPersonalContact GetEtalonBillingContact()
			{
				return SafeGetEtalon(Base.Billing_Contact.Cache) as IPersonalContact;
			}

			protected override IAddress SelectAddress()
			{
				return Base.Quote_Address.SelectSingle();
			}

			protected override IAddress SelectShippingAddress()
			{
				return Base.Shipping_Address.SelectSingle();
			}

			protected override IAddress SelectBillingAddress()
			{
				return Base.Billing_Address.SelectSingle();
			}

			protected override IAddress GetEtalonAddress()
			{
				return SafeGetEtalon(Base.Quote_Address.Cache) as IAddress;
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

			#region Cache Attached

			[PXMergeAttributes(Method = MergeMethod.Merge)]
			[PXDBDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual void _(Events.CacheAttached<CRContact.bAccountID> e) { }

			[PXMergeAttributes(Method = MergeMethod.Merge)]
			[PXDBDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual void _(Events.CacheAttached<CRAddress.bAccountID> e) { }

			[PXMergeAttributes(Method = MergeMethod.Merge)]
			[PXDBDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual void _(Events.CacheAttached<CRShippingContact.bAccountID> e) { }

			[PXMergeAttributes(Method = MergeMethod.Merge)]
			[PXDBDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual void _(Events.CacheAttached<CRShippingAddress.bAccountID> e) { }

			[PXMergeAttributes(Method = MergeMethod.Merge)]
			[PXDBDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual void _(Events.CacheAttached<CRBillingContact.bAccountID> e) { }

			[PXMergeAttributes(Method = MergeMethod.Merge)]
			[PXDBDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual void _(Events.CacheAttached<CRBillingAddress.bAccountID> e) { }

			#endregion
		}

		/// <exclude/>
		public class CRCreateSalesOrderExt : CRCreateSalesOrder<QuoteMaint.Discount, QuoteMaint, CRQuote>
		{
			#region Initialization

			public static bool IsActive() => IsExtensionActive();

			#endregion

			#region Events

			public virtual void _(Events.RowSelected<CRQuote> e)
			{
				CRQuote row = e.Row as CRQuote;
				if (row == null)
					return;

				bool hasProducts = Base.Products.SelectSingle() != null;

				var products = Base.Products.View.SelectMultiBound(new object[] { row }).RowCast<CROpportunityProducts>();

				bool allProductsHasNoInventoryID = products.Any(_ => _.InventoryID == null) && !products.Any(_ => _.InventoryID != null);

				CreateSalesOrder
					.SetEnabled(hasProducts
						&& !allProductsHasNoInventoryID
						&& e.Row.BAccountID != null);
			}

			[PXUIField(DisplayName = "Set Quote as Primary", Visible = true)]
			[PXMergeAttributes(Method = MergeMethod.Merge)]
			public virtual void _(Events.CacheAttached<CreateSalesOrderFilter.makeQuotePrimary> e)
			{
			}

			#endregion

			#region Overrides

			public override CRQuote GetQuoteForWorkflowProcessing()
			{
				return Base.QuoteCurrent.Current;
			}

			public override void DoCreateSalesOrder()
			{
				CreateSalesOrderFilter filter = CreateOrderParams.Current;
				if (filter?.MakeQuotePrimary == true)
				{
					CRQuote quote = Base.QuoteCurrent?.Current;
					if (quote?.IsPrimary == false)
					{
						Base.Actions[nameof(Base.PrimaryQuote)].Press();
					}
				}
				base.DoCreateSalesOrder();
			}
			#endregion
		}

		/// <exclude/>
		public class CRCreateInvoiceExt : CRCreateInvoice<QuoteMaint.Discount, QuoteMaint, CRQuote>
		{
			#region Initialization

			public static bool IsActive() => IsExtensionActive();

			#endregion

			#region Events

			public virtual void _(Events.RowSelected<CRQuote> e)
			{
				CRQuote row = e.Row as CRQuote;
				if (row == null)
					return;

				bool hasProducts = Base.Products.SelectSingle() != null;

				var products = Base.Products.View.SelectMultiBound(new object[] { row }).RowCast<CROpportunityProducts>();

				bool allProductsHasNoInventoryID = products.Any(_ => _.InventoryID == null) && !products.Any(_ => _.InventoryID != null);

				CreateInvoice
					.SetEnabled(hasProducts
						&& !allProductsHasNoInventoryID
						&& e.Row.BAccountID != null);
			}

			[PXUIField(DisplayName = "Set Quote as Primary", Visible = true)]
			[PXMergeAttributes(Method = MergeMethod.Merge)]
			public virtual void _(Events.CacheAttached<CreateInvoicesFilter.makeQuotePrimary> e)
			{
			}

			#endregion

			#region Overrides

			public override CRQuote GetQuoteForWorkflowProcessing()
			{
				return Base.QuoteCurrent.Current;
			}

			protected override void DoCreateInvoice()
			{
				CreateInvoicesFilter filter = CreateInvoicesParams.Current;
				if (filter?.MakeQuotePrimary == true)
				{
					CRQuote quote = Base.QuoteCurrent?.Current;
					if (quote?.IsPrimary == false)
					{
						Base.Actions[nameof(Base.PrimaryQuote)].Press();
					}
				}
				base.DoCreateInvoice();
			}
			#endregion
		}

        #region Address Lookup Extension

        /// <exclude/>
        // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
        public class QuoteMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<QuoteMaint, CRQuote, CRAddress>
		{
			protected override string AddressView => nameof(Base.Quote_Address);
			protected override string ViewOnMap => nameof(Base.viewMainOnMap);
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class QuoteMaintShippingAddressLookupExtension : CR.Extensions.AddressLookupExtension<QuoteMaint, CRQuote, CRShippingAddress>
		{
			protected override string AddressView => nameof(Base.Shipping_Address);
			protected override string ViewOnMap => nameof(Base.ViewShippingOnMap);
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class QuoteMaintBillingAddressLookupExtension : CR.Extensions.AddressLookupExtension<QuoteMaint, CRQuote, CRBillingAddress>
		{
			protected override string AddressView => nameof(Base.Billing_Address);
			protected override string ViewOnMap => nameof(Base.ViewBillingOnMap);
		}

		#endregion

		/// <exclude/>
		public class ExtensionSort
			: SortExtensionsBy<ExtensionOrderFor<QuoteMaint>
				.FilledWith<
					ContactAddress,
					MultiCurrency,
					SalesPrice,
					Discount,
					SalesTax
				>>
		{ }

		#endregion
    }
}

