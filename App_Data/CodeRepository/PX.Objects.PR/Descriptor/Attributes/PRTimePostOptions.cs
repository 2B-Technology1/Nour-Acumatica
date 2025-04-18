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
using PX.Objects.EP;
using System;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class PRTimePostOptions : EPPostOptions
	{
		public new class ListAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
		{
			public ListAttribute() : base(GetValuesAndLabels())
			{
			}

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				PRSetup row = e.Row as PRSetup;

				SetList(sender, row, FieldName, GetValuesAndLabels(row?.ProjectCostAssignment, false));
			}

			private static Tuple<string, string>[] GetValuesAndLabels(string projectCostAssignmentType = null, bool addAllValues = true)
			{
				List<Tuple<string, string>> valuesAndLabels = new List<Tuple<string, string>>();

				if (addAllValues || projectCostAssignmentType == ProjectCostAssignmentType.NoCostAssigned)
				{
					valuesAndLabels.Add(new Tuple<string, string>(DoNotPost, Messages.DoNotPost));
					valuesAndLabels.Add(new Tuple<string, string>(PostToOffBalance, Messages.PostFromTime));
				}
				if (addAllValues || projectCostAssignmentType == ProjectCostAssignmentType.WageCostAssigned || projectCostAssignmentType == ProjectCostAssignmentType.WageLaborBurdenAssigned)
				{
					valuesAndLabels.Add(new Tuple<string, string>(OverridePMInPayroll, Messages.OverridePMInPayroll));
					valuesAndLabels.Add(new Tuple<string, string>(OverridePMAndGLInPayroll, Messages.OverridePMAndGLInPayroll));
					valuesAndLabels.Add(new Tuple<string, string>(PostPMAndGLFromPayroll, Messages.PostPMandGLFromPayroll));
				}

				return valuesAndLabels.ToArray();
			}
		}
	}
}
