using System;
using Maintenance.MM;
using MyMaintaince;
using Nour20230821VTaxFieldsSolveError;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;

namespace Nour20231012VSolveUSDNew
{
  [Serializable]
  [PXCacheName("InspectionFormMaster")]
  public class InspectionFormMaster : IBqlTable
  {


        #region Master 
        #region InspectionFormNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCC")]
        //[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Inspection Number", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<inspectionFormNbr>), typeof(inspectionFormNbr), typeof(status))]
        [AutoNumber(typeof(SetupForm.numberingID), typeof(createdDateTime))]
        public virtual string InspectionFormNbr { get; set; }
        public abstract class inspectionFormNbr : PX.Data.BQL.BqlString.Field<inspectionFormNbr> { }
        #endregion

        #region ReserveID
        //protected string _ReserveID;
        [PXDBString(50,IsUnicode = true)]
        [PXUIField(DisplayName = "Reserve ID")]
        [PXSelector(typeof(SearchFor<ReserveForm.refNbr>))]

        //[PXSelector(typeof(Search<ReserveForm.refNbr, Where<ReserveForm.refNbr, Equal<Current<InspectionFormMaster.reserveID>>>>))]
        public virtual string ReserveID {get;set;}
        public abstract class reserveID: PX.Data.BQL.BqlString.Field<reserveID> { }
        #endregion

        #region Status
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        [PXDefault("O")]
        [PXStringList(
            new string[] { "J", "O", "C" }, new string[] { "Job Ordered", "Open", "Canceled" })]
        public virtual string Status { get; set; }
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        #endregion

        #region Inspection Type
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "Inspection Type", Enabled = true)]
        [PXDefault("F")]
        [PXStringList(
            new string[] { "F", "S" }, new string[] { "Full Inspection", "Specific Inspection" })]
        public virtual string InspectionType { get; set; }
        public abstract class inspectionType : PX.Data.BQL.BqlString.Field<inspectionType> { }
        #endregion

        #region Inspection Class
        [PXDBString(2, InputMask = "")]
        [PXUIField(DisplayName = "Inspection Class", Enabled = true)]
        //[PXDefault("MC")]
        [PXStringList(
            new string[] { "MC", "CO", "RT", "SS", "M"  }, new string[] { "Malfunction Check", "Car Outside", "Road Test", "Suspension System", "Motor" })]
        public virtual string InspectionClass { get; set; }
        public abstract class inspectionClass : PX.Data.BQL.BqlString.Field<inspectionClass> { }
        #endregion

        #region Date
        [PXDBDate()]
        [PXUIField(DisplayName = "Date")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? Date { get; set; }
        public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
        #endregion

        #region JobOrderID
        [PXDBString(20, InputMask = "")]
        [PXUIField(DisplayName = "Job Order ID", Enabled = false)]
        [PXSelector(
            typeof(SearchFor<JobOrder.jobOrdrID>.Where<JobOrder.inspectionNbr.IsEqual<inspectionFormNbr.FromCurrent>>))]
        //[PXSelector(
        //    typeof(Search<JobOrder.jobOrdrID, Where<JobOrder.inspectionNbr, Equal<Current<inspectionFormNbr>>>>))]
        public virtual string JobOrderID { get; set; }
        public abstract class jobOrderID : PX.Data.BQL.BqlString.Field<jobOrderID> { }
        #endregion

        #region Branches

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Search<SetUp.branchCD, Where<SetUp.autoNumbering, Equal<True>>>))]
        [PXDefault()]
        public virtual string Branches { get; set; }
        public abstract class branches : PX.Data.BQL.BqlString.Field<branches> { }
        #endregion

        #region Customer
        [PXDBString(20, InputMask = "")]
        [PXUIField(DisplayName = "Customer")]
        [PXDefault()]
        [PXSelector(typeof(Search2<Customer.acctCD, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>>, Where<ItemCustomers.itemsID, Equal<Current<vehicle>>>>)
                   , new Type[]
                   {
                       typeof(Customer.acctCD),
                       typeof(Customer.acctName)
                   }
                   , DescriptionField = typeof(Customer.acctName)
                   , SubstituteKey = typeof(Customer.acctCD))]
        public virtual string Customer { get; set; }
        public abstract class customer : PX.Data.BQL.BqlString.Field<customer> { }
        #endregion

        #region Vehicle
        [PXDBInt()]
        [PXDefault()]
        [PXUIField(DisplayName = "Vehicle")]
        [PXSelector(typeof(Search<Items2.itemsID>)
                    , new Type[] {
                        typeof(Items2.code),
                        typeof(Items2.name),
                        //typeof(Items.customer),
                        typeof(Items2.brandID),
                        typeof(Items2.modelID),
                        typeof(Items2.purchesDate),
                        typeof(Items2.lincensePlat),
                        typeof(Items2.mgfDate),
                        typeof(Items2.gurarantYear),
                    }
                    , DescriptionField = typeof(Items2.name)
                    , SubstituteKey = typeof(Items2.code))]
        public virtual int? Vehicle { get; set; }
        public abstract class vehicle : PX.Data.BQL.BqlInt.Field<vehicle> { }
        #endregion

        #region KM

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Kilometer")]
        public virtual string KM { get; set; }
        public abstract class kM : PX.Data.BQL.BqlString.Field<kM>
        {
        }
        #endregion

        #region Phone
        [PXDBString(20, InputMask = "")]
        [PXUIField(DisplayName = "Phone Number")]
        public virtual string Phone { get; set; }
        public abstract class phone : PX.Data.BQL.BqlString.Field<phone> { }
        #endregion

        #region Comment
        [PXDBString(InputMask = "", IsUnicode = true)]
        [PXUIField(DisplayName = "Notes")]
        public virtual string Comment { get; set; }
        public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
        #endregion

        #endregion



        //----------------------------------
    //    #region InspectionFormNbr
    //    [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
    //[PXUIField(DisplayName = "Inspection Form Nbr")]
    //public virtual string InspectionFormNbr { get; set; }
    //public abstract class inspectionFormNbr : PX.Data.BQL.BqlString.Field<inspectionFormNbr> { }
    //#endregion

    //#region Status
    //[PXDBString(1, IsFixed = true, InputMask = "")]
    //[PXUIField(DisplayName = "Status")]
    //public virtual string Status { get; set; }
    //public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
    //#endregion

    

    //    #region Inspection Type
    //    [PXDBString(1, InputMask = "")]
    //    [PXUIField(DisplayName = "Inspection Type", Enabled = true)]
    //    [PXDefault("F")]
    //    [PXStringList(
    //        new string[] { "F", "S" }, new string[] { "Full Inspection", "Specific Inspection" })]
    //    public virtual string InspectionType { get; set; }
    //    public abstract class inspectionType : PX.Data.BQL.BqlString.Field<inspectionType> { }
    //    #endregion

    //    #region Inspection Class
    //    [PXDBString(2, InputMask = "")]
    //    [PXUIField(DisplayName = "Inspection Class", Enabled = true)]
    //    //[PXDefault("MC")]
    //    [PXStringList(
    //        new string[] { "MC", "SS", "RT", "M", "CO" }, new string[] { "Malfunction Check", "Suspension System", "Road Test", "Motor", "Car Outside" })]
    //    public virtual string InspectionClass { get; set; }
    //    public abstract class inspectionClass : PX.Data.BQL.BqlString.Field<inspectionClass> { }
    //    #endregion

    //    #region Date
    //    [PXDBDate()]
    //[PXUIField(DisplayName = "Date")]
    //public virtual DateTime? Date { get; set; }
    //public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
    //#endregion

    //#region Branches
    //[PXDBString(50, IsUnicode = true, InputMask = "")]
    //[PXUIField(DisplayName = "Branches")]
    //public virtual string Branches { get; set; }
    //public abstract class branches : PX.Data.BQL.BqlString.Field<branches> { }
    //#endregion

    //#region Customer
    //[PXDBString(20, IsUnicode = true, InputMask = "")]
    //[PXUIField(DisplayName = "Customer")]
    //public virtual string Customer { get; set; }
    //public abstract class customer : PX.Data.BQL.BqlString.Field<customer> { }
    //#endregion

    //#region Vehicle
    //[PXDBInt()]
    //[PXUIField(DisplayName = "Vehicle")]
    //public virtual int? Vehicle { get; set; }
    //public abstract class vehicle : PX.Data.BQL.BqlInt.Field<vehicle> { }
    //#endregion

    //#region JobOrderID
    //[PXDBString(20, IsUnicode = true, InputMask = "")]
    //    [PXUIField(DisplayName = "Job Order ID", Enabled = false)]
    //    [PXSelector(
    //        typeof(Search<JobOrder.jobOrdrID, Where<JobOrder.inspectionNbr, Equal<Current<inspectionFormNbr>>>>))]
    //    public virtual string JobOrderID { get; set; }
    //    public abstract class jobOrderID : PX.Data.BQL.BqlString.Field<jobOrderID> { }
    //    #endregion

        

    //    #region Km
    //    [PXDBString(50, IsUnicode = true, InputMask = "")]
    //[PXUIField(DisplayName = "Km")]
    //public virtual string Km { get; set; }
    //public abstract class km : PX.Data.BQL.BqlString.Field<km> { }
    //#endregion

    //#region Phone
    //[PXDBString(20, IsUnicode = true, InputMask = "")]
    //[PXUIField(DisplayName = "Phone")]
    //public virtual string Phone { get; set; }
    //public abstract class phone : PX.Data.BQL.BqlString.Field<phone> { }
    //#endregion

    //#region Comment
    //[PXDBString(255, IsUnicode = true, InputMask = "")]
    //[PXUIField(DisplayName = "Comment")]
    //public virtual string Comment { get; set; }
    //public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
    //    #endregion


        //----------------------------------







    #region Tstamp
        [PXDBTimestamp()]
    [PXUIField(DisplayName = "Tstamp")]
    public virtual byte[] Tstamp { get; set; }
    public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
    #endregion

    #region CreatedByID
    [PXDBCreatedByID()]
    public virtual Guid? CreatedByID { get; set; }
    public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
    #endregion

    #region CreatedByScreenID
    [PXDBCreatedByScreenID()]
    public virtual string CreatedByScreenID { get; set; }
    public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
    #endregion

    #region CreatedDateTime
    [PXDBCreatedDateTime()]
    public virtual DateTime? CreatedDateTime { get; set; }
    public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
    #endregion

    #region LastModifiedByID
    [PXDBLastModifiedByID()]
    public virtual Guid? LastModifiedByID { get; set; }
    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
    #endregion

    #region LastModifiedByScreenID
    [PXDBLastModifiedByScreenID()]
    public virtual string LastModifiedByScreenID { get; set; }
    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
    #endregion

    #region LastModifiedDateTime
    [PXDBLastModifiedDateTime()]
    public virtual DateTime? LastModifiedDateTime { get; set; }
    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
    #endregion
  }
}