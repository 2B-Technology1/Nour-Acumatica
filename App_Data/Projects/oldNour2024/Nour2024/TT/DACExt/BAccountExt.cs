using PX.Data.EP;

using PX.Data.ReferentialIntegrity.Attributes;

using PX.Data;

using PX.Objects.AP;

using PX.Objects.AR;

using PX.Objects.CR.MassProcess;

using PX.Objects.CR;

using PX.Objects.CS;

using PX.Objects.EP;

using PX.Objects.GL;

using PX.Objects.TX;

using PX.Objects;

using PX.SM;

using PX.TM;

using System.Collections.Generic;

using System.Diagnostics;

using System;



namespace Maintenance
{

    public class BAccountExt : PXCacheExtension<PX.Objects.CR.BAccount>
    {

        #region UsrCommercialRecord

        [PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        [PXUIField(DisplayName = "Commercial Record")]

        public virtual string UsrCommercialRecord { get; set; }

        public abstract class usrCommercialRecord : IBqlField { }

        #endregion



        #region UsrTaxNo

        [PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        [PXUIField(DisplayName = "Tax No.")]

        public virtual string UsrTaxNo { get; set; }

        public abstract class usrTaxNo : IBqlField { }

        #endregion



        #region UsrTaxFile

        [PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        [PXUIField(DisplayName = "Tax File")]

        public virtual string UsrTaxFile { get; set; }

        public abstract class usrTaxFile : IBqlField { }

        #endregion



        //#region UsrLegalForm

        //[PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        //[PXUIField(DisplayName = "Legal Form")]

        //[PXSelector(typeof(Maintenance.LegalForm.legalFormID),

        //      new Type[]

        //      {

        //        typeof(Maintenance.LegalForm.legalFormID),

        //        typeof(Maintenance.LegalForm.descr)

        //      },

        //      SubstituteKey = typeof(Maintenance.LegalForm.legalFormID),

        //      DescriptionField = typeof(Maintenance.LegalForm.descr))]

        //public virtual string UsrLegalForm { get; set; }

        //public abstract class usrLegalForm : IBqlField { }

        //#endregion



        #region UsrBusinessDate

        [PXDBDate(IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount))]

        [PXUIField(DisplayName = "Business Date")]

        public virtual string UsrBusinessDate { get; set; }

        public abstract class usrBusinessDate : IBqlField { }

        #endregion



        #region UsrVCommercialRecord

        [PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        [PXUIField(DisplayName = "Commercial Record")]

        public virtual string UsrVCommercialRecord { get; set; }

        public abstract class usrVCommercialRecord : IBqlField { }

        #endregion



        #region UsrVTaxNo

        [PXDBString(IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        [PXUIField(DisplayName = "Tax No.")]

        public virtual string UsrVTaxNo { get; set; }

        public abstract class usrVTaxNo : IBqlField { }

        #endregion



        #region UsrVTaxFile

        [PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        [PXUIField(DisplayName = "Tax File")]

        public virtual string UsrVTaxFile { get; set; }

        public abstract class usrVTaxFile : IBqlField { }

        #endregion



        #region UsrVRegistrationNo

        [PXDBString(30, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        [PXUIField(DisplayName = "Registration No.")]

        public virtual string UsrVRegistrationNo { get; set; }

        public abstract class usrVRegistrationNo : IBqlField { }

        #endregion



        //#region UsrCustomerActivityID

        //[PXDBString(20, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        //[PXUIField(DisplayName = "Customer Activity")]

        //[PXSelector(typeof(Maintenance.CustomerActivity.customerActivityID),

        //      new Type[]

        //      {

        //        typeof(Maintenance.CustomerActivity.customerActivityID),

        //        typeof(Maintenance.CustomerActivity.descr)

        //      },

        //      SubstituteKey = typeof(Maintenance.CustomerActivity.customerActivityID),

        //      DescriptionField = typeof(Maintenance.CustomerActivity.descr))]

        //public virtual string UsrCustomerActivityID { get; set; }

        //public abstract class usrCustomerActivityID : IBqlField { }

        //#endregion



        #region UsrTaxesID

        [PXDBString(20, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        [PXUIField(DisplayName = "Taxes ID")]

        [PXSelector(typeof(Maintenance.Taxes.taxesID),

              new Type[]

              {

                typeof(Maintenance.Taxes.taxesID),

                typeof(Maintenance.Taxes.descr)

              },

              SubstituteKey = typeof(Maintenance.Taxes.taxesID),

              DescriptionField = typeof(Maintenance.Taxes.descr))]

        public virtual string UsrTaxesID { get; set; }

        public abstract class usrTaxesID : IBqlField { }

        #endregion



        #region UsrVTaxesID

        [PXDBString(20, IsKey = false, BqlTable = typeof(PX.Objects.CR.BAccount), IsFixed = false, IsUnicode = true)]

        [PXUIField(DisplayName = "Taxes ID")]

        [PXSelector(typeof(Maintenance.Taxes.taxesID),

              new Type[]

              {

                typeof(Maintenance.Taxes.taxesID),

                typeof(Maintenance.Taxes.descr)

              },

              SubstituteKey = typeof(Maintenance.Taxes.taxesID),

              DescriptionField = typeof(Maintenance.Taxes.descr))]

        public virtual string UsrVTaxesID { get; set; }

        public abstract class usrVTaxesID : IBqlField { }

        #endregion

        #region UsrCustomerType
        [PXDBString(10)]
        [PXUIField(DisplayName = "Tax Customer Type", Required = true)]
        [PXDefault("P")]
        [PXStringList(new string[] { "P", "B", "F" }, new string[] { "Person", "Business", "Foreigner" })]
        public virtual string UsrCustomerType { get; set; }
        public abstract class usrCustomerType : IBqlField { }
        #endregion

        #region UsrLoyPoints
        [PXDBInt]
        [PXUIField(DisplayName = "Loyality Pts")]

        public virtual int? UsrLoyPoints { get; set; }
        public abstract class usrLoyPoints : IBqlField { }
        #endregion
        

        [PXNonInstantiatedExtension]
        public class CR_BAccount_ExistingColumn : PXCacheExtension<PX.Objects.CR.BAccount>
        {
            #region AcctReferenceNbr  
            [PXDBString(50, IsUnicode = true)]
            [PXUIField(DisplayName = "National ID", Visibility = PXUIVisibility.SelectorVisible)]
            [PXMassMergableField]
            public string AcctReferenceNbr { get; set; }
            #endregion
        }

        

    

    }

}