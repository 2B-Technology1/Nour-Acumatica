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
using PX.Objects.CM;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FSContractPeriodFilter : PX.Data.IBqlTable
    {
        #region Actions
        public abstract class actions : ListField_ContractPeriod_Actions
        {
        }

        [PXString(3, IsFixed = true)]
        [actions.ListAtrribute]
        [PXUIField(DisplayName = "Actions")]
        public virtual string Actions { get; set; }
        #endregion    
        #region ContractPeriodID
        public abstract class contractPeriodID : PX.Data.BQL.BqlInt.Field<contractPeriodID> { }

        [PXInt]
        [PXUIField(DisplayName = "Billing Period")]
        [FSSelectorContractBillingPeriod]
        [PXDefault(typeof(Search<FSContractPeriod.contractPeriodID,
                            Where2<
                                Where<Current<FSContractPeriodFilter.actions>, Equal<FSContractPeriodFilter.actions.ModifyBillingPeriod>,
                                    And<FSContractPeriod.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>,
                                    And<FSContractPeriod.status, Equal<FSContractPeriod.status.Inactive>>>>,
                                Or<Where<Current<FSContractPeriodFilter.actions>, Equal<FSContractPeriodFilter.actions.SearchBillingPeriod>,
                                        And<FSContractPeriod.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>,
                                        And<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>>>,
                            OrderBy<Desc<FSContractPeriod.startPeriodDate>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        
        public virtual int? ContractPeriodID { get; set; }
        #endregion
        #region StandardizedBillingTotal
        public abstract class standardizedBillingTotal : PX.Data.BQL.BqlDecimal.Field<standardizedBillingTotal> { }

        [PXBaseCury]
        [PXUIField(DisplayName = "Contract Total", IsReadOnly = true)]
        public virtual decimal? StandardizedBillingTotal { get; set; }
        #endregion
        #region PostDocRefNbr
        public abstract class postDocRefNbr : PX.Data.BQL.BqlString.Field<postDocRefNbr> { }

        [PXString]
        [PXUIField(DisplayName = "Reference Nbr.", IsReadOnly = true)]
        public virtual string PostDocRefNbr { get; set; }
        #endregion
    }
}
