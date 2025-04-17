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
using PX.Common;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.IN
{
	public class INItemXRefBAccountAttribute : BAccountAttribute
	{
		public override bool HideInactiveCustomers { get; set; } = false;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.RemoveHandler<INItemXRef.bAccountID>(SelectorAttribute.FieldUpdating);
			sender.Graph.FieldUpdating.AddHandler<INItemXRef.bAccountID>(FieldUpdating);

			sender.Graph.FieldSelecting.RemoveHandler<INItemXRef.bAccountID>(SelectorAttribute.FieldSelecting);
			sender.Graph.FieldSelecting.AddHandler<INItemXRef.bAccountID>(FieldSelecting);
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var crossItem = (INItemXRef)e.Row;
			if (crossItem?.AlternateType?.IsNotIn(INAlternateType.CPN, INAlternateType.VPN) ?? true)
                e.ReturnValue = null;//0 -> null

            SelectorAttribute.FieldSelecting(sender, e);
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			SelectorAttribute.FieldUpdating(sender, e);

			var crossItem = (INItemXRef)e.Row;
			if (crossItem?.AlternateType?.IsNotIn(INAlternateType.CPN, INAlternateType.VPN) ?? true)
			{
				e.NewValue = 0;//null -> 0
				e.Cancel = true;
			}
		}
	}
}
