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
using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[Serializable]
	[PXCacheName(Messages.POAccrualStatus)]
	public class POAccrualStatus : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POAccrualStatus>.By<refNoteID, lineNbr, type>
		{
			public static POAccrualStatus Find(PXGraph graph, Guid? refNoteID, int? lineNbr, string type, PKFindOptions options = PKFindOptions.None) => FindBy(graph, refNoteID, lineNbr, type, options);

			public static POAccrualStatus FindDirty(PXGraph graph, Guid? refNoteID, int? lineNbr, string type)
			{
				return PXSelect<POAccrualStatus,
					Where<refNoteID, Equal<Required<refNoteID>>,
					And<lineNbr, Equal<Required<lineNbr>>,
					And<type, Equal<Required<type>>>>>>
					.SelectWindowed(graph, 0, 1, refNoteID, lineNbr, type);
			}
		}
		public static class FK
		{
			public class Order : POOrder.PK.ForeignKeyOf<POAccrualStatus>.By<orderType, orderNbr> { }
			public class OrderLine : POLine.PK.ForeignKeyOf<POAccrualStatus>.By<orderType, orderNbr, orderLineNbr> { }
			public class Receipt : POReceipt.PK.ForeignKeyOf<POAccrualStatus>.By<receiptType, receiptNbr> { }
			public class ReceiptLine : POReceiptLine.PK.ForeignKeyOf<POAccrualStatus>.By<receiptType, receiptNbr, lineNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<POAccrualStatus>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<POAccrualStatus>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<POAccrualStatus>.By<siteID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<POAccrualStatus>.By<vendorID> { }
			public class PayToVendor : AP.Vendor.PK.ForeignKeyOf<POAccrualStatus>.By<payToVendorID> { }
			public class Account : GL.Account.PK.ForeignKeyOf<POAccrualStatus>.By<acctID> { }
			public class Subaccount : GL.Sub.PK.ForeignKeyOf<POAccrualStatus>.By<subID> { }
			public class OriginalCurrency : CM.Currency.PK.ForeignKeyOf<POAccrualStatus>.By<origCuryID> { }
			public class BillingCurrency : CM.Currency.PK.ForeignKeyOf<POAccrualStatus>.By<billCuryID> { }
			//todo public class OriginalUnitOfMeasure : INUnit.UK.ByInventory.ForeignKeyOf<POAccrualSplit>.By<inventoryID, uOM> { }
			//todo public class ReceivedUnitOfMeasure : INUnit.UK.ByInventory.ForeignKeyOf<POAccrualSplit>.By<inventoryID, receivedUOM> { }
			//todo public class BilledUnitOfMeasure : INUnit.UK.ByInventory.ForeignKeyOf<POAccrualSplit>.By<inventoryID, billedUOM> { }
		}
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID>
		{
		}
		[PXDBGuid(IsKey = true)]
		[PXDefault]
		public virtual Guid? RefNoteID
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault]
		[POAccrualType.List]
		public virtual string Type
		{
			get;
			set;
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType>
		{
		}
		[PXDBString(2, IsFixed = true)]
		[PXDefault]
		public virtual string LineType
		{
			get;
			set;
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType>
		{
		}
		[PXDBString(2, IsFixed = true)]
		[POOrderType.List()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string OrderType
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr>
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string OrderNbr
		{
			get;
			set;
		}
		#endregion
		#region OrderLineNbr
		public abstract class orderLineNbr : PX.Data.BQL.BqlInt.Field<orderLineNbr>
		{
		}
		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? OrderLineNbr
		{
			get;
			set;
		}
		#endregion
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType>
		{
		}
		[PXDBString(2, IsFixed = true)]
		[POReceiptType.List()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string ReceiptType
		{
			get;
			set;
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr>
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ReceiptNbr
		{
			get;
			set;
		}
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID>
		{
		}
		[Vendor]
		[PXDBDefault]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region PayToVendorID
		public abstract class payToVendorID : PX.Data.BQL.BqlInt.Field<payToVendorID>
		{
		}
		[BasePayToVendor]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? PayToVendorID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		[AnyInventory]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID>
		{
		}
		[SubItem]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID>
		{
		}
		[Site]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? SiteID
		{
			get;
			set;
		}
		#endregion
		#region AcctID
		public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID>
		{
		}
		[Account]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? AcctID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID>
		{
		}
		[SubAccount]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion

		#region OrigUOM
		public abstract class origUOM : PX.Data.BQL.BqlString.Field<origUOM>
		{
		}
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String OrigUOM
		{
			get;
			set;
		}
		#endregion
		#region OrigQty
		public abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty>
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? OrigQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOrigQty
		public abstract class baseOrigQty : PX.Data.BQL.BqlDecimal.Field<baseOrigQty>
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseOrigQty
		{
			get;
			set;
		}
		#endregion
		#region OrigCuryID
		public abstract class origCuryID : PX.Data.BQL.BqlString.Field<origCuryID>
		{
		}
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string OrigCuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigAmt
		public abstract class curyOrigAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryOrigAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigAmt
		public abstract class origAmt : PX.Data.BQL.BqlDecimal.Field<origAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrigAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigCost
		public abstract class curyOrigCost : PX.Data.BQL.BqlDecimal.Field<curyOrigCost>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryOrigCost
		{
			get;
			set;
		}
		#endregion
		#region OrigCost
		public abstract class origCost : PX.Data.BQL.BqlDecimal.Field<origCost>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrigCost
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDiscAmt
		public abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryOrigDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigDiscAmt
		public abstract class origDiscAmt : PX.Data.BQL.BqlDecimal.Field<origDiscAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrigDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region ReceivedUOM
		public abstract class receivedUOM : PX.Data.BQL.BqlString.Field<receivedUOM>
		{
		}
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ReceivedUOM
		{
			get;
			set;
		}
		#endregion
		#region ReceivedQty
		public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty>
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? ReceivedQty
		{
			get;
			set;
		}
		#endregion
		#region BaseReceivedQty
		public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty>
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseReceivedQty
		{
			get;
			set;
		}
		#endregion
		#region ReceivedCost
		public abstract class receivedCost : PX.Data.BQL.BqlDecimal.Field<receivedCost>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ReceivedCost
		{
			get;
			set;
		}
		#endregion
		#region BilledUOM
		public abstract class billedUOM : PX.Data.BQL.BqlString.Field<billedUOM>
		{
		}
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String BilledUOM
		{
			get;
			set;
		}
		#endregion
		#region BilledQty
		public abstract class billedQty : PX.Data.BQL.BqlDecimal.Field<billedQty>
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? BilledQty
		{
			get;
			set;
		}
		#endregion
		#region BaseBilledQty
		public abstract class baseBilledQty : PX.Data.BQL.BqlDecimal.Field<baseBilledQty>
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseBilledQty
		{
			get;
			set;
		}
		#endregion
		#region BillCuryID
		public abstract class billCuryID : PX.Data.BQL.BqlString.Field<billCuryID>
		{
		}
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string BillCuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledAmt
		public abstract class curyBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryBilledAmt
		{
			get;
			set;
		}
		#endregion
		#region BilledAmt
		public abstract class billedAmt : PX.Data.BQL.BqlDecimal.Field<billedAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BilledAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledCost
		public abstract class curyBilledCost : PX.Data.BQL.BqlDecimal.Field<curyBilledCost>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryBilledCost
		{
			get;
			set;
		}
		#endregion
		#region BilledCost
		public abstract class billedCost : PX.Data.BQL.BqlDecimal.Field<billedCost>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BilledCost
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledDiscAmt
		public abstract class curyBilledDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledDiscAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryBilledDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region BilledDiscAmt
		public abstract class billedDiscAmt : PX.Data.BQL.BqlDecimal.Field<billedDiscAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BilledDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region PPVAmt
		public abstract class pPVAmt : PX.Data.BQL.BqlDecimal.Field<pPVAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? PPVAmt
		{
			get;
			set;
		}
		#endregion
		#region ReceivedTaxAdjCost
		public abstract class receivedTaxAdjCost : PX.Data.BQL.BqlDecimal.Field<receivedTaxAdjCost> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ReceivedTaxAdjCost
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledTaxAdjCost
		public abstract class curyBilledTaxAdjCost : PX.Data.BQL.BqlDecimal.Field<curyBilledTaxAdjCost> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryBilledTaxAdjCost
		{
			get;
			set;
		}
		#endregion
		#region BilledTaxAdjCost
		public abstract class billedTaxAdjCost : PX.Data.BQL.BqlDecimal.Field<billedTaxAdjCost> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BilledTaxAdjCost
		{
			get;
			set;
		}
		#endregion

		#region IsClosed
		[PXDBBool]
		[PXFormula(typeof(lineType.IsIn<POLineType.service, POLineType.freight>
			.Or<
				unreleasedReceiptCntr.IsEqual<int0>
				.And<unreleasedPPVAdjCntr.IsEqual<int0>>
				.And<unreleasedTaxAdjCntr.IsEqual<int0>>
				.And<receivedCost.Add<receivedTaxAdjCost>.IsEqual<billedCost.Add<pPVAmt.Add<billedTaxAdjCost>>>>
				.And<Use<
					Switch<Case<Where<receivedQty.IsNotNull
							.And<billedQty.IsNotNull>
							.And<Brackets<receivedUOM.IsNull.And<billedUOM.IsNull>>
								.Or<receivedUOM.IsEqual<billedUOM>>>>,
						True.When<receivedQty.IsEqual<billedQty>>.Else<False>>,
						True.When<baseReceivedQty.IsEqual<baseBilledQty>>.Else<False>>>
					.AsBool.IsEqual<True>>>))]
		public virtual bool? IsClosed { get; set; }
		public abstract class isClosed : Data.BQL.BqlBool.Field<isClosed> { }
		#endregion

		#region UOM
		[Obsolete]
		public abstract class uOM : Data.BQL.BqlString.Field<uOM>
		{
			public class PreventEditINUnitIfExist : PreventEditOf<INUnit.unitMultDiv, INUnit.unitRate, INUnit.fromUnit>
					.On<InventoryItemMaint>.IfExists<Select<POAccrualStatus,
						Where<inventoryID, Equal<Current<INUnit.inventoryID>>,
							And<Current<INUnit.fromUnit>, In3<origUOM, receivedUOM, billedUOM>,
							And<isClosed, Equal<False>>>>>>
			{
				public static bool IsActive() => true;

				protected override string CreateEditPreventingReason(GetEditPreventingReasonArgs arg, object firstPreventingEntity, string fieldName, string currentTableName, string foreignTableName)
				{
					var accrualstatus = (POAccrualStatus)firstPreventingEntity;
					if (accrualstatus.Type == POAccrualType.Receipt)
					{
						var receiptType = arg.Graph.Caches<POAccrualStatus>().GetStateExt<POAccrualStatus.receiptType>(accrualstatus);
						return PXMessages.LocalizeFormat(IN.Messages.ConversionCantModifyNotFullyCompletedTransactionExists, Messages.POReceipt, receiptType, accrualstatus.ReceiptNbr);
					}
					else
					{
						var orderType = arg.Graph.Caches<POAccrualStatus>().GetStateExt<POAccrualStatus.orderType>(accrualstatus);
						return PXMessages.LocalizeFormat(IN.Messages.ConversionCantModifyNotFullyCompletedTransactionExists, Messages.POOrder, orderType, accrualstatus.OrderNbr);
					}
				}

				public virtual void _(Events.RowDeleting<INUnit> e)
				{
					if (AllowEditInsertedRecords && (e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted || e.Cache.Locate(e.Row) == null)) return;

					string editPreventingReason = GetEditPreventingReason(new GetEditPreventingReasonArgs(e.Cache, typeof(INUnit.unitRate), e.Row, e.Row.UnitRate));
					if (!string.IsNullOrEmpty(editPreventingReason))
					{
						throw new PXException(editPreventingReason);
					}
				}
			}
		}

		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later [Justification: POAccrualStatus DAC is not documented yet]
		[PXString(6, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string UOM
		{
			get;
			set;
		}
		#endregion

		#region MaxFinPeriodID
		[PXDBString(FinPeriodUtils.FULL_LENGHT, IsFixed = true)]
		public virtual string MaxFinPeriodID { get; set; }
		public abstract class maxFinPeriodID : Data.BQL.BqlString.Field<maxFinPeriodID> { }
		#endregion

		#region ClosedFinPeriod
		[PXDBString(FinPeriodUtils.FULL_LENGHT, IsFixed = true)]
		[PXFormula(typeof(maxFinPeriodID.When<isClosed.IsEqual<True>>.Else<Null>))]
		public virtual string ClosedFinPeriodID { get; set; }
		public abstract class closedFinPeriodID : Data.BQL.BqlString.Field<closedFinPeriodID> { }
		#endregion

		#region UnreleasedReceiptCntr
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? UnreleasedReceiptCntr { get; set; }
		public abstract class unreleasedReceiptCntr : BqlInt.Field<unreleasedReceiptCntr> { }
		#endregion
		#region UnreleasedPPVAdjCntr
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? UnreleasedPPVAdjCntr { get; set; }
		public abstract class unreleasedPPVAdjCntr : BqlInt.Field<unreleasedPPVAdjCntr> { }
		#endregion
		#region UnreleasedTaxAdjCntr
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? UnreleasedTaxAdjCntr { get; set; }
		public abstract class unreleasedTaxAdjCntr : BqlInt.Field<unreleasedTaxAdjCntr> { }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime>
		{
		}
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID>
		{
		}
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID>
		{
		}
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
		{
		}
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
		{
		}
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
		{
		}
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}

	[Serializable]
	[PXHidden]
	[PXProjection(typeof(Select4<POAccrualStatus,
		Where<POAccrualStatus.type, Equal<POAccrualType.receipt>>,
		Aggregate<
			GroupBy<POAccrualStatus.orderType,
			GroupBy<POAccrualStatus.orderNbr,
			GroupBy<POAccrualStatus.orderLineNbr,
			Sum<POAccrualStatus.receivedQty,
			Sum<POAccrualStatus.baseReceivedQty,
			Sum<POAccrualStatus.receivedCost,
			Sum<POAccrualStatus.billedQty,
			Sum<POAccrualStatus.baseBilledQty,
			Sum<POAccrualStatus.curyBilledAmt,
			Sum<POAccrualStatus.billedAmt,
			Sum<POAccrualStatus.curyBilledCost,
			Sum<POAccrualStatus.billedCost,
			Sum<POAccrualStatus.billedDiscAmt,
			Sum<POAccrualStatus.curyBilledDiscAmt,
			Sum<POAccrualStatus.pPVAmt>>>>>>>>>>>>>>>>>), Persistent = false)]
	public class POAccrualStatusSummary : IBqlTable
	{
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType>
		{
		}
		[PXDBString(2, IsFixed = true, BqlField = typeof(POAccrualStatus.lineType))]
		public virtual string LineType
		{
			get;
			set;
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType>
		{
		}
		[PXDBString(2, IsFixed = true, BqlField = typeof(POAccrualStatus.orderType))]
		public virtual string OrderType
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr>
		{
		}
		[PXDBString(15, IsUnicode = true, BqlField = typeof(POAccrualStatus.orderNbr))]
		public virtual string OrderNbr
		{
			get;
			set;
		}
		#endregion
		#region OrderLineNbr
		public abstract class orderLineNbr : PX.Data.BQL.BqlInt.Field<orderLineNbr>
		{
		}
		[PXDBInt(BqlField = typeof(POAccrualStatus.orderLineNbr))]
		public virtual int? OrderLineNbr
		{
			get;
			set;
		}
		#endregion
		#region OrigUOM
		public abstract class origUOM : PX.Data.BQL.BqlString.Field<origUOM>
		{
		}
		[PXDBString(6, IsUnicode = true, BqlField = typeof(POAccrualStatus.origUOM))]
		public virtual String OrigUOM
		{
			get;
			set;
		}
		#endregion
		#region OrigQty
		public abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POAccrualStatus.origQty))]
		public virtual decimal? OrigQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOrigQty
		public abstract class baseOrigQty : PX.Data.BQL.BqlDecimal.Field<baseOrigQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POAccrualStatus.baseOrigQty))]
		public virtual decimal? BaseOrigQty
		{
			get;
			set;
		}
		#endregion
		#region OrigCuryID
		public abstract class origCuryID : PX.Data.BQL.BqlString.Field<origCuryID>
		{
		}
		[PXDBString(5, IsUnicode = true, BqlField = typeof(POAccrualStatus.origCuryID))]
		public virtual string OrigCuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigAmt
		public abstract class curyOrigAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.curyOrigAmt))]
		public virtual decimal? CuryOrigAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigAmt
		public abstract class origAmt : PX.Data.BQL.BqlDecimal.Field<origAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.origAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrigAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigCost
		public abstract class curyOrigCost : PX.Data.BQL.BqlDecimal.Field<curyOrigCost>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.curyOrigCost))]
		public virtual decimal? CuryOrigCost
		{
			get;
			set;
		}
		#endregion
		#region OrigCost
		public abstract class origCost : PX.Data.BQL.BqlDecimal.Field<origCost>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.origCost))]
		public virtual decimal? OrigCost
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDiscAmt
		public abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.curyOrigDiscAmt))]
		public virtual decimal? CuryOrigDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigDiscAmt
		public abstract class origDiscAmt : PX.Data.BQL.BqlDecimal.Field<origDiscAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.origDiscAmt))]
		public virtual decimal? OrigDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region ReceivedUOM
		public abstract class receivedUOM : PX.Data.BQL.BqlString.Field<receivedUOM>
		{
		}
		[PXDBString(6, IsUnicode = true, BqlField = typeof(POAccrualStatus.receivedUOM))]
		public virtual String ReceivedUOM
		{
			get;
			set;
		}
		#endregion
		#region ReceivedQty
		public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POAccrualStatus.receivedQty))]
		public virtual decimal? ReceivedQty
		{
			get;
			set;
		}
		#endregion
		#region BaseReceivedQty
		public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POAccrualStatus.baseReceivedQty))]
		public virtual decimal? BaseReceivedQty
		{
			get;
			set;
		}
		#endregion
		#region ReceivedCost
		public abstract class receivedCost : PX.Data.BQL.BqlDecimal.Field<receivedCost>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.receivedCost))]
		public virtual decimal? ReceivedCost
		{
			get;
			set;
		}
		#endregion
		#region BilledUOM
		public abstract class billedUOM : PX.Data.BQL.BqlString.Field<billedUOM>
		{
		}
		[PXDBString(6, IsUnicode = true, BqlField = typeof(POAccrualStatus.billedUOM))]
		public virtual String BilledUOM
		{
			get;
			set;
		}
		#endregion
		#region BilledQty
		public abstract class billedQty : PX.Data.BQL.BqlDecimal.Field<billedQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POAccrualStatus.billedQty))]
		public virtual decimal? BilledQty
		{
			get;
			set;
		}
		#endregion
		#region BaseBilledQty
		public abstract class baseBilledQty : PX.Data.BQL.BqlDecimal.Field<baseBilledQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POAccrualStatus.baseBilledQty))]
		public virtual decimal? BaseBilledQty
		{
			get;
			set;
		}
		#endregion
		#region BillCuryID
		public abstract class billCuryID : PX.Data.BQL.BqlString.Field<billCuryID>
		{
		}
		[PXDBString(5, IsUnicode = true, BqlField = typeof(POAccrualStatus.billCuryID))]
		public virtual string BillCuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledAmt
		public abstract class curyBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.curyBilledAmt))]
		public virtual decimal? CuryBilledAmt
		{
			get;
			set;
		}
		#endregion
		#region BilledAmt
		public abstract class billedAmt : PX.Data.BQL.BqlDecimal.Field<billedAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.billedAmt))]
		public virtual decimal? BilledAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledCost
		public abstract class curyBilledCost : PX.Data.BQL.BqlDecimal.Field<curyBilledCost>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.curyBilledCost))]
		public virtual decimal? CuryBilledCost
		{
			get;
			set;
		}
		#endregion
		#region BilledCost
		public abstract class billedCost : PX.Data.BQL.BqlDecimal.Field<billedCost>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.billedCost))]
		public virtual decimal? BilledCost
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledDiscAmt
		public abstract class curyBilledDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledDiscAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.curyBilledDiscAmt))]
		public virtual decimal? CuryBilledDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region BilledDiscAmt
		public abstract class billedDiscAmt : PX.Data.BQL.BqlDecimal.Field<billedDiscAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.billedDiscAmt))]
		public virtual decimal? BilledDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region PPVAmt
		public abstract class pPVAmt : PX.Data.BQL.BqlDecimal.Field<pPVAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POAccrualStatus.pPVAmt))]
		public virtual decimal? PPVAmt
		{
			get;
			set;
		}
		#endregion
	}
}
