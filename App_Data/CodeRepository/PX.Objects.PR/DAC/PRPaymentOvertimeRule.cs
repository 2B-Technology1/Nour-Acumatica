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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information about which rules apply to the paycheck, and what are the condition to trigger it. The information will be displayed on the Paychecks and Adjustments (PR302000) form.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.PRPaymentOvertimeRule)]
	public class PRPaymentOvertimeRule : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRPaymentOvertimeRule>.By<paymentDocType, paymentRefNbr, overtimeRuleID>
		{
			public static PRPaymentOvertimeRule Find(PXGraph graph, string paymentDocType, string paymentRefNbr, string overtimeRuleID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, paymentDocType, paymentRefNbr, overtimeRuleID, options);
		}

		public static class FK
		{
			public class Payment : PRPayment.PK.ForeignKeyOf<PRPaymentOvertimeRule>.By<paymentDocType, paymentRefNbr> { }
			public class OvertimeRule : PROvertimeRule.PK.ForeignKeyOf<PRPaymentOvertimeRule>.By<overtimeRuleID> { }
		}
		#endregion

		#region PaymentDocType
		public abstract class paymentDocType : BqlString.Field<paymentDocType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Payment Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public virtual string PaymentDocType { get; set; }
		#endregion
		#region PaymentRefNbr
		public abstract class paymentRefNbr : BqlString.Field<paymentRefNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Reference Nbr.")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<paymentDocType>>, And<PRPayment.refNbr, Equal<Current<paymentRefNbr>>>>>))]
		public virtual string PaymentRefNbr { get; set; }
		#endregion
		#region OvertimeRuleID
		public abstract class overtimeRuleID : BqlString.Field<overtimeRuleID> { }
		[PXDBString(30, IsKey = true, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Overtime Rule", IsReadOnly = true)]
		[PXForeignReference(typeof(Field<overtimeRuleID>.IsRelatedTo<PROvertimeRule.overtimeRuleID>))]
		public virtual string OvertimeRuleID { get; set; }
		#endregion
		#region RuleType
		public abstract class ruleType : BqlString.Field<ruleType> { }
		[PXString]
		[PXUIField(Visible = false)]
		[PXUnboundDefault(typeof(SearchFor<PROvertimeRule.ruleType>.Where<PROvertimeRule.overtimeRuleID.IsEqual<overtimeRuleID.FromCurrent>>))]
		public virtual string RuleType { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public virtual byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
