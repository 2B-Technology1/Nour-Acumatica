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
using PX.Objects.EP;
using System;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class EarningDetailType : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		private Type _HoursField;
		private Type _UnitsField;
		private Type _WorkCodeIDField;
		private IEnumerable<Type> _DependsOnFields;

		public EarningDetailType(Type hoursField, Type unitsField, Type workCodeIDField, Type[] dependsOnFields)
		{
			_HoursField = hoursField;
			_UnitsField = unitsField;
			_WorkCodeIDField = workCodeIDField;
			_DependsOnFields = dependsOnFields;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			foreach (Type field in _DependsOnFields)
			{
				sender.Graph.FieldUpdated.AddHandler(field.DeclaringType, field.Name, UpdateHoursUnits);
			}
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var epEarningType = PXSelectorAttribute.Select(sender, e.Row, _FieldName) as EPEarningType;
			if (epEarningType != null)
			{
				PREarningType prEarningType = PXCache<EPEarningType>.GetExtension<PREarningType>(epEarningType);
				if (prEarningType?.IsWCCCalculation != true)
				{
					sender.SetValue(e.Row, _WorkCodeIDField.Name, null);
				}
				else if (sender.GetValue(e.Row, _WorkCodeIDField.Name) == null)
				{
					sender.SetDefaultExt(e.Row, _WorkCodeIDField.Name);
				}
			}
		}

		public void UpdateHoursUnits(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var epEarningType = PXSelectorAttribute.Select(sender, e.Row, _FieldName) as EPEarningType;
			if (epEarningType != null)
			{
				PREarningType prEarningType = PXCache<EPEarningType>.GetExtension<PREarningType>(epEarningType);

				if (prEarningType?.IsAmountBased == true)
				{
					sender.SetValue(e.Row, _HoursField.Name, null);
					sender.SetValue(e.Row, _UnitsField.Name, null);
				}
				else
				{
					if (sender.GetValue(e.Row, _HoursField.Name) == null)
					{
						sender.SetDefaultExt(e.Row, _HoursField.Name);
					}

					if (prEarningType?.IsPiecework != true)
					{
						sender.SetValue(e.Row, _UnitsField.Name, null);
					}
					else if (sender.GetValue(e.Row, _UnitsField.Name) == null)
					{
						sender.SetDefaultExt(e.Row, _UnitsField.Name);
					}
				} 
			}
		}
	}
}
