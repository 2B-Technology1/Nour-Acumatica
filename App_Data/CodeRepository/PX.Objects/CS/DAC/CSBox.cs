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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CS
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.CSBox, PXDacType.Catalogue)]
	public partial class CSBox : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CSBox>.By<boxID>
		{
			public static CSBox Find(PXGraph graph, string boxID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, boxID, options);
		}
		#endregion
		#region BoxID
		public abstract class boxID : PX.Data.BQL.BqlString.Field<boxID> { }
		protected String _BoxID;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Box ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String BoxID
		{
			get
			{
				return this._BoxID;
			}
			set
			{
				this._BoxID = value;
			}
		}
		#endregion
		#region MaxWeight
		public abstract class maxWeight : PX.Data.BQL.BqlDecimal.Field<maxWeight> { }
		protected Decimal? _MaxWeight;

		[PXDBDecimal(4, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Max. Weight", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? MaxWeight
		{
			get
			{
				return this._MaxWeight;
			}
			set
			{
				this._MaxWeight = value;
			}
		}
		#endregion
        #region BoxWeight
        public abstract class boxWeight : PX.Data.BQL.BqlDecimal.Field<boxWeight> { }
        protected Decimal? _BoxWeight;

        [PXDBDecimal(4, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Box Weight", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? BoxWeight
        {
            get
            {
                return this._BoxWeight;
            }
            set
            {
                this._BoxWeight = value;
            }
        }
        #endregion
        #region MaxVolume
        public abstract class maxVolume : PX.Data.BQL.BqlDecimal.Field<maxVolume> { }
        protected Decimal? _MaxVolume;

        [PXDBDecimal(4, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Max Volume", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? MaxVolume
        {
            get
            {
                return this._MaxVolume;
            }
            set
            {
                this._MaxVolume = value;
            }
        }
        #endregion
		#region Length
		public abstract class length : PX.Data.BQL.BqlDecimal.Field<length> { }
		protected decimal? _Length;
		[PXDBDecimal(2, MinValue = 0)]
		[PXUIField(DisplayName = "Length", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? Length
		{
			get
			{
				return this._Length;
			}
			set
			{
				this._Length = value;
			}
		}
		#endregion
		#region Width
		public abstract class width : PX.Data.BQL.BqlDecimal.Field<width> { }
		protected decimal? _Width;
		[PXDBDecimal(2, MinValue =0)]
		[PXUIField(DisplayName = "Width", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? Width
		{
			get
			{
				return this._Width;
			}
			set
			{
				this._Width = value;
			}
		}
		#endregion
		#region Height
		public abstract class height : PX.Data.BQL.BqlDecimal.Field<height> { }
		protected decimal? _Height;
		[PXDBDecimal(2, MinValue = 0)]
		[PXUIField(DisplayName = "Height", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? Height
		{
			get
			{
				return this._Height;
			}
			set
			{
				this._Height = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region CarrierBox
		public abstract class carrierBox : PX.Data.BQL.BqlString.Field<carrierBox> { }
		protected String _CarrierBox;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Carrier's Package", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CarrierBox
		{
			get
			{
				return this._CarrierBox;
			}
			set
			{
				this._CarrierBox = value;
			}
		}
		#endregion
		#region AllowOverrideDimension
		public abstract class allowOverrideDimension : Data.BQL.BqlBool.Field<allowOverrideDimension> { }

		/// <summary>
		/// A Boolean value that specifies whether the <see cref="Width"/>, <see cref="Height"/>, and <see cref="Length"/>
		/// dimension values of a box can be overridden when the box is selected in a package.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Editable Dimensions")]
		public virtual bool? AllowOverrideDimension { get; set; }
		#endregion
		#region ActiveByDefault
		public abstract class activeByDefault : PX.Data.BQL.BqlBool.Field<activeByDefault> { }
		protected Boolean? _ActiveByDefault;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active by Default")]
		public virtual Boolean? ActiveByDefault
		{
			get
			{
				return this._ActiveByDefault;
			}
			set
			{
				this._ActiveByDefault = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
        #endregion

        
	}
}
