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
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.ActivityStatistics)]
	public class CRActivityStatistics : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CRActivityStatistics>.By<noteID>
		{
			public static CRActivityStatistics Find(PXGraph graph, Guid? noteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, noteID, options);
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXDBGuid(IsKey = true)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region LastIncomingActivityNoteID
		public abstract class lastIncomingActivityNoteID : PX.Data.BQL.BqlGuid.Field<lastIncomingActivityNoteID> { }
		[PXDBGuid]
		[PXDBDefault(typeof(CRActivity.noteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Guid? LastIncomingActivityNoteID { get; set; }
		#endregion

		#region LastOutgoingActivityNoteID
		public abstract class lastOutgoingActivityNoteID : PX.Data.BQL.BqlGuid.Field<lastOutgoingActivityNoteID> { }
		[PXDBGuid]
		[PXDBDefault(typeof(CRActivity.noteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Guid? LastOutgoingActivityNoteID { get; set; }
		#endregion

		#region LastIncomingActivityDate
		public abstract class lastIncomingActivityDate : PX.Data.BQL.BqlDateTime.Field<lastIncomingActivityDate> { }
		[PXDBDate(PreserveTime = true, UseSmallDateTime = false)]
		[PXUIField(DisplayName = "Last Incoming Activity", Enabled = false)]
		public virtual DateTime? LastIncomingActivityDate { get; set; }
		#endregion

		#region LastOutgoingActivityDate
		public abstract class lastOutgoingActivityDate : PX.Data.BQL.BqlDateTime.Field<lastOutgoingActivityDate> { }
        [PXDBDate(PreserveTime = true, UseSmallDateTime = false)]
		[PXUIField(DisplayName = "Last Outgoing Activity", Enabled = false)]
		public virtual DateTime? LastOutgoingActivityDate { get; set; }
		#endregion

		#region InitialOutgoingActivityCompletedAtNoteID
		public abstract class initialOutgoingActivityCompletedAtNoteID : PX.Data.BQL.BqlGuid.Field<initialOutgoingActivityCompletedAtNoteID> { }
		[PXDBGuid]
		[PXDBDefault(typeof(CRActivity.noteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Guid? InitialOutgoingActivityCompletedAtNoteID { get; set; }
		#endregion

		#region InitialOutgoingActivityCompletedAtDate
		public abstract class initialOutgoingActivityCompletedAtDate : PX.Data.BQL.BqlDateTime.Field<initialOutgoingActivityCompletedAtDate> { }
		[PXDBDate(PreserveTime = true, UseSmallDateTime = false)]
		[PXUIField(DisplayName = "Initial Outgoing Activity", Enabled = false)]
		public virtual DateTime? InitialOutgoingActivityCompletedAtDate { get; set; }
		#endregion

		#region LastActivityDate
		public abstract class lastActivityDate : PX.Data.BQL.BqlDateTime.Field<lastActivityDate> { }
		[PXDBCalced(typeof(Switch<
				Case<Where<lastIncomingActivityDate, IsNotNull, And<lastOutgoingActivityDate, IsNull>>, lastIncomingActivityDate,
				Case<Where<lastOutgoingActivityDate, IsNotNull, And<lastIncomingActivityDate, IsNull>>, lastOutgoingActivityDate,
				Case<Where<lastIncomingActivityDate, Greater<lastOutgoingActivityDate>>, lastIncomingActivityDate>>>,
			lastOutgoingActivityDate>),
			typeof(DateTime))]
		[PXUIField(DisplayName = "Last Activity Date", Enabled = false)]
		[PXDate]
		public virtual DateTime? LastActivityDate { get; set; }
		#endregion

		#region Activity Aging
		public enum LastActivityAgingEnum
		{
			None = 0,
			Last30days,
			Last3060days,
			Last6090days,
			Over90days
		}

		public class LastActivityAgingConst
		{
			public class none : PX.Data.BQL.BqlInt.Constant<none>
			{
				public none() : base((int)LastActivityAgingEnum.None) { }
			}
			public class last30days : Data.BQL.BqlInt.Constant<last30days>
			{
				public last30days() : base((int)LastActivityAgingEnum.Last30days) { }
			}
			public class last3060days : Data.BQL.BqlInt.Constant<last3060days>
			{
				public last3060days() : base((int)LastActivityAgingEnum.Last3060days) { }
			}
			public class last6090days : Data.BQL.BqlInt.Constant<last6090days>
			{
				public last6090days() : base((int)LastActivityAgingEnum.Last6090days) { }
			}
			public class over90days : Data.BQL.BqlInt.Constant<over90days>
			{
				public over90days() : base((int)LastActivityAgingEnum.Over90days) { }
			}
		}

		public abstract class lastActivityAging : PX.Data.BQL.BqlInt.Field<lastActivityAging> { }
		[PXUIField(DisplayName = "Last Activity Aging")]
		[PXInt]
		[PXIntList(typeof(LastActivityAgingEnum), 
			new[] {
				Messages.None,
				Messages.Last30days,
				Messages.Last3060days,
				Messages.Last6090days,
				Messages.Over90days
			})]
		[PXDBCalced(typeof(
				Switch<
						Case<
					Where<lastOutgoingActivityDate, IsNull>, LastActivityAgingConst.none,
						Case<
						Where<DateDiff<lastOutgoingActivityDate, CurrentValue<AccessInfo.businessDate>, DateDiff.day>, LessEqual<int30>>, LastActivityAgingConst.last30days,
						Case<
						Where<DateDiff<lastOutgoingActivityDate, CurrentValue<AccessInfo.businessDate>, DateDiff.day>, LessEqual<int60>>, LastActivityAgingConst.last3060days,
						Case<
						Where<DateDiff<lastOutgoingActivityDate, CurrentValue<AccessInfo.businessDate>, DateDiff.day>, LessEqual<int90>>, LastActivityAgingConst.last6090days>
					>>>,
					LastActivityAgingConst.over90days
					>)
			, typeof(int?))]
		public int? LastActivityAging { get; set; }
		#endregion

	}

	[PXHidden]
	public class CRLeadActivityStatistics : CRActivityStatistics
	{
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
	}

	[PXHidden]
	public class CRBAccountActivityStatistics : CRActivityStatistics
	{
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
	}
}
