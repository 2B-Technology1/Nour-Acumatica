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
using PX.Data.BQL;
using PX.Objects.CM;

namespace PX.Objects.CR.CRMarketingListMaint_Extensions
{
	[PXHidden]
	public class AddMembersFromGIFilter : IBqlTable
	{
		#region GIDesignID

		// Acuminator disable once PX1030 PXDefaultIncorrectUse [required filter]]
		[PXGuid]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Generic Inquiry")]
		[ContactGISelector]
		public Guid? GIDesignID { get; set; }
		public abstract class gIDesignID : BqlGuid.Field<CRMarketingList.gIDesignID> { }

		#endregion

		#region SharedGIFilter

		[PXGuid]
		[PXUIField(DisplayName = "Shared Filter")]
		[FilterList(typeof(AddMembersFromGIFilter.gIDesignID), IsSiteMapIdentityScreenID = false, IsSiteMapIdentityGIDesignID = true)]
		[PXFormula(typeof(Default<AddMembersFromGIFilter.gIDesignID>))]
		public virtual Guid? SharedGIFilter { get; set; }
		public abstract class sharedGIFilter : BqlGuid.Field<CRMarketingList.sharedGIFilter> { }

		#endregion
	}
}
