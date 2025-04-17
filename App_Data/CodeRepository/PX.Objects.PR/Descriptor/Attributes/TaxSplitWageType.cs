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
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PX.Objects.PR
{
	public class TaxSplitWageTypeListAttribute : PXIntListAttribute
	{
		public TaxSplitWageTypeListAttribute() : base(
			new int[] { TaxSplitWageType.Tips, TaxSplitWageType.Others },
			new string[] { Messages.Tips, Messages.NotTips })
		{
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			PRPayment currentPayment = (PRPayment)sender.Graph.Caches[typeof(PRPayment)].Current;
			if (currentPayment != null)
			{
				if (currentPayment.CountryID == LocationConstants.USCountryCode)
				{
					List<PRTypeMeta> wageTypes = PRTypeSelectorAttribute.GetAll<PRWage>(currentPayment.CountryID);
					_AllowedValues = wageTypes.Select(x => x.ID).ToArray();
					_AllowedLabels = wageTypes.Select(x => x.Name.ToUpper()).ToArray();
				}
				else
				{
					_AllowedValues = new int[] { TaxSplitWageType.Tips, TaxSplitWageType.Others };
					_AllowedLabels = new string[] { Messages.Tips, Messages.NotTips };
				}
			}
		}
	}
}
