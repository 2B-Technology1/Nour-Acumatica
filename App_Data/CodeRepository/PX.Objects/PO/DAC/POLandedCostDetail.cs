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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO.LandedCosts.Attributes;
using PX.Objects.TX;

namespace PX.Objects.PO
{
	[Serializable]
	[PXCacheName(Messages.POLandedCostDetail)]
	public class POLandedCostDetail : PX.Data.IBqlTable, ISortOrder
	{
		#region Keys
		public class PK : PrimaryKeyOf<POLandedCostDetail>.By<docType, refNbr, lineNbr>
		{
			public static POLandedCostDetail Find(PXGraph graph, string docType, string refNbr, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, docType, refNbr, lineNbr, options);
		}
		public static class FK
		{
			public class LandedCostDocument : POLandedCostDoc.PK.ForeignKeyOf<POLandedCostDetail>.By<docType, refNbr> { }
			public class LandedCostCode : Objects.PO.LandedCostCode.PK.ForeignKeyOf<POLandedCostDetail>.By<landedCostCodeID> { }
			public class TaxCategory : TX.TaxCategory.PK.ForeignKeyOf<POLandedCostDetail>.By<taxCategoryID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<POLandedCostDetail>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<POLandedCostDetail>.By<subItemID> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<POLandedCostDetail>.By<branchID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<POLandedCostDetail>.By<curyInfoID> { }
			public class LandedCostAccrualAccount : Account.PK.ForeignKeyOf<POLandedCostDetail>.By<lCAccrualAcct> { }
			public class LandedCostAccrualSubaccount : Sub.PK.ForeignKeyOf<POLandedCostDetail>.By<lCAccrualSub> { }
			public class APInvoice : AP.APInvoice.PK.ForeignKeyOf<POLandedCostDetail>.By<aPDocType, aPRefNbr> { }
			public class INRegister : IN.INRegister.PK.ForeignKeyOf<POLandedCostDetail>.By<iNDocType, iNRefNbr> { }
		}
		#endregion
		
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Branch">Branch</see>, to which the transaction belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.GL.Branch.BranchID">Branch.BranchID</see> field.
		/// </value>
		[Branch(typeof(POLandedCostDoc.branchID))]
		public virtual Int32? BranchID
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		/// <summary>
		/// The type of the landed cost receipt line.
		/// </summary>
		/// <value>
		/// The field is determined by the type of the parent <see cref="POLandedCostDoc">document</see>.
		/// For the list of possible values see <see cref="POLandedCostDoc.DocType"/>.
		/// </value>
		[POLandedCostDocType.List()]
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(POLandedCostDoc.docType))]
		[PXUIField(DisplayName = "Doc. Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// Reference number of the parent <see cref="POLandedCostDoc">document</see>.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(POLandedCostDoc.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.LandedCostDocument))]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <summary>
		/// The number of the transaction line in the document.
		/// </summary>
		/// <value>
		/// Note that the sequence of line numbers of the transactions belonging to a single document may include gaps.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(POLandedCostDoc.lineCntr))]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder>
		{
			public const string DispalyName = "Line Order";
		}

		[PXDBInt]
		[PXUIField(DisplayName = sortOrder.DispalyName, Visible = false, Enabled = false)]
		public virtual Int32? SortOrder
		{
			get;
			set;
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(POLandedCostDoc.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region LandedCostCodeID
		public abstract class landedCostCodeID : PX.Data.BQL.BqlString.Field<landedCostCodeID> { }

		/// <summary>
		/// The <see cref="PX.Objects.PO.LandedCostCode">landed cost code</see> used to describe the specific landed costs incurred for the line.
		/// This code is one of the codes associated with the vendor.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.PO.LandedCostCode.LandedCostCodeID">LandedCostCode.LandedCostCodeID</see> field.
		/// </value>
		[PXDBString(15, IsUnicode = true, IsFixed = false)]
		[PXUIField(DisplayName = "Landed Cost Code")]
		[PXSelector(typeof(Search<Objects.PO.LandedCostCode.landedCostCodeID>))]
		public virtual String LandedCostCodeID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(150, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXLocalizableDefault(typeof(Search<LandedCostCode.descr, Where<LandedCostCode.landedCostCodeID, Equal<Current<landedCostCodeID>>>>),
			typeof(Customer.localeName), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String Descr
		{
			get;
			set;
		}
		#endregion
		#region AllocationMethod
		public abstract class allocationMethod : PX.Data.BQL.BqlString.Field<allocationMethod> { }
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Selector<landedCostCodeID, LandedCostCode.allocationMethod>))]
		[LandedCostAllocationMethod.List()]
		[PXUIField(DisplayName = "Allocation Method", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String AllocationMethod
		{
			get;
			set;
		}
		#endregion
		#region CuryLineAmt
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(lineAmt))]
		[PXUIField(DisplayName = "Amount")]
		//[PXFormula(typeof(curyLineAmt), typeof(SumCalc<POLandedCostDoc.curyOrigDocAmt>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineAmt
		{
			get;
			set;
		}
		#endregion
		#region LineAmt
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

		/// <summary>
		/// Identifier of the <see cref="TaxCategory">tax category</see> associated with the line.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="TaxCategory.TaxCategoryID"/> field.
		/// Defaults to the <see cref="InventoryItem.TaxCategoryID">tax category associated with the line item</see>.
		/// </value>
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[POLandedCostTax(typeof(POLandedCostDoc), typeof(POLandedCostTax), typeof(POLandedCostTaxTran))]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(Search<LandedCostCode.taxCategoryID, Where<LandedCostCode.landedCostCodeID, Equal<Current<landedCostCodeID>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TaxCategoryID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.IN.InventoryItem">inventory item</see> associated with the transaction.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.IN.InventoryItem.InventoryID">InventoryItem.InventoryID</see> field.
		/// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
		[LandedCostDetailInventory]
		[PXForeignReference(typeof(FK.InventoryItem))]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
				Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>,
					And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[SubItem(typeof(inventoryID))]
		public virtual Int32? SubItemID
		{
			get;
			set;
		}
		#endregion

		#region LCAccrualAcct
		public abstract class lCAccrualAcct : PX.Data.BQL.BqlInt.Field<lCAccrualAcct> { }
		protected Int32? _LCAccrualAcct;

		[PXDBInt]
		[PXDefault(typeof(Search<LandedCostCode.lCAccrualAcct, Where<LandedCostCode.landedCostCodeID, Equal<Current<landedCostCodeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LCAccrualAcct
		{
			get
			{
				return this._LCAccrualAcct;
			}
			set
			{
				this._LCAccrualAcct = value;
			}
		}
		#endregion
		#region LCAccrualSub
		public abstract class lCAccrualSub : PX.Data.BQL.BqlInt.Field<lCAccrualSub> { }
		protected Int32? _LCAccrualSub;

		[PXDBInt()]
		[PXDefault(typeof(Search<LandedCostCode.lCAccrualSub, Where<LandedCostCode.landedCostCodeID, Equal<Current<landedCostCodeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LCAccrualSub
		{
			get
			{
				return this._LCAccrualSub;
			}
			set
			{
				this._LCAccrualSub = value;
			}
		}
		#endregion

		#region APDocType
		public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }

		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "AP Doc. Type", Enabled = false)]
		[APDocType.List()]
		public virtual String APDocType
		{
			get;
			set;
		}
		#endregion
		#region APRefNbr
		public abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "AP Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<Current<aPDocType>>>>))]
		public virtual String APRefNbr
		{
			get;
			set;
		}
		#endregion
		#region INDocType
		public abstract class iNDocType : PX.Data.BQL.BqlString.Field<iNDocType> { }

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "IN Doc. Type", Enabled = false)]
		[INDocType.List()]
		public virtual String INDocType
		{
			get;
			set;
		}
		#endregion
		#region INRefNbr
		public abstract class iNRefNbr : PX.Data.BQL.BqlString.Field<iNRefNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "IN Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<iNDocType>>>>))]
		public virtual String INRefNbr
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote()]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
