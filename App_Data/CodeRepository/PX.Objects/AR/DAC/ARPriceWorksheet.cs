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
    using PX.Objects.IN;
    using PX.Objects.CM;
    using PX.Objects.TX;
    using PX.Objects.CS;
	using PX.Data.ReferentialIntegrity.Attributes;

	/// <summary>
	/// An accounts receivable price worksheet. Price worksheets are used 
	/// to conveniently make mass changes to <see cref="ARSalesPrice">sales prices</see>.
	/// Prices defined in the worksheet (<see cref="ARPriceWorksheetDetail"/>) update sales 
	/// prices upon worksheet release. The entities of this type can be edited on the
	/// Sales Price Worksheets (AR202010) form, which corresponds to the <see 
	/// cref="ARPriceWorksheetMaint"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(ARPriceWorksheetMaint))]
	[PXCacheName(Messages.ARPriceWorksheet)]
	public partial class ARPriceWorksheet : PX.Data.IBqlTable
    {
      #region Keys
      public class PK : PrimaryKeyOf<ARPriceWorksheet>.By<refNbr>
      {
         public static ARPriceWorksheet Find(PXGraph graph, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, refNbr, options);
      }
      #endregion

      #region RefNbr
      public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{           
            #region Numbering
            public class NumberingAttribute : AutoNumberAttribute
            {
                public NumberingAttribute()
                    : base(typeof(ARSetup.priceWSNumberingID), typeof(ARPriceWorksheet.effectiveDate)) { ; }
            }
            #endregion
        }
        protected String _RefNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<ARPriceWorksheet.refNbr>), new Type[] { typeof(ARPriceWorksheet.refNbr), typeof(ARPriceWorksheet.status), typeof(ARPriceWorksheet.descr), typeof(ARPriceWorksheet.effectiveDate) })]
        [refNbr.Numbering()]
        //[PXFieldDescription]
        public virtual String RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        protected String _Status;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(SPWorksheetStatus.Hold)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [SPWorksheetStatus.List()]
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
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;
        [PXDBBool()]
        [PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true)]
        //[PXNoUpdate]
        public virtual Boolean? Hold
        {
            get
            {
                return this._Hold;
            }
            set
            {
                this._Hold = value;
            }
        }
        #endregion
        #region Approved
        public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
        protected Boolean? _Approved;
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Boolean? Approved
        {
            get
            {
                return this._Approved;
            }
            set
            {
                this._Approved = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected String _Descr;
        [PXDBString(150, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
        #region EffectiveDate
        public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
        protected DateTime? _EffectiveDate;
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBDate()]
        [PXUIField(DisplayName = "Effective Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? EffectiveDate
        {
            get
            {
                return this._EffectiveDate;
            }
            set
            {
                this._EffectiveDate = value;
            }
        }
        #endregion
        #region ExpirationDate
        public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
        protected DateTime? _ExpirationDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.Visible)]
        public virtual DateTime? ExpirationDate
        {
            get
            {
                return this._ExpirationDate;
            }
            set
            {
                this._ExpirationDate = value;
            }
        }
        #endregion
        #region IsPromotional
        public abstract class isPromotional : PX.Data.BQL.BqlBool.Field<isPromotional> { }
        protected bool? _IsPromotional;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Promotional")]
        public virtual bool? IsPromotional
        {
            get
            {
                return _IsPromotional;
            }
            set
            {
                _IsPromotional = value;
            }
        }
		#endregion
		#region SkipLineDiscounts
		public abstract class skipLineDiscounts : PX.Data.BQL.BqlBool.Field<skipLineDiscounts> { }
		/// <summary>
		/// Default value for the Skip Line Discounts check box in the lines.
		/// </summary>
		[PXDBBool]
		[PXFormula(typeof(ARPriceWorksheet.isPromotional))]
		[PXUIField(DisplayName = "Ignore Automatic Line Discounts")]
		[PXDefault()]
		public virtual bool? SkipLineDiscounts { get; set; }
		#endregion
		#region IsFairValue
		public abstract class isFairValue : PX.Data.BQL.BqlBool.Field<isFairValue> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the price
		/// will be used in revenue reallocaiton process
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Fair Value", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual bool? IsFairValue { get; set; }
		#endregion
		#region IsProrated
		public abstract class isProrated : PX.Data.BQL.BqlBool.Field<isProrated> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the price
		/// will be increased or decreased proportionaly the term
		/// specified on invoice line
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Prorated", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual bool? IsProrated { get; set; }
		#endregion
		#region Discountable
		public abstract class discountable : PX.Data.BQL.BqlBool.Field<discountable> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the discount can be applied
		/// to a fair value price (that is, when the <see cref="ARPriceWorksheet.IsFairValue"/> property is <see langword="true" />)
		/// in the revenue reallocation process in a deferral schedule.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Apply Discounts to Fair Value", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual bool? Discountable { get; set; }
		#endregion
		#region OverwriteOverlapping
		public abstract class overwriteOverlapping : PX.Data.BQL.BqlBool.Field<overwriteOverlapping> { }

        protected bool? _OverwriteOverlapping;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Overwrite Overlapping Prices")]
        public virtual bool? OverwriteOverlapping
        {
            get
            {
                return _OverwriteOverlapping;
            }
            set
            {
                _OverwriteOverlapping = value;
            }
        }
        #endregion
        #region TaxCalcMode
        public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(PriceTaxCalculationMode.Undefined)]
        [PriceTaxCalculationMode.List]
        [PXUIField(DisplayName = "Tax Calculation Mode")]
        public virtual string TaxCalcMode
        {
            get;
            set;
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion

        #region System Columns
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
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #endregion
    }
    public class SPWorksheetStatus
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Hold, Open, PendingApproval, Released },
                new string[] { Messages.Hold, Messages.Open, Messages.PendingApproval, Messages.Released }) { ; }
        }

        public const string Hold = "H";
        public const string Open = "N";
        public const string PendingApproval = "P";
        public const string Released = "R";

        public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
            public hold() : base(Hold) { ;}
        }

        public class open : PX.Data.BQL.BqlString.Constant<open>
		{
            public open() : base(Open) { ;}
        }

        public class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
		{
            public pendingApproval() : base(PendingApproval) { ;}
        }

        public class released : PX.Data.BQL.BqlString.Constant<released>
		{
            public released() : base(Released) { ;}
        }

    }

    public class PriceTaxCalculationMode
    {
        public const string Net = TX.TaxCalculationMode.Net;
        public const string Gross = TX.TaxCalculationMode.Gross;
        public const string AllModes = "A";
        public const string Undefined = "U";

        public class ListWithAllAttribute : PXStringListAttribute
        {
            public ListWithAllAttribute() : base(new[] { AllModes, Gross, Net, Undefined }, new[] { Messages.AllModes, Messages.TaxGross, Messages.TaxNet, Messages.Undefined}) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute() : base(new[] { Gross, Net, Undefined }, new[] { AP.Messages.TaxGross, AP.Messages.TaxNet, Messages.Undefined }) { }
        }

        public class gross : PX.Data.BQL.BqlString.Constant<gross>
        {
            public gross() : base(Gross) { }
        }

        public class net : PX.Data.BQL.BqlString.Constant<net>
        {
            public net() : base(Net) { }
        }

        public class allModes : PX.Data.BQL.BqlString.Constant<allModes>
        {
            public allModes() : base(AllModes) { }
        }

        public class undefined : PX.Data.BQL.BqlString.Constant<undefined>
        {
            public undefined() : base(Undefined) { }
        }
    }
}
