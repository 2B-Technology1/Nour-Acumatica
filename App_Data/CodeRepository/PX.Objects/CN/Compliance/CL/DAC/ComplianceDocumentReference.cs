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
using PX.Objects.AP;
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [PXCacheName("Compliance Document Reference")]
    public class ComplianceDocumentReference : BaseCache, IBqlTable
    {
        #region ComplianceDocumentReferenceId
        public abstract class complianceDocumentReferenceId : PX.Data.BQL.BqlGuid.Field<complianceDocumentReferenceId>
        {
        }

        [PXDBGuid(IsKey = true)]
        public virtual Guid? ComplianceDocumentReferenceId
        {
            get;
            set;
        }
        #endregion

        #region Type
        public abstract class type : PX.Data.BQL.BqlString.Field<type>
        {
        }

        [PXDBString]
        public virtual string Type
        {
            get;
            set;
        }
        #endregion

        #region ReferenceNumber

        public abstract class referenceNumber : PX.Data.BQL.BqlString.Field<referenceNumber>
        {
        }

        [PXDBString]
        public virtual string ReferenceNumber
        {
            get;
            set;
        }
        #endregion

        #region RefNoteId
        public abstract class refNoteId : PX.Data.BQL.BqlGuid.Field<refNoteId>
        {
        }
        [PXDBGuid]
        public virtual Guid? RefNoteId
        {
            get;
            set;
        } 
        #endregion

        [PXDBCreatedByID(Visibility = PXUIVisibility.Invisible)]
        public override Guid? CreatedById
        {
            get;
            set;
		}
	}

	[PXHidden]
	[PXBreakInheritance]
	public class ComplianceDocumentPaymentReference : ComplianceDocumentReference
	{
		public new abstract class complianceDocumentReferenceId : PX.Data.BQL.BqlGuid.Field<complianceDocumentReferenceId>
		{
		}


		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
		}
		[PXDefault(typeof(APPayment.docType))]
		[PXDBString]
		public override string Type
		{
			get;
			set;
		}
		#endregion

		#region ReferenceNumber

		public new abstract class referenceNumber : PX.Data.BQL.BqlString.Field<referenceNumber>
		{
		}

		[PXDBDefault(typeof(APPayment.refNbr))]
		[PXDBString]
		public override string ReferenceNumber
		{
			get;
			set;
		}
		#endregion

		#region RefNoteId
		public new abstract class refNoteId : PX.Data.BQL.BqlGuid.Field<refNoteId>
		{
		}
		[PXDefault(typeof(APPayment.noteID))]
		[PXDBGuid]
		public override Guid? RefNoteId
		{
			get;
			set;
		}
		#endregion
	}

}
