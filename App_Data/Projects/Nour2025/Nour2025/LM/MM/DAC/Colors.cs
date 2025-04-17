﻿namespace MyMaintaince
 {
     using System;
     using PX.Data;

    [System.SerializableAttribute()]
     public class Colors : PX.Data.IBqlTable
     {
        #region ColorID
        public abstract class colorID : PX.Data.IBqlField
         {
         }
        protected int? _ColorID;
        [PXDBIdentity()]
        public virtual int? ColorID
         {
             get
             {
                 return this._ColorID;
             }
             set
             {
                 this._ColorID = value;
             }
         }
         #endregion
        #region ColorCD
        public abstract class colorCD : PX.Data.IBqlField
         {
         }
        protected string _ColorCD;
         [PXDBString(50, IsKey = true, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaaaaaaaaaa")]
         [PXDefault("")]
         [PXSelector(typeof(Colors.colorCD),
                     new Type[] { typeof(Colors.colorCD), typeof(Colors.descr) }
                     , DescriptionField = typeof(Colors.descr)
                     , SubstituteKey = typeof(Colors.colorCD))]
         [PXUIField(DisplayName = "Color ID")]
        public virtual string ColorCD
         {
             get
             {
                 return this._ColorCD;
             }
             set
             {
                 this._ColorCD = value;
             }
         }
         #endregion
        #region Descr
         public abstract class descr : PX.Data.IBqlField
         {
         }
         protected string _Descr;
         [PXDBString(50, IsUnicode = true)]
         [PXDefault("")]
         [PXUIField(DisplayName = "Descr.")]
         public virtual string Descr
         {
             get
             {
                 return this._Descr;
             }
             set
             {
                 this._Descr = value;
             }
         }
         #endregion
     }
}
