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

using AutoMapper;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;

namespace PX.Objects.PJ.Common.Mappers
{
    public class DrawingLogRelationMapperProfile : Profile
    {
        public DrawingLogRelationMapperProfile()
        {
            CreateMap<RequestForInformation, UnlinkedDrawingLogRelation>()
                .ForMember(relation => relation.DueDate, member => member
                    .MapFrom(requestForInformation => requestForInformation.DueResponseDate))
                .ForMember(relation => relation.DocumentCd, member => member
                    .MapFrom(requestForInformation => requestForInformation.RequestForInformationCd))
                .ForMember(relation => relation.DocumentId, member => member
                    .MapFrom(requestForInformation => requestForInformation.NoteID))
                .ForMember(relation => relation.DocumentType, member => member
                    .MapFrom(src => CacheNames.RequestForInformation));
            CreateMap<ProjectIssue, UnlinkedDrawingLogRelation>()
                .ForMember(relation => relation.DocumentId, member => member
                    .MapFrom(projectIssue => projectIssue.NoteID))
                .ForMember(relation => relation.DocumentCd, member => member
                    .MapFrom(projectIssue => projectIssue.ProjectIssueCd))
                .ForMember(relation => relation.DocumentType, member => member
                    .MapFrom(src => CacheNames.ProjectIssue));
            CreateMap<RequestForInformation, LinkedDrawingLogRelation>()
                .ForMember(relation => relation.DueDate, member => member
                    .MapFrom(requestForInformation => requestForInformation.DueResponseDate))
                .ForMember(relation => relation.DocumentCd, member => member
                    .MapFrom(requestForInformation => requestForInformation.RequestForInformationCd))
                .ForMember(relation => relation.DocumentId, member => member
                    .MapFrom(requestForInformation => requestForInformation.NoteID))
                .ForMember(relation => relation.DocumentType, member => member
                    .MapFrom(src => CacheNames.RequestForInformation));
            CreateMap<ProjectIssue, LinkedDrawingLogRelation>()
                .ForMember(relation => relation.DocumentId, member => member
                    .MapFrom(projectIssue => projectIssue.NoteID))
                .ForMember(relation => relation.DocumentCd, member => member
                    .MapFrom(projectIssue => projectIssue.ProjectIssueCd))
                .ForMember(relation => relation.DocumentType, member => member
                    .MapFrom(src => CacheNames.ProjectIssue));
            CreateMap<UnlinkedDrawingLogRelation, LinkedDrawingLogRelation>();
            CreateMap<LinkedDrawingLogRelation, UnlinkedDrawingLogRelation>();
            CreateMap<LinkedDrawingLogRelation, LinkedDrawingLogRelation>()
                .ForMember(relation => relation.Selected, member => member
                    .Ignore());
        }
    }
}
