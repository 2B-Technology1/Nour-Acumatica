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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CN.Common.Extensions;
using System.Collections.Generic;
using PX.Objects.TX;

namespace PX.Objects.AR
{
	public class ARInvoiceEntryVATRecognitionOnPrepayments : PXGraphExtension<ARInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.vATRecognitionOnPrepayments>();
		}
		public override void Initialize()
		{
			AddPrepaymentInvoiceDocType();

			base.Initialize();

			Base.TaxesList.WhereAnd<Where<ARTaxTran.taxType, NotEqual<TaxType.recognition>>>();
		}
		private void AddPrepaymentInvoiceDocType()
		{
			var allowedValues = ARDocType.PrepaymentInvoice.CreateArray();
			var allowedLabels = Messages.PrepaymentInvoice.CreateArray();
			PXStringListAttribute.AppendList<ARInvoice.docType>(Base.Document.Cache, null, allowedValues, allowedLabels);
			PXStringListAttribute.AppendList<ARAdjust.adjdDocType>(Base.Adjustments.Cache, null, allowedValues, allowedLabels);
			PXStringListAttribute.AppendList<ARAdjust2.adjgDocType>(Base.Adjustments.Cache, null, allowedValues, allowedLabels);
			PXStringListAttribute.AppendList<ARAdjust.displayDocType>(Base.Adjustments.Cache, null, allowedValues, allowedLabels);
		}

		[PXMergeAttributes]
		[PXRestrictor(typeof(Where<
			Current<ARRegister.docType>, NotEqual<ARDocType.prepaymentInvoice>,
			Or<Current<ARRegister.docType>, Equal<ARDocType.prepaymentInvoice>,
				And<Terms.installmentType, Equal<TermsInstallmentType.single>>>>), Messages.CannotSelectMultipleInstallmentCreditTermsForPrepaymentInvoice)]
		[PXRestrictor(typeof(Where<
			Current<ARRegister.docType>, NotEqual<ARDocType.prepaymentInvoice>,
			Or<Current<ARRegister.docType>, Equal<ARDocType.prepaymentInvoice>,
				And<Terms.discPercent, Equal<decimal0>>>>), Messages.CannotSelectCreditTermsWithDiscountForPrepaymentInvoice)]
		protected virtual void ARInvoice_TermsID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(AccountAttribute))]
		[Account(typeof(ARInvoice.branchID), typeof(Search<Account.accountID,
			Where2<Match<Current<AccessInfo.userName>>,
				And<Account.active, Equal<True>,
				And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
					Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>), DisplayName = "Prepayment Account")]
		protected virtual void ARInvoice_PrepaymentAccountID_CacheAttached(PXCache sender) { }

		protected virtual void _(Events.FieldDefaulting<ARInvoice, ARInvoice.prepaymentAccountID> e)
		{
			if (e.Row != null && Base.customer.Current != null)
			{
				e.NewValue = Base.GetAcctSub<Customer.prepaymentAcctID>(Base.customer.Cache, Base.customer.Current);
			}
		}

		protected virtual void _(Events.FieldDefaulting<ARInvoice, ARInvoice.prepaymentSubID> e)
		{
			if (e.Row != null && Base.customer.Current != null)
			{
				e.NewValue = Base.GetAcctSub<Customer.prepaymentSubID>(Base.customer.Cache, Base.customer.Current);
			}
		}
		public virtual void ARInvoice_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARInvoice.prepaymentAccountID>(e.Row);
			sender.SetDefaultExt<ARInvoice.prepaymentSubID>(e.Row);
		}

		public virtual void ARTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (Base.Document.Current != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
			{
				if (Base.Document.Current.IsPrepaymentInvoiceDocument())
				{
					ARTaxTran taxTran = (ARTaxTran)e.Row;
					taxTran.TaxType = TaxType.PendingSales;
					taxTran.CuryAdjustedTaxableAmt = taxTran.CuryTaxableAmt;
					taxTran.AdjustedTaxableAmt = taxTran.TaxableAmt;
					taxTran.CuryAdjustedTaxAmt = taxTran.CuryTaxAmt;
					taxTran.AdjustedTaxAmt = taxTran.TaxAmt;
				}
			}
		}

		public virtual void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARInvoice doc = (ARInvoice)e.Row;
			PXUIFieldAttribute.SetEnabled<ARInvoice.curyDocUnpaidBal>(cache, doc, false);

			bool isUnreleasedMigratedDocument = doc.IsMigratedRecord == true && doc.Released != true;
			bool substituteBalanceWithUnpaidBalance = doc.IsPrepaymentInvoiceDocument()
				&& doc.Status != ARDocStatus.Open
				&& doc.Status != ARDocStatus.Closed
				&& doc.Status != ARDocStatus.Voided
				&& doc.Status != ARDocStatus.Reserved;

			PXUIFieldAttribute.SetVisible<ARInvoice.curyDocBal>(cache, doc, !isUnreleasedMigratedDocument && !substituteBalanceWithUnpaidBalance);
			PXUIFieldAttribute.SetVisible<ARInvoice.curyDocUnpaidBal>(cache, doc, !isUnreleasedMigratedDocument && substituteBalanceWithUnpaidBalance);
			PXUIFieldAttribute.SetVisible<ARTaxTran.curyAdjustedTaxableAmt>(Base.Taxes.Cache, null, doc.IsPrepaymentInvoiceDocument());
			PXUIFieldAttribute.SetVisible<ARTaxTran.curyAdjustedTaxAmt>(Base.Taxes.Cache, null, doc.IsPrepaymentInvoiceDocument());

			PXUIFieldAttribute.SetDisplayName<ARTaxTran.curyTaxableAmt>(Base.Taxes.Cache, doc.IsPrepaymentInvoiceDocument() ? Messages.OrigTaxableAmount : "Taxable Amount");
			PXUIFieldAttribute.SetDisplayName<ARTaxTran.curyTaxAmt>(Base.Taxes.Cache, doc.IsPrepaymentInvoiceDocument() ? Messages.OrigTaxAmount : "Tax Amount");
		}

		[PXOverride]
		public void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers)
		{
			Base.CopyPasteGetScript(isImportSimple, script, containers);

			if (Base.CurrentDocument.Current?.DocType == ARDocType.PrepaymentInvoice)
			{
				(string objectNamePrefix, string fieldName)[] commandsToDelete =
				{
					("Document", "RetainageApply"),
					("CurrentDocument", "RetainageAcctID"),
					("CurrentDocument", "RetainageSubID"),
					("CurrentDocument", "DefRetainagePct"),
					("Transactions", "RetainagePct"),
					("Transactions", "CuryRetainageAmt"),
					("Transactions", "CuryRetainageBal")
				};

				foreach (var (objectNamePrefix, fieldName) in commandsToDelete)
				{
					int commandIndex = script.FindIndex(cmd => cmd.ObjectName.StartsWith(objectNamePrefix)
						&& cmd.FieldName == fieldName);
					if (commandIndex != -1)
					{
						script.RemoveAt(commandIndex);
						containers.RemoveAt(commandIndex);
					}
				}
			}
		}

	}
}
