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
using PX.Objects.Common.Attributes;

namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName(Messages.CCProcessingCenterFeeType)]
	public partial class CCProcessingCenterFeeType : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CCProcessingCenterFeeType>.By<processingCenterID, feeType>
		{
			public static CCProcessingCenterFeeType Find(PXGraph graph, string processingCenterID, string feeType, PKFindOptions options = PKFindOptions.None) => FindBy(graph, processingCenterID, feeType, options);
		}

		public static class FK
		{
			public class ProcessingCenter : CCProcessingCenter.PK.ForeignKeyOf<CCProcessingCenterFeeType>.By<processingCenterID> { }
			public class EntryType : CAEntryType.PK.ForeignKeyOf<CCProcessingCenterFeeType>.By<entryTypeID> { }
		}
		#endregion

		#region ProcessingCenterID
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CCProcessingCenter.processingCenterID))]
		[PXParent(typeof(Select<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Current<CCProcessingCenterFeeType.processingCenterID>>>>))]
		public virtual string ProcessingCenterID { get; set; }
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
		#endregion

		#region FeeType
		[PXDBString(256, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Fee Type")]
		public virtual string FeeType { get; set; }
		public abstract class feeType : PX.Data.BQL.BqlString.Field<feeType> { }
		#endregion

		#region EntryTypeId
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Entry Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId, InnerJoin<CashAccountETDetail,
								On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>,
								And<CashAccountETDetail.cashAccountID, Equal<Current<CCProcessingCenter.depositAccountID>>>>>,
								Where<CAEntryType.module, Equal<GL.BatchModule.moduleCA>,
								And<CAEntryType.useToReclassifyPayments, Equal<False>>>>))]
		public virtual string EntryTypeID { get; set; }
		public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region Tstamp
		[PXDBTimestamp]
		public virtual byte[] Tstamp { get; set; }
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
		#endregion
	}
}
