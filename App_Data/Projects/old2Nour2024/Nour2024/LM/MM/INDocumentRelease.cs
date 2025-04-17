//#region Assembly PX.Objects.dll, v1.0.0.0
//// E:\ERP projects\Modules\Bin\PX.Objects.dll
//#endregion
//using PX.Data;
//using PX.Objects.GL;
//using PX.Objects.IN;
//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace MyMaintaince
//{
//    [TableAndChartDashboardType]
//    public class INDocumentRelease : PXGraph<INDocumentRelease>
//    {
//        public PXCancel<INRegister> Cancel;
//        public PXProcessing<INRegister, Where<INRegister.released, Equal<PX.Objects.CS.boolFalse>, And<INRegister.hold, Equal<PX.Objects.CS.boolFalse>>>> INDocumentList;
//        public PXSetup<INSetup> insetup;
//        public PXAction<INRegister> viewDocument;
//        public INDocumentRelease();
//        public static void ReleaseDoc(List<INRegister> list, bool isMassProcess);
//        [PXLookupButton]
//        [PXUIField(DisplayName = "View Document")]
//        protected virtual IEnumerable ViewDocument(PXAdapter adapter);
//    }
//}
