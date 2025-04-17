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
using PX.Data.BQL.Fluent;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRAccountSubHelper
	{
		public static bool IsVisiblePerSetup<TExpenseAcctDefault, TExpenseSubMask>(PXGraph graph, string compareValue)
			where TExpenseAcctDefault : IBqlField
			where TExpenseSubMask : IBqlField
		{
			PXCache setupCache = graph.Caches[typeof(PRSetup)];
			PRSetup payrollPreferences = setupCache?.Current as PRSetup ??
				new SelectFrom<PRSetup>.View(graph).SelectSingle();

			if (payrollPreferences == null)
			{
				return false;
			}
			if (compareValue.Equals(setupCache.GetValue(payrollPreferences, typeof(TExpenseAcctDefault).Name)))
			{
				return true;
			}

			if (setupCache != null)
			{
				PRSubAccountMaskAttribute subMaskAttribute = setupCache.GetAttributesOfType<PRSubAccountMaskAttribute>(payrollPreferences, typeof(TExpenseSubMask).Name).FirstOrDefault();
				if (subMaskAttribute != null)
				{
					string subMask = (string)setupCache.GetValue(payrollPreferences, typeof(TExpenseSubMask).Name);
					PRDimensionMaskAttribute dimensionMaskAttribute = subMaskAttribute.GetAttribute<PRDimensionMaskAttribute>();
					if (dimensionMaskAttribute != null)
					{
						return dimensionMaskAttribute.GetSegmentMaskValues(subMask).Contains(compareValue);
					}
				}
			}

			return false;
		}
	}
}
