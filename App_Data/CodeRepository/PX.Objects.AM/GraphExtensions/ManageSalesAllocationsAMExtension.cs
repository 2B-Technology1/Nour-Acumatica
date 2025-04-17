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
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.SO;

namespace PX.Objects.AM.GraphExtensions
{
	public class ManageSalesAllocationsAMExtension: PXGraphExtension<ManageSalesAllocations>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();

		/// <summary>
		/// Overrides <see cref="ManageSalesAllocations.CreateBaseQuery(SalesAllocationsFilter, List{object})"/>
		/// </summary>
		[PXOverride]
		public PXSelectBase<SalesAllocation> CreateBaseQuery(SalesAllocationsFilter filter, List<object> parameters, Func<SalesAllocationsFilter, List<object>, PXSelectBase<SalesAllocation>> baseImpl)
		{
			var query = baseImpl(filter, parameters);

			query.WhereAnd<Where<SalesAllocationExt.aMProdCreate.IsNull.Or<SalesAllocationExt.aMProdCreate.IsNotEqual<True>>>>();

			return query;
		}
	}

	/// <exclude/>
	public sealed class SalesAllocationExt: PXCacheExtension<SalesAllocation>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();

		#region AMProdCreate
		[PXDBBool(BqlField = typeof(SOLineExt.aMProdCreate))]
		public bool? AMProdCreate { get; set; }
		public abstract class aMProdCreate : BqlBool.Field<aMProdCreate> { }
		#endregion
	}
}
