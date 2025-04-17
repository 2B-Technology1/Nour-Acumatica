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
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class DimensionMaintExt : PXGraphExtension<DimensionMaint>
	{
		public PXSelect<Dimension, Where<Dimension.dimensionID, InFieldClassActivated,
			Or<Dimension.dimensionID, IsNull,
			Or<Dimension.dimensionID, Equal<PM.ProjectAttribute.dimension>,
			Or<Dimension.dimensionID, Equal<PM.ProjectAttribute.dimensionTM>,
			Or<Dimension.dimensionID, Equal<CT.ContractAttribute.dimension>,
			Or<Dimension.dimensionID, Equal<CT.ContractTemplateAttribute.dimension>>>>>>>> Header;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search<Dimension.dimensionID, Where<Dimension.dimensionID, InFieldClassActivated,
			Or<Dimension.dimensionID, Equal<PM.ProjectAttribute.dimension>,
			Or<Dimension.dimensionID, Equal<PM.ProjectAttribute.dimensionTM>,
			Or<Dimension.dimensionID, Equal<CT.ContractAttribute.dimension>,
			Or<Dimension.dimensionID, Equal<CT.ContractTemplateAttribute.dimension>>>>>>>))]
		protected virtual void _(Events.CacheAttached<Dimension.dimensionID> e) { }

		[PXOverride]
		public virtual IEnumerable GetSimpleDetails(Dimension dim)
		{
			var select = new PXSelect<Segment,
					Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>,
					And<Where<Segment.dimensionID, InFieldClassActivated,
					Or<Segment.dimensionID, Equal<PM.ProjectAttribute.dimension>,
					Or<Segment.dimensionID, Equal<PM.ProjectAttribute.dimensionTM>,
					Or<Segment.dimensionID, Equal<CT.ContractAttribute.dimension>,
					Or<Segment.dimensionID, Equal<CT.ContractTemplateAttribute.dimension>>>>>>>>>(Base);

			foreach (Segment item in select.Select(dim.DimensionID))
			{
				yield return item;
			}
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>() || 
				PXAccess.FeatureInstalled<FeaturesSet.contractManagement>();
		}
	}
}
