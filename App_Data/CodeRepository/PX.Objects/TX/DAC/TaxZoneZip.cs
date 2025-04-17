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

namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxZoneZip)]
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
	public partial class TaxZoneZip : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<TaxZoneZip>.By<taxZoneID, zipCode, zipMin>
		{
			public static TaxZoneZip Find(PXGraph graph, String taxZoneID, String zipCode, Int32? zipMin, PKFindOptions options = PKFindOptions.None) => FindBy(graph, taxZoneID, zipCode, zipMin, options);
		}
		public static class FK
		{
			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<TaxZoneZip>.By<taxZoneID> { }
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXParent(typeof(Select<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<TaxZoneZip.taxZoneID>>>>))]
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXDefault(typeof(TaxZone.taxZoneID))]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region ZipCode
		public abstract class zipCode : PX.Data.BQL.BqlString.Field<zipCode> { }
		protected String _ZipCode;
		[PXUIField(DisplayName="Zip Code")]
		[PXDBString(9, IsKey = true)]
		[PXDefault()]
		public virtual String ZipCode
		{
			get
			{
				return this._ZipCode;
			}
			set
			{
				this._ZipCode = value;
			}
		}
		#endregion
		#region ZipMin
		public abstract class zipMin : PX.Data.BQL.BqlInt.Field<zipMin> { }
		protected Int32? _ZipMin;
		[PXUIField(DisplayName = "Zip Code+4 (Min.)")]
		[PXDBInt(IsKey = true, MinValue=0, MaxValue=9999)]
		[PXDefault(1)]
		public virtual Int32? ZipMin
		{
			get
			{
				return this._ZipMin;
			}
			set
			{
				this._ZipMin = value;
			}
		}
		#endregion
		#region ZipMax
		public abstract class zipMax : PX.Data.BQL.BqlInt.Field<zipMax> { }
		protected Int32? _ZipMax;
		[PXUIField(DisplayName = "Zip Code+4 (Max.)")]
		[PXDBInt(MinValue = 0, MaxValue = 9999)]
		[PXDefault(9999)]
		public virtual Int32? ZipMax
		{
			get
			{
				return this._ZipMax;
			}
			set
			{
				this._ZipMax = value;
			}
		}
		#endregion
	}
}
