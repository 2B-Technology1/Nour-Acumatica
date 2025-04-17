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
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.CacheExtensions
{
    [Serializable]
    public sealed class INTranExt : PXCacheExtension<INTran>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMDocType

        public abstract class aMDocType : PX.Data.BQL.BqlString.Field<aMDocType> { }

        [PXDBString(1, IsFixed = true)]
        [AM.Attributes.AMDocType.List]
        [PXUIField(DisplayName = "MFG Doc. Type", Visible = false, Enabled = false)]
        public String AMDocType { get; set; }
        #endregion
        #region AMBatNbr
        public abstract class aMBatNbr : PX.Data.BQL.BqlString.Field<aMBatNbr> { }

        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "MFG Batch Nbr", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<AM.AMBatch.batNbr, Where<AM.AMBatch.docType, Equal<Current<INTranExt.aMDocType>>>>), ValidateValue = false)]
        public String AMBatNbr { get; set; }
        #endregion
        #region AMLineNbr
        public abstract class aMLineNbr : PX.Data.BQL.BqlInt.Field<aMLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "MFG Line Nbr.", Visible = false, Enabled = false)]
        public Int32? AMLineNbr { get; set; }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXUIField(DisplayName = "Prod. Order Type", Enabled = false)]
        public string AMOrderType { get; set; }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }

        [ProductionNbr(Enabled = false)]
        [ProductionOrderSelector(typeof(INTranExt.aMOrderType), includeAll: true, ValidateValue = false)]
        public String AMProdOrdID { get; set; }
        #endregion
    }
}
