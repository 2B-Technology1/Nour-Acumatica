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
using PX.Objects.EP;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Shift differential types
    /// </summary>
    public class ShiftDiffType
    {
        /// <summary>
        /// Amount = A
        ///     Differential is a fixed amount per hour
        /// </summary>
        public const string Amount = "A";
        /// <summary>
        /// Rate = R
        ///     Differential is a rate per hour
        /// </summary>
        public const string Rate = "R";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Amount => Messages.GetLocal(Messages.AmountDiff);
            public static string Rate => Messages.GetLocal(Messages.RateDiff);
        }


        public class amount : PX.Data.BQL.BqlString.Constant<amount>
        {
            public amount() : base(Amount) { ;}
        }
        public class rate : PX.Data.BQL.BqlString.Constant<rate>
        {
            public rate() : base(Rate) { ;}
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                        new string[] { Amount, Rate },
                        new string[] { Messages.AmountDiff, Messages.RateDiff }) { ; }
        }

        /// <summary>
        /// Get the shifts differential cost for the given work center
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="workCenterID">work center ID</param>
        /// <returns>Calculated total differential cost</returns>
        public static decimal GetShiftDifferentialCost(PXGraph graph, string workCenterID)
        {
            if (graph == null || string.IsNullOrWhiteSpace(workCenterID))
            {
                return 0;
            }

			PXResult<AMWC, AMShift, EPShiftCode, EPShiftCodeRate, AMWCCury> result = (PXResult<AMWC, AMShift, EPShiftCode, EPShiftCodeRate, AMWCCury>)PXSelectJoin<AMWC,
				InnerJoin<AMShift, On<AMShift.wcID, Equal<AMWC.wcID>>,
				InnerJoin<EPShiftCode, On<EPShiftCode.shiftCD, Equal<AMShift.shiftCD>>,
				InnerJoin<EPShiftCodeRate, On<EPShiftCodeRate.shiftID, Equal<EPShiftCode.shiftID>>,
				LeftJoin<AMWCCury, On<AMWCCury.wcID, Equal<AMWC.wcID>
					,And<AMWCCury.curyID, Equal<EPShiftCodeRate.curyID>>
					>>>>>,
				Where<AMWC.wcID, Equal<Required<AMWC.wcID>>,
					And<EPShiftCodeRate.curyID, Equal<Required<EPShiftCodeRate.curyID>>>>,
				OrderBy<Asc<AMShift.shiftCD, Desc<EPShiftCodeRate.effectiveDate>>>>.SelectWindowed(graph, 0, 1, workCenterID, graph.Accessinfo.BaseCuryID);

			var shiftCodeRate = (EPShiftCodeRate)result;
			var wcCury = (AMWCCury)result;
			

			(decimal? shftDiff, string diffType) = ShiftMaint.GetShiftDiffAndType(shiftCodeRate);
			return GetShiftDifferentialCost(wcCury?.StdCost ?? 0, shftDiff, diffType);
		}

        /// <summary>
        /// Get the shifts differential cost
        /// </summary>
        /// <param name="laborCost">Work center/Employee standard labor cost</param>
        /// <param name="shiftDifferentialCost">Shift differential cost</param>
        /// <param name="wcShiftDiffType">Differential type</param>
        /// <returns>Calculated total differential cost</returns>
        public static decimal GetShiftDifferentialCost(decimal? laborCost, decimal? shiftDifferentialCost, string wcShiftDiffType)
        {
            if (string.IsNullOrWhiteSpace(wcShiftDiffType))
            {
                return 0;
            }

            if (wcShiftDiffType.EqualsWithTrim(Amount))
            {
                return laborCost.GetValueOrDefault() + shiftDifferentialCost.GetValueOrDefault();
            }

            return laborCost.GetValueOrDefault() * shiftDifferentialCost.GetValueOrDefault();

        }
    }
}
