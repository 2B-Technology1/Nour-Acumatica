using System;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20230821VTaxFieldsSolveError
{
    public class SetupFormMaint : PXGraph<SetupFormMaint>
    {

        public SelectFrom<SetupForm>.View SetupForm;

        public PXSave<SetupForm> Save;
        public PXCancel<SetupForm> Cancel;



    }
}