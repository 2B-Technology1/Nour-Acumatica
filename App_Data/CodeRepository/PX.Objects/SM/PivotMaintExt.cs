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
using PX.Objects.GL;
using PX.Olap;
using PX.Olap.Maintenance;

namespace PX.Objects.SM
{
	public class PivotMaintExt : PXGraphExtension<PivotMaint>
	{
		public static bool TryDetermineSortType(DataScreenBase dataScreen, string field, out SortType sortType)
		{
			if (IsFinPeriod(dataScreen, field))
			{
				sortType = SortType.ByValue;
				return true;
			}

			sortType = SortType.ByDisplayValue;
			return false;
		}

		public static bool IsFinPeriod(DataScreenBase dataScreen, string field)
		{
			if (!String.IsNullOrEmpty(field))
			{
				var attr = dataScreen?.View.Cache
					.GetAttributesReadonly(field, true)
					.OfType<FinPeriodIDFormattingAttribute>()
					.FirstOrDefault();

				if (attr != null)
				{
					return true;
				}
			}

			return false;
		}

		[PXOverride]
		public virtual SortType DetermineSortType(string field, Func<string, SortType> del)
		{
			if (TryDetermineSortType(Base.DataScreen, field, out var sortType))
				return sortType;

			return del(field);
		}
	}
}
