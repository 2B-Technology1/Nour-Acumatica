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
using PX.Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class TaxJurisdiction
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Federal, State, Local, Municipal, SchoolDistrict, Unknown },
				new string[] { Messages.Federal, Messages.State, Messages.Local, Messages.Municipal, Messages.SchoolDistrict, Messages.UnknownJurisdiction })
			{ }
		}

		public class federal : PX.Data.BQL.BqlString.Constant<federal>
		{
			public federal() : base(Federal) { }
		}

		public class state : PX.Data.BQL.BqlString.Constant<state>
		{
			public state() : base(State) { }
		}

		public class local : PX.Data.BQL.BqlString.Constant<local>
		{
			public local() : base(Local) { }
		}

		public class municipal : PX.Data.BQL.BqlString.Constant<municipal>
		{
			public municipal() : base(Municipal) { }
		}

		public class schoolDistrict : PX.Data.BQL.BqlString.Constant<schoolDistrict>
		{
			public schoolDistrict() : base(SchoolDistrict) { }
		}

		public const string Federal = "FED";
		public const string State = "STE";
		public const string Local = "LCL";
		public const string Municipal = "MUN";
		public const string SchoolDistrict = "SCH";
		public const string Unknown = "ZZZ";

		public static string GetTaxJurisdiction(Payroll.TaxJurisdiction taxJurisdiction)
		{
			switch (taxJurisdiction)
			{
				case Payroll.TaxJurisdiction.Federal:
					return TaxJurisdiction.Federal;
				case Payroll.TaxJurisdiction.State:
					return TaxJurisdiction.State;
				case Payroll.TaxJurisdiction.Local:
					return TaxJurisdiction.Local;
				case Payroll.TaxJurisdiction.Municipal:
					return TaxJurisdiction.Municipal;
				case Payroll.TaxJurisdiction.SchoolDistrict:
					return TaxJurisdiction.SchoolDistrict;
				default:
					return TaxJurisdiction.Unknown;
			}
		}

		public static Payroll.TaxJurisdiction GetTaxJurisdiction(string taxJurisdiction)
		{
			switch (taxJurisdiction)
			{
				case TaxJurisdiction.Federal:
					return Payroll.TaxJurisdiction.Federal;
				case TaxJurisdiction.State:
					return Payroll.TaxJurisdiction.State;
				case TaxJurisdiction.Local:
					return Payroll.TaxJurisdiction.Local;
				case TaxJurisdiction.Municipal:
					return Payroll.TaxJurisdiction.Municipal;
				case TaxJurisdiction.SchoolDistrict:
					return Payroll.TaxJurisdiction.SchoolDistrict;
				default:
					throw new PXException(Messages.UnsupportedTaxJurisdictionLevel, taxJurisdiction);
			}
		}
	}
}
