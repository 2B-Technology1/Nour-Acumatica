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

namespace PX.Objects.AP.MigrationMode
{
	public class APMigrationModeDependentInvoiceTypeListAttribute : APInvoiceType.ListAttribute
	{
		public APMigrationModeDependentInvoiceTypeListAttribute()
			: base()
		{ }

		public override void CacheAttached(PXCache sender)
		{
				APSetup setup = PXSetup<APSetup>.Select(sender.Graph);

            if (setup == null || setup.MigrationMode != true)
            {
                base.CacheAttached(sender);
                return;
            }

			_AllowedValues = new[] { APDocType.Invoice, APDocType.DebitAdj, APDocType.CreditAdj, APDocType.Prepayment };
			_AllowedLabels = new[] { Messages.Invoice, Messages.DebitAdj, Messages.CreditAdj, Messages.Prepayment };
			_NeutralAllowedLabels = new[] { Messages.Invoice, Messages.DebitAdj, Messages.CreditAdj, Messages.Prepayment };

			base.CacheAttached(sender);
		}
	}
}
