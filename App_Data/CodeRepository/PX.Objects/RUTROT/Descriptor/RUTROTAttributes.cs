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

namespace PX.Objects.RUTROT
{
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class RUTROTTypes
	{
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { RUT, ROT }, new string[] { RUTROTMessages.RUTType, RUTROTMessages.ROTType }) { }
		}

		public const string RUT = "U";

		public const string ROT = "O";

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class rut : PX.Data.BQL.BqlString.Constant<rut>
		{
			public rut() : base(RUT) { }
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class rot : PX.Data.BQL.BqlString.Constant<rot>
		{
			public rot() : base(ROT) { }
		}
	}

	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class RUTROTItemTypes
	{
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Service, MaterialCost, OtherCost }, 
					  new string[] { RUTROTMessages.Service, RUTROTMessages.MaterialCost, RUTROTMessages.OtherCost }) { }
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class CostsListAttribute : PXStringListAttribute
        {
            public CostsListAttribute()
                : base(new string[] { MaterialCost, OtherCost }, new string[] { RUTROTMessages.MaterialCost, RUTROTMessages.OtherCost }) {; }
        }

        public const string Service = "S";

		public const string MaterialCost = "M";

		public const string OtherCost = "O";

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class service : PX.Data.BQL.BqlString.Constant<service>
		{
			public service() : base(Service) { }
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class materialCost : PX.Data.BQL.BqlString.Constant<materialCost>
		{
			public materialCost() : base(MaterialCost) { }
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class otherCost : PX.Data.BQL.BqlString.Constant<otherCost>
		{
			public otherCost() : base(OtherCost) { }
		}
	}

	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class WorkTypeSelectorAttribute : PXSelectorAttribute
	{
		internal static Type ComposeSearchCommand(Type dateField)
		{
			if (!typeof(IBqlField).IsAssignableFrom(dateField))
			{
				throw new PXArgumentException(nameof(dateField));
			}
			return BqlCommand.Compose(
				typeof(Search<,>),
				typeof(RUTROTWorkType.workTypeID),
				typeof(Where2<,>),
				typeof(Where<,,>),
				typeof(RUTROTWorkType.endDate),
				typeof(Greater<>),
				typeof(Current<>),
				dateField,
				typeof(Or<,>),
				typeof(RUTROTWorkType.endDate),
				typeof(IsNull),
				typeof(And<,,>),
				typeof(RUTROTWorkType.startDate),
				typeof(LessEqual<>),
				typeof(Current<>),
				dateField,
				typeof(And<RUTROTWorkType.rUTROTType, Equal<Current2<RUTROT.rUTROTType>>, Or<Current2<RUTROT.rUTROTType>, IsNull>>));
		}

		public WorkTypeSelectorAttribute(Type date) : base(ComposeSearchCommand(date))
		{
			SubstituteKey = typeof(RUTROTWorkType.description);
			DescriptionField = typeof(RUTROTWorkType.xmlTag);
		}
	}

	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class RUTROTBalanceOn
    {
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[] { Release, Claim }, new string[] { RUTROTMessages.Release, RUTROTMessages.Claim }) { }
        }

        public const string Release = "R";

        public const string Claim = "C";

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class release : PX.Data.BQL.BqlString.Constant<release>
		{
            public release() : base(Release) { }
        }

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public class claim : PX.Data.BQL.BqlString.Constant<claim>
		{
            public claim() : base(Claim) { }
        }
    }
}
