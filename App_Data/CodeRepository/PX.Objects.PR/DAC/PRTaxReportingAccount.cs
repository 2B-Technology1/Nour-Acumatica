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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores information about about different accounts requested by the Canadian Government to generate the T4 and RL1 forms.
	/// </summary>
	[PXCacheName(Messages.PRTaxReportingAccount)]
	[Serializable]
	public class PRTaxReportingAccount : IBqlTable
	{
		#region Keys
		public new class PK : PrimaryKeyOf<PRTaxReportingAccount>.By<bAccountID>
		{
			public static PRTaxReportingAccount Find(PXGraph graph, int? bAccountID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bAccountID, options);
		}

		public new static class FK
		{
			public class BusinessAccount : BAccount.PK.ForeignKeyOf<PRTaxReportingAccount>.By<bAccountID> { }
		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		/// <summary>
		/// The unique identifier of the business account associated with the employee.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.Invisible)]
		[PXDBDefault(typeof(BAccount.bAccountID))]
		[PXParent(typeof(FK.BusinessAccount))]
		public int? BAccountID { get; set; }
		#endregion
		#region CRAPayrollAccountNumber
		public abstract class craPayrollAccountNumber : PX.Data.BQL.BqlString.Field<craPayrollAccountNumber> { }
		/// <summary>
		/// The CRA Payroll Account Number.
		/// </summary>
		[PXDBString(15, InputMask = ">000000000LL0000")]
		[PXUIField(DisplayName = "CRA Payroll Account Number")]
		public virtual String CRAPayrollAccountNumber { get; set; }
		#endregion
		#region RL1IdentificationNumber
		public abstract class rl1IdentificationNumber : PX.Data.BQL.BqlString.Field<rl1IdentificationNumber> { }
		/// <summary>
		/// The RL1 Identification Number.
		/// </summary>
		[PXDBString(10, InputMask = "0000000000")]
		[PXUIField(DisplayName = "Identification Number")]
		public virtual String RL1IdentificationNumber { get; set; }
		#endregion
		#region RL1FileNumber
		public abstract class rl1FileNumber : PX.Data.BQL.BqlString.Field<rl1FileNumber> { }
		/// <summary>
		/// The RL1 File Number.
		/// </summary>
		[PXDBString(4, InputMask = "0000")]
		[PXUIField(DisplayName = "File Number")]
		public virtual String RL1FileNumber { get; set; }
		#endregion
		#region RL1QuebecEnterpriseNumber
		public abstract class rl1QuebecEnterpriseNumber : PX.Data.BQL.BqlString.Field<rl1QuebecEnterpriseNumber> { }
		/// <summary>
		/// The RL1 Quebec Enterprise Number.
		/// </summary>
		[PXDBString(10, InputMask = "0000000000")]
		[PXUIField(DisplayName = "Quebec Enterprise Number")]
		public virtual String RL1QuebecEnterpriseNumber { get; set; }
		#endregion
		#region RL1QuebecTransmitterNumber
		public abstract class rl1QuebecTransmitterNumber : PX.Data.BQL.BqlString.Field<rl1QuebecTransmitterNumber> { }
		/// <summary>
		/// The RL1 Quebec Transmitter Number.
		/// </summary>
		[PXDBString(8, InputMask = ">LL000000")]
		[PXUIField(DisplayName = "Quebec Transmitter Number")]
		public virtual String RL1QuebecTransmitterNumber { get; set; }
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
