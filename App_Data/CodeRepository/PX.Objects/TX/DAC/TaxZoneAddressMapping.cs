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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Data.BQL;

namespace PX.Objects.TX
{
	[PXCacheName(Messages.TaxZoneAddressMapping)]
	public partial class TaxZoneAddressMapping : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<TaxZoneAddressMapping>.By<countryID, stateID, fromPostalCode, taxZoneID>
		{
			public static TaxZoneAddressMapping Find(PXGraph graph, String countryID, String stateID, String fromPostalCode, String taxZoneID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, countryID, stateID, fromPostalCode, taxZoneID, options);
		}
		public static class FK
		{
			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<TaxZoneAddressMapping>.By<taxZoneID> { }
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		[PXParent(typeof(Select<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<TaxZoneAddressMapping.taxZoneID>>>>))]
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXDefault(typeof(TaxZone.taxZoneID))]
		public virtual String TaxZoneID { get; set; }
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		[PXUIField(DisplayName = "Country", Required = true)]
		[PXDBString(2, IsKey = true)]
		[PXSelector(typeof(Country.countryID), CacheGlobal = true, DescriptionField = typeof(Country.description))]
		[PXDefault(typeof(TaxZone.countryID))]
		public virtual String CountryID{ get; set;}
		#endregion
		#region State
		public abstract class stateID : PX.Data.BQL.BqlString.Field<stateID> { }
		[PXUIField(DisplayName = "State", Required = true)]
		[PXDBString(50, IsUnicode = true, IsKey = true)]
		[State(typeof(TaxZoneAddressMapping.countryID))]
		[PXUIRequired(typeof(Where<TaxZone.mappingType, Equal<MappingTypesAttribute.oneOrMoreStates>>))]
		[PXDefault("")]
		public virtual String StateID { get; set; }
		#endregion
		#region Name
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXUIField(DisplayName = "Name", Enabled = false)]
		[PXString(20)]
		[PXUIVisible(typeof(Where<TaxZone.mappingType.FromCurrent, NotEqual<MappingTypesAttribute.oneOrMorePostalCodes>>))]
		[PXFormula(typeof(Switch<Case<Where<TaxZone.mappingType.FromCurrent, Equal<MappingTypesAttribute.oneOrMoreStates>>, Selector<TaxZoneAddressMapping.stateID, State.name>>, Selector<TaxZoneAddressMapping.countryID, Country.description>>))]
		public virtual String Description { get; set; }
		#endregion
		#region FromPostalCode
		public abstract class fromPostalCode : PX.Data.BQL.BqlString.Field<fromPostalCode> { }
		[PXUIField(DisplayName = "From Postal Code", Required = true)]
		[PXDBString(20, IsKey = true, InputMask = "")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(TaxZoneAddressMapping.countryID))]
		[PXUIRequired(typeof(Where<TaxZone.mappingType, Equal<MappingTypesAttribute.oneOrMorePostalCodes>>))]
		[PXDefault("")]
		public virtual String FromPostalCode { get; set;}
		#endregion
		#region ToPostalCode
		public abstract class toPostalCode : PX.Data.BQL.BqlString.Field<toPostalCode> { }
		[PXUIField(DisplayName = "To Postal Code", Required = true)]
		[PXDBString(20)]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(TaxZoneAddressMapping.countryID))]
		[PXUIRequired(typeof(Where<TaxZone.mappingType, Equal<MappingTypesAttribute.oneOrMorePostalCodes>>))]
		[PXDefault("")]
		public virtual String ToPostalCode { get; set; }
		#endregion
		#region ToPostalCodeCalced
		public abstract class toPostalCodeSuffixed : PX.Data.BQL.BqlString.Field<toPostalCodeSuffixed> { }
		[PXString(20)]
		[PXDBCalced(typeof(IIf<TaxZoneAddressMapping.fromPostalCode.IsEqual<Space>, Space, TaxZoneAddressMapping.toPostalCode.Concat<ToPostalCodeSuffix>>), typeof(string))]
		public virtual String ToPostalCodeSuffixed { get; set; }
		#endregion
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
	}

	public class ToPostalCodeSuffix : BqlType<IBqlString, string>.Constant<ToPostalCodeSuffix>
	{
		public const string ToPostalCodeSuffixCharacter = "ZZZZZZZZZZZZZZZZZZZZ";
		public ToPostalCodeSuffix() : base(ToPostalCodeSuffixCharacter) {}
	}
}
