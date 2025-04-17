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

using PX.Objects.PJ.Submittals.PJ.Descriptor;
using PX.Data;
using PX.Data.EP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PJ.Submittals.PJ.DAC
{
    public class SubmittalReportParameters : IBqlTable
    {
        #region SubmittalID
        public abstract class submittalID : PX.Data.BQL.BqlInt.Field<submittalID> { }

        [PXFieldDescription]
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [SubmittalIDOfLastRevisionSelector(ValidateValue = false)]
        [PXUIField(DisplayName = "Submittal ID",
            Visibility = PXUIVisibility.SelectorVisible,
            Required = true)]
        [PXDefault]
        public string SubmittalID { get; set; }
        #endregion

        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        [PXDBInt(IsKey = true)]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Revision ID",
            Visibility = PXUIVisibility.SelectorVisible,
            Required = true)]
        [PXFieldDescription]
        [SubmittalRevisionIDSelector(typeof(submittalID))]
        public virtual int? RevisionID
        {
            get;
            set;
        }
        #endregion
    }
}
