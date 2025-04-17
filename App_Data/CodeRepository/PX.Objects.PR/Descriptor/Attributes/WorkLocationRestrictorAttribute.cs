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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PR
{
	public class WorkLocationRestrictorAttribute : PXRestrictorAttribute
	{
		public WorkLocationRestrictorAttribute() : base(
			typeof(Where<PREmployee.locationUseDflt.IsNotEqual<True>
			.And<PRLocation.locationID.IsInSubselect<SelectFrom<PREmployeeWorkLocation>
				.Where<PREmployeeWorkLocation.employeeID.IsEqual<PREarningDetail.employeeID.FromCurrent>>
				.SearchFor<PREmployeeWorkLocation.locationID>>>
			.Or<PREmployee.locationUseDflt.IsEqual<True>
				.And<PRLocation.locationID.IsInSubselect<Search2<PREmployeeClassWorkLocation.locationID,
					InnerJoin<PREmployee, On<PREmployee.bAccountID, Equal<Current<PREarningDetail.employeeID>>>>,
					Where<PREmployeeClassWorkLocation.employeeClassID, Equal<PREmployee.employeeClassID>>>>>>>), "")
		{ }

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<PRLocation, PREmployee> result = GetItem(sender, this, e.Row, e.NewValue) as PXResult<PRLocation, PREmployee>;
			PRLocation location = result;
			PREmployee employee = result;

			if (location == null || employee == null)
			{
				return;
			}	

			if (employee.LocationUseDflt == true && !SelectFrom<PREmployeeClassWorkLocation>
				.Where<PREmployeeClassWorkLocation.locationID.IsEqual<P.AsInt>
					.And<PREmployeeClassWorkLocation.employeeClassID.IsEqual<P.AsString>>>.View.Select(sender.Graph, location.LocationID, employee.EmployeeClassID).Any())
			{
				if (_SubstituteKey != null)
				{
					object errorValue = e.NewValue;
					sender.RaiseFieldSelecting(_FieldName, e.Row, ref errorValue, false);
					PXFieldState state = errorValue as PXFieldState;
					e.NewValue = state != null ? state.Value : errorValue;
				}

				throw new PXSetPropertyException(Messages.LocationNotSetInEmployeeClass, location.LocationCD, employee.EmployeeClassID);
			}
			else if (employee.LocationUseDflt != true && !SelectFrom<PREmployeeWorkLocation>
				.Where<PREmployeeWorkLocation.locationID.IsEqual<P.AsInt>
					.And<PREmployeeWorkLocation.employeeID.IsEqual<P.AsInt>>>.View.Select(sender.Graph, location.LocationID, employee.BAccountID).Any())
			{
				if (_SubstituteKey != null)
				{
					object errorValue = e.NewValue;
					sender.RaiseFieldSelecting(_FieldName, e.Row, ref errorValue, false);
					PXFieldState state = errorValue as PXFieldState;
					e.NewValue = state != null ? state.Value : errorValue;
				}

				throw new PXSetPropertyException(Messages.LocationNotSetInEmployee, location.LocationCD, employee.AcctCD);
			}
		}
	}
}
