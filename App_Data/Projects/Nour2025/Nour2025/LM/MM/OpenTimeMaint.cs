using System;
using PX.Data;
using System.Globalization;
using PX.Objects.EP;
using System.Collections;

namespace MyMaintaince
{
    public class OpenTimeMaint:PXGraph<OpenTimeMaint>//,OPenTime>
    {
        public PXSelect<OPenTime> openTime;
        public PXSave<OPenTime> Save;


        public PXSelect<WorkerOpenTime, Where<WorkerOpenTime.OpenTimeID, Equal<Current<OPenTime.openTimeID>>>> workerOpenTime;
      

       

        public OpenTimeMaint() 
        {
            OPenTime row = openTime.Current;
            if (row != null)
            {
                if (row.Status == OpenTimeStatus.Closed)
                {
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(workerOpenTime.Cache, null, false);

                    workerOpenTime.AllowDelete = false;
                    workerOpenTime.AllowInsert = false;
                    workerOpenTime.AllowUpdate = false;

                }
                else
                {
                    workerOpenTime.AllowDelete = true;
                    workerOpenTime.AllowInsert = true;
                    workerOpenTime.AllowUpdate = true;
                }
            }
        
        }

       protected void OPenTime_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            if (e.Row != null)
            {
                OPenTime row = e.Row as OPenTime;
                if (row.Status == OpenTimeStatus.Closed)
                {
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(workerOpenTime.Cache, null, false);
                    workerOpenTime.AllowDelete = false;
                    workerOpenTime.AllowInsert = false;
                    workerOpenTime.AllowUpdate = false;

                }
                else 
                {
                    workerOpenTime.AllowDelete = true;
                    workerOpenTime.AllowInsert = true;
                    workerOpenTime.AllowUpdate = true;
                }
            }
        }

       protected virtual void WorkerOpenTime_Code_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
       {
           WorkerOpenTime row = e.Row as WorkerOpenTime;
           PXSelectBase<EPEmployee> emps = new PXSelectReadonly<EPEmployee, Where<EPEmployee.acctCD, Equal<Required<EPEmployee.acctCD>>>>(this);
           emps.Cache.ClearQueryCache();
           EPEmployee ep = emps.Select(row.Code);
           sender.SetValue<WorkerOpenTime.name>(row, ep.AcctName);
           
       }
       protected virtual void OPenTime_clse_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
       {
           if (e.Row != null)
           {
               OPenTime row = e.Row as OPenTime;
               if (row.Clse == true)
               {
                   DateTime date = DateTime.Now;
                   this.openTime.Current.EndeTime = date;

                 //  String hour;
                   String min;
                  
                   if (date.Minute < 10)
                   {
                       min = "0" + date.Minute;
                   }
                   else
                   {
                       min = date.Minute.ToString();
                   }


                   if (date.Hour < 12)
                   {
                       if (date.Hour < 10)
                           this.openTime.Current.EndTime = "0" + date.Hour + ":" + min + ":00 AM";
                       else
                           this.openTime.Current.EndTime = date.Hour + ":" + min + ":00 AM";
                   }
                   else
                   {
                       if ((date.Hour - 12) < 10)
                           this.openTime.Current.EndTime = "0" + (date.Hour - 12).ToString() + ":" + min + ":00 PM";
                       else
                           this.openTime.Current.EndTime = (date.Hour - 12).ToString() + ":" + min + ":00 PM";
                   }



                   DateTime? startDate = openTime.Current.StartTime;
                   DateTime? endDate = openTime.Current.EndeTime;


                   String startTime = openTime.Current.StarTime;
                   String startTimeSuffix = startTime.Substring(startTime.Length - 2, 2);
                   String startTimeWithoutSuffix = startTime.Substring(0, startTime.Length - 3); // space + AM

                   DateTime startTimetime = DateTime.ParseExact(startTimeWithoutSuffix, "HH:mm:ss", CultureInfo.InvariantCulture);
                   int startHour = startTimetime.Hour;
                   if (startTimeSuffix.Equals("PM"))
                       startHour += 12;
                   int startMin = startTimetime.Minute;


                   String endTime = openTime.Current.EndTime;
                   String endTimeSuffix = endTime.Substring(endTime.Length - 2, 2);
                   String endTimeWithoutSuffix = endTime.Substring(0, endTime.Length - 3); // space + AM


                   DateTime endTimetime = DateTime.ParseExact(endTimeWithoutSuffix, "HH:mm:ss", CultureInfo.InvariantCulture);
                   int endHour = endTimetime.Hour;
                   if (endTimeSuffix.Equals("PM"))
                       endHour += 12;
                   int endtMin = endTimetime.Minute;


                   TimeSpan? daysDiff = endDate - startDate.Value;

                   int hourDiff = endHour - startHour;
                   int minDiff = endtMin - startMin;

                   double hoursFromMin = (minDiff) / 60.0;

                   double total = hourDiff + hoursFromMin;
                   openTime.Current.Dauration = ((daysDiff.Value.Days * 9) + (total)).ToString();

                   #region Query standard time
                   PXSelectBase<JobOrderNonStockItems> rows = new PXSelectReadonly<JobOrderNonStockItems, Where<JobOrderNonStockItems.jobOrdrID, Equal<Required<JobOrderNonStockItems.jobOrdrID>>, And<JobOrderNonStockItems.inventoryCode, Equal<Required<JobOrderNonStockItems.inventoryCode>>>>>(this);
                   rows.Cache.ClearQueryCache();
                   JobOrderNonStockItems rec = rows.Select(row.JobOrderID,row.InventoryCode);
                   #endregion
                   openTime.Current.ManualDauration = rec.Time + "";

                   double variation = ((double)rec.Time) - ((daysDiff.Value.Days * 9) + (total));
                   openTime.Current.Variation = variation + "";
                   if (variation < 0)
                   {
                       sender.RaiseExceptionHandling<OPenTime.variation>(row, variation + "", new PXSetPropertyException("This job take more than specified standard time !", PXErrorLevel.Warning));
                   }
                   this.openTime.Current.Status = OpenTimeStatus.Closed;
               }
           }
       }

       public PXAction<OPenTime> ReStartTime;
       [PXUIField(DisplayName = "ReStart Time",
        MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
       [PXButton()]
       public IEnumerable reStartTime(PXAdapter adapter)
       {
           OPenTime op = openTime.Current as OPenTime;
           DateTime date = DateTime.Now;
           op.StartTime = date;

           //String hour;
           String min;
           /**
           if (date.Hour < 10)
           {
               hour = "0" + date.Hour;
           }
           else
           {
               hour = date.Hour.ToString();
           }
           **/
           if (date.Minute < 10)
           {
               min = "0" + date.Minute;
           }
           else
           {
               min = date.Minute.ToString();
           }

           if (date.Hour < 12)
           {
               if (date.Hour < 10)
                   op.StarTime = "0" + date.Hour + ":" + min + ":00 AM";
               else
                   op.StarTime = date.Hour + ":" + min + ":00 AM";
           }
           else
           {
               if ((date.Hour - 12) < 10)
                   op.StarTime = "0" + (date.Hour - 12).ToString() + ":" + min + ":00 PM";
               else
                   op.StarTime = (date.Hour - 12).ToString() + ":" + min + ":00 PM";
           }

           return adapter.Get();
       }
       
    }
}
