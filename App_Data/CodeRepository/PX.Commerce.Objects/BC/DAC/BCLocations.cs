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
using PX.Commerce.Core;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// Represents a mapping from locations in the external store to a location/warehouse in the ERP.
	/// </summary>
    [Serializable]
	[PXCacheName("Locations")]
	public class BCLocations : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BCLocations>.By<BCLocations.bCLocationsID>
		{
			public static BCLocations Find(PXGraph graph, int? bCLocationsID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bCLocationsID, options);
		}
		public static class FK
		{
			public class Binding : BCBinding.BindingIndex.ForeignKeyOf<BCLocations>.By<bindingID> { }
			public class Site : INSite.PK.ForeignKeyOf<BCLocations>.By<siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<BCLocations>.By<locationID> { }
		}
		#endregion

		#region BCLocationsID
		/// <summary>
		/// The identity of this record. 
		/// </summary>
		[PXDBIdentity(IsKey = true)]
        public int? BCLocationsID { get; set; }
		///<inheritdoc cref="BindingID"/>
		public abstract class bCLocationsID : PX.Data.BQL.BqlInt.Field<bCLocationsID> { }
		#endregion

		#region BindingID
		/// <summary>
		/// Represents a store to which the entity belongs.
		/// The property is a key field.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Store")]
		[PXSelector(typeof(BCBinding.bindingID),
					typeof(BCBinding.bindingName),
					SubstituteKey = typeof(BCBinding.bindingName))]
		[PXParent(typeof(Select<BCBinding,
			Where<BCBinding.bindingID, Equal<Current<BCLocations.bindingID>>>>))]
		[PXDBDefault(typeof(BCBinding.bindingID), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? BindingID { get; set; }
		///<inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion

		#region SiteID
		/// <summary>
		/// The ID of the warehouse the external location is mapped to.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse")]
		[PXSelector(typeof(INSite.siteID),
					SubstituteKey = typeof(INSite.siteCD),
					DescriptionField = typeof(INSite.descr))]
		[PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>),
							 PX.Objects.IN.Messages.TransitSiteIsNotAvailable)]
		[PXDefault()]
		public virtual int? SiteID { get; set; }
		///<inheritdoc cref="SiteID"/>
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion

		#region LocationID
		/// <summary>
		/// The ID of the location the external location is mapped to.
		/// </summary>
		[PXDBInt()]
        [PXUIField(DisplayName = "Location ID")]
		[PXSelector(typeof(Search<INLocation.locationID, 
			Where<INLocation.siteID, Equal<Current<BCLocations.siteID>>>>),
			SubstituteKey = typeof(INLocation.locationCD),
			DescriptionField = typeof(INLocation.descr)
			)]
		public virtual int? LocationID { get; set; }
		///<inheritdoc cref="LocationID"/>
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion

		#region ExternalLocationID
		/// <summary>
		/// The external ID of the external location.
		/// </summary>
		[PXDBString(20, IsUnicode = true)]
		[PXUIField(DisplayName = "External Location ID")]
		public virtual string ExternalLocationID { get; set; }
		///<inheritdoc cref="ExternalLocationID"/>
		public abstract class externalLocationID : PX.Data.BQL.BqlString.Field<externalLocationID> { }
		#endregion

		#region MappingDirection
		/// <summary>
		/// The direction of the mapping, import or export.
		/// </summary>
		[PXDBString(1)]
		[PXUIField(DisplayName = "Mapping Direction", Visible = false)]
		[BCMappingDirection]
		[PXDefault()]
		public virtual string MappingDirection { get; set; }
		///<inheritdoc cref="MappingDirection"/>
		public abstract class mappingDirection : PX.Data.BQL.BqlString.Field<mappingDirection> { }
		#endregion

		#region SiteCD
		/// <summary>
		/// The code of the site identified by <see cref="SiteID"/>.
		/// </summary>
		public virtual string SiteCD { get; set; }
		#endregion

		#region LocationCD
		/// <summary>
		/// The code of the location identified by <see cref="LocationID"/>.
		/// </summary>
		public virtual string LocationCD { get; set; }
		#endregion
	}

	/// <summary>
	/// Represents an export mapping from locations in the external store to a location/warehouse in the ERP.
	/// </summary>
	[Serializable]
	[PXCacheName("ExportLocations")]
	public class ExportBCLocations : BCLocations
	{
		#region MappingDirection
		/// <summary>
		/// The direction of the mapping, defaulting to export.
		/// </summary>
		[PXDBString(1)]
		[BCMappingDirection]
		[PXDefault(BCMappingDirectionAttribute.Export, PersistingCheck = PXPersistingCheck.Nothing)]
		public override string MappingDirection { get; set; }
		///<inheritdoc cref="MappingDirection"/>
		public abstract class mappingDirection : PX.Data.BQL.BqlString.Field<mappingDirection> { }
		#endregion

		#region SiteID
		/// <summary>
		/// The ID of the warehouse the external location is mapped to.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse")]
		[PXSelector(typeof(INSite.siteID),
					SubstituteKey = typeof(INSite.siteCD),
					DescriptionField = typeof(INSite.descr))]
		[PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>, And<INSite.active, Equal<True>>>),
							 PX.Objects.IN.Messages.TransitSiteIsNotAvailable)]
		[PXDefault()]
		public override int? SiteID { get; set; }
		///<inheritdoc cref="SiteID"/>
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion

		#region LocationID
		/// <summary>
		/// The ID of the location the external location is mapped to.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt()]
		[PXUIField(DisplayName = "Location ID")]
		[PXSelector(typeof(Search<INLocation.locationID,
			Where<INLocation.siteID, Equal<Current<ExportBCLocations.siteID>>, And<INLocation.active, Equal<True>, And<INLocation.salesValid, Equal<True>>>>>),
			SubstituteKey = typeof(INLocation.locationCD),
			DescriptionField = typeof(INLocation.descr)
			)]
		public override int? LocationID { get; set; }
		///<inheritdoc cref="LocationID"/>
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
	}

	/// <summary>
	/// Represents an import mapping from locations in the external store to a location/warehouse in the ERP.
	/// </summary>
	[Serializable]
	[PXCacheName("ImportLocations")]
	public class ImportBCLocations : BCLocations
	{
		#region MappingDirection
		/// <summary>
		/// The direction of the mapping, defaulting to import.
		/// </summary>
		[PXDBString(1)]
		[BCMappingDirection]
		[PXDefault(BCMappingDirectionAttribute.Import, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public override string MappingDirection { get; set; }
		///<inheritdoc cref="MappingDirection"/>
		public abstract class mappingDirection : PX.Data.BQL.BqlString.Field<mappingDirection> { }
		#endregion

		#region SiteID
		/// <summary>
		/// The ID of the warehouse the external location is mapped to.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse")]
		[PXSelector(typeof(INSite.siteID),
					SubstituteKey = typeof(INSite.siteCD),
					DescriptionField = typeof(INSite.descr))]
		[PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>, And<INSite.active, Equal<True>>>),
							 PX.Objects.IN.Messages.TransitSiteIsNotAvailable)]
		[PXDefault()]
		public override int? SiteID { get; set; }
		///<inheritdoc cref="SiteID"/>
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion

		#region LocationID
		/// <summary>
		/// The ID of the location the external location is mapped to.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt()]
		[PXUIField(DisplayName = "Location ID")]
		[PXSelector(typeof(Search<INLocation.locationID,
			Where<INLocation.siteID, Equal<Current<ImportBCLocations.siteID>>, And<INLocation.active, Equal<True>, And<INLocation.salesValid, Equal<True>>>>>),
			SubstituteKey = typeof(INLocation.locationCD),
			DescriptionField = typeof(INLocation.descr)
			)]
		public override int? LocationID { get; set; }
		///<inheritdoc cref="LocationID"/>
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
	}
}
