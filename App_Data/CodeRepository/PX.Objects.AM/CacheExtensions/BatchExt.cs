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
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// Manufacturing Cache Extension for <see cref="PX.Objects.GL.Batch"/>
    /// </summary>
    [Serializable]
    public sealed class BatchExt : PXCacheExtension<GL.Batch>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMDocType
        /// <summary>
        /// Manufacturing Document Type
        /// </summary>
        public abstract class aMDocType : PX.Data.BQL.BqlString.Field<aMDocType> { }
        /// <summary>
        /// Manufacturing Document Type
        /// </summary>
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "MFG Document Type", Visible = true, Enabled = false)]
        [AMDocType.List]
        public string AMDocType { get; set; }
        #endregion
        #region AMBatNbr
        /// <summary>
        /// Manufacturing Batch Number
        /// </summary>
        public abstract class aMBatNbr : PX.Data.BQL.BqlString.Field<aMBatNbr> { }
        /// <summary>
        /// Manufacturing Batch Number
        /// </summary>
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "MFG Batch Nbr", Visible = true, Enabled = false)]
        [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<Current<BatchExt.aMDocType>>>>), ValidateValue = false)]
        public string AMBatNbr { get; set; }
        #endregion
    }
}