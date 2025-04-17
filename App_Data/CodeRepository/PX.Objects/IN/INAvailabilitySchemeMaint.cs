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

namespace PX.Objects.IN
{
	public class INAvailabilitySchemeMaint : PXGraph<INAvailabilitySchemeMaint, INAvailabilityScheme>
	{
		public PXSelect<INAvailabilityScheme> Schemes;

		protected virtual void INAvailabilityScheme_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var itemClass = (INItemClass)PXSelectReadonly<INItemClass, 
				Where<INItemClass.availabilitySchemeID, Equal<Current<INAvailabilityScheme.availabilitySchemeID>>>>.SelectWindowed(this, 0, 1);
			if (itemClass != null)
			{
				throw new PXException(Messages.NotPossibleDeleteINAvailScheme);
			}
		}
	}
}
