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

using PX.Common;
using PX.Data;
using System;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.GS1UOMSetup)]
	public class GS1UOMSetup : IBqlTable
	{
		#region Keys
		public static class FK
		{
			//todo public class Kilogram : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<kilogram> { }
			//todo public class Pound : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<pound> { }
			//todo public class Ounce : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<ounce> { }
			//todo public class TroyOunce : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<troyOunce> { }
			
			//todo public class Metre : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<metre> { }
			//todo public class Inch : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<inch> { }
			//todo public class Foot : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<foot> { }
			//todo public class YardOunce : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<yard> { }
			
			//todo public class SquareMetre : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<sqrMetre> { }
			//todo public class SquareInch : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<sqrInch> { }
			//todo public class SquareFoot : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<sqrFoot> { }
			//todo public class SquareYard : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<sqrYard> { }
			
			//todo public class CubicMetre : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<cubicMetre> { }
			//todo public class CubicInch : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<cubicInch> { }
			//todo public class CubicFoot : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<cubicFoot> { }
			//todo public class CubicYard : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<cubicYard> { }
			//todo public class Litre : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<litre> { }
			//todo public class Quart : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<quart> { }
			//todo public class GallonUS : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<gallonUS> { }

			//todo public class KilogramPerSqrMetre : INUnit.UK.ByGlobal.ForeignKeyOf<GS1UOMSetup>.By<kilogramPerSqrMetre> { }
		}
		#endregion

		#region Weight
		#region Kilogram
		[INUnit(DisplayName = "Kilogram")]
		public virtual String Kilogram { get; set; }
		public abstract class kilogram : PX.Data.BQL.BqlString.Field<kilogram> { }
		#endregion
		#region Pound
		[INUnit(DisplayName = "Pound")]
		public virtual String Pound { get; set; }
		public abstract class pound : PX.Data.BQL.BqlString.Field<pound> { }
		#endregion
		#region Ounce
		[INUnit(DisplayName = "Ounce")]
		public virtual String Ounce { get; set; }
		public abstract class ounce : PX.Data.BQL.BqlString.Field<ounce> { }
		#endregion
		#region TroyOunce
		[INUnit(DisplayName = "Troy Ounce")]
		public virtual String TroyOunce { get; set; }
		public abstract class troyOunce : PX.Data.BQL.BqlString.Field<troyOunce> { }
		#endregion
		#endregion
		#region Length
		#region Metre
		[INUnit(DisplayName = "Metre")]
		public virtual String Metre { get; set; }
		public abstract class metre : PX.Data.BQL.BqlString.Field<metre> { }
		#endregion
		#region Inch
		[INUnit(DisplayName = "Inch")]
		public virtual String Inch { get; set; }
		public abstract class inch : PX.Data.BQL.BqlString.Field<inch> { }
		#endregion
		#region Foot
		[INUnit(DisplayName = "Foot")]
		public virtual String Foot { get; set; }
		public abstract class foot : PX.Data.BQL.BqlString.Field<foot> { }
		#endregion
		#region Yard
		[INUnit(DisplayName = "Yard")]
		public virtual String Yard { get; set; }
		public abstract class yard : PX.Data.BQL.BqlString.Field<yard> { }
		#endregion
		#endregion
		#region Area
		#region SqrMetre
		[INUnit(DisplayName = "Square Metre")]
		public virtual String SqrMetre { get; set; }
		public abstract class sqrMetre : PX.Data.BQL.BqlString.Field<sqrMetre> { }
		#endregion
		#region SqrInch
		[INUnit(DisplayName = "Square Inch")]
		public virtual String SqrInch { get; set; }
		public abstract class sqrInch : PX.Data.BQL.BqlString.Field<sqrInch> { }
		#endregion
		#region SqrFoot
		[INUnit(DisplayName = "Square Foot")]
		public virtual String SqrFoot { get; set; }
		public abstract class sqrFoot : PX.Data.BQL.BqlString.Field<sqrFoot> { }
		#endregion
		#region SqrYard
		[INUnit(DisplayName = "Square Yard")]
		public virtual String SqrYard { get; set; }
		public abstract class sqrYard : PX.Data.BQL.BqlString.Field<sqrYard> { }
		#endregion
		#endregion
		#region Volume
		#region CubicMetre
		[INUnit(DisplayName = "Cubic Metre")]
		public virtual String CubicMetre { get; set; }
		public abstract class cubicMetre : PX.Data.BQL.BqlString.Field<cubicMetre> { }
		#endregion
		#region CubicInch
		[INUnit(DisplayName = "Cubic Inch")]
		public virtual String CubicInch { get; set; }
		public abstract class cubicInch : PX.Data.BQL.BqlString.Field<cubicInch> { }
		#endregion
		#region CubicFoot
		[INUnit(DisplayName = "Cubic Foot")]
		public virtual String CubicFoot { get; set; }
		public abstract class cubicFoot : PX.Data.BQL.BqlString.Field<cubicFoot> { }
		#endregion
		#region CubicYard
		[INUnit(DisplayName = "Cubic Yard")]
		public virtual String CubicYard { get; set; }
		public abstract class cubicYard : PX.Data.BQL.BqlString.Field<cubicYard> { }
		#endregion
		#region Litre
		[INUnit(DisplayName = "Litre")]
		public virtual String Litre { get; set; }
		public abstract class litre : PX.Data.BQL.BqlString.Field<litre> { }
		#endregion
		#region Quart
		[INUnit(DisplayName = "Quart")]
		public virtual String Quart { get; set; }
		public abstract class quart : PX.Data.BQL.BqlString.Field<quart> { }
		#endregion
		#region GallonUS
		[INUnit(DisplayName = "Gallon U.S.")]
		public virtual String GallonUS { get; set; }
		public abstract class gallonUS : PX.Data.BQL.BqlString.Field<gallonUS> { }
		#endregion
		#endregion
		#region KilogramPerSqrMetre
		[INUnit(DisplayName = "Kilogram per Square Metre")]
		public virtual String KilogramPerSqrMetre { get; set; }
		public abstract class kilogramPerSqrMetre : PX.Data.BQL.BqlString.Field<kilogramPerSqrMetre> { }
		#endregion

		#region tstamp
		public abstract class Tstamp : Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}

	public static class GS1UOMSetupExt
	{
		public static string GetUOMOf(this GS1UOMSetup setup, PX.Common.GS1.AI ai)
		{
			if (ai.Format != PX.Common.GS1.DataType.Decimal)
				return null;

			if (ai.IsIn(PX.Common.GS1.Codes.KilogramCodes))
				return setup.Kilogram;
			if (ai.IsIn(PX.Common.GS1.Codes.PoundCodes))
				return setup.Pound;
			if (ai.IsIn(PX.Common.GS1.Codes.OunceCodes))
				return setup.Ounce;
			if (ai.IsIn(PX.Common.GS1.Codes.TroyOunceCodes))
				return setup.TroyOunce;

			if (ai.IsIn(PX.Common.GS1.Codes.KilogramPerSqrMetreCodes))
				return setup.KilogramPerSqrMetre;

			if (ai.IsIn(PX.Common.GS1.Codes.MetreCodes))
				return setup.Metre;
			if (ai.IsIn(PX.Common.GS1.Codes.InchCodes))
				return setup.Inch;
			if (ai.IsIn(PX.Common.GS1.Codes.FootCodes))
				return setup.Foot;
			if (ai.IsIn(PX.Common.GS1.Codes.YardCodes))
				return setup.Yard;

			if (ai.IsIn(PX.Common.GS1.Codes.SqrMetreCodes))
				return setup.SqrMetre;
			if (ai.IsIn(PX.Common.GS1.Codes.SqrInchCodes))
				return setup.SqrInch;
			if (ai.IsIn(PX.Common.GS1.Codes.SqrFootCodes))
				return setup.SqrFoot;
			if (ai.IsIn(PX.Common.GS1.Codes.SqrYardCodes))
				return setup.SqrYard;

			if (ai.IsIn(PX.Common.GS1.Codes.CubicMetreCodes))
				return setup.CubicMetre;
			if (ai.IsIn(PX.Common.GS1.Codes.CubicInchCodes))
				return setup.CubicInch;
			if (ai.IsIn(PX.Common.GS1.Codes.CubicFootCodes))
				return setup.CubicFoot;
			if (ai.IsIn(PX.Common.GS1.Codes.CubicYardCodes))
				return setup.CubicYard;
			if (ai.IsIn(PX.Common.GS1.Codes.LitreCodes))
				return setup.Litre;
			if (ai.IsIn(PX.Common.GS1.Codes.QuartCodes))
				return setup.Quart;
			if (ai.IsIn(PX.Common.GS1.Codes.GallonUSCodes))
				return setup.GallonUS;

			return null;
		}
	}
}
