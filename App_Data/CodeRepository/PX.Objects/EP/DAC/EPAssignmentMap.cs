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

namespace PX.Objects.EP
{

	[EPAssignmentMapPrimaryGraph]
	[Serializable]
	[PXCacheName(Messages.AssignmentMap)]
	public partial class EPAssignmentMap : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<EPAssignmentMap>.By<assignmentMapID>
		{
			public static EPAssignmentMap Find(PXGraph graph, int? assignmentMapID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, assignmentMapID, options);
		}
		#endregion

		#region AssignmentMapID
		public abstract class assignmentMapID : PX.Data.BQL.BqlInt.Field<assignmentMapID> { }
		protected Int32? _AssignmentMapID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Map ID")]
		[EPAssignmentMapSelector]
		public virtual Int32? AssignmentMapID
		{
			get
			{
				return this._AssignmentMapID;
			}
			set
			{
				this._AssignmentMapID = value;
			}
		}
		#endregion

		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		protected String _Name;
		[PXDBString(60, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Map Name", Required = false)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}
		#endregion		

		#region EntityType
		public abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		protected string _EntityType;
		[PXDBString(255)]
		[PXDefault]
		[PXUIField(DisplayName = "Entity", Required = false)]
		public virtual string EntityType
		{
			get
			{
				return this._EntityType;
			}
			set
			{
				this._EntityType = value;
			}
		}
		#endregion

        #region GraphType
        public abstract class graphType : PX.Data.BQL.BqlString.Field<graphType> { }
        protected string _GraphType;
        [PXDBString(255)]
        [PXDefault]
        [PXUIField(DisplayName = "Entity")]
        public virtual string GraphType
        {
            get
            {
                return this._GraphType;
            }
            set
            {
                this._GraphType = value;
            }
        }
		#endregion

		#region MapType
		public abstract class mapType : PX.Data.BQL.BqlInt.Field<mapType> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Map Type")]                
		[PXDefault(EPMapType.Legacy, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXIntList(new[] { 0, 1, 2 }, new[] { "Assignment and Approval Map", "Assignment Map", "Approval Map" })]
		public virtual int? MapType { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(EPAssignmentMap.assignmentMapID),
			Selector = typeof(Search<EPAssignmentMap.assignmentMapID>))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
        #endregion        
    }
	
	public static class EPMapType
	{
		public const int Legacy = 0;
		public const int Assignment = 1;
		public const int Approval = 2;

		public class legacy : PX.Data.BQL.BqlInt.Constant<legacy>
		{
			public legacy() : base(Legacy) { }
		}

		public class assignment : PX.Data.BQL.BqlInt.Constant<assignment>
		{
			public assignment() : base(Assignment) { }
		}

		public class approval : PX.Data.BQL.BqlInt.Constant<approval>
		{
			public approval() : base(Approval) { }
		}
	}

	/// <exclude/>
	public static class AssignmentMapType
	{
		public class AssignmentMapTypeLead : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeLead>
		{
			public AssignmentMapTypeLead() : base(typeof(PX.Objects.CR.CRLead).FullName) { }
		}

		public class AssignmentMapTypeContact : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeContact>
		{
			public AssignmentMapTypeContact() : base(typeof(PX.Objects.CR.Contact).FullName) { }
		}

		public class AssignmentMapTypeCase : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeCase>
		{
			public AssignmentMapTypeCase() : base(typeof(PX.Objects.CR.CRCase).FullName) { }
		}

		public class AssignmentMapTypeOpportunity : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeOpportunity>
		{
			public AssignmentMapTypeOpportunity() : base(typeof(PX.Objects.CR.CROpportunity).FullName) { }
		}

		public class AssignmentMapTypeExpenceClaim : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeExpenceClaim>
		{
			public AssignmentMapTypeExpenceClaim() : base(typeof(PX.Objects.EP.EPExpenseClaim).FullName) { }
		}
		
        public class AssignmentMapTypeTimeCard : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeTimeCard>
		{
			public AssignmentMapTypeTimeCard() : base(typeof(PX.Objects.EP.EPTimeCard).FullName) { }
		}

        public class AssignmentMapTypeEquipmentTimeCard : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeEquipmentTimeCard>
		{
            public AssignmentMapTypeEquipmentTimeCard() : base(typeof(PX.Objects.EP.EPEquipmentTimeCard).FullName) { }
        }

		public class AssignmentMapTypeSalesOrder : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeSalesOrder>
		{
			public AssignmentMapTypeSalesOrder() : base(typeof(PX.Objects.SO.SOOrder).FullName) { }
		}
		public class AssignmentMapTypeSalesOrderShipment : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeSalesOrderShipment>
		{
			public AssignmentMapTypeSalesOrderShipment() : base(typeof(PX.Objects.SO.SOShipment).FullName) { }
		}
		public class AssignmentMapTypePurchaseOrder : PX.Data.BQL.BqlString.Constant<AssignmentMapTypePurchaseOrder>
		{
			public AssignmentMapTypePurchaseOrder() : base(typeof(PX.Objects.PO.POOrder).FullName) { }
		}
		public class AssignmentMapTypePurchaseOrderReceipt : PX.Data.BQL.BqlString.Constant<AssignmentMapTypePurchaseOrderReceipt>
		{
			public AssignmentMapTypePurchaseOrderReceipt() : base(typeof(PX.Objects.PO.POReceipt).FullName) { }
		}
		public class AssignmentMapTypePurchaseRequestItem : PX.Data.BQL.BqlString.Constant<AssignmentMapTypePurchaseRequestItem>
		{
			public AssignmentMapTypePurchaseRequestItem() : base(typeof(PX.Objects.RQ.RQRequest).FullName) { }
		}
		public class AssignmentMapTypePurchaseRequisition : PX.Data.BQL.BqlString.Constant<AssignmentMapTypePurchaseRequisition>
		{
			public AssignmentMapTypePurchaseRequisition() : base(typeof(PX.Objects.RQ.RQRequisition).FullName) { }
		}
		public class AssignmentMapTypeCashTransaction : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeCashTransaction>
		{
			public AssignmentMapTypeCashTransaction() : base(typeof(PX.Objects.CA.CAAdj).FullName) { }
		}
		public class AssignmentMapTypeProspect : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeProspect>
		{
			public AssignmentMapTypeProspect() : base(typeof(PX.Objects.CR.BAccount).FullName) { }
		}
		public class AssignmentMapTypeProject : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeProject>
		{
			public AssignmentMapTypeProject() : base(typeof(PX.Objects.PM.PMProject).FullName) { }
		}
		public class AssignmentMapTypeActivity : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeActivity>
		{
			public AssignmentMapTypeActivity() : base(typeof(PX.Objects.CR.CRActivity).FullName) { }
		}
        public class AssignmentMapTypeImplementationScenario : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeImplementationScenario>
		{
             public AssignmentMapTypeImplementationScenario() : base(typeof(PX.Objects.WZ.WZScenario).FullName) { }
        }

		public class AssignmentMapTypeAPInvoice : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeAPInvoice>
		{
			public AssignmentMapTypeAPInvoice() : 
				base(typeof(PX.Objects.AP.APInvoice).FullName) { }
		}
		public class AssignmentMapTypeAPPayment : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeAPPayment>
		{
			public AssignmentMapTypeAPPayment() :
				base(typeof(PX.Objects.AP.APPayment).FullName){ }
		}
		public class AssignmentMapTypeAPQuickCheck : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeAPQuickCheck>
		{
			public AssignmentMapTypeAPQuickCheck() :
				base(typeof(PX.Objects.AP.Standalone.APQuickCheck).FullName) { }
		}
		public class AssignmentMapTypeExpenceClaimDetails : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeExpenceClaimDetails>
		{
			public AssignmentMapTypeExpenceClaimDetails() : base(typeof(PX.Objects.EP.EPExpenseClaimDetails).FullName) { }
		}
		public class AssignmentMapTypeARInvoice : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeARInvoice>
		{
			public AssignmentMapTypeARInvoice() : base(typeof(AR.ARInvoice).FullName) { }
		}
		public class AssignmentMapTypeARPayment : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeARPayment>
		{
			public AssignmentMapTypeARPayment() : base(typeof(AR.ARPayment).FullName) { }
		}
		public class AssignmentMapTypeARCashSale : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeARCashSale>
		{
			public AssignmentMapTypeARCashSale() : base(typeof(AR.Standalone.ARCashSale).FullName) { }
		}
	
		public class AssignmentMapTypeProforma : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeProforma>
		{
			public AssignmentMapTypeProforma() : base(typeof(PX.Objects.PM.PMProforma).FullName) { }
		}
		public class AssignmentMapTypeEmail : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeEmail>
		{
			public AssignmentMapTypeEmail() : base(typeof(PX.Objects.CR.CRSMEmail).FullName) { }
		}

		public class AssignmentMapTypeChangeOrder : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeChangeOrder>
		{
			public AssignmentMapTypeChangeOrder() : base(typeof(PX.Objects.PM.PMChangeOrder).FullName) { }
		}

		public class AssignmentMapTypeChangeRequest : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeChangeRequest>
		{
			public AssignmentMapTypeChangeRequest() : base(typeof(PX.Objects.PM.PMChangeRequest).FullName) { }
		}

		public class AssignmentMapTypeQuotes : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeQuotes>
		{
			public AssignmentMapTypeQuotes() : base(typeof(PX.Objects.CR.CRQuote).FullName) { }
		}

		public class AssignmentMapTypeProjectQuotes : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeProjectQuotes>
		{
			public AssignmentMapTypeProjectQuotes() : base(typeof(PX.Objects.PM.PMQuote).FullName) { }
		}

		public class AssignmentMapTypeGLBatch : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeGLBatch>
		{
			public AssignmentMapTypeGLBatch() : base(typeof(GL.Batch).FullName) { }
		}

		public class AssignmentMapTypeProgressWorksheet : PX.Data.BQL.BqlString.Constant<AssignmentMapTypeProgressWorksheet>
		{
			public AssignmentMapTypeProgressWorksheet() : base(typeof(PX.Objects.PM.PMProgressWorksheet).FullName) { }
		}
	}
}
