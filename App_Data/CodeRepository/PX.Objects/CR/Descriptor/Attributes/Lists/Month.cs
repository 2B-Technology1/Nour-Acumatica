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
	public class Month
	{
		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() : base(
				new int[]
				{
					_Jan, _Feb, _Mar, _Apr, _May, _Jun, _Jul, _Aug, _Sep, _Oct, _Nov, _Dec
				},
				new string[]
				{
					Messages.January,
					Messages.February,
					Messages.March,
					Messages.April,
					Messages.May,
					Messages.June,
					Messages.July,
					Messages.August,
					Messages.September,
					Messages.October,
					Messages.November,
					Messages.December
				})
			{
			}

			private static int _Jan = 1;
			private static int _Feb = 2;
			private static int _Mar = 3;
			private static int _Apr = 4;
			private static int _May = 5;
			private static int _Jun = 6;
			private static int _Jul = 7;
			private static int _Aug = 8;
			private static int _Sep = 9;
			private static int _Oct = 10;
			private static int _Nov = 11;
			private static int _Dec = 12;
		}
	}
}