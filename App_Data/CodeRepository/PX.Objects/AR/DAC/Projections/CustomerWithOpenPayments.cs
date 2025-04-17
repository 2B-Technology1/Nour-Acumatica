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

namespace PX.Objects.AR
{

		[PXProjection(typeof(Select5<
			Standalone.ARRegister,
			InnerJoin<Customer,
				On<Customer.bAccountID, Equal<Standalone.ARRegister.customerID>>>,
			Where<Standalone.ARRegister.released, Equal<True>,
				And<Standalone.ARRegister.openDoc, Equal<True>,
				And<Where<
					Standalone.ARRegister.docType, Equal<ARDocType.payment>,
					Or<Standalone.ARRegister.docType, Equal<ARDocType.prepayment>,
					Or<Standalone.ARRegister.docType, Equal<ARDocType.creditMemo>>>>>>>,
			Aggregate<
				GroupBy<Standalone.ARRegister.customerID,
				GroupBy<Customer.statementCycleId>>>>)
		, Persistent = false)]
		[PXHidden]
		public partial class CustomerWithOpenPayments : PX.Data.IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[PXDBInt(IsKey = true, BqlField = typeof(ARRegister.customerID))]
			public virtual int? CustomerID { get; set; }
			#endregion
			
			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
			[PXDBString(10, IsUnicode = true, BqlField = typeof(Customer.statementCycleId))]
			public virtual String StatementCycleId { get; set; }
			#endregion
		}

		[PXProjection(typeof(Select5<
			Standalone.ARRegister,
			InnerJoin<Customer,
				On<Customer.bAccountID, Equal<Standalone.ARRegister.customerID>>>,
			Where<Standalone.ARRegister.released, Equal<True>,
				And<Standalone.ARRegister.openDoc, Equal<True>,
				And<Where<
					Standalone.ARRegister.docType, Equal<ARDocType.invoice>,
					Or<Standalone.ARRegister.docType, Equal<ARDocType.finCharge>,
					Or<Standalone.ARRegister.docType, Equal<ARDocType.debitMemo>>>>>>>,
			Aggregate<
				GroupBy<Standalone.ARRegister.customerID,
				GroupBy<Customer.statementCycleId>>>>)
		, Persistent = false)]
		[PXHidden]
		public partial class CustomerWithOpenInvoices : PX.Data.IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[PXDBInt(IsKey = true, BqlField = typeof(ARRegister.customerID))]
			public virtual int? CustomerID { get; set; }
			#endregion
			
			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
			[PXDBString(10, IsUnicode = true, BqlField = typeof(Customer.statementCycleId))]
			public virtual String StatementCycleId { get; set; }
			#endregion
		}
}