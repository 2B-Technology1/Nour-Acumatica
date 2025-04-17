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

namespace PX.Objects.PR
{
	public class PXPrimaryDacOuterException : PXOuterException
	{
		public PXPrimaryDacOuterException(PXOuterException oe, PXCache cache, Type primaryDac) : base(
			new Dictionary<string, string>(),
			oe.GraphType,
			oe.Row,
			oe.Message)
		{
			// If any error message exists for primaryDac, or other unrelated DAC's, discard messages from DAC's inherited by primaryDac
			for (int i = 0; i < oe.InnerFields.Length; i++)
			{
				Type primaryDacField = cache.GetBqlField(oe.InnerFields[i]);
				if (primaryDacField == null || primaryDacField.DeclaringType == primaryDac)
				{
					_InnerExceptions[oe.InnerFields[i]] = oe.InnerMessages[i];
				}
			}

			// If previous filtering cleared all errors, restore original errors
			if (!_InnerExceptions.Any())
			{
				for (int i = 0; i < oe.InnerFields.Length; i++)
				{
					_InnerExceptions[oe.InnerFields[i]] = oe.InnerMessages[i];
				}
			}
		}
	}
}
