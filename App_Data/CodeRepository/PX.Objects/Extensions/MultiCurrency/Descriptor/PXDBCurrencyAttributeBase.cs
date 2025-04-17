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
using PX.Objects.Extensions.MultiCurrency;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CM.Extensions
{
	public abstract class PXDBCurrencyAttributeBase : PXDBDecimalAttribute, ICurrencyAttribute
	{
		#region State

		public Type ResultField { get; }
		public Type KeyField { get; }

		public virtual bool BaseCalc { get; set; } = true;
		public virtual int? CustomPrecision => null;

		#endregion

		#region Initialization

		public PXDBCurrencyAttributeBase(Type keyField, Type resultField)
		{
			ResultField = resultField;
			KeyField = keyField;
		}

		public PXDBCurrencyAttributeBase(Type precision, Type keyField, Type resultField)
			: base(precision)
		{
			ResultField = resultField;
			KeyField = keyField;
		}

		public PXDBCurrencyAttributeBase(int precision, Type keyField, Type resultField)
			: base(precision)
		{
			ResultField = resultField;
			KeyField = keyField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			void subscribeToEvents(PXGraph graph)
			{
				Type itemType = sender.GetItemType();
				var curyHost = sender.Graph.FindImplementation<ICurrencyHost>();

				if (curyHost != null && !curyHost.IsTrackedType(itemType))
				{
					//We need an ability to toggle cury - base values on DACs from Join<> that are shown on UI.
					sender.Graph.FieldSelecting.AddHandler(itemType, FieldName, (s, e) => CuryFieldSelecting(s, e, new CuryField(this)));
					sender.Graph.RowPersisting.AddHandler(itemType, (s, e) => CuryRowPersisting(s, e, new CuryField(this)));
				}
			}

			if (sender.Graph.IsInitializing)
				sender.Graph.Initialized += subscribeToEvents;
			else
				subscribeToEvents(sender.Graph);
		}

		#endregion

		#region Implementation

		protected virtual void CuryFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, CuryField curyField)
		{
			if (sender.Graph.Accessinfo.CuryViewState && !string.IsNullOrEmpty(curyField.BaseName))
			{
				e.ReturnValue = sender.GetValue(e.Row, curyField.BaseName);
				var curyHost = sender.Graph.FindImplementation<ICurrencyHost>();
				if (CM.PXCurrencyAttribute.IsNullOrEmpty(e.ReturnValue as decimal?) && curyHost != null)
				{
					object curyValue = sender.GetValue(e.Row, curyField.CuryName);
					CurrencyInfo curyInfo = curyHost.GetCurrencyInfo(sender, e.Row, curyField.CuryInfoIDName);

					curyField.RecalculateFieldBaseValue(sender, e.Row, curyValue, curyInfo, true);
					e.ReturnValue = sender.GetValue(e.Row, curyField.BaseName);
				}

				if (e.IsAltered)
				{
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, enabled: false);
				}
			}
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CuryField curyField = new CuryField(this);

			decimal? curyValue = (decimal?)sender.GetValue(e.Row, curyField.CuryName);

			if (curyValue != null
				&& ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && !object.Equals(curyValue, sender.GetValueOriginal(e.Row, _FieldName))
				|| (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert))
			{
				if (curyField.CustomPrecision == null)
				{
					CurrencyInfo curyInfo = sender.Graph.FindImplementation<ICurrencyHost>()?.GetCurrencyInfo(sender, e.Row, curyField.CuryInfoIDName);
					sender.SetValue(e.Row, curyField.CuryName, curyInfo?.RoundCury(curyValue.Value) ?? curyValue.Value);
				}
				else
				{
					sender.SetValue(e.Row, curyField.CuryName, Math.Round(curyValue.Value, curyField.CustomPrecision.Value, MidpointRounding.AwayFromZero));
				}
			}

			base.RowPersisting(sender, e);
		}

		protected virtual void CuryRowPersisting(PXCache sender, PXRowPersistingEventArgs e, CuryField curyField)
		{
			var curyHost = sender.Graph.FindImplementation<ICurrencyHost>();
			CurrencyInfo curyInfo = curyHost.GetCurrencyInfo(sender, e.Row, curyField.CuryInfoIDName);

			decimal? curyValue = (decimal?)sender.GetValue(e.Row, curyField.CuryName);
			curyField.RecalculateFieldBaseValue(sender, e.Row, curyValue, curyInfo);
		}

		#endregion
	}
}
