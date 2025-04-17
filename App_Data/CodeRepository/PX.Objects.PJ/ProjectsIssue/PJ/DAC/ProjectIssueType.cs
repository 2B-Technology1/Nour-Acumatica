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

using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PJ.ProjectsIssue.PJ.DAC
{
    [PXCacheName("Project Issue Type")]
    public class ProjectIssueType : IBqlTable
    {

        #region Keys

        /// <summary>
        /// Primary Key
        /// </summary>
        public class PK : PrimaryKeyOf<ProjectIssueType>.By<projectIssueTypeId>
        {
            public static ProjectIssueType Find(PXGraph graph, int? projectIssueTypeId, PKFindOptions options = PKFindOptions.None) => FindBy(graph, projectIssueTypeId, options);
        }

        #endregion

        [PXDBIdentity(IsKey = true)]
        public virtual int? ProjectIssueTypeId
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault]
        [Unique(ErrorMessage = ProjectManagementMessages.ProjectIssueTypeUniqueConstraint)]
        [PXUIField(DisplayName = "Project Issue Type", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string TypeName
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Description
        {
            get;
            set;
        }

        public abstract class projectIssueTypeId : BqlInt.Field<projectIssueTypeId>
        {
        }

        public abstract class typeName : BqlString.Field<typeName>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }
    }
}