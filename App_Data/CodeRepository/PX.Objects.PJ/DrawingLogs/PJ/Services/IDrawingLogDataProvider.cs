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
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.DrawingLogs.PJ.Services
{
    public interface IDrawingLogDataProvider
    {
        bool IsAttachedToEntity<TTable, TField>(Guid? fileId)
            where TTable : class, IBqlTable, new()
            where TField : IBqlField;

        IEnumerable<Guid> GetDrawingLogFileIds(IEnumerable<Guid> fileIds);

        IEnumerable<DrawingLog> GetRequestForInformationDrawingLogs(int? requestForInformationId);

        DrawingLogDiscipline GetDiscipline<TDisciplineField>(object disciplineFieldValue)
            where TDisciplineField : IBqlOperand, IBqlField;

        DrawingLogStatus GetStatus(int? statusId);

        IEnumerable<DrawingLog> GetDrawingLogs<TDrawingLogField>(object drawingLogFieldValue)
            where TDrawingLogField : IBqlField;

        DrawingLog GetDrawingLog<TDrawingLogField>(object drawingLogFieldValue)
            where TDrawingLogField : IBqlField;

        DrawingLog GetDrawingLog(Guid? fileId);

        IEnumerable<DrawingLog> GetOriginalDrawingLogWithRevisions(Guid? originalDrawingLogNoteId);

        IEnumerable<int?> GetDisciplineSortOrders();
    }
}