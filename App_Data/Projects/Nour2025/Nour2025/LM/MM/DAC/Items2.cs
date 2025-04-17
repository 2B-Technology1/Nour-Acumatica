﻿namespace Maintenance.MM
 
{
	using System;
	using PX.Data;
    using MyMaintaince;
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(CarEntry))]
    public class Items2 : PX.Data.IBqlTable
	{
        #region Code
        public abstract class code : PX.Data.IBqlField
        {
        }
        protected string _Code;
        [PXDBString(50, IsKey = true, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaaaa")]
        [PXDefault("")]
        [PXSelector(typeof(Search<Items2.code>),
        new Type[] { typeof(Items2.code), typeof(Items2.name) })]
        [PXUIField(DisplayName = "Vin Number")]
        public virtual string Code
        {
            get
            {
                return this._Code;
            }
            set
            {
                this._Code = value;
            }
        }
        #endregion
        #region Name
        public abstract class name : PX.Data.IBqlField
        {
        }
        protected string _Name;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Descr.")]
        public virtual string Name
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
        #region BrandID
        public abstract class brandID : PX.Data.IBqlField
        {
        }
        protected int? _BrandID;
        [PXDBInt()]
        [PXUIField(DisplayName = "Brand ID")]
        //[PXDefault("")]
        [PXSelector(typeof(Search<Brand.brandID>)
                   , new Type[] { typeof(Brand.Code), typeof(Brand.name) }
                   , DescriptionField = typeof(Brand.name)
                   , SubstituteKey = typeof(Brand.Code))]
        public virtual int? BrandID
        {
            get
            {
                return this._BrandID;
            }
            set
            {
                this._BrandID = value;
            }
        }
        #endregion
        #region ModelID
        public abstract class modelID : PX.Data.IBqlField
        {
        }
        protected int? _ModelID;
        [PXDBInt()]
        [PXUIField(DisplayName = "Model ID")]
        [PXDefault(typeof(Search<Model.modelID, Where<Model.brandID, Equal<Current<Items2.brandID>>>>))]
        [PXSelector(typeof(Search<Model.modelID, Where<Model.brandID, Equal<Current<Items2.brandID>>>>)
                          , new Type[] { typeof(Model.code), typeof(Model.name) }
                          , DescriptionField = typeof(Model.name)
                          , SubstituteKey = typeof(Model.code))]


        public virtual int? ModelID
        {
            get
            {
                return this._ModelID;
            }
            set
            {
                this._ModelID = value;
            }
        }
        #endregion
        /**
		#region Customer
		public abstract class customer : PX.Data.IBqlField
		{
		}
		protected string _Customer;
		[PXDBString(50, IsUnicode = true)]
		//[PXDefault("")]
		[PXSelector(typeof(Search<PX.Objects.AR.Customer.acctCD>),typeof(PX.Objects.AR.Customer.acctCD),typeof(PX.Objects.AR.Customer.acctName))]
        [PXUIField(DisplayName = "Customer")]
        public virtual string Customer
		{
			get
			{
				return this._Customer;
			}
			set
			{
				this._Customer = value;
			}
		}
		#endregion
        **/
        #region ItemsID
        public abstract class itemsID : PX.Data.IBqlField
        {
        }
        protected int? _ItemsID;
        [PXDBIdentity()]
        [PXUIField(DisplayName = "Items2 Nbr")]
        [PXSelector(typeof(Search<Items2.itemsID>), new Type[] { typeof(Items2.itemsID), typeof(Items2.code) }, DescriptionField = typeof(Items2.name))]
        public virtual int? ItemsID
        {
            get
            {
                return this._ItemsID;
            }
            set
            {
                this._ItemsID = value;
            }
        }
        #endregion
        #region ChassisNo
        public abstract class chassisNo : PX.Data.IBqlField
        {
        }
        protected string _ChassisNo;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Vin Number")]
        public virtual string ChassisNo
        {
            get
            {
                return this._ChassisNo;
            }
            set
            {
                this._ChassisNo = value;
            }
        }
        #endregion
        #region CharactersAndNumbers
        public abstract class charactersAndNumbers : PX.Data.IBqlField
        {
        }
        protected bool? _CharactersAndNumbers;
        [PXDBBool()]
        [PXUIField(DisplayName = "Characters And Numbers")]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? CharactersAndNumbers
        {
            get
            {
                return this._CharactersAndNumbers;
            }
            set
            {
                this._CharactersAndNumbers = value;
            }
        }
        #endregion
        #region LincensePlat
        public abstract class lincensePlat : PX.Data.IBqlField
        {
        }
        protected string _LincensePlat;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Plate Number")]
        [PXDefault("")]
        //[PXDimensionAttribute("LICENCEPLATEDIM")]
        public virtual string LincensePlat
        {
            get
            {
                return this._LincensePlat;
            }
            set
            {
                this._LincensePlat = value;
            }
        }
        #endregion
        #region PurchesDate
        public abstract class purchesDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _PurchesDate;
        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Purches Date")]
        public virtual DateTime? PurchesDate
        {
            get
            {
                return this._PurchesDate;
            }
            set
            {
                this._PurchesDate = value;
            }
        }
        #endregion
        #region MgfDate
        public abstract class mgfDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _MgfDate;
        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? MgfDate
        {
            get
            {
                return this._MgfDate;
            }
            set
            {
                this._MgfDate = value;
            }
        }
        #endregion
        #region Color
        public abstract class color : PX.Data.IBqlField
        {
        }
        protected string _Color;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Body Color")]
        [PXDefault("")]
        [PXSelector(typeof(Search<Colors.colorCD>),
         new Type[] { typeof(Colors.colorCD), typeof(Colors.descr) })]

        public virtual string Color
        {
            get
            {
                return this._Color;
            }
            set
            {
                this._Color = value;
            }
        }
        #endregion
        #region GurarantYear
        public abstract class gurarantYear : PX.Data.IBqlField
        {
        }
        protected int? _GurarantYear;
        [PXDBInt()]
        [PXUIField(DisplayName = "Gurarante Year")]
       // [PXDefault("")]
       // [PXSelector(typeof(Search<YearIndex.yearID>),
       //new Type[] { typeof(YearIndex.yearID) })]
        public virtual int? GurarantYear
        {
            get
            {
                return this._GurarantYear;
            }
            set
            {
                this._GurarantYear = value;
            }
        }
        #endregion
        #region ExpiredDate
        public abstract class expiredDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _ExpiredDate;
        [PXDBDate()]
        [PXDefault("")]
        [PXUIField(DisplayName = "Warranty End Date")]
        public virtual DateTime? ExpiredDate
        {
            get
            {
                return this._ExpiredDate;
            }
            set
            {
                this._ExpiredDate = value;
            }
        }
        #endregion
        #region Transmission
        public abstract class transmission : PX.Data.IBqlField
        {
        }
        protected string _Transmission;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Transmission")]
        [PXStringList(
              new string[] { "M", "A", "S" ,"C"},
              new string[] { "Manual", "Automatic", "Steptronic","CVT" }
            )]
        public virtual string Transmission
        {
            get
            {
                return this._Transmission;
            }
            set
            {
                this._Transmission = value;
            }
        }
        #endregion
        #region EngineNo
        public abstract class engineNo : PX.Data.IBqlField
        {
        }
        protected string _EngineNo;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Motor No.")]
        [PXDefault("")]

        public virtual string EngineNo
        {
            get
            {
                return this._EngineNo;
            }
            set
            {
                this._EngineNo = value;
            }
        }
        #endregion
        #region EngineCapcity
        public abstract class engineCapcity : PX.Data.IBqlField
        {
        }
        protected string _EngineCapcity;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Cylinder Nbr")]
        [PXStringList(
             new string[] { "3", "4", "5","6","8","10","12" },
             new string[] { "3", "4", "5", "6", "8", "10", "12" }
           )]
        public virtual string EngineCapcity
        {
            get
            {
                return this._EngineCapcity;
            }
            set
            {
                this._EngineCapcity = value;
            }
        }
        #endregion
        #region Fuel
        public abstract class fuel : PX.Data.IBqlField
        {
        }
        protected string _Fuel;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Fuel")]
        [PXStringList(
           new string[] { "Petrol", "Diesel" },
           new string[] { "Petrol", "Diesel" }
         )]
        public virtual string Fuel
        {
            get
            {
                return this._Fuel;
            }
            set
            {
                this._Fuel = value;
            }
        }
        #endregion
        #region Inductionsystem
        public abstract class inductionsystem : PX.Data.IBqlField
        {
        }
        protected string _Inductionsystem;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Induction System")]
        public virtual string Inductionsystem
        {
            get
            {
                return this._Inductionsystem;
            }
            set
            {
                this._Inductionsystem = value;
            }
        }
        #endregion
        #region BodyType
        public abstract class bodyType : PX.Data.IBqlField
        {
        }
        protected string _BodyType;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Body Type")]
        [PXStringList(
            new string[] { "HatchBack", "Sedan", "Suv", "Other"},
            new string[] { "HatchBack", "Sedan", "Suv", "Other" }
          )]
        public virtual string BodyType
        {
            get
            {
                return this._BodyType;
            }
            set
            {
                this._BodyType = value;
            }
        }
        #endregion
        #region Make
        public abstract class make : PX.Data.IBqlField
        {
        }
        protected string _Make;
        [PXDBString(1, IsUnicode = true, IsFixed = true)]
        [PXUIField(DisplayName = "Make")]
        public virtual string Make
        {
            get
            {
                return this._Make;
            }
            set
            {
                this._Make = value;
            }
        }
        #endregion
        #region AC
        public abstract class aC : PX.Data.IBqlField
        {
        }
        protected bool? _AC;
        [PXDBBool()]
        [PXUIField(DisplayName = "AC")]
        public virtual bool? AC
        {
            get
            {
                return this._AC;
            }
            set
            {
                this._AC = value;
            }
        }
        #endregion
        #region Brakes
        public abstract class brakes : PX.Data.IBqlField
        {
        }
        protected string _Brakes;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Brakes")]
        public virtual string Brakes
        {
            get
            {
                return this._Brakes;
            }
            set
            {
                this._Brakes = value;
            }
        }
        #endregion
        #region PowerSteering
        public abstract class powerSteering : PX.Data.IBqlField
        {
        }
        protected string _PowerSteering;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Power Steering")]
        public virtual string PowerSteering
        {
            get
            {
                return this._PowerSteering;
            }
            set
            {
                this._PowerSteering = value;
            }
        }
        #endregion
        #region TireType
        public abstract class tireType : PX.Data.IBqlField
        {
        }
        protected string _TireType;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Tire Type")]
        public virtual string TireType
        {
            get
            {
                return this._TireType;
            }
            set
            {
                this._TireType = value;
            }
        }
        #endregion
        #region TireSize
        public abstract class tireSize : PX.Data.IBqlField
        {
        }
        protected string _TireSize;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Tire Size")]
        public virtual string TireSize
        {
            get
            {
                return this._TireSize;
            }
            set
            {
                this._TireSize = value;
            }
        }
        #endregion
        #region TirePreasure
        public abstract class tirePreasure : PX.Data.IBqlField
        {
        }
        protected string _TirePreasure;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Tire Preasure")]
        public virtual string TirePreasure
        {
            get
            {
                return this._TirePreasure;
            }
            set
            {
                this._TirePreasure = value;
            }
        }
        #endregion
        #region AirBag
        public abstract class airBag : PX.Data.IBqlField
        {
        }
        protected string _AirBag;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Air Bag")]
        public virtual string AirBag
        {
            get
            {
                return this._AirBag;
            }
            set
            {
                this._AirBag = value;
            }
        }
        #endregion
        #region ConsumptionRate
        public abstract class consumptionRate : PX.Data.IBqlField
        {
        }
        protected float? _ConsumptionRate;
        [PXDBFloat(4)]
        [PXUIField(DisplayName = "Consumption Rate")]
        public virtual float? ConsumptionRate
        {
            get
            {
                return this._ConsumptionRate;
            }
            set
            {
                this._ConsumptionRate = value;
            }
        }
        #endregion
        #region WarrantySDate
        public abstract class warrantySDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _WarrantySDate;
        [PXDBDate()]
        [PXDefault("")]
        [PXUIField(DisplayName = "Warranty Start Date")]
        public virtual DateTime? WarrantySDate
        {
            get
            {
                return this._WarrantySDate;
            }
            set
            {
                this._WarrantySDate = value;
            }
        }
        #endregion
		#region ModelYear
		public abstract class modelYear : PX.Data.IBqlField
		{
		}
		protected int? _ModelYear;
		[PXDBInt()]
		[PXUIField(DisplayName = "Model Year")]
        [PXDefault("")]
        [PXSelector(typeof(Search<YearIndex.yearID>),
       new Type[] { typeof(YearIndex.yearID)})]
		public virtual int? ModelYear
		{
			get
			{
				return this._ModelYear;
			}
			set
			{
				this._ModelYear = value;
			}
		}
		#endregion
		#region TrimColor
		public abstract class trimColor : PX.Data.IBqlField
		{
		}
		protected string _TrimColor;
		[PXDBString(50, IsUnicode = true)]
        [PXDefault("")]
		[PXUIField(DisplayName = "Trim Color")]
		public virtual string TrimColor
		{
			get
			{
				return this._TrimColor;
			}
			set
			{
				this._TrimColor = value;
			}
		}
		#endregion
		
	}
}
