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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.BarcodeProcessing;

namespace PX.Objects.AM
{
	public abstract class OperationStateBase<TScanBasis> : EntityState<TScanBasis, AMProdOper>
		where TScanBasis : PXGraphExtension, IBarcodeDrivenStateMachine
	{
		public const string Value = "OPER";
		public class value : BqlString.Constant<value> { public value() : base(OperationStateBase<TScanBasis>.Value) { } }

		public override string Code => Value;
		protected override string StatePrompt => Msg.Prompt;

		protected abstract string OrderType { get; set; }
		protected abstract string ProdOrdID { get; set; }
		protected abstract int? OperationID { get; set; }

		protected override AMProdOper GetByBarcode(string barcode) => AMProdOper.UK.Find(Basis.Graph, OrderType, ProdOrdID, barcode);
		protected override void ReportMissing(string barcode) => Basis.Reporter.Error(Msg.Missing, barcode);
		protected override void Apply(AMProdOper oper) => OperationID = oper.OperationID;
		protected override void ClearState() => OperationID = null;
		protected override void ReportSuccess(AMProdOper selection) => Basis.Reporter.Info(Msg.Ready, selection.OperationCD);

		[PXLocalizable]
		public abstract class Msg
		{
			public const string Prompt = "Scan the Operation ID.";
			public const string Missing = "The {0} operation is not found.";
			public const string Ready = "The {0} operation is selected.";
		}
	}
}
