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

namespace PX.Objects.CS
{
	[PXCacheName(Messages.DaylightShift)]
	[Serializable]
	public partial class DaylightShift : IBqlTable
	{
		#region Year

		public abstract class year : PX.Data.BQL.BqlInt.Field<year> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Year")]
		public virtual Int32? Year { get; set; }

		#endregion

		#region TimeZone

		public abstract class timeZone : PX.Data.BQL.BqlString.Field<timeZone> { }

		[PXDBString(9, IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual String TimeZone { get; set; }

		#endregion

		#region TimeZoneDescription

		public abstract class timeZoneDescription : PX.Data.BQL.BqlString.Field<timeZoneDescription> { }

		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Time Zone", Enabled = false)]
		public virtual String TimeZoneDescription { get; set; }

		#endregion

		#region IsActive

		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Custom")]
		public virtual Boolean? IsActive { get; set; }

		#endregion

		#region FromDate

		public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

		[PXDBDateAndTime(UseTimeZone = false, PreserveTime = true)]
		[PXUIField(DisplayName = "From")]
		public virtual DateTime? FromDate { get; set; }

		#endregion

		#region ToDate

		public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

		private DateTime? _toDate;

		[PXDBDateAndTime(UseTimeZone = false, PreserveTime = true)]
		[PXUIField(DisplayName = "To")]
		public virtual DateTime? ToDate
		{
			get { return _toDate; }
			set { _toDate = value; }
		}

		#endregion

		#region Shift

		public abstract class shift : PX.Data.BQL.BqlInt.Field<shift> { }

		[PXDBInt(MinValue = -360, MaxValue = 360)] //NOTE: -6 hours and +6 hours
		[PXUIField(DisplayName = "Shift (minutes)")]
		public virtual Int32? Shift { get; set; }

		#endregion

		#region OriginalShift

		public abstract class originalShift : PX.Data.BQL.BqlDouble.Field<originalShift> { }

		[PXDouble]
		[PXUIField(Visible = false)]
		public virtual Double? OriginalShift { get; set; }

		#endregion
	}
}
