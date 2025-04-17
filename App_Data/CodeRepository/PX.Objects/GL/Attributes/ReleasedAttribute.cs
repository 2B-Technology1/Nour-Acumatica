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
using System.Collections;
using PX.Data;
using PX.Objects.GL.DAC;
using PX.Objects.CR;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PM;

namespace PX.Objects.GL
{
	[PXDBBool]
	public class ReleasedAttribute : PXAggregateAttribute, IPXRowDeletingSubscriber
	{
		public bool PreventDeletingReleased { get; set; } = false;

		public void RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (PreventDeletingReleased
				&& sender.GetValue(e.Row, _FieldName) != null
				&& (bool?)sender.GetValue(e.Row, _FieldName) == true)
			{
				throw new PXException(InfoMessages.ReleasedDocCannotBeDeleted);
			}
		}
	}
}
