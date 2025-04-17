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
using PX.SM;

namespace PX.Objects.AM
{
	/// <summary>
	/// The table with the settings for barcode scanning forms, where the settings apply only to a single user.
	/// </summary>
	/// <remarks>
	/// These settings are used on the following forms:
	/// <list type="bullet">
	/// <item><description>Scan Move (AM302010) (corresponding to the <see cref="ScanMove"/> graph)</description></item>
	/// <item><description>Scan Labor (AM302020) (corresponding to the <see cref="ScanLabor"/> graph)</description></item>
	/// <item><description>Scan Materials (AM300030) (corresponding to the <see cref="ScanMaterial"/> graph)</description></item>
	/// </list>
	/// </remarks>
	[PXCacheName(Messages.AMScanUserSetup, PXDacType.Config)]
	public class AMScanUserSetup : IBqlTable
	{
		#region UserID
		[PXDBGuid(IsKey = true)]
		[PXDefault(typeof(Search<Users.pKID, Where<Users.pKID, Equal<Current<AccessInfo.userID>>>>))]
		[PXUIField(DisplayName = "User")]
		public virtual Guid? UserID { get; set; }
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
		#endregion
		#region IsOverridden
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Overridden", Enabled = false)]
		public virtual bool? IsOverridden { get; set; }
		public abstract class isOverridden : PX.Data.BQL.BqlBool.Field<isOverridden> { }
		#endregion

		#region Mode
		[PXDBString(4, IsKey = true)]
		[PXUIField(DisplayName = "Mode", Enabled = false, Visible = false)]
		public virtual string Mode { get; set; }
		public abstract class mode : PX.Data.BQL.BqlString.Field<mode> { }
		#endregion

		#region DefaultWarehouse
		[PXDBBool]
		[PXDefault(true, typeof(Search<AMScanSetup.defaultWarehouse, Where<AMScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Default Warehouse from User Profile")]
		public virtual bool? DefaultWarehouse { get; set; }
		public abstract class defaultWarehouse : PX.Data.BQL.BqlBool.Field<defaultWarehouse> { }
		#endregion
		#region DefaultLotSerialNumber
		/// <summary>
		/// Auto generated lot/serial number.
		/// </summary>
		[PXDBBool]
		[PXDefault(true, typeof(Search<AMScanSetup.defaultLotSerialNumber, Where<AMScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Auto-Generated Lot/Serial Nbr.")]
		public virtual bool? DefaultLotSerialNumber { get; set; }

		public abstract class defaultLotSerialNumber : PX.Data.BQL.BqlBool.Field<defaultLotSerialNumber> { }
		#endregion
		#region DefaultExpireDate
		/// <summary>
		/// Default Expire Date 
		/// </summary>
		[PXDBBool]
		[PXDefault(true, typeof(Search<AMScanSetup.defaultExpireDate, Where<AMScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXUIField(DisplayName = "Use Default Expiration Date")]
		public virtual bool? DefaultExpireDate { get; set; }

		public abstract class defaultExpireDate : PX.Data.BQL.BqlBool.Field<defaultExpireDate> { }
		#endregion
    }
}
