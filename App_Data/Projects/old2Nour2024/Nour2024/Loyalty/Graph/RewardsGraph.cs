using System;
using PX.Data;

namespace Nour20240918
{
  public class RewardsGraph : PXGraph<RewardsGraph, Rewards>
    {


        public PXSelect<Rewards> Rewards;


        protected virtual void Rewards_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            if (e.Row == null) return;
            //var row = e.Row as Rewards;
            //if (row.Id == null)
            //{
            //    e.Cancel = true;
            //}
        }


    }
}