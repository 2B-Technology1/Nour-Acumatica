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
using PX.Objects.IN;
using System;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.AM.CacheExtensions
{
	[Serializable]
	public sealed class INSiteExt : PXCacheExtension<INSite>
	{
		public static bool IsActive()
		{
			return Features.ManufacturingOrDRPOrReplenishmentEnabled();
		}

		#region AMMRPFcst
		/// <summary>
		/// MRP Include Forecasts
		/// </summary>
		public abstract class aMMRPFcst : PX.Data.BQL.BqlBool.Field<aMMRPFcst> { }
		/// <summary>
		/// MRP Include Forecasts
		/// </summary>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Forecasts", FieldClass = Features.MRPDRP)]
		public Boolean? AMMRPFcst { get; set; }
		#endregion
		#region AMMRPFlag
		/// <summary>
		/// MRP Include Inventory On Hand
		/// </summary>
		public abstract class aMMRPFlag : PX.Data.BQL.BqlBool.Field<aMMRPFlag> { }
		/// <summary>
		/// MRP Include Inventory On Hand
		/// </summary>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Inventory On Hand", FieldClass = Features.MRPDRP)]
		public Boolean? AMMRPFlag { get; set; }
		#endregion
		#region AMMRPMPS
		/// <summary>
		/// MRP Include MPS
		/// </summary>
		public abstract class aMMRPMPS : PX.Data.BQL.BqlBool.Field<aMMRPMPS> { }
		/// <summary>
		/// MRP Include MPS
		/// </summary>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "MPS", FieldClass = Features.MRPFIELDCLASS)]
        public Boolean? AMMRPMPS { get; set; }
        #endregion
        #region AMMRPPO
        /// <summary>
        /// MRP Include Purchase Orders
        /// </summary>
        public abstract class aMMRPPO : PX.Data.BQL.BqlBool.Field<aMMRPPO> { }
        /// <summary>
        /// MRP Include Purchase Orders
        /// </summary>
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Purchase Orders", FieldClass = Features.MRPDRP)]
        public Boolean? AMMRPPO { get; set; }
        #endregion
        #region AMMRPProd
        /// <summary>
        /// MRP Include Production Orders
        /// </summary>
        public abstract class aMMRPProd : PX.Data.BQL.BqlBool.Field<aMMRPProd> { }
        /// <summary>
        /// MRP Include Production Orders
        /// </summary>
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Production Orders", FieldClass = Features.MRPFIELDCLASS)]
        public Boolean? AMMRPProd { get; set; }
        #endregion
        #region AMMRPShip
        /// <summary>
        /// MRP Include Shipments
        /// </summary>
        public abstract class aMMRPShip : PX.Data.BQL.BqlBool.Field<aMMRPShip> { }
        /// <summary>
        /// MRP Include Shipments
        /// </summary>
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Shipments", FieldClass = Features.MRPDRP)]
        public Boolean? AMMRPShip { get; set; }
        #endregion
        #region AMMRPSO
        /// <summary>
        /// MRP Include Sales Orders
        /// </summary>
        public abstract class aMMRPSO : PX.Data.BQL.BqlBool.Field<aMMRPSO> { }
        /// <summary>
        /// MRP Include Sales Orders
        /// </summary>
        [PXDBBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Sales Orders", FieldClass = Features.MRPDRP)]
        public Boolean? AMMRPSO { get; set; }
        #endregion
        #region AMWIPAcctID
        public abstract class aMWIPAcctID : PX.Data.BQL.BqlInt.Field<aMWIPAcctID> { }

        [Account(DisplayName = "Work in Process Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true, FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXForeignReference(typeof(Field<INSiteExt.aMWIPAcctID>.IsRelatedTo<Account.accountID>))]
        public Int32? AMWIPAcctID { get; set; }
        #endregion
        #region AMWIPSubID
        public abstract class aMWIPSubID : PX.Data.BQL.BqlInt.Field<aMWIPSubID> { }

        [SubAccount(typeof(INSiteExt.aMWIPAcctID), DisplayName = "Work In Process Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXForeignReference(typeof(Field<INSiteExt.aMWIPSubID>.IsRelatedTo<Sub.subID>))]
        public Int32? AMWIPSubID { get; set; }
        #endregion
        #region AMWIPVarianceAcctID
        public abstract class aMWIPVarianceAcctID : PX.Data.BQL.BqlInt.Field<aMWIPVarianceAcctID> { }

        [Account(DisplayName = "WIP Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true, FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXForeignReference(typeof(Field<INSiteExt.aMWIPVarianceAcctID>.IsRelatedTo<Account.accountID>))]
        public Int32? AMWIPVarianceAcctID { get; set; }
        #endregion
        #region AMWIPVarianceSubID
        public abstract class aMWIPVarianceSubID : PX.Data.BQL.BqlInt.Field<aMWIPVarianceSubID> { }

        [SubAccount(typeof(INSiteExt.aMWIPVarianceAcctID), DisplayName = "WIP Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXForeignReference(typeof(Field<INSiteExt.aMWIPVarianceSubID>.IsRelatedTo<Sub.subID>))]
        public Int32? AMWIPVarianceSubID { get; set; }
        #endregion
        #region AMScrapSiteID
        public abstract class aMScrapSiteID : PX.Data.BQL.BqlInt.Field<aMScrapSiteID> { }

#if DEBUG
        // Cannot have a default value on INSite as it itself is the defaulting value (causes site to crash)
        // See bug # 1787
#endif
        [Site(DisplayName = "Scrap Warehouse", SetDefaultValue = false, FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        public Int32? AMScrapSiteID { get; set; }
        #endregion
        #region AMScrapLocationID
        public abstract class aMScrapLocationID : PX.Data.BQL.BqlInt.Field<aMScrapLocationID> { }

        [Location(typeof(INSiteExt.aMScrapSiteID))]
        [PXUIField(DisplayName = "Scrap Location", FieldClass = Features.MANUFACTURINGFIELDCLASS)]
        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>),
            PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        public Int32? AMScrapLocationID { get; set; }
        #endregion
    }
}
