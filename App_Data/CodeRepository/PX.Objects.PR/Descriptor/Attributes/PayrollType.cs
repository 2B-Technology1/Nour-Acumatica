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
using System;

namespace PX.Objects.PR
{
	public class PayrollType
	{
		public class BatchListAttribute : PXStringListAttribute
		{
			public BatchListAttribute()
				: base(
				new string[] { Regular, Special },
				new string[] { Messages.Regular, Messages.Special })
			{ }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Regular, Special, Adjustment, VoidCheck, Final },
				new string[] { Messages.Regular, Messages.Special, Messages.Adjustment, Messages.VoidCheck, Messages.Final })
			{ }
		}



		public class regular : PX.Data.BQL.BqlString.Constant<regular>
		{
			public regular() : base(Regular) { }
		}

		public class special : PX.Data.BQL.BqlString.Constant<special>
		{
			public special() : base(Special) { }
		}

		public class adjustment : PX.Data.BQL.BqlString.Constant<adjustment>
		{
			public adjustment() : base(Adjustment) { }
		}

		public class voidCheck : PX.Data.BQL.BqlString.Constant<voidCheck>
		{
			public voidCheck() : base(VoidCheck) { }
		}

		public class final : PX.Data.BQL.BqlString.Constant<final>
		{
			public final() : base(Final) { }
		}

		public const string Regular = "REG";
		public const string Special = "SPC";
		public const string Adjustment = "ADJ";
		public const string VoidCheck = "VCK";
		public const string VoidPayment = "RPM";
		public const string Final = "FIN";

		public static string DrCr(string docType)
		{
			switch (docType)
			{
				case Regular:
				case Special:
				case Adjustment:
				case VoidCheck:
				case Final:
					return GL.DrCr.Debit;
				default:
					return null;
			}
		}

		/// <summary>
		/// Specialized for PRPayments version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the PR Payment. <br/>
		/// References PRPayment.docType and PRPayment.transactionDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in PR Setup and APPayment types:<br/>
		/// namely - PRSetup.tranNumberingCD for all the types beside VoidCheck <br/>
		/// </summary>
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(PRPayment.docType), typeof(PRPayment.transactionDate),
					_DocTypes,
					_SetupFields)
			{ }

			private static string[] _DocTypes
			{
				get
				{
					return new string[] { Regular, Special, Adjustment, VoidCheck, Final };
				}
			}

			private static Type[] _SetupFields
			{
				get
				{
					return new Type[]
					{
						typeof(PRSetup.tranNumberingCD),
						typeof(PRSetup.tranNumberingCD),
						typeof(PRSetup.tranNumberingCD),
						null,
						typeof(PRSetup.tranNumberingCD),
					};
				}
			}
		}
	}
}
