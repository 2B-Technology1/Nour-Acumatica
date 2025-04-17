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
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CS;

namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
	/// <summary>
	/// Represents a project management class.
	/// Project management classes provide the default settings for <see cref="ProjectIssue">project issues</see>
	/// and <see cref="RequestForInformation">requests for information</see>.
	/// The records of this type are created and edited through the Project Management Classes (PJ201000) form
	/// (which corresponds to the <see cref="ProjectManagementClassMaint"/> graph).
	/// </summary>
    [Serializable]
    [PXPrimaryGraph(typeof(ProjectManagementClassMaint))]
    [PXCacheName(CacheNames.ProjectManagementClass)]
    public class ProjectManagementClass : BaseCache, IBqlTable
    {
		/// <summary>
		/// The identifier of the project management class.
		/// </summary>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Project Management Class ID",
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(projectManagementClassId),
            typeof(projectManagementClassId),
            typeof(description),
            typeof(useForProjectIssue),
            typeof(useForRequestForInformation),
            DescriptionField = typeof(description))]
        [CascadeDelete(typeof(ProjectManagementClassPriority), typeof(ProjectManagementClassPriority.classId))]
        public virtual string ProjectManagementClassId
        {
            get;
            set;
        }

		/// <summary>
		/// The description of the project management class.
		/// </summary>
		[PXDBLocalizableString(255, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Description",
            Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Description
        {
            get;
            set;
        }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the project management class is internal.
		/// </summary>
		[PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Internal")]
        public virtual bool? IsInternal
        {
            get;
            set;
        }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the project management class is available for selection in a project issue.
		/// </summary>
		[PXDBBool]
        [PXUIField(DisplayName = "Project Issues",
            Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? UseForProjectIssue
        {
            get;
            set;
        }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the project management class is available for selection in a request for information.
		/// </summary>
		[PXDBBool]
        [PXUIField(DisplayName = "Requests For Information",
            Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? UseForRequestForInformation
        {
            get;
            set;
        }

		/// <summary>
		/// The response time frame of the request for information that is related a project management class.
		/// </summary>
		[PXDBInt(MinValue = 0)]
        [PXDefault(typeof(IIf<useForRequestForInformation.IsEqual<True>,
            int5, Null>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(useForRequestForInformation.IsEqual<True>))]
        [PXFormula(typeof(Default<useForRequestForInformation>))]
        [PXUIField(DisplayName = "Answer Days Default")]
        public virtual int? RequestForInformationResponseTimeFrame
        {
            get;
            set;
        }

		/// <summary>
		/// The response time frame of a project issue that is related to a project management class.
		/// </summary>
		[PXDBInt(MinValue = 0)]
        [PXDefault(typeof(IIf<useForProjectIssue.IsEqual<True>,
            int5, Null>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(useForProjectIssue.IsEqual<True>))]
        [PXFormula(typeof(Default<useForProjectIssue>))]
        [PXUIField(DisplayName = "Answer Days Default")]
        public virtual int? ProjectIssueResponseTimeFrame
        {
            get;
            set;
        }

		/// <inheritdoc cref="RequestForInformationResponseTimeFrame"/>
		public abstract class requestForInformationResponseTimeFrame : BqlInt.Field<requestForInformationResponseTimeFrame>
        {
        }

		/// <inheritdoc cref="ProjectIssueResponseTimeFrame"/>
		public abstract class projectIssueResponseTimeFrame : BqlInt.Field<projectIssueResponseTimeFrame>
        {
        }

		/// <inheritdoc cref="IsInternal"/>
		public abstract class isInternal : BqlBool.Field<isInternal>
        {
        }

		/// <inheritdoc cref="Description"/>
		public abstract class description : BqlString.Field<description>
        {
        }

		/// <inheritdoc cref="UseForProjectIssue"/>
		public abstract class useForProjectIssue : BqlBool.Field<useForProjectIssue>
        {
        }

		/// <inheritdoc cref="UseForRequestForInformation"/>
		public abstract class useForRequestForInformation : BqlBool.Field<useForRequestForInformation>
        {
        }

		/// <inheritdoc cref="ProjectManagementClassId"/>
		public abstract class projectManagementClassId : BqlString.Field<projectManagementClassId>
        {
        }
    }
}
