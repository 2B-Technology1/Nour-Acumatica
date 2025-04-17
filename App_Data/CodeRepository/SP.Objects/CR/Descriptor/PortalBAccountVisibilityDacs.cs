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
using PX.Common;
using PX.Data;
using PX.Data.BQL;

namespace SP.Objects.CR.PortalBAccountVisibilityDacs
{
	// readonly
	[PXInternalUseOnly]
	[PXHidden]
	public class BAccount : IBqlTable
	{
		[PXDBInt]
		public virtual int? BAccountID { get; set; }
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }

		[PXDBInt]
		public virtual int? ParentBAccountID { get; set; }
		public abstract class parentBAccountID : BqlInt.Field<parentBAccountID> {}

		[PXDBBool]
		public virtual bool? DeletedDatabaseRecord { get; set; }
		public abstract class deletedDatabaseRecord : BqlBool.Field<deletedDatabaseRecord> {}

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> {}
	}

	// readonly
	[PXInternalUseOnly]
	[PXHidden]
	public class Contact : IBqlTable
	{
		[PXDBInt]
		public virtual int? ContactID { get; set; }
		public abstract class contactID : BqlInt.Field<contactID> {}

		[PXDBGuid]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : BqlGuid.Field<userID> {}

		[PXDBInt]
		public virtual int? BAccountID { get; set; }
		public abstract class bAccountID : BqlInt.Field<bAccountID> {}

		[PXDBBool]
		public virtual bool? DeletedDatabaseRecord { get; set; }
		public abstract class deletedDatabaseRecord : BqlBool.Field<deletedDatabaseRecord> {}

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> {}
	}
}
