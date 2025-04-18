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
using PX.TaxProvider;

namespace PX.Objects.TX
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxPluginDetail)]
	public partial class TaxPluginDetail : PX.Data.IBqlTable, ITaxProviderSetting
	{
		public const int Text = 1;
		public const int Combo = 2;
		public const int CheckBox = 3;
		public const int Password = 4;

		public const int ValueFieldLength = 1024;

		#region Keys
		public class PK : PrimaryKeyOf<TaxPluginDetail>.By<taxPluginID, settingID>
		{
			public static TaxPluginDetail Find(PXGraph graph, string taxPluginID, string settingID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, taxPluginID, settingID, options);
		}
		public static class FK
		{
			public class TaxPlugin : TX.TaxPlugin.PK.ForeignKeyOf<TaxPluginDetail>.By<taxPluginID> { }
		}
		#endregion

		#region TaxPluginID
		public abstract class taxPluginID : PX.Data.BQL.BqlString.Field<taxPluginID> { }
		protected String _TaxPluginID;
		[PXParent(typeof(Select<TaxPlugin, Where<TaxPlugin.taxPluginID, Equal<Current<TaxPluginDetail.taxPluginID>>>>))]
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(TaxPlugin.taxPluginID))]
		public virtual String TaxPluginID
		{
			get
			{
				return this._TaxPluginID;
			}
			set
			{
				this._TaxPluginID = value;
			}
		}
		#endregion
		#region SettingID
		public abstract class settingID : PX.Data.BQL.BqlString.Field<settingID> { }
		protected String _SettingID;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "ID", Enabled = false)]
		public virtual String SettingID
		{
			get
			{
				return this._SettingID;
			}
			set
			{
				this._SettingID = value;
			}
		}
        #endregion

		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected int? _SortOrder;

		int? ITaxProviderSetting.SortOrder
		{
			get
			{
				return this._SortOrder.HasValue ? _SortOrder.Value : 0;
			}
			set
			{
				this._SortOrder = value;
			}
		}

		[PXDBInt()]
		[PXDefault()]
		public virtual int? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Enabled = false)]
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
		#region ControlTypeValue
		public abstract class controlTypeValue : PX.Data.BQL.BqlInt.Field<controlTypeValue> { }
		protected Int32? _ControlTypeValue;
		[PXDBInt()]
		[PXDefault(1)]
		[PXUIField(DisplayName = "Control Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXIntList(new int[] { Text, Combo, CheckBox }, new string[] { "Text", "Combo", "Checkbox" })]
		public virtual Int32? ControlTypeValue
		{
			get
			{
				return this._ControlTypeValue;
			}
			set
			{
				this._ControlTypeValue = value;
			}
		}
		#endregion
		#region ComboValuesStr
		public abstract class comboValuesStr : PX.Data.BQL.BqlString.Field<comboValuesStr> { }
		protected String _ComboValuesStr;
		[PXDBString(4000, IsUnicode = true)]
		public virtual String ComboValuesStr
		{
			get
			{
				return this._ComboValuesStr;
			}
			set
			{
				this._ComboValuesStr = value;
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

		#region ITaxDetail Members
		public IDictionary<string, string> ComboValues
		{
			get
			{
				var list = new Dictionary<string, string>();

				string[] parts = _ComboValuesStr.Split(';');
				foreach (string part in parts)
				{
					if (!string.IsNullOrEmpty(part))
					{
						string[] keyval = part.Split('|');

						if (keyval.Length == 2)
						{
							list.Add(keyval[0], keyval[1]);
						}
					}
				}

				return list;
			}
			set
			{
				StringBuilder sb = new StringBuilder();

				foreach (var kv in value)
				{
					sb.AppendFormat("{0}|{1};", kv.Key, kv.Value);
				}

				_ComboValuesStr = sb.ToString();
			}
		}

		public TaxProviderSettingControlType ControlType
		{
			get
			{
				try
				{
					if (_ControlTypeValue != null)
					{
						var controlType = (TaxProviderSettingControlType) _ControlTypeValue;
						return controlType;
					}

					return TaxProviderSettingControlType.Undefined;
				}
				catch
				{
					return TaxProviderSettingControlType.Undefined;
				}
			}
			set { _ControlTypeValue = (int) value; }
		}
		#endregion
	}
}
