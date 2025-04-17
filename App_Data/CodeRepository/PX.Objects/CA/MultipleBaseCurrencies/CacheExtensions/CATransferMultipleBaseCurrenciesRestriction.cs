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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;

namespace PX.Objects.CA
{
    public sealed class CATransferMultipleBaseCurrenciesRestriction : PXCacheExtension<CATransfer>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXRestrictor(typeof(Where<Current2<CATransfer.inAccountID>, IsNull,
                Or<CashAccount.baseCuryID, Equal<Current<inAccountCuryID>>>>), null)]
        public int? OutAccountID { get; set; }
        
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXRestrictor(typeof(Where<Current2<CATransfer.outAccountID>, IsNull,
            Or<CashAccount.baseCuryID, Equal<Current<outAccountCuryID>>>>), null)]
        public int? InAccountID { get; set; }
        
        #region InAccountCuryID
        public new abstract class inAccountCuryID : PX.Data.BQL.BqlString.Field<inAccountCuryID> { }

        [PXString]
        [PXFormula(typeof(Selector<CATransfer.inAccountID, CashAccount.baseCuryID>))]
        public string InAccountCuryID { get; set; }
        #endregion
        #region OutAccountCuryID
        public new abstract class outAccountCuryID : PX.Data.BQL.BqlString.Field<outAccountCuryID> { }

        [PXString]
        [PXFormula(typeof(Selector<CATransfer.outAccountID, CashAccount.baseCuryID>))]
        public string OutAccountCuryID { get; set; }
        #endregion
    }
}