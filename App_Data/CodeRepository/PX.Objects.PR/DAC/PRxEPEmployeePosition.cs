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
using PX.Objects.EP;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Payroll Module's extension of the EPEmployeePosition DAC.
	/// </summary>
	public sealed class PRxEPEmployeePosition : PXCacheExtension<EPEmployeePosition>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region SettlementPaycheckRefNoteID
		public abstract class settlementPaycheckRefNoteID : PX.Data.BQL.BqlGuid.Field<settlementPaycheckRefNoteID> { }
		[PXUIField(DisplayName = "Final Payment", Enabled = false, Visible = false)]
		[PXRefNote]
		public Guid? SettlementPaycheckRefNoteID { get; set; }
		#endregion
	}
}
