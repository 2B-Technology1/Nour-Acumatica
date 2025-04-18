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
using PX.Objects.CS;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FADepreciationMethodLines)]
	public partial class FADepreciationMethodLines : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<FADepreciationMethodLines>.By<methodID, year>
		{
			public static FADepreciationMethodLines Find(PXGraph graph, int? methodID, Int32? year, PKFindOptions options = PKFindOptions.None) => FindBy(graph, methodID, year, options);
		}
		public static class FK
		{
			public class DepreciationMethod : FA.FADepreciationMethod.PK.ForeignKeyOf<FADepreciationMethodLines>.By<methodID> { }
		}
		#endregion
		#region MethodID
		public abstract class methodID : PX.Data.BQL.BqlInt.Field<methodID> { }
		protected Int32? _MethodID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(FADepreciationMethod.methodID))]
		[PXParent(typeof(Select<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Current<FADepreciationMethodLines.methodID>>>>), UseCurrent = true, LeaveChildren = false)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? MethodID
		{
			get
			{
				return this._MethodID;
			}
			set
			{
				this._MethodID = value;
			}
		}
		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlInt.Field<year> { }
		protected Int32? _Year;
		[PXDBInt(IsKey = true, MaxValue = 500, MinValue = 0)]
		[PXUIField(DisplayName = "Recovery Year", Enabled = false)]
		public virtual Int32? Year
		{
			get
			{
				return this._Year;
			}
			set
			{
				this._Year = value;
			}
		}
		#endregion
		#region DisplayRatioPerYear
		public abstract class displayRatioPerYear : PX.Data.BQL.BqlDecimal.Field<displayRatioPerYear> { }
		[PXDecimal(3, MinValue = 0, MaxValue = 100)]
		[PXFormula(typeof(Mult<Current<ratioPerYear>, decimal100>))]
		[PXUIField(DisplayName = "Percent per Year")]
		public virtual decimal? DisplayRatioPerYear { get; set; }
		#endregion
		#region RatioPerYear
		public abstract class ratioPerYear : PX.Data.BQL.BqlDecimal.Field<ratioPerYear> { }
		protected Decimal? _RatioPerYear;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(5)]
        [PXFormula(typeof(Div<displayRatioPerYear, decimal100>), typeof(SumCalc<FADepreciationMethod.totalPercents>))]
		public virtual Decimal? RatioPerYear
		{
			get
			{
				return this._RatioPerYear;
			}
			set
			{
				this._RatioPerYear = value;
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
	}
}
