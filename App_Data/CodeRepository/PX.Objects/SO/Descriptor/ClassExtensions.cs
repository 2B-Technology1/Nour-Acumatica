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

namespace PX.Objects.SO
{
	public static class ClassExtensions
	{
		internal static void ClearPOFlags(this SOLineSplit split)
		{
			split.POCompleted = false;
			split.POCancelled = false;
			
			split.POCreate = false;
			split.POSource = null;
		}

		internal static void ClearPOReferences(this SOLineSplit split)
		{
			split.POType = null;
			split.PONbr = null;
			split.POLineNbr = null;
			
			split.POReceiptType = null;
			split.POReceiptNbr = null;
		}

		internal static void ClearSOReferences(this SOLineSplit split)
		{
			split.SOOrderType = null;
			split.SOOrderNbr = null;
			split.SOLineNbr = null;
			split.SOSplitLineNbr = null;
		}
	}
}
