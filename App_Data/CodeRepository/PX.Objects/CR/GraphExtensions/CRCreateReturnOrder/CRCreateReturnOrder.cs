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
using System.Collections;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;

namespace PX.Objects.CR.Extensions.CRCreateReturnOrder
{
	/// <summary>
	/// Extension that is used for creating return orders purposes.
	/// </summary>
	public abstract class CRCreateReturnOrder<TGraph, TMaster> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TMaster : class, IBqlTable, new()
	{
		public static bool IsExtensionActive() => PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

		#region ctor

		[InjectDependency]
		internal IPXPageIndexingService PageService { get; private set; }

		public override void Initialize()
		{
			base.Initialize();

			PopupValidator = CRPopupValidator.Create(CreateOrderParams);
		}

		#endregion

		#region Views

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.CreateSalesOrder)]
		public CRValidationFilter<CreateReturnOrderFilter> CreateOrderParams;

		public CRPopupValidator.Generic<CreateReturnOrderFilter> PopupValidator { get; private set; }

		#endregion

		#region Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXRestrictorAttribute))]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDefault(typeof(Search2<
			SOOrderType.orderType,
			InnerJoin<SOSetup,
				On<SOOrderType.orderType, Equal<SOSetup.defaultReturnOrderType>>>,
			Where<
				SOOrderType.active, Equal<boolTrue>>>))]
		[PXRestrictor(typeof(Where<SOOrderType.active, Equal<boolTrue>>), Messages.OrderTypeIsNotActive, typeof(SOOrderType.descr))]
		[PXRestrictor(typeof(
				Where<SOOrderType.behavior
					.IsEqual<SOBehavior.rM>>),
			Messages.OrderTypeOfWrongType)]
		public virtual void _(Events.CacheAttached<CreateReturnOrderFilter.orderType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		public virtual void _(Events.CacheAttached<CreateReturnOrderFilter.orderNbr> e) { }

		#endregion

		#region Actions

		public PXAction<TMaster> CreateReturnOrder;
		[PXUIField(DisplayName = "Create Return Order", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable createReturnOrder(PXAdapter adapter)
		{
			if (!CheckBAccountStateBeforeConvert())
				throw new PXException(MessagesNoPrefix.CannotCreateReturnOrderWhenAccountIsEmptyOrProspect);

			if (!HasAccessToCreateReturnOrder())
				throw new PXException(MessagesNoPrefix.NoAccessToCreateReturnOrder);

			if (PopupValidator.AskExt().IsPositive() && PopupValidator.TryValidate())
			{
				Base.Actions.PressSave();

				var graph = Base.CloneGraphState();

				PXLongOperation.StartOperation(Base, delegate ()
				{
					var extension = graph.GetProcessingExtension<CRCreateReturnOrder<TGraph, TMaster>>();

					extension.DoCreateReturnOrder();
				});
			}

			return adapter.Get();
		}

		public virtual void DoCreateReturnOrder()
		{
			CreateReturnOrderFilter filter = this.CreateOrderParams.Current;
			TMaster masterEntity = (TMaster)this.Base.Caches[typeof(TMaster)].Current;

			if (filter == null || masterEntity == null)
				return;

			SOOrderEntry soGraph = PXGraph.CreateInstance<SOOrderEntry>();

			DoCreateReturnOrder(soGraph, masterEntity, filter);

			if (!Base.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(soGraph, PXRedirectHelper.WindowMode.New);

			soGraph.Save.Press();
		}

		public virtual void DoCreateReturnOrder(SOOrderEntry soGraph, TMaster document, CreateReturnOrderFilter filter)
		{
			var salesOrder = new SOOrder();

			salesOrder.OrderType = CreateOrderParams.Current.OrderType ?? SOOrderTypeConstants.RMAOrder;

			if (!string.IsNullOrWhiteSpace(filter.OrderNbr))
			{
				salesOrder.OrderNbr = filter.OrderNbr;
			}

			salesOrder = soGraph.Document.Insert(salesOrder);
			salesOrder = PXCache<SOOrder>.CreateCopy(soGraph.Document.Search<SOOrder.orderNbr>(salesOrder.OrderNbr));

			salesOrder = FillSalesOrder(soGraph, document, salesOrder);

			FillRelations(soGraph, document, salesOrder);



			PXNoteAttribute.CopyNoteAndFiles(Base.Caches[typeof(TMaster)], document, soGraph.Document.Cache, salesOrder, Base.Caches[typeof(CRSetup)].Current as PXNoteAttribute.IPXCopySettings);

			UDFHelper.CopyAttributes(Base.Caches[typeof(TMaster)], document, soGraph.Document.Cache, soGraph.Document.Cache.Current, salesOrder.OrderType);



			salesOrder = soGraph.Document.Update(salesOrder);
		}

		#endregion

		#region Methods

		public virtual SOOrder FillSalesOrder(SOOrderEntry docgraph, TMaster document, SOOrder salesOrder)
		{
			return salesOrder;
		}

		public virtual CRRelation FillRelations(SOOrderEntry docgraph, TMaster document, SOOrder salesOrder)
		{
			return null;

		}

		public virtual bool CheckBAccountStateBeforeConvert()
		{
			return true;
		}

		public virtual bool HasAccessToCreateReturnOrder()
		{
			var graphType = typeof(SOOrderEntry);
			var node = PXSiteMap.Provider.FindSiteMapNode(graphType);
			if (node == null) return false;

			var primaryViewName = PageService.GetPrimaryView(graphType.FullName);
			var cacheInfo = GraphHelper.GetGraphView(graphType, primaryViewName).Cache;

			PXAccess.Provider.GetRights(node.ScreenID, graphType.Name, cacheInfo.CacheType,
				out var rights, out var _, out var _);

			return rights >= PXCacheRights.Insert;
		}

		#endregion
	}
}
