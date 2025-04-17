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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CA;
using PX.Objects.CR;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information related to the payment associated with a paycheck.
	/// </summary>
	[PXPrimaryGraph(
		new Type[] { typeof(PRDirectDepositBatchEntry) },
		new Type[] { typeof(Where<CABatch.origModule.IsEqual<GL.BatchModule.modulePR>>) })]
	[PXCacheName(Messages.PRCABatch)]
	[Serializable]
	[PXProjection(typeof(SelectFrom<CABatch>
		.InnerJoin<PaymentMethod>
			.On<CABatch.FK.PaymentMethod>
		.InnerJoin<CABatchDetail>
			.On<CABatchDetail.FK.CashAccountBatch>
		.InnerJoin<PRPayment>
			.On<PRPayment.docType.IsEqual<CABatchDetail.origDocType>
			.And<PRPayment.refNbr.IsEqual<CABatchDetail.origRefNbr>>>
		.LeftJoin<PRDirectDepositSplit>
			.On<PRDirectDepositSplit.docType.IsEqual<CABatchDetail.origDocType>
			.And<PRDirectDepositSplit.refNbr.IsEqual<CABatchDetail.origRefNbr>>
			.And<PRDirectDepositSplit.lineNbr.IsEqual<CABatchDetail.origLineNbr>>>
		.AggregateTo<
			GroupBy<CABatch.batchNbr>,
			GroupBy<CABatch.paymentMethodID>,
			GroupBy<PRxPaymentMethod.prPrintChecks>,
			GroupBy<CABatch.noteID>,
			Sum<PRPayment.netAmount>,
			Sum<PRDirectDepositSplit.amount>>),
		new Type[] { typeof(CABatch) },
		Persistent = true)]
	public class PRCABatch : CABatch
	{
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[CABatchType.Numbering]
		[PRCABatchRefNbr(typeof(Search<CABatch.batchNbr, Where<CABatch.origModule, Equal<GL.BatchModule.modulePR>>>))]
		public override string BatchNbr { get; set; }
		#endregion
		#region PaymentMethodID
		public new abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(SearchFor<PaymentMethod.paymentMethodID>.Where<PRxPaymentMethod.useForPR.IsEqual<True>>))]
		public override string PaymentMethodID { get; set; }
		#endregion
		#region PRPrintChecks
		public abstract class prPrintChecks : PX.Data.BQL.BqlBool.Field<prPrintChecks> { }
		[PXDBBool(BqlField = typeof(PRxPaymentMethod.prPrintChecks))]
		[PXUIField(Visible = false)]
		public virtual bool? PRPrintChecks { get; set; }
		#endregion
		#region BatchTotal
		public abstract class batchTotal : PX.Data.BQL.BqlDecimal.Field<batchTotal> { }
		[PXUIField(DisplayName = "Batch Total", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(paymentTotal.When<prPrintChecks.IsEqual<True>.And<paymentTotal.IsEqual<curyDetailTotal>>>
			.Else<directDepositSplitTotal>.When<prPrintChecks.IsEqual<False>.And<directDepositSplitTotal.IsEqual<curyDetailTotal>>>
			.Else<Null>))]
		public virtual decimal? BatchTotal { get; set; }
		#endregion
		#region PaymentTotal
		public abstract class paymentTotal : PX.Data.BQL.BqlDecimal.Field<paymentTotal> { }
		[PRCurrency(BqlField = typeof(PRPayment.netAmount))]
		[PXUIField(Visible = false)]
		public virtual decimal? PaymentTotal { get; set; }
		#endregion
		#region DirectDepositSplitTotal
		public abstract class directDepositSplitTotal : PX.Data.BQL.BqlDecimal.Field<directDepositSplitTotal> { }
		[PRCurrency(BqlField = typeof(PRDirectDepositSplit.amount))]
		[PXUIField(Visible = false)]
		public virtual decimal? DirectDepositSplitTotal { get; set; }
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.PR, Messages.SearchableTitlePRCABatch, new Type[] { typeof(batchNbr), typeof(BAccount.acctName) },
			new Type[] { typeof(paymentMethodID), typeof(batchSeqNbr), typeof(tranDesc), typeof(extRefNbr), typeof(BAccount.acctCD) },
			NumberFields = new Type[] { typeof(batchNbr) },
			Line1Format = "{0}{1:d}{2}", Line1Fields = new Type[] { typeof(extRefNbr), typeof(tranDate), typeof(batchSeqNbr) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(tranDesc) }
		)]
		[PXNote(BqlField = typeof(CABatch.noteID))]
		public override Guid? NoteID { get; set; }
		#endregion
		#region HeaderDescription
		public abstract class headerDescription : PX.Data.BQL.BqlString.Field<headerDescription> { }
		[BatchHeader(typeof(paymentMethodID), typeof(tranDate))]
		public string HeaderDescription { get; set; }
		#endregion
	}

	public class BatchHeaderAttribute : PXStringAttribute, IPXFieldDefaultingSubscriber
	{
		private readonly Type _PaymentMethodIDField;
		private readonly Type _TranDateField;

		public BatchHeaderAttribute(Type paymentMethodID, Type tranDate)
		{
			_PaymentMethodIDField = paymentMethodID;
			_TranDateField = tranDate;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (e.Row == null)
				return;

			e.ReturnValue = GetHeader(sender, e.Row);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null)
				return;

			e.NewValue = GetHeader(sender, e.Row);
		}

		private string GetHeader(PXCache sender, object row)
		{
			string paymentMethodID = (PXSelectorAttribute.Select(sender, row, _PaymentMethodIDField.Name) as PaymentMethod)?.Descr;
			string tranDate = (sender.GetValue(row, _TranDateField.Name) as DateTime?)?.ToShortDateString();

			if (string.IsNullOrWhiteSpace(paymentMethodID) || string.IsNullOrWhiteSpace(tranDate))
				return string.Empty;

			return $"{paymentMethodID} - {tranDate}";
		}
	}
}
