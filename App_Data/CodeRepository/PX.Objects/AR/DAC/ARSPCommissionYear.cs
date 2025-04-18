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

namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	/// <summary>
	/// Represents a year in which salesperson commissions are
	/// calculated and to which commission periods (<see 
	/// cref="ARSPCommissionPeriod"/>) belong. The records of 
	/// this type are created during the Calculate Commissions 
	/// (AR505500) process, which corresponds to the <see 
	/// cref="ARSPCommissionProcess"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARSPCommissionYear)]
	public partial class ARSPCommissionYear : PX.Data.IBqlTable
	{
		#region Keys
		/// <exclude/>
		public class PK : PrimaryKeyOf<ARSPCommissionYear>.By<year>
		{
			public static ARSPCommissionYear Find(PXGraph graph, String year, PKFindOptions options = PKFindOptions.None) => FindBy(graph, year, options);
		}
		#endregion

		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		protected String _Year;
		[PXDBString(4, IsKey=true, IsFixed = true)]
		[PXDefault()]
		public virtual String Year
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
		#region Filed
		public abstract class filed : PX.Data.BQL.BqlBool.Field<filed> { }
		protected Boolean? _Filed;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Filed
		{
			get
			{
				return this._Filed;
			}
			set
			{
				this._Filed = value;
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