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

using PX.CloudServices.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using System;

namespace PX.Objects.AP.InvoiceRecognition.DAC
{
	[PXInternalUseOnly]
	[Serializable]
	[PXHidden]
	public class RecognizedRecordErrorHistory : IBqlTable
	{
		[PXDBIdentity(IsKey = true)]
		public virtual int? ErrorNbr { get; set; }
		public abstract class errorNbr : BqlInt.Field<errorNbr> { }

		[PXDefault]
		[PXDBGuid]
		public virtual Guid? RefNbr { get; set; }
		public abstract class refNbr : BqlGuid.Field<refNbr> { }

		[PXDefault]
		[RecognizedRecordEntityTypeList]
		[PXDBString(3, IsFixed = true)]
		public virtual string EntityType { get; set; }
		public abstract class entityType : BqlString.Field<entityType> { }

		[PXDefault]
		[PXDBGuid]
		[PXUIField(DisplayName = "Cloud File ID", Enabled = false)]
		public virtual Guid? CloudFileId { get; set; }
		public abstract class cloudFileId : BqlGuid.Field<cloudFileId> { }

		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Error Message", Enabled = false)]
		public virtual string ErrorMessage { get; set; }
		public abstract class errorMessage : BqlString.Field<errorMessage> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : BqlGuid.Field<createdByID> { }

		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = "Date", Enabled = false)]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBTimestamp]
		public virtual byte[] TStamp { get; set; }
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
	}
}
