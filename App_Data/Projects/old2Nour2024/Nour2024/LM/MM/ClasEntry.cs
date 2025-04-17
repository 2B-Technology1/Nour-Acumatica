using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.SM;
namespace MyMaintaince
{
   public  class ClasEntry:PXGraph<ClasEntry>
    {
       public PXSave<Cls> Save;
       public PXCancel<Cls> Cancel;


       [PXImport(typeof(Cls))]
       public PXSelect<Cls> Clas;

       public virtual void Cls_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
       {

           Cls row = this.Clas.Current;
           PXSelectBase<JobOrder> job = new PXSelect<JobOrder, Where<JobOrder.classID, Equal<Required<JobOrder.classID>>>>(this);
           if (row != null)
           {
               PXResultset<JobOrder> result = job.Select(row.ClassID);
               if (result.Count > 0)
               {
                   throw new PXSetPropertyException("can not delete this class Because it selected in Job Order.");
               }
           }

       }
    }
}
