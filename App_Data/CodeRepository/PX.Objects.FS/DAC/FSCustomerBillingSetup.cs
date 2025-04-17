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
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.FSCustomerBillingSetup)]
    public class FSCustomerBillingSetup : PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSCustomerBillingSetup>.By<cBID>
        {
            public static FSCustomerBillingSetup Find(PXGraph graph, int? cBID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, cBID, options);
        }

        public class UK : PrimaryKeyOf<FSCustomerBillingSetup>.By<customerID, cBID>
        {
            public static FSCustomerBillingSetup Find(PXGraph graph, int? customerID, int? cBID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, customerID, cBID, options);
        }

        public static class FK
        {
            public class Customer : AR.Customer.PK.ForeignKeyOf<FSCustomerBillingSetup>.By<customerID> { }
            public class ServiceOrderType : FSSrvOrdType.PK.ForeignKeyOf<FSCustomerBillingSetup>.By<srvOrdType> { }
            public class BillingCycle : FSBillingCycle.PK.ForeignKeyOf<FSCustomerBillingSetup>.By<billingCycleID> { }
        }
        #endregion

        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt]
		[PXCheckUnique(new Type[] { typeof(srvOrdType) })]
        [PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<FSCustomerBillingSetup.customerID>>>>))]
        [PXDBDefault(typeof(Customer.bAccountID))]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region CBID
        public abstract class cBID : PX.Data.BQL.BqlInt.Field<cBID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? CBID { get; set; }
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		/// <summary>
		/// Non-used field
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active { get; set; }
		#endregion
		#region SrvOrdType
		public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, InputMask = ">AAAA")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRestrictor(typeof(Where<FSSrvOrdType.active, Equal<True>>), null)]
		[FSSelectorSrvOrdTypeNOTQuoteInternal]
        [PXUIField(DisplayName = "Service Order Type")]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region BillingCycleID
        public abstract class billingCycleID : PX.Data.BQL.BqlInt.Field<billingCycleID> { }

        [PXDBInt]
        [PXDefault]
        [PXSelector(
                    typeof(Search<FSBillingCycle.billingCycleID>),
                    SubstituteKey = typeof(FSBillingCycle.billingCycleCD),
                    DescriptionField = typeof(FSBillingCycle.descr))]
        [PXUIField(DisplayName = "Billing Cycle")]
        public virtual int? BillingCycleID { get; set; }
        #endregion
        #region FrequencyType
        public abstract class frequencyType : ListField_Frequency_Type
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Frequency_Type.NONE)]
        [frequencyType.ListAtrribute]
        [PXUIField(DisplayName = "Frequency Type")]
        public virtual string FrequencyType { get; set; }
        #endregion
        #region WeeklyFrequency
        public abstract class weeklyFrequency : ListField_WeekDaysNumber
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Frequency Week Day", Visible = false, Visibility = PXUIVisibility.Invisible)]
        [weeklyFrequency.ListAtrribute]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? WeeklyFrequency { get; set; }
        #endregion
        #region MonthlyFrequency
        public abstract class monthlyFrequency : PX.Data.BQL.BqlInt.Field<monthlyFrequency> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Frequency Month Day", Visible = false, Visibility = PXUIVisibility.Invisible)]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? MonthlyFrequency { get; set; }
        #endregion
        #region SendInvoicesTo
        public abstract class sendInvoicesTo : ListField_Send_Invoices_To
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO)]
        [sendInvoicesTo.ListAtrribute]
        [PXUIField(DisplayName = "Bill-To Address")]
        public virtual string SendInvoicesTo { get; set; }
        #endregion
        #region BillShipmentSource
        public abstract class billShipmentSource : ListField_Ship_To
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Ship_To.SERVICE_ORDER_ADDRESS)]
        [billShipmentSource.ListAtrribute]
        [PXUIField(DisplayName = "Ship-To Address")]
        public virtual string BillShipmentSource { get; set; }
        #endregion

        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "CreatedByID")]
        public virtual Guid? CreatedByID { get; set; }

        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }

        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }

        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }

        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }

        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public virtual DateTime? LastModifiedDateTime { get; set; }

        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        [PXUIField(DisplayName = "tstamp")]
        public virtual byte[] tstamp { get; set; }

        #endregion 
    }
}
