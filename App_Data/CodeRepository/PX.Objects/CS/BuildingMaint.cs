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
using PX.Objects.GL;
using System;

namespace PX.Objects.CS
{
    public class BuildingMaint : PXGraph<BuildingMaint, BuildingMaint.BuildingFilter>
    {
        [Serializable]
        public partial class BuildingFilter : IBqlTable
        {
            public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

            [Branch(typeof(AccessInfo.branchID), IsDBField = false)]
            public int? BranchID { get; set; }
        }

        public PXFilter<BuildingFilter> filter;
        public PXSelect<Building, Where<Building.branchID, Equal<Current<BuildingFilter.branchID>>>> building;
    }
}