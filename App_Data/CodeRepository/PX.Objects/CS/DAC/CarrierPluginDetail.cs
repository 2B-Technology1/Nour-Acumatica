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
using PX.CarrierService;
using PX.Data;

namespace PX.Objects.CS
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.CarrierPluginDetail)]
	public partial class CarrierPluginDetail : PX.Data.IBqlTable, ICarrierDetail
	{
		public const int Text = 1;
		public const int Combo = 2;
		public const int CheckBox = 3;
		public const int Password = 4;

		public const int ValueFieldLength = 1024;

		#region CarrierPluginID
		public abstract class carrierPluginID : PX.Data.BQL.BqlString.Field<carrierPluginID> { }
		protected String _CarrierPluginID;
		[PXParent(typeof(Select<CarrierPlugin, Where<CarrierPlugin.carrierPluginID, Equal<Current<CarrierPluginDetail.carrierPluginID>>>>))]
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(CarrierPlugin.carrierPluginID))]
		public virtual String CarrierPluginID
		{
			get
			{
				return this._CarrierPluginID;
			}
			set
			{
				this._CarrierPluginID = value;
			}
		}
		#endregion
		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID> { }
		protected String _DetailID;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "ID", Enabled = false)]
		public virtual String DetailID
		{
			get
			{
				return this._DetailID;
			}
			set
			{
				this._DetailID = value;
			}
		}
		#endregion
		#region DetailLineNbr
		[PXDBInt()]
		[PXLineNbr(typeof(CarrierPlugin.detailLineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Enabled = false, Visible = false)]
		public virtual int? DetailLineNbr { get; set; }

		public abstract class detailLineNbr : PX.Data.BQL.BqlInt.Field<detailLineNbr> { }
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
		protected String _Value;
		[PXDBString(ValueFieldLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
		public virtual String Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				this._Value = value;
			}
		}
		#endregion
		#region ControlType
		public abstract class controlType : PX.Data.BQL.BqlInt.Field<controlType> { }
		protected Int32? _ControlType;
		[PXDBInt()]
		[PXDefault(1)]
		[PXUIField(DisplayName = "Control Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXIntList(new int[] { Text, Combo, CheckBox }, new string[] { "Text", "Combo", "Checkbox" })]
		public virtual Int32? ControlType
		{
			get
			{
				return this._ControlType;
			}
			set
			{
				this._ControlType = value;
			}
		}
		#endregion
		#region ComboValues
		public abstract class comboValues : PX.Data.BQL.BqlString.Field<comboValues> { }
		protected String _ComboValues;
		[PXDBString(4000, IsUnicode = true)]
		public virtual String ComboValues
		{
			get
			{
				return this._ComboValues;
			}
			set
			{
				this._ComboValues = value;
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

		#region ICarrierDetail Members

		public IList<KeyValuePair<string, string>> GetComboValues()
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

			string[] parts = ComboValues.Split(';');
			foreach (string part in parts)
			{
				if (!string.IsNullOrEmpty(part))
				{
					string[] keyval = part.Split('|');

					if (keyval.Length == 2)
					{
						list.Add(new KeyValuePair<string, string>(keyval[0], keyval[1]));
					}
				}
			}

			return list;
		}

		public virtual void SetComboValues(IList<KeyValuePair<string, string>> list)
		{
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string, string> kv in list)
			{
				sb.AppendFormat("{0}|{1};", kv.Key, kv.Value);
			}

			ComboValues = sb.ToString();
		}

		#endregion

		#region ICarrierDetail Members

		public string CarrierID
		{
			get
			{
				return CarrierPluginID;
			}
			set
			{
				CarrierPluginID = value;
			}
		}

		#endregion
	}
}
