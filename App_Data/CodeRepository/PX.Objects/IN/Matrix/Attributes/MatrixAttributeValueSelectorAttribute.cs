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
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC.Unbound;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.IN.Matrix.Attributes
{
	public class MatrixAttributeValueSelectorAttribute : PXCustomSelectorAttribute
	{
		protected int? _attributeNumber;
		protected bool _showDisabled;

		public MatrixAttributeValueSelectorAttribute(int attributeNumber, bool showDisabled)
			: base(typeof(CSAttributeDetail.valueID), new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) })
		{
			_attributeNumber = attributeNumber;
			_showDisabled = showDisabled;
			base.CacheGlobal = false;
			base.ValidateValue = true;
			base.DescriptionField = typeof(CSAttributeDetail.description);
		}

		public MatrixAttributeValueSelectorAttribute()
			: base(typeof(CSAttributeDetail.valueID), new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) })
		{
			_attributeNumber = null;
			base.CacheGlobal = false;
			base.ValidateValue = true;
			base.DescriptionField = typeof(CSAttributeDetail.description);
		}


		protected virtual IEnumerable GetRecords()
		{
			int? ControlType;
			IEnumerable Values;
			string attributeID = GetAttributeID();
			ControlType = this.GetControlType(attributeID);

			switch (ControlType)
			{
				case CSAttribute.Combo:
					var valuesSelect = new PXSelect<CSAttributeDetail,
						Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>>,
						OrderBy<Asc<CSAttributeDetail.sortOrder>>>(this._Graph);

					if (!_showDisabled)
						valuesSelect.WhereAnd<Where<CSAttributeDetail.disabled, NotEqual<True>>>();

					Values = valuesSelect.Select(attributeID);
					break;
				case CSAttribute.CheckBox:
					Values = new List<CSAttributeDetail>()
					{
						new CSAttributeDetail() { Description = "True", ValueID = "True" },
						new CSAttributeDetail() { Description = "False", ValueID = "False" }
					};
					break;
				default:
					Values = new List<CSAttributeDetail>();
					break;
			}

			return Values;
		}

		protected virtual int? GetControlType(string AttributeID)
		{
			CSAttribute Attr;
			int? ControlType;

			ControlType = null;
			if (AttributeID != null)
			{
				Attr = new PXSelect<CSAttribute, Where<CSAttribute.attributeID, Equal<Required<CSAttribute.attributeID>>>>(this._Graph).SelectSingle(AttributeID);
				if (Attr != null)
					ControlType = Attr.ControlType;
			}
			return ControlType;
		}

		protected virtual string GetAttributeID()
		{
			if (_attributeNumber == null)
				return null;

			var additionalAttributes = (AdditionalAttributes)_Graph.Caches<AdditionalAttributes>().Current;
			return additionalAttributes.AttributeIdentifiers[(int)_attributeNumber];
		}

		protected override void EmitColumnForDescriptionField(PXCache sender)
		{
		}
	}
}
