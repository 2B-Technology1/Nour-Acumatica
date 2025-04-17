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

using System.Linq;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Common;
using System;
using static PX.Data.BQL.BqlPlaceholder;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public class RequestForInformationRelationTypeAttribute : PXEventSubscriberAttribute,
		IPXFieldSelectingSubscriber, IPXFieldDefaultingSubscriber, IPXFieldUpdatedSubscriber, IPXFieldVerifyingSubscriber
	{
        public const string Contact = "Contact";
        public const string Project = "Project";
        public const string ProjectTask = "Project Task";
        public const string PurchaseOrder = "Purchase Order";
        public const string Subcontract = "Subcontract";
        public const string ApInvoice = "AP Invoice";
        public const string ArInvoice = "AR Invoice";
        public const string RequestForInformation = "RFI";

		public const string ContactLabel = "Contact";
		public const string ProjectLabel = "Project";
		public const string ProjectTaskLabel = "Project Task";
		public const string PurchaseOrderLabel = "Purchase Order";
		public const string SubcontractLabel = "Subcontract";
		public const string ApInvoiceLabel = "Bill";
		public const string ArInvoiceLabel = "AR Invoice";
		public const string RequestForInformationLabel = "RFI";

		public void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs args)
        {
            if (args.Row is RequestForInformationRelation requestForInformationRelation
                && requestForInformationRelation.Role != null)
            {
                var types = GetRoleTypes(requestForInformationRelation);

				if (types.Length == 0)
					return;

                args.NewValue = types.First();
            }
        }

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as RequestForInformationRelation;

			if (row != null)
			{
				if (row.Type != null)
				{
					if (TypesForRelatedEntityRole.Contains(row.Type))
					{
						e.ReturnState = GetFieldState(e.ReturnState, TypesForRelatedEntityRole, TypeLabelsForRelatedEntityRole, true);
					}
					else
					{
						e.ReturnState = GetFieldState(e.ReturnState, TypesForOtherRoles, TypeLabelsForOtherRoles, false);
					}
				}
				else
				{
					e.ReturnState = GetFieldState(e.ReturnState, AllTypes, AllTypeLabels, false);
				}
			}
		}

		public void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as RequestForInformationRelation;

			if (row != null)
			{
				if (row.Role != null && e.NewValue != null)
				{
					var availableRoleTypes = GetRoleTypes(row);

					if (!availableRoleTypes.Contains(e.NewValue))
					{
						e.NewValue = availableRoleTypes.FirstOrDefault();
					}
				}
			}
		}

		public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (cache.Graph.Views.ContainsKey(nameof(RequestForInformationRelationDocumentSelectorAttribute)))
            {
                cache.Graph.Views[nameof(RequestForInformationRelationDocumentSelectorAttribute)].RequestRefresh();
            }
        }

		private PXFieldState GetFieldState(object returnState, string[] allowedTypes, string[] allowedTypeLabels, bool enabled)
		{
			var fieldState = CreateFieldState(returnState, allowedTypes, allowedTypeLabels);
			fieldState.Enabled = enabled;
			return fieldState;
		}

		private PXFieldState CreateFieldState(object originalFieldState, string[] allowedTypes, string[] allowedTypeLabels)
		{
			return PXStringState.CreateInstance(originalFieldState,
				null, null, _FieldName, null, -1, null, allowedTypes, allowedTypeLabels, true, null);
		}

		private static string[] GetRoleTypes(RequestForInformationRelation requestForInformationRelation)
        {
            switch (requestForInformationRelation.Role)
            {
                case RequestForInformationRoleListAttribute.RelatedEntity:
                    return TypesForRelatedEntityRole;
                default:
                    return TypesForOtherRoles;
            }
        }

		private string[] AllTypes { get; } = TypesForRelatedEntityRole.Concat(TypesForOtherRoles).ToArray();

		private string[] AllTypeLabels { get; } = TypeLabelsForRelatedEntityRole.Concat(TypeLabelsForOtherRoles).ToArray();

		private static string[] TypesForRelatedEntityRole { get; } = new[]
			{
				Project,
				ProjectTask,
				PurchaseOrder,
				Subcontract,
				ApInvoice,
				ArInvoice,
				RequestForInformation
			};

		private static string[] TypeLabelsForRelatedEntityRole { get; } = new[]
			{
				ProjectLabel,
				ProjectTaskLabel,
				PurchaseOrderLabel,
				SubcontractLabel,
				ApInvoiceLabel,
				ArInvoiceLabel,
				RequestForInformationLabel
			};

		private static string[] TypesForOtherRoles { get; } = new[] { Contact };

		private static string[] TypeLabelsForOtherRoles { get; } = new[] { ContactLabel };

		public sealed class apInvoice : BqlString.Constant<apInvoice>
        {
            public apInvoice()
                : base(ApInvoice)
            {
            }
        }

        public sealed class arInvoice : BqlString.Constant<arInvoice>
        {
            public arInvoice()
                : base(ArInvoice)
            {
            }
        }

        public sealed class requestForInformation : BqlString.Constant<requestForInformation>
        {
            public requestForInformation()
                : base(RequestForInformation)
            {
            }
        }
    }
}
