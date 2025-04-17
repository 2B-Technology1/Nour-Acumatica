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

namespace PX.Objects.CR
{
	public class Quarter
	{
		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() : base(
				new int[] { 1, 2, 3, 4 },
				new string[]
				{
					PXMessages.LocalizeFormatNoPrefix(GL.Messages.QuarterDescr, 1),
					PXMessages.LocalizeFormatNoPrefix(GL.Messages.QuarterDescr, 2),
					PXMessages.LocalizeFormatNoPrefix(GL.Messages.QuarterDescr, 3),
					PXMessages.LocalizeFormatNoPrefix(GL.Messages.QuarterDescr, 4)
				})
			{
			}
		}
	}
}