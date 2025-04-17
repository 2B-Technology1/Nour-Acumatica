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
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [Serializable]
    [PXProjection(typeof(SelectFrom<DrawingLog>
	    .InnerJoin<RequestForInformationDrawingLog>
			.On<DrawingLog.drawingLogId.IsEqual<RequestForInformationDrawingLog.drawingLogId>>
	    .InnerJoin<RequestForInformation>
			.On<RequestForInformationDrawingLog.requestForInformationId
				.IsEqual<RequestForInformation.requestForInformationId>>
	    .InnerJoin<NoteDoc>
			.On<DrawingLog.noteID.IsEqual<NoteDoc.noteID>>
	    .InnerJoin<CRSMEmail>
			.On<CRSMEmail.refNoteID.IsEqual<RequestForInformation.noteID>>
        .InnerJoin<NoteDoc2>
			.On<CRSMEmail.noteID.IsEqual<NoteDoc2.noteID>
                .And<NoteDoc.fileID.IsEqual<NoteDoc2.fileID>>>
        .AggregateTo<GroupBy<RequestForInformation.requestForInformationCd>,
		    GroupBy<RequestForInformation.requestForInformationCd>,
		    GroupBy<DrawingLog.drawingLogCd>,
		    GroupBy<NoteDoc.fileID>,
		    GroupBy<CRSMEmail.noteID>>))]
    [PXCacheName(CacheNames.EmailDrawings)]
    public class EmailDrawings : IBqlTable
    {
        [PXDBInt(BqlField = typeof(RequestForInformation.requestForInformationId))]
        public virtual int? RequestForInformationId
        {
            get;
            set;
        }

        [PXDBString(10, IsKey = true, IsUnicode = true,
            BqlField = typeof(RequestForInformation.requestForInformationCd))]
        public virtual string RequestForInformationCd
        {
            get;
            set;
        }

        [PXDBInt(BqlField = typeof(DrawingLog.drawingLogId))]
        public virtual int? DrawingLogId
        {
            get;
            set;
        }

        [PXDBString(IsKey = true, BqlField = typeof(DrawingLog.drawingLogCd))]
        public virtual string DrawingLogCd
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true, BqlField = typeof(DrawingLog.number))]
        public virtual string Number
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true, BqlField = typeof(DrawingLog.revision))]
        public virtual string Revision
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true, BqlField = typeof(DrawingLog.sketch))]
        public virtual string Sketch
        {
            get;
            set;
        }

        [PXDBGuid(BqlField = typeof(CRSMEmail.noteID))]
        public virtual Guid? EmailNoteId
        {
            get;
            set;
        }

        public abstract class requestForInformationId : BqlInt.Field<requestForInformationId>
        {
        }

        public abstract class requestForInformationCd : BqlString.Field<requestForInformationCd>
        {
        }

        public abstract class drawingLogId : BqlInt.Field<drawingLogId>
        {
        }

        public abstract class drawingLogCd : BqlString.Field<drawingLogCd>
        {
        }

        public abstract class number : BqlString.Field<number>
        {
        }

        public abstract class revision : BqlString.Field<revision>
        {
        }

        public abstract class sketch : BqlString.Field<sketch>
        {
        }

        public abstract class emailNoteId : BqlGuid.Field<emailNoteId>
        {
        }
    }
}