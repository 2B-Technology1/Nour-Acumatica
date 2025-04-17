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

using PX.Data;
using PX.Objects.IN;
using PX.Objects.PM;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class GroupDocLineExt
    {
        public int? DocID;
        public int? LineID;
        public List<DocLineExt> Group;

        public GroupDocLineExt(int? docID, int? lineID, List<DocLineExt> group)
        {
            this.DocID = docID;
            this.LineID = lineID;
            this.Group = group;
        }
    }

    public class DocLineExt
    {
        public IDocLine docLine;
        public FSPostDoc fsPostDoc;
        public FSSrvOrdType fsSrvOrdType;
        public FSServiceOrder fsServiceOrder;
        public FSAppointment fsAppointment;
        public FSPostInfo fsPostInfo;
        public FSSODet fsSODet;
        public FSAppointmentDet fsAppointmentDet;
        public PMTask pmTask;

        public DocLineExt(PXResult<FSSODet, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo,  PMTask> soDetLine)
        {
            this.docLine = (IDocLine)(FSSODet)soDetLine;
            this.fsPostDoc = (FSPostDoc)soDetLine;
            this.fsServiceOrder = (FSServiceOrder)soDetLine;
            this.fsSrvOrdType = (FSSrvOrdType)soDetLine;
            this.fsAppointment = null;
            this.fsPostInfo = (FSPostInfo)soDetLine;
            this.fsSODet = (FSSODet)soDetLine;
            this.fsAppointmentDet = null;
            this.pmTask = (PMTask)soDetLine;
        }

        public DocLineExt(PXResult<FSSODet, FSServiceOrder, FSSrvOrdType,  PMTask> soDetLine)
        {
            this.docLine = (IDocLine)(FSSODet)soDetLine;
            this.fsPostDoc = null;
            this.fsServiceOrder = (FSServiceOrder)soDetLine;
            this.fsSrvOrdType = (FSSrvOrdType)soDetLine;
            this.fsAppointment = null;
            this.fsPostInfo = null;
            this.fsSODet = (FSSODet)soDetLine;
            this.fsAppointmentDet = null;
            this.pmTask = (PMTask)soDetLine;

        }

        public DocLineExt(PXResult<FSAppointmentDet, FSAppointment, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo, FSSODet,  PMTask> appointmentDetLine)
        {
            this.docLine = (IDocLine)(FSAppointmentDet)appointmentDetLine;
            this.fsPostDoc = (FSPostDoc)appointmentDetLine;
            this.fsServiceOrder = (FSServiceOrder)appointmentDetLine;
            this.fsSrvOrdType = (FSSrvOrdType)appointmentDetLine;
            this.fsAppointment = (FSAppointment)appointmentDetLine;
            this.fsPostInfo = (FSPostInfo)appointmentDetLine;
            this.fsSODet = (FSSODet)appointmentDetLine;
            this.fsAppointmentDet = (FSAppointmentDet)appointmentDetLine;
            this.pmTask = (PMTask)appointmentDetLine;
        }

        public DocLineExt(PXResult<FSAppointmentDet, FSAppointment, FSServiceOrder, FSSrvOrdType, FSPostDoc, FSPostInfo, PMTask> appointmentDetLine)
        {
            this.docLine = (IDocLine)(FSAppointmentDet)appointmentDetLine;
            this.fsPostDoc = (FSPostDoc)appointmentDetLine;
            this.fsServiceOrder = (FSServiceOrder)appointmentDetLine;
            this.fsSrvOrdType = (FSSrvOrdType)appointmentDetLine;
            this.fsAppointment = (FSAppointment)appointmentDetLine;
            this.fsPostInfo = (FSPostInfo)appointmentDetLine;
            this.fsSODet = null;
            this.fsAppointmentDet = (FSAppointmentDet)appointmentDetLine;
            this.pmTask = (PMTask)appointmentDetLine;
        }
    }
}
