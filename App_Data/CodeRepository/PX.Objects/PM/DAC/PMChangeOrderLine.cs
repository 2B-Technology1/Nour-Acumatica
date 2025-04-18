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
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.TX;
using System;

namespace PX.Objects.PM
{
	/// <summary>A commitment line of a <see cref="PMChangeOrder">change order</see>. The records of this type are created and edited through the <strong>Commitments</strong> tab of the
	/// Change Orders (PM308000) form (which corresponds to the <see cref="ChangeOrderEntry" /> graph).</summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.ChangeOrderLine)]
	[PXPrimaryGraph(typeof(ChangeOrderEntry))]
	[Serializable]
	public class PMChangeOrderLine : PX.Data.IBqlTable, IQuantify
	{
		#region Keys
		/// <summary>
		/// Primary Key
		/// </summary>
		/// <exclude />
		public class PK : PrimaryKeyOf<PMChangeOrderLine>.By<refNbr, lineNbr>
		{
			public static PMChangeOrderLine Find(PXGraph graph, string refNbr, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, refNbr, lineNbr, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		/// <exclude />
		public static class FK
		{
			/// <summary>Chnage Order</summary>
			/// <exclude />
			public class ChangeOrder : PMProject.PK.ForeignKeyOf<PMChangeOrderLine>.By<refNbr> { }

			/// <summary>
			/// Project
			/// </summary>
			/// <exclude />
			public class Project : PMProject.PK.ForeignKeyOf<PMChangeOrderLine>.By<projectID> { }

			/// <summary>
			/// Project Task
			/// </summary>
			/// <exclude />
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PMChangeOrderLine>.By<projectID, taskID> { }

			/// <summary>
			/// Cost Code
			/// </summary>
			/// <exclude />
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PMChangeOrderLine>.By<costCodeID> { }

			/// <summary>
			/// Inventory Item
			/// </summary>
			/// <exclude />
			public class Item : IN.InventoryItem.PK.ForeignKeyOf<PMChangeOrderLine>.By<inventoryID> { }
		}

		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// The reference number of the parent <see cref="PMChangeOrder">change order</see>.
		/// </summary>
		[PXDBString(PMChangeOrder.refNbr.Length, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(PMChangeOrder.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
		[PXParent(typeof(Select<PMChangeOrder, Where<PMChangeOrder.refNbr, Equal<Current<PMChangeOrderLine.refNbr>>>>))]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;

		/// <summary>
		/// The original sequence number of the line.
		/// </summary>
		/// <remarks>The sequence of line numbers that belongs to a single document can include gaps.</remarks>
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PMChangeOrder.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;

		/// <summary>The identifier of the <see cref="PMProject">project</see> associated with the commitment.</summary>
		/// <value>
		/// Defaults to the <see cref="PMChangeOrder.ProjectID">project</see> of the parent change order.
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID" /> field.
		/// </value>
		[PXDBDefault(typeof(PMChangeOrder.projectID))]
		[PXDBInt]
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
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		/// <summary>
		/// The identifier of the <see cref="PMTask">Task</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.TaskID"/> field.
		/// </value>
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>))]
		[ProjectTask(typeof(projectID), AlwaysEnabled = true)]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<taskID>.IsRelatedTo<PMTask.taskID>>))]
		public virtual Int32? TaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

		/// <summary>
		/// The identifier of the <see cref="PMCostCode">Cost Code</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMCostCode.costCodeID"/> field.
		/// </value>
		protected Int32? _CostCodeID;
		[CostCode(typeof(accountID), typeof(taskID), GL.AccountType.Expense, ReleasedField = typeof(released))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;

		/// <summary>
		/// The identifier of the <see cref="InventoryItem">inventory item</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		[Inventory(Filterable = true)]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>))]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		/// <summary>
		/// The identifier of the <see cref="INSubItem">Subitem</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="INSubItem.SubItemID"/> field.
		/// </value>
		protected Int32? _SubItemID;
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[SubItem(typeof(inventoryID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The description of the commitment.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		/// <summary>
		/// The identifier of the <see cref="Vendor"/> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CR.BAccount.BAccountID"/> field.
		/// </value>
		protected Int32? _VendorID;
		[PXDefault]
		[POVendor]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region POOrderType
		public abstract class pOOrderType : PX.Data.BQL.BqlString.Field<pOOrderType> { }

		/// <summary>
		/// The type of the <see cref="POOrder">purchase order</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="POOrderType.RBDSListAttribute"/>.
		/// </value>
		[PXDefault(PO.POOrderType.RegularOrder)]
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "PO Type", Enabled = true)]
		[POOrderType.RPList]
		public virtual String POOrderType
		{
			get;
			set;
		}
		#endregion
		#region POOrderNbr
		public abstract class pOOrderNbr : PX.Data.BQL.BqlString.Field<pOOrderNbr> { }

		/// <summary>
		/// The reference number of the <see cref="POOrder">purchase order</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="POLine.OrderNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "PO Nbr.")]
		[PXRestrictor(typeof(Where<POOrder.hold, Equal<False>>), Messages.OrderIsOnHold)]
		[PXRestrictor(typeof(Where<POOrder.approved, Equal<True>>), Messages.OrderIsNotApproved)]
		[PXSelector(typeof(Search5<POLine.orderNbr,
			InnerJoin<POOrder, On<POLine.orderType, Equal<POOrder.orderType>, And<POLine.orderNbr, Equal<POOrder.orderNbr>>>>,
			Where<POLine.orderType, Equal<Current<pOOrderType>>,
			And<POLine.projectID, Equal<Current<PMChangeOrder.projectID>>,
			And<Where<Current<vendorID>, IsNull, Or<POLine.vendorID, Equal<Current<vendorID>>>>>>>,
			Aggregate<GroupBy<POLine.orderType, GroupBy<POLine.orderNbr, GroupBy<POLine.vendorID>>>>>),
			typeof(POLine.orderType), typeof(POLine.orderNbr), typeof(POLine.vendorID), DescriptionField = typeof(POOrder.vendorID))]
		public virtual String POOrderNbr
		{
			get;
			set;
		}
		#endregion
		#region POLineNbr
		public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }

		/// <summary>
		/// The number of the <see cref="POLine">purchase order line</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="POLine.LineNbr"/> field.
		/// </value>
		protected Int32? _POLineNbr;
		[PXDBInt]
		[PXUIField(DisplayName = "PO Line Nbr.")]
		[PXSelector(typeof(Search<POLine.lineNbr, Where<POLine.orderType, Equal<Current<pOOrderType>>,
			And<POLine.orderNbr, Equal<Current<pOOrderNbr>>,
			And<POLine.projectID, Equal<Current<PMChangeOrder.projectID>>>>>>),
			typeof(POLine.lineNbr), typeof(POLine.lineType), typeof(POLine.inventoryID), typeof(POLine.tranDesc),
			typeof(POLine.uOM), typeof(POLine.orderQty), typeof(POLine.curyExtCost))]
		public virtual Int32? POLineNbr
		{
			get
			{
				return this._POLineNbr;
			}
			set
			{
				this._POLineNbr = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;

		/// <summary>
		/// The identifier of the commitment <see cref="Currency">currency</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PX.Objects.CM.Currency.CuryID"/> field.
		/// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Current<AccessInfo.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;

		/// <summary>
		/// The unit of measure of the commitment.
		/// </summary>
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PMUnit(typeof(inventoryID))]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;

		/// <summary>
		/// The <see cref="Account">expense account</see> associated with the commitment.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(null, typeof(Search<Account.accountID, Where<Account.accountGroupID, IsNotNull>>),
			DisplayName = "Account",
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;

		/// <summary>
		/// The quantity of the commitment.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
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
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

		/// <summary>The cost of the specified unit of the commitment. The value can be manually modified.</summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Cost")]
		public virtual Decimal? UnitCost
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

		/// <summary>The amount of the commitment. The value can be manually modified.</summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		[PXFormula(typeof(Mult<qty, unitCost>))]
		public virtual Decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region AmountInProjectCury
		public abstract class amountInProjectCury : PX.Data.BQL.BqlDecimal.Field<amountInProjectCury> { }

		/// <summary>The <see cref="Amount">amount</see> of the commitment in the project currency.</summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount in Project Currency", Enabled = false)]
		[PXFormula(null, typeof(SumCalc<PMChangeOrder.commitmentTotal>))]
		public virtual Decimal? AmountInProjectCury
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the parent <see cref="PMChangeOrder">change order</see> has been released.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }

		/// <summary>
		/// The status of the commitment line of the change order.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"U"</c>: Update,
		/// <c>"L"</c>: New Document,
		/// <c>"D"</c>: New Line,
		/// <c>"R"</c>: Reopen
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[ChangeOrderLineType.List]
		[PXDefault(ChangeOrderLineType.NewDocument)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public virtual String LineType
		{
			get;
			set;
		}
		#endregion

		#region ChangeRequestRefNbr
		public abstract class changeRequestRefNbr : PX.Data.BQL.BqlString.Field<changeRequestRefNbr> { }

		/// <summary>
		/// The reference number of the corresponding change request if the commitment line has been created based on this change request.
		/// </summary>
		[PXDBString(PMChangeRequest.refNbr.Length, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Change Request Ref. Nbr.", Enabled = false, FieldClass = nameof(CS.FeaturesSet.ChangeRequest))]
		public virtual String ChangeRequestRefNbr
		{
			get;
			set;
		}
		#endregion
		#region ChangeRequestLineNbr
		public abstract class changeRequestLineNbr : PX.Data.BQL.BqlInt.Field<changeRequestLineNbr> { }

		/// <summary>
		/// The number of the corresponding change request line.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Change Request Line Nbr.", Enabled = false, FieldClass = nameof(CS.FeaturesSet.ChangeRequest))]
		public virtual Int32? ChangeRequestLineNbr
		{
			get;
			set;
		}
		#endregion

		#region PotentialRevisedQty
		public abstract class potentialRevisedQty : PX.Data.BQL.BqlDecimal.Field<potentialRevisedQty> { }

		/// <summary>The sum of the <see cref="Qty">quantity</see> and <see cref="POLinePM.OrderQty">original quantity of the purchase order line</see> associated with the commitment.</summary>
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? PotentialRevisedQty
		{
			get;
			set;
		}
		#endregion
		#region PotentialRevisedAmount
		public abstract class potentialRevisedAmount : PX.Data.BQL.BqlDecimal.Field<potentialRevisedAmount> { }

		/// <summary>The sum of the <see cref="amount" /> and the <see cref="POLinePM.CuryLineAmt">original amount of the purchase order line</see> associated with the commitment.</summary>
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? PotentialRevisedAmount
		{
			get;
			set;
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public virtual String TaxCategoryID { get; set; }
		#endregion

		#region RetainagePct
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct>
		{
			public const int Precision = 6;
		}

		/// <summary>
		/// The line retainage percentage
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(retainagePct.Precision, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Retainage Percent", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion
		#region RetainageAmt
		public abstract class retainageAmt : PX.Data.BQL.BqlDecimal.Field<retainageAmt> { }

		/// <summary>
		/// The line retainage amount in base currency
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Retainage Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageAmtInProjectCury
		public abstract class retainageAmtInProjectCury : PX.Data.BQL.BqlDecimal.Field<retainageAmtInProjectCury> { }

		/// <summary>
		/// The line retainage amount in project currency
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Retainage Amount in Project Currency", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainageAmtInProjectCury
		{
			get;
			set;
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(PMChangeOrderLine.description))]
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
		[PXDBTimestamp]
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
		[PXDBCreatedByScreenID]
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
		[PXDBLastModifiedByScreenID]
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
	public static class ChangeOrderLineType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				  new[]
				  {
					Pair(Update, Messages.ChangeOrderLine_Update),
					Pair(NewDocument, Messages.ChangeOrderLine_NewDocument),
					Pair(NewLine, Messages.ChangeOrderLine_NewLine),
					Pair(Reopen, Messages.ChangeOrderLine_Reopen),
				  })
			{ }

		}
		public const string Update = "U";
		public const string NewDocument = "L";
		public const string NewLine = "D";
		public const string Reopen = "R";
	}
}
