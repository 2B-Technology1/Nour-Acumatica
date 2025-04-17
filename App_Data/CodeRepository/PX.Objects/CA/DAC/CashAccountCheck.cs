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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.Common;

namespace PX.Objects.CA
{
	/// <summary>
	/// A service table that is used to maintain the numbers of already printed check forms
	/// to avoid double count of the used numbers. 
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.CashAccountCheck)]
	public partial class CashAccountCheck : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CashAccountCheck>.By<cashAccountID, paymentMethodID, checkNbr>
		{
			public static CashAccountCheck Find(PXGraph graph, int? cashAccountID, string paymentMethodID, string checkNbr, PKFindOptions options = PKFindOptions.None) 
				=> FindBy(graph, cashAccountID, paymentMethodID, checkNbr, options);
		}

		public static class FK
		{
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<CashAccountCheck>.By<cashAccountID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<CashAccountCheck>.By<paymentMethodID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<CashAccountCheck>.By<vendorID> { }
			public class APPayment : AP.APPayment.PK.ForeignKeyOf<CashAccountCheck>.By<docType, refNbr> { }
		}

		#endregion

		#region AccountID
		[Obsolete(InternalMessages.FieldIsObsoleteAndWillBeRemoved2023R2)]
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[PXInt]
		[Obsolete(InternalMessages.PropertyIsObsoleteAndWillBeRemoved2023R2)]
		public virtual int? AccountID
		{
			get
			{
				return this.CashAccountID;
			}
			set
			{
				this.CashAccountID = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Cash Account ID", Visible = false)]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(PaymentMethod.paymentMethodID))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountCheckID
		public abstract class cashAccountCheckID : PX.Data.BQL.BqlInt.Field<cashAccountCheckID> { }
		[PXDBIdentity]
		public virtual int? CashAccountCheckID
		{
			get;
			set;
		}
		#endregion
		#region CheckNbr
		public abstract class checkNbr : PX.Data.BQL.BqlString.Field<checkNbr> { }

		[PXDBString(40, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Check Number")]
		[PXDefault]
		[PXParent(typeof(Select<APAdjust, Where<APAdjust.stubNbr, Equal<Current<CashAccountCheck.checkNbr>>,
			And<APAdjust.paymentMethodID, Equal<Current<CashAccountCheck.paymentMethodID>>,
			And<APAdjust.cashAccountID, Equal<Current<CashAccountCheck.cashAccountID>>,
			And<APAdjust.voided, NotEqual<True>>>>>>))]
		public virtual string CheckNbr
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[APDocType.List]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[PXDBString(6, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Application Period", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[GL.FinPeriodIDFormatting]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Document Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[PXDefault]
		[Vendor]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
