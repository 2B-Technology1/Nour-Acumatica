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

namespace PX.Objects.CN.Subcontracts.PO.DAC
{
    [PXHidden]
    public class PurchaseOrderTypeFilter : IBqlTable
    {
        [PXString]
        public string Type1
        {
            get;
            set;
        }

        [PXString]
        public string Type2
        {
            get;
            set;
        }

        [PXString]
        public string Type3
        {
            get;
            set;
        }

        [PXString]
        public string Type4
        {
            get;
            set;
        }

        [PXString]
        public string Type5
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(Visibility = PXUIVisibility.Invisible)]
        public virtual string Graph
        {
            get;
            set;
        }

        public abstract class graph : IBqlField
        {
        }

        public abstract class type1 : IBqlField
        {
        }

        public abstract class type2 : IBqlField
        {
        }

        public abstract class type3 : IBqlField
        {
        }

        public abstract class type4 : IBqlField
        {
        }

        public abstract class type5 : IBqlField
        {
        }
    }
}