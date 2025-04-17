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

namespace PX.Objects.CR
{
	[Serializable]
	[PXProjection(typeof(Select<CRLead>))]
	[PXCacheName(Messages.LeadStatistics)]
	public partial class CRLeadStatistics : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBIdentity(IsKey = true, BqlField = typeof(CRLead.contactID))]
		[PXUIField(DisplayName = "Lead ID", Visibility = PXUIVisibility.Invisible)]
		[PXPersonalDataWarning]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region LeadQualificationTime
		public abstract class leadQualificationTime : PX.Data.BQL.BqlInt.Field<leadQualificationTime> { }

		[CRTimeSpanCalced(typeof(DateDiff<CRLead.createdDateTime, CRLead.qualificationDate, DateDiff.minute>))]
		[PXTimeSpanLong(Format = TimeSpanFormatType.DaysHoursMinites, InputMask = "#### d\\ays ## hrs ## mins")]
		[PXUIField(DisplayName = "Lead Qualification Time")]
		public int? LeadQualificationTime { get; set; }
		#endregion

		#region LeadResponseTime
		public abstract class leadResponseTime : PX.Data.BQL.BqlInt.Field<leadResponseTime> { }

		[CRTimeSpanCalced(typeof(Minus1<
			Search<CRActivityStatistics.initialOutgoingActivityCompletedAtDate,
				Where<CRActivityStatistics.noteID, Equal<CRLead.noteID>>>,
			CRLead.createdDateTime>))]
		[PXUIField(DisplayName = "Lead Response Time")]
		[PXTimeSpanLong(Format = TimeSpanFormatType.DaysHoursMinites, InputMask = "#### d\\ays ## hrs ## mins")]
		public Int32? LeadResponseTime { get; set; }
		#endregion
	}
}
