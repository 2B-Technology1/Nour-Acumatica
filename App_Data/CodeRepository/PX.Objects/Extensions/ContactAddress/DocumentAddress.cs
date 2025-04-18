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

namespace PX.Objects.Extensions.ContactAddress
{
	public partial class DocumentAddress : PXMappedCacheExtension
	{
        #region IsDefaultAddress
        public abstract class isDefaultAddress : PX.Data.BQL.BqlBool.Field<isDefaultAddress> { }
        public virtual Boolean? IsDefaultAddress { get; set; }
        #endregion
        #region OverrideAddress
        public abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }
		public virtual Boolean? OverrideAddress { get; set; }
		#endregion

		#region IsValidated
		public abstract class isValidated : IBqlField { }
		public virtual Boolean? IsValidated { get; set; }
		#endregion

		#region AddressLine1
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
		public virtual String AddressLine1 { get; set; }
		#endregion

		#region AddressLine2
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
		public virtual String AddressLine2 { get; set; }
		#endregion

		#region AddressLine3
		public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
		public virtual String AddressLine3 { get; set; }
		#endregion

		#region City
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		public virtual String City { get; set; }
		#endregion

		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		public virtual String CountryID { get; set; }
		#endregion

		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		public virtual String State { get; set; }
		#endregion

		#region PostalCode
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		public virtual String PostalCode { get; set; }
		#endregion
	}
}
