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

namespace PX.Objects.AM
{
	[PXCacheName("Diagram parameters")]
	public class DiagramParameters : IBqlTable
	{
		#region StartDate

		public virtual DateTime? StartDate { get; set; }

		public abstract class startDate : Data.BQL.BqlDateTime.Field<startDate>
		{
		}

		#endregion

		#region EndDate

		public virtual DateTime? EndDate { get; set; }

		public abstract class endDate : Data.BQL.BqlDateTime.Field<endDate>
		{
		}

		#endregion

		#region DisplayNonWorkingDays

		public virtual bool? DisplayNonWorkingDays { get; set; }

		public abstract class displayNonWorkingDays : Data.BQL.BqlBool.Field<displayNonWorkingDays>
		{
		}

		#endregion

		#region ColorCodingOrders

		public virtual string ColorCodingOrders { get; set; }

		public abstract class colorCodingOrders : Data.BQL.BqlString.Field<colorCodingOrders>
		{
		}

		#endregion

		#region BlockSizeInMinutes

		/// <summary>
		/// Value from <see cref="AMPSetup.schdBlockSize"/>
		/// </summary>
		public virtual int? BlockSizeInMinutes { get; set; }

		public abstract class blockSizeInMinutes : Data.BQL.BqlInt.Field<blockSizeInMinutes>	{	}

		#endregion

		#region WorkCentreCalendarType

		/// <summary>
		/// Defines how the Histogram data is displayed
		/// </summary>
		public virtual string WorkCentreCalendarType { get; set; }

		public abstract class workCentreCalendarType : Data.BQL.BqlString.Field<workCentreCalendarType>	{	}

		#endregion
	}
}
