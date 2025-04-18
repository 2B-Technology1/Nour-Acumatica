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
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CA
{
	[Serializable]
	public partial class PaymentInfo : IBqlTable
	{
		public class PK : PrimaryKeyOf<PaymentInfo>.By<module, docType, refNbr>
		{
			public static PaymentInfo Find(PXGraph graph, string module, string docType, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, module, docType, refNbr, options);
		}

		public static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<PaymentInfo>.By<bAccountID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<PaymentInfo>.By<bAccountID> { }
			public class Location : CR.Location.PK.ForeignKeyOf<PaymentInfo>.By<bAccountID, locationID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<PaymentInfo>.By<paymentMethodID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<PaymentInfo>.By<curyID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<PaymentInfo>.By<curyInfoID> { }
			public class CashAccountDeposit : CA.CADeposit.PK.ForeignKeyOf<PaymentInfo>.By<depositType, depositNbr> { }
			public class CustomerPaymentMethod : AR.CustomerPaymentMethod.PK.ForeignKeyOf<PaymentInfo>.By<pMInstanceID> { }
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<PaymentInfo>.By<cashAccountID> { }
		}

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module>
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Doc. Module", Visibility = PXUIVisibility.Visible, Visible = true)]
		public virtual string Module
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType>
		{
		}


		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(AR.Standalone.ARPayment.docType))]
		[CAAPARTranType.ListByModule(typeof(module))]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
		}

		[PXDBString(15, IsKey = true, InputMask = "", BqlField = typeof(AR.Standalone.ARPayment.refNbr))]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID>
		{
		}

		[PXDBInt]
		[PXSelector(typeof(BAccountR.bAccountID), SubstituteKey = typeof(BAccountR.acctCD), DescriptionField = typeof(BAccountR.acctName), CacheGlobal = true)]
		[PXUIField(DisplayName = "Customer/Vendor", Enabled = true, Visible = true)]
		public virtual int? BAccountID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID>
		{
		}

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<PaymentInfo.bAccountID>>>), DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		[PXUIField(DisplayName = "Location")]
		public virtual int? LocationID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID>
		{
		}

		[PXDBString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "Payment Method", Visible = true)]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr>
		{
		}

		[PXDBString(40, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.Visible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
		}

		[PXDBString(1, IsFixed = true)]
		[PXDefault(APDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[CADepositDetailsStatus.List]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate>
		{
		}

		[PXDBDate]
		[PXUIField(DisplayName = "Payment Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID>
		{
		}

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visible = true, Enabled = false)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID>
		{
		}

		[PXDBLong]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt>
		{
		}

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(PaymentInfo.curyID))]
		[PXUIField(DisplayName = "Payment Amount", Visibility = PXUIVisibility.SelectorVisible)]
		[PXParent(typeof(Select<CADepositEntry.PaymentFilter>), UseCurrent = true)]
		[PXUnboundFormula(typeof(Switch<Case<Where<PaymentInfo.selected, Equal<True>>, PaymentInfo.curyOrigAmtSigned>, decimal0>), typeof(SumCalc<CADepositEntry.PaymentFilter.selectionTotal>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<PaymentInfo.selected, Equal<True>>, int1>, int0>), typeof(SumCalc<CADepositEntry.PaymentFilter.numberOfDocuments>))]
		public virtual decimal? CuryOrigDocAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt>
		{
		}

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigDocAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryChargeTotal
		public abstract class curyChargeTotal : PX.Data.BQL.BqlDecimal.Field<curyChargeTotal>
		{
		}

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(PaymentInfo.curyID))]
		[PXUIField(DisplayName = "Charge Amount", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryChargeTotal
		{
			get;
			set;
		}
		#endregion
		#region ChargeTotal
		public abstract class chargeTotal : PX.Data.BQL.BqlDecimal.Field<chargeTotal>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ChargeTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryGrossPaymentAmount
		public abstract class curyGrossPaymentAmount : PX.Data.BQL.BqlDecimal.Field<curyGrossPaymentAmount>
		{
		}

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(PaymentInfo.curyID))]
		[PXUIField(DisplayName = "Gross Payment Amount", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryGrossPaymentAmount
		{
			get;
			set;
		}
		#endregion
		#region GrossPaymentAmount
		public abstract class grossPaymentAmount : PX.Data.BQL.BqlDecimal.Field<grossPaymentAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? GrossPaymentAmount
		{
			get;
			set;
		}
		#endregion
		#region DepositAfter
		public abstract class depositAfter : PX.Data.BQL.BqlDateTime.Field<depositAfter>
		{
		}

		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deposit After", Enabled = false, Visible = false)]
		public virtual DateTime? DepositAfter
		{
			get;
			set;
		}
		#endregion
		#region DepositType
		public abstract class depositType : PX.Data.BQL.BqlString.Field<depositType>
		{
		}

		[PXDBString(3, IsFixed = true, BqlField = typeof(AR.Standalone.ARPayment.depositType))]
		public virtual string DepositType
		{
			get;
			set;
		}
		#endregion
		#region DepositNbr
		public abstract class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr>
		{
		}

		[PXDBString(15, IsUnicode = true, BqlField = typeof(AR.Standalone.ARPayment.depositNbr))]
		[PXUIField(DisplayName = "Batch Deposit Nbr.", Enabled = false)]
		public virtual string DepositNbr
		{
			get;
			set;
		}
		#endregion
		#region Deposited
		public abstract class deposited : PX.Data.BQL.BqlBool.Field<deposited>
		{
		}

		[PXDBBool(BqlField = typeof(AR.Standalone.ARPayment.deposited))]
		[PXDefault(false)]
		public virtual bool? Deposited
		{
			get;
			set;
		}
		#endregion
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID>
		{
		}

		[PXDBInt]
		[PXUIField(DisplayName = "Card/Account No")]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
		public virtual int? PMInstanceID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID>
		{
		}

		[CashAccount(Visibility = PXUIVisibility.Visible)]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }

		[PXDBString(1, IsFixed = true)]
		public virtual string DrCr
		{
			get;
			set;
		}
		#endregion
		#region OrigDocSign
		public abstract class origDocSign : PX.Data.BQL.BqlDecimal.Field<origDocSign> { }
		/// <summary>
		/// The sign of the amount.
		/// </summary>
		public decimal? OrigDocSign
		{
			[PXDependsOnFields(typeof(drCr))]
			get
			{
				return (this.DrCr == CADrCr.CACredit) ? decimal.MinusOne : decimal.One;
			}
		}
		#endregion
		#region CuryOrigAmtSigned

		public abstract class curyOrigAmtSigned : PX.Data.BQL.BqlDecimal.Field<curyOrigAmtSigned> { }
		/// <summary>
		/// The signed amount of the original payment document in the selected currency.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? CuryOrigAmtSigned
		{
			[PXDependsOnFields(typeof(curyOrigDocAmt), typeof(origDocSign))]
			get
			{
				return this.CuryOrigDocAmt * this.OrigDocSign;
			}
		}
		#endregion
	}

	public class CADepositDetailsStatus
	{
		public static readonly string[] Values =
		{
			Hold,
			Balanced,
			Voided,
			Scheduled,
			Open,
			Closed,
			Printed,
			Prebooked,
			PendingApproval,
			Rejected,
			Reserved,
			PendingPrint,
			Released
		};
		public static readonly string[] Labels =
		{
			AP.Messages.Hold,
			AP.Messages.Balanced,
			AP.Messages.Voided,
			AP.Messages.Scheduled,
			AP.Messages.Open,
			AP.Messages.Closed,
			AP.Messages.Printed,
			AP.Messages.Prebooked,
			AP.Messages.PendingApproval,
			AP.Messages.Rejected,
			AP.Messages.Reserved,
			AP.Messages.PendingPrint,
			CA.Messages.Released
		};

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(Values, Labels) { }
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Voided = "V";
		public const string Scheduled = "S";
		public const string Open = "N";
		public const string Closed = "C";
		public const string Printed = "P";
		public const string Prebooked = "K";
		public const string PendingApproval = "E";
		public const string Rejected = "R";
		public const string Reserved = "Z";
		public const string PendingPrint = "G";
		public const string Released = "D";

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) {; }
		}

		public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(Balanced) {; }
		}

		public class voided : PX.Data.BQL.BqlString.Constant<voided>
		{
			public voided() : base(Voided) {; }
		}

		public class scheduled : PX.Data.BQL.BqlString.Constant<scheduled>
		{
			public scheduled() : base(Scheduled) {; }
		}

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) {; }
		}

		public class closed : PX.Data.BQL.BqlString.Constant<closed>
		{
			public closed() : base(Closed) {; }
		}

		public class printed : PX.Data.BQL.BqlString.Constant<printed>
		{
			public printed() : base(Printed) {; }
		}

		public class prebooked : PX.Data.BQL.BqlString.Constant<prebooked>
		{
			public prebooked() : base(Prebooked) {; }
		}

		public class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
		{
			public pendingApproval() : base(PendingApproval) { }
		}

		public class rejected : PX.Data.BQL.BqlString.Constant<rejected>
		{
			public rejected() : base(Rejected) { }
		}

		public class reserved : PX.Data.BQL.BqlString.Constant<reserved>
		{
			public reserved() : base(Reserved) { }
		}
		public class pendingPrint : PX.Data.BQL.BqlString.Constant<pendingPrint>
		{
			public pendingPrint() : base(PendingPrint) { }
		}

		public class released : PX.Data.BQL.BqlString.Constant<released>
		{
			public released() : base(Released) { }
		}
	}
}
