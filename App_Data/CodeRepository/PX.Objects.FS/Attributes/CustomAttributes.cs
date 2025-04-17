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
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PX.Objects.CT;
using PX.Data.BQL;

namespace PX.Objects.FS
{
    [PXDBInt]
    [PXUIField(DisplayName = "Service ID", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<
            InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
        And<
            InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>),
        PX.Objects.IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus))]
    [PXRestrictor(typeof(Where<InventoryItem.isTemplate, Equal<False>>), IN.Messages.InventoryItemIsATemplate, ShowWarning = true)]
    public class FSInventoryAttribute : CrossItemAttribute
    {
        public FSInventoryAttribute(Type searchType, Type substituteKey, Type descriptionField, Type[] listField)
            : base(searchType, substituteKey, descriptionField, INPrimaryAlternateType.CPN)
        {            
        }

        public FSInventoryAttribute(Type searchType, Type substituteKey, Type descriptionField)
            : base(searchType, substituteKey, descriptionField, INPrimaryAlternateType.CPN)
        {
        }

        public FSInventoryAttribute()
            : this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
        {
        }
    }

    public class FSAttributeGroupList<TClass, TEntity1, TEntity2, TEntity3> : 
                 CSAttributeGroupList<TClass, TEntity1>, IPXRowInsertedSubscriber, IPXRowDeletedSubscriber, IPXRowUpdatedSubscriber
        where TClass : class
    {
        public FSAttributeGroupList(PXGraph graph) 
            : base(graph)
        {
            graph.RowInserted.AddHandler<CSAttributeGroup>(RowInserted);
            graph.RowUpdated.AddHandler<CSAttributeGroup>(RowUpdated);
            graph.RowDeleted.AddHandler<CSAttributeGroup>(RowDeleted);
        }

        public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            CacheEventHandler(sender, e.Row, PXDBOperation.Update, e.ExternalCall);
        }

        public void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            CacheEventHandler(sender, e.Row, PXDBOperation.Delete, e.ExternalCall);
        }

        public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            CacheEventHandler(sender, e.Row, PXDBOperation.Insert, e.ExternalCall);
        }

        public void CacheEventHandler(PXCache sender, object row, PXDBOperation operation, bool externalCall)
        {
            if (row == null)
            {
                return;
            }

            var entity1 = (CSAttributeGroup)row;

            if (entity1.EntityType != typeof(TEntity1).FullName)
            {
                return;
            }

            var entity2 = (CSAttributeGroup)sender.CreateCopy(entity1);
            entity2.EntityType = typeof(TEntity2).FullName;
            UpdateCacheRecord(sender, entity2, operation);

            var entity3 = (CSAttributeGroup)sender.CreateCopy(entity1);
            entity3.EntityType = typeof(TEntity3).FullName;
            UpdateCacheRecord(sender, entity3, operation);
        }

        public void UpdateCacheRecord(PXCache sender, CSAttributeGroup cSAttributeGroup, PXDBOperation operation)
        {
            if (operation == PXDBOperation.Insert || operation == PXDBOperation.Update)
            {
                sender.Update(cSAttributeGroup);
            }
            else if (operation == PXDBOperation.Delete)
            {
                sender.Delete(cSAttributeGroup);
            }
        }
    }

    public class FSAttributeList<TEntity> : CRAttributeList<TEntity>
    {
        public FSAttributeList(PXGraph graph): base(graph)
        {
        }

		protected override string GetSelectInternalExceptionMessage()
		{
			return TX.Error.AttributeNotValid;
		}
    }

    #region ViewLinkedDoc

    public class ViewLinkedDoc<TNode, TDetail> : PXAction<TNode>
        where TNode : class, IBqlTable, new()
        where TDetail : class, IBqlTable, IFSSODetBase, new()
    {
        public ViewLinkedDoc(PXGraph graph, string name)
            : base(graph, name)
        {
        }

        [PXUIField(DisplayName = "View Related Doc.", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        protected override System.Collections.IEnumerable Handler(PXAdapter adapter)
        {
            PXCache cache = adapter.View.Cache;

            if (adapter.View.Cache.IsDirty == true)
            { 
                adapter.View.Graph.GetSaveAction().Press();
            }

            foreach (object ret in adapter.Get())
            {
                TNode item;

                if (ret is PXResult)
                {
                    item = (TNode)((PXResult)ret)[0];
                }
                else
                {
                    item = (TNode)ret;
                }

                if (item != null)
                {
                    PXCache detailCache = adapter.View.Graph.Caches[typeof(TDetail)];
                    TDetail detail = (TDetail)detailCache.Current;
                    if (detail != null) 
                    { 
                        if (detail.LinkedEntityType == FSAppointmentDet.linkedEntityType.Values.SalesOrder)
                        {
                            SOOrderEntry graph = PXGraph.CreateInstance<SOOrderEntry>();
                            graph.Document.Current = graph.Document.Search<SOOrder.orderNbr>(detail.LinkedDocRefNbr, detail.LinkedDocType);

                            throw new PXRedirectRequiredException(graph, true, TX.Linked_Entity_Type.SalesOrder) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }
                        else if (detail.IsExpenseReceiptItem == true)
                        {
                            ExpenseClaimDetailEntry graph = PXGraph.CreateInstance<ExpenseClaimDetailEntry>();
                            graph.ClaimDetails.Current = graph.ClaimDetails.Search<EPExpenseClaimDetails.claimDetailCD>(detail.LinkedDocRefNbr);

                            throw new PXRedirectRequiredException(graph, true, TX.Linked_Entity_Type.ExpenseReceipt) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }
                        else if (detail.IsAPBillItem == true)
                        {
                            APInvoiceEntry graph = PXGraph.CreateInstance<APInvoiceEntry>();
                            graph.Document.Current = graph.Document.Search<APInvoice.refNbr>(detail.LinkedDocRefNbr, detail.LinkedDocType);

                            throw new PXRedirectRequiredException(graph, true, TX.Linked_Entity_Type.APBill) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }
                    }
                }

                yield return ret;
            }
        }
    }
    #endregion

    #region ViewPostBatch

    public class ViewPostBatch<TNode> : PXAction<TNode>
        where TNode : class, IBqlTable, new()
    {
        public ViewPostBatch(PXGraph graph, string name)
            : base(graph, name)
        {
        }

        [PXUIField(DisplayName = "View Post Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        protected override System.Collections.IEnumerable Handler(PXAdapter adapter)
        {
            PXCache cache = adapter.View.Cache;

            if (adapter.View.Cache.IsDirty == true)
            {
                adapter.View.Graph.GetSaveAction().Press();
            }

            foreach (object ret in adapter.Get())
            {
                TNode item;

                if (ret is PXResult)
                {
                    item = (TNode)((PXResult)ret)[0];
                }
                else
                {
                    item = (TNode)ret;
                }

                if (item != null)
                {
                    PXCache detailCache = adapter.View.Graph.Caches[typeof(FSBillHistory)];
                    FSBillHistory detail = (FSBillHistory)detailCache.Current;

                    if (detail != null)
                    {
                        if (string.IsNullOrEmpty(detail.ServiceContractRefNbr))
                        {
                            PostBatchMaint graph = PXGraph.CreateInstance<PostBatchMaint>();
                            graph.BatchRecords.Current = graph.BatchRecords.Search<FSPostBatch.batchID>(detail.BatchID);

                            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }
                        else
                        {
                            ContractPostBatchMaint graph = PXGraph.CreateInstance<ContractPostBatchMaint>();
                            graph.ContractBatchRecords.Current = graph.ContractBatchRecords.Search<FSContractPostBatch.contractPostBatchID>(detail.BatchID);

                            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                        }
                        
                    }
                }

                yield return ret;
            }
        }
    }
    #endregion

    public class FSEntityIDSelectorAttribute : EntityIDSelectorAttribute
    {
        private const string _DESCRIPTION_FIELD_POSTFIX = "_Description";

        private readonly Type _typeBqlField;
        private string _typeField;
        private string _descriptionFieldName;

        public FSEntityIDSelectorAttribute(Type typeBqlField)
            : base(typeBqlField)
        {
            _typeBqlField = typeBqlField;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            _typeField = sender.GetField(_typeBqlField);
            _descriptionFieldName = _FieldName + _DESCRIPTION_FIELD_POSTFIX;
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            //if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) //NOTE: for different items it's different
            {
                var itemType = (e.Row ?? sender.Current).
                     With(row => sender.GetValue(row, _typeField) as string).
                     With(typeName => System.Web.Compilation.PXBuildManager.GetType(typeName, false));
                if (itemType != null)
                {
                    var graph = sender.Graph;
                    var itemCache = graph.Caches[itemType];
                    var noteAtt = EntityHelper.GetNoteAttribute(itemType);
                    string viewName = null;
                    string[] fieldList = null;
                    string[] headerList = null;

                    CreateSelectorView(graph, itemType, noteAtt, out viewName, out fieldList, out headerList);

                    if (noteAtt.FieldList != null && noteAtt.FieldList.Length > 0)
                    {
                        fieldList = new string[noteAtt.FieldList.Length];
                        for (int i = 0; i < noteAtt.FieldList.Length; i++)
                        {
                            fieldList[i] = noteAtt.FieldList[i].Name;
                        }
                        headerList = null;
                    }

                    if (fieldList == null)
                        fieldList = new EntityHelper(graph).GetFieldList(itemType);
                    if (headerList == null)
                        headerList = GetFieldDisplayNames(itemCache, fieldList);

                    var keys = itemCache.Keys.ToArray();
                    var valueField = EntityHelper.GetNoteField(itemType);
                    var textField = noteAtt.DescriptionField.With(df => df.Name) ?? keys.Last();
                    var fieldState = PXFieldState.CreateInstance(e.ReturnState, null, null, null,
                              null, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null,
                              PXUIVisibility.Undefined, viewName, fieldList, headerList);
                    fieldState.ValueField = valueField;
                    fieldState.DescriptionName = textField;

                    // for cb it should be guid
                    if (!sender.Graph.IsContractBasedAPI)
                    {
                        e.ReturnState = fieldState;
                    }
                }
                else
                {
                    e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null,
                              null, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, e.Row == null, null, null,
                              PXUIVisibility.Undefined, sender.Graph.PrimaryView, null, null);
                    ((PXFieldState)e.ReturnState).ValueField = "noteID";
                    ((PXFieldState)e.ReturnState).SelectorMode = PXSelectorMode.NoAutocomplete;
                    ((PXFieldState)e.ReturnState).DescriptionName = _descriptionFieldName;
                }
            }
        }

        public string[] GetFieldDisplayNames(PXCache itemCache, string[] fieldList)
        {
            var res = new string[fieldList.Length];
            for (int i = 0; i < fieldList.Length; i++)
            {
                var field = fieldList[i];
                var fs = itemCache.GetStateExt(null, field) as PXFieldState;
                if (fs != null && !string.IsNullOrEmpty(fs.DisplayName))
                    res[i] = fs.DisplayName;
                else res[i] = field;
            }
            return res;
        }
    }

    public class FSEntityIDExpenseSelectorAttribute : FSEntityIDSelectorAttribute
    {
        private readonly Type baccountBqlField;
        private readonly Type projectBqlField;
		private readonly Type customerLocationIDBqlField;
        

		public FSEntityIDExpenseSelectorAttribute(Type typeBqlField, Type baccountBqlField, Type projectBqlField, Type customerLocationIDBqlField)
              : base(typeBqlField)
        {
            this.baccountBqlField = baccountBqlField;
            this.projectBqlField = projectBqlField;
			this.customerLocationIDBqlField = customerLocationIDBqlField;
        }

        protected override void CreateSelectorView(PXGraph graph, Type itemType, PXNoteAttribute noteAtt, out string viewName, out string[] fieldList, out string[] headerList)
        {
            Type search = null;
            if (itemType == typeof(FSServiceOrder))
                search =
					BqlCommand.Compose(
						typeof(Search2<,,>),
							typeof(FSServiceOrder.refNbr),
							typeof(LeftJoin<,>),
								typeof(Contract),
								typeof(On<,>),
									typeof(Contract.contractID), typeof(Equal<>), typeof(Current<>), projectBqlField,
							typeof(Where<,,>),
								typeof(PMProject.restrictProjectSelect), typeof(Equal<>), typeof(PMRestrictOption.allProjects),
								typeof(Or<,,>),
									typeof(FSServiceOrder.billCustomerID), typeof(Equal<>), typeof(Current<>), baccountBqlField,
									typeof(And<,,>), typeof(FSServiceOrder.projectID), typeof(Equal<>), typeof(Current<>), projectBqlField,
									typeof(And<,,>), typeof(Current<>), baccountBqlField, typeof(IsNotNull),
								typeof(Or<>),
									typeof(Where<,,>), typeof(Current<>), baccountBqlField, typeof(IsNull),
										typeof(And<,,>), typeof(FSServiceOrder.projectID), typeof(Equal<>), typeof(Current<>), projectBqlField,
										typeof(And<,>), typeof(Contract.nonProject), typeof(Equal<>), typeof(True));

			if (itemType == typeof(FSAppointment))
                search =
					BqlCommand.Compose(
						typeof(Search2<,,>),
							typeof(FSAppointment.refNbr),
							typeof(LeftJoin<,,>),
								typeof(FSServiceOrder),
									typeof(On<,,>),
										typeof(FSServiceOrder.srvOrdType), typeof(Equal<>), typeof(FSAppointment.srvOrdType),
										typeof(And<,>), typeof(FSServiceOrder.refNbr), typeof(Equal<>), typeof(FSAppointment.soRefNbr),
							typeof(LeftJoin<,>),
								typeof(Contract),
									typeof(On<,>),
										typeof(Contract.contractID), typeof(Equal<>), typeof(Current<>), projectBqlField,
							typeof(Where<,,>),
								typeof(PMProject.restrictProjectSelect), typeof(Equal<>), typeof(PMRestrictOption.allProjects),
								typeof(Or<,,>),
									typeof(FSServiceOrder.billCustomerID), typeof(Equal<>), typeof(Current<>), baccountBqlField,
									typeof(And<,,>), typeof(FSAppointment.projectID), typeof(Equal<>), typeof(Current<>), projectBqlField,
									typeof(And<,,>), typeof(Current<>), baccountBqlField, typeof(IsNotNull),
							typeof(Or<>),
								typeof(Where<,,>), typeof(FSAppointment.projectID), typeof(Equal<>), typeof(Current<>), projectBqlField,
									typeof(And<,,>), typeof(Current<>), baccountBqlField, typeof(IsNull),
									typeof(And<,>), typeof(Contract.nonProject), typeof(Equal<>), typeof(True));

			if (search != null)
            {
                viewName = AddSelectorView(graph, search);
                PXFieldState state = AddFieldView(graph, search.GenericTypeArguments[0]);
                fieldList = null;
                headerList = null;
            }
            else
            {
                base.CreateSelectorView(graph, itemType, noteAtt, out viewName, out fieldList, out headerList);
            }
        }
    }

    public class FSEntityIDAPInvoiceSelectorAttribute : FSEntityIDSelectorAttribute
    {
        public FSEntityIDAPInvoiceSelectorAttribute(Type typeBqlField)
              : base(typeBqlField)
        {
        }

        public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (sender.Graph.Accessinfo.ScreenID != null && sender.Graph.Accessinfo.ScreenID.Substring(0, 2) == "FS")
            {
                return;
            }

            base.FieldUpdating(sender, e);
        }

        protected override void CreateSelectorView(PXGraph graph, Type itemType, PXNoteAttribute noteAtt, out string viewName, out string[] fieldList, out string[] headerList)
        {
            Type search = null;
            if (itemType == typeof(FSServiceOrder))
                search =
                    BqlCommand.Compose(typeof(Search<,>), typeof(FSServiceOrder.refNbr),
                        typeof(Where<,,>),
                            typeof(FSServiceOrder.quote), typeof(Equal<False>),
                        typeof(And<,,>),
                            typeof(FSServiceOrder.hold), typeof(Equal<False>),
                        typeof(And<,>),
                            typeof(FSServiceOrder.canceled), typeof(Equal<False>));

            if (itemType == typeof(FSAppointment))
                search =
                    BqlCommand.Compose(typeof(Search<,>), typeof(FSAppointment.refNbr),
                        typeof(Where<,,>),
                            typeof(FSAppointment.hold), typeof(Equal<False>),
                        typeof(And<,>),
                            typeof(FSAppointment.canceled), typeof(Equal<False>));

            if (search != null)
            {
                viewName = AddSelectorView(graph, search);
                PXFieldState state = AddFieldView(graph, search.GenericTypeArguments[0]);
                fieldList = null;
                headerList = null;
            }
            else
            {
                base.CreateSelectorView(graph, itemType, noteAtt, out viewName, out fieldList, out headerList);
            }
        }
    }
}
