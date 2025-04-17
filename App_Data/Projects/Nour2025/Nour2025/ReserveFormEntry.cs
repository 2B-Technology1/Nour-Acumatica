using System;
using MyMaintaince;
using PX.Data;

namespace Nour2024
{
    public class ReservationFormMaint : PXGraph<ReservationFormMaint, ReserveForm>
    {

        public PXSelect<ReserveForm> reserveForm;

        public PXSave<MasterTable> Save;
        public PXCancel<MasterTable> Cancel;


        public PXFilter<MasterTable> MasterView;
        public PXFilter<DetailsTable> DetailsView;

        [Serializable]
        public class MasterTable : IBqlTable
        {

        }

        [Serializable]
        public class DetailsTable : IBqlTable
        {

        }


    }
}