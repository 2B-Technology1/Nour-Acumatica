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
using PX.Objects.AM;

namespace PX.Objects.PM.MaterialManagement.GraphExtensions.ItemAvailability.Allocated
{
	// TODO: ensure this class is even needed - could project availability be used in ProdItem graphs?
	// if yes, then the GetStatusWithAllocatedProject's meaningful implementation is missing, otherwise this class should be removed
	public abstract class AMProdItemAvailabilityAllocatedProjectExtension<TGraph, TProdItemAvailExt, TProdItemAvailAllocExt, TProdItemAvailProjExt> : ItemAvailabilityAllocatedProjectExtension<TGraph, TProdItemAvailExt, TProdItemAvailAllocExt, TProdItemAvailProjExt, AMProdItem, AMProdItemSplit>
		where TGraph : PXGraph
		where TProdItemAvailExt : AMProdItemAvailabilityExtension<TGraph>
		where TProdItemAvailAllocExt : AMProdItemAvailabilityAllocatedExtension<TGraph, TProdItemAvailExt>
		where TProdItemAvailProjExt : AMProdItemAvailabilityProjectExtension<TGraph, TProdItemAvailExt>
	{
		protected override string GetStatusWithAllocatedProject(AMProdItem line) => null;
	}

	// TODO: ensure this class is even needed - could project availability be used in ProdDetail?
	public class ProdDetail_ItemAvailabilityAllocatedProjectExtension : AMProdItemAvailabilityAllocatedProjectExtension<
		ProdDetail,
		ProdDetail.ItemAvailabilityExtension,
		ProdDetail.ItemAvailabilityAllocatedExtension,
		ProdDetail_ItemAvailabilityProjectExtension>
	{
		public static bool IsActive() => UseProjectAvailability;
	}

	public class SortProdDetail : SortExtensionsBy<ExtensionOrderFor<ProdDetail>.FilledWith<
		ProdDetail_ItemAvailabilityProjectExtension,
		ProdDetail.ItemAvailabilityAllocatedExtension>> { }

	// TODO: ensure this class is even needed - could project availability be used in ProdMaint?
	public class ProdMaint_ItemAvailabilityAllocatedProjectExtension
		: AMProdItemAvailabilityAllocatedProjectExtension<
			ProdMaint,
			ProdMaint.ItemAvailabilityExtension,
			ProdMaint.ItemAvailabilityAllocatedExtension,
			ProdMaint_ItemAvailabilityProjectExtension>
	{
		public static bool IsActive() => UseProjectAvailability;
	}

	public class SortProdMaint : SortExtensionsBy<ExtensionOrderFor<ProdMaint>.FilledWith<
		ProdMaint_ItemAvailabilityProjectExtension,
		ProdMaint.ItemAvailabilityAllocatedExtension>>
	{ }
}
