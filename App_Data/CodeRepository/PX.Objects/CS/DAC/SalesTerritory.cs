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
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.CS
{
	/// <summary>
	/// Represents a Sales territory
	/// Records of this type are created and edited through the Sales territory (CS204100) screen
	/// (corresponds to the <see cref="SalesTerritoryMaint"/> graph).
	/// </summary>
	[PXCacheName(Messages.SalesTerritory)]
	[PXPrimaryGraph(typeof(SalesTerritoryMaint))]
	[Serializable]
	public partial class SalesTerritory : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SalesTerritory>.By<salesTerritoryID>
		{
			public static SalesTerritory Find(PXGraph graph, string salesTerritoryID) => FindBy(graph, salesTerritoryID);
		}
		public static class FK
		{
			public class Country : CS.Country.PK.ForeignKeyOf<SalesTerritory>.By<countryID> { }
		}
		#endregion
		#region SalesTerritoryID
		/// <summary>
		/// The primary key. 
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Sales Territory ID")]
		[PXSelector(typeof(
			Search2<SalesTerritory.salesTerritoryID,
				LeftJoin<Country, On<Country.countryID, Equal<SalesTerritory.countryID>>>>),
			fieldList: new []
			{
				typeof(SalesTerritory.salesTerritoryID),
				typeof(SalesTerritory.name),
				typeof(SalesTerritory.salesTerritoryType),
				typeof(Country.countryID),
				typeof(Country.description),
				typeof(SalesTerritory.isActive)
			},
			DescriptionField = typeof(SalesTerritory.name))]
		[PXReferentialIntegrityCheck]
		public virtual String SalesTerritoryID { get; set; }
		public abstract class salesTerritoryID : PX.Data.BQL.BqlString.Field<salesTerritoryID> { }
		#endregion
		#region Name
		/// <summary>
		/// The name of Sales territory. 
		/// </summary>
		[PXDBLocalizableString(50, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Territory Name", FieldClass = FeaturesSet.customerModule.FieldClass)]
		public virtual String Name { get; set; }
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		#endregion
		#region IsActive
		/// <summary>
		/// Indicates (if set to <see langword="true" />) that the sales territory is active.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion
		#region SalesTerritoryType
		/// <summary>
		/// Type of sales territory <see cref="SalesTerritoryTypeAttribute"/>.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Territory Type", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		[SalesTerritoryType]
		[PXDefault(SalesTerritoryTypeAttribute.ByState)]
		public virtual string SalesTerritoryType { get; set; }
		public abstract class salesTerritoryType : PX.Data.BQL.BqlString.Field<salesTerritoryType> { }
		#endregion
		#region CountryID
		/// <summary>
		/// The <see cref="CR.Address.CountryID"/> identifier for Sales Territories of type <see cref="SalesTerritoryTypeAttribute.byState"/> 
		/// </summary>
		/// <inheritdoc cref="CR.Address.CountryID" />
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIVisible(typeof(Where<salesTerritoryType.IsEqual<SalesTerritoryTypeAttribute.byState>>))]
		[PXUIRequired(typeof(Where<salesTerritoryType.IsEqual<SalesTerritoryTypeAttribute.byState>>))]
		[CR.Country]
		public virtual String CountryID { get; set; }
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
