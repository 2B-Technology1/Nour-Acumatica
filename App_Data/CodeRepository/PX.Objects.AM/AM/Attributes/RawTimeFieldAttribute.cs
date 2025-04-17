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

namespace PX.Objects.AM.Attributes
{
	/// <summary>
	/// <see cref="PXIntAttribute"/> time field which has no formating.
	/// </summary>
	[PXInt]
	[PXUIField(Visibility = PXUIVisibility.Invisible)]
	public class RawTimeFieldAttribute : PXEntityAttribute, IPXFieldVerifyingSubscriber
	{
		public int RawMinValue = 0;
		public int RawMaxValue = int.MaxValue;
		protected readonly Type DependsOnField;

		public RawTimeFieldAttribute(Type dependsOnField)
		{
			DependsOnField = dependsOnField ?? throw new ArgumentNullException(nameof(dependsOnField));
			this.UIAttribute.MapErrorTo = dependsOnField;
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!sender.Graph.IsImport && !sender.Graph.IsContractBasedAPI)
			{
				return;
			}

			var newValueInt = (int?)e.NewValue;
			if (newValueInt == null)
			{
				return;
			}

			if (newValueInt < RawMinValue)
			{
				throw new PXSetPropertyException(Messages.ValueCannotBeLessThan, PXUIFieldAttribute.GetDisplayName(sender, DependsOnField.Name), newValueInt, RawMinValue);
			}

			if (newValueInt > RawMaxValue)
			{
				throw new PXSetPropertyException(Messages.ValueCannotBeGreaterThan, PXUIFieldAttribute.GetDisplayName(sender, DependsOnField.Name), newValueInt, RawMinValue);
			}
		}
	}
}
