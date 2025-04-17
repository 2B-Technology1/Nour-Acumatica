using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Common;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.FA;
using PX.Objects.GL.JournalEntryState;
using PX.Objects.GL.JournalEntryState.PartiallyEditable;
using PX.Objects.GL.Overrides.PostGraph;
using PX.Objects.GL.Reclassification.UI;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects;
using PX.Objects.GL;
using MyMaintaince.SN;
namespace PX.Objects.GL
{
  
  public class JournalEntry_Extension:PXGraphExtension<JournalEntry>
  {

   #region Event Handlers

      protected virtual void GLTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
      {
          GLTran row = (GLTran)e.Row;
          GLTranExt rowExt = PXCache<GLTran>.GetExtension<GLTranExt>(row);
          if (row != null)
          {
              if (row.DebitAmt != 0)
              {
                  AccountSubSerials line = PXSelect<AccountSubSerials, Where<AccountSubSerials.accountID, Equal<Required<AccountSubSerials.accountID>>, And<AccountSubSerials.subID, Equal<Required<AccountSubSerials.subID>>, And<AccountSubSerials.tranType, Equal<ASSTTypes.debit>>>>>.Select(this.Base, row.AccountID, row.SubID);
                  if ((rowExt.UsrSerialNbr == 0 || String.IsNullOrEmpty(rowExt.UsrSerialNbr + "")) && line != null)
                  {
                      rowExt.UsrSerialNbr = line.LastRefNbr + 1;
                      sender.SetValueExt<GLTranExt.usrSerialNbr>(row, line.LastRefNbr + 1);

                      //
                      AccountSubSerialEntry g = PXGraph.CreateInstance<AccountSubSerialEntry>();
                      g.accountSubSerials.Current = line;
                      line.LastRefNbr += 1;
                      g.accountSubSerials.Update(line);
                      g.Persist(typeof(AccountSubSerials), PXDBOperation.Update);
                      g.Actions.PressSave();

                  }
              }
              else
              {
                  AccountSubSerials line = PXSelect<AccountSubSerials, Where<AccountSubSerials.accountID, Equal<Required<AccountSubSerials.accountID>>, And<AccountSubSerials.subID, Equal<Required<AccountSubSerials.subID>>, And<AccountSubSerials.tranType, Equal<ASSTTypes.credit>>>>>.Select(this.Base, row.AccountID, row.SubID);
                  if ((rowExt.UsrSerialNbr == 0 || String.IsNullOrEmpty(rowExt.UsrSerialNbr + "")) && line != null)
                  {
                      rowExt.UsrSerialNbr = line.LastRefNbr + 1;
                      sender.SetValueExt<GLTranExt.usrSerialNbr>(row, line.LastRefNbr + 1);

                      //
                      AccountSubSerialEntry g = PXGraph.CreateInstance<AccountSubSerialEntry>();
                      g.accountSubSerials.Current = line;
                      line.LastRefNbr += 1;
                      g.accountSubSerials.Update(line);
                      g.Persist(typeof(AccountSubSerials), PXDBOperation.Update);
                      g.Actions.PressSave();
                  }
              }
          }
      }

      protected virtual void GLTran_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
      {
          //GLTran row = (GLTran)e.Row;
          //GLTranExt rowExt = PXCache<GLTran>.GetExtension<GLTranExt>(row);
          //rowExt.UsrSerialNbr = 0;
          //sender.SetValueExt<GLTranExt.usrSerialNbr>(row, 0);
      }

      protected virtual void GLTran_SubID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
      {

          GLTran row = (GLTran)e.Row;
          GLTranExt rowExt = PXCache<GLTran>.GetExtension<GLTranExt>(row);
          rowExt.UsrSerialNbr = 0;
          sender.SetValueExt<GLTranExt.usrSerialNbr>(row, 0);
          
      }
    #endregion
        
  }


}