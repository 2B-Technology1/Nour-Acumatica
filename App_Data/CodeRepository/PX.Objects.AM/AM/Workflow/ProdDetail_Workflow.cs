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

using PX.Data.WorkflowAPI;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
	using State = ProductionOrderStatus;

	public class ProdDetail_Workflow : ProdMaintBase_Workflow<ProdDetail>
	{
		[PXWorkflowDependsOnType(typeof(AMPSetup))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProdDetail, AMProdItem>());
		protected static void Configure(WorkflowContext<ProdDetail, AMProdItem> context)
		{
			ConfigureCommon(context);
			var conditions = context.Conditions.GetPack<Conditions>();
			context.UpdateScreenConfigurationFor(screen =>
				screen.UpdateDefaultFlow(flow => flow
					.WithTransitions(transitions =>
					{
						transitions.UpdateGroupFrom<State.planned>(ts =>
						{
						});
					}))
					.WithActions(actions =>
					{
						#region Side Panels

						actions.AddNew("ShowProdMaint", a => a
							.DisplayName("Production Order Maintenance")
							.IsSidePanelScreen(sp => sp
								.NavigateToScreen<ProdMaint>()
								.WithIcon("visibility")
								.WithAssignments(ass =>
								{
									ass.Add(nameof(AMProdItem.OrderType), e => e.SetFromField<AMProdItem.orderType>());
									ass.Add(nameof(AMProdItem.ProdOrdID), e => e.SetFromField<AMProdItem.prodOrdID>());
								})));

						actions.AddNew("ShowLinkedSupply", a => a
							.DisplayName("Production Order Supply Documents")
							.IsSidePanelScreen(sp => sp
								.NavigateToScreen("AM0026SP")
								.WithIcon("flow")
								.WithAssignments(ass =>
								{
									ass.Add("OrderType", e => e.SetFromField<AMProdItem.orderType>());
									ass.Add("ProdID", e => e.SetFromField<AMProdItem.prodOrdID>());
								})));

						actions.AddNew("ShowVendorShipments", a => a
							.DisplayName("Vendor Shipments by Production Order")
							.IsSidePanelScreen(sp => sp
								.NavigateToScreen("AM3100SP")
								.WithIcon("local_shipping")
								.WithAssignments(ass =>
								{
									ass.Add("ProdOrderType", e => e.SetFromField<AMProdItem.orderType>());
									ass.Add("ProdOrder", e => e.SetFromField<AMProdItem.prodOrdID>());
								})));

						actions.AddNew("ShowWhereUsed", a => a
							.DisplayName("Where Used in Production Orders")
							.IsSidePanelScreen(sp => sp
								.NavigateToScreen("AM0027SP")
								.WithIcon("rate-review")
								.WithAssignments(ass =>
								{
									ass.Add(nameof(AMProdItem.OrderType), e => e.SetFromField<AMProdItem.orderType>());
									ass.Add(nameof(AMProdItem.ProdOrdID), e => e.SetFromField<AMProdItem.prodOrdID>());
								})));

						#endregion

					}));
		}
	}
}
