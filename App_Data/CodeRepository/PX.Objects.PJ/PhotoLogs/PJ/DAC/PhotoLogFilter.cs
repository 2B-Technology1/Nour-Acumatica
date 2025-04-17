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
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.PhotoLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName("Photo Log Filter")]
    public class PhotoLogFilter : IBqlTable
    {
        [Project(typeof(PMProject.nonProject.IsEqual<False>
                .And<PMProject.baseType.IsEqual<CTPRType.project>>),
            DisplayName = "Project", WarnIfCompleted = false)]
        public int? ProjectId
        {
            get;
            set;
        }

        [ProjectTask(typeof(projectId), DisplayName = "Project Task")]
        public int? ProjectTaskId
        {
            get;
            set;
        }

        [PXDate]
        [PXUIField(DisplayName = "Date From")]
        public DateTime? DateFrom
        {
            get;
            set;
        }

        [PXDate]
        [PXUIField(DisplayName = "Date To")]
        public DateTime? DateTo
        {
            get;
            set;
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class projectTaskId : BqlInt.Field<projectTaskId>
        {
        }

        public abstract class dateFrom : BqlDateTime.Field<dateFrom>
        {
        }

        public abstract class dateTo : BqlDateTime.Field<dateTo>
        {
        }
    }
}
