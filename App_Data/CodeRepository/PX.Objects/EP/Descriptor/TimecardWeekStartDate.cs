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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.EP
{
	public class TimecardWeekStartDate : PXDateAttribute, IPXFieldDefaultingSubscriber, IPXRowSelectingSubscriber
	{
		Type weekID;

		public TimecardWeekStartDate(Type weekID)
		{
			this.weekID = weekID;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), weekID.Name, OnWeekIdUpdated);
		}

		protected virtual DateTime? GetWeekStartDate(PXCache sender, object row)
		{
			DateTime? result = null;

			if (weekID != null)
			{
				int? week = (int?)sender.GetValue(row, weekID.Name);
				if (week != null)
					result = PXWeekSelector2Attribute.GetWeekStartDate(sender.Graph, week.Value);
			}

			return result;
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				DateTime? val = GetWeekStartDate(sender, e.Row);
				e.NewValue = val;
			}
		}
		
		protected virtual void OnWeekIdUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null)
			{
				DateTime? val = GetWeekStartDate(sender, e.Row);
				sender.SetValue(e.Row, FieldName, val);
			}
		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			int? week = (int?)sender.GetValue(e.Row, weekID.Name);

			if (e.Row != null && week != null)
			{
				using (new PXConnectionScope())
				{
					DateTime? val = GetWeekStartDate(sender, e.Row);
					sender.SetValue(e.Row, FieldName, val);
				}
			}
		}
	}
}
