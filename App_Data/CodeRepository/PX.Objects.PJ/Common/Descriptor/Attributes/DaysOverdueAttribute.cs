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
using System;

namespace PX.Objects.PJ.Common.Descriptor.Attributes
{
	public class DaysOverdueAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		private readonly Type SourceDateField;
		private readonly Type FinalDateField;

		public DaysOverdueAttribute(Type sourceDateField, Type finalDateField)
		{
			SourceDateField = sourceDateField;
			FinalDateField = finalDateField;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			DateTime? sourceDate = (DateTime?)sender.GetValue(e.Row, SourceDateField.Name);
			if (sourceDate != null)
			{
				DateTime? closedDate = (DateTime?)sender.GetValue(e.Row, FinalDateField.Name);

				DateTime oppositeDate = closedDate ?? sender.Graph.Accessinfo.BusinessDate.Value;

				int deltaDays = oppositeDate.Subtract(sourceDate.Value).Days;

				e.ReturnValue = deltaDays > 0 ? (int?)deltaDays : null;
			}
		}
	}
}
