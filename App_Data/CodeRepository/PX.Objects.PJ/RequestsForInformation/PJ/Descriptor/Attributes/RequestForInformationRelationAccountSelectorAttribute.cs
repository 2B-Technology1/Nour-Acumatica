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
using System.Collections.Generic;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Services;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.PO;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public sealed class RequestForInformationRelationAccountSelectorAttribute : PXCustomSelectorAttribute,
        IPXFieldDefaultingSubscriber
    {
        private const string FilterViewName = "_BAccount_";
        private readonly ProjectContactFilterService projectContactFilterService;

        public RequestForInformationRelationAccountSelectorAttribute()
            : base(typeof(BAccount.bAccountID), GetSelectorFields())
        {
            SubstituteKey = typeof(BAccount.acctCD);
			DescriptionField = typeof(BAccount.acctName);
            Filterable = true;
            projectContactFilterService = new ProjectContactFilterService();
        }

        public void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs args)
        {
            if (args.Row is RequestForInformationRelation requestForInformationRelation)
            {
                args.NewValue = GetDefaultValue(requestForInformationRelation);
            }
        }

        public override void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
        {
            base.FieldSelecting(cache, args);
            if (args.ReturnState is PXFieldState fieldState
                && args.Row is RequestForInformationRelation requestForInformationRelation)
            {
                fieldState.Enabled = IsEnabled(requestForInformationRelation);
            }
        }

        public IEnumerable GetRecords()
        {
            return projectContactFilterService.UpdateAdditionalFields(_Graph, GetBusinessAccounts());
        }

        protected override void CreateFilter(PXGraph graph)
        {
            projectContactFilterService.CreateFilterView(graph, _ViewName, FilterViewName);
        }

        private IEnumerable<BAccount> GetBusinessAccounts()
        {
            var query = new PXSelect<BAccount,
                Where<
                    BAccount.type, In3<BAccountType.prospectType, BAccountType.customerType, BAccountType.combinedType>,
                        And<BAccount.status, NotEqual<CustomerStatus.inactive>,
                    Or<Where<BAccount.type, In3<BAccountType.vendorType, BAccountType.combinedType>,
                        And<BAccount.vStatus, NotEqual<VendorStatus.inactive>>>>>>>(_Graph);
            return query.Select().FirstTableItems;
        }

        private static bool IsEnabled(RequestForInformationRelation requestForInformationRelation)
        {
            return requestForInformationRelation.Type == RequestForInformationRelationTypeAttribute.Contact;
        }

        private object GetDefaultValue(RequestForInformationRelation requestForInformationRelation)
        {
            return requestForInformationRelation.DocumentNoteId != null
                ? GetBusinessAccountId(requestForInformationRelation)
                : null;
        }

        private int? GetBusinessAccountId(RequestForInformationRelation requestForInformationRelation)
        {
            switch (requestForInformationRelation.Type)
            {
                case RequestForInformationRelationTypeAttribute.PurchaseOrder:
                case RequestForInformationRelationTypeAttribute.Subcontract:
                    return GetCommitmentVendorId(requestForInformationRelation.DocumentNoteId);
                case RequestForInformationRelationTypeAttribute.ApInvoice:
                    return GetApInvoiceVendorId(requestForInformationRelation.DocumentNoteId);
                case RequestForInformationRelationTypeAttribute.ArInvoice:
                    return GetArInvoiceCustomerId(requestForInformationRelation.DocumentNoteId);
                case RequestForInformationRelationTypeAttribute.RequestForInformation:
                    return GetRequestForInformationBusinessAccountId(requestForInformationRelation.DocumentNoteId);
                default:
                    return null;
            }
        }

        private int? GetApInvoiceVendorId(Guid? noteId)
        {
            var query = new PXSelect<APInvoice,
                Where<APInvoice.noteID, Equal<Required<APInvoice.noteID>>>>(_Graph);
            return query.SelectSingle(noteId).VendorID;
        }

        private int? GetArInvoiceCustomerId(Guid? noteId)
        {
            var query = new PXSelect<ARInvoice,
                Where<ARInvoice.noteID, Equal<Required<ARInvoice.noteID>>>>(_Graph);
            return query.SelectSingle(noteId).CustomerID;
        }

        private int? GetCommitmentVendorId(Guid? noteId)
        {
            var query = new PXSelect<POOrder,
                Where<POOrder.noteID, Equal<Required<POOrder.noteID>>>>(_Graph);
            return query.SelectSingle(noteId).VendorID;
        }

        private int? GetRequestForInformationBusinessAccountId(Guid? noteId)
        {
            var query = new PXSelect<RequestForInformation,
                Where<RequestForInformation.noteID, Equal<Required<RequestForInformation.noteID>>>>(_Graph);
            return query.SelectSingle(noteId).BusinessAccountId;
        }

        private static Type[] GetSelectorFields()
        {
            return new[]
            {
                typeof(BAccount.acctCD),
                typeof(BAccount.acctName),
                typeof(BAccount.classID),
                typeof(BAccount.type),
                typeof(BAccount.parentBAccountID),
                typeof(BAccount.acctReferenceNbr)
            };
        }
    }
}
