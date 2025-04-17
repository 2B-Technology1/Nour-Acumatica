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

using PX.Objects.GL;
using PX.Objects.GL.Attributes;

namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.AP1099Year)]
	public partial class AP1099Year : PX.Data.IBqlTable
	{
        private class string0101 : PX.Data.BQL.BqlString.Constant<string0101>
		{
            public string0101()
                : base("0101")
            {
            }
        }
        private class string1231 : PX.Data.BQL.BqlString.Constant<string1231>
		{
            public string1231()
                : base("1231")
            {
            }
        }

		#region Keys
		public class PK : PrimaryKeyOf<AP1099Year>.By<organizationID, finYear>
		{
			public static AP1099Year Find(PXGraph graph, Int32? organizationID, String finYear, PKFindOptions options = PKFindOptions.None) => FindBy(graph, organizationID, finYear, options);
		}
		public static class FK
		{
			public class Organization : GL.DAC.Organization.PK.ForeignKeyOf<AP1099Year>.By<organizationID> { }
		}
		#endregion

		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[PXDefault]
		[Organization(true, IsKey = true, FieldClass = "MULTICOMPANY")]
		public virtual int? OrganizationID { get; set; }
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		protected String _FinYear;
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName="1099 Year", Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AP1099Year.finYear,
									Where<AP1099Year.organizationID, Equal<Current2<AP1099Year.organizationID>>>>))]
		public virtual String FinYear
		{
			get
			{
				return this._FinYear;
			}
			set
			{
				this._FinYear = value;
			}
		}
		#endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlString.Field<startDate> { }
        protected String _StartDate;
        [PXDBCalced(typeof(AP1099Year.finYear.Concat<string0101>), typeof(string))]
        public virtual String StartDate
        {
            get
            {
                return this._StartDate;
            }
            set
            {
                this._StartDate = value;
            }
        }
        #endregion
        #region EndDate
        public abstract class endDate : PX.Data.BQL.BqlString.Field<endDate> { }
        protected String _EndDate;
        [PXDBCalced(typeof(AP1099Year.finYear.Concat<string1231>), typeof(string))]
        public virtual String EndDate
        {
            get
            {
                return this._EndDate;
            }
            set
            {
                this._EndDate = value;
            }
        }
        #endregion
        #region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new string[] { Open, Closed },
						new string[] { "Open", "Closed" }) { }
			}

			public const string Open = "N";
			public const string Closed = "C";

			public class open : PX.Data.BQL.BqlString.Constant<open>
			{
				public open() : base(Open) { ;}
			}

			public class closed : PX.Data.BQL.BqlString.Constant<closed>
			{
				public closed() : base(Closed) { ;}
			}
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(status.Open)]
		[status.List()]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
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
	}
}
