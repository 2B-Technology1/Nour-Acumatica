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
using PX.Data.EP;

namespace PX.Objects.EP
{
	/// <summary>
	/// The helper for approval automation that does not set the Hold value to true (while the base class of the helper sets the Hold value to true).
	/// </summary>
	/// <typeparam name="SourceAssign"></typeparam>
	/// <typeparam name="Approved"></typeparam>
	/// <typeparam name="Rejected"></typeparam>
	/// <typeparam name="Hold"></typeparam>
	/// <typeparam name="SetupApproval"></typeparam>
	public class EPApprovalAutomationWithoutHoldDefaulting<SourceAssign, Approved, Rejected, Hold, SetupApproval> 
		:  EPApprovalAutomation<SourceAssign, Approved, Rejected, Hold, SetupApproval>
		where SourceAssign : class, IAssign, IBqlTable, new()
		where Approved : class, IBqlField
		where Rejected : class, IBqlField
		where Hold : class, IBqlField
		where SetupApproval : class, IAssignedMap, IBqlTable, new()
	{
		public EPApprovalAutomationWithoutHoldDefaulting(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
	
		}

		public EPApprovalAutomationWithoutHoldDefaulting(PXGraph graph)
			: base(graph)
		{
	
		}

		protected override void Hold_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
		}
	}
}