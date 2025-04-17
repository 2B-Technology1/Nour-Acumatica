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

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// Manufacturing cache extension to <see cref="ReasonCode"/>
    /// </summary>
    public sealed class ReasonCodeExt : PXCacheExtension<ReasonCode>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
        }

        #region Usage
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [ReasonCodeUsagesExt]
#pragma warning disable PX1002 // The field must have the type attribute corresponding to the list attribute
        public string Usage { get; set; }
#pragma warning restore PX1002
        #endregion


        /// <summary>
        /// Constant for Production Reason Code Usage
        /// </summary>
        public const string Production = "P";

        /// <summary>
        /// BQL Constant for Production Reason Code Usage
        /// </summary>
        public class production : PX.Data.BQL.BqlString.Constant<production>
        {
            public production() : base(Production) { }
        }

        /// <summary>
        /// Manufacturing extension to <see cref="ReasonCodeUsages.ListAttribute"/> adding Production to the list of usages
        /// </summary>
        public class ReasonCodeUsagesExt : ReasonCodeUsages.ListAttribute
        {
            public override void CacheAttached(PXCache sender)
            {
                base.CacheAttached(sender);

                string[] vals = new string[_AllowedValues.Length + 1];
                string[] lbls = new string[vals.Length];
                _AllowedValues.CopyTo(vals, 0);
                _AllowedLabels.CopyTo(lbls, 0);
                vals[vals.Length - 1] = Production;
                lbls[lbls.Length - 1] = AM.Messages.GetLocal(AM.Messages.Production);
                _AllowedValues = vals;
                _AllowedLabels = lbls;
            }
        }
    }
}