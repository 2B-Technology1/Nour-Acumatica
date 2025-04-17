//using MyMaintaince;
//using PX.Data;
//using PX.Data.Licensing;
//using PX.Objects.AR;
//using System;
//using System.Collections;

//namespace Nour20231012VSolveUSDNew
//{
//    public class ReserveFormMaint1 : PXGraph<ReserveFormMaint1, ReserveForm>
//    {

//        public PXSelect<ReserveForm> reserveForm;


//        public virtual void ReserveForm_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
//        {


//            try
//            {
//                if (e.Row != null)
//                {
//                    if (e.Operation == PXDBOperation.Insert)
//                    {
//                        if (this.reserveForm.Current.RefNbr == "<NEW>")
//                        {
//                            SetUp result = PXSelect<SetUp, Where<SetUp.branchCD, Equal<Required<ReserveForm.branchCD>>>>.Select(this, this.reserveForm.Current.BranchCD);
//                            if (result != null)
//                            {
//                                string lastNumber = result.ReserveRefNbr;
//                                char[] symbols = lastNumber.ToCharArray();
//                                for (int i = symbols.Length - 1; i >= 0; i--)
//                                {
//                                    if (!char.IsDigit(symbols[i]))
//                                        break;
//                                    if (symbols[i] < '9')
//                                    {
//                                        symbols[i]++;
//                                        break;
//                                    }
//                                    symbols[i] = '0';
//                                }

//                                this.reserveForm.Current.RefNbr = new string(symbols);
//                                this.ProviderUpdate<SetUp>(new PXDataFieldAssign("ReserveRefNbr", this.reserveForm.Current.RefNbr), new PXDataFieldRestrict("branchCD", this.reserveForm.Current.BranchCD));
//                            }
//                            else
//                            {
//                                e.Cancel = true;
//                                throw new PXException("Please Define auto Numbering For the Selected Branch !");
//                            }

//                        }
//                        else
//                        {
//                        }

//                    }
//                }
//            }
//            catch { }
//            //reserveForm.View.RequestRefresh();


//        }
//        public virtual void ReserveForm_customer_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
//        {
//            ReserveForm row = e.Row as ReserveForm;
//            Customer c = PXSelect<Customer, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>.Select(this, e.NewValue);
//            Contact c2 = PXSelect<Contact, Where<Contact.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, c.BAccountID);
//            this.reserveForm.Current.Name = c.AcctName;
//            this.reserveForm.Current.Phone = c2.Phone1;
//            ;
//        }
//        protected virtual void ReserveForm_Vechile_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
//        {

//            if (e.Row != null)
//            {
//                ReserveForm jobOrder = e.Row as ReserveForm;

//                if (!String.IsNullOrEmpty(jobOrder.Vechile + ""))
//                {
//                    string itemID = (string)jobOrder.Vechile;
//                    PXSelectBase<Items> ChassisBase = new PXSelectReadonly<Items, Where<Items.code, Equal<Required<Items.code>>>>(this);
//                    ChassisBase.Cache.ClearQueryCache();
//                    Items Chassis = ChassisBase.Select(itemID);
//                    sender.SetValue<ReserveForm.licensePlate>(jobOrder, Chassis.LincensePlat);
//                    sender.SetValue<ReserveForm.brandName>(jobOrder, Chassis.BrandID);
//                    sender.SetValue<ReserveForm.modelName>(jobOrder, Chassis.ModelID);
//                }
//            }

//        }
//        protected void ReserveForm_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
//        {
//            ReserveForm jobOrder = e.Row as ReserveForm;

//            if (e.Row != null)
//            {

//                if (!String.IsNullOrEmpty(jobOrder.Vechile + ""))
//                {
//                    string itemID = (string)jobOrder.Vechile;
//                    PXSelectBase<Items> ChassisBase = new PXSelectReadonly<Items, Where<Items.code, Equal<Required<Items.code>>>>(this);
//                    ChassisBase.Cache.ClearQueryCache();
//                    // Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [Justification]
//                    Items Chassis = ChassisBase.Select(itemID);
//                    sender.SetValue<ReserveForm.licensePlate>(jobOrder, Chassis.LincensePlat);
//                    sender.SetValue<ReserveForm.brandName>(jobOrder, Chassis.BrandID);
//                    sender.SetValue<ReserveForm.modelName>(jobOrder, Chassis.ModelID);
//                }
//            }
//        }
//        protected void ReserveForm_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
//        {
//            ReserveForm jobOrder = e.Row as ReserveForm;

//            if (e.Row != null)
//            {

//                if (!String.IsNullOrEmpty(jobOrder.Vechile + ""))
//                {
//                    string itemID = (string)jobOrder.Vechile;
//                    PXSelectBase<Items> ChassisBase = new PXSelectReadonly<Items, Where<Items.code, Equal<Required<Items.code>>>>(this);
//                    ChassisBase.Cache.ClearQueryCache();
//                    Items Chassis = ChassisBase.Select(itemID);
//                    sender.SetValue<ReserveForm.licensePlate>(jobOrder, Chassis.LincensePlat);
//                    sender.SetValue<ReserveForm.brandName>(jobOrder, Chassis.BrandID);
//                    sender.SetValue<ReserveForm.modelName>(jobOrder, Chassis.ModelID);
//                }
//            }
//        }



//        public PXAction<ReserveForm> SaveMobile;
//        [PXUIField(DisplayName = "SaveAction", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
//        [PXButton(CommitChanges = true)]
//        protected IEnumerable saveMobile(PXAdapter adapter)
//        {
//            this.Actions.PressSave();
//            return adapter.Get();
//        }
//        public PXAction<ReserveForm> Createinspection;
//        [PXButton]
//        [PXUIField(DisplayName = "Create Inspection Order", Enabled = true)]
//        protected virtual void createinspection()
//        {

//            ReserveForm row = reserveForm.Current;

//            // 1. Create an instance of the BLC (graph)
//            InspectionFormEntryN graph = PXGraph.CreateInstance<InspectionFormEntryN>();

//            // 2. Create an instance of the BSMTFuncLoc DAC, set key field values (besides the ones whose values are generated by the system), 
//            //    and insert the record into the cache
//            InspectionFormMaster inse = new InspectionFormMaster();
//            if (row.Phone != null)
//            {
//                inse.Phone = row.Phone;
//            }
//            if (row.RefNbr != "<NEW>")
//            {
//                //TODO:
//                //replace RearBrakeLining with the new field that will hold the refnbr hyperlink
//                inse.ReserveID = row.RefNbr;
//            }


//            //inse.Customer = row.Customer;
//            //inse.Phone = row.Phone;
//            if (row.BranchCD != null)
//            {
//                inse.Branches = row.BranchCD;
//            }
//            //if (row.Status != null)
//            //{
//            //    inse.Status = row.Status;
//            //}
//            if (row.Vechile != null)
//            {
//                Items item = PXSelect<Items, Where<Items.code, Equal<Required<Items.code>>>>.Select(this, row.Vechile);
//                //PXSelect<Items,Where<>>
//                inse.Vehicle = item.ItemsID;
//            }


//            // 3. Set non-key field values and update the record in the cache
//            inse = graph.inspectionFormView.Insert(inse);
//            //inse = graph.inspectionForm.Update(inse);

//            // 4. Redirect
//            if (graph.inspectionFormView.Current != null)
//            {
//                throw new PXRedirectRequiredException(graph, true, "Functional Location");
//            }
//        }

//        //protected virtual void ReserveForm_Descrption2_CacheAttached(PXCache cache)
//        //{
//        //    PXStringListAttribute.SetList<ReserveForm.descrption2>(cache, null,
//        //        new string[] {
//        //            "S of 1000 K.M",
//        //            "S of 5.000 K.M",
//        //            "S of 10.000 K.M",
//        //            "S of 20.000 K.M",
//        //            "S of 30.000 K.M",
//        //            "S of 40.000 K.M",
//        //            "S of 50.000 K.M",
//        //            "S of 60.000 K.M",
//        //            "S of 70.000 K.M",
//        //            "S of 80.000 K.M",
//        //            "S of 90.000 K.M",
//        //            "S of 100.000 K.M",
//        //            "S of 110.000 K.M",
//        //            "S of 120.000 K.M",
//        //            "S of 130.000 K.M",
//        //            "S of 140000 K.M",
//        //            "S of 150000.000 K.M",
//        //            "S of 160000 K.M",
//        //            "S of 1700000 K.M",
//        //            "S of 180.000 K.M",
//        //            "S of 190000 K.M",
//        //            "S of 200000 K.M",
//        //            "تغيير زيت وفلتر",
//        //            "الكشف علي الكهرباء",
//        //            "فحص مشكلة فنية",
//        //            "الكشف علي العفشة",
//        //            "فحص سمكرة ودهان",
//        //            "تقرير فحص شامل للسيارة",
//        //            "فحص عيب ظهر بعد الاصلاح او الصيانة",
//        //            "حجز عرض للصيانة او الاصلاح",
//        //            "ضبط زوايا واتزان للسيارة",
//        //            "ترصيص عجل وفحص الكاوتش",
//        //            "فحص عيب بالخامات الداخلية او الخارجية",
//        //            "تغيير زجاج للسيارة",
//        //            "تركيب قطعة خاصة بالضمان"
//        //        },
//        //        new string[] {
//        //            "S of 1000 K.M",
//        //            "S of 5.000 K.M",
//        //            "S of 10.000 K.M",
//        //            "S of 20.000 K.M",
//        //            "S of 30.000 K.M",
//        //            "S of 40.000 K.M",
//        //            "S of 50.000 K.M",
//        //            "S of 60.000 K.M",
//        //            "S of 70.000 K.M",
//        //            "S of 80.000 K.M",
//        //            "S of 90.000 K.M",
//        //            "S of 100.000 K.M",
//        //            "S of 110.000 K.M",
//        //            "S of 120.000 K.M",
//        //            "S of 130.000 K.M",
//        //            "S of 140000 K.M",
//        //            "S of 150000.000 K.M",
//        //            "S of 160000 K.M",
//        //            "S of 1700000 K.M",
//        //            "S of 180.000 K.M",
//        //            "S of 190000 K.M",
//        //            "S of 200000 K.M",
//        //            "تغيير زيت وفلتر",
//        //            "الكشف علي الكهرباء",
//        //            "فحص مشكلة فنية",
//        //            "الكشف علي العفشة",
//        //            "فحص سمكرة ودهان",
//        //            "تقرير فحص شامل للسيارة",
//        //            "فحص عيب ظهر بعد الاصلاح او الصيانة",
//        //            "حجز عرض للصيانة او الاصلاح",
//        //            "ضبط زوايا واتزان للسيارة",
//        //            "ترصيص عجل وفحص الكاوتش",
//        //            "فحص عيب بالخامات الداخلية او الخارجية",
//        //            "تغيير زجاج للسيارة",
//        //            "تركيب قطعة خاصة بالضمان"
//        //        });
//        //}




//    }
//}