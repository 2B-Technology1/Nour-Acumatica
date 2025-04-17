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
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.ProjectsIssue.PJ.DAC
{
    [Serializable]
    [PXCacheName("Project Issue Drawing Log")]
    public class ProjectIssueDrawingLog : DrawingLogReferenceBase, IBqlTable
    {
        [PXDBInt(IsKey = true)]
        public virtual int? ProjectIssueId
        {
            get;
            set;
        }

        public abstract class projectIssueId : BqlInt.Field<projectIssueId>
        {
        }

        public abstract class drawingLogId : BqlInt.Field<drawingLogId>
        {
        }
    }
}