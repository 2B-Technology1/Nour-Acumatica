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
using PX.Objects.CS;
using PX.Objects.SO;
using System;

namespace PX.Objects.CR.Extensions.CRCreateSalesOrder
{
	[Serializable]
	[PXHidden]
	public class CreateSalesOrderFilter : IBqlTable
	{
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXUnboundDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Order Nbr.", TabOrder = 1)]
		public virtual String OrderNbr { get; set; }

		#endregion

		#region MakeQuotePrimary
		public abstract class makeQuotePrimary : PX.Data.BQL.BqlBool.Field<makeQuotePrimary> { }
		[PXBool()]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Set Quote as Primary", Visible = false)]
		public virtual bool? MakeQuotePrimary { get; set; }
		#endregion

		#region OrderType
		public abstract class orderType : BqlString.Field<orderType> { }

		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXDefault(typeof(Search2<
				SOOrderType.orderType,
			InnerJoin<SOSetup,
				On<SOOrderType.orderType, Equal<SOSetup.defaultOrderType>>>,
			Where<
				SOOrderType.active, Equal<boolTrue>>>))]
		[PXSelector(typeof(Search<SOOrderType.orderType>), DescriptionField = typeof(SOOrderType.descr))]
		[PXRestrictor(typeof(Where<SOOrderType.active, Equal<boolTrue>>), Messages.OrderTypeIsNotActive, typeof(SOOrderType.descr))]
		[PXRestrictor(typeof(
			Where<SOOrderType.behavior
				.IsIn<SOBehavior.sO, SOBehavior.iN, SOBehavior.bL, SOBehavior.qT>>),
			Messages.OrderTypeOfWrongType)]
		[PXUIField(DisplayName = "Order Type")]
		public virtual string OrderType { get; set; }
		#endregion

		#region RecalcDiscounts
		public abstract class recalcDiscount : BqlBool.Field<recalcDiscount> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Recalculate Prices and Discounts")]
		[Obsolete]
		public virtual bool? RecalcDiscounts { get; set; }
		#endregion

		#region RecalculatePrices
		public abstract class recalculatePrices : BqlBool.Field<recalculatePrices> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Recalculate Prices")]
		public virtual bool? RecalculatePrices { get; set; }
		#endregion

		#region OverrideManualPrices
		public abstract class overrideManualPrices : BqlBool.Field<overrideManualPrices> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override Manual Prices")]
		public virtual bool? OverrideManualPrices { get; set; }
		#endregion

		#region RecalculateDiscounts
		public abstract class recalculateDiscounts : BqlBool.Field<recalculateDiscounts> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Recalculate Discounts")]
		public virtual bool? RecalculateDiscounts { get; set; }
		#endregion

		#region OverrideManualDiscounts
		public abstract class overrideManualDiscounts : BqlBool.Field<overrideManualDiscounts> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override Manual Line Discounts")]
		public virtual bool? OverrideManualDiscounts { get; set; }
		#endregion

		#region OverrideManualDocGroupDiscounts
		public abstract class overrideManualDocGroupDiscounts : BqlBool.Field<overrideManualDocGroupDiscounts> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override Manual Group and Document Discounts")]
		public virtual bool? OverrideManualDocGroupDiscounts { get; set; }
		#endregion

		#region ConfirmManualAmount
		public abstract class confirmManualAmount : BqlBool.Field<confirmManualAmount> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Create a Sales Order Regardless of the Specified Manual Amount")]
		public virtual bool? ConfirmManualAmount { get; set; }
		#endregion
	}
}
