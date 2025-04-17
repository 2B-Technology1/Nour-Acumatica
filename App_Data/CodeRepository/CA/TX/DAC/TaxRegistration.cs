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

using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.TX;
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.Localizations.CA.TX
{
	/// <summary>
	/// The information about the tax registration number for the branch or organization of the specified tax type.
	/// </summary>
	[Serializable]
    [PXCacheName("Tax Registration")]
    public class TaxRegistration : IBqlTable
    {
		/// <summary>
		/// Tax registration primary key
		/// </summary>
		public class PK : PrimaryKeyOf<TaxRegistration>.By<bAccountID, taxID>
		{
			public static TaxRegistration Find(PXGraph graph, int? bAccountID, string taxID) => FindBy(graph, bAccountID, taxID);
		}

		/// <summary>
		/// Tax registration foreign key
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Referenced business account foreign key
			/// </summary>
			public class TaxRegistrationFK : Field<bAccountID>.IsRelatedTo<BAccount.bAccountID>.AsSimpleKey.WithTablesOf<BAccount, TaxRegistration> { }
		}

		#region BAccountID

		public abstract class bAccountID : BqlInt.Field<bAccountID> { }

		/// <summary>
		/// The <see cref="BAccount">business account</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(BAccount.bAccountID))]
        [PXParent(typeof(FK.TaxRegistrationFK))]
			//Select<BAccount,
   //             Where<BAccount.bAccountID,
   //                 Equal<Current<TaxRegistration.bAccountID>>>>))]
        public virtual int? BAccountID
        {
            get;
            set;
        }

        #endregion

        #region TaxID

        public abstract class taxID : BqlString.Field<taxID> { }

		/// <summary>
		/// The tax ID of the <see cref="Tax">tax</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Tax.TaxID"/> field.
		/// </value>
		[PXDBString(Tax.taxID.Length, IsKey = true, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Tax ID")]
        [PXSelector(typeof(
                Search<Tax.taxID,
                    Where<Tax.isExternal, Equal<False>>>),
            DescriptionField = typeof(Tax.descr))]
        public virtual string TaxID
        {
            get;
            set;
        }

        #endregion

        #region TaxRegistrationNumber

        public abstract class taxRegistrationNumber : BqlString.Field<taxRegistrationNumber> { }

		/// <summary>
		/// The tax registration number for the branch or organization of the specified tax type.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Tax Registration Number", Required = true)]
        public virtual string TaxRegistrationNumber
        {
            get;
            set;
        }

        #endregion

        #region System Columns

        #region Tstamp

        public abstract class tstamp : BqlByteArray.Field<tstamp> { }

        [PXDBTimestamp()]
        public virtual byte[] Tstamp
        {
            get;
            set;
        }

        #endregion

        #region CreatedByID

        public abstract class createdByID : BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get;
            set;
        }

        #endregion

        #region CreatedByScreenID

        public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID
        {
            get;
            set;
        }

        #endregion

        #region CreatedDateTime

        public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get;
            set;
        }

        #endregion

        #region LastModifiedByID

        public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get;
            set;
        }

        #endregion

        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID
        {
            get;
            set;
        }

        #endregion

        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get;
            set;
        }

        #endregion

        #endregion
    }
}
