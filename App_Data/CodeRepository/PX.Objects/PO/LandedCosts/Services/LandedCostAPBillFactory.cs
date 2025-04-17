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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;

namespace PX.Objects.PO.LandedCosts
{
	public class LandedCostAPBillFactory
	{
		private readonly PXGraph _pxGraph;

		public LandedCostAPBillFactory(PXGraph pxGraph)
		{
			_pxGraph = pxGraph;
		}

		public APInvoice CreateLandedCostBillHeader(POLandedCostDoc doc, IEnumerable<POLandedCostDetail> details, APInvoice newdoc)
		{
			decimal handledLinesAmt = details.Sum(d => d.CuryLineAmt) ?? 0m;
			newdoc.DocType = (handledLinesAmt >= 0m) ? APDocType.Invoice : APDocType.DebitAdj;

			if (doc.PayToVendorID.HasValue && doc.PayToVendorID != doc.VendorID && PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
			{
				newdoc.VendorID = doc.PayToVendorID;
				newdoc.SuppliedByVendorID = doc.VendorID;
				newdoc.SuppliedByVendorLocationID = doc.VendorLocationID;
			}
			else
			{
				newdoc.VendorID = doc.VendorID;
				newdoc.VendorLocationID = doc.VendorLocationID;
				newdoc.SuppliedByVendorID = doc.VendorID;
				newdoc.SuppliedByVendorLocationID = doc.VendorLocationID;
			}

			newdoc.DocDate = doc.BillDate ?? newdoc.DocDate;
			newdoc.TaxCalcMode = TaxCalculationMode.TaxSetting;
			newdoc.TermsID = doc.TermsID;
			newdoc.InvoiceNbr = doc.VendorRefNbr;
			newdoc.BranchID = doc.BranchID;
			newdoc.TaxCalcMode = TaxCalculationMode.TaxSetting;

			newdoc.TaxZoneID = doc.TaxZoneID;
			newdoc.CuryOrigDocAmt = doc.CuryDocTotal;
			newdoc.DueDate = doc.DueDate;
			newdoc.DiscDate = doc.DiscDate;

			return newdoc;
		}

		public APInvoiceWrapper CreateLandedCostBill(POLandedCostDoc doc, IEnumerable<POLandedCostDetail> details,
			IEnumerable<POLandedCostTaxTran> taxes, APInvoice newdoc)
		{
			newdoc = CreateLandedCostBillHeader(doc, details, newdoc);
			decimal mult = newdoc.DocType == APDocType.Invoice ? 1m : -1m;

			var apTransactions = CreateTransactions(doc, details, mult);

			var apTaxes = taxes.Select(tax => new APTaxTran()
			{
				Module = BatchModule.AP,
				TaxID = tax.TaxID,
				JurisType = tax.JurisType,
				JurisName = tax.JurisName,
				TaxRate = tax.TaxRate,
				CuryID = doc.CuryID,
				CuryInfoID = doc.CuryInfoID,
				CuryTaxableAmt = mult * tax.CuryTaxableAmt,
				TaxableAmt = mult * tax.TaxableAmt,
				CuryTaxAmt = mult * tax.CuryTaxAmt,
				NonDeductibleTaxRate = mult * tax.NonDeductibleTaxRate,
				TaxAmt = mult * tax.TaxAmt,
				CuryExpenseAmt = mult * tax.CuryExpenseAmt,
				ExpenseAmt = mult * tax.ExpenseAmt,
				TaxZoneID = tax.TaxZoneID
			}).ToList();

			var result = new APInvoiceWrapper(newdoc, apTransactions, apTaxes);

			return result;
		}

		public virtual APTran[] CreateTransactions(POLandedCostDoc doc, IEnumerable<POLandedCostDetail> landedCostDetail, decimal mult)
		{
			var result = new List<APTran>();

			foreach (var detail in landedCostDetail)
			{
				var aLCCode = GetLandedCostCode(detail.LandedCostCodeID);

				APTran aDest = new APTran();

				aDest.AccountID = detail.LCAccrualAcct;
				aDest.SubID = detail.LCAccrualSub;
				
				aDest.UOM = null;
				aDest.Qty = Decimal.One;

				aDest.CuryUnitCost = mult * detail.CuryLineAmt ?? 0m;
				aDest.CuryTranAmt = mult * detail.CuryLineAmt ?? 0m;
				
				aDest.TranDesc = detail.Descr;
				aDest.InventoryID = null;
				aDest.TaxCategoryID = detail.TaxCategoryID;
				//aDest.TaxID = aSrc.TaxID;
				aDest.LCDocType = doc.DocType;
				aDest.LCRefNbr = doc.RefNbr;
				aDest.LCLineNbr = detail.LineNbr;
				// retainage is always zero for landed cost trans
				aDest.RetainagePct = 0m;
				aDest.CuryRetainageAmt = 0m;
				aDest.RetainageAmt = 0m;

				aDest.BranchID = detail.BranchID;

				aDest.ReceiptLineNbr = null;
				aDest.LandedCostCodeID = detail.LandedCostCodeID;

				result.Add(aDest);
			}

			return result.ToArray();
		}

		protected virtual LandedCostCode GetLandedCostCode(string landedCostCodeID) => LandedCostCode.PK.Find(_pxGraph, landedCostCodeID);
	}
}
