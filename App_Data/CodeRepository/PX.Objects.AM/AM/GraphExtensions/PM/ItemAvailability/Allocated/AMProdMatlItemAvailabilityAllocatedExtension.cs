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
	// TODO: ensure this class is even needed - could project availability be used in ProdMatl graphs?
	// if yes, then the GetStatusWithAllocatedProject's meaningful implementation is missing, otherwise this class should be removed
	public abstract class AMProdMatlItemAvailabilityAllocatedProjectExtension<TGraph, TProdMatlAvailExt, TProdMatlAvailAllocExt, TProdMatlAvailProjExt> : ItemAvailabilityAllocatedProjectExtension<TGraph, TProdMatlAvailExt, TProdMatlAvailAllocExt, TProdMatlAvailProjExt, AMProdMatl, AMProdMatlSplit>
		where TGraph : PXGraph
		where TProdMatlAvailExt : AMProdMatlItemAvailabilityExtension<TGraph>
		where TProdMatlAvailAllocExt : AMProdMatlItemAvailabilityAllocatedExtension<TGraph, TProdMatlAvailExt>
		where TProdMatlAvailProjExt : AMProdMatlItemAvailabilityProjectExtension<TGraph, TProdMatlAvailExt>
	{
		protected override string GetStatusWithAllocatedProject(AMProdMatl line) => null;
	}

	// TODO: ensure this class is even needed - could project availability be used in ProdDetail?
	public class ProdDetail_MatlItemAvailabilityAllocatedProjectExtension : AMProdMatlItemAvailabilityAllocatedProjectExtension<
		ProdDetail,
		ProdDetail.MatlItemAvailabilityExtension,
		ProdDetail.MatlItemAvailabilityAllocatedExtension,
		ProdDetail_MatlItemAvailabilityProjectExtension>
	{
		public static bool IsActive() => UseProjectAvailability;
	}

	public class SortProdDetailMatl : SortExtensionsBy<ExtensionOrderFor<ProdDetail>.FilledWith<
		ProdDetail_MatlItemAvailabilityProjectExtension,
		ProdDetail.MatlItemAvailabilityAllocatedExtension>>
	{ }

	// TODO: ensure this class is even needed - could project availability be used in ProdMaint?
	public class ProdMaint_MatlItemAvailabilityAllocatedProjectExtension : AMProdMatlItemAvailabilityAllocatedProjectExtension<
		ProdMaint,
		ProdMaint.MatlItemAvailabilityExtension,
		ProdMaint.MatlItemAvailabilityAllocatedExtension,
		ProdMaint_MatlItemAvailabilityProjectExtension>
	{
		public static bool IsActive() => UseProjectAvailability;
	}

	public class SortProdMaintMatl : SortExtensionsBy<ExtensionOrderFor<ProdMaint>.FilledWith<
		ProdMaint_MatlItemAvailabilityProjectExtension,
		ProdMaint.MatlItemAvailabilityAllocatedExtension>>
	{ }

	// TODO: ensure this class is even needed - could project availability be used in ProductionScheduleEngine?
	public class ProductionScheduleEngine_MatlItemAvailabilityAllocatedProjectExtension : AMProdMatlItemAvailabilityAllocatedProjectExtension<
		ProductionScheduleEngine,
		ProductionScheduleEngine.MatlItemAvailabilityExtension,
		ProductionScheduleEngine.MatlItemAvailabilityAllocatedExtension,
		ProductionScheduleEngine_MatlItemAvailabilityProjectExtension>
	{
		public static bool IsActive() => UseProjectAvailability;
	}

	public class SortProductionScheduleEngine : SortExtensionsBy<ExtensionOrderFor<ProductionScheduleEngine>.FilledWith<
		ProductionScheduleEngine_MatlItemAvailabilityProjectExtension,
		ProductionScheduleEngine.MatlItemAvailabilityAllocatedExtension>>
	{ }
}
