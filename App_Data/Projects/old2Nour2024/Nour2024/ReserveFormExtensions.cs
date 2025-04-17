//using MyMaintaince;
//using PX.Data;
//using PX.Objects;
//using System;

//namespace MyMaintaince
//{
//    public class ReserveFormExt : PXCacheExtension<MyMaintaince.ReserveForm>
//    {
//        #region UsrMaintenanceStatus
//        [PXDBString(100,IsFixed =true,IsUnicode =true)]
//        [PXUIField(DisplayName = "NEW Maintenance Status",Enabled =false)]
//        [PXDefault]
//        [PXStringList(
//                new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" },
//                new string[] { "S of 1000 K.M", "S of 5000 K.M", "S of 10000 K.M", "S of 20000 K.M", "S of 30000 K.M", "S of 40000 K.M", "S of 50000 K.M", "S of 60000 K.M", "S of 70000 K.M", "S of 80000 K.M", "S of 90000 K.M", "S of 100000 K.M", "S of 110000 K.M", "S of 120000 K.M", "S of 130000 K.M", "S of 140000 K.M", "S of 150000 K.M", "S of 160000 K.M", "S of 170000 K.M", "S of 180000 K.M", "S of 190000 K.M", "S of 200000 K.M", "تغيير زيت وفلتر", "الكشف علي الكهرباء", "فحص مشكلة فنية", "الكشف علي العفشة", "فحص سمكرة ودهان", "تقرير فحص شامل للسيارة", "فحص عيب ظهر بعد الاصلاح او الصيانة", "حجز عرض للصيانة او الاصلاح", "ضبط زوايا واتزان للسيارة", "ترصيص عجل وفحص الكاوتش", "فحص عيب بالخامات الداخلية او الخارجية", "تغيير زجاج للسيارة", "تركيب قطعة خاصة بالضمان" }
//                )]

//        public virtual string UsrMaintenanceStatus { get; set; }
//        public abstract class usrMaintenanceStatus : PX.Data.BQL.BqlString.Field<usrMaintenanceStatus> { }
//        #endregion
//    }
//}