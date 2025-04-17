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
using PX.Objects.FS;

namespace PX.Objects.PM.MaterialManagement.GraphExtensions.ItemAvailability.Allocated
{
	// TODO: ensure this class is even needed - could project availability be used in ServiceOrderEntry?
	// if yes, then the GetStatusWithAllocatedProject's meaningful implementation is missing, otherwise this class should be removed
	public class FSServiceOrderItemAvailabilityAllocatedProjectExtension : ItemAvailabilityAllocatedProjectExtension<
		ServiceOrderEntry,
		FSServiceOrderItemAvailabilityExtension,
		FSServiceOrderItemAvailabilityAllocatedExtension,
		FSServiceOrderItemAvailabilityProjectExtension,
		FSSODet, FSSODetSplit>
	{
		public static bool IsActive() => UseProjectAvailability;
		protected override string GetStatusWithAllocatedProject(FSSODet line) => null;
	}

	public class SortServiceOrderEntry : SortExtensionsBy<ExtensionOrderFor<ServiceOrderEntry>.FilledWith<
		FSServiceOrderItemAvailabilityProjectExtension,
		FSServiceOrderItemAvailabilityAllocatedExtension>>
	{ }
}
