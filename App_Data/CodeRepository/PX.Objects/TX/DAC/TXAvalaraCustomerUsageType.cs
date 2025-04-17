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
using PX.TaxProvider;

namespace PX.Objects.TX
{
	public static class TXAvalaraCustomerUsageType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(FederalGovt, Messages.FederalGovt),
					Pair(StateLocalGovt, Messages.StateLocalGovt),
					Pair(TribalGovt, Messages.TribalGovt),
					Pair(ForeignDiplomat, Messages.ForeignDiplomat),
					Pair(CharitableOrg, Messages.CharitableOrg),
					Pair(Religious, Messages.Religious),
					Pair(Resale, Messages.Resale),
					Pair(AgriculturalProd, Messages.AgriculturalProd),
					Pair(IndustrialProd, Messages.IndustrialProd),
					Pair(DirectPayPermit, Messages.DirectPayPermit),
					Pair(DirectMail, Messages.DirectMail),
					Pair(Other, Messages.Other),
					Pair(Education, Messages.Education),
					Pair(LocalGovt, Messages.LocalGovt),
					Pair(ComAquaculture, Messages.ComAquaculture),
					Pair(ComFishery, Messages.ComFishery),
					Pair(NonResident, Messages.NonResident),
					Pair(Taxable, Messages.Taxable),
					Pair(Default, Messages.Default)
				}) { }
		}

		public const string FederalGovt = "A";
		public const string StateLocalGovt = "B";
		public const string TribalGovt = "C";
		public const string ForeignDiplomat = "D";
		public const string CharitableOrg = "E";
		public const string Religious = "F";
		public const string Resale = "G";
		public const string AgriculturalProd = "H";
		public const string IndustrialProd = "I";
		public const string DirectPayPermit = "J";
		public const string DirectMail = "K";
		public const string Other = "L";
		public const string Education = "M";
		public const string LocalGovt = "N";
		public const string ComAquaculture = "P";
		public const string ComFishery = "Q";
		public const string NonResident = "R";
		public const string Taxable = EntityUsageType.Taxable;
		public const string Default = EntityUsageType.Default;
	}
}
