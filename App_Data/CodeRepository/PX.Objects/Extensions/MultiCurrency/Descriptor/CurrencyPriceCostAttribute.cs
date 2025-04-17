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

using CommonServiceLocator;
using PX.Data;
using System;

namespace PX.Objects.CM.Extensions
{
	/// <summary>
	/// Marks the field for processing by MultiCurrencyGraph. When attached to a Field that stores Amount in pair with BaseAmount Field
	/// MultiCurrencyGraph handles conversion and rounding when this field is updated. 
	/// This Attribute forces the system to use precision specified for Price/Cost instead one comming from Currency
	/// Use this Attribute for Non DB fields. See <see cref="PXDBCurrencyAttribute"/> for DB version.
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXCurrencyPriceCostAttribute : PXDecimalAttribute, ICurrencyAttribute
	{
		#region State
		internal protected Type ResultField;
		internal protected Type KeyField;
		public virtual bool BaseCalc { get; set; } = true;

		int? ICurrencyAttribute.CustomPrecision => _Precision;

		Type ICurrencyAttribute.ResultField => ResultField;

		Type ICurrencyAttribute.KeyField => KeyField;
		#endregion

		#region Ctor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="keyField">Field in this table used as a key for CurrencyInfo table.</param>
		/// <param name="resultField">Field in this table to store the result of currency conversion.</param>
		public PXCurrencyPriceCostAttribute(Type keyField, Type resultField)
			: base(typeof(Search<CS.CommonSetup.decPlPrcCst>))
		{
			ResultField = resultField;
			KeyField = keyField;
		}

		#endregion

		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ensurePrecision(sender, null);
		}

		protected override void _ensurePrecision(PXCache sender, object row) =>
			_Precision = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(sender.Graph).PriceCostDecimalPlaces();
		#endregion
	}
}
