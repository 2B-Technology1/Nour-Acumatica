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

using System.Collections.Generic;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public class PluginSettingDetail
	{
		/// <summary>The ID of the key required by the plug-in developed for the processing center.
		public string DetailID { get; set; }
		/// <summary>A description of the key.
		public string Descr { get; set; }
		/// <summary>The value selected by the user for the key.</summary>
		public string Value { get; set; }
		/// <summary>The type of the control.</summary>
		public int? ControlType { get; set; }
		/// <summary>Indicates (if set to <tt>true</tt>) that encryption of the key value is required.</summary>
		public bool? IsEncryptionRequired { get; set; }
		/// <summary>Sets/Retrieves the values in the combo box if the <see cref="ControlType" /> is <see cref="SettingsControlType.Combo" />.</summary>
		public ICollection<KeyValuePair<string,string>> ComboValuesCollection { get; set; }

		public PluginSettingDetail(string detailID, string descr, string value) : this(detailID, descr, value, 1) { }
		public PluginSettingDetail(string detailID, string descr, string value, int? controlType)
		{
			this.DetailID = detailID;
			this.Descr = descr;
			this.Value = value;
			this.ControlType = controlType;
			ComboValuesCollection = new Dictionary<string, string>();
		}

		public PluginSettingDetail()
		{
		}
	}
}
