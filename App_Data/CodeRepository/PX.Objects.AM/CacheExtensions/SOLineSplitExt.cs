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
using PX.Objects.SO;

namespace PX.Objects.AM.CacheExtensions
{
	/// <summary>
	/// Manufacturing cache extension of <see cref="SOLineSplit"/>
	/// </summary>
    [Serializable]
    public sealed class SOLineSplitExt : PXCacheExtension<SOLineSplit>
    {
        // Developer note: new fields added here should also be added to SOLineSplitMfgOnly
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMProdCreate
		/// <summary>
		/// Mark for Production
		/// </summary>
		[Obsolete("Extension on field is obsolete. The field itself in SOLineSplit is not obsolete.")]
        public abstract class aMProdCreate : PX.Data.BQL.BqlBool.Field<aMProdCreate> { }

		/// <summary>
		/// Mark for Production
		/// </summary>
        [PXMergeAttributes(Method = MergeMethod.Merge)]
		[Obsolete("Extension on field is obsolete. The field itself in SOLineSplit is not obsolete.")]
		public Boolean? AMProdCreate { get; set; }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXUIField(DisplayName = "Prod. Order Type", Enabled = false)]
        public string AMOrderType { get; set; }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }

        [ProductionNbr]
        public string AMProdOrdID { get; set; }
        #endregion
        #region AMProdQtyComplete
        public abstract class aMProdQtyComplete : PX.Data.BQL.BqlDecimal.Field<aMProdQtyComplete> { }
        [PXDBQuantity]
        [PXUIField(DisplayName = "Production Qty Complete", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMProdQtyComplete { get; set; }
        #endregion
        #region AMProdBaseQtyComplete
        public abstract class aMProdBaseQtyComplete : PX.Data.BQL.BqlDecimal.Field<aMProdBaseQtyComplete> { }

        [PXDBQuantity]
        [PXUIField(DisplayName = "Production Base Qty Complete", Enabled = false, Visible = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public Decimal? AMProdBaseQtyComplete { get; set; }
        #endregion
        #region AMProdStatusID
        public abstract class aMProdStatusID : PX.Data.BQL.BqlString.Field<aMProdStatusID> { }

        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Production Status", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        [ProductionOrderStatus.List]
        public String AMProdStatusID { get; set; }
        #endregion
    }
}
