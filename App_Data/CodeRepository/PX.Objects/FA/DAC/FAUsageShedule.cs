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
using PX.Objects.IN;

namespace PX.Objects.FA
{

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(UsageScheduleMaint))]
	[PXCacheName(Messages.FAUsageSchedule)]
	public partial class FAUsageSchedule : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<FAUsageSchedule>.By<scheduleID>
		{
			public static FAUsageSchedule Find(PXGraph graph, Int32? scheduleID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, scheduleID, options);
		}
		#endregion
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }
		protected Int32? _ScheduleID;
		[PXDBIdentity()]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region ScheduleCD
		public abstract class scheduleCD : PX.Data.BQL.BqlString.Field<scheduleCD> { }
		protected String _ScheduleCD;
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual String ScheduleCD
		{
			get
			{
				return this._ScheduleCD;
			}
			set
			{
				this._ScheduleCD = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region ReadUsageEveryValue
		public abstract class readUsageEveryValue : PX.Data.BQL.BqlInt.Field<readUsageEveryValue> { }
		protected Int32? _ReadUsageEveryValue;
		[PXDBInt()]
		[PXDefault(1)]
		[PXUIField(DisplayName = "Read Every", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual Int32? ReadUsageEveryValue
		{
			get
			{
				return this._ReadUsageEveryValue;
			}
			set
			{
				this._ReadUsageEveryValue = value;
			}
		}
		#endregion
		#region ReadUsageEveryPeriod
		public abstract class readUsageEveryPeriod : PX.Data.BQL.BqlString.Field<readUsageEveryPeriod>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Day, Week, Month, Year },
					new string[] { Messages.Day, Messages.Week, Messages.Month, Messages.Year }) { }
			}

			public const string Day = "D";
			public const string Week = "W";
			public const string Month = "M";
			public const string Year = "Y";

			public class day : PX.Data.BQL.BqlString.Constant<day>
			{
				public day() : base(Day) { ;}
			}
			public class week : PX.Data.BQL.BqlString.Constant<week>
			{
				public week() : base(Week) { ;}
			}
			public class month : PX.Data.BQL.BqlString.Constant<month>
			{
				public month() : base(Month) { ;}
			}
			public class year : PX.Data.BQL.BqlString.Constant<year>
			{
				public year() : base(Year) { ;}
			}
		}
		protected String _ReadUsageEveryPeriod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(readUsageEveryPeriod.Month)]
		[PXUIField(DisplayName = "Interval", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		[readUsageEveryPeriod.List]
		public virtual String ReadUsageEveryPeriod
		{
			get
			{
				return _ReadUsageEveryPeriod;
			}
			set
			{
				_ReadUsageEveryPeriod = value;
			}
		}
		#endregion
		#region UsageUOM
		public abstract class usageUOM : PX.Data.BQL.BqlString.Field<usageUOM> { }
		protected String _UsageUOM;
		[PXUIField(DisplayName = "UOM")]
		[INUnit(typeof(INTran.inventoryID))]
		public virtual String UsageUOM
		{
			get
			{
				return _UsageUOM;
			}
			set
			{
				_UsageUOM = value;
			}
		}
		#endregion
		
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}
