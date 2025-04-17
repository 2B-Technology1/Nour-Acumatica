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

namespace PX.Objects.PM.MaterialManagement.GraphExtensions.ItemAvailability
{
	// TODO: ensure this class is even needed - could project availability be used in ProdItem graphs?
	// if yes, then the GetStatusProject's meaningful implementation is missing, otherwise this class should be removed
	public abstract class AMProdItemAvailabilityProjectExtension<TGraph, TProdItemAvailExt> : ItemAvailabilityProjectExtension<TGraph, TProdItemAvailExt, AMProdItem, AMProdItemSplit>
		where TGraph : PXGraph
		where TProdItemAvailExt : AMProdItemAvailabilityExtension<TGraph>
	{
		protected override string GetStatusProject(AMProdItem line) => null;
	}

	// TODO: ensure this class is even needed - could project availability be used in ProdDetail?
	[PXProtectedAccess(typeof(ProdDetail.ItemAvailabilityExtension))]
	public abstract class ProdDetail_ItemAvailabilityProjectExtension
		: AMProdItemAvailabilityProjectExtension<ProdDetail, ProdDetail.ItemAvailabilityExtension>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
	}

	// TODO: ensure this class is even needed - could project availability be used in ProdMaint?
	[PXProtectedAccess(typeof(ProdMaint.ItemAvailabilityExtension))]
	public abstract class ProdMaint_ItemAvailabilityProjectExtension
		: AMProdItemAvailabilityProjectExtension<ProdMaint, ProdMaint.ItemAvailabilityExtension>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
	}
}
