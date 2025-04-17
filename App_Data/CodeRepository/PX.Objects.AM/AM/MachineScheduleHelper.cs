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
using PX.Common;

namespace PX.Objects.AM
{
    public static class MachineScheduleHelper
    {
        /// <summary>
        /// Does the list of machine schedule detail all contain the same <see cref="AMMachSchdDetail.RunTimeBase"/> value
        /// </summary>
        public static bool HasSameSchdTime(this List<AMMachSchdDetail> value)
        {
            if (value == null)
            {
                return false;
            }

            if (value.Count() <= 1)
            {
                return true;
            }

            for (var i = 1; i < value.Count; i++)
            {
                if (!value[i - 1].RunTimeBase.GetValueOrDefault().Equals(value[i].RunTimeBase.GetValueOrDefault()))
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<AMMachSchdDetail> GetHasSchdTime(this List<AMMachSchdDetail> value)
        {
            if (value == null)
            {
                yield break;
            }

            foreach (var machSchdDetail in value)
            {
                if ((machSchdDetail?.RunTimeBase ?? 0) > 0)
                {
                    yield return machSchdDetail;
                }
            }
        }

        public static MachineCalendarHelper FindCalendarByMachineId(this List<MachineCalendarHelper> value, AMMachSchdDetail machSchdDetail)
        {
            return FindCalendarByMachineId(value, machSchdDetail?.MachID);
        }

        public static MachineCalendarHelper FindCalendarByMachineId(this List<MachineCalendarHelper> value, string machineId)
        {
            if (value == null || string.IsNullOrWhiteSpace(machineId))
            {
                return null;
            }

            foreach (var machineCalendarHelper in value)
            {
                if (machineCalendarHelper?.Machine?.MachID.Equals(machineId) == true)
                {
                    return machineCalendarHelper;
                }
            }

            return null;
        }
    }
}