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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	public class TaxStateSelectorAttribute : PXSelectorAttribute
	{
		private Type _CountryIDField;

		public TaxStateSelectorAttribute(Type countryIDField)
			: base(BqlTemplate.OfCommand<
				  SearchFor<State.stateID>
					.Where<State.countryID.IsEqual<BqlPlaceholder.A.AsField.FromCurrent>>>
				.Replace<BqlPlaceholder.A>(countryIDField)
				.ToType())
		{
			DescriptionField = typeof(State.name);
			Filterable = true;
			_CountryIDField = countryIDField;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (e.ReturnValue == null)
			{
				if (sender.GetValue(e.Row, _CountryIDField.Name)?.Equals(LocationConstants.CanadaCountryCode) == true)
				{
					e.ReturnValue = LocationConstants.CanadaFederalStateCode;
				}
				else
				{
					e.ReturnValue = LocationConstants.USFederalStateCode;
				}
			}
		}
	}
}
