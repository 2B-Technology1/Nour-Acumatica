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
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
    [Serializable]
    [PXPrimaryGraph(typeof(ProjectManagementClassMaint))]
    [PXCacheName(CacheNames.ProjectManagementClassPriority)]
    public class ProjectManagementClassPriority : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public int? PriorityId
        {
            get;
            set;
        }

        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Search<ProjectManagementClass.projectManagementClassId,
            Where<ProjectManagementClass.projectManagementClassId,
                Equal<Current<ProjectManagementClass.projectManagementClassId>>>>))]
        public string ClassId
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public bool? IsActive
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Priority Name",
            Visibility = PXUIVisibility.SelectorVisible)]
        public string PriorityName
        {
            get;
            set;
        }

        [PXDBInt(MinValue = 1)]
        [PXUIField(DisplayName = "Sort Order")]
        public int? SortOrder
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Default")]
        public bool? IsDefault
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        public bool? IsSystemPriority
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        public bool? IsHighestPriority
        {
            get;
            set;
        }

        public abstract class priorityId : BqlInt.Field<priorityId>
        {
        }

        public abstract class classId : BqlString.Field<classId>
        {
        }

        public abstract class isActive : BqlBool.Field<isActive>
        {
        }

        public abstract class priorityName : BqlString.Field<priorityName>
        {
        }

        public abstract class sortOrder : BqlInt.Field<sortOrder>
        {
        }

        public abstract class isDefault : BqlBool.Field<isDefault>
        {
        }

        public abstract class isSystemPriority : BqlBool.Field<isSystemPriority>
        {
        }

        public abstract class isHighestPriority : BqlBool.Field<isHighestPriority>
        {
        }
    }
}