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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR.Standalone
{
	/// <summary>
	/// Standalone DAC related to PR.Objects.PR.PREmployee />
	/// </summary>
	[PXCacheName("Payroll Employee")]
	[Serializable]
	public class PREmployee : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployee>.By<bAccountID>
		{
			public static PREmployee Find(PXGraph graph, int? bAccountID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bAccountID, options);
		}

		public new static class FK
		{
			public class EmployeeClass : PREmployeeClass.PK.ForeignKeyOf<PREmployee>.By<employeeClassID> { }
			public class WorkCode : PMWorkCode.PK.ForeignKeyOf<PREmployee>.By<workCodeID> { }
		}
		#endregion Keys
		
		#region BAccountID
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }
		/// <summary>
		/// Key field used to retrieve an Employee
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(EPEmployee.bAccountID))]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<PREmployee.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		#endregion
		#region ActiveInPayroll
		public abstract class activeInPayroll : BqlBool.Field<activeInPayroll> { }
		/// <summary>
		/// Indicates whether the employee is active in the payroll module
		/// </summary>
		[PXDBBool]
		public bool? ActiveInPayroll { get; set; }
		#endregion
		#region EmployeeClassID
		public abstract class employeeClassID : BqlString.Field<employeeClassID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(PREmployeeClass.employeeClassID))]
		public string EmployeeClassID { get; set; }
		#endregion
		#region WorkCodeUseDflt
		public abstract class workCodeUseDflt : BqlBool.Field<workCodeUseDflt> { }
		[PXDBBool]
		public bool? WorkCodeUseDflt { get; set; }
		#endregion
		#region WorkCodeID
		public abstract class workCodeID : BqlString.Field<workCodeID> { }
		[PXDBString(PMWorkCode.workCodeID.Length)]
		[PXUnboundDefault(typeof(Switch<Case<Where<workCodeUseDflt.IsEqual<True>>, Selector<employeeClassID, PREmployeeClass.workCodeID>>,
			workCodeID>))]
		public string WorkCodeID { get; set; }
		#endregion
	}
}