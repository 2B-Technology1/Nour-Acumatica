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

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the position of different fields on the Canadian Annual form.
	/// </summary>
	[PXCacheName(Messages.PRGovernmentSlipField)]
	[Serializable]
	public class PRGovernmentSlipField : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRGovernmentSlipField>.By<slipName, year, fieldCode, page>
		{
			public static PRGovernmentSlipField Find(PXGraph graph, string slipName, string year, string fieldCode, int? page, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, slipName, year, fieldCode, page, options);
		}

		public static class FK
		{
			public class GovernmentSlip : PRGovernmentSlip.PK.ForeignKeyOf<PRGovernmentSlipField>.By<slipName, year> { }
		}
		#endregion

		#region SlipName
		public abstract class slipName : PX.Data.BQL.BqlString.Field<slipName> { }
		[PXDBString(50, IsKey = true, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Slip Name")]
		public virtual string SlipName { get; set; }
		#endregion
		#region Year
		public abstract class year : PX.Data.BQL.BqlInt.Field<year> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXParent(typeof(FK.GovernmentSlip))]
		[PXUIField(DisplayName = "Year")]
		public virtual int? Year { get; set; }
		#endregion
		#region Page
		public abstract class page : PX.Data.BQL.BqlInt.Field<page> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Page")]
		public virtual int? Page { get; set; }
		#endregion
		#region FieldCode
		public abstract class fieldCode : PX.Data.BQL.BqlString.Field<fieldCode> { }
		[PXDBString(255, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Field Code")]
		public virtual string FieldCode { get; set; }
		#endregion
		#region FieldName
		public abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }
		[PXDBString(50, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Field Name")]
		public virtual string FieldName { get; set; }
		#endregion
		#region DataType
		public abstract class dataType : PX.Data.BQL.BqlString.Field<dataType> { }
		[PXDBString(50)]
		[PXDefault]
		[PXUIField(DisplayName = "Data Type")]
		public virtual string DataType { get; set; }
		#endregion
		#region Fillable
		public abstract class fillable : PX.Data.BQL.BqlBool.Field<fillable> { }
		[PXDBBool]
		[PXDefault]
		[PXUIField(DisplayName = "Fillable")]
		public virtual bool? Fillable { get; set; }
		#endregion
		#region Multiline
		public abstract class multiline : PX.Data.BQL.BqlBool.Field<multiline> { }
		[PXDBBool]
		[PXDefault]
		[PXUIField(DisplayName = "Multiline")]
		public virtual bool? Multiline { get; set; }
		#endregion
		#region FontName
		public abstract class fontName : PX.Data.BQL.BqlString.Field<fontName> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Font Name")]
		public virtual string FontName { get; set; }
		#endregion
		#region FontSize
		public abstract class fontSize : PX.Data.BQL.BqlDouble.Field<fontSize> { }
		[PXDBDouble]
		[PXUIField(DisplayName = "Font Size")]
		public virtual double? FontSize { get; set; }
		#endregion
		#region Color
		public abstract class color : PX.Data.BQL.BqlString.Field<color> { }
		[PXDBString(50)]
		[PXUIField(DisplayName = "Color")]
		public virtual string Color { get; set; }
		#endregion
		#region Alignment
		public abstract class alignment : PX.Data.BQL.BqlString.Field<alignment> { }
		[PXDBString(50)]
		[PXUIField(DisplayName = "Alignment")]
		public virtual string Alignment { get; set; }
		#endregion
		#region LeftX
		public abstract class leftX : PX.Data.BQL.BqlDouble.Field<leftX> { }
		[PXDBDouble]
		[PXUIField(DisplayName = "Left X")]
		public virtual double? LeftX { get; set; }
		#endregion
		#region TopY
		public abstract class topY : PX.Data.BQL.BqlDouble.Field<topY> { }
		[PXDBDouble]
		[PXUIField(DisplayName = "Top Y")]
		public virtual double? TopY { get; set; }
		#endregion
		#region Width
		public abstract class width : PX.Data.BQL.BqlDouble.Field<width> { }
		[PXDBDouble]
		[PXUIField(DisplayName = "Width")]
		public virtual double? Width { get; set; }
		#endregion
		#region Height
		public abstract class height : PX.Data.BQL.BqlDouble.Field<height> { }
		[PXDBDouble]
		[PXUIField(DisplayName = "Height")]
		public virtual double? Height { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
