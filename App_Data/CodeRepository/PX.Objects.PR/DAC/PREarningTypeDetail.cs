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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// A record determining whether or not an earning type is taxable by a certain tax code.
	/// </summary>
	[PXCacheName(Messages.PREarningTypeDetail)]
	[Serializable]
	public class PREarningTypeDetail : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREarningTypeDetail>.By<typecd, taxID, countryID>
		{
			public static PREarningTypeDetail Find(PXGraph graph, string typeCD, int? taxID, string countryID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, typeCD, taxID, countryID, options);
		}

		public static class FK
		{
			public class EarningType : EPEarningType.PK.ForeignKeyOf<PREarningTypeDetail>.By<typecd> { }
			public class TaxCode : PRTaxCode.PK.ForeignKeyOf<PREarningTypeDetail>.By<taxID> { }
		}
		#endregion

		#region TypeCD
		public abstract class typecd : BqlString.Field<typecd> { }
		/// <summary>
		/// The user-friendly unique identifier of the earning type.
		/// The field is included in <see cref="FK.EarningType"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPEarningType.TypeCD"/> field.
		/// </value>
		[PXDBString(EPEarningType.typeCD.Length, IsKey = true, IsUnicode = true)]
		[PXDefault(typeof(EPEarningType.typeCD))]
		[PXUIField(DisplayName = "Earning Type Code")]
		[PXParent(typeof(FK.EarningType))]
		public string TypeCD { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : BqlInt.Field<taxID> { }
		/// <summary>
		/// The unique identifier of the tax code.
		/// The field is included in <see cref="FK.TaxCode"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRTaxCode.TaxID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Tax Code")]
		[PXSelector(typeof(
			SearchFor<PRTaxCode.taxID>
				.Where<PRTaxCode.countryID.IsEqual<countryID.FromCurrent>>),
			DescriptionField = typeof(PRTaxCode.description),
			SubstituteKey = typeof(PRTaxCode.taxCD))]
		[PXParent(typeof(FK.TaxCode))]
		public int? TaxID { get; set; }
		#endregion
		#region Taxability
		public abstract class taxability : PX.Data.BQL.BqlInt.Field<taxability> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the tax code may be applied to the earning type.
		/// </summary>
		[PXDBInt]
		[EarningTypeTaxability(typeof(countryID), typeof(taxID))]
		public int? Taxability { get; set; }
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PRCountry]
		public string CountryID { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
