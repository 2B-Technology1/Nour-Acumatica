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

using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
	/// <summary>
	/// Represents the class for the link between a <see cref="PMProgressWorksheet">progress worksheet</see> and a <see cref="DailyFieldReport">daily field report</see>.
	/// </summary>
	[PXCacheName("Daily Field Report Progress Worksheet")]
	public class DailyFieldReportProgressWorksheet : IBqlTable
	{
		#region DailyFieldReportProgressWorksheetId
		public abstract class dailyFieldReportProgressWorksheetId : BqlInt.Field<dailyFieldReportProgressWorksheetId> { }

		/// <summary>
		/// The unique ID of the link between the <see cref="PMProgressWorksheet">progress worksheet</see> and the <see cref="DailyFieldReport">daily field report</see>.
		/// </summary>
		[PXDBIdentity]
		public virtual int? DailyFieldReportProgressWorksheetId
		{
			get;
			set;
		}
		#endregion

		#region DailyFieldReportId
		public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId> { }

		/// <summary>
		/// The unique ID of the <see cref="DailyFieldReport">daily field report</see>.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
		[PXParent(typeof(SelectFrom<DailyFieldReport>
			.Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId.FromCurrent>>))]
		public virtual int? DailyFieldReportId
		{
			get;
			set;
		}
		#endregion

		#region ProgressWorksheetId
		public abstract class progressWorksheetId : BqlString.Field<progressWorksheetId> { }

		/// <summary>
		/// The unique ID of the <see cref="PMProgressWorksheet">progress worksheet</see>.
		/// </summary>
		[PXDefault]
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXSelector(typeof(Search<PMProgressWorksheet.refNbr>), Filterable = true)]
		[PXParent(typeof(SelectFrom<PMProgressWorksheet>
			.Where<PMProgressWorksheet.refNbr.IsEqual<progressWorksheetId.FromCurrent>>))]
		[PXUIField(DisplayName = "Reference Nbr.", Required = true)]
		public virtual string ProgressWorksheetId
		{
			get;
			set;
		}
		#endregion
	}
}
