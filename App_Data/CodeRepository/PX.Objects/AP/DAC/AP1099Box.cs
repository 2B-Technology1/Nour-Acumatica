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

namespace PX.Objects.AP
{
	using System;
	using PX.Common;
	using PX.Data;
	using PX.Data.BQL.Fluent;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.GL;
	
	public class Box1099NumberSelectorAttribute : PXSelectorAttribute
	{
		public Box1099NumberSelectorAttribute()
			: base(typeof(SearchFor<AP1099Box.boxNbr>.In<SelectFrom<AP1099Box>>), fieldList: new Type[] {
					typeof(AP1099Box.boxCD),
					typeof(AP1099Box.descr)
				})
		{
			CacheGlobal = true;
			SubstituteKey = typeof(AP1099Box.boxCD);
			DescriptionField = typeof(AP1099Box.descr);
		}
	}

    /// <summary>
    /// Represents a type of 1099 payment, which generally corresponds to a box on the 1099-MISC form.
    /// This DAC is used by Acumatica ERP to track 1099-related payments.
    /// 1099 boxes are configured through the Accounts Payable Preferences (AP101000) form.
    /// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.AP1099Box)]
	public partial class AP1099Box : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<AP1099Box>.By<boxNbr>
		{
			public static AP1099Box Find(PXGraph graph, Int16? boxNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, boxNbr, options);
		}
		public class UK : PrimaryKeyOf<AP1099Box>.By<boxCD>
		{
			public static AP1099Box Find(PXGraph graph, string boxCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, boxCD, options);
		}

		public static class FK
		{
			public class Account : GL.Account.PK.ForeignKeyOf<AP1099Box>.By<accountID> { }
		}
		#endregion

		#region BoxNbr
		public abstract class boxNbr : PX.Data.BQL.BqlShort.Field<boxNbr> { }
		protected Int16? _BoxNbr;

        /// <summary>
        /// The line number, which is automatically added. A box is used for each payment made to a 1099 vendor.
        /// </summary>
		[PXDBShort(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "1099 Box", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual Int16? BoxNbr
		{
			get
			{
				return this._BoxNbr;
			}
			set
			{
				this._BoxNbr = value;
			}
		}
		#endregion

		#region BoxCD
		/// <summary>
		/// Key field.
		/// The user-friendly unique identifier of the 1099 Box ID.
		/// </summary>
		public abstract class boxCD : PX.Data.BQL.BqlString.Field<boxCD> { }
		[PXDBString(20, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "1099 Box", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String BoxCD { get; set; }
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

		/// <summary>
		/// The description of the 1099 type, which usually is based on the box name on the 1099-MISC form.
		/// </summary>
		[PXDBString(60, IsUnicode=true)]
		[PXUIField(DisplayName="Description", Visibility=PXUIVisibility.Visible, Enabled=false)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region MinReportAmt
		public abstract class minReportAmt : PX.Data.BQL.BqlDecimal.Field<minReportAmt> { }
		protected Decimal? _MinReportAmt;

        /// <summary>
        /// The minimum payment amount for the 1099 type to be included for reporting.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Minimum Report Amount", Visibility=PXUIVisibility.Visible)]
		public virtual Decimal? MinReportAmt
		{
			get
			{
				return this._MinReportAmt;
			}
			set
			{
				this._MinReportAmt = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;

        /// <summary>
        /// The optional default expense account associated with this type of 1099 payment.
        /// </summary>
        /// <value>
        /// Serves as a link to <see cref="Account"/>.
        /// </value>
		[UnboundAccount(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
		[AvoidControlAccounts]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region OldAccountID
		public abstract class oldAccountID : PX.Data.BQL.BqlInt.Field<oldAccountID> { }
		protected Int32? _OldAccountID;
        /// <summary>
        /// System field used to perform two sided update of the 1099Box-<see cref="Account"/> relation.
        /// </summary>
		[PXInt()]
		public virtual Int32? OldAccountID
		{
			get
			{
				return this._OldAccountID;
			}
			set
			{
				this._OldAccountID = value;
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
