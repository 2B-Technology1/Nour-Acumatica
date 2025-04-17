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


namespace PX.Objects.IN.AffectedAvailability
{
	public class EntityType
	{
		public class soOrder : PX.Data.BQL.BqlString.Constant<soOrder>
		{
			public soOrder() : base(SOOrder) { }
		}

		public class soShipment : PX.Data.BQL.BqlString.Constant<soShipment>
		{
			public soShipment() : base(SOShipment) { }
		}

		public class inRegister : PX.Data.BQL.BqlString.Constant<inRegister>
		{
			public inRegister() : base(INRegister) { }
		}

		public class inKitRegister : PX.Data.BQL.BqlString.Constant<inKitRegister>
		{
			public inKitRegister() : base(INKitRegister) { }
		}

		public static readonly string SOOrder = typeof(PX.Objects.SO.SOOrder).FullName;

		public static readonly string SOShipment = typeof(PX.Objects.SO.SOShipment).FullName;

		public static readonly string INRegister = typeof(PX.Objects.IN.INRegister).FullName;

		public static readonly string INKitRegister = typeof(PX.Objects.IN.INKitRegister).FullName;
	}
}
