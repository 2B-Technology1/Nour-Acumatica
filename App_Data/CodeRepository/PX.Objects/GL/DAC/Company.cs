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
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.SM;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.GL
{
    /// <summary>
    /// Provides access to the settings of the company.
    /// The information related to the company is edited on the Branches (CS102000) form
    /// (which corresponds to the <see cref="BranchMaint"/> graph).
    /// </summary>
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(OrganizationMaint))]
	[ForceSaveInDash(false)]
	[PXCacheName(Messages.Company)]
	public partial class Company : PX.Data.IBqlTable
	{
		#region Keys
		public class UK : PrimaryKeyOf<Company>.By<companyCD>
		{
			public static Company Find(PXGraph graph, string companyCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, companyCD, options);
		}
		public static class FK
		{
			public class BaseCurrency : CM.Currency.PK.ForeignKeyOf<Company>.By<baseCuryID> { }
		}
		#endregion

		#region CompanyCD
		public abstract class companyCD : PX.Data.BQL.BqlString.Field<companyCD> { }
		protected String _CompanyCD;

        /// <summary>
        /// Unique user-friendly identifier of the Company.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="BAccount.AcctCD"/> field of the associated <see cref="BAccount">Business Account</see>.
        /// </value>
		[PXDBString(128, IsFixed = false, IsUnicode = true)]
		[PXDBDefault(typeof(BAccount.acctCD))]
		//[PXDimension("BIZACCT")]
		[PXUIField(DisplayName = "Company", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CompanyCD
		{
			get
			{
				return this._CompanyCD;
			}
			set
			{
				this._CompanyCD = value;
			}
		}
		#endregion
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		protected String _BaseCuryID;

        /// <summary>
        /// The base <see cref="Currency"/> of the company.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Currency.CuryID"/> field.
        /// </value>
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Base Currency ID")]
		[PXSelector(typeof(Search<CurrencyList.curyID>), DescriptionField = typeof(CurrencyList.description))]
		public virtual String BaseCuryID
		{
			get
			{
				return this._BaseCuryID;
			}
			set
			{
				this._BaseCuryID = value;
			}
		}
		#endregion
		#region PhoneMask
		public abstract class phoneMask : PX.Data.BQL.BqlString.Field<phoneMask> { }
		protected String _PhoneMask;

        /// <summary>
        /// The mask used to display phone numbers for all branches of the company.
        /// </summary>
		[PXDBString(50)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone Mask")]
		public virtual String PhoneMask
		{
			get
			{
				return this._PhoneMask;
			}
			set
			{
				this._PhoneMask = value;
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
