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

namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLConsolData)]
	public partial class GLConsolData : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<GLConsolData>.By<accountCD, mappedValue>
		{
			public static GLConsolData Find(PXGraph graph, String accountCD, String mappedValue, PKFindOptions options = PKFindOptions.None) => FindBy(graph, accountCD, mappedValue, options);
		}
		#endregion

		#region AccountCD
		public abstract class accountCD : PX.Data.BQL.BqlString.Field<accountCD> { }
		protected String _AccountCD;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Account")]
		public virtual String AccountCD
		{
			get
			{
				return this._AccountCD;
			}
			set
			{
				this._AccountCD = value;
			}
		}
		#endregion
		#region MappedValue
		public abstract class mappedValue : PX.Data.BQL.BqlString.Field<mappedValue> { }
		protected String _MappedValue;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Mapped Sub.")]
		public virtual String MappedValue
		{
			get
			{
				return this._MappedValue;
			}
			set
			{
				this._MappedValue = value;
			}
		}
		#endregion
		#region FinPeriod
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey = true, IsDBField = false)]
		[PXDefault()]
		[PXUIField(DisplayName = "Fin. Period")]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region ConsolAmtCredit
		public abstract class consolAmtCredit : PX.Data.BQL.BqlDecimal.Field<consolAmtCredit> { }
		protected Decimal? _ConsolAmtCredit;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Credit Amount")]
		public virtual Decimal? ConsolAmtCredit
		{
			get
			{
				return this._ConsolAmtCredit;
			}
			set
			{
				this._ConsolAmtCredit = value;
			}
		}
		#endregion
		#region ConsolAmtDebit
		public abstract class consolAmtDebit : PX.Data.BQL.BqlDecimal.Field<consolAmtDebit> { }
		protected Decimal? _ConsolAmtDebit;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Debit Amount")]
		public virtual Decimal? ConsolAmtDebit
		{
			get
			{
				return this._ConsolAmtDebit;
			}
			set
			{
				this._ConsolAmtDebit = value;
			}
		}
		#endregion
		#region MappedValueLength
		public abstract class mappedValueLength : PX.Data.BQL.BqlInt.Field<mappedValueLength> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Mapped Sub. Length")]
		public virtual int? MappedValueLength { get; set; }
		#endregion
	}
}
