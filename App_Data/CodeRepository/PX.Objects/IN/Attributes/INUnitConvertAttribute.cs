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

namespace PX.Objects.IN
{
	[PXDecimal(4)]
	[PXUIField]
	public class INUnitConvertAttribute : PXAggregateAttribute
	{
		protected PXDecimalAttribute DecimalAttribute => GetAttribute<PXDecimalAttribute>();
		protected PXUIFieldAttribute UIFieldAttribute => GetAttribute<PXUIFieldAttribute>();

		protected readonly Type BaseValue;
		protected readonly Type FromUOM;
		protected readonly Type ToUOM;

		public string DisplayName
		{
			get => UIFieldAttribute?.DisplayName;
			set
			{
				if (UIFieldAttribute != null)
					UIFieldAttribute.DisplayName = value;
			}
		}

		public bool Enabled
		{
			get => UIFieldAttribute?.Enabled ?? true;
			set
			{
				if (UIFieldAttribute != null)
					UIFieldAttribute.Enabled = value;
			}
		}

		public INUnitConvertAttribute(Type baseValue, Type fromUOM, Type toUOM)
		{
			BaseValue = baseValue;
			FromUOM = fromUOM;
			ToUOM = toUOM;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.RowSelecting.AddHandler(BqlTable, RowSelecting);

			sender.Graph.FieldUpdated.AddHandler(BqlTable, BaseValue.Name, RelatedValueUpdated);
			sender.Graph.FieldUpdated.AddHandler(BqlTable, FromUOM.Name, RelatedValueUpdated);
			sender.Graph.FieldUpdated.AddHandler(BqlTable, ToUOM.Name, RelatedValueUpdated);
		}

		protected virtual void RelatedValueUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ReclaculateValue(sender, e.Row);
		}

		protected virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			// Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [not needed after AC-201935]
			var value = ConvertBaseValue(sender, e.Row, (decimal?)sender.GetValue(e.Row, BaseValue.Name));
			if (value != null)
				sender.SetValue(e.Row, FieldOrdinal, value);
		}

		protected virtual decimal? ConvertBaseValue(PXCache sender, object row, decimal? baseValue)
		{
			if (baseValue == null)
				return null;

			if (baseValue == 0)
				return 0m;

			var fromUOM = (string)sender.GetValue(row, FromUOM.Name);
			var toUOM = (string)sender.GetValue(row, ToUOM.Name);

			if (fromUOM == null || toUOM == null)
				return null;

			if (string.Equals(fromUOM, toUOM, StringComparison.InvariantCultureIgnoreCase))
				return baseValue;

			bool viceVersa = false;
			var conversion = INUnit.UK.ByGlobal.Find(sender.Graph, fromUOM, toUOM);
			if (conversion == null)
			{
				conversion = INUnit.UK.ByGlobal.Find(sender.Graph, toUOM, fromUOM);
				viceVersa = true;
			}
			if (conversion == null)
				return null;

			var value = INUnitAttribute.Convert(sender, conversion, baseValue ?? 0, INPrecision.NOROUND, viceVersa);
			return value;
		}

		protected virtual void ReclaculateValue(PXCache cache, object row)
		{
			var oldValue = (decimal?)cache.GetValue(cache, FieldOrdinal);
			var newValue = ConvertBaseValue(cache, row, (decimal?)cache.GetValue(row, BaseValue.Name));

			if (!Equals(oldValue, newValue))
				cache.SetValueExt(row, FieldName, newValue);
		}
	}
}
