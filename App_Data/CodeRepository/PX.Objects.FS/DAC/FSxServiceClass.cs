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
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class FSxServiceClass : PXCacheExtension<INItemClass>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

		#region DefaultBillingRule
		public abstract class defaultBillingRule : ListField_BillingRule { }
        [PXDBString(4, IsFixed = true)]
        [defaultBillingRule.ListAttribute]
        [PXUIVisible(typeof(Where<INItemClass.itemType, Equal<INItemTypes.serviceItem>>))]
        [PXUIField(DisplayName = "Default Billing Rule")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string DefaultBillingRule { get; set; }
        #endregion
        #region RequireRoute
        public abstract class requireRoute : PX.Data.BQL.BqlBool.Field<requireRoute> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Route Service Class", FieldClass = FSRouteSetup.RouteManagementFieldClass)]
        public virtual bool? RequireRoute { get; set; }
        #endregion
        #region ChkServiceManagement
        public abstract class ChkServiceManagement : PX.Data.BQL.BqlBool.Field<ChkServiceManagement> { }
        [PXBool]
        [PXUIField(Visible = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? chkServiceManagement
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region Mem_RouteService
        // This memory field exists to show the RequireRoute value in selectors with a different header (DisplayName)
        public abstract class mem_RouteService : PX.Data.BQL.BqlBool.Field<mem_RouteService> { }
        [PXBool]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Route Service", FieldClass = FSRouteSetup.RouteManagementFieldClass)]
        public virtual bool? Mem_RouteService
        {
            get
            {
                return RequireRoute;
            }
        }
        #endregion
    }
}
