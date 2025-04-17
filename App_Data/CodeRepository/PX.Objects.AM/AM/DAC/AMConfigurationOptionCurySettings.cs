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
using PX.Objects.AM.Attributes;
using System;

namespace PX.Objects.AM
{
	/// <summary>
	/// The maintenance table for product configurator option based on the base currency. The data from this table is not directly visible in the UI.
	/// Parent: <see cref="AMConfigurationOption"/>
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ConfigOptionCurySettings)]
	public class AMConfigurationOptionCurySettings : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<AMConfigurationOptionCurySettings>.By<configurationID, revision, configFeatureLineNbr, lineNbr, curyID>
		{
			public static AMConfigurationOptionCurySettings Find(PXGraph graph, string configurationID, string revision, int? configFeatureLineNbr, int? lineNbr,
				string curyID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, configurationID, revision, configFeatureLineNbr, lineNbr, curyID, options);
		}
		public static class FK
		{
			public class ConfigOption : AMConfigurationOption.PK.ForeignKeyOf<AMConfigurationOptionCurySettings>.By<configurationID, revision, configFeatureLineNbr, lineNbr> { }
		}
		#endregion

		#region ConfigurationID (key)
		/// <summary>
		/// key field
		/// </summary>
		public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

		protected string _ConfigurationID;
		/// <summary>
		/// key field
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Configuration ID", Visible = false, Enabled = false)]
		[PXDBDefault(typeof(AMConfigurationOption.configurationID))]
		[PXParent(typeof(FK.ConfigOption))]
		public virtual string ConfigurationID
		{
			get
			{
				return this._ConfigurationID;
			}
			set
			{
				this._ConfigurationID = value;
			}
		}
		#endregion
		#region Revision (key)
		/// <summary>
		/// key field
		/// </summary>
		public abstract class revision : PX.Data.BQL.BqlString.Field<revision> { }

		protected string _Revision;
		/// <summary>
		/// key field
		/// </summary>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(AMConfigurationOption.revision))]
		[PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
		public virtual string Revision
		{
			get
			{
				return this._Revision;
			}
			set
			{
				this._Revision = value;
			}
		}
		#endregion
		#region ConfigFeatureLineNbr (key)
		/// <summary>
		/// key field
		/// </summary>
		public abstract class configFeatureLineNbr : PX.Data.BQL.BqlInt.Field<configFeatureLineNbr> { }

		protected int? _ConfigFeatureLineNbr;
		/// <summary>
		/// key field
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Feature Line Nbr", Visible = false, Enabled = false)]
		[PXDBDefault(typeof(AMConfigurationOption.configFeatureLineNbr))]
		public virtual int? ConfigFeatureLineNbr
		{
			get
			{
				return this._ConfigFeatureLineNbr;
			}
			set
			{
				this._ConfigFeatureLineNbr = value;
			}
		}
		#endregion
		#region LineNbr (key)
		/// <summary>
		/// key field
		/// </summary>
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		protected int? _LineNbr;
		/// <summary>
		/// key field
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(AMConfigurationOption.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr", Visible = false, Enabled = false)]
		public virtual int? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		protected String _CuryID;
		[PXDBString(10, IsKey = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, Required = false)]
		[PXDefault(typeof(AccessInfo.baseCuryID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXUIField(DisplayName = "Site ID", Visible = false, Enabled = false)]
		[AMSite]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		protected Int32? _LocationID;
		[PXDBInt]
		[PXUIField(DisplayName = "Location ID", Visible = false, Enabled = false)]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID

		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime

		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID

		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID

		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}
