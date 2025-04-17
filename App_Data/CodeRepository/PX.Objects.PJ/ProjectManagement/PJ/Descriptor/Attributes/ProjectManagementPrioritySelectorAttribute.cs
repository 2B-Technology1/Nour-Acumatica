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
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes
{
    [PXRestrictor(typeof(Where<ProjectManagementClassPriority.isActive, Equal<True>>),
        ProjectManagementMessages.OnlyActivePrioritiesAreAllowed)]
    public sealed class ProjectManagementPrioritySelectorAttribute : PXEntityAttribute
    {
        public ProjectManagementPrioritySelectorAttribute(Type classIdField)
        {
            var searchType = BqlCommand.Compose(
                typeof(Search<,,>),
                typeof(ProjectManagementClassPriority.priorityId),
                typeof(Where<,>),
                typeof(ProjectManagementClassPriority.classId),
                typeof(Equal<>),
                typeof(Current<>),
                classIdField,
                typeof(OrderBy<Asc<ProjectManagementClassPriority.sortOrder>>));
            CreateSelectorAttribute(searchType);
        }

        public ProjectManagementPrioritySelectorAttribute()
        {
            var searchType = typeof(Search3<ProjectManagementClassPriority.priorityId,
                OrderBy<Asc<ProjectManagementClassPriority.sortOrder>>>);
            CreateSelectorAttribute(searchType);
        }

        private void CreateSelectorAttribute(Type searchType)
        {
            var selectorAttribute = new PXSelectorAttribute(searchType)
            {
                DescriptionField = typeof(ProjectManagementClassPriority.priorityName),
                SubstituteKey = typeof(ProjectManagementClassPriority.priorityName)
            };
            _Attributes.Add(selectorAttribute);
        }
    }
}
