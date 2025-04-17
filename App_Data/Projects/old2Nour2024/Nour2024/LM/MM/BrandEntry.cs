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
    public class BrandEntry:PXGraph<BrandEntry,Brand>
    {
        public PXSelect<Brand> brand;
        public PXSelect<Model, Where<Model.brandID, Equal<Current<Brand.brandID>>>> model;
        public virtual void Brand_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
         {
             Brand row = brand.Current;
             PXSelectBase<Items> itm = new PXSelect<Items, Where<Items.brandID, Equal<Required<Items.brandID>>>>(this);
             PXResultset<Items> result=itm.Select(row.BrandID);
             if (result.Count>0)
             {
                 //brand.Current.Name = result.Count.ToString();
                 throw new PXSetPropertyException("this brand selected in one of the Vechile Chassiss");
             }
         }
        public virtual void Model_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            Model row = model.Current;
            PXSelectBase<Items> itm = new PXSelect<Items, Where<Items.brandID, Equal<Required<Items.brandID>>>>(this);
            PXResultset<Items> result = itm.Select(row.BrandID);
            if (result.Count > 0)
            {
                //brand.Current.Name = result.Count.ToString();
                throw new PXSetPropertyException("this model can not delete it.");
            }
        }
    }
   
}
