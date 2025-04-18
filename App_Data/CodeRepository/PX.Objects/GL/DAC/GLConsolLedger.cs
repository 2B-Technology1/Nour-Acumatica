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
	[PXCacheName(Messages.GLConsolLedger)]
	public partial class GLConsolLedger : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<GLConsolHistory>.By<setupID, ledgerCD>
		{
			public static GLConsolHistory Find(PXGraph graph, Int32? setupID, String ledgerCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, setupID, ledgerCD, options);
		}
		#endregion

		#region SetupID
		public abstract class setupID : PX.Data.BQL.BqlInt.Field<setupID> { }
		protected Int32? _SetupID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? SetupID
		{
			get
			{
				return this._SetupID;
			}
			set
			{
				this._SetupID = value;
			}
		}
		#endregion
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
		protected String _LedgerCD;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual String LedgerCD
		{
			get
			{
				return this._LedgerCD;
			}
			set
			{
				this._LedgerCD = value;
			}
		}
		#endregion
		#region PostInterCompany
		public abstract class postInterCompany : PX.Data.BQL.BqlBool.Field<postInterCompany> { }
		protected Boolean? _PostInterCompany;

		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		[PXDBBool()]
		[PXUIField(DisplayName = "Generates Inter-Branch Transactions", Enabled = false)]
		public virtual Boolean? PostInterCompany
		{
			get
			{
				return this._PostInterCompany;
			}
			set
			{
				this._PostInterCompany = value;
			}
		}
		#endregion
		#region BalanceType
		public abstract class balanceType : PX.Data.BQL.BqlString.Field<balanceType> { }

		/// <summary>
		/// The type of balance of the ledger in the source company.
		/// </summary>
		/// <value>
		/// For more info see the <see cref="Ledger.BalanceType"/> field.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[LedgerBalanceType.List]
		[PXUIField(DisplayName = "Balance Type", Enabled = false)]
		public virtual String BalanceType { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
	}


	[PXProjection(typeof(Select<GLConsolLedger>))]
	public class GLConsolLedger2 : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<GLConsolHistory>.By<setupID, ledgerCD>
		{
			public static GLConsolHistory Find(PXGraph graph, Int32? setupID, String ledgerCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, setupID, ledgerCD, options);
		}
		#endregion

		#region SetupID
		public abstract class setupID : PX.Data.BQL.BqlInt.Field<setupID> { }
		protected Int32? _SetupID;
		[PXDBInt(IsKey = true, BqlField = typeof(GLConsolLedger.setupID))]
		public virtual Int32? SetupID
		{
			get
			{
				return this._SetupID;
			}
			set
			{
				this._SetupID = value;
			}
		}
		#endregion
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
		protected String _LedgerCD;
		[PXDBString(10, IsUnicode = true, IsKey = true, BqlField = typeof(GLConsolLedger.ledgerCD))]
		[PXDefault]
		public virtual String LedgerCD
		{
			get
			{
				return this._LedgerCD;
			}
			set
			{
				this._LedgerCD = value;
			}
		}
		#endregion
		#region BalanceType
		public abstract class balanceType : PX.Data.BQL.BqlString.Field<balanceType> { }

		[PXDBString(1, IsFixed = true, BqlField = typeof(GLConsolLedger.balanceType))]
		public virtual String BalanceType { get; set; }
		#endregion
	}
}
