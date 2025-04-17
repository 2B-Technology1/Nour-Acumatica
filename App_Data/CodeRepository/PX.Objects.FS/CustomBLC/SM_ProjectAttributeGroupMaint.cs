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
using PX.Objects.CS;
using PX.Objects.PM;
using System;

namespace PX.Objects.FS
{
    #region Graph
    public class SM_ProjectAttributeGroupMaint : PXGraphExtension<ProjectAttributeGroupMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>()
                || PXAccess.FeatureInstalled<FeaturesSet.routeManagementModule>();
        }

        [PXOverride]
        public string getEntityName(string classid)
        {
            switch (classid)
            {
                case PX.Objects.FS.GroupTypes.ListAttribute.ServiceContract:
                    return typeof(FSServiceContract).FullName;
                default:
                    return ProjectAttributeGroupMaint.getEntityNameStatic(classid);
            }
        }
    }
    #endregion
    #region Messages
    [PXLocalizable]
    public class Attributes_Messages
    {
        public const string GroupTypes_ServiceContract = "Service Contract";
    }
    #endregion
    #region Cache Extension
    public static class GroupTypes
    {
        public class ListAttribute : PM.GroupTypes.ListAttribute
        {

            public const string ServiceContract = "Service Contract";

            public ListAttribute() : base()
            {
                Array.Resize(ref _AllowedValues, _AllowedValues.Length + 1);
                _AllowedValues[_AllowedValues.Length - 1] = ServiceContract;
                Array.Resize(ref _AllowedLabels, _AllowedLabels.Length + 1);
                _AllowedLabels[_AllowedLabels.Length - 1] = Attributes_Messages.GroupTypes_ServiceContract;
            }

            public class ServiceContractType : PX.Data.BQL.BqlString.Constant<ServiceContractType>
            {
                public ServiceContractType() : base(GroupTypes.ListAttribute.ServiceContract) { ;}
            }
        }
    }

    public class FSxGroupTypeFilter : PXCacheExtension<PM.ProjectAttributeGroupMaint.GroupTypeFilter>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>()
                || PXAccess.FeatureInstalled<FeaturesSet.routeManagementModule>();
        }

        [PXRemoveBaseAttribute(typeof(PM.GroupTypes.ListAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [GroupTypes.List()]
        public String ClassID { get; set; }
    }
    #endregion
}