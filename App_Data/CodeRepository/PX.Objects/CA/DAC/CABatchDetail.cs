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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.TM;


namespace PX.Objects.CA
{
    /// <summary>
    /// A link between <see cref="CABatch"/> and <see cref="AP.APPayment"/>.
    /// </summary>
    [PXCacheName(Messages.CABatchDetail)]
    [Serializable]
	public partial class CABatchDetail : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CABatchDetail>.By<batchNbr, origModule, origDocType, origRefNbr, origLineNbr>
		{
			public static CABatchDetail Find(PXGraph graph, string batchNbr, string origModule, string origDocType, string origRefNbr, string origLineNbr, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, batchNbr, origModule, origDocType, origRefNbr, origLineNbr, options);
		}

		public static class FK
		{
			public class CashAccountBatch : CA.CABatch.PK.ForeignKeyOf<CABatchDetail>.By<batchNbr> { }
			public class APPayment : AP.APPayment.PK.ForeignKeyOf<CABatchDetail>.By<origDocType, origRefNbr> { }
		}

		#endregion

		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }

        /// <summary>
        /// This field is the key field.
        /// Corresponds to the <see cref="CABatch.BatchNbr"/> field.
        /// </summary>
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDBDefault(typeof(CABatch.batchNbr))]
		[PXParent(typeof(Select<CABatch, Where<CABatch.batchNbr, Equal<Current<CABatchDetail.batchNbr>>>>))]

		public virtual string BatchNbr
		{
			get;
			set;
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }

		/// <summary>
		/// This field is a part of the compound key of the document.
		/// It is either equals to <see cref="GL.BatchModule.AP"/> or <see cref="GL.BatchModule.PR"/>in current implementation.
		/// Potentially it may be equal to <see cref="GL.BatchModule.AR"/>.
		/// </summary>
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault(GL.BatchModule.AP)]
		[PXStringList(new string[] { GL.BatchModule.AP, GL.BatchModule.AR, GL.BatchModule.PR }, new string[] { BatchModule.AP, BatchModule.AR, BatchModule.PR })]
		[PXUIField(DisplayName = "Module", Enabled = false)]
		public virtual string OrigModule
		{
			get;
			set;
		}
		#endregion
		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

        /// <summary>
        /// The type of payment document.
        /// This field is a part of the compound key of the document.
        /// Corresponds to the <see cref="PX.Objects.AP.APRegister.DocType"/> field and the <see cref="PX.Objects.AP.APPayment.DocType"/> field.
        /// </summary>
		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Type")]
		public virtual string OrigDocType
		{
			get;
			set;
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

        /// <summary>
        /// The payment's reference number.
        /// This number is a link to payment document on the Checks and Payments (AP302000) form.
        /// This field is a part of the compound key of the document.
        /// Corresponds to the <see cref="PX.Objects.AP.APRegister.RefNbr"/> field and the <see cref="PX.Objects.AP.APPayment.RefNbr"/> field.
        /// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public virtual string OrigRefNbr
		{
			get;
			set;
		}
		#endregion
		#region OrigLineNbr
		/// <summary>
		/// Key field used to differentiate between Direct Deposit splits in PR module. For other modules, it isn't necessary and defaults to 0.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		[PXDefault(origLineNbr.DefaultValue)]
		public virtual int? OrigLineNbr { get; set; }
		public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> 
		{
			public const int DefaultValue = 0;

			public class defaultValue : PX.Data.BQL.BqlInt.Constant<defaultValue>
			{
				public defaultValue() : base(DefaultValue) {; }
			}
		}
		#endregion
		#region AddendaPaymentRelatedInfo
		public abstract class addendaPaymentRelatedInfo : PX.Data.BQL.BqlString.Field<addendaPaymentRelatedInfo> { }

		[PXDBString(80)]
		[PXUIField(DisplayName = "Payment-Related Info (Addenda)")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string AddendaPaymentRelatedInfo { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]		
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		public virtual void Copy(AP.APPayment payment)
		{
			this.OrigRefNbr = payment.RefNbr;
			this.OrigDocType = payment.DocType;
			this.OrigModule = GL.BatchModule.AP;
		}

		public virtual void Copy(AR.ARPayment payment)
		{
			this.OrigRefNbr = payment.RefNbr;
			this.OrigDocType = payment.DocType;
			this.OrigModule = GL.BatchModule.AR;
		}
	}
}
