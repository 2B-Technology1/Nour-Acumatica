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
using PX.Common;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common;
using PX.Objects.Common.Attributes;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.Attributes;

namespace PX.Objects.IN.Matrix.DAC
{
	[PXCacheName(Messages.INMatrixExcludedData)]
	public class INMatrixExcludedData : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INMatrixExcludedData>.By<templateID, type, tableName, fieldName>
		{
			public static INMatrixExcludedData Find(PXGraph graph, int? templateID, string type, string tableName, string fieldName, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, templateID, type, tableName, fieldName, options);
		}
		public static class FK
		{
			public class TemplateInventoryItem : InventoryItem.PK.ForeignKeyOf<INMatrixGenerationRule>.By<templateID> { }
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type>
		{
			public const string Field = "F";
			public const string Attribute = "A";

			[PXLocalizable]
			public class DisplayNames
			{
				public const string Field = "Field";
				public const string Attribute = "Attribute";
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					new[]
					{
						Pair(Field, DisplayNames.Field),
						Pair(Attribute, DisplayNames.Attribute),
					})
				{ }
			}

			public class field : PX.Data.BQL.BqlString.Constant<field>
			{
				public field() : base(Field) { }
			}

			public class attribute : PX.Data.BQL.BqlString.Constant<attribute>
			{
				public attribute() : base(Attribute) { }
			}
		}

		/// <summary>
		/// Type of row: 'F' - field, 'A' - attribute.
		/// </summary>
		[PXDBString(1, IsKey = true, IsFixed = true, IsUnicode = false)]
		[type.List]
		public virtual string Type
		{
			get;
			set;
		}
		#endregion
		#region TableName
		public abstract class tableName : PX.Data.BQL.BqlString.Field<tableName> { }

		/// <summary>
		/// References to a DAC name.
		/// </summary>
		[PXDBString(255, IsKey = true)]
		[PXUIField(DisplayName = "Table Name", Required = true)]
		[PXDefault]
		[PXStringList]
		public virtual string TableName
		{
			get;
			set;
		}
		#endregion
		#region FieldName
		public abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }

		/// <summary>
		/// References to field name of related DAC <see cref="TableName"/>.
		/// </summary>
		[PXDBString(255, IsKey = true)]
		[PXUIField(DisplayName = "Field Name", Required = true)]
		[PXDefault]
		public virtual string FieldName
		{
			get;
			set;
		}
		#endregion
		#region TemplateID
		public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }

		/// <summary>
		/// Template Inventory Item identifier.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		public virtual int? TemplateID
		{
			get;
			set;
		}
		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		/// <summary>
		/// Indicates if the exclusion is active at the moment
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote()]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
