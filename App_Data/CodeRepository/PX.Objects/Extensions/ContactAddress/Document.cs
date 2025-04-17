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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.Extensions.ContactAddress
{
	/// <exclude/>
	public partial class Document : PXMappedCacheExtension
	{
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region DocumentContactID
		public abstract class documentContactID : PX.Data.BQL.BqlInt.Field<documentContactID> { }
		public virtual Int32? DocumentContactID { get; set; }
		#endregion

		#region DocumentAddressID
		public abstract class documentAddressID : PX.Data.BQL.BqlInt.Field<documentAddressID> { }
		public virtual Int32? DocumentAddressID { get; set; }
		#endregion

		#region ShipContactID
		public abstract class shipContactID : IBqlField { }
		public virtual Int32? ShipContactID { get; set; }
		#endregion

		#region ShipAddressID
		public abstract class shipAddressID : IBqlField { }
		public virtual Int32? ShipAddressID { get; set; }
		#endregion


		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		public virtual Int32? LocationID { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public virtual Int32? BAccountID { get; set; }
		#endregion

		#region AllowOverrideContactAddress
		public abstract class allowOverrideContactAddress : PX.Data.BQL.BqlBool.Field<allowOverrideContactAddress> { }
		public virtual bool? AllowOverrideContactAddress { get; set; }
		#endregion

		#region AllowOverrideShippingContactAddress
		public abstract class allowOverrideShippingContactAddress : IBqlField { }
		public virtual bool? AllowOverrideShippingContactAddress { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		public virtual Int32? ProjectID { get; set; }
		#endregion
	}
}
