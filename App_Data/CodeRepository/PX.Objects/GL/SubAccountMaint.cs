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
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	public class SubAccountMaint : PXGraph<SubAccountMaint>
	{
        public PXSavePerRow<Sub, Sub.subID> Save;
		public PXCancel<Sub> Cancel;
		[PXImport(typeof(Sub))]
		[PXFilterable]
		public PXSelectOrderBy<Sub, OrderBy<Asc<Sub.subCD>>> SubRecords;

		public PXSetup<Branch> Company;

		public SubAccountMaint()
		{
			viewRestrictionGroups.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.rowLevelSecurity>());
			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
		}

		#region Repository methods

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public static int? FindSubIDByCD(PXGraph graph, string subCD) => Sub.UK.Find(graph, subCD)?.SubID;

		#endregion

		public PXAction<Sub> viewRestrictionGroups;
		[PXUIField(DisplayName = Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (SubRecords.Current != null)
			{
				GLAccessBySub graph = CreateInstance<GLAccessBySub>();
				graph.Sub.Current = graph.Sub.Search<Sub.subCD>(SubRecords.Current.SubCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}
	}
}
