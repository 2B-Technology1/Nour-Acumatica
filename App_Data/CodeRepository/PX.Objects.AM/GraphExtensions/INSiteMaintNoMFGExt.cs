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
using PX.Data.BQL.Fluent;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
	// WORK AROUND FOR AC-244146 / AC-243737. Once resolved this extension will be removed.
	public class INSiteMaintNoMFGExt : PXGraphExtension<INSiteMaint>
	{
		public static bool IsActive()
		{
			return !Features.ManufacturingOrDRPOrReplenishmentEnabled();
		}

		public override void Initialize()
		{
			base.Initialize();
			AMLeadTimes.AllowSelect = false;
		}

		[PXCopyPasteHiddenView]
		[PXHidden]
		[PXImport(typeof(INSite))]
		public SelectFrom<AMSiteTransfer>.View AMLeadTimes;
	}
}
