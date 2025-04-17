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

using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// Manufacturing Employee Extension
    /// </summary>
    [Serializable]
    public sealed class EPEmployeeExt : PXCacheExtension<EPEmployee>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMProductionEmployee
        /// <summary>
        /// Indicates an employee is a production employee for use in Manufacturing processes such as 
        /// labor transactions and scheduling as a resource
        /// </summary>
        public abstract class amProductionEmployee : PX.Data.BQL.BqlBool.Field<amProductionEmployee> { }

        /// <summary>
        /// Indicates an employee is a production employee for use in Manufacturing processes such as 
        /// labor transactions and scheduling as a resource
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Production Employee")]
        public Boolean? AMProductionEmployee { get; set; }
        #endregion
    }
}
