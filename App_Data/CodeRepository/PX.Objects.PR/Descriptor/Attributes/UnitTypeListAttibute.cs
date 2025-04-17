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
using System;

namespace PX.Objects.PR
{
	public class UnitType
	{
		public class ListAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
		{
			private readonly Type _pieceworkEarningTypeField;
			private readonly Type _unitTypeField;

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				if (_pieceworkEarningTypeField == null || _unitTypeField == null || e.Row == null)
					return;

				bool showTimeUnits = true;

				PRSetup preferences = sender.Graph.Caches[typeof(PRSetup)].Current as PRSetup ??
					new PXSetupSelect<PRSetup>(sender.Graph).SelectSingle();

				if (preferences.EnablePieceworkEarningType == true)
				{
					bool? isPieceworkEarningType = (bool?)sender.GetValue(e.Row, _pieceworkEarningTypeField.Name);
					if (isPieceworkEarningType == true)
						showTimeUnits = false;
				}

				Tuple<string, string>[] valuesToLabels = showTimeUnits ? GetTimeUnits() : GetMiscUnit();
				SetList(sender, e.Row, _unitTypeField.Name, valuesToLabels);
			}

			public ListAttribute() :
				base(new string[] { Hour, Year, Misc }, new string[] { Messages.Hour, Messages.Year, Messages.Misc })
			{
			}

			public ListAttribute(Type pieceworkEarningTypeField, Type unitTypeField) : this()
			{
				_pieceworkEarningTypeField = pieceworkEarningTypeField;
				_unitTypeField = unitTypeField;
			}

			private static Tuple<string, string>[] GetTimeUnits()
			{
				return new Tuple<string, string>[]
				{
					Pair(Hour, Messages.Hour),
					Pair(Year, Messages.Year)
				};
			}

			private static Tuple<string, string>[] GetMiscUnit()
			{
				return new Tuple<string, string>[]
				{
					Pair(Misc, Messages.Misc)
				};
			}
		}

		public class hour : PX.Data.BQL.BqlString.Constant<hour>
		{
			public hour() : base(Hour)
			{
			}
		}

		public class year : PX.Data.BQL.BqlString.Constant<year>
		{
			public year() : base(Year)
			{
			}
		}

		public class misc : PX.Data.BQL.BqlString.Constant<misc>
		{
			public misc() : base(Misc)
			{
			}
		}

		public const string Hour = "HOR";
		public const string Year = "SAL";
		public const string Misc = "MSC";
	}
}
