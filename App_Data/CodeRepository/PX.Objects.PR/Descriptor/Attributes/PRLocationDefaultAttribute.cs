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
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	public class PRLocationDefaultAttribute : PXDefaultAttribute
	{
		private Type _EmployeeIDField;
		private Type _ProjectIDField;

		public PRLocationDefaultAttribute(Type employeeIDField, Type projectIDField)
		{
			_EmployeeIDField = employeeIDField;
			_ProjectIDField = projectIDField;
		}

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);

			int? employeeID = sender.GetValue(e.Row, _EmployeeIDField.Name) as int?;
			int? projectID = sender.GetValue(e.Row, _ProjectIDField.Name) as int?;
			PREmployee employee = PREmployee.PK.Find(sender.Graph, employeeID);
			PREarningDetail row = e.Row as PREarningDetail;

			if (employee == null)
			{
				return;
			}

			int? payrollProjectWorkLocationID = null;

			if (projectID != null && !ProjectDefaultAttribute.IsNonProject(projectID))
			{
				PMProject project = PMProject.PK.Find(sender.Graph, projectID);
				PMProjectExtension projectExtension = sender.Graph.Caches[typeof(PMProject)].GetExtension<PMProjectExtension>(project);
				payrollProjectWorkLocationID = projectExtension.PayrollWorkLocationID;
			}

			if (employee.UsePayrollProjectWorkLocation == true && payrollProjectWorkLocationID != null )
			{
				e.NewValue = payrollProjectWorkLocationID;
			}
			else if (row.LocationID == null)
			{
				if (employee.LocationUseDflt == false)
				{
					PREmployeeWorkLocation employeeWorkLocation = SelectFrom<PREmployeeWorkLocation>
						.Where<PREmployeeWorkLocation.employeeID.IsEqual<P.AsInt>
							.And<PREmployeeWorkLocation.isDefault.IsEqual<True>>>
						.View.SelectSingleBound(sender.Graph, null, employeeID);

					e.NewValue = employeeWorkLocation?.LocationID;
				}
				else if (employee.LocationUseDflt == true)
				{
					PREmployeeClassWorkLocation employeeClassWorkLocation = SelectFrom<PREmployeeClassWorkLocation>
						.InnerJoin<PREmployee>.On<PREmployee.bAccountID.IsEqual<P.AsInt>>
						.Where<PREmployeeClassWorkLocation.employeeClassID.IsEqual<PREmployee.employeeClassID>
							.And<PREmployee.locationUseDflt.IsEqual<True>>
							.And<PREmployeeClassWorkLocation.isDefault.IsEqual<True>>>
						.View.SelectSingleBound(sender.Graph, null, employeeID);

					e.NewValue = employeeClassWorkLocation?.LocationID;
				}
			}
			else
			{
				e.NewValue = row.LocationID;
			}
		}
	}
}
