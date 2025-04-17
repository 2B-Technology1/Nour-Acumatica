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
using PX.Objects.IN;

namespace PX.Objects.Extensions
{
	/// <summary>
	/// Disabling of validation for decimal values for drop ship lines
	/// </summary>
	public abstract class NonDecimalUnitsNoVerifyOnDropShipExt<TGraph, TLine> : PXGraphExtension<TGraph>
		where TGraph: PXGraph
		where TLine : class, IBqlTable, new()
	{
		protected abstract bool IsDropShipLine(TLine line);

		protected virtual void _(Events.RowSelected<TLine> e)
		{
			if (e.Row != null)
				SetDecimalVerifyMode(e.Cache, e.Row);
		}

		protected virtual void _(Events.RowPersisting<TLine> e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
				SetDecimalVerifyMode(e.Cache, e.Row);
		}

		public virtual void SetDecimalVerifyMode(PXCache cache, TLine line)
		{
			var decimalVerifyMode = GetLineVerifyMode(cache, line);
			cache.Adjust<PXDBQuantityAttribute>(line).ForAllFields(a => a.SetDecimalVerifyMode(line, decimalVerifyMode));
		}

		protected virtual DecimalVerifyMode GetLineVerifyMode(PXCache cache, TLine line) 
			=> IsDropShipLine(line) ? DecimalVerifyMode.Off : DecimalVerifyMode.Error;
	}
}
