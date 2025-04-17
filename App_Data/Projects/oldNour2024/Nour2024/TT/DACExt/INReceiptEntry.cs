using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using System.Text;
using PX.Objects;
using PX.Objects.IN;
using MyMaintaince.SN;

namespace PX.Objects.IN
{
  
  public class INReceiptEntry_Extension:PXGraphExtension<INReceiptEntry>
  {

        public static bool IsActive()
        {
            return true;
        }

 #region Event Handlers

      protected virtual void INTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
      {
          try
          {
              INTran row = (INTran)e.Row;
              INRegister reg = this.Base.CurrentDocument.Select();
              INRegisterExt regExt = PXCache<INRegister>.GetExtension<INRegisterExt>(reg);

              if (row != null)
              {
                  InventoryWarehouseSerials line = PXSelect<InventoryWarehouseSerials, Where<InventoryWarehouseSerials.warehouse, Equal<Required<InventoryWarehouseSerials.warehouse>>, And<InventoryWarehouseSerials.tranType, Equal<WSTTypes.receipt>>>>.Select(this.Base, row.SiteID);
                  if ((regExt.UsrWarehouseSerial == 0 || String.IsNullOrEmpty(regExt.UsrWarehouseSerial + "")) && line != null)
                  {

                      if (this.Base.transactions.Select().Count <= 0)
                          return;

                      regExt.UsrWarehouseSerial = line.LastRefNbr + 1;
                      sender.SetValueExt<INRegisterExt.usrWarehouseSerial>(reg, line.LastRefNbr + 1);

                        // Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [Justification]
                        WarehouseSerialEntry g = PXGraph.CreateInstance<WarehouseSerialEntry>();
                      g.warehouseSerial.Current = line;
                      line.LastRefNbr = line.LastRefNbr + 1;
                      g.warehouseSerial.Update(line);
                        // Acuminator disable once PX1043 SavingChangesInEventHandlers [Justification]
                        g.Persist(typeof(InventoryWarehouseSerials), PXDBOperation.Update);
                        // Acuminator disable once PX1043 SavingChangesInEventHandlers [Justification]
                        g.Actions.PressSave();

                  }
              }
          }
          catch
          { }
          

      }


      protected virtual void INTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
      {

          try
          {
              INRegister reg = this.Base.CurrentDocument.Current;
              INRegisterExt regExt = PXCache<INRegister>.GetExtension<INRegisterExt>(reg);
              if (this.Base.transactions.Select().Count <= 0)
              {
                  regExt.UsrWarehouseSerial = 0;
                  sender.SetValueExt<INRegisterExt.usrWarehouseSerial>(reg, 0);
              }
          }
          catch { }
      }

     
      #endregion 
 
        
        }


}