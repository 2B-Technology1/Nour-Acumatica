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

using System.Collections.Generic;
using PX.Objects.GL.FinPeriods.TableDefinition;


namespace PX.Objects.Common.Abstractions.Periods
{
    public class OrganizationDependedPeriodKey
    {
        public string PeriodID { get; set; }

        public int? OrganizationID { get; set; }

        public virtual bool Defined => PeriodID != null && OrganizationID != null;

        public virtual List<object> ToListOfObjects(bool skipPeriodID = false)
        {
            var values = new List<object>();

            if (!skipPeriodID)
            {
                values.Add(PeriodID);
            }

            values.Add(OrganizationID);

            return values;
        }

        public virtual bool IsNotPeriodPartsEqual(object otherKey)
        {
            return ((OrganizationDependedPeriodKey)otherKey).OrganizationID == OrganizationID;
        }

		public virtual bool IsMasterCalendar => OrganizationID == FinPeriod.organizationID.MasterValue;
    }
}
