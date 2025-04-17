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

namespace PX.Objects.CA
{
	/// <summary>
	/// Contains the rules of mapping for Bank Feeds
	/// </summary>
	[PXCacheName(nameof(CABankFeedFieldMapping))]
	public class CABankFeedFieldMapping : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CABankFeedFieldMapping>.By<bankFeedID, lineNbr>
		{
			public static CABankFeedFieldMapping Find(PXGraph graph, string bankFeedID, int? bankFeedFieldMappingId) => FindBy(graph, bankFeedID, bankFeedFieldMappingId);
		}

		public static class FK
		{
			public class BankFeed : CABankFeed.PK.ForeignKeyOf<CABankFeedFieldMapping>.By<bankFeedID> { }
		}
		#endregion

		#region BankFeedID
		public abstract class bankFeedID : PX.Data.BQL.BqlString.Field<bankFeedID> { }
		/// <summary>
		/// Specifies the parent Bank Feed ID
		/// </summary>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CABankFeed.bankFeedID))]
		[PXParent(typeof(FK.BankFeed))]
		public virtual string BankFeedID { get; set; }
		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		/// <summary>
		/// Specifies line number
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(CABankFeed))]
		[PXUIField(Visible = false)]
		public virtual int? LineNbr { get; set; }
		#endregion

		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		/// <summary>
		/// Specifies the rule is active
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active { get; set; }
		#endregion

		#region TargetField
		public abstract class targetField : PX.Data.BQL.BqlString.Field<targetField> { }
		/// <summary>
		/// Specifies the target field in table CABankTran
		/// </summary>
		[PXDBString]
		[PXDefault]
		[CABankFeedMappingTarget.List]
		[PXUIField(DisplayName = "Target Field", Required = true)]
		public virtual string TargetField { get; set; }
		#endregion

		#region SourceFieldOrValue
		public abstract class sourceFieldOrValue : PX.Data.BQL.BqlString.Field<sourceFieldOrValue> { }
		/// <summary>
		/// Specifies the source field from table BankFeedTransaction or formula
		/// </summary>
		[PXDBString]
		[PXDefault]
		[PXUIField(DisplayName = "Source Field or Value", Required = true)]
		public virtual string SourceFieldOrValue { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = "Created Date Time")]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = "Last Modified Date Time")]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

		#region Noteid
		public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
		[PXNote]
		[PXUIField(DisplayName = "Noteid")]
		public virtual Guid? Noteid { get; set; }
		#endregion

		#region Tstamp
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
		[PXDBTimestamp]
		[PXUIField(DisplayName = "Tstamp")]
		public virtual byte[] Tstamp { get; set; }
		#endregion
	}
}
