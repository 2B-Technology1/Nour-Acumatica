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
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CT;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Weather Processing Log Filter")]
    public class WeatherProcessingLogFilter : IBqlTable
    {
        [Project(typeof(Where<PMProject.nonProject.IsEqual<False>
            .And<PMProject.baseType.IsEqual<CTPRType.project>>>), DisplayName = "Project")]
        public int? ProjectId
        {
            get;
            set;
        }

        [PXString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Weather API Service")]
        [WeatherApiService.List]
        public string WeatherApiService
        {
            get;
            set;
        }

        [PXDate]
        [PXUIField(DisplayName = "Request Date From")]
        public DateTime? RequestDateFrom
        {
            get;
            set;
        }

        [PXDate]
        [PXUIField(DisplayName = "Request Date To")]
        public DateTime? RequestDateTo
        {
            get;
            set;
        }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Show Errors Only")]
        public bool? IsShowErrorsOnly
        {
            get;
            set;
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class weatherApiService : BqlString.Field<weatherApiService>
        {
        }

        public abstract class requestDateFrom : BqlDateTime.Field<requestDateFrom>
        {
        }

        public abstract class requestDateTo : BqlDateTime.Field<requestDateTo>
        {
        }

        public abstract class isShowErrorsOnly : BqlBool.Field<isShowErrorsOnly>
        {
        }
    }
}