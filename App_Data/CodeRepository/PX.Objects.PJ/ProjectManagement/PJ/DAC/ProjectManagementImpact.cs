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
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
    public class ProjectManagementImpact : BaseCache, IProjectManagementImpact
    {
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Schedule Impact", Visibility = PXUIVisibility.Visible)]
        public virtual bool? IsScheduleImpact
        {
            get;
            set;
        }

        [PXDBInt]
        [PXUIVisible(typeof(isScheduleImpact.IsEqual<True>))]
        [PXUIField(DisplayName = "Schedule Impact (days)")]
        public virtual int? ScheduleImpact
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Cost Impact", Visibility = PXUIVisibility.Visible)]
        public virtual bool? IsCostImpact
        {
            get;
            set;
        }

        [PXDBDecimal]
        [PXUIVisible(typeof(isCostImpact.IsEqual<True>))]
        [PXUIField(DisplayName = "Cost Impact")]
        public virtual decimal? CostImpact
        {
            get;
            set;
        }

        public abstract class isScheduleImpact : BqlBool.Field<isScheduleImpact>
        {
        }

        public abstract class scheduleImpact : BqlInt.Field<scheduleImpact>
        {
        }

        public abstract class isCostImpact : BqlBool.Field<isCostImpact>
        {
        }

        public abstract class costImpact : BqlDecimal.Field<costImpact>
        {
        }
    }
}