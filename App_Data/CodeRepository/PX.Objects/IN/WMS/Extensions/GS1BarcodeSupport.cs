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

using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Common.GS1;
using PX.Data;
using PX.BarcodeProcessing;

namespace PX.Objects.IN.WMS
{
	public abstract class GS1BarcodeSupport<TScanBasis, TScanGraph> : CompositeBarcodeSupport<TScanBasis, TScanGraph, EAN128BarcodeParser>
		where TScanBasis : BarcodeDrivenStateMachine<TScanBasis, TScanGraph>
		where TScanGraph : PXGraph, new()
	{
		public PXSetupOptional<GS1UOMSetup> gs1Setup;
		protected override string GetUOMOf(string code) => PX.Common.GS1.Parser.TryGetAI(code, out AI ai) ? gs1Setup.Current.GetUOMOf(ai) : null;
		protected override void ReportNoneWasApplied() => Basis.ReportError(Msg.GS1BarcodeWasNotHandled);

		#region Messages
		[PXLocalizable]
		public new abstract class Msg
		{
			public const string GS1BarcodeWasNotHandled = "The values in the GS1 barcode are not valid for the current step.";
		}
		#endregion
	}

	public class EAN128BarcodeParser : ICompositeBarcodeParser
	{
		private readonly Parser _ean128Parser = new Parser();
		public bool IsCompositeBarcode(string barcode) => barcode.Contains(_ean128Parser.GroupSeparator) || PX.Common.GS1.StartCodes.All.Any(sc => barcode.StartsWith(sc));
		public IReadOnlyDictionary<string, ParseResult> Parse(string compositeBarcode) => _ean128Parser.Parse(compositeBarcode).ToDictionary(kvp => kvp.Key.Code, kvp => getResult(kvp.Value));
		private static ParseResult getResult(StringData data) =>
			data is DateTimeData date		?	ParseResult.From(date.Value):
			data is DecimalData @decimal	?	ParseResult.From(@decimal.Value):
												ParseResult.From(data.RawValue);
	}
}
