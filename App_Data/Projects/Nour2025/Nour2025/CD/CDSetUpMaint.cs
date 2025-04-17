using Maintenance.CD;
using PX.Data;

namespace MyProject.CD
{
    public class CDSetUpMaint : PXGraph<CDSetUpMaint,CDSetUp>
    {
        //public PXSave<CDSetUp> Save;
        //public PXCancel<CDSetUp> Cancel;
        public PXSelect<CDSetUp> cdSetUp;
       
    }
}