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
using PX.Data;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PM
{
	/// <summary>
	/// Represents a batch of <see cref="PMTran">project transactions</see>.
	/// The records of this type are created through the Project Transactions (PM304000) form
	/// (which corresponds to the <see cref="RegisterEntry"/> graph), 
	/// or can also originate from other documents.
	/// </summary>
	[PXCacheName(Messages.PMRegister)]
	[PXPrimaryGraph(typeof(RegisterEntry))]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMRegister : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;

		/// <summary>
		/// The identifier of the functional area, to which the batch belongs.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// "GL", "AP", "AR", "IN", "PM", "CA", "DR", "PR".
		/// </value>
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(BatchModule.PM)]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		[BatchModule.PMListAttribute()]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
			public const int Length = 15;
		}
		protected String _RefNbr;

		/// <summary>
		/// The reference number of the document.
		/// </summary>
		/// <value>
		/// The number is generated from the <see cref="Numbering">numbering sequence</see>, 
		/// which is specified on the <see cref="PMSetup">Project Preferences</see> form.
		/// </value>
		[PXDBString(PMRegister.refNbr.Length, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXSelector(typeof(Search<PMRegister.refNbr, Where<PMRegister.module, Equal<Current<PMRegister.module>>>, OrderBy<Desc<PMRegister.refNbr>>>), Filterable = true)]
		[PXUIField(DisplayName = "Ref. Number", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(Search<PMSetup.tranNumbering>), typeof(AccessInfo.businessDate))]
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
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		protected DateTime? _Date;

		/// <summary>
		/// The date of the document.
		/// </summary>
		/// <value>Defaults to the current <see cref="AccessInfo.BusinessDate">business date</see>.</value>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? Date
		{
			get
			{
				return this._Date;
			}
			set
			{
				this._Date = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;

		/// <summary>
		/// The description of the document.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFieldDescription]
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
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
            #region List

	        public class ListAttribute : PXStringListAttribute
	        {
		        public ListAttribute() : base(
			        new[]
					{
						Pair(Hold, Messages.Hold),
						Pair(Balanced, Messages.Balanced),
						Pair(Released, Messages.Released),
					}) {}
	        }

	        public const string Hold = "H";
            public const string Balanced = "B";
            public const string Released = "R";

            public class hold : PX.Data.BQL.BqlString.Constant<hold>
			{
                public hold() : base(Hold) { ;}
            }

            public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
			{
                public balanced() : base(Balanced) { ;}
            }

            public class released : PX.Data.BQL.BqlString.Constant<released>
			{
                public released() : base(Released) { ;}
            }
            #endregion
        }
        protected String _Status;

		/// <summary>
		/// The read-only status of the document.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"H"</c>: Hold,
		/// <c>"B"</c>: Balanced,
		/// <c>"R"</c>: Released
		/// </value>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(status.Balanced)]
        [status.List()]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual String Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
            }
        }
        #endregion
        #region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the document has been released.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName="Released", Enabled=false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				_Released = value;
            }
		}
		#endregion
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the document is on hold.
		/// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "On Hold")]
        public virtual Boolean? Hold
        {
            get
            {
                return _Hold;
            }
            set
            {
                _Hold = value;
            }
        }
        #endregion
		#region IsAllocation
		public abstract class isAllocation : PX.Data.BQL.BqlBool.Field<isAllocation> { }
		protected Boolean? _IsAllocation;

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the batch was created as a result of the allocation process.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsAllocation
		{
			get
			{
				return this._IsAllocation;
			}
			set
			{
				_IsAllocation = value;
			}
		}
		#endregion
        
		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		protected String _OrigDocType;

		/// <summary>
		/// The type of the original document.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"AL"</c>: Allocation,
		/// <c>"TC"</c>: Time Card,
		/// <c>"CS"</c>: Case,
		/// <c>"EC"</c>: Expense Claim,
		/// <c>"ET"</c>: Equipment Time Card,
		/// <c>"AR"</c>: Allocation Reversal,
		/// <c>"RV"</c>: Reversal,
		/// <c>"IN"</c>: Invoice,
		/// <c>"CR"</c>: Credit Memo,
		/// <c>"DM"</c>: Debit Memo,
		/// <c>"UR"</c>: Unbilled Remainder,
		/// <c>"RR"</c>: Unbilled Remainder Reversal,
		/// <c>"PB"</c>: Pro Forma Billing,
		/// <c>"BL"</c>: Bill,
		/// <c>"CA"</c>: Credit Adjustment,
		/// <c>"DA"</c>: Debit Adjustment,
		/// <c>"WR"</c>: WIP Reversal,
		/// <c>"AP"</c>: Service Order,
		/// <c>"SO"</c>: Appointment,
		/// <c>"PR"</c>: Regular Paycheck,
		/// <c>"PS"</c>: Special Paycheck,
		/// <c>"PA"</c>: Adjustment Paycheck,
		/// <c>"PV"</c>: Void Paycheck
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PMOrigDocType.List()]
		[PXUIField(DisplayName = "Orig. Doc. Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String OrigDocType
		{
			get
			{
				return this._OrigDocType;
			}
			set
			{
				this._OrigDocType = value;
			}
		}
		#endregion
		#region OrigDocNbr
		public abstract class origDocNbr : PX.Data.BQL.BqlString.Field<origDocNbr> { }
		protected String _OrigDocNbr;

		/// <summary>
		/// The reference number of the original document.
		/// </summary>
		[PXDBString]
		[PXUIField(DisplayName = "Orig. Doc. Nbr.", Visible = false, Enabled = false)]
		public virtual String OrigDocNbr
		{
			get
			{
				return this._OrigDocNbr;
			}
			set
			{
				this._OrigDocNbr = value;
			}
		}
		#endregion
		#region OrigNoteID
		public abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }
		/// <summary>
		/// NoteID of the original document.
		/// </summary>
		[PXRefNote(LastKeyOnly = true)]
		[PXUIField(DisplayName = "Orig. Doc. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Guid? OrigNoteID
		{
			get;
			set;
		}
		#endregion

		#region QtyTotal
		public abstract class qtyTotal : PX.Data.BQL.BqlDecimal.Field<qtyTotal> { }
		protected Decimal? _QtyTotal;

		/// <summary>
		/// The total quantity of items in the <see cref="PMTran">project transactions</see>.
		/// </summary>
		[PXDBQuantity]
		[PXUIField(DisplayName = "Total Quantity", Enabled = false)]
		public virtual Decimal? QtyTotal
		{
			get
			{
				return this._QtyTotal;
			}
			set
			{
				this._QtyTotal = value;
			}
		}
		#endregion
		#region BillableQtyTotal
		public abstract class billableQtyTotal : PX.Data.BQL.BqlDecimal.Field<billableQtyTotal> { }
		protected Decimal? _BillableQtyTotal;

		/// <summary>
		/// The total billable quantity for the <see cref="PMTran">project transactions</see>.
		/// </summary>
		[PXDBQuantity]
		[PXUIField(DisplayName = "Total Billable Quantity", Enabled = false)]
		public virtual Decimal? BillableQtyTotal
		{
			get
			{
				return this._BillableQtyTotal;
			}
			set
			{
				this._BillableQtyTotal = value;
			}
		}
		#endregion
		#region AmtTotal
		public abstract class amtTotal : PX.Data.BQL.BqlDecimal.Field<amtTotal> { }
		protected Decimal? _AmtTotal;

		/// <summary>
		/// The total amount for the <see cref="PMTran">project transactions</see> in the base currency.
		/// </summary>
		[PXDBQuantity]
		[PXUIField(DisplayName = "Total Amount", Enabled = false)]
		public virtual Decimal? AmtTotal
		{
			get
			{
				return this._AmtTotal;
			}
			set
			{
				this._AmtTotal = value;
			}
		}
		#endregion

		#region IsMigratedRecord

		public abstract class isMigratedRecord : BqlBool.Field<isMigratedRecord> { }

		/// <summary>
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Migrated", Visible = false, Enabled = false)]
		[PXDefault(false)]
		public virtual bool? IsMigratedRecord { get; set; }

		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
        [PXNote]
		[NotePersist(typeof(noteID))]
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
		[PXDBCreatedByID]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedByID]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
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

		#region IsBaseCury
		public abstract class isBaseCury : PX.Data.BQL.BqlBool.Field<isBaseCury> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the values in the <see cref="PMTran.Amount"/> field
		/// of project transactions are displayed in the base currency.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? IsBaseCury
		{
			get;
			set;
		}
		#endregion
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMOrigDocType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Allocation, Messages.Allocation),
					Pair(Timecard, Messages.Timecard),
					Pair(Case, Messages.Case),
					Pair(ExpenseClaim, Messages.ExpenseClaim),
					Pair(EquipmentTimecard, Messages.EquipmentTimecard),
					Pair(AllocationReversal, Messages.AllocationReversal),
					Pair(Reversal, Messages.Reversal),
					Pair(ARInvoice, Messages.ARInvoice),
					Pair(CreditMemo, Messages.CreditMemo),
					Pair(DebitMemo, Messages.DebitMemo),
					Pair(APBill, Messages.APBill),
					Pair(CreditAdjustment, Messages.CreditAdjustment),
					Pair(DebitAdjustment, Messages.DebitAdjustment),
					Pair(UnbilledRemainder, Messages.UnbilledRemainder),
					Pair(UnbilledRemainderReversal, Messages.UnbilledRemainderReversal),
					Pair(ProformaBilling, Messages.ProformaBilling),
					Pair(WipReversal, Messages.WipReversal),
					Pair(ServiceOrder, Messages.ServiceOrder),
					Pair(Appointment, Messages.Appointment),
					Pair(RegularPaycheck, Messages.RegularPaycheck),
					Pair(SpecialPaycheck, Messages.SpecialPaycheck),
					Pair(AdjustmentPaycheck, Messages.AdjustmentPaycheck),
					Pair(VoidPaycheck, Messages.VoidPaycheck)
				}) {}
		}

		public class ListARAttribute : PXStringListAttribute
		{
			public ListARAttribute() : base(
				new[]
				{
					Pair(ARInvoice, Messages.ARInvoice),
					Pair(CreditMemo, Messages.CreditMemo),
					Pair(DebitMemo, Messages.DebitMemo)
				}) {}
		}

		public const string Allocation = "AL";
		public const string Timecard = "TC";
		public const string Case = "CS";
		public const string ExpenseClaim = "EC";
		public const string EquipmentTimecard = "ET";
		public const string AllocationReversal = "AR";
		public const string Reversal = "RV";
		public const string ARInvoice = "IN";
		public const string CreditMemo = "CR";
		public const string DebitMemo = "DM";
		public const string UnbilledRemainder = "UR";
		public const string UnbilledRemainderReversal = "RR";
		public const string ProformaBilling = "PB";
		public const string APBill = "BL";
		public const string CreditAdjustment = "CA";
		public const string DebitAdjustment = "DA";
		public const string WipReversal = "WR";
		public const string ServiceOrder = "AP";
		public const string Appointment = "SO";
		public const string RegularPaycheck = "PR";
		public const string SpecialPaycheck = "PS";
		public const string AdjustmentPaycheck = "PA";
		public const string VoidPaycheck = "PV";

		public class timeCard : PX.Data.BQL.BqlString.Constant<timeCard>
		{
			public timeCard() : base(Timecard) {}
		}

		public class proformaBilling : PX.Data.BQL.BqlString.Constant<proformaBilling>
		{
			public proformaBilling() : base(ProformaBilling) { }
		}

		/// <summary>
		/// Reversal
		/// </summary>
		/// <exclude />
		public class reversal : PX.Data.BQL.BqlString.Constant<reversal>
		{
			public reversal() : base(Reversal) { }
		}

		/// <summary>
		/// Allocation Reversal
		/// </summary>
		/// <exclude />
		public class allocationReversal : PX.Data.BQL.BqlString.Constant<allocationReversal>
		{
			public allocationReversal() : base(AllocationReversal) { }
		}

		public static string GetOrigDocType(string pmOrigDocType)
		{
			switch (pmOrigDocType)
			{
				case PMOrigDocType.ARInvoice:
					return ARInvoiceType.Invoice;
				case PMOrigDocType.CreditMemo:
					return ARInvoiceType.CreditMemo;
				case PMOrigDocType.DebitMemo:
					return ARInvoiceType.DebitMemo;
				default:
					return null;
			}
		}
	}

	/// <summary>
	/// Persist record in Note table with given NoteID
	/// </summary>
	/// <exclude />
	public class NotePersistAttribute : PXEventSubscriberAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber
	{
		protected Type _NoteID;

		public NotePersistAttribute(Type NoteID)
		{
			_NoteID = NoteID;
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, _NoteID.Name);
			sender.Graph.Caches[typeof(Note)].IsDirty = false;
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, _NoteID.Name);
		}
	}
}
