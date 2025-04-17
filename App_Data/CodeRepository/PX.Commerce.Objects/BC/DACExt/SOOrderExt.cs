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
using PX.Objects.SO;
using System.Collections.Generic;
using PX.Commerce.Core;
using PX.Data.WorkflowAPI;
using PX.Objects.CS;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// DAC extension of SOOrder to add additional attributes and properties.
	/// </summary>
	[Serializable]
	public sealed class BCSOOrderExt : PXCacheExtension<SOOrder>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		/// <summary>
		/// <inheritdoc cref="SOOrder.OrderNbr"/>
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXAppendSelectorColumns(typeof(SOOrder.customerRefNbr))]
		public String OrderNbr { get; set; }

		#region ExternalOrderOriginal
		/// <summary>
		/// Indicates whether original order is external.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "External Order Original")]
		public Boolean? ExternalOrderOriginal { get; set; }
		/// <inheritdoc cref="ExternalOrderOriginal"/>
		public abstract class externalOrderOriginal : PX.Data.BQL.BqlBool.Field<externalOrderOriginal> { }
		#endregion

		#region ExternalRefundRef
		/// <summary>
		/// The external ID of any refund for this order.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		public string ExternalRefundRef { get; set; }
		/// <inheritdoc cref="ExternalRefundRef"/>
		public abstract class externalRefundRef : PX.Data.BQL.BqlString.Field<externalRefundRef> { }
		#endregion

		#region ExternalOrderOrigin
		/// <summary>
		/// The external ID of the original order.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "External Order Origin")]
		[PXSelector(
			typeof(Search<BCBinding.bindingID>),
			new Type[] {
				typeof(BCBinding.bindingID),
				typeof(BCBinding.connectorType),
				typeof(BCBinding.bindingName),
				typeof(BCBinding.isActive),
				typeof(BCBinding.isDefault) },
			SubstituteKey = typeof(BCBinding.bindingName))]
		public int? ExternalOrderOrigin { get; set; }
		/// <inheritdoc cref="ExternalOrderOrigin"/>
		public abstract class externalOrderOrigin : PX.Data.BQL.BqlInt.Field<externalOrderOrigin> { }
		#endregion

		#region ExternalOrderSource
		/// <summary>
		/// The name of the external source for this order.
		/// </summary>
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "External Order Source")]
		public string ExternalOrderSource { get; set; }
		/// <inheritdoc cref="ExternalOrderSource"/>
		public abstract class externalOrderSource : PX.Data.BQL.BqlString.Field<externalOrderSource> { }
		#endregion

		#region RiskHold
		/// <summary>
		/// Indicates whether to put this order on hold due to potential risk of fraud or other issues.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public Boolean? RiskHold { get; set; }
		/// <inheritdoc cref="RiskHold"/>
		public abstract class riskHold : PX.Data.BQL.BqlBool.Field<riskHold> { }
		#endregion

		#region MaxRiskScore
		/// <summary>
		/// The max risk score of this order.
		/// </summary>
		[PXDBDecimal()]
		[PXUIField(DisplayName = "Max Risk Score")]
		public decimal? MaxRiskScore { get; set; }
		/// <inheritdoc cref="MaxRiskScore"/>
		public abstract class maxRiskScore : PX.Data.BQL.BqlDecimal.Field<maxRiskScore> { }
		#endregion

		#region RiskStatus
		/// <summary>
		/// The current risk status of this order. Either low, medium, or high.
		/// </summary>
		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Risk Status", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<maxRiskScore, Greater<decimal60>>, BCRiskStatusAttribute.high,
			Case<Where<maxRiskScore, Greater<decimal20>, And<maxRiskScore, LessEqual<decimal60>>>, BCRiskStatusAttribute.medium,
			Case<Where<maxRiskScore, GreaterEqual<decimal0>, And<maxRiskScore, LessEqual<decimal20>>>, BCRiskStatusAttribute.low>>>,
			BCRiskStatusAttribute.none>
			))]
		public string RiskStatus { get; set; }
		/// <inheritdoc cref="RiskStatus"/>
		public abstract class riskStatus : PX.Data.BQL.BqlString.Field<riskStatus> { }
		#endregion

		#region ExternalOrderExported
		/// <summary>
		/// Indicates if this order has been exported.
		/// </summary>
		[PXBool()]
		public bool? ExternalOrderExported { get; set; }
		/// <inheritdoc cref="ExternalOrderExported"/>
		public abstract class externalOrderExported : PX.Data.BQL.BqlBool.Field<externalOrderExported> { }
		#endregion

		#region AllowModifyingItems
		/// <summary>
		/// Indicates if this item is allowed to be modified.
		/// </summary>
		[PXBool()]
		public bool? AllowModifyingItems { get; set; }
		/// <inheritdoc cref="AllowModifyingItems"/>
		public abstract class allowModifyingItems : PX.Data.BQL.BqlBool.Field<allowModifyingItems> { }
		#endregion

		#region Events
		public class Events : PXEntityEvent<SOOrder>.Container<Events>
		{
			public PXEntityEvent<SOOrder> RiskHoldConditionStatisfied;
		}
		#endregion

		#region Constants
		public class decimal60 : PX.Data.BQL.BqlDecimal.Constant<decimal60>
		{
			public decimal60()
				: base(60m)
			{
			}
		}

		public class decimal20 : PX.Data.BQL.BqlDecimal.Constant<decimal20>
		{
			public decimal20()
				: base(20m)
			{
			}
		}

		public class decimal0 : PX.Data.BQL.BqlDecimal.Constant<decimal0>
		{
			public decimal0()
				: base(0m)
			{
			}
		}

		public class decimal100 : PX.Data.BQL.BqlDecimal.Constant<decimal100>
		{
			public decimal100()
				: base(100m)
			{
			}
		}

		#endregion
	}
}
