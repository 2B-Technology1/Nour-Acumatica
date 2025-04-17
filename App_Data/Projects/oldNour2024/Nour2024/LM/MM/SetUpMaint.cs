using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.GL;
namespace MyMaintaince
{
    public class SetUpMaint:PXGraph<SetUpMaint>
    {
        public PXCancel<SetUp> Cancel;
        public PXSave<SetUp> Save;
        public PXSelect<SetUp> DocLastNumbers;
        protected virtual void Setup_branchID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            SetUp row = e.Row as SetUp;
            Branch p = PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, e.NewValue);
            row.BranchCD = p.BranchCD;
        }
         
    }
}
