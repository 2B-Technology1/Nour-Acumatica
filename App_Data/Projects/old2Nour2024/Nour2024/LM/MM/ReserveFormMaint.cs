using System;
using System.Collections;
using Newtonsoft.Json;
using Nour20220913V1;
using Nour2024.Helpers;
using Nour2024.Models;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR;


namespace MyMaintaince
{
    public class ReserveFormMaint : PXGraph<ReserveFormMaint, ReserveForm>
    {
        public PXSelect<ReserveForm> reserveForm;

        protected bool IsSmsSent = false;
        public virtual void ReserveForm_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            try
            {
                if (e.Row != null)
                {
                    if (e.Operation == PXDBOperation.Insert)
                    {
                        if (this.reserveForm.Current.RefNbr == "<NEW>")
                        {
                            SetUp result = PXSelect<SetUp, Where<SetUp.branchCD, Equal<Required<ReserveForm.branchCD>>>>.Select(this, this.reserveForm.Current.BranchCD);
                            if (result != null)
                            {
                                string lastNumber = result.ReserveRefNbr;
                                char[] symbols = lastNumber.ToCharArray();
                                for (int i = symbols.Length - 1; i >= 0; i--)
                                {
                                    if (!char.IsDigit(symbols[i]))
                                        break;
                                    if (symbols[i] < '9')
                                    {
                                        symbols[i]++;
                                        break;
                                    }
                                    symbols[i] = '0';
                                }

                                this.reserveForm.Current.RefNbr = new string(symbols);
                                this.ProviderUpdate<SetUp>(new PXDataFieldAssign("ReserveRefNbr", this.reserveForm.Current.RefNbr), new PXDataFieldRestrict("branchCD", this.reserveForm.Current.BranchCD));
                            }
                            else
                            {
                                e.Cancel = true;
                                throw new PXException("Please Define auto Numbering For the Selected Branch !");
                            }

                        }
                        else
                        {
                        }

                    }
                }
            }
            catch { }

                 
        }
        public virtual void ReserveForm_customer_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            ReserveForm row = e.Row as ReserveForm;
            Customer c = PXSelect<Customer, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>.Select(this, e.NewValue);
            Contact contact = SelectFrom<Contact>.Where<Contact.bAccountID.IsEqual<@P.AsInt>>.View.Select(this, c.BAccountID);
            
            this.reserveForm.Current.Name = c.AcctName;
            this.reserveForm.Current.Phone = contact?.Phone1;
        }
        protected virtual void ReserveForm_Vechile_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
           

            if (e.Row != null)
            {
                ReserveForm jobOrder = e.Row as ReserveForm;

                if (!String.IsNullOrEmpty(jobOrder.Vechile + ""))
                {
                    string itemID = (string)jobOrder.Vechile;
                    PXSelectBase<Items> ChassisBase = new PXSelectReadonly<Items, Where<Items.code, Equal<Required<Items.code>>>>(this);
                    ChassisBase.Cache.ClearQueryCache();
                    Items Chassis = ChassisBase.Select(itemID);
                    sender.SetValue<ReserveForm.licensePlate>(jobOrder, Chassis.LincensePlat);
                    sender.SetValue<ReserveForm.brandName>(jobOrder, Chassis.BrandID);
                    sender.SetValue<ReserveForm.modelName>(jobOrder, Chassis.ModelID);
                }

                else
                {
                    jobOrder.LicensePlate = null;
                    jobOrder.ModelName = null;
                    jobOrder.BrandName = null;
                }
            }

        }
        protected void ReserveForm_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            ReserveForm jobOrder = e.Row as ReserveForm;
           
            if (e.Row != null)
            {

                if (!String.IsNullOrEmpty(jobOrder.Vechile + ""))
                {
                    string itemID = (string)jobOrder.Vechile;
                    PXSelectBase<Items> ChassisBase = new PXSelectReadonly<Items, Where<Items.code, Equal<Required<Items.code>>>>(this);
                    ChassisBase.Cache.ClearQueryCache();
                    Items Chassis = ChassisBase.Select(itemID);
                    sender.SetValue<ReserveForm.licensePlate>(jobOrder, Chassis.LincensePlat);
                    sender.SetValue<ReserveForm.brandName>(jobOrder, Chassis.BrandID);
                    sender.SetValue<ReserveForm.modelName>(jobOrder, Chassis.ModelID);
                }
            }
        }
        protected void ReserveForm_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {

            ReserveForm row = e.Row as ReserveForm;
            bool IsVechileFieldEmpty = String.IsNullOrEmpty(row.Vechile);
            PXUIFieldAttribute.SetEnabled<ReserveForm.brandName>(sender, e.Row, IsVechileFieldEmpty);
            PXUIFieldAttribute.SetEnabled<ReserveForm.modelName>(sender, e.Row, IsVechileFieldEmpty);
           
            ReserveForm jobOrder = e.Row as ReserveForm;

            if (e.Row != null)
            {

                if (!String.IsNullOrEmpty(jobOrder.Vechile + ""))
                {
                    string itemID = (string)jobOrder.Vechile;
                    PXSelectBase<Items> ChassisBase = new PXSelectReadonly<Items, Where<Items.code, Equal<Required<Items.code>>>>(this);
                    ChassisBase.Cache.ClearQueryCache();
                    Items Chassis = ChassisBase.Select(itemID);
                    sender.SetValue<ReserveForm.licensePlate>(jobOrder, Chassis.LincensePlat);
                    sender.SetValue<ReserveForm.brandName>(jobOrder, Chassis.BrandID);
                    sender.SetValue<ReserveForm.modelName>(jobOrder, Chassis.ModelID);
                }
            }
        }


        protected virtual void _(Events.RowPersisted<ReserveForm> e)
        {
            var row = e.Row;
            if(row == null)
            {
                return;
            }
            bool IsSavingForFirtTime = (!IsSmsSent && (e.TranStatus == PXTranStatus.Completed));
            if (IsSavingForFirtTime == true)
            {

                string reserveFormNbr = row.RefNbr??"";
                string branchName = row.BranchCD ?? "";
                DateTime? requestDate = row.RequstDate;
                string dateString = String.Format("{0:yyyy/MM/dd}", requestDate) ?? "";
                string requestTime = row.TimeCD?.ToString()??"";
                //string locationGps = "http://bit.ly/3MqZpFx";
                SetUp result = PXSelect<SetUp, Where<SetUp.branchCD, Equal<Required<ReserveForm.branchCD>>>>.Select(this, this.reserveForm?.Current?.BranchCD);
                string locationGps = (result?.UsrGpsLink ?? "") + ":" ;
               
                string message = $@"تم تأكيد حجزكم رقم {reserveFormNbr} في مركز {branchName}, بتاريخ: {requestTime} {dateString}, موقع: {locationGps} يجب تواجد الرخصة و صورة من الرقم القومي لمالك السيارة عند حضوركم, و شكرا لمزيد من الاستفسارات اتصل علي 19943";

                string phone = row.Phone;
                if (!String.IsNullOrEmpty(phone) && reserveFormNbr != "<NEW>" && !String.IsNullOrEmpty(row.TimeCD))
                {
                    string response = SmsSender.SendMessage(message, phone);
                    IsSmsSent = true;
                    salssmsrespons responseRoot = JsonConvert.DeserializeObject<salssmsrespons>(response);

                    if (responseRoot != null)
                    {
                        if (responseRoot.code == 0)
                        {
                            string responseMessage = responseRoot.message;
                            string smsid = responseRoot.smsid;
                        }

                    }
                }

            }

        }


        public PXAction<ReserveForm> SaveMobile;
        [PXUIField(DisplayName = "SaveAction", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true)]
        protected IEnumerable saveMobile(PXAdapter adapter)
        {
            this.Actions.PressSave();
            return adapter.Get();
        }

        public PXAction<ReserveForm> Createinspection;
        [PXButton]
        [PXUIField(DisplayName = "Create Inspection Order", Enabled = true)]
        protected virtual void createinspection()
        {

            ReserveForm row = reserveForm.Current;

            // 1. Create an instance of the BLC (graph)
            InspectionFormEntry graph = PXGraph.CreateInstance<InspectionFormEntry>();

            // 2. Create an instance of the BSMTFuncLoc DAC, set key field values (besides the ones whose values are generated by the system), 
            //    and insert the record into the cache
            InspectionFormInq inse = new InspectionFormInq();
            inse.Phone = row.Phone;


            //inse.Customer = row.Customer;
            //inse.Phone = row.Phone;
            inse.Branches = row.BranchCD;
            inse.Status = row.Status;
            Items item = PXSelect<Items, Where<Items.code, Equal<Required<Items.code>>>>.Select(this, row.Vechile);
            //PXSelect<Items,Where<>>
            inse.Vehicle = item.ItemsID;


            // 3. Set non-key field values and update the record in the cache
            inse = graph.inspectionForm.Insert(inse);
            //inse = graph.inspectionForm.Update(inse);

            // 4. Redirect
            if (graph.inspectionForm.Current != null)
            {
                throw new PXRedirectRequiredException(graph, true, "Functional Location");
            }
        }




 //       [PXDBString(200, IsUnicode = true)]
 //       [PXUIField(DisplayName = "Maintenance Status", Required = true)]
 //       [PXDefault]
 //       [PXStringList(
 //new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" },
 //new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" }
 //)]

 //       public virtual void ReserveForm_MaintenanceStatus_CacheAttached(PXCache cache)
 //       {

 //       }


    }

}
