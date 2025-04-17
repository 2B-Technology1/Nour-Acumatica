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
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using System.Collections.Generic;
using PX.Commerce.Core;
using PX.Objects.CA;
using PX.Objects.CR;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// DAC Extension of BCBinding to add additional properties.
	/// </summary>
	[Serializable]
	[PXNonInstantiatedExtension]
	public sealed class BCBindingCommerce : PXCacheExtension<BCBinding>
	{
		public static bool IsActive() { return true; }

		#region Keys
		public static class FK
		{
			public class BindingsBranch : Branch.PK.ForeignKeyOf<BCBinding>.By<branchID> { }
		}
		#endregion

		#region BranchID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [Need to check if it empty while Persisting]
		/// <summary>
		/// The branch associated with binding.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[Branch(typeof(AccessInfo.branchID))]
		[PXDefault(typeof(AccessInfo.branchID))]
		public int? BranchID { get; set; }
		/// <inheritdoc cref="BranchID"/>
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
	}
}
