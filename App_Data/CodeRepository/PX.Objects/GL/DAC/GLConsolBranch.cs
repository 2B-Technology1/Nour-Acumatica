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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.GL
{
	[Serializable()]
	[PXCacheName(Messages.GLConsolBranch)]
	public partial class GLConsolBranch : IBqlTable 
	{
		#region Keys
		public class PK : PrimaryKeyOf<GLConsolBranch>.By<setupID, branchCD>
		{
			public static GLConsolBranch Find(PXGraph graph, Int32? setupID, String branchCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, setupID, branchCD, options);
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
		#region BranchCD
		public abstract class branchCD : PX.Data.BQL.BqlString.Field<branchCD> { }
		protected String _BranchCD;
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")] //InputMask = "" for using dash ("-") character
		[PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXDefault]
		public virtual String BranchCD
		{
			get
			{
				return this._BranchCD;
			}
			set
			{
				this._BranchCD = value;
			}
		}
		#endregion
		#region OrganizationCD
		public abstract class organizationCD : PX.Data.BQL.BqlString.Field<organizationCD> { }
		[PXDBString(30, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Company", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual string OrganizationCD { get; set; }
		#endregion
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
		protected String _LedgerCD;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region IsOrganization
		public abstract class isOrganization : PX.Data.BQL.BqlBool.Field<isOrganization> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsOrganization { get; set; }
		#endregion
		#region DisplayName
		public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Company/Branch", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(
			IIf<organizationCD.IsNotNull.And<isOrganization.IsNull.Or<isOrganization.IsEqual<False>>>,
					organizationCD.Concat<HyphenSpace>.Concat<branchCD>,
					branchCD>
			))]
		public virtual string DisplayName { get; set; }
		#endregion
	}
}
