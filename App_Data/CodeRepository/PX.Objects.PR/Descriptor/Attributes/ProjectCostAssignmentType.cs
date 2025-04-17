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

namespace PX.Objects.PR
{
	public class ProjectCostAssignmentType : PX.Objects.PR.Standalone.ProjectCostAssignmentType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { NoCostAssigned, WageCostAssigned, WageLaborBurdenAssigned },
				new string[] { Messages.NoCostAssigned, Messages.WageCostAssigned, Messages.WageLaborBurdenAssigned })
			{ }
		}

		#region Avoid 2020R1 breaking changes
		public new class noCostAssigned : Standalone.ProjectCostAssignmentType.noCostAssigned { }
		public new class wageCostAssigned : Standalone.ProjectCostAssignmentType.wageCostAssigned { }
		public new class wageLaborBurdenAssigned : Standalone.ProjectCostAssignmentType.wageLaborBurdenAssigned { }

		public new const string NoCostAssigned = Standalone.ProjectCostAssignmentType.NoCostAssigned;
		public new const string WageCostAssigned = Standalone.ProjectCostAssignmentType.WageCostAssigned;
		public new const string WageLaborBurdenAssigned = Standalone.ProjectCostAssignmentType.WageLaborBurdenAssigned; 
		#endregion
	}
}
