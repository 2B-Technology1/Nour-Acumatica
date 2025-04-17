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
using System;

namespace PX.Objects.Common.DataIntegrity
{
	[Serializable]
	[PXHidden]
	[Obsolete(Messages.ClassIsObsoleteAndWillBeRemoved2019R2)]
	public class DataIntegrityLog : IBqlTable
	{
		public abstract class logEntryID : PX.Data.BQL.BqlInt.Field<logEntryID> { }
		[PXDBIdentity(IsKey = true)]
		public virtual int? LogEntryID { get; set; }

		public abstract class utcTime : PX.Data.BQL.BqlDateTime.Field<utcTime> { }
		[PXDBDateAndTime]
		public virtual DateTime? UtcTime { get; set; }

		public abstract class inconsistencyCode : PX.Data.BQL.BqlString.Field<inconsistencyCode> { }
		[PXDBString(30)]
		public virtual string InconsistencyCode { get; set; }

		public abstract class exceptionMessage : PX.Data.BQL.BqlString.Field<exceptionMessage> { }
		[PXDBString(255, IsUnicode = true)]
		public virtual string ExceptionMessage { get; set; }

		public abstract class contextInfo : PX.Data.BQL.BqlString.Field<contextInfo> { }
		[PXDBText(IsUnicode = true)]
		public virtual string ContextInfo { get; set; }

		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		[PXDBGuid]
		public virtual Guid? UserID { get; set; }

		public abstract class userBranchID : PX.Data.BQL.BqlInt.Field<userBranchID> { }
		[PXDBInt]
		public virtual int? UserBranchID { get; set; }
	}
}
