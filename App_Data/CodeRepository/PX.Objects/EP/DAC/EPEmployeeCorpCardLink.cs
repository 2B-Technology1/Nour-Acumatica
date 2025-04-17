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
using PX.Objects.CA;

namespace PX.Objects.EP.DAC
{
	[PXCacheName(Messages.EmployeeCorpCardReference)]
	public class EPEmployeeCorpCardLink : IBqlTable
	{
		[PXDBInt(IsKey = true)]
		[PXEPEmployeeSelector]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<employeeID>>>>))]
		[PXUIField(DisplayName = "Employee ID")]
		public int? EmployeeID { get; set; }
		public abstract class employeeID : BqlInt.Field<employeeID> { }

		[PXDBInt(IsKey = true)]
		[PXParent(typeof(Select<CACorpCard, Where<CACorpCard.corpCardID, Equal<Current<corpCardID>>>>))]
		[PXSelector(typeof(Search<CACorpCard.corpCardID>),
			typeof(CACorpCard.corpCardCD), typeof(CACorpCard.name), typeof(CACorpCard.cardNumber), typeof(CACorpCard.cashAccountID),
			SubstituteKey = typeof(CACorpCard.corpCardCD))]
		[PXUIField(DisplayName = "Corporate Card ID")]
		public int? CorpCardID { get; set; }
		public abstract class corpCardID : BqlInt.Field<corpCardID> { }
	}
}