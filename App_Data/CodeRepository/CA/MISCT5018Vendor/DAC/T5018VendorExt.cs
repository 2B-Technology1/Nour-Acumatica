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
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.Localizations.CA.Messages;

namespace PX.Objects.Localizations.CA
{
	/// <summary>
	/// An extension of <see cref="Vendor"/> that contains T5018-specific fields.
	/// </summary>
	public sealed class T5018VendorExt : PXCacheExtension<BAccount>
    {
        #region IsActive

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
        }

		#endregion

		#region VendorT5018
		public abstract class vendorT5018 : BqlType<IBqlBool, bool>.Field<vendorT5018>
		{
		}
		/// <summary>
		/// A Boolean value that indicates whether the vendor should be included in T5018 reporting.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "T5018 Vendor")]
		public bool? VendorT5018
		{
			get;
			set;
		} 
		#endregion

		#region BoxT5018
		public abstract class boxT5018 : BqlType<IBqlInt, int>.Field<boxT5018>
		{
		#region BoxT5018Constants
		public const int Corporation = 1;
		public class corporation : BqlInt.Constant<corporation>
		{
			public corporation() :
				base(Corporation)
			{ }
		}

		public const int Partnership = 2;
		public class partnership : BqlInt.Constant<partnership>
		{
			public partnership() :
				base(Partnership)
			{ }
		}

		public const int Individual = 3;
		public class individual : BqlInt.Constant<individual>
		{
			public individual() :
				base(Individual)
			{ }
		}
		#endregion
		}
		/// <summary>
		/// The type of vendor for T5018 reporting.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <list>
		/// <item><description><c>1</c>: Corporation</description></item>
		/// <item><description><c>2</c>: Partnership</description></item>
		/// <item><description><c>3</c>: Individual</description></item>
		/// </list>
		/// </value>
		[PXDBInt]
		[PXIntList(new int[] { boxT5018.Corporation, boxT5018.Partnership, boxT5018.Individual }, new string[] { T5018Messages.Corporation, T5018Messages.Partnership, T5018Messages.Individual })]
		[PXUIField(DisplayName = "T5018 Box")]
		public int? BoxT5018
		{
			get;
			set;
		}
		#endregion

		#region SocialInsuranceNumber
		public abstract class socialInsNum : BqlType<IBqlString, string>.Field<socialInsNum>
		{
		}
		/// <summary>
		/// The social insurance number of the vendor if the vendor is an individual.
		/// </summary>
		[PXDBString(9, InputMask = "#########")]
		[PXUIField(DisplayName = "SIN")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public string SocialInsNum
		{
			get;
			set;
		}
		#endregion

		#region BusinessNumber
		public abstract class businessNumber : BqlString.Field<businessNumber> { }
		/// <summary>
		/// The tax registration ID of the vendor.
		/// </summary>
		[PXDBString(70)]
		[PXUIField(DisplayName = "Tax Registration ID")]
		public string BusinessNumber
		{
			get;
			set;
		}
		#endregion
	}

	/// <summary>
	/// An extension of <see cref="VendorR"/> that contains T5018-specific fields.
	/// </summary>
	public sealed class T5018VendorRExt : PXCacheExtension<BAccountR> {
		#region IsActive

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
		}

		#endregion

		#region VendorT5018
		public abstract class vendorT5018 : BqlType<IBqlBool, bool>.Field<vendorT5018> {
		}
		/// <summary>
		/// A Boolean value that indicates whether the vendor should be included in T5018 reporting.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "T5018 Vendor")]
		public bool? VendorT5018
		{
			get;
			set;
		}
		#endregion

		#region BoxT5018
		public abstract class boxT5018 : BqlType<IBqlInt, int>.Field<boxT5018> {
			#region BoxT5018Constants
			public const int Corporation = 1;
			public class corporation : BqlInt.Constant<corporation> {
				public corporation() :
					base(Corporation)
				{ }
			}

			public const int Partnership = 2;
			public class partnership : BqlInt.Constant<partnership> {
				public partnership() :
					base(Partnership)
				{ }
			}

			public const int Individual = 3;
			public class individual : BqlInt.Constant<individual> {
				public individual() :
					base(Individual)
				{ }
			}
			#endregion
		}
		/// <summary>
		/// The type of vendor for T5018 reporting.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <list>
		/// <item><description><c>1</c>: Corporation</description></item>
		/// <item><description><c>2</c>: Partnership</description></item>
		/// <item><description><c>3</c>: Individual</description></item>
		/// </list>
		/// </value>
		[PXDBInt]
		[PXIntList(new int[] { boxT5018.Corporation, boxT5018.Partnership, boxT5018.Individual }, new string[] { T5018Messages.Corporation, T5018Messages.Partnership, T5018Messages.Individual })]
		[PXUIField(DisplayName = "T5018 Box")]
		public int? BoxT5018
		{
			get;
			set;
		}
		#endregion

		#region SocialInsuranceNumber
		public abstract class socialInsNum : BqlType<IBqlString, string>.Field<socialInsNum> {
		}
		/// <summary>
		/// The social insurance number of the vendor if the vendor is an individual.
		/// </summary>
		[PXDBString(9, InputMask = "#########")]
		[PXUIField(DisplayName = "SIN")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public string SocialInsNum
		{
			get;
			set;
		}
		#endregion

		#region BusinessNumber
		public abstract class businessNumber : BqlString.Field<businessNumber> { }
		/// <summary>
		/// The tax registration ID of the vendor.
		/// </summary>
		[PXDBString(70)]
		[PXUIField(DisplayName = "Tax Registration ID")]
		public string BusinessNumber
		{
			get;
			set;
		}
		#endregion
	}
}
