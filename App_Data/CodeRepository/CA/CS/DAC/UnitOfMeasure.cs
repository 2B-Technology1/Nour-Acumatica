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
using System;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.Localizations.CA.CS
{
	/// <summary>
	/// Localizable descriptions for units of measure.
	/// </summary>
	// Acuminator disable once PX1034 MissingForeignKeyDeclaration [ForeignKey currently unnecessary for DAC]
	[Serializable]
    [PXCacheName("Unit of Measure")]
    [PXPrimaryGraph(typeof(UnitOfMeasureMaint))]
    public class UnitOfMeasure : IBqlTable
    {
		/// <summary>
		/// Unit of measure primary key
		/// </summary>
		public class PK : PrimaryKeyOf<UnitOfMeasure>.By<unit>
		{
			public static UnitOfMeasure Find(PXGraph graph, string unit) => FindBy(graph, unit);
		}

		#region Unit

		public abstract class unit : BqlString.Field<unit> { }

		/// <summary>
		/// The unit of measure.
		/// </summary>
		[PXDBString(6, IsUnicode = true, IsKey = true)]
        [PXDefault()]
		[PXUIField(DisplayName = "Unit ID")]
        [PXSelector(
            typeof(Search<UnitOfMeasure.unit>),
            typeof(UnitOfMeasure.unit),
            typeof(UnitOfMeasure.descr))]
        public virtual string Unit
        {
            get;
            set;
        }

        #endregion

        #region Descr

        public abstract class descr : BqlString.Field<descr> { }
		/// <summary>
		/// The localizable description for use in reports.
		/// </summary>
		[PXDBLocalizableString(6, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Description for Reports")]
        public virtual string Descr
        {
            get;
            set;
        }

        #endregion

        #region NoteID

        public abstract class noteID : BqlGuid.Field<noteID> { }

        [PXNote()]
        public virtual Guid? NoteID
        {
            get;
            set;
        }

        #endregion

        #region tstamp

        public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp()]
        public virtual byte[] tstamp
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

        [PXDBCreatedDateTime()]
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
    }
}
