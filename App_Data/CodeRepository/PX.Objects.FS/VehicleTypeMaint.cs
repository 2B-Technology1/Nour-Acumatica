/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class VehicleTypeMaint : PXGraph<VehicleTypeMaint, FSVehicleType>
    {
        #region Select
        public PXSelect<FSVehicleType> VehicleTypeRecords;

        public PXSelect<FSVehicleType,
               Where<
                   FSVehicleType.vehicleTypeID, Equal<Current<FSVehicleType.vehicleTypeID>>>> VehicleTypeSelected;

        [PXViewName(CR.Messages.Attributes)]
        public CSAttributeGroupList<FSVehicleType, FSVehicle> Mapping;
        #endregion

        #region CacheAttached
        #region FSVehicleType_VehicleTypeCD
        [PXDefault]
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", IsFixed = true)]
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "Vehicle Type ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSVehicleType.vehicleTypeCD>))]
        protected virtual void FSVehicleType_VehicleTypeCD_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region EntityClassID
        [PXDBString(15, IsUnicode = true, IsKey = true, IsFixed = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Entity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void CSAttributeGroup_EntityClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }
}