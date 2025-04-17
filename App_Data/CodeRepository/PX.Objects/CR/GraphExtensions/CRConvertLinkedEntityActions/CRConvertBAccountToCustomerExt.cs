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
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.Objects.CS;

namespace PX.Objects.CR.Extensions.CRConvertLinkedEntityActions
{
	public abstract class CRConvertBAccountToCustomerExt<TGraph, TMain> : CRCreateActionBase<TGraph, TMain, CustomerMaint, Customer, CustomerFilter, CustomerConversionOptions>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region ctor

		[InjectDependency]
		internal IPXPageIndexingService PageService { get; private set; }

		#endregion

		#region Views

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<CustomerFilter> CustomerInfo;
		protected override CRValidationFilter<CustomerFilter> FilterInfo => CustomerInfo;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupAttributes> CustomerInfoAttributes;
		protected virtual IEnumerable customerInfoAttributes()
		{
			return GetFilledAttributes();
		}

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupUDFAttributes> CustomerInfoUDF;
		protected virtual IEnumerable<PopupUDFAttributes> customerInfoUDF()
		{
			return GetRequiredUDFFields();
		}

		protected override ICRValidationFilter[] AdditionalFilters =>
			new ICRValidationFilter[] {CustomerInfoAttributes, CustomerInfoUDF};

		#endregion

		#region Actions

		public PXAction<TMain> CreateCustomerInPanel;
		[PXUIField(DisplayName = "Create Customer")]
		[PXButton(DisplayOnMainToolbar = false)]
		protected virtual IEnumerable createCustomerInPanel(PXAdapter adapter)
		{
			var result = TryConvert();
			CustomerInfo.ClearAnswers(clearCurrent: true);
			if (result.Graph == null)
				return adapter.Get();

			throw new PXRedirectRequiredException(result.Graph, "Edit Customer")
			{
				Mode = PXBaseRedirectException.WindowMode.New
			};
		}

		#endregion

		#region Methods

		protected override CustomerMaint CreateTargetGraph()
		{
			var baccountGraph = PXGraph.CreateInstance<BusinessAccountMaint>();
			baccountGraph.BAccount.Current = BAccount.PK.Find(Base, Documents.Current?.BAccountID);
			var ext = baccountGraph.GetExtension<BusinessAccountMaint.ExtendToCustomer>();
			if(ext == null)
				throw new PXException(MessagesNoPrefix.CannotConvertBusinessAccountToCustomer);
			return ext.Extend();
		}

		protected override Customer CreateMaster(CustomerMaint graph, CustomerConversionOptions options)
		{
			var customer = graph.BAccount.Current;
			customer.CustomerClassID = FilterInfo.Current.ClassID;

			// class change
			graph.BAccount.View.Answer = WebDialogResult.Yes;
			graph.BAccount.UpdateCurrent();

			var contact = graph.GetProcessingExtension<CustomerMaint.DefContactAddressExt>().DefContact.SelectSingle();
			contact.EMail = FilterInfo.Current.Email;

			graph.ContactDummy.Update(contact);

			FillAttributes(graph.Answers, customer);

			FillUDF(CustomerInfoUDF.Cache, GetMainCurrent(), graph.BAccount.Cache, customer, customer.ClassID);

			return customer;
		}

		public virtual void TryConvertInPanel(CustomerConversionOptions options = null)
		{
			var convertResult = TryConvert(options);
			if (convertResult.Exception != null)
			{
				CustomerInfo.Current.WarningMessage = PXMessages.LocalizeNoPrefix(
					MessagesNoPrefix.CannotCreateCustomerFromCreateSalesOrder);
				CustomerInfo.UpdateCurrent();
			}
		}

		public override ConversionResult<Customer> TryConvert(CustomerConversionOptions options = null)
		{
			if (Customer.PK.Find(Base, Documents.Current?.BAccountID) is { } customer)
				return new ConversionResult<Customer>
				{
					Converted = false,
					Entity    = customer,
				};
			return base.TryConvert(options);
		}

		public virtual bool CanConvert()
		{
			return Customer.PK.Find(Base, Documents.Current?.BAccountID) == null;
		}

		public virtual bool HasAccessToCreateCustomer()
		{
			var graphType = typeof(CustomerMaint);
			var node = PXSiteMap.Provider.FindSiteMapNode(graphType);
			if (node == null) return false;

			var primaryViewName = PageService.GetPrimaryView(graphType.FullName);
			var cacheInfo = GraphHelper.GetGraphView(graphType, primaryViewName).Cache;
			

			PXAccess.Provider.GetRights(node.ScreenID, graphType.Name, cacheInfo.CacheType,
			                            out var rights, out var _, out var _);

			return rights >= PXCacheRights.Insert;
		}

		#endregion

		#region Events
		protected virtual void _(Events.RowSelected<CustomerFilter> e)
		{
			bool canConvert = CanConvert();
			bool warningVisible = canConvert && e.Row?.WarningMessage != null;
			bool emailRequired = canConvert
				&& e.Row?.ClassID != null
				&& CustomerClass.PK.Find(Base, e.Row.ClassID) is {} customerClass
				&& (customerClass.SendStatementByEmail is true
					|| customerClass.MailInvoices is true
					|| (customerClass.MailDunningLetters is true
						&& PXAccess.FeatureInstalled<FeaturesSet.dunningLetter>()));

			e.Cache.AdjustUI(e.Row)
				.ForAllFields(ui => ui.Visible = canConvert)
				.For<CustomerFilter.warningMessage>(ui => ui.Visible = warningVisible)
				.For<CustomerFilter.classID>(ui => ui.Enabled = !warningVisible)
				.For<CustomerFilter.email>(ui => ui.Enabled = !warningVisible);

			e.Cache.Adjust<PXDefaultAttribute>(e.Row)
				.For<CustomerFilter.email>(d =>
					d.PersistingCheck = emailRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			CreateCustomerInPanel.SetVisible(warningVisible);
		}


		protected virtual void _(Events.FieldDefaulting<CustomerFilter, CustomerFilter.acctCD> e)
		{
			e.NewValue = BAccount.PK.Find(Base, Documents.Current?.BAccountID)?.AcctCD;
		}

		protected virtual void _(Events.FieldDefaulting<CustomerFilter, CustomerFilter.email> e)
		{
			e.NewValue = BAccount.FK.ContactInfo.FindParent(Base, BAccount.PK.Find(Base, Documents.Current?.BAccountID))?.EMail;
		}

		#endregion

	}
}
