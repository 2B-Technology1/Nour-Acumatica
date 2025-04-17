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

namespace PX.Objects.DR.DAC.ReportParameters
{
	public class DRReportParameters: IBqlTable
	{
		#region pendingRevenueValidate
		public abstract class pendingRevenueValidate : PX.Data.BQL.BqlBool.Field<pendingRevenueValidate> { }

		[PXDBBool]
		[PXDefault(typeof(Search<DRSetup.pendingRevenueValidate>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? PendingRevenueValidate { get; set; }
		#endregion
		#region pendingExpenseValidate
		public abstract class pendingExpenseValidate : PX.Data.BQL.BqlBool.Field<pendingExpenseValidate> { }

		[PXDBBool]
		[PXDefault(typeof(Search<DRSetup.pendingExpenseValidate>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? PendingExpenseValidate { get; set; }
		#endregion
	}
}
