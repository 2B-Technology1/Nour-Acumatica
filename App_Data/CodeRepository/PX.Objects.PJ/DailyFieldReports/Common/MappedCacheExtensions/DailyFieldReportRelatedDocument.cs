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
using PX.Data.BQL;

namespace PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions
{
    [PXHidden]
    public class DailyFieldReportRelatedDocument : PXMappedCacheExtension
    {
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        public virtual string ReferenceNumber
        {
            get;
            set;
        }

        public virtual int? ReferenceId
        {
            get;
            set;
        }

        public virtual int? ProjectId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class referenceNumber : BqlString.Field<referenceNumber>
        {
        }

        public abstract class referenceId : BqlInt.Field<referenceId>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }
    }
}
