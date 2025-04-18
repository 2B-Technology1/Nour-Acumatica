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
using System.Web.Compilation;

using Autofac;

using PX.Api.Models;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Discount;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CROpportunityContactAddress;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.Extensions.Discount;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.SalesPrice;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.IN.Attributes;
using PX.Objects.SO;
using PX.Objects.TX;

using static PX.Objects.Common.Discount.DiscountEngine;
using static PX.Objects.PM.ProjectEntry;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PM
{
	public class PMQuoteMaint : PXGraph<PMQuoteMaint>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		private const string DefaultTaskCD = "0";
		private string PersistingTaskCD = null;
		private readonly Dictionary<string, string> PersistingTaskMap = new Dictionary<string, string>();

		[InjectDependency]
		public IUnitRateService RateService { get; set; }

		#region DAC Overrides (CacheAttached)

		#region CROpportunityProducts

		[PXDBLong]
		[CurrencyInfo(typeof(PMQuote.curyInfoID))]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.curyInfoID> e)
		{
		}

		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(PMQuote.quoteID))]
		[PXParent(typeof(Select<PMQuote,
			Where<PMQuote.quoteID, Equal<Current<CR.CROpportunityProducts.quoteID>>>>))]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.quoteID> e)
		{
		}

		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PMQuote.productCntr))]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.lineNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(PMQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.customerID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(CROpportunityProductLineTypeAttribute.scopeOfWork))]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.lineType> e)
		{
		}

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Price")]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.manualPrice> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Discount Amount")]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.curyDiscAmt> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Discount, %")]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.discPct> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Amount")]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.curyAmount> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Discount Code", Visible = false)]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.discountID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Manual Discount", Visible = false)]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.manualDisc> e)
		{
		}
		
		[PXDBCurrency(typeof(CROpportunityProducts.curyInfoID), typeof(CROpportunityProducts.extCost))]
		[PXUIField(DisplayName = "Ext. Cost")]
		[PXFormula(typeof(Mult<CROpportunityProducts.quantity, CROpportunityProducts.curyUnitCost>), typeof(SumCalc<PMQuote.curyCostTotal>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.curyExtCost> e)
		{
		}

		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXSelector(typeof(Search<PMAccountGroup.groupID, Where<PMAccountGroup.isExpense, Equal<True>>>), SubstituteKey = typeof(PMAccountGroup.groupCD))]
		[PXUIField(DisplayName = "Cost Account Group")]
		[PXDBInt]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.expenseAccountGroupID> e)
		{
		}

		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXSelector(typeof(Search<PMAccountGroup.groupID, Where<PMAccountGroup.type, Equal<GL.AccountType.income>>>), SubstituteKey = typeof(PMAccountGroup.groupCD))]
		[PXUIField(DisplayName = "Revenue Account Group")]
		[PXDBInt]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.revenueAccountGroupID> e)
		{
		}

		[PXDBString()]
		[PXUIField(DisplayName = "Project Task")]
		[PXDimensionSelector(ProjectTaskAttribute.DimensionName,
			typeof(Search<PMQuoteTask.taskCD, Where<PMQuoteTask.quoteID, Equal<Current<PMQuote.quoteID>>>>),
			 DirtyRead = true, DescriptionField = typeof(PMQuoteTask.description))]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.taskCD> e)
		{
		}

		[ProjectTask(typeof(CROpportunityProducts.projectID), Enabled = false)]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.taskID> e)
		{
		}

		[CostCode(null, null, GL.AccountType.Expense, typeof(CROpportunityProducts.expenseAccountGroupID), true, SkipVerification = true, AllowNullValue = true, Filterable = false)]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.costCodeID> e)
		{
		}

		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.inventoryID> e)
		{
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts.customerID> e)
		{
			var quote = Quote.Current;

			if (quote != null)
			{
				e.NewValue = quote.BAccountID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts.projectID> e)
		{
			var quote = Quote.Current;

			if (quote != null)
			{
				e.NewValue = quote.QuoteProjectID;
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Template ID", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<PMProject.contractCD> e) { }
		#endregion

		#region CROpportunityDiscountDetail    


		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(PMQuote.quoteID))]
		protected virtual void _(Events.CacheAttached<CROpportunityDiscountDetail.quoteID> e)
		{
		}

		[PXDBUShort()]
		[PXLineNbr(typeof(PMQuote))]
		protected virtual void _(Events.CacheAttached<CROpportunityDiscountDetail.lineNbr> e)
		{
		}

		#endregion

		#region CROpportunityTax
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(PMQuote.quoteID))]
		[PXParent(typeof(Select<PMQuote,
			Where<PMQuote.quoteID, Equal<Current<CR.CROpportunityTax.quoteID>>>>))]
		protected virtual void _(Events.CacheAttached<CROpportunityTax.quoteID> e)
		{
		}
		[PXDBLong]
		[CurrencyInfo(typeof(PMQuote.curyInfoID))]
		protected virtual void _(Events.CacheAttached<CROpportunityTax.curyInfoID> e)
		{
		}


		#endregion

		#region CRTaxTran
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(PMQuote.quoteID))]
		[PXParent(typeof(Select<PMQuote,
			Where<PMQuote.quoteID, Equal<Current<CRTaxTran.quoteID>>>>))]
		protected virtual void _(Events.CacheAttached<CRTaxTran.quoteID> e)
		{
		}

		[PXDBLong]
		[CurrencyInfo(typeof(PMQuote.curyInfoID))]
		protected virtual void _(Events.CacheAttached<CRTaxTran.curyInfoID> e)
		{
		}

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

		#region CRAddress

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void _(Events.CacheAttached<CRAddress.countryID> e)
		{

		}

		#endregion

		#region CRShippingAddress

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void _(Events.CacheAttached<CRShippingAddress.countryID> e)
		{

		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		public virtual void _(Events.CacheAttached<CRShippingAddress.latitude> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		public virtual void _(Events.CacheAttached<CRShippingAddress.longitude> e) { }

		#endregion

		#region EPApproval Cahce Attached
		[PXDefault(typeof(PMQuote.documentDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<EPApproval.docDate> e) { }

		[PXDefault(typeof(PMQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<EPApproval.bAccountID> e) { }

		[PXDefault(typeof(Search<Contact.contactID,
				Where<Contact.contactID, Equal<Current<AccessInfo.contactID>>,
					And<Current<PMQuote.workgroupID>, IsNull>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<EPApproval.documentOwnerID> e) { }

		[PXDefault(typeof(PMQuote.subject), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<EPApproval.descr> e) { }

		[CurrencyInfo(typeof(PMQuote.curyInfoID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<EPApproval.curyInfoID> e) { }

		[PXDefault(typeof(PMQuote.curyProductsAmount), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<EPApproval.curyTotalAmount> e) { }

		[PXDefault(typeof(PMQuote.productsAmount), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<EPApproval.totalAmount> e) { }
		#endregion
		#endregion

		#region CopyQuoteFilter
		[PXHidden]
		[Serializable]
		public partial class CopyQuoteFilter : IBqlTable
		{
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
			[PXUIField(DisplayName = "Override Manual Discounts", Enabled = false)]
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

		#region ConvertToProjectSettings
		[PXHidden]
		[Serializable]
		public partial class ConvertToProjectFilter : IBqlTable
		{
			#region createLaborRates
			public abstract class createLaborRates : PX.Data.BQL.BqlBool.Field<createLaborRates> { }
			[PXBool()]
			[PXUIField(DisplayName = "Populate Labor Cost Rates")]
			public virtual bool? CreateLaborRates { get; set; }
			#endregion

			#region activateProject
			public abstract class activateProject : PX.Data.BQL.BqlBool.Field<activateProject> { }
			[PXBool()]
			[PXUIField(DisplayName = "Activate Project")]
			public virtual bool? ActivateProject { get; set; }
			#endregion

			#region activateTasks
			public abstract class activateTasks : PX.Data.BQL.BqlBool.Field<activateTasks> { }
			[PXBool()]
			[PXUIField(DisplayName = "Activate Tasks")]
			public virtual bool? ActivateTasks { get; set; }
			#endregion

			#region copyNotes
			public abstract class copyNotes : PX.Data.BQL.BqlBool.Field<copyNotes> { }
			[PXBool()]
			[PXUIField(DisplayName = "Copy Notes to Project")]
			public virtual bool? CopyNotes { get; set; }
			#endregion

			#region copyFiles
			public abstract class copyFiles : PX.Data.BQL.BqlBool.Field<copyFiles> { }
			[PXBool()]
			[PXUIField(DisplayName = "Copy Files to Project")]
			public virtual bool? CopyFiles { get; set; }
			#endregion

			#region moveActivities
			public abstract class moveActivities : PX.Data.BQL.BqlBool.Field<moveActivities> { }
			[PXBool()]
			[PXUIField(DisplayName = "Link Activities to Project")]
			public virtual bool? MoveActivities { get; set; }
			#endregion

			#region TaskCD
			public abstract class taskCD : PX.Data.BQL.BqlString.Field<taskCD> { }

			[PXDBString()]
			[PXUIField(DisplayName = "Project Task")]
			[PXDimension(ProjectTaskAttribute.DimensionName)]
			[PXSelector(typeof(Search<PMQuoteTask.taskCD, Where<PMQuoteTask.quoteID, Equal<Current<PMQuote.quoteID>>>>))]
			public virtual string TaskCD
			{
				get;
				set;
			}
			#endregion
		}
		#endregion

		#region Selects / Views
		
		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSetupOptional<SOSetup>	sosetup;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSetup<PMSetup>	Setup;

		[PXViewName(CR.Messages.PMQuote)]
		[PXCopyPasteHiddenFields(
			typeof(PMQuote.quoteNbr),
			typeof(PMQuote.isPrimary),
			typeof(PMQuote.status),
			typeof(PMQuote.documentDate),
			typeof(PMQuote.expirationDate),
			typeof(PMQuote.quoteProjectCD),
			typeof(PMQuote.curyAmount),
			typeof(PMQuote.curyCostTotal),
			typeof(PMQuote.curyGrossMarginAmount),
			typeof(PMQuote.grossMarginPct), 
			typeof(PMQuote.curyTaxTotal),
			typeof(PMQuote.curyQuoteTotal),
			typeof(PMQuote.approved),
			typeof(PMQuote.rejected))]
		public SelectFrom<PMQuote>
			.LeftJoin<PMProject>.On<PMProject.contractID.IsEqual<PMQuote.quoteProjectID>>
			.Where<PMQuote.quoteProjectID.IsNull.Or<MatchUserFor<PMProject>>>
			.View Quote;

		[PXHidden]
		[PXCopyPasteHiddenFields(typeof(PMQuote.createdByID), typeof(PMQuote.approved), typeof(PMQuote.rejected))]
		public PXSelect<PMQuote,
				Where<PMQuote.quoteID, Equal<Current<PMQuote.quoteID>>>> QuoteCurrent;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<Address> Address;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSetup<Contact>.Where<Contact.contactID.IsEqual<PMQuote.contactID.AsOptional>> Contacts;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSetup<Customer>.Where<Customer.bAccountID.IsEqual<PMQuote.bAccountID.AsOptional>> customer;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSetup<BAccount>.Where<BAccount.bAccountID.IsEqual<PMQuote.bAccountID.AsOptional>> baccount;

		[PXViewName(Messages.PMTasks)]
		[PXImport(typeof(PMQuote))]
		[PXCopyPasteHiddenFields(typeof(PMQuoteTask.plannedStartDate), typeof(PMQuoteTask.plannedEndDate))]
		public PXSelect<PMQuoteTask, Where<PMQuoteTask.quoteID, Equal<Current<PMQuote.quoteID>>>> Tasks;

		[PXViewName(Messages.Estimates)]
		[PXImport(typeof(PMQuote))]
		public ProductLinesSelect Products;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<CROpportunityRevision> FakeRevisionCache;

		[PXCopyPasteHiddenView]
		public PXSelect<CR.CROpportunityTax,
			Where<CR.CROpportunityTax.quoteID, Equal<Current<PMQuote.quoteID>>,
				And<CR.CROpportunityTax.lineNbr, Less<intMax>>>,
			OrderBy<Asc<CR.CROpportunityTax.taxID>>> TaxLines;

		[PXViewName(CR.Messages.QuoteTax)]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<CRTaxTran,
			InnerJoin<Tax, On<Tax.taxID, Equal<CRTaxTran.taxID>>>,
			Where<CRTaxTran.quoteID, Equal<Current<PMQuote.quoteID>>>,
			OrderBy<Asc<CRTaxTran.lineNbr, Asc<CRTaxTran.taxID>>>> Taxes;

		[PXCopyPasteHiddenView]
		public PXSetup<CR.Location>.
			Where<CR.Location.bAccountID.IsEqual<PMQuote.bAccountID.FromCurrent>.
				And<CR.Location.locationID.IsEqual<PMQuote.locationID.FromCurrent>>> location;


		[PXViewName(CR.Messages.QuoteContact)]
		public PXSelect<CRContact, Where<CRContact.contactID, Equal<Current<PMQuote.opportunityContactID>>>> Quote_Contact;

		[PXViewName(CR.Messages.QuoteAddress)]
		public PXSelect<CRAddress, Where<CRAddress.addressID, Equal<Current<PMQuote.opportunityAddressID>>>> Quote_Address;

		[PXViewName(CR.Messages.ShippingContact)]
		public PXSelect<CRShippingContact, Where<CRShippingContact.contactID, Equal<Current<PMQuote.shipContactID>>>> Shipping_Contact;

		[PXViewName(CR.Messages.ShippingAddress)]
		public PXSelect<CRShippingAddress, Where<CRShippingAddress.addressID, Equal<Current<PMQuote.shipAddressID>>>> Shipping_Address;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<Contact,
			LeftJoin<Address, On<Contact.defAddressID, Equal<Address.addressID>>>,
			Where<Contact.contactID, Equal<Current<PMQuote.contactID>>>> CurrentContact;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<CR.Standalone.CROpportunity,
			LeftJoin<CROpportunityRevision,
				On<CROpportunityRevision.noteID, Equal<CR.Standalone.CROpportunity.defQuoteID>>,
			LeftJoin<CR.Standalone.CRQuote,
				On<CR.Standalone.CRQuote.quoteID, Equal<CROpportunityRevision.noteID>>>>,
			Where<CR.Standalone.CROpportunity.opportunityID, Equal<Optional<PMQuote.opportunityID>>>> Opportunity;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelectReadonly<PMQuote, Where<PMQuote.quoteID, Equal<Required<PMQuote.quoteID>>>> QuoteInDb;

		[PXHidden]
		public PXSelect<AP.Vendor> Vendors;

		[PXViewName(Messages.Approval)]
		public PMQuoteApproval Approval;

		[PXViewName(CR.Messages.CopyQuote)]
		[PXCopyPasteHiddenView]
		public PXFilter<CopyQuoteFilter> CopyQuoteInfo;

		[PXViewName(Messages.QuoteAnswers)]
		public CRAttributeList<PMQuote> Answers;

		[PXViewName(Messages.ConvertToProject)]
		[PXCopyPasteHiddenView]
		public PXFilter<ConvertToProjectFilter> ConvertQuoteInfo;

		[PXCopyPasteHiddenView]
		public PXSelect<CR.CROpportunityDiscountDetail,
				Where<CR.CROpportunityDiscountDetail.quoteID, Equal<Current<PMQuote.quoteID>>>,
				OrderBy<Asc<CR.CROpportunityDiscountDetail.lineNbr>>> _DiscountDetails;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<NotificationSource,
			InnerJoin<PMProject, On<NotificationSource.refNoteID, Equal<PMProject.noteID>>>,
			Where<PMProject.contractID, Equal<Current<PMQuote.templateID>>>> NotificationSources;

		[PXCopyPasteHiddenView]
		public PXSetup<EPSetup> epsetup;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<PMProject> DummyProject;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Current<PMQuote.noteID>>>> QuoteNotes;

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
							throw new PXDbOperationSwitchRequiredException(table.Name, Messages.NeedUpdate);
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

		[PXCopyPasteHiddenView]
		[PXFilterable]
		public SelectFrom<SelectedTask>
				.LeftJoin<PMProject>.On<SelectedTask.projectID.IsEqual<PMProject.contractID>>
				.Where<SelectedTask.autoIncludeInPrj.IsNotEqual<True>
				.And<SelectedTask.projectID.IsEqual<PMQuote.templateID.FromCurrent>>
				.Or<PMProject.nonProject.IsEqual<True>>>
				.View TasksForAddition;

		protected IEnumerable tasksForAddition()
		{
			HashSet<string> existingTasksCDs = Tasks.Select().RowCast<PMQuoteTask>()
				.Where(task => !string.IsNullOrWhiteSpace(task.TaskCD))
				.Select(task => task.TaskCD.Trim().ToUpper())
				.Distinct()
				.ToHashSet();

			var availableTasks = SelectFrom<SelectedTask>
				.LeftJoin<PMProject>.On<SelectedTask.projectID.IsEqual<PMProject.contractID>>
				.Where<SelectedTask.autoIncludeInPrj.IsNotEqual<True>
				.And<SelectedTask.projectID.IsEqual<PMQuote.templateID.FromCurrent>>
				.Or<PMProject.nonProject.IsEqual<True>>>
				.View.Select(this);

			var notExistingTasks = availableTasks.RowCast<SelectedTask>()
				.Where(task => !existingTasksCDs.Contains(task.TaskCD.Trim().ToUpper()));

			return notExistingTasks;
		}
		#endregion

		#region Ctors
		public PMQuoteMaint()
		{
			if (string.IsNullOrEmpty(Setup.Current.QuoteNumberingID))
			{
				throw new PXSetPropertyException(Messages.QuoteNumberingIDIsNull, Messages.PMSetup);
			}

			var bAccountCache = Caches[typeof(BAccount)];
			var PMQuoteCache = Caches[typeof(PMQuote)];

			PMQuote quotecurrent = PMQuoteCache.Current as PMQuote;

			actionsFolder.MenuAutoOpen = true;
			actionsFolder.AddMenuAction(copyQuote);
			actionsFolder.AddMenuAction(sendQuote);
			actionsFolder.AddMenuAction(validateAddresses);

			if (quotecurrent != null)
			{
				PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, CR.Messages.BAccountCD);
				PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, CR.Messages.BAccountName);

				PXUIFieldAttribute.SetEnabled<PMQuote.quoteProjectID>(PMQuoteCache, null, false);

				PXDefaultAttribute.SetPersistingCheck<PMQuote.branchID>(PMQuoteCache, null, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<PMQuote.quoteProjectID>(PMQuoteCache, null, PXPersistingCheck.Nothing);

				PXUIFieldAttribute.SetEnabled<PMQuote.locationID>(PMQuoteCache, quotecurrent, quotecurrent.BAccountID != null);
				PXDefaultAttribute.SetPersistingCheck<PMQuote.locationID>(PMQuoteCache, quotecurrent, quotecurrent.BAccountID == null ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			}
			this.Views.Caches.Remove(typeof(CR.Standalone.CROpportunity));
			this.Views.Caches.Remove(typeof(CR.CROpportunity));

			PXUIFieldAttribute.SetVisible<PMQuote.opportunityID>(PMQuoteCache, null, PXAccess.FeatureInstalled<FeaturesSet.customerModule>());
			PXUIFieldAttribute.SetVisible<PMQuote.isPrimary>(PMQuoteCache, null, PXAccess.FeatureInstalled<FeaturesSet.customerModule>());
			Actions[nameof(PrimaryQuote)].SetVisible(PXAccess.FeatureInstalled<FeaturesSet.customerModule>());
		}

		#endregion

		#region Actions

		public PXSave<PMQuote> Save;
		public PXAction<PMQuote> cancel;
		public PXInsert<PMQuote> insert;
		public PXDelete<PMQuote> Delete;
		public PXCopyPasteAction<PMQuote> CopyPaste;
		public PXFirst<PMQuote> First;
		public PXPrevious<PMQuote> previous;
		public PXNext<PMQuote> next;
		public PXLast<PMQuote> Last;

		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			string oppID = Quote.Current != null ? Quote.Current.OpportunityID : null;
			Quote.Cache.Clear();
			foreach (PXResult<PMQuote, PMProject> result in new PXCancel<PMQuote>(this, "Cancel").Press(adapter))
			{
				PMQuote quote = result;
				return new object[] { quote };
			}
			return new object[0];
		}

		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		protected virtual IEnumerable Previous(PXAdapter adapter)
		{
			foreach (PXResult<PMQuote, PMProject> result in (new PXPrevious<PMQuote>(this, "Prev")).Press(adapter))
			{
				PMQuote loc = result;
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
			foreach (PXResult<PMQuote, PMProject> result in (new PXNext<PMQuote>(this, "Next")).Press(adapter))
			{
				PMQuote loc = result;
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

		public PXAction<PMQuote> convertToProject;
		[PXUIField(DisplayName = "Convert to Project", MapEnableRights = PXCacheRights.Select)]
		[PXButton(Category = Messages.Processing, DisplayOnMainToolbar = true, IsLockedOnToolbar = true)]
		public IEnumerable ConvertToProject(PXAdapter adapter)
		{
			var list = new List<PMQuote>(adapter.Get()
				.Cast<PXResult<PMQuote, PMProject>>()
				.Select(qp => { PMQuote q = qp; return q; }));

			Save.Press();

			if (ConvertQuoteInfo.View.Answer == WebDialogResult.None)
			{
				ConvertQuoteInfo.Cache.Clear();
				var settings = ConvertQuoteInfo.Cache.Insert() as ConvertToProjectFilter;
				settings.CopyNotes = false;
				settings.CopyFiles = false;
				settings.MoveActivities = false;
				settings.TaskCD = GetDefaultTaskCD();
			}

			bool failed = false;
			int errorCount = 0;
			foreach (PMQuote quote in list)
			{
				if (!ValidateQuoteBeforeConvertToProject(quote, out errorCount))
					failed = true;
			}

			if (failed)
				throw new PXException(PM.Messages.ConvertToProjectRaisedError, errorCount, Products.Select().Count);

			if (ConvertQuoteInfo.AskExt() != WebDialogResult.Yes)
				return list;
						
			foreach (PMQuote quote in list)
			{
				PXLongOperation.StartOperation(this, delegate () {

					PMQuoteMaint graph = PXGraph.CreateInstance<PMQuoteMaint>();
					graph.SelectTimeStamp();
					graph.Quote.Current = Quote.Current;
					graph.ConvertQuoteToProject(Quote.Current, ConvertQuoteInfo.Current);
					
					ProjectEntry target = PXGraph.CreateInstance<ProjectEntry>();
                    target.Clear(PXClearOption.ClearAll);
                    target.SelectTimeStamp();
                    target.Project.Current = PMProject.PK.Find(target, Quote.Current.QuoteProjectID);
                    throw new PXRedirectRequiredException(target, true, "ViewProject") { Mode = PXBaseRedirectException.WindowMode.Same };
                });
			}
			
			return list;
		}

		public PXAction<PMQuote> actionsFolder;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable ActionsFolder(PXAdapter adapter)
		{
			return adapter.Get();
		}


		public PXAction<PMQuote> copyQuote;
		[PXUIField(DisplayName = "Copy", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(Category = Messages.Other)]
		public virtual IEnumerable CopyQuote(PXAdapter adapter)
		{
			List<PMQuote> CRQoutes = new List<PMQuote>(adapter.Get()
				.Cast<PXResult<PMQuote, PMProject>>()
				.Select(r => { PMQuote q = r; return q; }));
			foreach (PMQuote quote in CRQoutes)
			{
				if (CopyQuoteInfo.View.Answer == WebDialogResult.None)
				{
					CopyQuoteInfo.Cache.Clear();
					CopyQuoteFilter filterdata = CopyQuoteInfo.Cache.Insert() as CopyQuoteFilter;
					filterdata.Description = quote.Subject + CR.Messages.QuoteCopy;
					filterdata.RecalculatePrices = false;
					filterdata.RecalculateDiscounts = false;
					filterdata.OverrideManualPrices = false;
					filterdata.OverrideManualDiscounts = false;
					filterdata.OverrideManualDocGroupDiscounts = false;
				}

				if (CopyQuoteInfo.AskExt() != WebDialogResult.Yes)
					return CRQoutes;

				Save.Press();

				PXLongOperation.StartOperation(this, () => CopyToQuote(quote, CopyQuoteInfo.Current));
			}
			return CRQoutes;
		}

		
		/// <summary>
		/// Returns true both for source as well as target graph during copy-paste procedure. 
		/// </summary>
		public bool IsCopyPaste
		{
			get;
			private set;
		}

		/// <summary>
		/// During Paste this propert holds the reference to the Graph with source data.
		/// </summary>
		public PMQuoteMaint CopySource
		{
			get;
			private set;
		}

		public void CopyToQuote(PMQuote currentquote, CopyQuoteFilter param)
		{
			this.Quote.Current = currentquote;

			var graph = PXGraph.CreateInstance<PMQuoteMaint>();
			graph.IsCopyPaste = true;
			graph.CopySource = this;
			graph.SelectTimeStamp();
			var quote = (PMQuote)graph.Quote.Cache.CreateInstance();
			quote = graph.Quote.Insert(quote);
			CurrencyInfo info =
				PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PMQuote.curyInfoID>>>>
					.Select(this);
			info.CuryInfoID = null;
			info = (CurrencyInfo)graph.Caches<CurrencyInfo>().Insert(info);

			foreach (string field in Quote.Cache.Fields)
			{
				if (graph.Quote.Cache.Keys.Contains(field)
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.isPrimary))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.quoteID))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.status))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.expirationDate))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.approved))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.rejected))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.quoteProjectID))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.quoteProjectCD))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.opportunityContactID))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.opportunityAddressID))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.shipContactID))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.shipAddressID))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.allowOverrideContactAddress))
					|| field == graph.Quote.Cache.GetField(typeof(PMQuote.allowOverrideShippingContactAddress))
					)
					continue;

				graph.Quote.Cache.SetValue(quote, field,
					Quote.Cache.GetValue(currentquote, field));
			}
			UDFHelper.CopyAttributes(Quote.Cache, currentquote, graph.Quote.Cache, quote, quote.ClassID);

			quote.CuryInfoID = info.CuryInfoID;
			quote.Subject = param.Description;
			quote.DocumentDate = this.Accessinfo.BusinessDate;

			string note = PXNoteAttribute.GetNote(Quote.Cache, currentquote);
			Guid[] files = PXNoteAttribute.GetFileNotes(Quote.Cache, currentquote);

			object quoteID;
			graph.Quote.Cache.RaiseFieldDefaulting<PMQuote.noteID>(quote, out quoteID);
			quote.QuoteID = quote.NoteID = (Guid?)quoteID;

			PXNoteAttribute.SetNote(graph.Quote.Cache, quote, note);
			PXNoteAttribute.SetFileNotes(graph.Quote.Cache, quote, files);

			Tasks.View.CloneView(graph, quote.QuoteID, info);
			Products.View.CloneView(graph, quote.QuoteID, info);
			foreach (CROpportunityProducts item in graph.Products.Select())
			{
				if (currentquote.QuoteProjectID != null)
				{
					item.ProjectID = null;
					item.TaskID = null;
				}

				if (param.OverrideManualPrices != true)
				{
					item.ManualPrice = true;
				}
			}

			var DiscountExt = this.GetExtension<PMDiscount>();
			Views[nameof(DiscountExt.DiscountDetails)].CloneView(graph, quote.QuoteID, info);
			TaxLines.View.CloneView(graph, quote.QuoteID, info);
			Taxes.View.CloneView(graph, quote.QuoteID, info, nameof(CRTaxTran.RecordID));

			graph.Answers.CopyAllAttributes(quote, Quote.Cache.Current);
			foreach (CSAnswers answer in graph.Answers.Cache.Inserted)
			{
				if (answer.RefNoteID != quote.QuoteID)
					graph.Answers.Delete(answer);
			}

			PMQuote copy = (PMQuote) graph.Quote.Cache.CreateCopy(quote);
			copy.AllowOverrideContactAddress = currentquote.AllowOverrideContactAddress;
			copy.AllowOverrideShippingContactAddress = currentquote.AllowOverrideShippingContactAddress;
			copy.OpportunityContactID = currentquote.OpportunityContactID;
			copy.OpportunityAddressID = currentquote.OpportunityAddressID;
			copy.ShipContactID = currentquote.ShipContactID;
			copy.ShipAddressID = currentquote.ShipAddressID;

			graph.Quote.Update(copy);

			var Discount = graph.GetExtension<PMQuoteMaint.PMDiscount>();
			Discount.recalcdiscountsfilter.Current.OverrideManualDiscounts = param.OverrideManualDiscounts == true;
			Discount.recalcdiscountsfilter.Current.OverrideManualDocGroupDiscounts = param.OverrideManualDocGroupDiscounts == true;
			Discount.recalcdiscountsfilter.Current.OverrideManualPrices = param.OverrideManualPrices == true;
			Discount.recalcdiscountsfilter.Current.RecalcDiscounts = param.RecalculateDiscounts == true;
			Discount.recalcdiscountsfilter.Current.RecalcUnitPrices = param.RecalculatePrices == true;
			graph.Actions[nameof(Discount.RecalculateDiscountsAction)].Press();

			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Same);
		}

		protected virtual string DefaultReportID => "PM604500";

		protected virtual string DefaultNotificationCD => "PMQUOTE";

		public PXAction<PMQuote> sendQuote;
		[PXUIField(DisplayName = "Email")]
		[PXButton(Category = Messages.PrintingAndEmailing)]
		public IEnumerable SendQuote(PXAdapter adapter)
		{
			foreach (PXResult<PMQuote, PMProject> res in adapter.Get())
			{
				PMQuote item = res;
				if (item.TemplateID == null)
					Quote.Cache.RaiseExceptionHandling<PMQuote.templateID>(item, item.TemplateID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMQuote.templateID>(Quote.Cache)));
				
				var parameters = new Dictionary<string, string>();
				parameters[nameof(PMQuote) + "." + nameof(PMQuote.QuoteNbr)] = item.QuoteNbr;
				this.GetExtension<PMQuoteMaint_ActivityDetailsExt>().SendNotification(PMNotificationSource.Project, DefaultNotificationCD, item.BranchID, parameters, adapter.MassProcess);
				item.Status = PMQuoteStatusAttribute.Sent;
				Quote.Update(item);
				Save.Press();
				yield return item;
			}
		}


		public PXAction<PMQuote> printQuote;
		[PXUIField(DisplayName = "Print")]
		[PXButton(Category = Messages.PrintingAndEmailing, DisplayOnMainToolbar = true)]
		public IEnumerable PrintQuote(PXAdapter adapter)
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			string actualReportID = DefaultReportID;

			foreach (PXResult<PMQuote, PMProject> res in adapter.Get())
			{
				PMQuote item = res;
				Save.Press();

				parameters[nameof(PMQuote.QuoteNbr)] = item.QuoteNbr;

				throw new PXReportRequiredException(parameters, actualReportID, "Report " + actualReportID);
			}
			return adapter.Get();
		}

		public PXAction<PMQuote> primaryQuote;
		[PXUIField(DisplayName = CR.Messages.MarkAsPrimary)]
		[PXButton(Category = Messages.Other)]
		public virtual IEnumerable PrimaryQuote(PXAdapter adapter)
		{
			foreach (PXResult<PMQuote, PMProject> res in adapter.Get())
			{
				PMQuote item = res;
				Opportunity.Cache.Clear();
				var rec = (PXResult<CR.Standalone.CROpportunity>) Opportunity.View.SelectSingleBound(new object[] { item });
                CR.Standalone.CROpportunity opp = rec[typeof(CR.Standalone.CROpportunity)] as CR.Standalone.CROpportunity;
                if (opp != null && opp.DefQuoteID != item.QuoteID)
                {
                    PMQuote primary = PXSelect<PMQuote, Where<PMQuote.quoteID, Equal<Required<PMQuote.quoteID>>>>.Select(this, opp.DefQuoteID);
                    if (primary != null && primary.QuoteProjectID != null)
                    {
                        throw new PXException(PM.Messages.QuoteIsClosed, opp.OpportunityID, primary.QuoteNbr);
                    }
                }

				this.Opportunity.Current = rec;
				this.Opportunity.Current.DefQuoteID = item.QuoteID;
				item.DefQuoteID = item.QuoteID;
				item.IsPrimary = true;
				CR.Standalone.CROpportunity opudate = Opportunity.Cache.Update(this.Opportunity.Current) as CR.Standalone.CROpportunity;
				this.Views.Caches.Add(typeof(CR.Standalone.CROpportunity));
				PMQuote upitem = QuoteCurrent.Cache.Update(item) as PMQuote;
				this.Persist();
				yield return upitem;
			}
		}

		public PXAction<PMQuote> validateAddresses;
		[PXUIField(DisplayName = CR.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select/*, FieldClass = CS.Messages.ValidateAddress*/)]
		[PXButton(Category = Messages.Other)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (PXResult<PMQuote, PMProject> res in adapter.Get())
			{
				PMQuote current = res;
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

			// Lift "Tasks" group over "Products" group.
			script.Sort((first, second) =>
			{
				if (first.ObjectName == nameof(Products) && second.ObjectName == nameof(Tasks))
				{
					return 1;
				}
				return first != second ? -1 : 0;
			});

			containers.Sort((first, second) =>
			{
				if (first.ViewName() == nameof(Products) && second.ViewName() == nameof(Tasks))
				{
					return 1;
				}
				return first != second ? -1 : 0;
			});
		}

		public PXAction<PMQuote> viewMainOnMap;

		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void ViewMainOnMap()
		{
			var address = Quote_Address.SelectSingle();
			if (address != null)
			{
				BAccountUtility.ViewOnMap(address);
			}
		}

		public PXAction<PMQuote> viewShippingOnMap;

		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void ViewShippingOnMap()
		{
			var address = Shipping_Address.SelectSingle();
			if (address != null)
			{
				BAccountUtility.ViewOnMap(address);
			}
		}

		public PXAction<PMQuote> addCommonTasks;
		[PXUIField(DisplayName = Messages.AddCommonTasks, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton]
		public virtual IEnumerable AddCommonTasks(PXAdapter adapter)
		{
			if (TasksForAddition.AskExt() == WebDialogResult.OK)
			{
				AddSelectedCommonTasks();
			}

			return adapter.Get();
		}
		#endregion

		#region Event Handlers

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

		#region RecalcDiscountsParamFilter
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
			}
		}

		protected virtual void RecalcDiscountsParamFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RecalcDiscountsParamFilter row = e.Row as RecalcDiscountsParamFilter;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<RecalcDiscountsParamFilter.overrideManualPrices>(sender, row, row.RecalcUnitPrices == true);
			PXUIFieldAttribute.SetEnabled<RecalcDiscountsParamFilter.overrideManualDiscounts>(sender, row, row.RecalcDiscounts == true);
		}
		#endregion
		
		#region PMQuote        

		protected virtual void PMQuote_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as PMQuote;
			if (row == null) return;

			var customerLocation = (CR.Location)PXSelect<CR.Location,
					Where<CR.Location.bAccountID, Equal<Required<PMQuote.bAccountID>>,
						And<CR.Location.locationID, Equal<Required<PMQuote.locationID>>>>>.
				Select(this, row.BAccountID, row.LocationID);
			if (customerLocation != null)
			{
				if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
				{
					e.NewValue = customerLocation.CTaxZoneID;
				}
				else
				{
					var address = (Address)PXSelect<Address,
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(this, customerLocation.DefAddressID);
					if (address != null )
					{
						e.NewValue = TaxBuilderEngine.GetTaxZoneByAddress(this, address);
					}
				}
			}
			if (e.NewValue == null)
			{
				var branchLocation = (CR.Location)PXSelectJoin<CR.Location,
					InnerJoin<Branch, On<Branch.branchID, Equal<Current<PMQuote.branchID>>>,
						InnerJoin<BAccount, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>>,
					Where<CR.Location.locationID, Equal<BAccount.defLocationID>>>.Select(this);
				if (branchLocation != null && branchLocation.VTaxZoneID != null)
					e.NewValue = branchLocation.VTaxZoneID;
				else
					e.NewValue = row.TaxZoneID;
			}
			if (sender.GetStatus(e.Row) != PXEntryStatus.Notchanged)
				e.Cancel = true;
		}

		protected virtual void PMQuote_BAccountID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PMQuote row = e.Row as PMQuote;
			if (row == null) return;

			if (row.BAccountID < 0)
				e.ReturnValue = "";
		}

		protected virtual void _(Events.FieldUpdated<PMQuote, PMQuote.bAccountID> e)
		{
			e.Cache.SetDefaultExt<PMQuote.taxCalcMode>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMQuote, PMQuote.locationID> e)
		{
			e.Cache.SetDefaultExt<PMQuote.taxCalcMode>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMQuote, PMQuote.taxCalcMode> e)
		{
			if (e.Row != null && !string.IsNullOrWhiteSpace(e.Row.OpportunityID))
			{
				CROpportunityRevision opportunity = SelectFrom<CROpportunityRevision>
					.Where<CROpportunityRevision.opportunityID.IsEqual<PMQuote.opportunityID.FromCurrent>>
					.View.SelectSingleBound(this, new[] { e.Row });

				if (opportunity != null)
				{
					e.NewValue = opportunity.TaxCalcMode;
					e.Cancel = true;
				}
			}
		}

		protected virtual void PMQuote_OpportunityID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as PMQuote;
			if (row == null) return;

			if (row.OpportunityID == null)
			{
				row.IsPrimary = false;
			}
			else
			{
				row.IsPrimary = IsFirstQuote(row.OpportunityID);

				CR.CROpportunity opp = PXSelect<CR.CROpportunity,
					Where<CR.CROpportunity.opportunityID, Equal<Required<CR.CROpportunity.opportunityID>>>>
					.SelectSingleBound(this, null, row.OpportunityID);

				if (row.IsPrimary == true)
                {
                    Quote.Cache.RaiseExceptionHandling<PMQuote.opportunityID>(row, row.OpportunityID,
                    new PXSetPropertyException(CR.Messages.FirstQuoteIsProject, PXErrorLevel.Warning));
                }
                else
                {
                    row.BranchID = opp.BranchID;
                }

				row.BAccountID = opp.BAccountID;

				object newContactId = row.ContactID;
				if (newContactId != null && !VerifyField<PMQuote.contactID>(row, newContactId))
					row.ContactID = null;

				if (row.ContactID != null)
				{
					object newCustomerId = row.BAccountID;
					if (newCustomerId == null)
						FillDefaultBAccountID(row);
				}

				object newLocationId = row.LocationID;
				if (newLocationId == null || !VerifyField<PMQuote.locationID>(row, newLocationId))
				{
					sender.SetDefaultExt<PMQuote.locationID>(row);
				}

				if (row.ContactID == null)
					sender.SetDefaultExt<PMQuote.contactID>(row);

				if (row.TaxZoneID == null)
					sender.SetDefaultExt<PMQuote.taxZoneID>(row);
			}
		}


		protected virtual void _(Events.FieldSelecting<PMQuote, PMQuote.quoteProjectCD> e)
		{
			if (DimensionMaint.IsAutonumbered(this, ProjectAttribute.DimensionName))
			{
				e.ReturnValue = PXMessages.LocalizeNoPrefix(Messages.NewKey);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMQuote, PMQuote.quoteProjectCD> e)
		{
			if (!ProjectAttribute.IsAutonumbered(this, ProjectAttribute.DimensionName) && !string.IsNullOrEmpty((string)e.NewValue))
			{
				PMProject duplicate = PXSelect<PMProject, Where<PMProject.contractCD, Equal<Required<PMProject.contractCD>>>>.Select(this, e.NewValue);
				if (duplicate != null)
				{
					throw new PXSetPropertyException(Messages.DuplicateProjectCD);
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMQuote, PMQuote.locationID> e)
		{
			if (e.Row == null || e.Row.BAccountID == null) return;

			var baccount = (BAccount)PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(this, e.Row.BAccountID);

			if (baccount != null)
			{
				e.NewValue = baccount.DefLocationID;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMQuote, PMQuote.templateID> e)
		{
			if (IsCopyPaste || IsCopyPasteContext)
				return;

			if (Products.Select().Count == 0)
			{
				DeleteNoteAndFilesOfTemplate(e.Row, (int?)e.OldValue);
				RedefaultTasksFromTemplate(e.Row);
				DefaultFromTemplate(e.Row);
			}
			else
			{
				WebDialogResult result = Quote.Ask(Messages.UpdateQuoteByTemplateDialogHeader, Messages.TemplateChangedDialogQuestion, MessageButtons.YesNo, MessageIcon.Question);
				if (result == WebDialogResult.Yes)
				{
					DeleteNoteAndFilesOfTemplate(e.Row, (int?)e.OldValue);
					RedefaultTasksFromTemplate(e.Row);
					DefaultFromTemplate(e.Row);
				}
			}
		}

		protected virtual void PMQuote_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PXNoteAttribute.SetTextFilesActivitiesRequired<CR.CROpportunityProducts.noteID>(Products.Cache, null);

			PMQuote row = e.Row as PMQuote;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<PMQuote.allowOverrideContactAddress>(cache, row, !(row.BAccountID == null && row.ContactID == null));
			Caches[typeof(CRContact)].AllowUpdate = row.AllowOverrideContactAddress == true;
			Caches[typeof(CRAddress)].AllowUpdate = row.AllowOverrideContactAddress == true;

			PXUIFieldAttribute.SetEnabled<PMQuote.curyAmount>(cache, row, row.ManualTotalEntry == true);
			PXUIFieldAttribute.SetEnabled<PMQuote.curyDiscTot>(cache, row, row.ManualTotalEntry == true);

			PXUIFieldAttribute.SetEnabled<PMQuote.locationID>(cache, row, row.BAccountID != null);
			PXDefaultAttribute.SetPersistingCheck<PMQuote.locationID>(cache, row, row.BAccountID == null ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

			PXUIFieldAttribute.SetEnabled<PMQuote.branchID>(cache, row, row.OpportunityID == null);
			PXUIFieldAttribute.SetEnabled<PMQuote.quoteProjectID>(cache, row, false);

			PXUIFieldAttribute.SetRequired<PMQuote.branchID>(cache, true);
			PXDefaultAttribute.SetPersistingCheck<PMQuote.quoteProjectID>(cache, row, PXPersistingCheck.Nothing);

			PXUIFieldAttribute.SetVisible<PMQuote.curyID>(cache, row, IsMultyCurrency);
			PXUIFieldAttribute.SetEnabled<PMQuote.curyID>(cache, row, PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>());
			if (row.ManualTotalEntry == true && row.CuryTaxTotal > 0)
			{
				cache.RaiseExceptionHandling<PMQuote.curyTaxTotal>(row, row.CuryTaxTotal,
					new PXSetPropertyException(CR.Messages.TaxAmountExcluded, PXErrorLevel.Warning));
			}
			else
			{
				cache.RaiseExceptionHandling<PMQuote.curyTaxTotal>(row, row.CuryTaxTotal, null);
			}

			bool isConverted = row.QuoteProjectID != null;
			PXUIFieldAttribute.SetVisible<PMQuote.quoteProjectID>(cache, row, isConverted);
			PXUIFieldAttribute.SetVisible<PMQuote.quoteProjectCD>(cache, row, !isConverted);
			
			if (!isConverted)
			{
				PXUIFieldAttribute.SetEnabled<PMQuote.quoteProjectCD>(cache, row, !DimensionMaint.IsAutonumbered(this, ProjectAttribute.DimensionName));
			}

			PXUIFieldAttribute.SetEnabled<PMQuote.opportunityID>(cache, row, row.OpportunityID == null || !IsReadonlyPrimaryQuote(row.QuoteID));

			if (!(row.OpportunityID != null && row.Status == PMQuoteStatusAttribute.Draft))
				PXUIFieldAttribute.SetEnabled<PMQuote.bAccountID>(cache, row, row.OpportunityID == null);

			Approval.AllowSelect = row.IsSetupApprovalRequired.GetValueOrDefault();

			if (row.OpportunityIsActive == false)
			{
				cache.RaiseExceptionHandling<PMQuote.opportunityID>(row, row.OpportunityID,
					new PXSetPropertyException(CR.Messages.OpportunityIsNotActive, PXErrorLevel.Warning));
			}

			if (!UnattendedMode)
			{
				CRShippingAddress shipAddress = this.Shipping_Address.Select();
				CRAddress contactAddress = this.Quote_Address.Select();
				bool enableAddressValidation = ((shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
												|| (contactAddress != null && (contactAddress.IsDefaultAddress == false || row.BAccountID == null && row.ContactID == null) && contactAddress.IsValidated == false));
				this.validateAddresses.SetEnabled(enableAddressValidation);
			}
		}

		protected virtual void PMQuote_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as PMQuote;
			if (row == null) return;

			foreach (var product in Products.Select().RowCast<CR.CROpportunityProducts>())
			{
				Products.Cache.Update(product);
			}

			if (row.TemplateID != null)
			{
				DefaultFromTemplate(row);
				RedefaultTasksFromTemplate(row);
			}
		}
		
		protected virtual void PMQuote_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var oldRow = e.OldRow as PMQuote;
			var row = e.Row as PMQuote;
			if (oldRow == null || row == null) return;

			if (row.ContactID != null && row.ContactID != oldRow.ContactID)
			{
				object newCustomerId = row.BAccountID;
				if (newCustomerId == null)
					FillDefaultBAccountID(row);
			}

			var customerChanged = row.BAccountID != oldRow.BAccountID;
			var locationChanged = row.LocationID != oldRow.LocationID;
			var docDateChanged = row.DocumentDate != oldRow.DocumentDate;
			var projectChanged = row.QuoteProjectID != oldRow.QuoteProjectID;
			if (locationChanged || docDateChanged || projectChanged || customerChanged)
			{
				var productsCache = Products.Cache;
				foreach (CR.CROpportunityProducts line in SelectProducts(row.QuoteID))
				{
					var lineCopy = (CR.CROpportunityProducts)productsCache.CreateCopy(line);
					lineCopy.ProjectID = row.QuoteProjectID;
					lineCopy.CustomerID = row.BAccountID;
					productsCache.Update(lineCopy);
				}
			}
		}

		protected virtual void PMQuote_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as PMQuote;
			if (row == null) return;

			if (row.Status == PMQuoteStatusAttribute.Closed)
			{
				sender.RaiseExceptionHandling<PMQuote.status>(e.Row, null, new PXSetPropertyException(Messages.ClosedQuoteCannotBeDeleted));
				e.Cancel = true;
			}

			if (row.IsPrimary == true && !IsSingleQuote(row.OpportunityID))
			{
				sender.RaiseExceptionHandling<PMQuote.isPrimary>(e.Row, null, new PXSetPropertyException(ErrorMessages.PrimaryQuote));
				e.Cancel = true;
			}
		}

		protected virtual void PMQuote_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (PMQuote)e.Row;
			if (row == null) return;

			if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.BAccountID != null && row.LocationID == null)
			{
				sender.RaiseExceptionHandling<PMQuote.locationID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
				e.Cancel = true;
			}

			var dbQuote = QuoteInDb.SelectSingle(row.QuoteID);
			if (row.OpportunityID != null && row.OpportunityID != dbQuote?.OpportunityID)
			{
				CR.Standalone.CROpportunity oppty = Opportunity.Select(row.OpportunityID);
				if (oppty != null && oppty.IsActive != true)
				{
					sender.RaiseExceptionHandling<PMQuote.opportunityID>(row, row.OpportunityID,
						new PXSetPropertyException(Messages.QuoteCannotBeLinkedToNotActiveOpportunity, PXErrorLevel.RowError));
					throw new PXSetPropertyException(Messages.QuoteCannotBeLinkedToNotActiveOpportunity, PXErrorLevel.RowError);
				}
			}

			if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.OpportunityID != null)
			{
				row.IsFirstQuote = false;
				if (IsFirstQuote(row.OpportunityID))
				{
					row.IsFirstQuote = true;
				}
			}
		}

		protected virtual void PMQuote_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			PMQuote row = e.Row as PMQuote;
			if (row == null) return;

			if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.OpportunityID != null && e.TranStatus == PXTranStatus.Open)
			{
				if (row.IsFirstQuote == true)
				{
					var opportunity = (CR.Standalone.CROpportunity)PXSelect<CR.Standalone.CROpportunity,
						Where<CR.Standalone.CROpportunity.opportunityID, Equal<Required<CR.Standalone.CROpportunity.opportunityID>>>>
						.SelectSingleBound(this, null, row.OpportunityID);

					if (opportunity.DefQuoteID != row.QuoteID)
					{
						PXDatabase.Delete<CROpportunityDiscountDetail>(new PXDataFieldRestrict<CROpportunityDiscountDetail.quoteID>(PXDbType.UniqueIdentifier, opportunity.DefQuoteID));
						PXDatabase.Delete<CROpportunityTax>(new PXDataFieldRestrict<CROpportunityTax.quoteID>(PXDbType.UniqueIdentifier, opportunity.DefQuoteID));
						PXDatabase.Delete<CROpportunityProducts>(new PXDataFieldRestrict<CROpportunityProducts.quoteID>(PXDbType.UniqueIdentifier, opportunity.DefQuoteID));
						PXDatabase.Delete<CROpportunityRevision>(new PXDataFieldRestrict<CROpportunityRevision.noteID>(PXDbType.UniqueIdentifier, opportunity.DefQuoteID));
					}

					PXDatabase.Update<CR.Standalone.CROpportunity>(
						new PXDataFieldAssign<CR.Standalone.CROpportunity.defQuoteID>(row.QuoteID),
						new PXDataFieldRestrict<CR.Standalone.CROpportunity.opportunityID>(PXDbType.VarChar, 255, row.OpportunityID, PXComp.EQ)
						);
				}
			}

			if (e.Operation == PXDBOperation.Insert && row.IsPrimary == true && row.OpportunityID != null && e.TranStatus == PXTranStatus.Open)
			{
				PXDatabase.Update<CR.Standalone.CROpportunity>(
					new PXDataFieldAssign<CR.Standalone.CROpportunity.defQuoteID>(row.QuoteID),
					new PXDataFieldRestrict<CR.Standalone.CROpportunity.opportunityID>(PXDbType.VarChar, 255, row.OpportunityID, PXComp.EQ)
				);
			}
		}
		
		protected virtual void _(Events.RowPersisting<PMQuote> e)
		{
			PXDimensionAttribute.SuppressAutoNumbering<PMQuote.quoteProjectCD>(e.Cache, true);

			if (e.Row.BranchID == null)
			{
				Quote.Cache.RaiseExceptionHandling<PMQuote.branchID>(e.Row, e.Row.BranchID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMQuote.branchID>(Quote.Cache)));
			}
		}

		protected virtual void _(Events.RowSelected<PMQuote> e)
		{
			UpdateAddCommonTasksActionAvailability(e.Row);
		}
		#endregion

		protected virtual void CROpportunityRevision_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			//Suppress update revision for quote, should be done by main DAC
			var row = (CROpportunityRevision)e.Row;
			if (row != null && this.Quote.Current != null &&
				row.NoteID == this.Quote.Current.QuoteID)
				e.Cancel = true;
		}

		#region CROpportunityProducts

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictSiteByBranch(typeof(PMQuote.branchID), ShowWarning = true)]
		protected virtual void _(Events.CacheAttached<CROpportunityProducts.siteID> e)
		{
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts, CROpportunityProducts.inventoryID> e)
		{
			if (e.Row == null) return;

			if (e.Row.EmployeeID != null)
			{
				EP.EPEmployee employee = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, e.Row.EmployeeID);

				if (employee != null)
				{
					e.NewValue = employee.LabourItemID;
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts, CROpportunityProducts.curyUnitCost> e)
		{
			if (e.Row == null) return;

			e.NewValue = RateService.CalculateUnitCost(e.Cache, e.Row.ProjectID, e.Row.TaskID, e.Row.InventoryID, e.Row.UOM, e.Row.EmployeeID, Quote.Current.DocumentDate, Quote.Current.CuryInfoID);
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts, CROpportunityProducts.expenseAccountGroupID> e)
		{
			if (e.Row == null) return;

			if (e.Row.InventoryID != null)
			{
				InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<CROpportunityProducts.inventoryID>(e.Cache, e.Row);
				if (item != null)
				{
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, item.COGSAcctID);
					if (account != null && account.AccountGroupID != null)
					{
						e.NewValue = account.AccountGroupID;
					}
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts, CROpportunityProducts.revenueAccountGroupID> e)
		{
			if (e.Row == null) return;
		
			var select = new PXSelect<PMAccountGroup, Where<PMAccountGroup.type, Equal<GL.AccountType.income>>>(this);

			var resultset = select.SelectWindowed(0, 2);

			if (resultset.Count == 1)
			{
				e.NewValue = ((PMAccountGroup)resultset).GroupID;
			}
			else
			{
				if (e.Row.ExpenseAccountGroupID != null)
				{
					PMAccountGroup ag = PMAccountGroup.PK.Find(this, e.Row.ExpenseAccountGroupID);
					if (ag != null && ag.RevenueAccountGroupID != null)
					{
						e.NewValue = ag.RevenueAccountGroupID;
					}
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts, CROpportunityProducts.taskCD> e)
		{
			if (e.Row == null) return;

			e.NewValue = GetDefaultTaskCD();
		}
		
		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.expenseAccountGroupID> e)
		{
			PXUIFieldAttribute.SetWarning<CROpportunityProducts.expenseAccountGroupID>(e.Cache, e.Row, null);
			e.Cache.SetDefaultExt<CROpportunityProducts.revenueAccountGroupID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdating<CROpportunityProducts, CROpportunityProducts.revenueAccountGroupID> e)
		{
			PXUIFieldAttribute.SetWarning<CROpportunityProducts.revenueAccountGroupID>(e.Cache, e.Row, null);
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.inventoryID> e)
		{
			if (e.Row == null) return;

			e.Cache.SetDefaultExt<CROpportunityProducts.expenseAccountGroupID>(e.Row);
			e.Cache.SetDefaultExt<CROpportunityProducts.uOM>(e.Row);
			e.Cache.SetDefaultExt<CROpportunityProducts.curyUnitCost>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.uOM> e)
		{
			if (e.Row == null) return;

			e.Cache.SetDefaultExt<CROpportunityProducts.curyUnitCost>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.employeeID> e)
		{
			if (e.Row == null) return;

			if (e.Row.InventoryID == null)
			{
				e.Cache.SetDefaultExt<CROpportunityProducts.inventoryID>(e.Row);

                if (e.Row.InventoryID == null)
                {
                    e.Row.UOM = EPSetup.Hour;
                }
			}

			e.Cache.SetDefaultExt<CROpportunityProducts.curyUnitCost>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.curyExtCost> e)
		{
			if (e.Row == null) return;

			if (e.Row.CuryExtCost.GetValueOrDefault() == 0m && e.Row.ExpenseAccountGroupID == null &&
				PXUIFieldAttribute.GetError<CROpportunityProducts.revenueAccountGroupID>(e.Cache, e.Row) == Messages.MissingRevenueAccountGroup )
			{
				PXUIFieldAttribute.SetWarning<CROpportunityProducts.expenseAccountGroupID>(e.Cache, e.Row, null);
			}
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.curyAmount> e)
		{
			if (e.Row == null) return;

			if (e.Row.CuryAmount.GetValueOrDefault() == 0m && e.Row.RevenueAccountGroupID == null &&
				PXUIFieldAttribute.GetError<CROpportunityProducts.expenseAccountGroupID>(e.Cache, e.Row) == Messages.MissingExpenseAccountGroup)
			{
				PXUIFieldAttribute.SetWarning<CROpportunityProducts.revenueAccountGroupID>(e.Cache, e.Row, null);
			}
		}

		protected virtual void _(Events.RowSelected<CROpportunityProducts> e)
		{
			if (e.Row == null) return;

			string messageRevenue = PXUIFieldAttribute.GetError<CROpportunityProducts.revenueAccountGroupID>(e.Cache, e.Row);

			if (e.Row.CuryAmount.GetValueOrDefault() != 0m  && e.Row.RevenueAccountGroupID == null && string.IsNullOrEmpty(messageRevenue) )
			{
				PXUIFieldAttribute.SetWarning<CROpportunityProducts.revenueAccountGroupID>(e.Cache, e.Row, Messages.MissingRevenueAccountGroup);
			}

			string messageExpense = PXUIFieldAttribute.GetError<CROpportunityProducts.expenseAccountGroupID>(e.Cache, e.Row);
			if (e.Row.CuryExtCost.GetValueOrDefault() != 0m && e.Row.ExpenseAccountGroupID == null && string.IsNullOrEmpty(messageExpense))
			{
				PXUIFieldAttribute.SetWarning<CROpportunityProducts.expenseAccountGroupID>(e.Cache, e.Row, Messages.MissingExpenseAccountGroup);
			}

		}

		protected virtual void _(Events.RowPersisting<CROpportunityProducts> e)
		{
			if(e.Row is CROpportunityProducts row && row?.TaskCD != null)
			{
				if(PersistingTaskMap.TryGetValue(row.TaskCD, out string newTaskCD))
				{
					row.TaskCD = newTaskCD;
				}
			}
		}

		#endregion

		protected virtual void _(Events.RowSelected<ConvertToProjectFilter> e)
		{
			if (e.Row == null) return;

			if (Setup.Current.AssignmentMapID != null)
			{
				PXUIFieldAttribute.SetDisplayName<ConvertToProjectFilter.activateProject>(e.Cache, Messages.SubmitProjectForApproval);
			}

			PXUIFieldAttribute.SetVisible<ConvertToProjectFilter.taskCD>(e.Cache, e.Row, Tasks.Select().Count > 1);
			PXUIFieldAttribute.SetEnabled<ConvertToProjectFilter.taskCD>(e.Cache, e.Row, e.Row.MoveActivities == true);
		}

		#region PMQuoteTask
		protected virtual void _(Events.FieldUpdated<PMQuoteTask, PMQuoteTask.isDefault> e)
		{
			if (e.Row.IsDefault == true)
			{
				bool requestRefresh = false;
				foreach (PMQuoteTask task in Tasks.Select())
				{
					if (task.IsDefault == true && task.TaskCD != e.Row.TaskCD)
					{
						Tasks.Cache.SetValue<PMQuoteTask.isDefault>(task, false);
						Tasks.Cache.SmartSetStatus(task, PXEntryStatus.Updated);

						requestRefresh = true;
					}
				}

				if (requestRefresh)
				{
					Tasks.View.RequestRefresh();
				}

				foreach(CROpportunityProducts item in Products.Select())
				{
					if (string.IsNullOrWhiteSpace(item.TaskCD))
					{
						Products.Cache.SetValue<CROpportunityProducts.taskCD>(item, e.Row.TaskCD);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMQuoteTask, PMQuoteTask.taskCD> e)
		{
			foreach (CROpportunityProducts item in Products.Select())
			{
				if (!string.IsNullOrWhiteSpace(item.TaskCD) &&  item.TaskCD == (string)e.OldValue)
				{
					Products.Cache.SetValue<CROpportunityProducts.taskCD>(item, e.Row.TaskCD);
					Products.Cache.MarkUpdated(item, assertError: true);
				}
			}
		}

		protected virtual void _(Events.RowDeleting<PMQuoteTask> e)
		{
			var resultset = Tasks.Select();
			if (resultset.Count > 1)
			{
				bool isUsed = false;
				foreach (CROpportunityProducts item in Products.Select())
				{
					if (item.TaskCD == e.Row.TaskCD)
					{
						isUsed = true;
						break;
					}
				}

				if (isUsed)
				{
					throw new PXException(Messages.CannotDeleteUsedTask);
				}
			}
		}

		protected virtual void _(Events.RowDeleted<PMQuoteTask> e)
		{
			var resultset = Tasks.Select();
			if (resultset.Count < 2)
			{
				string taskCD = null;

				if (resultset.Count > 0)
				{
					PMQuoteTask onlyTask = (PMQuoteTask)resultset[0];
					if (onlyTask != null)
					{
						taskCD = onlyTask.TaskCD;
					}
				}

				foreach (CROpportunityProducts item in Products.Select())
				{
					Products.Cache.SetValue<CROpportunityProducts.taskCD>(item, taskCD);
					Products.Cache.MarkUpdated(item, assertError: true);
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PMQuoteTask> e)
		{			
			if(e.Row is PMQuoteTask row)
			{
				PersistingTaskCD = row.TaskCD;
			}
		}

		protected virtual void _(Events.RowPersisted<PMQuoteTask> e)
		{
			if (e.Row is PMQuoteTask row)
			{
				PersistingTaskMap[PersistingTaskCD] = row.TaskCD;
			}
		}
		#endregion

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

		private void FillDefaultBAccountID(PMQuote row)
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

		private IEnumerable SelectProducts(object quoteId)
		{
			if (quoteId == null)
				return new CROpportunityProducts[0];

			return PXSelect<CR.CROpportunityProducts,
					Where<CR.CROpportunityProducts.quoteID, Equal<Required<PMQuote.quoteID>>>>.
				Select(this, quoteId).
				RowCast<CR.CROpportunityProducts>();
		}

		private IEnumerable SelectDiscountDetails(object quoteId)
		{
			if (quoteId == null)
				return new CROpportunityDiscountDetail[0];

			return PXSelect<CR.CROpportunityDiscountDetail,
					Where<CR.CROpportunityDiscountDetail.quoteID, Equal<Required<PMQuote.quoteID>>>>.
				Select(this, quoteId).
				RowCast<CR.CROpportunityDiscountDetail>();
		}

		private bool IsFirstQuote(string opportunityID)
		{
			var quotes = PXSelectJoin<CR.Standalone.CRQuote,
				InnerJoin<CROpportunityRevision,
					On<CROpportunityRevision.noteID, Equal<CR.Standalone.CRQuote.quoteID>>>,
				Where<CROpportunityRevision.opportunityID, Equal<Required<CROpportunityRevision.opportunityID>>,
				And<CR.Standalone.CRQuote.quoteID, NotEqual<Current<PMQuote.quoteID>>>>>
				.SelectSingleBound(this, null, opportunityID);

			return quotes.Count == 0;
		}

		private bool IsSingleQuote(string opportunityId)
		{
			var quote = PXSelect<CR.CRQuote, Where<CR.CRQuote.opportunityID, Equal<Optional<CR.CRQuote.opportunityID>>>>.SelectWindowed(this, 0, 2, opportunityId);
			return (quote.Count == 1);
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

		#region Public Methods
		public bool IsReadonlyPrimaryQuote(Guid? quoteID)
		{
			var opportunity = PXSelectReadonly<CR.Standalone.CROpportunity,
				Where<CR.Standalone.CROpportunity.defQuoteID, Equal<Required<CR.Standalone.CROpportunity.defQuoteID>>>>
				.SelectSingleBound(this, null, quoteID);
			return opportunity.Count == 1;
		} 
		#endregion

		#region Avalara Tax
		public virtual bool IsExternalTax(string taxZoneID)
		{
			return false;
		}
		public virtual PMQuote CalculateExternalTax(PMQuote quote)
		{
			return quote;
		}
		#endregion

		#region Implementation of IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (Quote.Current != null)
			{
				var row = Quote.Cache.GetExtension<Extensions.SalesTax.Document>(Quote.Current);
				if (row != null)
				{
					row.TaxCalc = TaxCalc.NoCalc;
				}
			}
			
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

		#region IPXProcess
		public void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			if (Quote.Current != null)
			{
				var row = Quote.Cache.GetExtension<Extensions.SalesTax.Document>(Quote.Current);
				if (row != null)
				{
					row.TaxCalc = TaxCalc.Calc;
				}

				Quote.Cache.RaiseFieldUpdated<PMQuote.taxZoneID>(Quote.Current, null);
			}
		}
		#endregion

		protected virtual void AddSelectedCommonTasks()
		{
			foreach (SelectedTask task in TasksForAddition.Cache.Updated)
			{
				if (task.Selected == true)
				{
					InsertNewTaskWithProjectTask(Quote.Current, task);
					task.Selected = false;
				}
			}
		}

		protected virtual void UpdateAddCommonTasksActionAvailability(PMQuote quote)
		{
			bool enabled = quote?.Status == CRQuoteStatusAttribute.Draft;
			addCommonTasks.SetEnabled(enabled);
		}

		protected void SuppressCascadeDeletion(PXView view, object row)
		{
			PXCache cache = this.Caches[row.GetType()];
			foreach (object rec in view.Cache.Deleted)
			{
				if (view.Cache.GetStatus(rec) == PXEntryStatus.Deleted)
				{
					bool own = true;
					foreach (string key in new[] { typeof(CR.CROpportunity.quoteNoteID).Name })
					{
						if (!object.Equals(cache.GetValue(row, key), view.Cache.GetValue(rec, key)))
						{
							own = false;
							break;
						}
					}
					if (own)
						view.Cache.SetStatus(rec, PXEntryStatus.Notchanged);
				}
			}
		}

		

		#region Convert To Project

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		[System.Diagnostics.DebuggerDisplay("{ProjectID}.{ProjectTaskID}.{InventoryID}.{EmployeeID}")]
		private struct LaborRateKey
		{
			public readonly int ProjectID;
			public readonly int ProjectTaskID;
			public readonly int InventoryID;
			public readonly int EmployeeID;

			public LaborRateKey(int projectID, int projectTaskID, int inventoryID, int employeeID)
			{
				ProjectID = projectID;
				ProjectTaskID = projectTaskID;
				InventoryID = inventoryID;
				EmployeeID = employeeID;
			}

			public override int GetHashCode()
			{
				unchecked // Overflow is fine, just wrap
				{
					int hash = 17;
					hash = hash * 23 + ProjectID.GetHashCode();
					hash = hash * 23 + ProjectTaskID.GetHashCode();
					hash = hash * 23 + InventoryID.GetHashCode();
					hash = hash * 23 + EmployeeID.GetHashCode();
					return hash;
				}
			}
		}

		public virtual void ConvertQuoteToProject(PMQuote row, ConvertToProjectFilter settings)
		{
			int errorCount = 0;
			if (!ValidateQuoteBeforeConvertToProject(row, out errorCount))
			{
				throw new PXException(Messages.QuoteConversionFailed);
			}

			ProjectEntry projectEntry = CreateInstance<ProjectEntry>();
			projectEntry.SuppressTemplateIDUpdated = true;
			projectEntry.Clear();

			PMProject project = new PMProject();
			project.BaseType = PMProject.ProjectBaseType.Project;

			CurrencyInfo info = projectEntry.GetExtension<ProjectEntry.MultiCurrency>().CloneCurrencyInfo(GetExtension<MultiCurrency>().GetDefaultCurrencyInfo());
			project.CuryID = row.CuryID;
			project.CuryInfoID = info.CuryInfoID;
			project.RateTypeID = info.CuryRateTypeID;
			
			
			if (!DimensionMaint.IsAutonumbered(this, ProjectAttribute.DimensionName))
				project.ContractCD = row.QuoteProjectCD;

			try
			{
				project = projectEntry.Project.Insert(project);
			}
			catch (PXFieldProcessingException ex)
			{
				if (string.Equals(ex.FieldName, typeof(PMProject.contractCD).Name, StringComparison.OrdinalIgnoreCase))
				{
					PMQuoteProjectCDDimensionAttribute.CheckProjectCD(this, row.QuoteProjectCD, ex.Message, false);
				}

				throw ex;
			}

			project.CustomerID = row.BAccountID;
			if (row.LocationID != null)
				project.LocationID = row.LocationID;
			if (row.TermsID != null)
				project.TermsID = row.TermsID;
			project.QuoteNbr = row.QuoteNbr;
			project = projectEntry.Project.Update(project);

			project.TemplateID = row.TemplateID;
			projectEntry.DefaultFromTemplate(project, row.TemplateID, new ProjectEntry.DefaultFromTemplateSettings() { CopyProperties = true, CopyAttributes = true, CopyEmployees = true, CopyEquipment = true, CopyNotification = true, CopyCurrency = false, CopyNotes = settings.CopyNotes, CopyFiles = settings.CopyFiles });
			PX.Objects.CR.Standalone.EPEmployee owner = PXSelect<PX.Objects.CR.Standalone.EPEmployee, Where<PX.Objects.CR.Standalone.EPEmployee.bAccountID, Equal<Required<PX.Objects.CR.Standalone.EPEmployee.bAccountID>>>>.Select(this, row.ProjectManager);
			if (owner != null)
            {
				project.OwnerID = owner.DefContactID;
				project.ApproverID = row.ProjectManager;
			}			
			project.Description = row.Subject;
			if (PXDBLocalizableStringAttribute.IsEnabled)
				PXDBLocalizableStringAttribute.DefaultTranslationsFromMessage(Caches[typeof(PMProject)], project, "Description", row.Subject);

			project.TermsID = row.TermsID;
			project.BaseCuryID = info.BaseCuryID;
			if (row.BranchID != null)
                project.DefaultBranchID = row.BranchID;
			project = projectEntry.Project.Update(project);
			if (string.IsNullOrEmpty(projectEntry.Billing.Current.Type))
				projectEntry.Billing.Current.Type = BillingType.OnDemand;

			PXNoteAttribute.CopyNoteAndFiles(Quote.Cache, row, projectEntry.Project.Cache, project, settings.CopyNotes, settings.CopyFiles);
			
			Dictionary<string, int> taskMap = new Dictionary<string, int>();
			AddingTasksToProject(row, projectEntry, taskMap, settings.CopyNotes, settings.CopyFiles);
			AddingCostBudgetToProject(projectEntry, taskMap, project.CostBudgetLevel);
			AddingRevenueBudgetToProject(projectEntry, taskMap, project.BudgetLevel);
			AddingBillingInfoToProject(row, projectEntry);
			AddingShippingInfoToProject(row, projectEntry);

			HashSet<int> employeeList = new HashSet<int>();
			foreach (CROpportunityProducts item in Products.Select())
			{				
				if (item.EmployeeID != null)
				{
					employeeList.Add(item.EmployeeID.Value);
				}
			}

			foreach (int employeeID in employeeList)
			{
				EPEmployeeContract item = projectEntry.EmployeeContract.Insert(new EPEmployeeContract() { ContractID = project.ContractID, EmployeeID = employeeID });
			}

			projectEntry.Answers.CopyAllAttributes(project, row);
			
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (settings.ActivateTasks == true)
				{
					projectEntry.activateTasks.Press();
				}

				if (settings.ActivateProject == true)
				{
					projectEntry.activate.Press();
				}

				projectEntry.Save.Press();

				row.QuoteProjectID = projectEntry.Project.Current.ContractID;
				row.Status = PMQuoteStatusAttribute.Closed;
				
				Quote.Update(row);

				Dictionary<string, int> taskMapPersisted = new Dictionary<string, int>();

				int? activityTaskID = null;
				List<PMTask> taskList = new List<PMTask>(projectEntry.Tasks.Select().RowCast<PMTask>());
				foreach (PMTask task in taskList)
				{
					taskMapPersisted.Add(task.TaskCD, task.TaskID.Value);
					if (task.TaskCD == settings.TaskCD)
					{
						activityTaskID = task.TaskID;
					}
				}
				
				if (taskList.Count == 1 || string.IsNullOrEmpty(settings.TaskCD))
				{
					activityTaskID = taskList[0].TaskID;
				}
				
				Dictionary<LaborRateKey, decimal> laborCosts = new Dictionary<LaborRateKey, decimal>();
				foreach (CROpportunityProducts item in Products.Select())
				{
					string taskcd = GetItemTaskCD(item);
					item.TaskID = taskMapPersisted[taskcd];
					Products.Update(item);

					if (settings.CreateLaborRates == true && item.InventoryID != null && item.EmployeeID != null && item.UnitCost != null)
					{
						InventoryItem laborItem = (InventoryItem)PXSelectorAttribute.Select<CROpportunityProducts.inventoryID>(Products.Cache, item);
						if (laborItem != null && laborItem.ItemType == INItemTypes.LaborItem)
						{
							LaborRateKey key = new LaborRateKey(item.ProjectID.Value, item.TaskID.Value, item.InventoryID.Value, item.EmployeeID.Value);
							if (!laborCosts.ContainsKey(key))
								laborCosts.Add(key, item.UnitCost.Value);
							else
							{
								laborCosts[key] = Math.Max(laborCosts[key], item.UnitCost.Value);
							}
						}						
					}
				}
				
				if (settings.MoveActivities == true)
				{
					CREmailActivityMaint timeActivityMaint = CreateInstance<CREmailActivityMaint>();

					var newTimeActivities = new List<PMTimeActivity>();

					var activitiesExt = this.GetExtension<PMQuoteMaint_ActivityDetailsExt>();

					foreach (CRPMTimeActivity activity in activitiesExt.Activities.Select())
					{
						if (activity.TrackTime == true) // CRActivity already has PMActivity
						{
							activity.ProjectID = row.QuoteProjectID;
							activity.ProjectTaskID = activityTaskID;

							if (activity.CostCodeID == null)
							{
								activity.CostCodeID = CostCodeAttribute.DefaultCostCode;
							}

							activitiesExt.Activities.Update(activity);
						}
						else // Create new PMActivity to link CRActivity to the project
						{
							PMTimeActivity timeActivity = timeActivityMaint.TAct.Cache.CreateInstance() as PMTimeActivity;

							// Linking to existing CRActivity
							timeActivity.RefNoteID = activity.NoteID;

							timeActivity = timeActivityMaint.TAct.Insert(timeActivity);

							timeActivityMaint.TAct.Cache.SetValueExt<PMTimeActivity.trackTime>(timeActivity, true);

							timeActivityMaint.TAct.Update(timeActivity);

							// Setting required properties
							timeActivity.Summary = activity.Subject;
							timeActivity.IsBillable = false;

							if (timeActivity.CostCodeID == null)
							{
								timeActivity.CostCodeID = CostCodeAttribute.DefaultCostCode;
							}

							timeActivityMaint.TAct.Update(timeActivity);

							newTimeActivities.Add(timeActivity);

							timeActivityMaint.Save.Press();
						}
					}

					Save.Press();

					if (newTimeActivities.Count > 0)
					{
						foreach (var pmActivity in newTimeActivities)
						{
							pmActivity.ProjectID = row.QuoteProjectID;
							pmActivity.ProjectTaskID = activityTaskID;

							timeActivityMaint.TAct.Update(pmActivity);
							timeActivityMaint.Save.Press();
						}
					}
				}

				if (settings.CreateLaborRates == true)
				{
					LaborCostRateMaint rateMaint = PXGraph.CreateInstance<LaborCostRateMaint>();
					rateMaint.Clear();
					rateMaint.Filter.Insert();
					rateMaint.Filter.Current.ProjectID = row.QuoteProjectID;

					foreach (KeyValuePair<LaborRateKey, decimal> item in laborCosts)
					{
						rateMaint.Items.Insert(new PMLaborCostRate() { Type = PMLaborCostRateType.Project, ProjectID = item.Key.ProjectID, TaskID = item.Key.ProjectTaskID, EmployeeID = item.Key.EmployeeID, InventoryID = item.Key.InventoryID, EffectiveDate = row.DocumentDate, Rate = item.Value });
					}

					rateMaint.Save.Press();
				}

				Save.Press();
				ts.Complete();
			}
		}

		public virtual void DefaultFromTemplate(PMQuote row)
		{
			var currentBranchID = PXAccess.GetBranchID();
			// If the project template is empty or does not contain default branch
			// replace quote branch with current branch.
			if (currentBranchID != null)
				row.BranchID = currentBranchID;

			var template = PMProject.PK.Find(this, row.TemplateID);
			if (template == null)
				return;

			if (template.DefaultBranchID != null)
				row.BranchID = template.DefaultBranchID;

			PX.Objects.CR.Standalone.EPEmployee owner
				= PXSelect<PX.Objects.CR.Standalone.EPEmployee,
					Where<PX.Objects.CR.Standalone.EPEmployee.defContactID,
						Equal<Required<PX.Objects.CR.Standalone.EPEmployee.defContactID>>>>
				.Select(this, template.OwnerID);
			if (owner != null)
				row.ProjectManager = owner.BAccountID;

			Answers.CopyAllAttributes(row, template);
			//Remove template attributes added to the cache by the CopyAllAttributes:
			foreach (CSAnswers answer in Answers.Cache.Inserted)
			{
				if (answer.RefNoteID == template.NoteID)
					Answers.Delete(answer);
			}

			string oldNote = PXNoteAttribute.GetNote(Quote.Cache, row);

			PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(PMProject)], template, Quote.Cache, row, string.IsNullOrWhiteSpace(oldNote), true);
		}

		public virtual void DeleteNoteAndFilesOfTemplate(PMQuote quote, int? templateId)
		{
			if (!templateId.HasValue)
				return;

			var template = PMProject.PK.Find(this, templateId);

			if (template == null)
				return;

			var templateCache = Caches[typeof(PMProject)];

			var currentNote = PXNoteAttribute.GetNote(Quote.Cache, quote);
			if (!string.IsNullOrWhiteSpace(currentNote))
			{
				var templateNote = PXNoteAttribute.GetNote(templateCache, template);
				if (string.Equals(currentNote, templateNote, StringComparison.Ordinal))
					PXNoteAttribute.SetNote(Quote.Cache, quote, null);
			}

			var templateFileNotes = PXNoteAttribute.GetFileNotes(templateCache, template);
			if (templateFileNotes.Length > 0)
			{
				var currentFileNotes = PXNoteAttribute.GetFileNotes(Quote.Cache, quote);
				if (currentFileNotes.Length > 0)
				{
					var intersection = templateFileNotes.Intersect(currentFileNotes);
					if (intersection.Any())
					{
						var fileReferencesToDelete
							= new PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>,
													And<NoteDoc.fileID, In<Required<NoteDoc.fileID>>>>>(this)
												   .Select(quote.NoteID, intersection.ToArray());														

						foreach (NoteDoc fileReference in fileReferencesToDelete)
							QuoteNotes.Delete(fileReference);
					}						
				}
			}
		}

		public virtual void DeleteAllTasks()
		{
			foreach (PMQuoteTask task in Tasks.Select())
			{
				Tasks.Delete(task);
			}
		}

		public virtual void RedefaultTasksFromTemplate(PMQuote row)
		{
			DeleteAllTasks();

			var projectTasksQuery = new PXSelect<PMTask,
				Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
					And<PMTask.autoIncludeInPrj, Equal<True>>>>(this);
						
			foreach (PMTask task in projectTasksQuery.Select(row.TemplateID))
			{
				InsertNewTaskWithProjectTask(row, task);
			}
		}

		public virtual PMQuoteTask InsertNewTaskWithProjectTask(PMQuote projectQuote, PMTask projectTask)
		{
			return InsertNewTaskWithProjectTask(projectQuote, projectTask, null);
		}

		public virtual PMQuoteTask InsertNewTaskWithProjectTask(PMQuote projectQuote, PMTask projectTask, Action<PMQuoteTask, PMTask> extension)
		{
			var newTask = new PMQuoteTask { QuoteID = projectQuote.QuoteID, TaskCD = projectTask.TaskCD, Description = projectTask.Description, IsDefault = projectTask.IsDefault, TaxCategoryID = projectTask.TaxCategoryID };

			if (extension != null)
			{
				extension(newTask, projectTask);
			}

			var quoteTask = Tasks.Insert(newTask);

			if (quoteTask == null)
				return null;

			var sourceCache = Caches[typeof(PMTask)];

			PXNoteAttribute.CopyNoteAndFiles(sourceCache, projectTask, Tasks.Cache, quoteTask);
			PXDBLocalizableStringAttribute.CopyTranslations<PMTask.description, PMQuoteTask.description>(sourceCache, projectTask, Tasks.Cache, quoteTask);

			return quoteTask;
		}

		public virtual void AddingBillingInfoToProject(PMQuote row, ProjectEntry projectEntry)
		{
			if (row.AllowOverrideContactAddress == true)
			{
				CRContact contact = Quote_Contact.SelectSingle();
				CRAddress address = Quote_Address.SelectSingle();

				if (contact != null)
				{
					PMContact billingContact = projectEntry.Billing_Contact.Select();

					if (billingContact != null)
					{
						billingContact.FullName = contact.FullName;
						billingContact.Salutation = contact.Salutation;
						billingContact.Phone1 = contact.Phone1;
						billingContact.Email = contact.Email;
						billingContact = projectEntry.Billing_Contact.Update(billingContact);
						billingContact.IsDefaultContact = false;
						billingContact = projectEntry.Billing_Contact.Update(billingContact);
					}
				}

				if (address != null)
				{
					PMAddress billingAddress = projectEntry.Billing_Address.Select();

					if (billingAddress != null)
					{
						billingAddress.AddressLine1 = address.AddressLine1;
						billingAddress.AddressLine2 = address.AddressLine2;
						billingAddress.City = address.City;
						billingAddress.CountryID = address.CountryID;
						billingAddress.State = address.State;
						billingAddress.PostalCode = address.PostalCode;
						billingAddress = projectEntry.Billing_Address.Update(billingAddress);
						billingAddress.IsDefaultAddress = false;
						billingAddress = projectEntry.Billing_Address.Update(billingAddress);
					}
				}
			}
		}

		public virtual void AddingShippingInfoToProject(PMQuote row, ProjectEntry projectEntry)
		{
			CRShippingAddress shippingAddress = Shipping_Address.SelectSingle();
				PMSiteAddress projectAddress = projectEntry.Site_Address.Select();

			if (shippingAddress == null || projectAddress == null) return;

					projectAddress.AddressLine1 = shippingAddress.AddressLine1;
					projectAddress.AddressLine2 = shippingAddress.AddressLine2;
					projectAddress.City = shippingAddress.City;
					projectAddress.CountryID = shippingAddress.CountryID;
					projectAddress.State = shippingAddress.State;
					projectAddress.PostalCode = shippingAddress.PostalCode;
					projectAddress.Latitude = shippingAddress.Latitude;
					projectAddress.Longitude = shippingAddress.Longitude;

			projectEntry.Site_Address.Update(projectAddress);
		}

		[Obsolete]
		public virtual void AddingTasksToProject(PMQuote row, ProjectEntry projectEntry, Dictionary<string, int> taskMap)
		{
			AddingTasksToProject(row, projectEntry, taskMap, false, false);
		}

		public virtual void AddingTasksToProject(PMQuote row, ProjectEntry projectEntry, Dictionary<string, int> taskMap, bool? copyNotes, bool? copyFiles)
		{
			PXDimensionAttribute.SuppressAutoNumbering<PMTask.taskCD>(projectEntry.Tasks.Cache, true);
			bool tasksAdded = false;
			var resultset = Tasks.Select();
			foreach (PMQuoteTask task in resultset)
			{
				tasksAdded = true;
				if (resultset.Count == 1)
				{
					task.IsDefault = true;
				}

				PMTask templateTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, 
					And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(this, row.TemplateID, task.TaskCD);

				PMTask newTask;
				if (templateTask != null)
				{
					newTask = projectEntry.CopyTask(templateTask, projectEntry.Project.Current.ContractID.GetValueOrDefault(), new ProjectEntry.DefaultFromTemplateSettings() { CopyProperties = true, CopyAttributes = true, CopyEmployees = false, CopyEquipment = false, CopyNotification = false, CopyRecurring = true, CopyNotes = copyNotes, CopyFiles = copyFiles });
					ConfigureNewTask(projectEntry, newTask, task);

					PXNoteAttribute.CopyNoteAndFiles(Tasks.Cache, task, projectEntry.Tasks.Cache, newTask, copyNotes, copyFiles);
				}
				else
				{
					newTask = projectEntry.Tasks.Insert(ConfigureNewTask(projectEntry, new PMTask(), task));
				}

				taskMap.Add(task.TaskCD, newTask.TaskID.GetValueOrDefault());
			}

			if (!tasksAdded)
			{
				PMTask newTask = projectEntry.Tasks.Insert(new PMTask { TaskCD = DefaultTaskCD, IsDefault = true });
				projectEntry.Tasks.Cache.SetValueExt<PMTask.description>(newTask, "Default");
				taskMap.Add(newTask.TaskCD, newTask.TaskID.GetValueOrDefault());
			}
		}

		public virtual PMTask ConfigureNewTask(ProjectEntry projectEntry, PMTask newTask, PMQuoteTask quoteTask)
		{
			newTask.TaskCD = quoteTask.TaskCD;
			newTask.Description = quoteTask.Description;
			if (projectEntry != null)
				PXDBLocalizableStringAttribute.CopyTranslations<PMQuoteTask.description, PMTask.description>(Tasks.Cache, quoteTask, projectEntry.Tasks.Cache, newTask);
			newTask.PlannedStartDate = quoteTask.PlannedStartDate;
			newTask.PlannedEndDate = quoteTask.PlannedEndDate;
			newTask.IsDefault = quoteTask.IsDefault;
			newTask.TaxCategoryID = quoteTask.TaxCategoryID;

			return newTask;
		}
				
		public virtual void AddingCostBudgetToProject(ProjectEntry projectEntry, Dictionary<string, int> taskMap, string budgetLevel)
		{
			Dictionary<BudgetKeyTuple, PMCostBudget> budget = new Dictionary<BudgetKeyTuple, PMCostBudget>();
			HashSet<BudgetKeyTuple> aggregated = new HashSet<BudgetKeyTuple>();

			foreach (CROpportunityProducts item in Products.Select())
			{
				if (item.ExpenseAccountGroupID == null)
					continue;

				string taskcd = GetItemTaskCD(item);

				int inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				bool isEmptyInventoryItem = true;
				if (budgetLevel.IsIn(BudgetLevels.Item, BudgetLevels.Detail) && item.InventoryID != null)
				{
					inventoryID = item.InventoryID.Value;
					isEmptyInventoryItem = inventoryID == PMInventorySelectorAttribute.EmptyInventoryID;
				}

				int costcodeID = CostCodeAttribute.GetDefaultCostCode();
				if (budgetLevel.IsIn(BudgetLevels.CostCode, BudgetLevels.Detail) && item.CostCodeID != null)
				{
					costcodeID = item.CostCodeID.Value;
				}

				BudgetKeyTuple key = new BudgetKeyTuple(projectEntry.Project.Current.ContractID.GetValueOrDefault(), taskMap[taskcd], item.ExpenseAccountGroupID.GetValueOrDefault(), inventoryID, costcodeID);

				PMCostBudget existing;
				if (budget.TryGetValue(key, out existing))
				{
					aggregated.Add(key);

					if (existing.UOM != null && existing.UOM != item.UOM)
					{
						string uom = item.UOM;
						if (isEmptyInventoryItem && string.IsNullOrEmpty(item.UOM))
						{
							uom = Setup.Current.EmptyItemUOM;
						}

						if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>() && existing.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
						{
							try
							{
								decimal inBase = IN.INUnitAttribute.ConvertToBase(Caches[typeof(PMQuote)], item.InventoryID, item.UOM, item.Qty.GetValueOrDefault(), IN.INPrecision.QUANTITY);
								existing.Qty += IN.INUnitAttribute.ConvertFromBase(Caches[typeof(PMQuote)], item.InventoryID, existing.UOM, inBase, IN.INPrecision.QUANTITY);
							}
							catch (IN.PXUnitConversionException)
							{
								existing.Qty = 0;
								existing.UOM = null;
							}
						}
						else
						{
						decimal convertedQty;
						if (IN.INUnitAttribute.TryConvertGlobalUnits(projectEntry, uom, existing.UOM, item.Qty.GetValueOrDefault(), IN.INPrecision.QUANTITY, out convertedQty)) 
						{
							existing.Qty += convertedQty;
						}
						else
					{
						existing.Qty = 0;
						existing.UOM = null;
					}
					}
					}
					else if (existing.UOM != null)
					{
						existing.Qty += item.Qty.GetValueOrDefault();
					}
					existing.CuryAmount += item.ExtCost.GetValueOrDefault();

					//Note: MaxAmount is just used to store the aggregate of CuryUnitPrice. (has nothing to do with MaxAmount)
					existing.CuryMaxAmount += item.Qty.GetValueOrDefault() * item.CuryUnitPrice.GetValueOrDefault();
				}
				else
				{
					existing = new PMCostBudget();
					existing.ProjectID = key.ProjectID;
					existing.ProjectTaskID = key.ProjectTaskID;
					existing.AccountGroupID = key.AccountGroupID;
					existing.InventoryID = key.InventoryID;
					existing.CostCodeID = key.CostCodeID;
					existing.CuryAmount = item.CuryExtCost.GetValueOrDefault();
					existing.CuryUnitPrice = item.DiscPct != 0 ? (item.CuryUnitPrice* (100-item.DiscPct))/100 : item.CuryUnitPrice; 
					existing.Description = item.Descr;

					if (isEmptyInventoryItem && string.IsNullOrEmpty(item.UOM))
					{
						existing.UOM = Setup.Current.EmptyItemUOM;
					}
					else
					{
						existing.UOM = item.UOM;
					}
					existing.Qty = item.Qty.GetValueOrDefault();


					//Note: MaxAmount is just used to store the aggregate of CuryUnitPrice. (has nothing to do with MaxAmount)
					existing.CuryMaxAmount = item.Qty.GetValueOrDefault() * existing.CuryUnitPrice.GetValueOrDefault();

					budget.Add(key, existing);
				}
			}

			foreach(PMCostBudget item in budget.Values)
			{
				if (item.Qty.GetValueOrDefault() != 0)
					item.CuryUnitRate = item.CuryAmount / item.Qty.Value;

				if (item.Qty.GetValueOrDefault() != 0)
					item.CuryUnitPrice = item.CuryMaxAmount / item.Qty.Value;
				item.CuryMaxAmount = null;

				string description = item.Description;
				var newItem = projectEntry.CostBudget.Insert(item);

				newItem.UOM = item.UOM;
				
				if (!aggregated.Contains(BudgetKeyTuple.Create(item)))
				{
					newItem.Description = description;
				}
				else
				{
					PMAccountGroup accountGroup = PMAccountGroup.PK.Find(this, item.AccountGroupID);
					if (accountGroup != null)
					{
						newItem.Description = PXMessages.LocalizeNoPrefix(Messages.Aggregated) + accountGroup.Description;
					}
				}
			}
		}

		public virtual void AddingRevenueBudgetToProject(ProjectEntry projectEntry, Dictionary<string, int> taskMap, string budgetLevel)
		{
			Dictionary<BudgetKeyTuple, PMRevenueBudget> budget = new Dictionary<BudgetKeyTuple, PMRevenueBudget>();
			HashSet<BudgetKeyTuple> aggregated = new HashSet<BudgetKeyTuple>();

			foreach (CROpportunityProducts item in Products.Select())
			{
				if (item.RevenueAccountGroupID == null)
					continue;

				string taskcd = GetItemTaskCD(item);

				int inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				bool isEmptyInventoryItem = true;
				if (budgetLevel.IsIn(BudgetLevels.Item, BudgetLevels.Detail) && item.InventoryID != null)
				{
					inventoryID = item.InventoryID.Value;
					isEmptyInventoryItem = inventoryID == PMInventorySelectorAttribute.EmptyInventoryID;
				}

				int costcodeID = CostCodeAttribute.GetDefaultCostCode();
				if (budgetLevel.IsIn(BudgetLevels.CostCode, BudgetLevels.Detail) && item.CostCodeID != null)
				{
					costcodeID = item.CostCodeID.Value;
				}

				BudgetKeyTuple key = new BudgetKeyTuple(projectEntry.Project.Current.ContractID.GetValueOrDefault(), taskMap[taskcd], item.RevenueAccountGroupID.Value, inventoryID, costcodeID);

				PMRevenueBudget existing;
				if (budget.TryGetValue(key, out existing))
				{
					aggregated.Add(key);

					if (existing.TaxCategoryID != null && item.TaxCategoryID != null && existing.TaxCategoryID != item.TaxCategoryID)
					{
						existing.TaxCategoryID = null;
					}

					if (existing.UOM != null && existing.UOM != item.UOM)
					{
						string uom = item.UOM;
						if (isEmptyInventoryItem && string.IsNullOrEmpty(item.UOM))
						{
							uom = Setup.Current.EmptyItemUOM;
						}

						if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>() && existing.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
						{
							try
							{
								decimal inBase = IN.INUnitAttribute.ConvertToBase(Caches[typeof(PMQuote)], item.InventoryID, item.UOM, item.Qty.GetValueOrDefault(), IN.INPrecision.QUANTITY);
								existing.Qty += IN.INUnitAttribute.ConvertFromBase(Caches[typeof(PMQuote)], item.InventoryID, existing.UOM, inBase, IN.INPrecision.QUANTITY);
							}
							catch (IN.PXUnitConversionException)
							{
								existing.Qty = 0;
								existing.UOM = null;
							}
						}
						else
						{
						decimal convertedQty;
						if (IN.INUnitAttribute.TryConvertGlobalUnits(projectEntry, uom, existing.UOM, item.Qty.GetValueOrDefault(), IN.INPrecision.QUANTITY, out convertedQty))
						{
							existing.Qty += convertedQty;
						}
						else
					{
						existing.Qty = 0;
						existing.UOM = null;
					}
					}
					}
					else if (existing.UOM != null)
					{
						existing.Qty += item.Qty.GetValueOrDefault();
					}
					existing.CuryAmount += item.CuryAmount.GetValueOrDefault();
				}
				else
				{
					existing = new PMRevenueBudget();
					existing.ProjectID = key.ProjectID;
					existing.ProjectTaskID = key.ProjectTaskID;
					existing.AccountGroupID = key.AccountGroupID;
					existing.InventoryID = key.InventoryID;
					existing.CostCodeID = key.CostCodeID;
					existing.CuryAmount = item.CuryAmount.GetValueOrDefault();
					existing.TaxCategoryID = item.TaxCategoryID;
					existing.Description = item.Descr;

					if (isEmptyInventoryItem && string.IsNullOrEmpty(item.UOM))
					{
						existing.UOM = Setup.Current.EmptyItemUOM;
					}
					else
					{
						existing.UOM = item.UOM;
					}
					existing.Qty = item.Qty.GetValueOrDefault();

					budget.Add(key, existing);
				}
			}

			foreach (PMRevenueBudget item in budget.Values)
			{
				if (item.Qty.GetValueOrDefault() != 0)
					item.CuryUnitRate = item.CuryAmount / item.Qty.Value;

				PMRevenueBudget keys = new PMRevenueBudget() { ProjectID = item.ProjectID, ProjectTaskID = item.ProjectTaskID, AccountGroupID = item.AccountGroupID, InventoryID = item.InventoryID, CostCodeID = item.CostCodeID };
				var newLine = projectEntry.RevenueBudget.Insert(keys);
				if (newLine != null)
				{
					newLine.Qty = item.Qty;
					newLine.UOM = item.UOM;
					newLine.CuryAmount = item.CuryAmount;
                    if (item.CuryUnitRate != null)
                        newLine.CuryUnitRate = item.CuryUnitRate;
                    newLine.TaxCategoryID = item.TaxCategoryID;
					if (!aggregated.Contains(BudgetKeyTuple.Create(item)))
					{
						newLine.Description = item.Description;
					}
					else
					{
						PMAccountGroup accountGroup = PMAccountGroup.PK.Find(this, item.AccountGroupID);
						if (accountGroup != null)
						{
							newLine.Description = PXMessages.LocalizeNoPrefix(Messages.Aggregated) + accountGroup.Description;
						}
					}

					projectEntry.RevenueBudget.Update(newLine);
				}				
			}

		}
		
		
		private string GetItemTaskCD(CROpportunityProducts item)
		{
			string taskcd = item.TaskCD;
			if (string.IsNullOrEmpty(taskcd))
			{
				taskcd = GetDefaultTaskCD();
				if (string.IsNullOrEmpty(taskcd))
				{
					taskcd = DefaultTaskCD;
				}
			}

			return taskcd;
		}

		public virtual bool ValidateQuoteBeforeConvertToProject(PMQuote row, out int errorCount)
		{
			bool valid = true;

			if (string.IsNullOrEmpty(row.Subject))
			{
				valid = false;
				Quote.Cache.RaiseExceptionHandling<PMQuote.subject>(row, row.Subject, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMQuote.subject>(Quote.Cache)));
			}

			if (row.BAccountID == null)
			{
				valid = false;
				Quote.Cache.RaiseExceptionHandling<PMQuote.bAccountID>(row, row.BAccountID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMQuote.bAccountID>(Quote.Cache)));
			}
			else if(customer.Current == null)
			{
				valid = false;
				Quote.Cache.RaiseExceptionHandling<PMQuote.bAccountID>(row, baccount.Current?.AcctCD, new PXSetPropertyException(Messages.QuoteBAccountIsNotACustomer));
			}

			if (row.TemplateID == null)
			{
				valid = false;
				Quote.Cache.RaiseExceptionHandling<PMQuote.templateID>(row, row.TemplateID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMQuote.templateID>(Quote.Cache)));
			}

			if (string.IsNullOrWhiteSpace(row.QuoteProjectCD) && !ProjectAttribute.IsAutonumbered(this, ProjectAttribute.DimensionName))
			{
				valid = false;
				Quote.Cache.RaiseExceptionHandling<PMQuote.quoteProjectCD>(row, row.QuoteProjectCD, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PMQuote.quoteProjectCD>(Quote.Cache)));
				throw new PXException(ErrorMessages.RecordRaisedErrors, ErrorMessages.Updating, Quote.Cache.DisplayName);
			}

			bool taskIsRequired = Tasks.Select().Count > 1;

			errorCount = 0;
			foreach(CROpportunityProducts item in Products.Select())
			{
				if (taskIsRequired && string.IsNullOrEmpty(item.TaskCD))
				{
					errorCount++;
					valid = false;
					Products.Cache.RaiseExceptionHandling<CROpportunityProducts.taskCD>(item, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CROpportunityProducts.taskCD>(Products.Cache)));
				}

				if (item.CuryAmount.GetValueOrDefault() != 0m && item.RevenueAccountGroupID == null)
				{
					errorCount++;
					valid = false;
					Products.Cache.RaiseExceptionHandling<CROpportunityProducts.revenueAccountGroupID>(item, null, new PXSetPropertyException(Messages.MissingRevenueAccountGroup));
				}

				if (item.CuryExtCost.GetValueOrDefault() != 0m && item.ExpenseAccountGroupID == null)
				{
					errorCount++;
					valid = false;
					Products.Cache.RaiseExceptionHandling<CROpportunityProducts.expenseAccountGroupID>(item, null, new PXSetPropertyException(Messages.MissingExpenseAccountGroup));
				}

				if (GetTaskType(item) == CN.ProjectAccounting.PM.Descriptor.ProjectTaskType.Cost && item.RevenueAccountGroupID != null)
				{
					errorCount++;
					valid = false;
					Products.Cache.RaiseExceptionHandling<CROpportunityProducts.taskCD>(item, item.TaskCD, new PXSetPropertyException(Messages.TaskHasCostType));
				}

				if (GetTaskType(item) == CN.ProjectAccounting.PM.Descriptor.ProjectTaskType.Revenue && item.ExpenseAccountGroupID != null)
				{
					errorCount++;
					valid = false;
					Products.Cache.RaiseExceptionHandling<CROpportunityProducts.taskCD>(item, item.TaskCD, new PXSetPropertyException(Messages.TaskHasRevenueType));
				}
			}
			
			return valid;
		}

		private string GetTaskType(CROpportunityProducts line)
		{
			if (!string.IsNullOrEmpty(line.TaskCD))
			{
				PMQuoteTask task = PXSelect<PMQuoteTask, Where<PMQuoteTask.taskCD, Equal<Required<PMQuoteTask.taskCD>>, And <PMQuoteTask.quoteID, Equal<Required<PMQuoteTask.quoteID>>>>>.Select(this, line.TaskCD, line.QuoteID);
				return task.Type;
			}
			else
			{
				var resultset = Tasks.Select();

				if (resultset.Count == 0)
				{
					return CN.ProjectAccounting.PM.Descriptor.ProjectTaskType.CostRevenue;
				}

				if (resultset.Count == 1)
				{
					return ((PMQuoteTask)resultset).Type;
				}
			}

			return null;
		}

		public virtual string GetDefaultTaskCD()
		{
			var resultset = Tasks.Select();

			if (resultset.Count == 1)
			{
				return ((PMQuoteTask)resultset).TaskCD;
			}
			else
			{
				foreach(PMQuoteTask task in Tasks.Select() )
				{
					if (task.IsDefault == true)
					{
						return task.TaskCD;
					}
				}

				PMTask templateDefaultTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
					And<PMTask.isDefault, Equal<True>>>>.Select(this, Quote.Current.TemplateID);
				if (templateDefaultTask != null)
				{
					foreach (PMQuoteTask task in resultset)
					{
						if (task.TaskCD == templateDefaultTask.TaskCD)
						{
							return task.TaskCD;
						}
					}
				}
			}

			return null;
		}
				
		public virtual EmployeeCostEngine CreateEmployeeCostEngine()
		{
			return new EmployeeCostEngine(this);
		}

		#endregion

		#region Extensions
		public class MultiCurrency : MultiCurrencyGraph<PMQuoteMaint, PMQuote>
		{
			protected override string Module => GL.BatchModule.PM;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(PMQuote)) { DocumentDate = typeof(PMQuote.documentDate) };
			}

			protected override CurySourceMapping GetCurySourceMapping()
			{
				return new CurySourceMapping(typeof(Customer));
			}

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.Quote,
					Base.Products,
					Base.TaxLines,
					Base.Taxes
				};
			}

			protected override CurySource CurrentSourceSelect()
			{
				var result = new CurySource();

				if (PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() && Base.customer.Current != null)
				{
					result.CuryID = Base.customer.Current.CuryID;
					result.CuryRateTypeID = Base.customer.Current.CuryRateTypeID;
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() &&
					Base.QuoteCurrent.Current != null && Base.QuoteCurrent.Current.Status == CRQuoteStatusAttribute.Draft)
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
		}

		public class SalesPrice : SalesPriceGraph<PMQuoteMaint, PMQuote>
		{
			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(PMQuote)) { CuryInfoID = typeof(PMQuote.curyInfoID) };
			}
			protected override DetailMapping GetDetailMapping()
			{
				return new DetailMapping(typeof(CR.CROpportunityProducts)) { CuryLineAmount = typeof(CR.CROpportunityProducts.curyAmount), Descr = typeof(CR.CROpportunityProducts.descr) };
			}
			protected override PriceClassSourceMapping GetPriceClassSourceMapping()
			{
				return new PriceClassSourceMapping(typeof(CR.Location)) { PriceClassID = typeof(CR.Location.cPriceClassID) };
			}

			
		}

		public class PMDiscount : DiscountGraph<PMQuoteMaint, PMQuote>
		{
			public override void Initialize()
			{
				base.Initialize();
				if (this.Discounts == null)
					this.Discounts = new PXSelectExtension<Extensions.Discount.Discount>(this.DiscountDetails);

				Base.actionsFolder.AddMenuAction(graphRecalculateDiscountsAction);
			}
			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(PMQuote)) { CuryDiscTot = typeof(PMQuote.curyLineDocDiscountTotal) };
			}
			protected override DetailMapping GetDetailMapping()
			{
				return new DetailMapping(typeof(CR.CROpportunityProducts)) { CuryLineAmount = typeof(CR.CROpportunityProducts.curyAmount), Quantity = typeof(CR.CROpportunityProducts.quantity) };
			}
			protected override DiscountMapping GetDiscountMapping()
			{
				return new DiscountMapping(typeof(CR.CROpportunityDiscountDetail));
			}

			[PXCopyPasteHiddenView()]
			[PXViewName(CR.Messages.DiscountDetails)]
			public PXSelect<CR.CROpportunityDiscountDetail,
					Where<CR.CROpportunityDiscountDetail.quoteID, Equal<Current<PMQuote.quoteID>>>,
					OrderBy<Asc<CR.CROpportunityDiscountDetail.lineNbr>>>
				DiscountDetails;

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
			[CurrencyInfo(typeof(PMQuote.curyInfoID))]
			public override void Discount_CuryInfoID_CacheAttached(PXCache sender)
            {
            }

            protected virtual void Discount_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				if (Base.IsExternalTax(null))
				{
					Base.QuoteCurrent.Current.IsTaxValid = false;
				}
			}
			protected virtual void Discount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
			{
				if (Base.IsExternalTax(null))
				{
					Base.QuoteCurrent.Current.IsTaxValid = false;
				}
			}
			protected virtual void Discount_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
			{
				if (Base.IsExternalTax(null))
				{
					Base.QuoteCurrent.Current.IsTaxValid = false;
				}
			}

			protected override bool AddDocumentDiscount => true;

			protected override void DefaultDiscountAccountAndSubAccount(Extensions.Discount.Detail det)
			{
			}

			public override DiscountCalculationOptions DefaultCalculationOptions => DiscountEngine.DefaultARDiscountCalculationParameters | DiscountCalculationOptions.DisableGroupAndDocumentDiscounts;

			public PXAction<PMQuote> graphRecalculateDiscountsAction;
			[PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
			[PXButton(Category = Messages.Other)]
			public virtual IEnumerable GraphRecalculateDiscountsAction(PXAdapter adapter)
			{
				List<PMQuote> CRQoutes = new List<PMQuote>(adapter.Get()
					.Cast<PXResult<PMQuote, PMProject>>()
					.Select(r => { PMQuote q = r; return q; }));
				foreach (PMQuote quote in CRQoutes)
				{
					if (recalcdiscountsfilter.View.Answer == WebDialogResult.None)
					{
						recalcdiscountsfilter.Cache.Clear();
						RecalcDiscountsParamFilter filterdata = recalcdiscountsfilter.Cache.Insert() as RecalcDiscountsParamFilter;
						filterdata.RecalcUnitPrices = false;
						filterdata.RecalcDiscounts = false;
						filterdata.OverrideManualPrices = false;
						filterdata.OverrideManualDiscounts = false;
					}

					if (recalcdiscountsfilter.AskExt() != WebDialogResult.OK)
						return CRQoutes;

					RecalculateDiscountsAction(adapter);
				}
				return CRQoutes;
			}

			protected override void _(Events.RowDeleted<Extensions.Discount.Detail> e)
			{//turn off document and group discount recalculation
			}
		}

		public class SalesTax : TaxGraph<PMQuoteMaint, PMQuote>
		{
			protected override PXView DocumentDetailsView => Base.Products.View;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(PMQuote))
				{
					DocumentDate = typeof(PMQuote.documentDate),
					CuryDocBal = typeof(PMQuote.curyProductsAmount),
					CuryDiscountLineTotal = typeof(PMQuote.curyLineDiscountTotal),
					CuryDiscTot = typeof(PMQuote.curyLineDocDiscountTotal),
					TaxCalcMode = typeof(PMQuote.taxCalcMode)
				};
			}
			protected override DetailMapping GetDetailMapping()
			{
				return new DetailMapping(typeof(CR.CROpportunityProducts)) { CuryTranAmt = typeof(CR.CROpportunityProducts.curyAmount), CuryTranDiscount = typeof(CR.CROpportunityProducts.curyDiscAmt), CuryTranExtPrice = typeof(CR.CROpportunityProducts.curyExtPrice), DocumentDiscountRate = typeof(CR.CROpportunityProducts.documentDiscountRate), GroupDiscountRate = typeof(CR.CROpportunityProducts.groupDiscountRate) };
			}

			protected override TaxDetailMapping GetTaxDetailMapping()
			{
				return new TaxDetailMapping(typeof(CR.CROpportunityTax), typeof(CR.CROpportunityTax.taxID));
			}
			protected override TaxTotalMapping GetTaxTotalMapping()
			{
				return new TaxTotalMapping(typeof(CRTaxTran), typeof(CRTaxTran.taxID));
			}

			protected virtual void _(Events.FieldUpdated<PMQuote, PMQuote.curyAmount> e)
			{
				if (e.Row != null && e.Row.ManualTotalEntry == true)
					e.Row.CuryProductsAmount = e.Row.CuryAmount - e.Row.CuryDiscTot;
			}

			protected virtual void _(Events.FieldUpdated<PMQuote, PMQuote.curyDiscTot> e)
			{
				if (e.Row != null && e.Row.ManualTotalEntry == true)
					e.Row.CuryProductsAmount = e.Row.CuryAmount - e.Row.CuryDiscTot;
			}

			protected virtual void _(Events.FieldUpdated<PMQuote, PMQuote.manualTotalEntry> e)
			{
				if (e.Row != null && e.Row.ManualTotalEntry == false)
				{
					CalcTotals(null, false);
				}
			}

			protected virtual void Document_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				var row = sender.GetExtension<Extensions.SalesTax.Document>(e.Row);
				if (row != null && row.TaxCalc == null)
					row.TaxCalc = TaxCalc.Calc;
			}

			protected override void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
			{
				base.CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);

				PMQuote doc = (PMQuote)this.Documents.Cache.GetMain<Extensions.SalesTax.Document>(this.Documents.Current);
				bool manualEntry = doc.ManualTotalEntry == true;
				decimal CuryManualAmount = (decimal)(doc.CuryAmount ?? 0m);
				decimal CuryManualDiscTot = (decimal)(doc.CuryDiscTot ?? 0m);
				decimal CuryLineTotal = (decimal)(ParentGetValue<CR.CROpportunity.curyLineTotal>() ?? 0m);
				decimal CuryDiscAmtTotal = (decimal)(ParentGetValue<CR.CROpportunity.curyLineDiscountTotal>() ?? 0m);
				decimal CuryExtPriceTotal = (decimal)(ParentGetValue<CR.CROpportunity.curyExtPriceTotal>() ?? 0m);
				decimal CuryDiscTotal = (decimal)(ParentGetValue<CR.CROpportunity.curyLineDocDiscountTotal>() ?? 0m);

				ParentSetValue<PMQuote.curyTaxInclTotal>(CuryInclTaxTotal);

				decimal CuryDocTotal =
					manualEntry
						? CuryManualAmount - CuryManualDiscTot
						: CuryLineTotal - CuryDiscTotal + CuryTaxTotal - CuryInclTaxTotal;

				if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<PMQuote.curyProductsAmount>() ?? 0m)) == false)
				{
					ParentSetValue<PMQuote.curyProductsAmount>(CuryDocTotal);
				}
			}

			protected override string GetExtCostLabel(PXCache sender, object row)
			{
				return ((PXDecimalState)sender.GetValueExt<CROpportunityProducts.curyExtPrice>(row)).DisplayName;
			}

			protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
			{
				IComparer<Tax> taxComparer = GetTaxByCalculationLevelComparer();
				taxComparer.ThrowOnNull(nameof(taxComparer));

				Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
				var currents = new[]
				{
					row != null && row is Extensions.SalesTax.Detail ? Details.Cache.GetMain((Extensions.SalesTax.Detail)row):null,
					((PMQuoteMaint)graph).Quote.Current
				};

				foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
						LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
							And<TaxRev.outdated, Equal<boolFalse>,
								And<TaxRev.taxType, Equal<TaxType.sales>,
									And<Tax.taxType, NotEqual<CSTaxType.withholding>,
										And<Tax.taxType, NotEqual<CSTaxType.use>,
											And<Tax.reverseTax, Equal<boolFalse>,
												And<Current<PMQuote.documentDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
						Where>
					.SelectMultiBound(graph, currents, parameters))
				{
					tail[((Tax)record).TaxID] = record;
				}
				List<object> ret = new List<object>();
				switch (taxchk)
				{
					case PXTaxCheck.Line:
						foreach (CR.CROpportunityTax record in PXSelect<CR.CROpportunityTax,
								Where<CR.CROpportunityTax.quoteID, Equal<Current<PMQuote.quoteID>>,
										And<CR.CROpportunityTax.quoteID, Equal<Current<CR.CROpportunityProducts.quoteID>>,
											And<CR.CROpportunityTax.lineNbr, Equal<Current<CR.CROpportunityProducts.lineNbr>>>>>>
							.SelectMultiBound(graph, currents))
						{
							if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
							{
								int idx;
								for (idx = ret.Count;
									(idx > 0) && taxComparer.Compare((PXResult<CR.CROpportunityTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
									idx--) ;

								Tax adjdTax = AdjustTaxLevel((Tax)line);
								ret.Insert(idx, new PXResult<CR.CROpportunityTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
							}
						}

						return ret;

					case PXTaxCheck.RecalcLine:
						foreach (CR.CROpportunityTax record in PXSelect<CR.CROpportunityTax,
								Where<CR.CROpportunityTax.quoteID, Equal<Current<PMQuote.quoteID>>,
										And<CR.CROpportunityTax.lineNbr, Less<intMax>>>>
							.SelectMultiBound(graph, currents))
						{
							if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
							{
								int idx;
								for (idx = ret.Count;
									(idx > 0)
									&& ((CR.CROpportunityTax)(PXResult<CR.CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
									&& taxComparer.Compare((PXResult<CR.CROpportunityTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
									idx--) ;

								Tax adjdTax = AdjustTaxLevel((Tax)line);
								ret.Insert(idx, new PXResult<CR.CROpportunityTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
							}
						}
						return ret;
					case PXTaxCheck.RecalcTotals:
						foreach (CRTaxTran record in PXSelect<CRTaxTran,
								Where<CR.CRTaxTran.quoteID, Equal<Current<PMQuote.quoteID>>>,
								OrderBy<Asc<CRTaxTran.lineNbr, Asc<CRTaxTran.taxID>>>>
							.SelectMultiBound(graph, currents))
						{
							if (record.TaxID != null && tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
							{
								int idx;
								for (idx = ret.Count;
									(idx > 0) && taxComparer.Compare((PXResult<CRTaxTran, Tax, TaxRev>)ret[idx - 1], line) > 0;
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

			protected override List<object> SelectDocumentLines(PXGraph graph, object row)
			{
				var result = SelectFrom<CROpportunityProducts>
					.Where<CROpportunityProducts.quoteID.IsEqual<PMQuote.quoteID.FromCurrent>>.View
					.Select(Base)
					.RowCast<CROpportunityProducts>()
					.Select(_ => (object)_)
					.ToList();

				return result;
			}

			#region CRTaxTran
			protected virtual void CRTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				if (e.Row == null)
					return;

				PXUIFieldAttribute.SetEnabled<CRTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
			}
			#endregion

			#region CROpportunityTax

			protected virtual void CROpportunityTax_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				CROpportunityTax row = e.Row as CROpportunityTax;
				if (row == null) return;
			}


			protected virtual void CRTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				CRTaxTran row = e.Row as CRTaxTran;
				if (row == null) return;

				if (e.Operation == PXDBOperation.Delete)
				{
					CROpportunityTax tax = (CR.CROpportunityTax)Base.TaxLines.Cache.Locate(FindCROpportunityTax(row));
					if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Deleted ||
						Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.InsertedDeleted)
						e.Cancel = true;
				}
				if (e.Operation == PXDBOperation.Update)
				{
					CROpportunityTax tax = (CR.CROpportunityTax)Base.TaxLines.Cache.Locate(FindCROpportunityTax(row));
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

		public class ContactAddress : CROpportunityContactAddressExt<PMQuoteMaint>
		{
			#region Override

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<CR.Extensions.CROpportunityContactAddress.DocumentAddress>(Base.Quote_Address);
				Contacts = new PXSelectExtension<CR.Extensions.CROpportunityContactAddress.DocumentContact>(Base.Quote_Contact);
			}
			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(PMQuote))
				{
					DocumentAddressID = typeof(PMQuote.opportunityAddressID),
					DocumentContactID = typeof(PMQuote.opportunityContactID),
					ShipAddressID = typeof(PMQuote.shipAddressID),
					ShipContactID = typeof(PMQuote.shipContactID),
					AllowOverrideContactAddress = typeof(PMQuote.allowOverrideContactAddress),
					AllowOverrideShippingContactAddress = typeof(PMQuote.allowOverrideShippingContactAddress),
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

			protected override IPersonalContact SelectContact()
			{
				return Base.Quote_Contact.SelectSingle();
			}

			protected override IPersonalContact SelectShippingContact()
			{
				return Base.Shipping_Contact.SelectSingle();
			}

			protected override IPersonalContact GetEtalonContact()
			{
				return SafeGetEtalon(Base.Quote_Contact.Cache) as IPersonalContact;
			}

			protected override IPersonalContact GetEtalonShippingContact()
			{
				return SafeGetEtalon(Base.Shipping_Contact.Cache) as IPersonalContact;
			}

			protected override IAddress SelectAddress()
			{
				return Base.Quote_Address.SelectSingle();
			}

			protected override IAddress SelectShippingAddress()
			{
				return Base.Shipping_Address.SelectSingle();
			}

			protected override IAddress GetEtalonAddress()
			{
				return SafeGetEtalon(Base.Quote_Address.Cache) as IAddress;
			}

			protected override IAddress GetEtalonShippingAddress()
			{
				return SafeGetEtalon(Base.Shipping_Address.Cache) as IAddress;
			}

			protected override string GetMessageForDefaultRecords(
				bool needAskFromContactAddress,
				bool needAskFromBillingContactAddress,
				bool needAskFromShippingContactAddress)
			{
				if (needAskFromContactAddress)
					if (needAskFromShippingContactAddress)
						return CR.Messages.ReplaceContactDetailsAndShippingInfo_PM;
					else
						return CR.Messages.ReplaceContactDetails_PM;
				else if (needAskFromShippingContactAddress)
					return CR.Messages.ReplaceShippingInfo_PM;

				throw new NotSupportedException();
			}

			#endregion

			#region Events

			protected override void _(Events.FieldUpdated<CR.Extensions.CROpportunityContactAddress.Document, CR.Extensions.CROpportunityContactAddress.Document.contactID> e)
			{
				if (e.Row == null)
				{
					base._(e);
					return;
				}

				BAccount bAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<PMQuote.bAccountID>>>>.SelectSingleBound(Base, null);
				if (bAccount != null)
				{
					Base.Quote.Cache.SetDefaultExt<PMQuote.locationID>(e.Row.Base);
				}
				else
				{
					object locationID, taxZoneID;
					Base.QuoteCurrent.Cache.RaiseFieldDefaulting<PMQuote.locationID>(e.Row.Base, out locationID);
					Base.QuoteCurrent.Cache.RaiseFieldDefaulting<PMQuote.taxZoneID>(e.Row.Base, out taxZoneID);
				}

				e.Cache.SetDefaultExt<PMQuote.locationID>(e.Row);
				e.Cache.SetDefaultExt<PMQuote.taxZoneID>(e.Row);

				base._(e);
				var allowOverrideContactAddress = (e.Row.AllowOverrideContactAddress == true) || (e.Row.BAccountID == null && e.Row.ContactID == null);
				Base.QuoteCurrent.Cache.SetValueExt<PMQuote.allowOverrideContactAddress>(e.Row.Base, allowOverrideContactAddress);
			}

			protected override void _(Events.FieldUpdated<CR.Extensions.CROpportunityContactAddress.Document, CR.Extensions.CROpportunityContactAddress.Document.bAccountID> e)
			{
				if (e.Row == null)
				{
					base._(e);
					return;
				}

				e.Cache.SetDefaultExt<PMQuote.locationID>(e.Row);
				e.Cache.SetDefaultExt<PMQuote.taxZoneID>(e.Row);

				base._(e);
			}

			#endregion
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class PMQuoteMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<PMQuoteMaint_ActivityDetailsExt, PMQuoteMaint, PMQuote, PMQuote.noteID> { }

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class PMQuoteMaint_ActivityDetailsExt : ActivityDetailsExt<PMQuoteMaint, PMQuote, PMQuote.noteID>
		{
			public override Type GetBAccountIDCommand() => typeof(PMQuote.bAccountID);
			public override Type GetContactIDCommand() => typeof(PMQuote.contactID);

			public override Type GetEmailMessageTarget() => typeof(Select<CRContact, Where<CRContact.contactID, Equal<Current<PMQuote.opportunityContactID>>>>);

			public override string GetCustomMailTo()
			{
				var current = Base.Quote.Current;
				if (current == null)
					return null;

				var contact = current.OpportunityContactID.With(_ => CRContact.PK.Find(Base, _.Value));

				return
					!string.IsNullOrWhiteSpace(contact?.Email)
						? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.Email, contact.DisplayName)
						: null;
			}
		}

		public class ExtensionSorting : Module
		{
			protected override void Load(ContainerBuilder builder) => builder.RunOnApplicationStart(() =>
				PXBuildManager.SortExtensions += list => PXBuildManager.PartialSort(list, _order)
				);

			private static readonly Dictionary<Type, int> _order = new Dictionary<Type, int>
			{
				{typeof(ContactAddress), 1},
				{typeof(MultiCurrency), 2},
				{typeof(SalesPrice), 3},
				{typeof(Discount), 4},
				{typeof(SalesTax), 5},
			};
		}
		#endregion


		#region Address Lookup Extension
		/// <exclude/>
		public class PMQuoteMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<PMQuoteMaint, PMQuote, CRAddress>
		{
			protected override string AddressView => nameof(Base.Quote_Address);
			protected override string ViewOnMap => nameof(Base.viewMainOnMap);
		}

		/// <exclude/>
		public class PMQuoteMaintShippingAddressLookupExtension : CR.Extensions.AddressLookupExtension<PMQuoteMaint, PMQuote, CRShippingAddress>
		{
			protected override string AddressView => nameof(Base.Shipping_Address);
			protected override string ViewOnMap => nameof(Base.viewShippingOnMap);
		}
		#endregion
	}

	public class PMQuoteProjectCDDimensionAttribute : PXDimensionAttribute
	{
		public PMQuoteProjectCDDimensionAttribute() : base(ProjectAttribute.DimensionName)
		{

		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			try
			{
				base.FieldVerifying(sender, e);
			}
			catch (PXSetPropertyException ex)
			{
				string quoteProjectCD = e.NewValue as string;

				CheckProjectCD(sender.Graph, quoteProjectCD, ex.Message, true);

				throw ex;
			}
		}

		public static void CheckProjectCD(PXGraph graph, string quoteProjectCD, string errorMessage, bool forAttribute)
		{
			PX.Objects.CS.Segment[] segments = PXSelect<PX.Objects.CS.Segment,
										Where<PX.Objects.CS.Segment.dimensionID, Equal<Required<PX.Objects.CS.Segment.dimensionID>>>>
										.Select(graph, ProjectAttribute.DimensionName)
										.RowCast<PX.Objects.CS.Segment>()
										.ToArray();

			short previousSegmentsLength = 0;
			for (int i = 0; i <= segments.Length - 1; i++)
			{
				PX.Objects.CS.Segment segment = segments[i];

				if (segment.AutoNumber == false && segment.Validate == true)
				{
					bool emptySegmentValue = false;
					if (string.IsNullOrWhiteSpace(quoteProjectCD))
					{
						emptySegmentValue = true;
					}
					else if (quoteProjectCD.Length < previousSegmentsLength + segment.Length.Value)
					{
						emptySegmentValue = true;
					}
					else
					{
						string currentSegmentValue = quoteProjectCD.Substring(previousSegmentsLength, segment.Length.Value);
						if (string.IsNullOrWhiteSpace(currentSegmentValue))
						{
							emptySegmentValue = true;
						}
					}

					if (emptySegmentValue == true)
					{
						if (forAttribute)
						{
							throw new PXSetPropertyException(Messages.ProjectQuoteEmptySegmentOfProjectCD, segment.SegmentID, segment.Descr);
						}
						else
						{
							throw new PXException(Messages.ProjectQuoteEmptySegmentOfProjectCD, segment.SegmentID, segment.Descr);
						}
					}
				}

				previousSegmentsLength += segment.Length.Value;
			}

			previousSegmentsLength = 0;
			foreach (PX.Objects.CS.Segment segment in segments)
			{
				if (segment.AutoNumber == false && segment.Validate == true)
				{
					string currentSegmentValue = quoteProjectCD.Substring(previousSegmentsLength, segment.Length.Value);

					string segmentDescr = String.IsNullOrEmpty(segment.Descr) ? segment.SegmentID.ToString() : segment.Descr;
					if (errorMessage.Contains(segmentDescr))
					{
						if (forAttribute)
						{
							throw new PXSetPropertyException(Messages.ProjectQuoteIncorrectProjectCD, currentSegmentValue, segment.SegmentID, segment.Descr, ProjectAttribute.DimensionName);
						}
						else
						{
							throw new PXException(Messages.ProjectQuoteIncorrectProjectCD, currentSegmentValue, segment.SegmentID, segment.Descr, ProjectAttribute.DimensionName);
						}
					}
				}

				previousSegmentsLength += segment.Length.Value;
			}
		}
	}

	public class ProductLinesSelect : PXOrderedSelect<PMQuote, CROpportunityProducts, Where<CR.CROpportunityProducts.quoteID, Equal<Current<PMQuote.quoteID>>>,
			OrderBy<Asc<CR.CROpportunityProducts.sortOrder>>>
	{
		public ProductLinesSelect(PXGraph graph) : base(graph) { }
		public ProductLinesSelect(PXGraph graph, Delegate handler) : base(graph, handler) { }
	
	}

	public class PMQuoteApproval
		: EPApprovalActionExtensionPersistent<PMQuote, PMQuote.approved, PMQuote.rejected, PMSetup.quoteApprovalMapID, PMSetup.quoteApprovalNotificationID>
	{
		[PXUIField(DisplayName = PX.Objects.EP.Messages.Submit)]
		[PXButton(Category = Messages.Processing, DisplayOnMainToolbar = true)]
		public override IEnumerable Submit(PXAdapter adapter)
		{
			foreach (var item in adapter.Get<PMQuote>())
			{
				var cache = _Graph.Caches[typeof(PMQuote)];
				cache.SetValue<PMQuote.submitCancelled>(item, false);

				if (item.TemplateID == null)
				{
					cache.RaiseExceptionHandling<PMQuote.templateID>(item, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{typeof(PMQuote.templateID).Name}]"));
					cache.SetValue<PMQuote.submitCancelled>(item, true);
					yield return item;
				}
				if (string.IsNullOrWhiteSpace(item.QuoteProjectCD) && DimensionMaint.IsAutonumbered(_Graph, ProjectAttribute.DimensionName, false))
				{
					cache.RaiseExceptionHandling<PMQuote.quoteProjectCD>(item, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{typeof(PMQuote.quoteProjectCD).Name}]"));
					cache.SetValue<PMQuote.submitCancelled>(item, true);
					yield return item;
				}
				else if (item.BAccountID == null)
				{
					cache.RaiseExceptionHandling<PMQuote.bAccountID>(item, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{typeof(PMQuote.bAccountID).Name}]"));
					cache.SetValue<PMQuote.submitCancelled>(item, true);
					yield return item;
				}

				DoSubmit(item);

				StatusHandler?.Invoke(item);

				_Graph.Caches[typeof(PMQuote)].Update(item);

				if (Persistent)
					_Graph.Persist();

				yield return item;
			}
		}

		#region Ctor
		public PMQuoteApproval(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
		}

		public PMQuoteApproval(PXGraph graph)
			: base(graph)
		{
		}
		#endregion
	}

	[Obsolete]
	public class EPApprovalActionExtensionPersistentRequired<SourceAssign, Approved, Rejected, ApprovalMapID, ApprovalNotificationID, SubmitCancelled, RequiredField1, RequiredField2>
		: EPApprovalActionExtensionPersistent<SourceAssign, Approved, Rejected, ApprovalMapID, ApprovalNotificationID>
		where SourceAssign : class, PX.Data.EP.IAssign, IBqlTable, new()
		where Approved : class, IBqlField
		where Rejected : class, IBqlField
		where ApprovalMapID : class, IBqlField
		where ApprovalNotificationID : class, IBqlField
		where SubmitCancelled : class, IBqlField
		where RequiredField1 : class, IBqlField
		where RequiredField2 : class, IBqlField
	{
		[PXUIField(DisplayName = PX.Objects.EP.Messages.Submit)]
		[PXButton(Category = Messages.Processing, DisplayOnMainToolbar = true)]
		public override IEnumerable Submit(PXAdapter adapter)
		{
			foreach (var item in adapter.Get<SourceAssign>())
			{
				var cache = _Graph.Caches[typeof(SourceAssign)];
				cache.SetValue<SubmitCancelled>(item, false);
				var requiredData1 = cache.GetValue<RequiredField1>(item);
				var requiredData2 = cache.GetValue<RequiredField2>(item);
				if (requiredData1 == null)
				{
					cache.RaiseExceptionHandling<RequiredField1>(item, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{typeof(RequiredField1).Name}]"));
					cache.SetValue<SubmitCancelled>(item, true);
					yield return item;
				}
				else if (requiredData2 == null && DimensionMaint.IsAutonumbered(Cache.Graph, ProjectAttribute.DimensionName))
				{
					cache.RaiseExceptionHandling<RequiredField2>(item, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{typeof(RequiredField2).Name}]"));
					cache.SetValue<SubmitCancelled>(item, true);
					yield return item;
				}

				DoSubmit(item);

				StatusHandler?.Invoke(item);

				_Graph.Caches[typeof(SourceAssign)].Update(item);

				if (Persistent)
					_Graph.Persist();

				yield return item;
			}
		}

		#region Ctor
		public EPApprovalActionExtensionPersistentRequired(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
		}

		public EPApprovalActionExtensionPersistentRequired(PXGraph graph)
			: base(graph)
		{
		}
		#endregion
	}
}

