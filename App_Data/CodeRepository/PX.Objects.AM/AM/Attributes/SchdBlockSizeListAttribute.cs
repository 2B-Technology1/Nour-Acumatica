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

using System.Linq;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class SchdBlockSizeListAttribute : PXIntListAttribute
    {
        public SchdBlockSizeListAttribute()
            : base(
                new int[] { 5, 10, 15, 30, 60 },
                new string[] { "00:05", "00:10", "00:15", "00:30", "01:00" }){ }

        /// <summary>
        /// Is the block size a standard block size value?
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static bool Contains(int? blockSize)
        {
            var intList = new SchdBlockSizeListAttribute();
            return intList._AllowedValues.Contains(blockSize.GetValueOrDefault());
        }
    }
}