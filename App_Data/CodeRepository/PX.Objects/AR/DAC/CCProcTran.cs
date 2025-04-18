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
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using System.Linq;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AR
{
	/// <summary>
	/// A credit card processing transaction of an accounts
	/// receivable payment or a cash sale. They are visible on the Credit 
	/// Card Processing Info tab of the Payments and Applications (AR302000) 
	/// and Cash Sales (AR304000) forms, which correspond to the <see 
	/// cref="ARPaymentEntry"/> and <see cref="ARCashSaleEntry"/> graphs, 
	/// respectively.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.CCProcTran)]
	public partial class CCProcTran : PX.Data.IBqlTable, ICCPaymentTransaction 
	{
		#region Keys
		public class PK : PrimaryKeyOf<CCProcTran>.By<tranNbr>
		{
			public static CCProcTran Find(PXGraph graph, Int32? tranNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, tranNbr, options);
		}

		public static class FK
		{
			public class ExternalTransaction : AR.ExternalTransaction.PK.ForeignKeyOf<CCProcTran>.By<transactionID> { }
			public class CustomerPaymentMethod : AR.CustomerPaymentMethod.PK.ForeignKeyOf<CCProcTran>.By<pMInstanceID> { }
			public class ProcessingCenter : CA.CCProcessingCenter.PK.ForeignKeyOf<CCProcTran>.By<processingCenterID> { }
			public class ARPayment : AR.ARPayment.PK.ForeignKeyOf<CCProcTran>.By<docType, refNbr> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<CCProcTran>.By<curyID> { }
			public class ParentProcessingTransaction : AR.CCProcTran.PK.ForeignKeyOf<CCProcTran>.By<refTranNbr> { }
		}
		#endregion

		#region TranNbr
		public abstract class tranNbr : PX.Data.BQL.BqlInt.Field<tranNbr> { }
		protected Int32? _TranNbr;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Tran. Nbr.",Visibility=PXUIVisibility.SelectorVisible)]
		public virtual Int32? TranNbr
		{
			get
			{
				return this._TranNbr;
			}
			set
			{
				this._TranNbr = value;
			}
		}
		#endregion

		#region TransactionID
		public abstract class transactionID : PX.Data.BQL.BqlInt.Field<transactionID> { }
		[PXDBInt]
		[PXDBDefault(typeof(ExternalTransaction.transactionID))]
		[PXParent(typeof(Select<ExternalTransaction, Where<ExternalTransaction.transactionID, Equal<Current<CCProcTran.transactionID>>>>))]
		[PXUIField(DisplayName = "Ext. Tran. ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? TransactionID { get; set; }
		#endregion

		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		protected Int32? _PMInstanceID;
		[PXDBInt()]
		public virtual Int32? PMInstanceID
		{
			get
			{
				return this._PMInstanceID;
			}
			set
			{
				this._PMInstanceID = value;
			}
		}
		#endregion
		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
		protected String _ProcessingCenterID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName="Proc. Center",Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String ProcessingCenterID
		{
			get
			{
				return this._ProcessingCenterID;
			}
			set
			{
				this._ProcessingCenterID = value;
			}
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(3)]
		[PXUIField(DisplayName = "Doc. Type",Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Doc. Reference Nbr.",Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		protected String _OrigDocType;
		[PXDBString(3)]
		[PXUIField(DisplayName = "Orig. Doc. Type")]
		public virtual String OrigDocType
		{
			get
			{
				return this._OrigDocType;
			}
			set
			{
				this._OrigDocType = value;
			}
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		protected String _OrigRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orig. Doc. Ref. Nbr.")]
		public virtual String OrigRefNbr
		{
			get
			{
				return this._OrigRefNbr;
			}
			set
			{
				this._OrigRefNbr = value;
			}
		}
		#endregion
		
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault()]
		[CCTranTypeCode.List()]
		[PXUIField(DisplayName="Tran. Type")]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region ProcStatus
		public abstract class procStatus : PX.Data.BQL.BqlString.Field<procStatus> { }
		protected String _ProcStatus;
		[PXDBString(3, IsFixed = true)]
		[PXDefault(CCProcStatus.Opened)]
		[CCProcStatus.List()]
		[PXUIField(DisplayName = "Proc. Status")]
		public virtual String ProcStatus
		{
			get
			{
				return this._ProcStatus;
			}
			set
			{
				this._ProcStatus = value;
			}
		}
		#endregion
		#region TranStatus
		public abstract class tranStatus : PX.Data.BQL.BqlString.Field<tranStatus> { }
		protected String _TranStatus;
		[PXDBString(3, IsFixed = true)]
		[CCTranStatusCode.List()]
		[PXUIField(DisplayName = "Tran. Status")]
		public virtual String TranStatus
		{
			get
			{
				return this._TranStatus;
			}
			set
			{
				this._TranStatus = value;
			}
		}
		#endregion
		#region CVVVerificationStatus
		public abstract class cVVVerificationStatus : PX.Data.BQL.BqlString.Field<cVVVerificationStatus> { }
		protected String _CVVVerificationStatus;
		[PXDBString(3, IsFixed = true)]
		[PXDefault(CVVVerificationStatusCode.RequiredButNotVerified)]
		[CVVVerificationStatusCode.List()]
		[PXUIField(DisplayName = "CVV Verification")]

		public virtual String CVVVerificationStatus
		{
			get
			{
				return this._CVVVerificationStatus;
			}
			set
			{
				this._CVVVerificationStatus = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency",Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(CM.Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
			#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		protected Decimal? _Amount;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran. Amount",Visibility=PXUIVisibility.SelectorVisible)]
		public virtual Decimal? Amount
		{
			get
			{
				return this._Amount;
			}
			set
			{
				this._Amount = value;
			}
		}
		#endregion
		#region SubtotalAmount
		public abstract class subtotalAmount : PX.Data.BQL.BqlDecimal.Field<subtotalAmount> { }
		/// <summary>Amount before tax.</summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Subtotal Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? SubtotalAmount
		{
			get;
			set;
		}
		#endregion
		#region Tax
		public abstract class tax : PX.Data.BQL.BqlDecimal.Field<tax> { }
		/// <summary>Total tax amount.</summary>
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Tax", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? Tax
		{
			get;
			set;
		}
		#endregion
		#region FundHoldExpDate

		public abstract class fundHoldExpDate : Data.BQL.BqlDateTime.Field<fundHoldExpDate> { }

		[PXDateAndTime]
		[PXFormula(typeof(Switch<Case<Where<tranType, Equal<CCTranTypeCode.authorize>>, Parent<ExternalTransaction.fundHoldExpDate>>>))]
		[PXUIField(DisplayName = "Expire On (Est.)", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? FundHoldExpDate
		{
			get;
			set;
		}
		#endregion
		#region RefTranNbr
		public abstract class refTranNbr : PX.Data.BQL.BqlInt.Field<refTranNbr> { }
		protected Int32? _RefTranNbr;
		[PXDBInt()]
		[PXUIField(DisplayName = "Referenced Tran. Nbr.")]
		public virtual Int32? RefTranNbr
		{
			get
			{
				return this._RefTranNbr;
			}
			set
			{
				this._RefTranNbr = value;
			}
		}
		#endregion
		#region RefPCTranNumber
		public abstract class refPCTranNumber : PX.Data.BQL.BqlString.Field<refPCTranNumber> { }
		protected String _RefPCTranNumber;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center Ref. Tran. Nbr.")]
		public virtual String RefPCTranNumber
		{
			get
			{
				return this._RefPCTranNumber;
			}
			set
			{
				this._RefPCTranNumber = value;
			}
		}
		#endregion
		#region PCTranNumber
		public abstract class pCTranNumber : PX.Data.BQL.BqlString.Field<pCTranNumber> { }
		protected String _PCTranNumber;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center Tran. Nbr.",Visibility=PXUIVisibility.SelectorVisible)]
		public virtual String PCTranNumber
		{
			get
			{
				return this._PCTranNumber;
			}
			set
			{
				this._PCTranNumber = value;
			}
		}
		#endregion
		#region AuthNumber
		public abstract class authNumber : PX.Data.BQL.BqlString.Field<authNumber> { }
		protected String _AuthNumber;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center Auth. Nbr.")]
		public virtual String AuthNumber
		{
			get
			{
				return this._AuthNumber;
			}
			set
			{
				this._AuthNumber = value;
			}
		}
		#endregion
		#region PCResponseCode
		public abstract class pCResponseCode : PX.Data.BQL.BqlString.Field<pCResponseCode> { }
		protected String _PCResponseCode;
		[PXDBString(10, IsUnicode = true)]
		public virtual String PCResponseCode
		{
			get
			{
				return this._PCResponseCode;
			}
			set
			{
				this._PCResponseCode = value;
			}
		}
		#endregion
		#region PCResponseReasonCode
		public abstract class pCResponseReasonCode : PX.Data.BQL.BqlString.Field<pCResponseReasonCode> { }
		protected String _PCResponseReasonCode;
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		public virtual String PCResponseReasonCode
		{
			get
			{
				return this._PCResponseReasonCode;
			}
			set
			{
				this._PCResponseReasonCode = value;
			}
		}
		#endregion
		#region PCResponseReasonText
		public abstract class pCResponseReasonText : PX.Data.BQL.BqlString.Field<pCResponseReasonText> { }
		protected String _PCResponseReasonText;
		[PXDBString(512, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center Response Reason")]
		public virtual String PCResponseReasonText
		{
			get
			{
				return this._PCResponseReasonText;
			}
			set
			{
				this._PCResponseReasonText = value;
			}
		}
		#endregion
		#region PCResponse
		public abstract class pCResponse : PX.Data.BQL.BqlString.Field<pCResponse> { }
		protected String _PCResponse;
		[PXRSACryptString(2048, IsUnicode = true)]
		public virtual String PCResponse
		{
			get
			{
				return this._PCResponse;
			}
			set
			{
				this._PCResponse = value;
			}
		}
		#endregion
		#region StartTime
		public abstract class startTime : PX.Data.BQL.BqlDateTime.Field<startTime> { }
		protected DateTime? _StartTime;
		[PXDBDate(PreserveTime = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tran. Time")]
		public virtual DateTime? StartTime
		{
			get
			{
				return this._StartTime;
			}
			set
			{
				this._StartTime = value;
			}
		}
		#endregion
		#region EndTime
		public abstract class endTime : PX.Data.BQL.BqlDateTime.Field<endTime> { }
		protected DateTime? _EndTime;
		[PXDBDate(PreserveTime = true)]
		public virtual DateTime? EndTime
		{
			get
			{
				return this._EndTime;
			}
			set
			{
				this._EndTime = value;
			}
		}
		#endregion
		#region ExpirationDate
		public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
		protected DateTime? _ExpirationDate;
		[PXDBDate(PreserveTime = true)]
		public virtual DateTime? ExpirationDate
		{
			get
			{
				return this._ExpirationDate;
			}
			set
			{
				this._ExpirationDate = value;
			}
		}
		#endregion
		#region ErrorSource
		public abstract class errorSource : PX.Data.BQL.BqlString.Field<errorSource> { }
		protected String _ErrorSource;
		[PXDBString(3, IsFixed = true)]
		[PXUIField(Visible = false, DisplayName = "Error Source")]
		public virtual String ErrorSource
		{
			get
			{
				return this._ErrorSource;
			}
			set
			{
				this._ErrorSource = value;
			}
		}
		#endregion
		#region ErrorText
		public abstract class errorText : PX.Data.BQL.BqlString.Field<errorText> { }
		protected String _ErrorText;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(Visible = false, DisplayName = "Error Text")]
		public virtual String ErrorText
		{
			get
			{
				return this._ErrorText;
			}
			set
			{
				this._ErrorText = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region Imported
		public abstract class imported : PX.Data.BQL.BqlBool.Field<imported> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Imported")]
		public virtual bool? Imported { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		
		public virtual void Copy(ICCPayment aPmtInfo) 
		{
			this.PMInstanceID = aPmtInfo.PMInstanceID;
			this.DocType = aPmtInfo.DocType;
			this.RefNbr = aPmtInfo.RefNbr;
			this.CuryID = aPmtInfo.CuryID;
			this.Amount = aPmtInfo.CuryDocBal;
			this.OrigDocType = aPmtInfo.OrigDocType;
			this.OrigRefNbr = aPmtInfo.OrigRefNbr;
		}

		public virtual bool IsManuallyEntered()
		{
			return (this.ProcStatus == CCProcStatus.Finalized 
				&& this.TranStatus == CCTranStatusCode.Approved
				&& string.IsNullOrEmpty(this.PCResponseCode));
		} 
	}

	public static class CCProcStatus 
	{
		public const string Opened = "OPN";
		public const string Finalized = "FIN";
		public const string Error = "ERR";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Opened, Finalized, Error },
				new string[] { "Open", "Completed", "Error" }) { }
		}
		public class opened : PX.Data.BQL.BqlString.Constant<opened>
		{
			public opened() : base(Opened) { ;}
		}

		public class finalized : PX.Data.BQL.BqlString.Constant<finalized>
		{
			public finalized() : base(Finalized) { ;}
		}
		public class error : PX.Data.BQL.BqlString.Constant<error>
		{
			public error() : base(Error) { ;}
		}
		
	}

	public static class CCTranStatusCode
	{
		public const string Approved = "APR";
		public const string Declined = "DEC";
		public const string HeldForReview = "HFR";
		public const string Error = "ERR";
		public const string Expired = "EXP";
		public const string Unknown = "UKN";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Approved, Declined, Error, HeldForReview, Expired, Unknown},
				new string[] { "Approved", "Declined", "Error", "Held for Review", "Expired", "Unknown" }) { }
		}
		public class approved : PX.Data.BQL.BqlString.Constant<approved>
		{
			public approved() : base(Approved ) { ;}
		}

		public class declined : PX.Data.BQL.BqlString.Constant<declined>
		{
			public declined() : base(Declined) { ;}
		}

		public class heldForReview : PX.Data.BQL.BqlString.Constant<heldForReview>
		{
			public heldForReview() : base(HeldForReview) { ;}
		}

		public class error : PX.Data.BQL.BqlString.Constant<error>
		{
			public error() : base(Error) { ;}
		}

		public static string GetCode(CCTranStatus status)
		{
			if (!statusCodes.Where(i => i.Item1 == status).Any())
			{
				throw new InvalidOperationException();
			}
			return statusCodes.Where(i => i.Item1 == status).First().Item2;
		}
		
		internal static CCTranStatus GetCCTranStatus(string tranStatusCode)
		{
			if (!statusCodes.Where(i => i.Item2.Equals(tranStatusCode)).Any())
			{
				throw new InvalidOperationException();
			}
			return statusCodes.Where(i => i.Item2.Equals(tranStatusCode)).First().Item1;
		}

		private static (CCTranStatus,string)[] statusCodes = {
			(CCTranStatus.Approved, Approved), (CCTranStatus.Declined, Declined),
			(CCTranStatus.Error, Error), (CCTranStatus.HeldForReview, HeldForReview),
			(CCTranStatus.Expired, Expired), (CCTranStatus.Unknown, Unknown)
		};

	}

	public static class CCTranTypeCode 
	{
		public const string Authorize = "AUT";
		public const string AuthorizeAndCapture = "AAC";
		public const string PriorAuthorizedCapture = "PAC";
		public const string CaptureOnly = "CAP";
		public const string VoidTran = "VDG";
		public const string Credit = "CDT";
		public const string Unknown = "UKN";

		public const string AUTLabel = "Authorize Only";
		public const string AACLabel = "Authorize and Capture";
		public const string PACLabel = "Capture Authorized";
		public const string CAPLabel = "Capture Manualy Authorized";
		public const string VDGLabel = "Void";
		public const string CDTLabel = "Refund";
		public const string UKNLabel = "Unknown";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Authorize, AuthorizeAndCapture, PriorAuthorizedCapture, CaptureOnly, VoidTran, Credit, Unknown },
				new string[] { AUTLabel, AACLabel, PACLabel, CAPLabel, VDGLabel, CDTLabel, UKNLabel }) { }
		}

		public class authorize : PX.Data.BQL.BqlString.Constant<authorize>
		{
			public authorize() : base(Authorize) { ;}
		}

		public class priorAuthorizedCapture : PX.Data.BQL.BqlString.Constant<priorAuthorizedCapture>
		{
			public priorAuthorizedCapture() : base(PriorAuthorizedCapture) { ;}
		}

		public class authorizeAndCapture : PX.Data.BQL.BqlString.Constant<authorizeAndCapture>
		{
			public authorizeAndCapture() : base(AuthorizeAndCapture) { ;}
		}

		public class captureOnly : PX.Data.BQL.BqlString.Constant<captureOnly>
		{
			public captureOnly() : base(CaptureOnly)
			{
			}
		}

		public class voidTran : PX.Data.BQL.BqlString.Constant<voidTran>
		{
			public voidTran() : base(VoidTran) { ;}
		}

		public class credit : PX.Data.BQL.BqlString.Constant<credit>
		{
			public credit() : base(Credit) { ;}
		}

		public static string GetTypeCode(CCTranType tranType)
		{
			if (!typeCodes.Where(i => i.Item1 == tranType).Any())
			{
				throw new InvalidOperationException();
			}
			return typeCodes.Where(i => i.Item1 == tranType).First().Item2;
		}

		public static string GetTypeLabel(CCTranType tranType)
		{
			string typeCode = GetTypeCode(tranType);
			ListAttribute attr = new ListAttribute();
			string label = PXMessages.LocalizeNoPrefix(attr.ValueLabelDic[typeCode]);
			return label;
		}

		public static CCTranType GetTranTypeByTranTypeStr(string tranTypeStr)
		{
			if (!typeCodes.Where(i => i.Item2 == tranTypeStr).Any())
			{
				throw new PXInvalidOperationException();
			}
			return typeCodes.Where(i => i.Item2 == tranTypeStr).First().Item1;
		}

		public static bool IsCaptured(CCTranType tranType)
		{
			bool ret = false;
			if (tranType == CCTranType.AuthorizeAndCapture
				|| tranType == CCTranType.CaptureOnly
				|| tranType == CCTranType.PriorAuthorizedCapture)
			{
				ret = true;
			}
			return ret;
		}

		private static (CCTranType, string)[] typeCodes = {
			(CCTranType.AuthorizeAndCapture, AuthorizeAndCapture), (CCTranType.AuthorizeOnly, Authorize),
			(CCTranType.PriorAuthorizedCapture, PriorAuthorizedCapture), (CCTranType.CaptureOnly, CaptureOnly),
			(CCTranType.Credit, Credit), (CCTranType.Void, VoidTran), (CCTranType.Unknown, Unknown)
		};
	}

	public static class CVVVerificationStatusCode 
	{
		public const string Matched = "MTH";
		public const string NotMatched = "NMH";
		public const string RequiredButNotVerified = "NOV";
		public const string RequiredButNotProvided = "SBP";
		public const string NotVerifiedDueToIssuer ="INV";
		public const string SkippedDueToPriorVerification = "RPV";
		public const string Empty = "EMP";
		public const string Unknown = "UKN";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
new string[] { CVVVerificationStatusCode.Matched, NotMatched, RequiredButNotVerified, RequiredButNotProvided, NotVerifiedDueToIssuer, SkippedDueToPriorVerification, Empty, Unknown },
				new string[] { "Matched", "Not Matched", "Required but Not Verified", "Required but Not Provided", "Not Verified Due to Issuer", "Skipped Due to Prior Verification", "Processing center response is empty", "Unknown" })
			{ }
		}

		public static string GetCCVCode(CcvVerificationStatus sStatus)
		{
			if (!_statuses.Where(i => i.Item1 == sStatus).Any())
			{
				throw new InvalidOperationException();
			}
			return _statuses.Where(i => i.Item1 == sStatus).First().Item2;
		}

		internal static CcvVerificationStatus GetCcvVerificationStatus(string CCVCode)
		{
			if(!_statuses.Where(i => i.Item2.Equals(CCVCode)).Any())
			{
				throw new InvalidOperationException();
			}
			return _statuses.Where(i => i.Item2.Equals(CCVCode)).First().Item1;
		}

		public class match : PX.Data.BQL.BqlString.Constant<match>
		{
			public match() : base(CVVVerificationStatusCode.Matched) { }
		}

		private static (CcvVerificationStatus, string)[] _statuses = new[] {
			(CcvVerificationStatus.Match, Matched), (CcvVerificationStatus.NotMatch, NotMatched), (CcvVerificationStatus.NotProcessed, RequiredButNotVerified),
			(CcvVerificationStatus.ShouldHaveBeenPresent, RequiredButNotProvided), (CcvVerificationStatus.IssuerUnableToProcessRequest, NotVerifiedDueToIssuer),
			(CcvVerificationStatus.RelyOnPreviousVerification, SkippedDueToPriorVerification), (CcvVerificationStatus.Empty, Empty), (CcvVerificationStatus.Unknown, Unknown)
		};
	}
}
