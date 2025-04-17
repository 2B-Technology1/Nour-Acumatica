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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM.Extensions;
using PX.Objects.GL;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// A commitment.
	/// Commitments are created and updated during processing of <see cref="PO.POOrder">purchase orders</see>
	/// or subcontracts with detail lines related to a project. Also the records of this type are created through the
	/// External Commitments (PM209000) form (which corresponds to the <see cref="ExternalCommitmentEntry"/> graph).
	/// The records of this type are displayed on the Commitments (PM306000) form (which corresponds to the
	/// <see cref="CommitmentInquiry"/> graph).
	/// </summary>
	[PXCacheName(Messages.Commitment)]
	[PXPrimaryGraph(typeof(CommitmentInquiry))]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMCommitment : PX.Data.IBqlTable, IProjectFilter, IQuantify
	{
		#region CommitmentID
		/// <inheritdoc cref="CommitmentID"/>
		public abstract class commitmentID : PX.Data.BQL.BqlGuid.Field<commitmentID> { }
		protected Guid? _CommitmentID;
		/// <summary>
		/// The identifier of the commitment.
		/// </summary>
		[PXDefault]
		[PXDBGuid(IsKey = true)]
		public virtual Guid? CommitmentID
		{
			get
			{
				return _CommitmentID;
			}
			set
			{
				_CommitmentID = value;
			}
		}
		#endregion

		#region BranchID
		/// <inheritdoc cref="BranchID"/>
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// The <see cref="Branch">branch</see> of the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Branch.branchID"/> field.
		/// </value>
		[Branch(IsDetail = true, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region Type
		/// <inheritdoc cref="Type"/>
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected string _Type;
		/// <summary>
		/// The type of the commitment.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMCommitmentType.ListAttribute"/>.
		/// </value>
		[PXDBString(1)]
		[PXDefault(PMCommitmentType.Internal)]
		[PMCommitmentType.List()]
		[PXUIField(DisplayName = "Type")]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region Status
		/// <inheritdoc cref="Status"/>
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		/// <summary>
		/// The status of the commitment.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMCommitmentStatus.ListAttribute"/>.
		/// </value>
		[PXDBString(1)]
		[PXDefault(PMCommitmentStatus.Open)]
		[PMCommitmentStatus.List()]
		[PXUIField(DisplayName = "Status")]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		/// <inheritdoc cref="AccountGroupID"/>
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		/// <summary>
		/// The <see cref="PMAccountGroup">account group</see> of the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.accountID"/> field.
		/// </value>
		[PXDefault]
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		[AccountGroup()]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region ProjectID
		/// <inheritdoc cref="ProjectID"/>
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		/// <summary>
		/// The <see cref="PMProject">project</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.contractID"/> field.
		/// </value>
		[PXDefault]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[PXRestrictor(typeof(Where<PMProject.nonProject, Equal<False>>), PM.Messages.NonProjectCodeIsInvalid)]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
		[ProjectBase]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region ProjectTaskID
		/// <inheritdoc cref="ProjectTaskID"/>
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		protected Int32? _ProjectTaskID;
		/// <summary>
		/// The <see cref="PMTask">project task</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID"/> field.
		/// </value>
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>))]
		[ActiveOrInPlanningProjectTask(typeof(PMCommitment.projectID))]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<projectTaskID>.IsRelatedTo<PMTask.taskID>>))]
		public virtual Int32? ProjectTaskID
		{
			get
			{
				return this._ProjectTaskID;
			}
			set
			{
				this._ProjectTaskID = value;
			}
		}

		/// <summary>
		/// The <see cref="PMTask">project task</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="projectTaskID"/> field.
		/// </value>
		public int? TaskID
		{
			get { return ProjectTaskID; }
		}
		#endregion
		#region InventoryID
		/// <inheritdoc cref="InventoryID"/>
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		/// <summary>
		/// The <see cref="InventoryItem">inventory item</see> of the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.inventoryID"/> field.
		/// </value>
		[PXUIField(DisplayName = "Inventory ID")]
		[PXDBInt()]
		[PMInventorySelector]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region CostCodeID
		/// <inheritdoc cref="CostCodeID"/>
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		/// <summary>
		/// The <see cref="PMCostCode">cost code</see> of the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostCode.costCodeID"/> field.
		/// </value>
		[CostCode(SkipVerification = true)]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region ExtRefNbr
		/// <inheritdoc cref="ExtRefNbr"/>
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		/// <summary>
		/// The reference number of the commitment of the external type.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref. Nbr")]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region UOM
		/// <inheritdoc cref="UOM"/>
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		/// <summary>
		/// The unit of measure of the commitment.
		/// </summary>
		[PMUnit(typeof(PMCommitment.inventoryID))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region OrigQty
		/// <inheritdoc cref="OrigQty"/>
		public abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
		/// <summary>
		/// The original quantity of the commitment.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBQuantity]
		[PXUIField(DisplayName = "Original Committed Quantity", FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? OrigQty
		{
			get;
			set;
		}
		#endregion
		#region OrigAmount
		/// <inheritdoc cref="OrigAmount"/>
		public abstract class origAmount : PX.Data.BQL.BqlDecimal.Field<origAmount> { }
		/// <summary>
		/// The original amount of the commitment.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Original Committed Amount", FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? OrigAmount
		{
			get;
			set;
		}
		#endregion
		#region Qty
		/// <inheritdoc cref="Qty"/>
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		/// <summary>
		/// The revised quantity of the commitment.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Quantity")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region Amount
		/// <inheritdoc cref="Amount"/>
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		protected Decimal? _Amount;
		/// <summary>
		/// The revised amount of the commitment.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Committed Amount")]
		public virtual Decimal? Amount
		{
			get
			{
				return this._Amount;
			}
			set
			{
				this._Amount = value;
			}
		}
		#endregion
		#region ReceivedQty
		/// <inheritdoc cref="ReceivedQty"/>
		public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
		protected Decimal? _ReceivedQty;
		/// <summary>
		/// The received quantity of the commitment.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Received Quantity")]
		public virtual Decimal? ReceivedQty
		{
			get
			{
				return this._ReceivedQty;
			}
			set
			{
				this._ReceivedQty = value;
			}
		}
		#endregion
		#region InvoicedQty
		/// <inheritdoc cref="InvoicedQty"/>
		public abstract class invoicedQty : PX.Data.BQL.BqlDecimal.Field<invoicedQty> { }
		protected Decimal? _InvoicedQty;
		/// <summary>
		/// The invoiced quantity of the commitment.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Quantity")]
		public virtual Decimal? InvoicedQty
		{
			get
			{
				return this._InvoicedQty;
			}
			set
			{
				this._InvoicedQty = value;
			}
		}
		#endregion
		#region InvoicedAmount
		/// <inheritdoc cref="InvoicedAmount"/>	
		public abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount> { }
		protected Decimal? _InvoicedAmount;
		/// <summary>
		/// The invoiced amount of the commitment.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Invoiced Amount")]
		public virtual Decimal? InvoicedAmount
		{
			get
			{
				return this._InvoicedAmount;
			}
			set
			{
				this._InvoicedAmount = value;
			}
		}
		#endregion
		#region CommittedCOQty
		/// <inheritdoc cref="CommittedCOQty"/>
		public abstract class committedCOQty : PX.Data.BQL.BqlDecimal.Field<committedCOQty> { }
		/// <summary>
		/// The total quantity of the cost commitment lines of released change orders that are associated with the commitment.
		/// </summary>
		[PXQuantity]
		[PXUIField(DisplayName = "Committed CO Quantity", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedCOQty
		{
			[PXDependsOnFields(typeof(qty), typeof(origQty))]
			get
			{
				return this.Qty.GetValueOrDefault() - this.OrigQty.GetValueOrDefault();
			}
		}
		#endregion
		#region CommittedCOAmount
		/// <inheritdoc cref="CommittedCOAmount"/>
		public abstract class committedCOAmount : PX.Data.BQL.BqlDecimal.Field<committedCOAmount> { }
		/// <summary>
		/// The total amount of the cost commitment lines of released change orders that are associated with the commitment.
		/// </summary>
		[PXBaseCury]
		[PXUIField(DisplayName = "Committed CO Amount", Enabled = false, FieldClass = PMChangeOrder.FieldClass)]
		public virtual Decimal? CommittedCOAmount
		{
			[PXDependsOnFields(typeof(amount), typeof(origAmount))]
			get
			{
				return this.Amount.GetValueOrDefault() - this.OrigAmount.GetValueOrDefault();
			}
		}
		#endregion
		#region CommittedVarianceQty
		/// <inheritdoc cref="CommittedVarianceQty"/>
		public abstract class committedVarianceQty : PX.Data.BQL.BqlDecimal.Field<committedVarianceQty> { }
		/// <summary>
		/// The difference between the values of <see cref="Qty"/> and <see cref="InvoicedQty"/>.
		/// </summary>
		[PXQuantity]
		[PXUIField(DisplayName = "Committed Variance Quantity", Enabled = false)]
		public virtual Decimal? CommittedVarianceQty
		{
			[PXDependsOnFields(typeof(qty), typeof(invoicedQty))]
			get
			{
				return this.Qty.GetValueOrDefault() - this.InvoicedQty.GetValueOrDefault();
			}
		}
		#endregion
		#region CommittedVarianceAmount
		/// <inheritdoc cref="CommittedVarianceAmount"/>
		public abstract class committedVarianceAmount : PX.Data.BQL.BqlDecimal.Field<committedVarianceAmount> { }
		/// <summary>
		/// The difference between the values of <see cref="Amount"/> and <see cref="InvoicedAmount"/>.
		/// </summary>
		[PXBaseCury]
		[PXUIField(DisplayName = "Committed Variance Amount", Enabled = false)]
		public virtual Decimal? CommittedVarianceAmount
		{
			[PXDependsOnFields(typeof(amount), typeof(invoicedAmount))]
			get
			{
				return this.Amount.GetValueOrDefault() - this.InvoicedAmount.GetValueOrDefault();
			}
		}
		#endregion
		#region ProjectCuryID
		/// <inheritdoc cref="ProjectCuryID"/>
		public abstract class projectCuryID : PX.Data.BQL.BqlString.Field<projectCuryID> { }
		/// <summary>
		/// The project currency.
		/// </summary>
		[PXString(5, IsUnicode = true)]
		[PXDBScalar(typeof(Search<PMProject.curyID, Where<PMProject.contractID, Equal<PMCommitment.projectID>>>))]
		[PXUIField(DisplayName = "Project Currency", Enabled = false, FieldClass = nameof(CS.FeaturesSet.ProjectMultiCurrency))]
		public virtual string ProjectCuryID
		{
			get;
			set;
		}
		#endregion

		#region OpenQty
		/// <inheritdoc cref="OpenQty"/>
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		/// <summary>
		/// The open quantity of the commitment that has not been received yet.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Quantity")]
		public virtual Decimal? OpenQty
		{
			get
			{
				return this._OpenQty;
			}
			set
			{
				this._OpenQty = value;
			}
		}
		#endregion
		#region OpenAmount
		/// <inheritdoc cref="OpenAmount"/>
		public abstract class openAmount : PX.Data.BQL.BqlDecimal.Field<openAmount> { }
		protected Decimal? _OpenAmount;
		/// <summary>
		/// The open amount of the commitment that has not been received yet.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Committed Open Amount")]
		public virtual Decimal? OpenAmount
		{
			get
			{
				return this._OpenAmount;
			}
			set
			{
				this._OpenAmount = value;
			}
		}
		#endregion
		#region RefNoteID
		/// <inheritdoc cref="RefNoteID"/>
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;
		/// <summary>
		/// The document from which the commitment originates.
		/// </summary>
		[PXUIField(DisplayName = "Related Document")]
		[PXRefNote()]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		public class PXRefNoteAttribute : Common.PXRefNoteBaseAttribute
		{
			public PXRefNoteAttribute()
				: base()
			{
			}

			public override void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
			{
				using (new PXReadBranchRestrictedScope())
				{
					var noteId = (Guid?)cache.GetValue(args.Row, _FieldOrdinal);
					args.ReturnValue = noteId != null ? helper.GetEntityRowID(noteId, null) : string.Empty;
				}
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				PXButtonDelegate del = delegate (PXAdapter adapter)
				{
					PXCache cache = adapter.View.Graph.Caches[typeof(PMCommitment)];
					if (cache.Current != null)
					{
						object val = cache.GetValueExt(cache.Current, _FieldName);

						PXLinkState state = val as PXLinkState;
						if (state != null)
						{
							helper.NavigateToRow(state.target.FullName, state.keys, PXRedirectHelper.WindowMode.NewWindow);
						}
						else
						{
							helper.NavigateToRow((Guid?)cache.GetValue(cache.Current, _FieldName), PXRedirectHelper.WindowMode.NewWindow);
						}
					}

					return adapter.Get();
				};

				string ActionName = sender.GetItemType().Name + "$" + _FieldName + "$Link";
				sender.Graph.Actions[ActionName] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(typeof(CommitmentInquiry.ProjectBalanceFilter)), new object[] { sender.Graph, ActionName, del, new PXEventSubscriberAttribute[] { new PXUIFieldAttribute { MapEnableRights = PXCacheRights.Select } } });
			}
		}

		#endregion

		#region System Columns
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
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMCommitmentType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Internal, External },
				new string[] { Messages.CommitmentType_Internal, Messages.CommitmentType_External })
			{ }


		}

		public const string Internal = "I";
		public const string External = "E";
		public class internalType : PX.Data.BQL.BqlString.Constant<internalType>
		{
			public internalType() : base(Internal) {; }
		}
		public class externalType : PX.Data.BQL.BqlString.Constant<externalType>
		{
			public externalType() : base(External) {; }
		}

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMCommitmentStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Open, Closed, Canceled },
				new string[] { Messages.CommitmentStatus_Open, Messages.CommitmentStatus_Closed, Messages.CommitmentStatus_Canceled })
			{ }


		}

		public const string Open = "O";
		public const string Closed = "C";
		public const string Canceled = "X";
		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) {; }
		}
		public class closed : PX.Data.BQL.BqlString.Constant<closed>
		{
			public closed() : base(Closed) {; }
		}
		public class canceled : PX.Data.BQL.BqlString.Constant<canceled>
		{
			public canceled() : base(Canceled) {; }
		}

	}
}
