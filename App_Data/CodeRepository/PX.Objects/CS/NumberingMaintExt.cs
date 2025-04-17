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

using System.Linq;
using PX.Data;
using PX.Objects.GL;


namespace PX.Objects.CS
{
    public class NumberingMaintExt : PXGraphExtension<NumberingMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>();
        }

        protected virtual void Numbering_UserNumbering_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            if ((bool)e.NewValue == true && PXSelect<GLWorkBook, Where<GLWorkBook.voucherNumberingID, Equal<Current<Numbering.numberingID>>>>.Select(Base).Any())
            {
                e.NewValue = false;
                var row = (Numbering) e.Row;
                cache.RaiseExceptionHandling<Numbering.userNumbering>(row, row.UserNumbering, new PXSetPropertyException(Messages.NubmeringCannotBeSetManual, PXErrorLevel.Warning));
            }
        }
    }
}
