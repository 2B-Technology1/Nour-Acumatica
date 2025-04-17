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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Operation CD field attribute
    /// </summary>
    [PXDBString(OperationFieldLength, IsUnicode = true, InputMask = "#####")]
    [PXUIField(DisplayName = "Operation ID")]
    public class OperationCDFieldAttribute : PXEntityAttribute, IPXFieldUpdatingSubscriber
    {
        /// <summary>
        /// Database field size
        /// </summary>
        public const int OperationFieldLength = 10;

        /// <summary>
        /// Operations are masked to 5 numbers "#####"
        /// </summary>
        internal const int OperationMaskLength = 5;

        public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Convert.ToString(e.NewValue)))
            {
                return;
            }
            // Prevent users from entering leading spaces...
            e.NewValue = Convert.ToString(e.NewValue).TrimStart();
        }
    }
}
