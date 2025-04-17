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

using PX.Commerce.Core;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// A location in an external system.
	/// </summary>
	[Serializable]
	[PXCacheName("BCExternLocations")]
	public class BCExternLocations : IBqlTable
	{
		#region BindingID
		/// <summary>
		/// The ID of the binding that connects the local system to the external system.
		/// </summary>
		[PXInt()]
		[PXDBDefault(typeof(BCBinding.bindingID))]
		public virtual int? BindingID { get; set; }
		/// <inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion

		#region ExternLocationName
		/// <summary>
		/// The name of the external location.
		/// </summary>
		[PXString()]
		public virtual string ExternLocationName { get; set; }
		/// <inheritdoc cref="ExternLocationName"/>
		public abstract class externLocationName : PX.Data.BQL.BqlString.Field<externLocationName> { }
		#endregion

		#region ExternLocationValue
		/// <summary>
		/// An identifier or value representing the location in the external system.
		/// </summary>
		[PXDBString(IsKey = true)]
		[PXDefault()]
		public virtual string ExternLocationValue { get; set; }
		/// <inheritdoc cref="ExternLocationValue"/>
		public abstract class externLocationValue : PX.Data.BQL.BqlString.Field<externLocationValue> { }
		#endregion

		#region Active
		/// <summary>
		/// Indicates if the location is active.
		/// </summary>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Active { get; set; }
		/// <inheritdoc cref="Active"/>
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
	}
}
