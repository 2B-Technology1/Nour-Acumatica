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
using PX.Objects.GL;
using PX.Data.Update;
using PX.Objects.AR;

namespace PX.Objects.CA
{
	///<summary>
	/// Contains credit cards data loaded by service
	///</summary>
	[System.SerializableAttribute()]
	public class CCSynchronizeCard : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CCSynchronizeCard>.By<recordID>
		{
			public static CCSynchronizeCard Find(PXGraph graph, int? recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
		}

		public static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<CCSynchronizeCard>.By<bAccountID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<CCSynchronizeCard>.By<paymentMethodID> { }
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<CCSynchronizeCard>.By<cashAccountID> { }
			public class ProcessingCenter : CCProcessingCenter.PK.ForeignKeyOf<CCSynchronizeCard>.By<cCProcessingCenterID> { }
			public class ProcessingCenterPaymentMethod : CCProcessingCenterPmntMethod.PK.ForeignKeyOf<CCSynchronizeCard>.By<cCProcessingCenterID, paymentMethodID> { }
		}
		#endregion

		#region RecordID
		public abstract class recordID : PX.Data.IBqlField
		{
		}
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual int? RecordID { get; set; }
		#endregion

		#region CCProcessingCenterID
		public abstract class cCProcessingCenterID : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName =  "Proc. Center ID", Enabled = false)]
		public virtual string CCProcessingCenterID { get; set; }
		#endregion

		#region CustomerCCPID
		public abstract class customerCCPID : PX.Data.IBqlField
		{
		}
		[PXDBString(1024, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Proc. Center Cust. Profile ID", Enabled = false)]
		public virtual string CustomerCCPID { get; set; }
		#endregion

		#region CustomerCCPIDHash
		public abstract class customerCCPIDHash : PX.Data.IBqlField
		{
		}
		[PXDBString(1024, IsUnicode = true)]
		[PXDefault]
		public virtual string CustomerCCPIDHash { get; set; }
		#endregion
		
		#region PaymentCCPID
		public abstract class paymentCCPID : PX.Data.IBqlField
		{
		}
		[PXRSACryptString(1024, IsUnicode = true, IsViewDecrypted = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Proc. Center Payment Profile ID", Enabled = false)]
		public virtual string PaymentCCPID { get; set; }
		#endregion
		
		#region PCCustomerID
		public abstract class pCCustomerID : PX.Data.IBqlField
		{
		}
		[PXDBString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center Cust. ID", Enabled = false)]
		public virtual string PCCustomerID { get; set; }
		#endregion
		
		#region PCCustomerDescription
		public abstract class pCCustomerDescription : PX.Data.IBqlField
		{
		}
		[PXDBString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center Cust. Descr.", Enabled = false)]
		public virtual string PCCustomerDescription { get;set; }
		#endregion
		
		#region PCCustomerEmail
		public abstract class pCCustomerEmail : PX.Data.IBqlField
		{
		}
		[PXRSACryptString(1024, IsUnicode = true, IsViewDecrypted = true)]
		[PXUIField(DisplayName = "Proc. Center Cust. Email", Enabled = false)]
		public virtual string PCCustomerEmail {get; set; }
		#endregion

		#region CardType
		public abstract class cardType : PX.Data.BQL.BqlString.Field<cardType> { }

		/// <summary>
		/// Type of a card associated with the card. 
		/// </summary>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Card Type", Enabled = false)]
		[CardType.List]
		public virtual string CardType
		{
			get;
			set;
		}
		#endregion

		#region ProcCenterCardTypeCode
		public abstract class procCenterCardTypeCode : PX.Data.BQL.BqlString.Field<procCenterCardTypeCode> { }

		/// <summary>
		/// Original card type value received from the processing center.
		/// </summary>
		[PXDBString(25, IsFixed = true)] // should it be longer than 10 ? 
		[PXUIField(DisplayName = "Proc. Center Card Type", Enabled = false)]
		public virtual string ProcCenterCardTypeCode
		{
			get;
			set;
		}
		#endregion

		#region CardNumber
		public abstract class cardNumber : PX.Data.IBqlField
		{
		}
		[PXDBString(1024, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Card Number")]
		public virtual string CardNumber { get; set; }
		#endregion
		
		#region ExpirationDate
		public abstract class expirationDate : PX.Data.IBqlField
		{
		}
		[PXDBDateString(DateFormat = "MM/yy")]
		[PXUIField(DisplayName = "Expiration Date")]
		public virtual DateTime? ExpirationDate { get; set; }
		#endregion
		
		#region FirstName
		public abstract class firstName : PX.Data.IBqlField
		{
		}
		[PXRSACryptString(1024, IsUnicode = true, IsViewDecrypted = true)]
		[PXUIField(DisplayName = "Proc. Center Payment Profile First Name", Enabled = false)]
		public virtual string FirstName { get;set; }
		#endregion
		
		#region LastName
		public abstract class lastName : PX.Data.IBqlField
		{
		}
		[PXRSACryptString(1024, IsUnicode = true, IsViewDecrypted = true)]
		[PXUIField(DisplayName = "Proc. Center Payment Profile Last Name", Enabled = false)]
		public virtual string LastName { get;set; }
		#endregion
		
		#region BAccountID
		public abstract class bAccountID : PX.Data.IBqlField
		{
		}
		[Customer(typeof(Search<Customer.bAccountID>),typeof(Customer.acctCD), typeof(Customer.acctName), DisplayName = "Customer ID")]
		public virtual int? BAccountID{ get; set; }
		#endregion

		#region
		public abstract class paymentType : PX.Data.IBqlField
		{
		}
		[PXDBString(3, IsFixed = true)]
		[PXDefault(PaymentMethodType.CashOrCheck, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PaymentMethodType.List]
		[PXUIField(DisplayName = "Means of Payment", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string PaymentType { get; set; }
		#endregion

		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search5<CCProcessingCenterPmntMethod.paymentMethodID,
			InnerJoin<PaymentMethod, On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
				And<PaymentMethod.paymentType, Equal<Current<CCSynchronizeCard.paymentType>>>>>,
				Where<CCProcessingCenterPmntMethod.processingCenterID, Equal<Current<CCSynchronizeCard.cCProcessingCenterID>>,
					And<PaymentMethod.isActive, Equal<True>, And<CCProcessingCenterPmntMethod.isActive, Equal<True>, And<PaymentMethod.useForAR, Equal<True>>>>>,
				Aggregate<GroupBy<CCProcessingCenterPmntMethod.paymentMethodID>>>), typeof(CCProcessingCenterPmntMethod.paymentMethodID), typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "Payment Method")]
		public virtual string PaymentMethodID { get; set; }
		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.IBqlField
		{
		}
		[CashAccount(null, typeof(Search2<CashAccount.cashAccountID, InnerJoin<PaymentMethodAccount,
			On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
			And<PaymentMethodAccount.paymentMethodID, Equal<Current<CCSynchronizeCard.paymentMethodID>>,
			And<PaymentMethodAccount.useForAR,Equal<True>>>>>,
			Where<Match<Current<AccessInfo.userName>>>>))]
		[PXUIField(DisplayName = "Cash Account")]
		public virtual int? CashAccountID{ get; set; }
		#endregion
		
		#region Imported
		public abstract class imported : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		public virtual bool? Imported { get; set; }
		#endregion
		
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		
		#region Selected
		public abstract class selected : PX.Data.IBqlField { }
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion
		
		#region NoteID 
		public abstract class noteID : PX.Data.IBqlField { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		public static string GetSha1HashString(string input)
		{
			byte[] hash = PXCriptoHelper.CalculateSHA(input);
			string ret = string.Empty;

			foreach (var b in hash)
			{
				ret += b.ToString("X2");
			}
			return ret;
		}
	}
}
