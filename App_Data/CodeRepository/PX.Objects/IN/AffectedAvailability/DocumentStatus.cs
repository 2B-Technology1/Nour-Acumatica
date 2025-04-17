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

using System.Linq;
using PX.Data;

namespace PX.Objects.IN.AffectedAvailability
{
	public class DocumentStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public static readonly (string, string)[] ValuesToLabels =
				SO.SOOrderStatus.ListAttribute.ValuesToLabels.Select(p => (EntityType.SOOrder + p.Item1, p.Item2))
				.Union(SO.SOShipmentStatus.ListAttribute.ValuesToLabels.Select(p => (EntityType.SOShipment + p.Item1, p.Item2)))
				.Union(INDocStatus.ListAttribute.ValuesToLabels.Select(p => (EntityType.INRegister + p.Item1, p.Item2)))
				.ToArray();

			public ListAttribute() : base(ValuesToLabels) { }
		}
	}
}
