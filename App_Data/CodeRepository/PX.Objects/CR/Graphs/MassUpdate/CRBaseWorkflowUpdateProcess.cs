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

using PX.Common;
using PX.Data;
using PX.Data.Automation;
using PX.Data.BQL;
using PX.Data.MassProcess;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.SM;


namespace PX.Objects.CR
{
	#region MassActionFilter

	/// <exclude/>
	[PXHidden]
	public partial class CRWorkflowMassActionFilter : IBqlTable
	{
		#region Operation
		public abstract class operation : BqlString.Field<operation> { }
		[PXUIField(DisplayName = "Operation")]
		[PXString]
		[CRWorkflowMassActionOperation.List]
		[PXUnboundDefault(CRWorkflowMassActionOperation.UpdateSettings)]
		public virtual string Operation { get; set; }
		#endregion

		#region Action
		public abstract class action : BqlString.Field<action> { }
		[PXWorkflowMassProcessing(DisplayName = "Action", AddUndefinedState = false)]
		public virtual string Action { get; set; }
		#endregion
	}

	/// <exclude/>
	public static class CRWorkflowMassActionOperation
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[] {
					UpdateSettings,
					ExecuteAction,
				},
				new[] {
					"Update Settings",
					"Execute Action",
				})
			{ }
		}

		public const string UpdateSettings = "Update";
		public const string ExecuteAction = "Execute";
		public class updateSettings : BqlString.Constant<updateSettings>
		{
			public updateSettings() : base(UpdateSettings) { }
		}
		public class executeAction : BqlString.Constant<executeAction>
		{
			public executeAction() : base(ExecuteAction) { }
		}
	}

	#endregion


	/// <exclude/>
	[PXInternalUseOnly]
	public abstract class CRBaseWorkflowUpdateProcess<TGraph, TPrimaryGraph, TPrimary, TMarkAttribute, TClassField> : CRBaseUpdateProcess<TGraph, TPrimary, TMarkAttribute, TClassField>
		where TGraph : PXGraph, IMassProcess<TPrimary>, new()
		where TPrimaryGraph : PXGraph, new()
		where TPrimary : class, IBqlTable, IPXSelectable, new()
		where TMarkAttribute : PXEventSubscriberAttribute
		where TClassField : IBqlField
	{
		public PXFilter<CRWorkflowMassActionFilter> Filter;
		public new PXCancel<CRWorkflowMassActionFilter> Cancel;
		protected abstract PXFilteredProcessing<TPrimary, CRWorkflowMassActionFilter> ProcessingView { get; }

		protected bool IsDefaultWorkflowExists = false;

		public CRBaseWorkflowUpdateProcess()
		{
			PXGraph pxGraph = CreateInstance<TPrimaryGraph>();
			IsDefaultWorkflowExists = pxGraph.IsWorkflowExists() && pxGraph.IsWorkflowDefinitionDefined();
		}

		protected virtual void _(Events.RowSelected<CRWorkflowMassActionFilter> e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetVisible<CRWorkflowMassActionFilter.operation>(e.Cache, null, IsDefaultWorkflowExists);
			PXUIFieldAttribute.SetVisible<CRWorkflowMassActionFilter.action>(e.Cache, null, IsDefaultWorkflowExists);

			if (e.Row.Operation != CRWorkflowMassActionOperation.ExecuteAction)
			{
				PXUIFieldAttribute.SetEnabled<CRWorkflowMassActionFilter.action>(e.Cache, e.Row, false);
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Filter value just for UI]
				e.Row.Action = string.Empty;
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<CRWorkflowMassActionFilter.action>(e.Cache, e.Row, true);
				if (!string.IsNullOrEmpty(e.Row.Action))
					ProcessingView.SetProcessWorkflowAction(e.Row.Action);
			}
		}
	}
}
