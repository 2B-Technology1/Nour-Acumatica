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
using PX.Objects.CR;
using System;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.BQL;

namespace PX.Objects.Common.DAC.ReportParameters
{
	[PXHidden(ServiceVisible = true)]
	[PXProjection(typeof(Select<BAccount>), Persistent = false)]
	[Serializable]
	public partial class BAccountNoMask : IBqlTable
	{
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(BAccount.bAccountID))]
		public virtual int? BAccountID { get; set; }

		public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(BAccount.acctCD))]
		public virtual string AcctCD { get; set; }

		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		[PXDBString(255, IsUnicode = true, BqlField = typeof(BAccount.acctName))]
		public virtual string AcctName { get; set; }

		public abstract class acctReferenceNbr : PX.Data.BQL.BqlString.Field<acctReferenceNbr> { }
		[PXDBString(50, IsUnicode = true, BqlField = typeof(BAccount.acctReferenceNbr))]
		public virtual string AcctReferenceNbr { get; set; }

		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		[ParentBAccount(typeof(BAccount.bAccountID), BqlField = typeof(BAccount.parentBAccountID))]
		public virtual int? ParentBAccountID { get; set; }

		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		[PXDBInt(BqlField = typeof(BAccount.ownerID))]
		public virtual int? OwnerID { get; set; }

		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(BAccount.type))]
		public virtual string Type { get; set; }

		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		[PXDBInt(BqlField = typeof(BAccount.defContactID))]
		public virtual int? DefContactID { get; set; }

		public abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		[PXDBInt(BqlField = typeof(BAccount.defLocationID))]
		public virtual int? DefLocationID { get; set; }

		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		[PXDBInt(BqlField = typeof(BAccount.defAddressID))]
		public virtual int? DefAddressID { get; set; }

		public abstract class isBranch : PX.Data.BQL.BqlBool.Field<isBranch> { }
		[PXDBBool(BqlField = typeof(BAccount.isBranch))]
		public virtual bool? IsBranch { get; set; }

		public abstract class cOrgBAccountID : PX.Data.BQL.BqlInt.Field<cOrgBAccountID> { }
		[PXDBInt(BqlField = typeof(BAccount.cOrgBAccountID))]
		public virtual int? COrgBAccountID { get; set; }

		public abstract class vOrgBAccountID : PX.Data.BQL.BqlInt.Field<vOrgBAccountID> { }
		[PXDBInt(BqlField = typeof(BAccount.vOrgBAccountID))]
		public virtual int? VOrgBAccountID { get; set; }

		public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }
		[PXDBString(50, IsUnicode = true, BqlField = typeof(BAccount.taxRegistrationID))]
		public virtual string TaxRegistrationID { get; set; }
	}

}
