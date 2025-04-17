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

using PX.SM;
using System;
using PX.Data;

namespace PX.Objects.EP
{
    public class DepartmentMaint : PXGraph<DepartmentMaint>
    {
        public PXSelect<EPDepartment> EPDepartment;
        public PXSavePerRow<EPDepartment> Save;
        public PXCancel<EPDepartment> Cancel;
        public PXInsert<EPDepartment> Insert;
        public PXDelete<EPDepartment> Delete;
        protected virtual void EPDepartment_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            string DepartmentID = ((EPDepartment)e.Row).DepartmentID;
            EPDepartment p = PXSelect<EPDepartment, Where<EPDepartment.departmentID, Equal<Required<EPDepartment.departmentID>>>>.SelectWindowed(this, 0, 1, DepartmentID);
            if (p != null)
            {
                cache.RaiseExceptionHandling<EPDepartment.departmentID>(e.Row, DepartmentID, new PXException(Messages.RecordExists));
                e.Cancel = true;
            }
        }

        protected virtual void EPDepartment_DepartmentID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            EPDepartment row = e.Row as EPDepartment;
            if (row != null)
            {
                if (e.NewValue != null && row.DepartmentID != null)
                {
                    EPEmployee employee =
                    PXSelect<EPEmployee, Where<EPEmployee.departmentID, Equal<Required<EPDepartment.departmentID>>>>
                       .SelectWindowed(this, 0, 1, row.DepartmentID);
                    if (employee != null && employee.DepartmentID != e.NewValue.ToString())
                    {
                        throw new PXSetPropertyException(Messages.DepartmentInUse);
                    }
                }
            }
        }
    }
}