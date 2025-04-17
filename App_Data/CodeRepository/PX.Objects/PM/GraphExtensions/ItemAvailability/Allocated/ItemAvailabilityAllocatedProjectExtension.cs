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

using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;
using PX.Objects.SO.GraphExtensions;

namespace PX.Objects.PM.MaterialManagement.GraphExtensions.ItemAvailability.Allocated
{
	public abstract class ItemAvailabilityAllocatedProjectExtension<TGraph, TItemAvailExt, TItemAvailAllocExt, TItemAvailProjExt, TLine, TSplit> : PXGraphExtension<TItemAvailProjExt, TItemAvailAllocExt, TItemAvailExt, TGraph>
		where TGraph : PXGraph
		where TItemAvailExt : ItemAvailabilityExtension<TGraph, TLine, TSplit>
		where TItemAvailAllocExt : ItemAvailabilityAllocatedExtension<TGraph, TItemAvailExt, TLine, TSplit>
		where TItemAvailProjExt : ItemAvailabilityProjectExtension<TGraph, TItemAvailExt, TLine, TSplit>
		where TLine : class, IBqlTable, ILSPrimary, new()
		where TSplit : class, IBqlTable, ILSDetail, new()
	{
		protected static bool UseProjectAvailability => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();

		protected TItemAvailExt ItemAvailBase => Base1;
		protected TItemAvailAllocExt ItemAvailAllocBase => Base2;
		protected TItemAvailProjExt ItemAvailProjExt => Base3;

		/// Overrides <see cref="ItemAvailabilityProjectExtension{TGraph, TItemAvailExt, TLine, TSplit}.GetStatusProject(TLine)"/>
		[PXOverride]
		public virtual string GetStatusProject(TLine line,
			Func<TLine, string> base_GetStatusProject)
		{
			if (Base2.IsAllocationEntryEnabled)
				return GetStatusWithAllocatedProject(line) ?? base_GetStatusProject(line);
			else
				return base_GetStatusProject(line);
		}

		/// Overrides <see cref="ItemAvailabilityAllocatedExtension{TGraph, TItemAvailExt, TLine, TSplit}.GetStatusWithAllocated(TLine)"/>
		[PXOverride]
		public virtual string GetStatusWithAllocated(TLine line,
			Func<TLine, string> base_GetStatusWithAllocated)
		{
			if (UseProjectAvailability)
				return GetStatusWithAllocatedProject(line) ?? base_GetStatusWithAllocated(line);
			else
				return base_GetStatusWithAllocated(line);
		}

		protected abstract string GetStatusWithAllocatedProject(TLine line);
	}
}
