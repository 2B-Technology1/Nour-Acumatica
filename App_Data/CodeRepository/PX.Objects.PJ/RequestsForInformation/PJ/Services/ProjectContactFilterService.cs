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
using PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PM.DAC;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Services
{
    public class ProjectContactFilterService
    {
        public void CreateFilterView(PXGraph graph, string viewName, string filterViewName)
        {
            var graphName = GetGraphName(graph.GetType());
            var siteMapNode = PXSiteMap.Provider.FindSiteMapNodeByGraphType(graphName);
            if (siteMapNode != null)
            {
                var filterView = new PXFilterView(graph, siteMapNode.ScreenID, filterViewName);
                PXFilterableAttribute.AddFilterView(graph, filterView, viewName);
                var filterDetailView = new PXFilterDetailView(graph, viewName);
                PXFilterableAttribute.AddFilterDetailView(graph, filterDetailView, viewName);
            }
        }

        public IEnumerable<BAccount> UpdateAdditionalFields(PXGraph graph, IEnumerable<BAccount> businessAccounts)
        {
            var projectContacts = GetProjectContacts(graph);
            foreach (var businessAccount in businessAccounts)
            {
                UpdateAdditionalFields(businessAccount, projectContacts);
                yield return businessAccount;
            }
        }

        public IEnumerable<Contact> UpdateAdditionalFields(PXGraph graph, IEnumerable<Contact> contacts)
        {
            var projectContacts = GetProjectContacts(graph);
            foreach (var contact in contacts)
            {
                UpdateAdditionalFields(contact, projectContacts);
                yield return contact;
            }
        }

        private void UpdateAdditionalFields(Contact contact, IEnumerable<ProjectContact> projectContacts)
        {
            var extension = PXCache<Contact>.GetExtension<ContactExt>(contact);
            extension.IsRelatedToProjectContact = projectContacts.Any(x => x.ContactId == contact.ContactID);
        }

        private void UpdateAdditionalFields(BAccount businessAccount, IEnumerable<ProjectContact> projectContacts)
        {
            var extension = PXCache<BAccount>.GetExtension<BAccountExt>(businessAccount);
            extension.IsRelatedToProjectContact =
                projectContacts.Any(x => x.BusinessAccountId == businessAccount.BAccountID);
        }

        private List<ProjectContact> GetProjectContacts(PXGraph graph)
        {
            var query = new PXSelect<ProjectContact,
                Where<ProjectContact.projectId, Equal<Current<RequestForInformation.projectId>>>>(graph);
            return query.Select().FirstTableItems.ToList();
        }

        private static string GetGraphName(Type graphType)
        {
            return graphType.FullName != null &&
                graphType.FullName.Contains(Constants.Wrapper) &&
                graphType.FullName.Contains(Constants.CstPrefix)
                    ? graphType.BaseType?.FullName
                    : graphType.FullName;
        }
    }
}