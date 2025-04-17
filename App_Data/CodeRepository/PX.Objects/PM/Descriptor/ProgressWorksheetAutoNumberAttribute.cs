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
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class ProgressWorksheetAutoNumberAttribute : AutoNumberAttribute
	{
		public ProgressWorksheetAutoNumberAttribute() : base(typeof(PMSetup.progressWorksheetNumbering), typeof(AccessInfo.businessDate)) { }

		public bool Disable { get; set; }

		public static void DisableAutonumbiring(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<PMProgressWorksheet.refNbr>())
			{
				if (attr is ProgressWorksheetAutoNumberAttribute)
				{
					((ProgressWorksheetAutoNumberAttribute)attr).Disable = true;
					((ProgressWorksheetAutoNumberAttribute)attr).UserNumbering = true;
				}
			}
		}

		protected override string GetNewNumberSymbol(string numberingID)
		{
			if (Disable)
				return NullString;

			return base.GetNewNumberSymbol(numberingID);
		}
	}
}
