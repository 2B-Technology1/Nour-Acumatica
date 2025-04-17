﻿namespace Maintenance.CD
 {
     using System;
    using MyProject.CD;
    using PX.Data;
     using PX.Objects.GL;

     [System.SerializableAttribute()]
     [PXPrimaryGraph(typeof(CDSetUpMaint))]
     [PXCacheName("SetUp Preferences")]
     public class CDSetUp : PX.Data.IBqlTable
     {
         #region SetupID
         public abstract class setupID : PX.Data.IBqlField
         {
         }
         protected int? _SetupID;
         [PXDBIdentity(IsKey = true)]
         [PXUIField(DisplayName = "SetupID", Enabled = false)]
         public virtual int? SetupID
         {
             get
             {
                 return this._SetupID;
             }
             set
             {
                 this._SetupID = value;
             }
         }
         #endregion
         #region LastRefNbr
         public abstract class lastRefNbr : PX.Data.IBqlField
         {
         }
         protected string _LastRefNbr;
         [PXDBString(15, IsUnicode = true)]
         [PXUIField(DisplayName = "Last RefNbr")]
         public virtual string LastRefNbr
         {
             get
             {
                 return this._LastRefNbr;
             }
             set
             {
                 this._LastRefNbr = value;
             }
         }
         #endregion
         #region AutoNumbering
         public abstract class autoNumbering : PX.Data.IBqlField
         {
         }
         protected bool? _AutoNumbering;
         [PXDBBool()]
         [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
         [PXUIField(DisplayName = "Auto Numbering")]
         public virtual bool? AutoNumbering
         {
             get
             {
                 return this._AutoNumbering;
             }
             set
             {
                 this._AutoNumbering = value;
             }

         }
         #endregion
     }
 }
