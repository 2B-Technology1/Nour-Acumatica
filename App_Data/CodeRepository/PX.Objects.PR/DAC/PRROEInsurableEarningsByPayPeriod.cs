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
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the insurable earnings for each pay period related to the record of employment.
	/// </summary>
	[PXCacheName(Messages.PRROEInsurableEarningsByPayPeriod)]
	[Serializable]
	public class PRROEInsurableEarningsByPayPeriod : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRROEInsurableEarningsByPayPeriod>.By<refNbr, payPeriodID>
		{
			public static PRROEInsurableEarningsByPayPeriod Find(PXGraph graph, string refNbr, string payPeriodID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, refNbr, payPeriodID, options);
		}

		public static class FK
		{
			public class RecordOfEmployment : PRRecordOfEmployment.PK.ForeignKeyOf<PRROEInsurableEarningsByPayPeriod>.By<refNbr> { }
		}
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		/// <summary>
		/// The user-friendly unique identifier of the record of employment.
		/// The field is included in <see cref="FK.RecordOfEmployment"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRRecordOfEmployment.RefNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXDBDefault(typeof(PRRecordOfEmployment.refNbr))]
		[PXParent(typeof(FK.RecordOfEmployment))]
		public string RefNbr { get; set; }
		#endregion

		#region PayPeriodID
		public abstract class payPeriodID : PX.Data.BQL.BqlString.Field<payPeriodID> { }
		/// <summary>
		/// The pay period.
		/// The field is included in <see cref="FK.RecordOfEmployment"/>.
		/// </summary>
		[PXDBString(6, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Pay Period ID")]
		public string PayPeriodID { get; set; }
		#endregion

		#region InsurableHours
		public abstract class insurableHours : PX.Data.BQL.BqlDecimal.Field<insurableHours> { }
		/// <summary>
		/// The amount of hours the employee has worked and for which they were paid.
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Insurable Hours")]
		public virtual decimal? InsurableHours { get; set; }
		#endregion

		#region InsurableEarnings
		public abstract class insurableEarnings : PX.Data.BQL.BqlDecimal.Field<insurableEarnings> { }
		/// <summary>
		/// The amount of earnings which the employee has received.
		/// </summary>
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Insurable Earnings")]
		public virtual decimal? InsurableEarnings { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
