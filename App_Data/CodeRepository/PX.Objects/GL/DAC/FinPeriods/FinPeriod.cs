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
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common.Abstractions.Periods;
using PX.Objects.CS;

namespace PX.Objects.GL.FinPeriods.TableDefinition
{
	[Serializable]
	[PXCacheName(Messages.FinancialPeriod)]
	public class FinPeriod : IBqlTable, IFinPeriod
	{
		#region Keys
		public class PK : PrimaryKeyOf<FinPeriod>.By<organizationID, finPeriodID>
		{
			public static FinPeriod Find(PXGraph graph, int? organizationID, string finPeriodID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, organizationID, finPeriodID, options);
			public static FinPeriod FindByBranch(PXGraph graph, int? branchID, string finPeriodID)
				=> (FinPeriod)PXSelectJoin<FinPeriod, InnerJoin<Branch, On<Branch.organizationID, Equal<FinPeriod.organizationID>>>,
									Where<finPeriodID, Equal<Required<finPeriodID>>, And<Branch.branchID, Equal<Required<Branch.branchID>>>>>
									.SelectWindowed(graph, 0, 1, finPeriodID, branchID);
		}
		public static class FK
		{
			public class Organization : GL.DAC.Organization.PK.ForeignKeyOf<FinPeriod>.By<organizationID> { }

			public class FinYear : GL.FinPeriods.TableDefinition.FinYear.PK.ForeignKeyOf<FinPeriod>.By<organizationID, finYear> { }
		}
		#endregion

		public class Key: OrganizationDependedPeriodKey
        {
            
        }

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		/// <summary>
		/// Indicates whether the record is selected for mass processing.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; } = false;
		#endregion

		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID>
		{
			public class masterValue : PX.Data.BQL.BqlInt.Constant<masterValue>
			{
				public masterValue() : base(MasterValue) { }
			}

			public const int MasterValue = 0;
		}

		[PXDBInt(IsKey = true)]
		public virtual int? OrganizationID { get; set; }
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;

		/// <summary>
		/// Key field.
		/// Unique identifier of the Financial Period.
		/// </summary>
		/// Consists of the year and the number of the period in the year. For more information see <see cref="FinPeriodIDAttribute"/>.
		[FinPeriodID(IsKey = true)]
		[PXDefault()]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false, DisplayName = "Financial Period ID")]
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

		#region MasterFinPeriodID
		public abstract class masterFinPeriodID : PX.Data.BQL.BqlString.Field<masterFinPeriodID> { }

		[FinPeriodID]
		public virtual String MasterFinPeriodID { get; set; }

		#endregion

		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		protected String _FinYear;

		/// <summary>
		/// The financial year of the period.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="FinYear.Year"/> field.
		/// </value>
		[PXDBString(4, IsFixed = true)]
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

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

		/// <summary>
		/// The description of the Financial Period.
		/// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
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

		#region PeriodNbr
		public abstract class periodNbr : PX.Data.BQL.BqlString.Field<periodNbr> { }
		protected String _PeriodNbr;

		/// <summary>
		/// The number of the period in the <see cref="FinYear">financial year</see>.
		/// </summary>
		[PXDBString(2, IsFixed = true)]
		[PXDefault("")]
		public virtual String PeriodNbr
		{
			get
			{
				return this._PeriodNbr;
			}
			set
			{
				this._PeriodNbr = value;
			}
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
				   new string[] { Inactive, Open, Closed, Locked },
				   new string[] { Messages.Inactive, Messages.Open, Messages.Closed, Messages.Locked })
				{ }
			}

			public const string Inactive = "Inactive";
			public const string Open = "Open";
			public const string Closed = "Closed";
			public const string Locked = "Locked";

			public class inactive : PX.Data.BQL.BqlString.Constant<inactive>
			{
				public inactive() : base(Inactive) { }
			}
			public class open : PX.Data.BQL.BqlString.Constant<open>
			{
				public open() : base(Open) { }
			}
			public class closed : PX.Data.BQL.BqlString.Constant<closed>
			{
				public closed() : base(Closed) { }
			}
			public class locked : PX.Data.BQL.BqlString.Constant<locked>
			{
				public locked() : base(Locked) { }
			}

		}
		/// <summary>
		/// The status of the financial period.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="status.ListAttribute"/>.
		/// </value>
		[PXDBString(8/*, IsFixed = true*/)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[status.List]
		[PXDefault(status.Inactive)]
		public virtual String Status { get; set; }
		#endregion

		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

        [Obsolete(PX.Objects.Common.Messages.FieldIsObsoleteAndWillBeRemoved2019R1)]
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active", Visible = false /* TODO: eliminate */)]
		[PXFormula(typeof(Switch<Case<
			Where<FinPeriod.status, Equal<FinPeriod.status.open>,
				Or<FinPeriod.status, Equal<FinPeriod.status.closed>>>, True>, False>))]
		public virtual Boolean? Active { get; set; }

		#endregion
		#region Closed
		public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }

	    [Obsolete(PX.Objects.Common.Messages.FieldIsObsoleteAndWillBeRemoved2019R1)]
        [PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in GL", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible /* TODO: eliminate */)]
		[PXFormula(typeof(Switch<Case<
			Where<FinPeriod.status, Equal<FinPeriod.status.locked>,
				Or<FinPeriod.status, Equal<FinPeriod.status.closed>>>, True>, False>))]
		public virtual Boolean? Closed { get; set; }
		#endregion

		#region APClosed
		public abstract class aPClosed : PX.Data.BQL.BqlBool.Field<aPClosed> { }
		protected Boolean? _APClosed;

		/// <summary>
		/// Indicates whether the period is closed in the Accounts Payable module.
		/// When <c>false</c>, the period is active in AP.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in AP", Enabled = false)]
		public virtual Boolean? APClosed
		{
			get
			{
				return this._APClosed;
			}
			set
			{
				this._APClosed = value;
			}
		}
		#endregion
		#region ARClosed
		public abstract class aRClosed : PX.Data.BQL.BqlBool.Field<aRClosed> { }
		protected Boolean? _ARClosed;

		/// <summary>
		/// Indicates whether the period is closed in the Accounts Receivable module.
		/// When <c>false</c>, the period is active in AR.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in AR", Enabled = false)]
		public virtual Boolean? ARClosed
		{
			get
			{
				return this._ARClosed;
			}
			set
			{
				this._ARClosed = value;
			}
		}
		#endregion
		#region INClosed
		public abstract class iNClosed : PX.Data.BQL.BqlBool.Field<iNClosed> { }
		protected Boolean? _INClosed;

		/// <summary>
		/// Indicates whether the period is closed in the Inventory module.
		/// When <c>false</c>, the period is active in IN.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in IN", Enabled = false)]
		public virtual Boolean? INClosed
		{
			get
			{
				return this._INClosed;
			}
			set
			{
				this._INClosed = value;
			}
		}
		#endregion
		#region CAClosed
		public abstract class cAClosed : PX.Data.BQL.BqlBool.Field<cAClosed> { }
		protected Boolean? _CAClosed;

		/// <summary>
		/// Indicates whether the period is closed in the Cash Management module.
		/// When <c>false</c>, the period is active in CA.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in CA", Enabled = false)]
		public virtual Boolean? CAClosed
		{
			get
			{
				return this._CAClosed;
			}
			set
			{
				this._CAClosed = value;
			}
		}
		#endregion
		#region FAClosed
		public abstract class fAClosed : PX.Data.BQL.BqlBool.Field<fAClosed> { }
		protected Boolean? _FAClosed;

		/// <summary>
		/// Indicates whether the period is closed in the Fixed Assets module.
		/// When <c>false</c>, the period is active in FA.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in FA", Enabled = false)]
		public virtual Boolean? FAClosed
		{
			get
			{
				return this._FAClosed;
			}
			set
			{
				this._FAClosed = value;
			}
		}
        #endregion
        #region PRClosed
        public abstract class pRClosed : PX.Data.BQL.BqlBool.Field<pRClosed>
		{
        }
        protected Boolean? _PRClosed;

        /// <summary>
        /// Indicates whether the period is closed in the Payroll module.
        /// When <c>false</c>, the period is active in PR.
        /// This field is relevant only if the <see cref="FeaturesSet.PayrollModule">Payroll feature</see> is enabled.
        /// </summary>
		[PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Closed in PR", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollModule))]
        public virtual Boolean? PRClosed
        {
            get
            {
                return this._PRClosed;
            }
            set
            {
                this._PRClosed = value;
            }
        }
        #endregion

		#region StartDateUI
		public abstract class startDateUI : PX.Data.BQL.BqlDateTime.Field<startDateUI> { }

		/// <summary>
		/// The field used to display and edit the <see cref="StartDate"/> of the period (inclusive) in the UI.
		/// </summary>
		/// <value>
		/// Depends on and changes the value of the <see cref="StartDate"/> field, performing additional transformations.
		/// </value>
		[PXDate]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? StartDateUI
		{
			[PXDependsOnFields(typeof(FinPeriod.startDate), typeof(FinPeriod.endDate))]
			get
			{
				return IsAdjustment == true ? _StartDate.Value.AddDays(-1) : _StartDate;
			}
			set
			{
				_StartDate = (value != null && _EndDate != null && value == EndDateUI) ? value.Value.AddDays(1) : value;
			}
		}
		#endregion
		#region EndDateUI
		public abstract class endDateUI : PX.Data.BQL.BqlDateTime.Field<endDateUI> { }

		/// <summary>
		/// The field used to display and edit the <see cref="EndDate"/> of the period (inclusive) in the UI.
		/// </summary>
		/// <value>
		/// Depends on and changes the value of the <see cref="EndDate"/> field, performing additional transformations.
		/// </value>
		[PXDate]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual DateTime? EndDateUI
		{
			[PXDependsOnFields(typeof(FinPeriod.endDate))]
			get
			{
				return _EndDate?.AddDays(-1);
			}
			set
			{
				_EndDate = value?.AddDays(1);
			}
		}
		#endregion

        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;

		/// <summary>
		/// The start date of the period.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;

		/// <summary>
		/// The end date of the period (exclusive).
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "EndDate", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region DateLocked
		public abstract class dateLocked : PX.Data.BQL.BqlBool.Field<dateLocked> { }
		protected Boolean? _DateLocked;

		/// <summary>
		/// When <c>true</c>, indicates that the <see cref="EndDate"/> of the period is locked and can not be edited.
		/// </summary>
		/// <value>
		/// Note, that if the date is locked for a particular period, it is also impossible to change end dates for the preceding periods.
		/// </value>
		[PXDBBool()]
		[PXUIField(DisplayName = "Date Locked", Enabled = false, Visible = false)]
		public virtual Boolean? DateLocked
		{
			get
			{
				return this._DateLocked;
			}
			set
			{
				this._DateLocked = value;
			}
		}
		#endregion

		#region Custom
		public abstract class custom : PX.Data.BQL.BqlBool.Field<custom> { }
		protected Boolean? _Custom;

		/// <summary>
		/// Indicates whether the start and end dates of the Financial Period are defined by user.
		/// </summary>
		/// <value>
		/// Defaults to the value of the <see cref="FinYear.CustomPeriods"/> field of the year of the period.
		/// </value>
		[PXDBBool()]
		public virtual Boolean? Custom
		{
			get
			{
				return this._Custom;
			}
			set
			{
				this._Custom = value;
			}
		}
		#endregion

		#region FinDate
		public abstract class finDate : PX.Data.BQL.BqlDateTime.Field<finDate> { }
		protected DateTime? _FinDate;

		/// <summary>
		/// The date used for <see cref="TaxTran">tax transactions</see> posted in the period.
		/// See the <see cref="TaxTran.FinDate"/> field.
		/// </summary>
		/// <value>
		/// The value of this field is calculated from the <see cref="FinPeriod.EndDate"/> field.
		/// </value>
		[PXDate]
		[PXUIField(DisplayName = "Fin. Date", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXDBCalced(typeof(Sub<FinPeriod.endDate, PX.Objects.CS.int1>), typeof(DateTime))]
		public virtual DateTime? FinDate
		{
			get
			{
				return this._FinDate;
			}
			set
			{
				this._FinDate = value;
			}
		}
		#endregion
		#region Length
		public abstract class length : PX.Data.BQL.BqlInt.Field<length> { }
		protected Int32? _Length;

		/// <summary>
		/// The read-only length of the period in days.
		/// </summary>
		[PXInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Length (Days)", Visible = true, Enabled = false)]
		public virtual Int32? Length
		{
			get
			{
				if (_StartDate != null && _EndDate != null && _StartDate != _EndDate)
				{
					return ((TimeSpan)(_EndDate - _StartDate)).Days;
				}
				else
					return 0;
			}
		}
		#endregion
		#region IsAdjustment
		public abstract class isAdjustment : PX.Data.BQL.BqlBool.Field<isAdjustment> { }

		/// <summary>
		/// Indicates whether the period is an adjustment period - an additional period used to post adjustments.
		/// The adjustment period can be created only if the <see cref="FinYear.CustomPeriod"/> option is activated for the corresponding Financial Year.
		/// For more information see the <see cref="FinYearSetup.HasAdjustmentPeriod"/> field and
		/// the documentation for the Financial Year (GL101000) form.
		/// </summary>
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Adjustment Period", Visible = false, Enabled = false)]
		[PXDBCalced(typeof(Switch<Case<Where<startDate, Equal<endDate>>, True>, False>), typeof(bool))]
		public virtual bool? IsAdjustment { get; set; }

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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
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
		[PXDBCreatedDateTime()]
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
		[PXDBLastModifiedDateTime()]
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
	}
	public class FinPeriod2 : FinPeriod
	{
		#region OrganizationID
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion
		#region StartDate
		public new abstract class startDate : IBqlField { }
		#endregion
		#region EndDate
		public new abstract class endDate : IBqlField { }
		#endregion
		#region Descr
		public new abstract class descr : IBqlField { }
		#endregion
		#region Status
		public new abstract class status : IBqlField { }
		#endregion
		#region Closed
		public new abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }
		#endregion
		#region APClosed
		public new abstract class aPClosed : PX.Data.BQL.BqlBool.Field<aPClosed> { }
		#endregion
		#region ARClosed
		public new abstract class aRClosed : PX.Data.BQL.BqlBool.Field<aRClosed> { }
		#endregion
		#region INClosed
		public new abstract class iNClosed : PX.Data.BQL.BqlBool.Field<iNClosed> { }
		#endregion
		#region CAClosed
		public new abstract class cAClosed : PX.Data.BQL.BqlBool.Field<cAClosed> { }
		#endregion
		#region FAClosed
		public new abstract class fAClosed : PX.Data.BQL.BqlBool.Field<fAClosed> { }
		#endregion
		#region DateLocked
		public new abstract class dateLocked : PX.Data.BQL.BqlBool.Field<dateLocked> { }
		#endregion
		#region Active
		public new abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion
		#region FinYear
		public new abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		#endregion
		#region PeriodNbr
		public new abstract class periodNbr : PX.Data.BQL.BqlString.Field<periodNbr> { }
		#endregion
		#region Custom
		public new abstract class custom : PX.Data.BQL.BqlBool.Field<custom> { }
		#endregion
		#region tstamp
		public new abstract class tstamp : IBqlField { }
		#endregion
		#region CreatedByID
		public new abstract class createdByID : IBqlField { }
		#endregion
		#region CreatedByScreenID
		public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : IBqlField { }
		#endregion
		#region LastModifiedByScreenID
		public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#region NoteID
		public new abstract class noteID : IBqlField { }
		#endregion
		#region MasterFinPeriodID
		public new abstract class masterFinPeriodID : PX.Data.BQL.BqlString.Field<masterFinPeriodID> { }
		#endregion
	}
}
