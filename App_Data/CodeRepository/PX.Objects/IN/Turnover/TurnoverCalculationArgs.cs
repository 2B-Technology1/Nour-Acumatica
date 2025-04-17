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

namespace PX.Objects.IN.Turnover
{
	public class TurnoverCalculationArgs
	{
		public virtual Guid? NoteID { get; set; } 

		public virtual int? BranchID { get; set; }

		public virtual string FromPeriodID { get; set; }

		public virtual string ToPeriodID { get; set; }

		public virtual int? SiteID { get; set; }

		public virtual int? ItemClassID { get; set; }

		public virtual int?[] Inventories { get; set; }

		public virtual bool IsFullCalc
		{
			get
			{
				return SiteID == null
					&& ItemClassID == null
					&& (Inventories?.Length ?? 0) == 0;
			}
		}

		public virtual int NumberOfDays { get; set; }
	}
}
