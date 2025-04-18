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
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.TaxProvider;

namespace PX.Objects.PM
{
	public class ProformaEntryExternalTax : ExternalTax<ProformaEntry, PMProforma>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		public override PMProforma CalculateExternalTax(PMProforma doc)
		{
			if (IsExternalTax(doc.TaxZoneID))
				return CalculateExternalTax(doc, false);

			return doc;
		}

		public virtual PMProforma CalculateExternalTax(PMProforma doc, bool forceRecalculate)
		{
			var toAddress = GetToAddress(doc);

			GetTaxRequest getRequest = null;
			bool isValidByDefault = true;
			if ((doc.IsTaxValid != true || forceRecalculate) && !IsNonTaxable(toAddress))
			{
				getRequest = BuildGetTaxRequest(doc);

				if (getRequest.CartItems.Count > 0)
				{
					isValidByDefault = false;
				}
				else
				{
					getRequest = null;
				}
			}

			if (isValidByDefault)
			{
				doc.CuryTaxTotal = 0;
				doc.IsTaxValid = true;
				Base.Document.Update(doc);

				foreach (PMTaxTran item in Base.Taxes.Select())
				{
					Base.Taxes.Delete(item);
				}

				using (var ts = new PXTransactionScope())
				{
					Base.Persist(typeof(PMTaxTran), PXDBOperation.Delete);
					Base.Persist(typeof(PMProforma), PXDBOperation.Update);
					PXTimeStampScope.PutPersisted(Base.Document.Cache, doc, PXDatabase.SelectTimeStamp());
					ts.Complete();
				}
				return doc;
			}

			GetTaxResult result = null;
			var service = TaxProviderFactory(Base, doc.TaxZoneID);
			bool getTaxFailed = false;
			if (getRequest != null)
			{
				result = service.GetTax(getRequest);
				if (!result.IsSuccess)
				{
					getTaxFailed = true;
				}
			}

			if (!getTaxFailed)
			{
				try
				{
					ApplyTax(doc, result);
					using (var ts = new PXTransactionScope())
					{
						doc.IsTaxValid = true;
						Base.Document.Update(doc);
						Base.Persist(typeof(PMProforma), PXDBOperation.Update);
						PXTimeStampScope.PutPersisted(Base.Document.Cache, doc, PXDatabase.SelectTimeStamp());
						ts.Complete();
					}
				}
				catch (PXOuterException ex)
				{
					string msg = TX.Messages.FailedToApplyTaxes;
					foreach (string err in ex.InnerMessages)
					{
						msg += Environment.NewLine + err;
					}

					throw new PXException(ex, msg);
				}
				catch (Exception ex)
				{
					string msg = TX.Messages.FailedToApplyTaxes;
					msg += Environment.NewLine + ex.Message;

					throw new PXException(ex, msg);
				}
			}
			else
			{
				LogMessages(result);

				throw new PXException(TX.Messages.FailedToGetTaxes);
			}

			return doc;
		}

		public virtual void RecalculateExternalTaxes()
		{
			if (Base.Document.Current != null && IsExternalTax(Base.Document.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && Base.Document.Current.IsTaxValid != true)
			{
				if (Base.RecalculateExternalTaxesSync)
				{
					CalculateExternalTax(Base.Document.Current);
				}
				else
				{
					PXLongOperation.StartOperation(Base, delegate ()
					{
						PMProforma doc = new PMProforma();
						doc.RefNbr = Base.Document.Current.RefNbr;
						doc.RevisionID = Base.Document.Current.RevisionID;
						PMExternalTaxCalc.Process(doc);

					});
				}
			}
		}

		[PXOverride]
		public virtual void Persist(Action basePersist)
		{
			if (Base.RecalculateExternalTaxesSync)
			{
				RecalculateExternalTaxes();
				basePersist();
			}
			else
			{
				basePersist();
				RecalculateExternalTaxes();
			}
		}

		public PXAction<PMProforma> recalcExternalTax;
		[PXUIField(DisplayName = "Recalculate External Tax", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable RecalcExternalTax(PXAdapter adapter)
		{
			if (Base.Document.Current != null && IsExternalTax(Base.Document.Current.TaxZoneID))
			{
				var proforma = Base.Document.Current;
				CalculateExternalTax(Base.Document.Current, true);

				Base.Clear(PXClearOption.ClearAll);
				Base.Document.Current = Base.Document.Search<PMProforma.refNbr>(proforma.RefNbr);

				yield return Base.Document.Current;
			}
			else
			{
				foreach (var res in adapter.Get())
				{
					yield return res;
				}
			}
		}

		protected virtual void _(Events.RowUpdated<PMProforma> e)
		{
			//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
			if (IsExternalTax(e.Row.TaxZoneID))
			{
				if (!e.Cache.ObjectsEqual<
					PMProforma.invoiceDate,
					PMProforma.taxZoneID,
					PMProforma.customerID,
					PMProforma.locationID,
					PMProforma.externalTaxExemptionNumber,
					PMProforma.avalaraCustomerUsageType,
					PMProforma.shipAddressID,
					PMProforma.branchID> (e.Row, e.OldRow))
				{
					e.Row.IsTaxValid = false;
				}
			}
		}

		protected virtual void _(Events.RowSelected<PMProforma> e)
		{
			if (Base.Document.Current == null)
				return;

			bool isEditable = Base.CanEditDocument(e.Row);
			bool isExternalTax = IsExternalTax(Base.Document.Current.TaxZoneID);

			Base.Taxes.Cache.AllowInsert = isEditable && !isExternalTax && e.Row.Hold == true;
			Base.Taxes.Cache.AllowUpdate = isEditable && !isExternalTax && e.Row.Hold == true;
			Base.Taxes.Cache.AllowDelete = isEditable && !isExternalTax && e.Row.Hold == true;

			if (isExternalTax && e.Row.IsTaxValid != true)
			{
				PXUIFieldAttribute.SetWarning<PMProforma.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
			}
		}

		protected virtual void _(Events.RowInserted<PMProformaTransactLine> e)
		{
			InvalidateTax(e.Row, (PMProformaTransactLine)e.Cache.CreateInstance());
		}

		protected virtual void _(Events.RowUpdated<PMProformaTransactLine> e)
		{
			InvalidateTax(e.Row, e.OldRow);
		}

		protected virtual void _(Events.RowInserted<PMProformaProgressLine> e)
		{
			InvalidateTax(e.Row, (PMProformaProgressLine)e.Cache.CreateInstance());
		}

		protected virtual void _(Events.RowUpdated<PMProformaProgressLine> e)
		{
			InvalidateTax(e.Row, e.OldRow);
		}

		public virtual void InvalidateTax(PMProformaLine row, PMProformaLine oldRow)
		{
			if (Base.Document.Current != null)
			{
				if (row.AccountID != oldRow.AccountID ||
				    row.InventoryID != oldRow.InventoryID ||
				    row.LineTotal != oldRow.LineTotal ||
				    row.TaxCategoryID != oldRow.TaxCategoryID ||
				    row.Description != oldRow.Description)
				{
					InvalidateExternalTax(Base.Document.Current);
				}
			}
		}

		#region PMShippingAddress Events

		protected virtual void _(Events.RowUpdated<PMShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null
				&& e.Cache.ObjectsEqual<PMShippingAddress.postalCode, PMShippingAddress.countryID, PMShippingAddress.state>(e.Row, e.OldRow) == false)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowInserted<PMShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<PMShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.FieldUpdating<PMShippingAddress, PMShippingAddress.overrideAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		#endregion

		private void InvalidateExternalTax(PMProforma doc)
		{
			if (IsExternalTax(doc.TaxZoneID))
			{
				doc.IsTaxValid = false;
				Base.Document.Cache.MarkUpdated(doc, assertError: true);
			}
		}

		public virtual GetTaxRequest BuildGetTaxRequest(PMProforma doc)
		{
			if (doc == null)
				throw new PXArgumentException(nameof(doc));

			Customer cust = (Customer)Base.Customer.View.SelectSingleBound(new object[] { doc });
			Location loc = (Location)Base.Location.View.SelectSingleBound(new object[] { doc });
			TaxZone taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { doc });

			IAddressLocation fromAddress = GetFromAddress(doc);
			IAddressLocation toAddress = GetToAddress(doc);

			if (fromAddress == null)
				throw new PXException(Messages.FailedGetFromAddress);

			if (toAddress == null)
				throw new PXException(Messages.FailedGetToAddress);

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(doc.TaxZoneID, doc.BranchID);
			request.CurrencyCode = doc.CuryID;
			request.CustomerCode = cust.AcctCD;
			request.BAccountClassID = cust.CustomerClassID;
			request.TaxRegistrationID = loc?.TaxRegistrationID;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
			request.DocCode = $"PM.{doc.RefNbr}";
			request.DocDate = doc.InvoiceDate.GetValueOrDefault();
			request.LocationCode = GetExternalTaxProviderLocationCode(doc);
			request.DocType = TaxDocumentType.SalesOrder;
			request.CustomerUsageType = doc.AvalaraCustomerUsageType;
			request.APTaxType = taxZone.ExternalAPTaxType;

			if (!string.IsNullOrEmpty(doc.ExternalTaxExemptionNumber))
			{
				request.ExemptionNo = doc.ExternalTaxExemptionNumber;
			}

			foreach (PMProformaProgressLine tran in Base.ProgressiveLines.Select())
			{
				InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<PMProformaProgressLine.inventoryID>(Base.ProgressiveLines.Cache, tran);
				Account salesAccount = (Account)PXSelectorAttribute.Select<PMProformaProgressLine.accountID>(Base.ProgressiveLines.Cache, tran);

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;

				line.UnitPrice = tran.CuryUnitPrice.GetValueOrDefault();
				line.Amount = tran.CuryLineTotal.GetValueOrDefault();

				line.Description = tran.Description;
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				if (item != null)
					line.ItemCode = item.InventoryCD;
				line.Quantity = tran.Qty.GetValueOrDefault();
				line.UOM = tran.UOM;
				line.Discounted = request.Discount != 0m;
				line.RevAcct = salesAccount.AccountCD;

				line.TaxCode = tran.TaxCategoryID;
				if (!string.IsNullOrEmpty(item.HSTariffCode))
				{
					line.CommodityCode = new CommodityCode(item.CommodityCodeType, item.HSTariffCode);
				}

				request.CartItems.Add(line);
			}

			foreach (PMProformaTransactLine tran in Base.TransactionLines.Select())
			{
				InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<PMProformaTransactLine.inventoryID>(Base.TransactionLines.Cache, tran);
				Account salesAccount = (Account)PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(Base.TransactionLines.Cache, tran);

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;

				line.UnitPrice = tran.CuryUnitPrice.GetValueOrDefault();
				line.Amount = tran.CuryLineTotal.GetValueOrDefault();

				line.Description = tran.Description;
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				if (item != null)
					line.ItemCode = item.InventoryCD;
				line.Quantity = tran.Qty.GetValueOrDefault();
				line.UOM = tran.UOM;
				line.Discounted = request.Discount != 0m;
				line.RevAcct = salesAccount.AccountCD;

				line.TaxCode = tran.TaxCategoryID;

				request.CartItems.Add(line);
			}

			return request;
		}

		public virtual void ApplyTax(PMProforma doc, GetTaxResult result)
		{
			TaxZone taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { doc });
			AP.Vendor vendor = GetTaxAgency(Base, taxZone);

			if (result != null)
			{
				//Clear all existing Tax transactions:
				PXSelectBase<PMTaxTran> TaxesSelect =
					new PXSelectJoin<PMTaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<PMTaxTran.taxID>>>,
						Where<PMTaxTran.refNbr, Equal<Current<PMProforma.refNbr>>, 
							And<PMTaxTran.revisionID, Equal<Current<PMProforma.revisionID>>>>>(Base);
				foreach (PXResult<PMTaxTran, Tax> res in TaxesSelect.View.SelectMultiBound(new object[] { doc }))
				{
					PMTaxTran taxTran = (PMTaxTran)res;
					Base.Taxes.Delete(taxTran);
				}

				Base.Views.Caches.Add(typeof(Tax));

				var taxDetails = new List<PX.TaxProvider.TaxDetail>();
				for (int i = 0; i < result.TaxSummary.Length; i++)
					taxDetails.Add(result.TaxSummary[i]);

				TaxCalc oldPLTaxCalc = TaxBaseAttribute.GetTaxCalc<PMProformaProgressLine.taxCategoryID>(Base.ProgressiveLines.Cache, null);
				TaxCalc oldTLTaxCalc = TaxBaseAttribute.GetTaxCalc<PMProformaTransactLine.taxCategoryID>(Base.TransactionLines.Cache, null);

				try
				{
					TaxBaseAttribute.SetTaxCalc<PMProformaProgressLine.taxCategoryID>(Base.ProgressiveLines.Cache, null, TaxCalc.ManualCalc);
					TaxBaseAttribute.SetTaxCalc<PMProformaTransactLine.taxCategoryID>(Base.TransactionLines.Cache, null, TaxCalc.ManualCalc);

					foreach (var taxDetail in taxDetails)
					{
						taxDetail.TaxType = CSTaxType.Sales;
						Tax tax = CreateTax(Base, taxZone, vendor, taxDetail);
						if (tax == null)
							continue;

						PMTaxTran taxTran = new PMTaxTran();
						taxTran.RefNbr = doc.RefNbr;
						taxTran.RevisionID = doc.RevisionID;
						taxTran.TaxID = tax?.TaxID;
						taxTran.CuryTaxAmt = taxDetail.TaxAmount;
						taxTran.CuryTaxableAmt = taxDetail.TaxableAmount;
						taxTran.TaxRate = Convert.ToDecimal(taxDetail.Rate) * 100;
						taxTran.JurisType = taxDetail.JurisType;
						taxTran.JurisName = taxDetail.JurisName;

						Base.Taxes.Insert(taxTran);
					}
				}
				finally
				{
					TaxBaseAttribute.SetTaxCalc<PMProformaProgressLine.taxCategoryID>(Base.ProgressiveLines.Cache, null, oldPLTaxCalc);
					TaxBaseAttribute.SetTaxCalc<PMProformaTransactLine.taxCategoryID>(Base.TransactionLines.Cache, null, oldTLTaxCalc);
				}
			}

			Base.Document.Update(doc);
			SkipTaxCalcAndSave();
		}

		public virtual IAddressLocation GetFromAddress(PMProforma doc)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

			foreach (PXResult<Branch, BAccountR, Address> res in select.Select(doc.BranchID))
				return (Address)res;

			return null;
		}

		public virtual IAddressLocation GetToAddress(PMProforma doc)
		{
			return (PMShippingAddress)Base.Shipping_Address.View.SelectSingleBound(new object[] { doc });
		}

		public virtual bool IsSame(GetTaxRequest x, GetTaxRequest y)
		{
			if (x.CartItems.Count != y.CartItems.Count)
				return false;

			for (int i = 0; i < x.CartItems.Count; i++)
			{
				if (x.CartItems[i].Amount != y.CartItems[i].Amount)
					return false;
			}

			return true;

		}
	}

	public class ProformaEntryExternalTax_Workflow : PXGraphExtension<ProformaEntry_ApprovalWorkflow, ProformaEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProformaEntry, PMProforma>());
		protected static void Configure(WorkflowContext<ProformaEntry, PMProforma> context)
		{
			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.UpdateDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Update<ProformaStatus.onHold>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add<ProformaEntryExternalTax>(g => g.recalcExternalTax);
									});
							});
							fss.Update<ProformaStatus.open>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add<ProformaEntryExternalTax>(g => g.recalcExternalTax);
									});
							});
							fss.Update<ProformaStatus.pendingApproval>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add<ProformaEntryExternalTax>(g => g.recalcExternalTax);
									});
							});
							fss.Update<ProformaStatus.rejected>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add<ProformaEntryExternalTax>(g => g.recalcExternalTax);
									});
							});
						}))
					.WithActions(actions =>
					{
						actions.Add<ProformaEntryExternalTax>(g => g.recalcExternalTax,
							c => c.InFolder(context.Categories.Get(ToolbarCategory.ActionCategoryNames.Other), g => g.send)
								.PlaceAfter(g => g.send));
					});
			});
		}
	}
}
