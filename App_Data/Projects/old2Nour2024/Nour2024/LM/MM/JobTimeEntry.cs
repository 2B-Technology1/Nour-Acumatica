using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;

namespace MyMaintaince
{
    public class JobTimeEntry:PXGraph<JobTimeEntry>
    {
        public PXSave<JobTime> Save;
        public PXCancel<JobTime> Cancel;

        [PXImport(typeof(JobTime))]
        public PXSelect<JobTime> jobTime;
    }
}
