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
using PX.Objects.GL;

namespace PX.Objects.AP
{
	/// <summary>
	/// Represents the contents of a 1099 form box for a specific financial year and 1099 vendor.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.AP1099History)]
	public partial class AP1099History : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<AP1099History>.By<branchID, vendorID, finYear, boxNbr>
		{
			public static AP1099History Find(PXGraph graph, Int32? branchID, Int32? vendorID, String finYear, Int16? boxNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, branchID, vendorID, finYear, boxNbr, options);
		}

		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<AP1099History>.By<branchID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<AP1099History>.By<vendorID> { }
			public class Box : AP.AP1099Box.PK.ForeignKeyOf<AP1099History>.By<boxNbr> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;

		/// <summary>
		/// Identifier of the <see cref="Branch"/>, for which the record provides 1099 information.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch(useDefaulting: false, IsKey = true)]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;

		/// <summary>
		/// Identifier of the 1099 <see cref="Vendor"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Vendor.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		protected String _FinYear;

		/// <summary>
		/// Financial year, to which 1099 information belongs.
		/// </summary>
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXDefault()]
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
		#region BoxNbr
		public abstract class boxNbr : PX.Data.BQL.BqlShort.Field<boxNbr> { }
		protected Int16? _BoxNbr;

		/// <summary>
		/// The number (ordinal) of the box on 1099 form. (See <see cref="AP1099Box"/>)
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="AP1099Box.BoxNbr"/> field.
		/// </value>
		[PXDBShort(IsKey = true)]
		[PXDefault()]
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
		#region HistAmt
		public abstract class histAmt : PX.Data.BQL.BqlDecimal.Field<histAmt> { }
		protected Decimal? _HistAmt;

		/// <summary>
		/// The amount (YTD) of the corresponding type of payments paid to the vendor specified in the <see cref="VendorID"/> field.
		/// </summary>
		[CM.PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Amount", Visibility=PXUIVisibility.Visible, Enabled=false)]
		public virtual Decimal? HistAmt
		{
			get
			{
				return this._HistAmt;
			}
			set
			{
				this._HistAmt = value;
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
