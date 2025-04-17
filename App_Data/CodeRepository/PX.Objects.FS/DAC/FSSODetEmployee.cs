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
using PX.Objects.IN;
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [Serializable]
    [PXPrimaryGraph(typeof(ServiceOrderEntry))]
    [PXBreakInheritance]
    [PXProjection(typeof(Select<FSSODet>), Persistent = false)]
    public class FSSODetEmployee : FSSODet
    {
        #region Keys
        public new class PK : PrimaryKeyOf<FSSODetEmployee>.By<srvOrdType, refNbr, lineNbr>
        {
            public static FSSODetEmployee Find(PXGraph graph, string srvOrdType, string refNbr, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, srvOrdType, refNbr, lineNbr, options);
        }
        #endregion

        public new abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        public new abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        public new abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }
    }
}