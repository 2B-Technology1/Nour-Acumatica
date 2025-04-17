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
using static Customization.RowViewField;

namespace PX.Objects.AM
{
	/// <summary>
	/// Overall summary of each process of Regenerate Inventory Planning (AM50500) to capture process information such as start date, end date, was there an error, duration, and some record counts.
	/// </summary>
	[Serializable]
	[PXCacheName("Inventory Planning History")]
	public class AMRPHistory : IBqlTable
	{
		#region ProcessID
		/// <summary>
		/// Process i d
		/// </summary>
		[PXDBGuid(IsKey = true)]
		[PXUIField(DisplayName = "Process ID", Visible = false)]
		[PXDefault]
		public virtual Guid? ProcessID { get; set; }
		public abstract class processID : PX.Data.BQL.BqlGuid.Field<processID> { }
		#endregion

		#region StartDateTime
		/// <summary>
		/// Start date time
		/// </summary>
		[PXDBDateAndTime(UseTimeZone = true, DisplayNameDate = "Start Date", DisplayNameTime = "Start Time")]
		[PXUIField(DisplayName = "Start Date")]
		[PXDefault]
		public virtual DateTime? StartDateTime { get; set; }
		public abstract class startDateTime : PX.Data.BQL.BqlDateTime.Field<startDateTime> { }
		#endregion

		#region EndDateTime
		/// <summary>
		/// End date time
		/// </summary>
		[PXDBDateAndTime(UseTimeZone = true, DisplayNameDate = "End Date", DisplayNameTime = "End Time")]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDateTime { get; set; }
		public abstract class endDateTime : PX.Data.BQL.BqlDateTime.Field<endDateTime> { }
		#endregion

		#region Duration
		/// <summary>
		/// Duration
		/// </summary>
		[PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Duration")]
		[PXDefault(0)]
		public virtual int? Duration { get; set; }
		public abstract class duration : PX.Data.BQL.BqlInt.Field<duration> { }
		#endregion

		#region HasError
		/// <summary>
		/// Has error
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Has Errors")]
		[PXDefault(false)]
		public virtual bool? HasError { get; set; }
		public abstract class hasError : PX.Data.BQL.BqlBool.Field<hasError> { }
		#endregion

		#region CountAMRPDetailPlan
		/// <summary>
		/// Count a m r p detail plan
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Detail Plan Count")]
		public virtual int? CountAMRPDetailPlan { get; set; }
		public abstract class countAMRPDetailPlan : PX.Data.BQL.BqlInt.Field<countAMRPDetailPlan> { }

		/// <summary>
		/// First Pass Detail Count
		/// </summary>
		#endregion

		#region CountAMRPDetailFP
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "First Pass Detail Count")]
		public virtual int? CountAMRPDetailFP { get; set; }
		public abstract class countAMRPDetailFP : PX.Data.BQL.BqlInt.Field<countAMRPDetailFP> { }
		#endregion

		#region CountAMRPDetail
		/// <summary>
		/// Count a m r p detail
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Detail Count")]
		public virtual int? CountAMRPDetail { get; set; }
		public abstract class countAMRPDetail : PX.Data.BQL.BqlInt.Field<countAMRPDetail> { }

		/// <summary>
		/// Exceptions Count
		/// </summary>
		#endregion

		#region CountAMRPExceptions
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Exceptions Count")]
		public virtual int? CountAMRPExceptions { get; set; }
		public abstract class countAMRPExceptions : PX.Data.BQL.BqlInt.Field<countAMRPExceptions> { }
		#endregion

		#region CountAMRPPlan
		/// <summary>
		/// Count a m r p plan
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Plan Count")]
		public virtual int? CountAMRPPlan { get; set; }
		public abstract class countAMRPPlan : PX.Data.BQL.BqlInt.Field<countAMRPPlan> { }

		/// <summary>
		/// Inventory Count
		/// </summary>
		#endregion

		#region CountAMRPItemSite
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Inventory Count")]
		public virtual int? CountAMRPItemSite { get; set; }
		public abstract class countAMRPItemSite : PX.Data.BQL.BqlInt.Field<countAMRPItemSite> { }
		#endregion

		#region CreatedByID
		[PXDBCreatedByID] 
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		[PXUIField(DisplayName = "Created By(Screen ID)")]		
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region Tstamp
		[PXDBTimestamp]
		[PXUIField(DisplayName = "Tstamp")]
		public virtual byte[] Tstamp { get; set; }
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
		#endregion
	}
}
