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

using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.ProjectManagement.PM.CacheExtensions
{
    public sealed class PmChangeRequestExtension : PXCacheExtension<PMChangeRequest>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();

		}

		public abstract class convertedFrom : BqlString.Field<convertedFrom> { }
	
		/// <summary>
		/// The type of the document from which the change request was created.
		/// </summary>
		[PXDBString(50)]
        public string ConvertedFrom
        {
            get;
            set;
        }

        public abstract class rfiID : PX.Data.BQL.BqlInt.Field<rfiID> { }

		/// <summary>
		/// The identifier of the <see cref="RequestForInformation">request for information</see> that is associated with the change request.
		/// </summary>
        [PXDBInt]
        [PXUIField(DisplayName = "RFI", FieldClass = nameof(FeaturesSet.ConstructionProjectManagement))]
        [PXSelector(typeof(SearchFor<RequestForInformation.requestForInformationId>
                .Where<RequestForInformation.projectId.IsEqual<PMChangeRequest.projectID.FromCurrent>>),
            SubstituteKey = typeof(RequestForInformation.requestForInformationCd))]
        [PXFormula(typeof(Default<PMChangeRequest.projectID>))]
        public int? RFIID
        {
            get;
            set;
        }
        
        public abstract class projectIssueID : PX.Data.BQL.BqlInt.Field<projectIssueID> { }

		/// <summary>
		/// The identifier of the <see cref="ProjectIssue">project issue</see> associated with the change request.
		/// </summary>
        [PXDBInt]
        [PXUIField(DisplayName = "Project Issue", FieldClass = nameof(FeaturesSet.ConstructionProjectManagement))]
        [PXSelector(typeof(SearchFor<ProjectIssue.projectIssueId>
                .Where<ProjectIssue.projectId.IsEqual<PMChangeRequest.projectID.FromCurrent>>),
            SubstituteKey = typeof(ProjectIssue.projectIssueCd))]
        [PXFormula(typeof(Default<PMChangeRequest.projectID>))]
        public int? ProjectIssueID
        {
            get;
            set;
        }
    }
}
