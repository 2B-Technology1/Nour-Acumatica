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
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.SO.GraphExtensions
{
	public abstract class ARAdjustCorrectionExtension<TGraph> : ARAdjustCorrectionExtension<TGraph, object>
		where TGraph : PXGraph
	{
	}

	public abstract class ARAdjustCorrectionExtension<TGraph, TCancellationField> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<ARAdjust.ARInvoice.isUnderCorrection, NotEqual<True>>),
			Messages.CantCreateApplicationToInvoiceUnderCorrection, typeof(ARAdjust.ARInvoice.refNbr))]
		public virtual void _(Events.CacheAttached<ARAdjust.adjdRefNbr> e)
		{
		}

		public virtual void _(Events.FieldVerifying<ARAdjust.adjdRefNbr> e)
		{
			if (this.GetCancellationFieldValue() == true)
			{
				e.Cancel = true;
			}
		}

		public virtual void _(Events.RowPersisting<ARAdjust> e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert
				&& this.GetCancellationFieldValue() != true)
			{
				var origInv = PXParentAttribute.SelectParent<ARInvoice>(e.Cache, e.Row);
				if (origInv?.IsUnderCorrection == true)
				{
					e.Cache.RaiseExceptionHandling<ARAdjust.adjdRefNbr>(e.Row, e.Row.AdjdRefNbr,
						new PXSetPropertyException(Messages.CantCreateApplicationToInvoiceUnderCorrection, origInv.RefNbr));
				}
			}
		}
		
		public virtual bool? GetCancellationFieldValue()
		{
			if (!typeof(IBqlField).IsAssignableFrom(typeof(TCancellationField)))
				return false;

			PXCache parentCache = Base.Caches[typeof(TCancellationField).DeclaringType];
			return (bool?)parentCache.GetValue(parentCache.Current, typeof(TCancellationField).Name);
		}
	}
}
