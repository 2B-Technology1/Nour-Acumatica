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

namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using System.Diagnostics;
	
	[System.SerializableAttribute()]
	[DebuggerDisplay("{ZipCode}-{CountyName}:[{Plus4PortionOfZipCode}-{Plus4PortionOfZipCode2}]")]
	public partial class TXImportZipFileData : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<TXImportZipFileData>.By<recordID>
		{
			public static TXImportZipFileData Find(PXGraph graph, Int32? recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
		}
		#endregion
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region ZipCode
		public abstract class zipCode : PX.Data.BQL.BqlString.Field<zipCode> { }
		protected String _ZipCode;
		[PXUIField(DisplayName="Zip Code")]
		[PXDBString(25)]
		[PXDefault("")]
		public virtual String ZipCode
		{
			get
			{
				return this._ZipCode;
			}
			set
			{
				this._ZipCode = value;
			}
		}
		#endregion
		#region StateCode
		public abstract class stateCode : PX.Data.BQL.BqlString.Field<stateCode> { }
		protected String _StateCode;
		[PXUIField(DisplayName = "State")]
		[PXDBString(25)]
		[PXDefault("")]
		public virtual String StateCode
		{
			get
			{
				return this._StateCode;
			}
			set
			{
				this._StateCode = value;
			}
		}
		#endregion
		#region CountyName
		public abstract class countyName : PX.Data.BQL.BqlString.Field<countyName> { }
		protected String _CountyName;
        [PXUIField(DisplayName = "Country Name")]
		[PXDBString(25)]
		[PXDefault("")]
		public virtual String CountyName
		{
			get
			{
				return this._CountyName;
			}
			set
			{
				this._CountyName = value;
			}
		}
		#endregion
		#region Plus4PortionOfZipCode
		public abstract class plus4PortionOfZipCode : PX.Data.BQL.BqlInt.Field<plus4PortionOfZipCode> { }
		protected Int32? _Plus4PortionOfZipCode;
		[PXUIField(DisplayName = "Zip Plus Min.")]
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? Plus4PortionOfZipCode
		{
			get
			{
				return this._Plus4PortionOfZipCode;
			}
			set
			{
				this._Plus4PortionOfZipCode = value;
			}
		}
		#endregion
		#region Plus4PortionOfZipCode2
		public abstract class plus4PortionOfZipCode2 : PX.Data.BQL.BqlInt.Field<plus4PortionOfZipCode2> { }
		protected Int32? _Plus4PortionOfZipCode2;
		[PXUIField(DisplayName = "Zip Plus Max.")]
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? Plus4PortionOfZipCode2
		{
			get
			{
				return this._Plus4PortionOfZipCode2;
			}
			set
			{
				this._Plus4PortionOfZipCode2 = value;
			}
		}
		#endregion
	}
}
