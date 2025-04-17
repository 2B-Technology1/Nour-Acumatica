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
using PX.Objects.AR;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    [PXPrimaryGraph(typeof(CustomerMaintBridge))]
    // TODO: AC-137974 Delete this DAC
    public partial class FSCustomer : Customer
    {
        #region Keys
        public new class PK : PrimaryKeyOf<FSCustomer>.By<acctCD>
        {
            public static FSCustomer Find(PXGraph graph, string acctCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, acctCD, options);
        }
        #endregion

        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        public new abstract class cOrgBAccountID : PX.Data.BQL.BqlInt.Field<cOrgBAccountID> { }
    }
}
