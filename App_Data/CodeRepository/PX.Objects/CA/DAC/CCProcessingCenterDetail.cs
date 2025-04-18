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
using System.Text;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CA
{
	public static class ControlTypeDefintion
	{
		public const int Text = 1;
		public const int Combo = 2;
		public const int CheckBox = 3;
		public const int Password = 4;

		public class List : PXIntListAttribute
		{
			public List() : base(new int[] { Text, Combo, CheckBox }, new string[] { "Text", "Combo", "Checkbox" })
			{
			}
		}
	}

	[Serializable]
	[PXCacheName(Messages.CCProcessingCenterDetail)]
	public partial class CCProcessingCenterDetail : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CCProcessingCenterDetail>.By<processingCenterID, detailID>
		{
			public static CCProcessingCenterDetail Find(PXGraph graph, string processingCenterID, string detailID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, processingCenterID, detailID, options);
		}

		public static class FK
		{
			public class ProcessingCenter : CCProcessingCenter.PK.ForeignKeyOf<CCProcessingCenterDetail>.By<processingCenterID> { }
		}
		#endregion

		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(CCProcessingCenter.processingCenterID))]
		[PXParent(typeof(Select<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Current<CCProcessingCenterDetail.processingCenterID>>>>))]
		public virtual string ProcessingCenterID
		{
			get;
			set;
		}
		#endregion
		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "ID")]
		public virtual string DetailID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(255, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Description")]
		public virtual string Descr
		{
			get;
			set;
		}
		#endregion
		#region IsEncryptionRequired
		public abstract class isEncryptionRequired : PX.Data.BQL.BqlBool.Field<isEncryptionRequired> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsEncryptionRequired
		{
			get;
			set;
		}
		#endregion
		#region IsEncrypted
		public abstract class isEncrypted : PX.Data.BQL.BqlBool.Field<isEncrypted> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsEncrypted
		{
			get;
			set;
		}
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }

		[PXRSACryptStringWithConditional(1024, typeof(CCProcessingCenterDetail.isEncryptionRequired), typeof(CCProcessingCenterDetail.isEncrypted))]
		[PXDBDefault]
		[PXUIField(DisplayName = "Value")]
		public virtual string Value
		{
			get;
			set;
		}
		#endregion
		#region ControlType
		public abstract class controlType : PX.Data.BQL.BqlInt.Field<controlType> { }

		[PXDBInt]
		[PXDefault(ControlTypeDefintion.Text)]
		[PXUIField(DisplayName = "Control Type", Visibility = PXUIVisibility.SelectorVisible)]
		[ControlTypeDefintion.List]
		public virtual int? ControlType
		{
			get;
			set;
		}
		#endregion
		#region ComboValues
		public abstract class comboValues : PX.Data.BQL.BqlString.Field<comboValues> { }

		[PXDBString(4000, IsUnicode = true)]
		public virtual string ComboValues
		{
			get;
			set;
		}
		#endregion


		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		#region IPCDetail Members

		public const int ValueFieldLength = 1024;


		public virtual ICollection<KeyValuePair<string, string>> ComboValuesCollection
		{
			get { return GetComboValues(this);  }
			set { SetComboValues(this, value); }
		}

		private static IList<KeyValuePair<string, string>> GetComboValues(CCProcessingCenterDetail detail)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			if (detail.ComboValues != null)
			{
				string[] parts = detail.ComboValues.Split(';');
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
			}
			return list;
		}

		private static void SetComboValues(CCProcessingCenterDetail detail, ICollection<KeyValuePair<string, string>> collection)
		{
			if (collection != null)
			{
				StringBuilder sb = new StringBuilder();
				foreach (KeyValuePair<string, string> kv in collection)
				{
					sb.AppendFormat("{0}|{1};", kv.Key, kv.Value);
				}

				detail.ComboValues = sb.ToString();
			}
			else
			{
				detail.ComboValues = null;
			}
		}

		#endregion
		public static void Copy(PluginSettingDetail src, CCProcessingCenterDetail dst)
		{
			dst.DetailID = src.DetailID;
			dst.Value = src.Value;
			dst.Descr = src.Descr;
			dst.ControlType = src.ControlType;
			dst.IsEncryptionRequired = src.IsEncryptionRequired;
			dst.ComboValuesCollection = src.ComboValuesCollection;
		}
	}

}
