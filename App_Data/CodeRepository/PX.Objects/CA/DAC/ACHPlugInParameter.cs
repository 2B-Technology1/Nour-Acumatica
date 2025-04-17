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

using PX.ACHPlugInBase;
using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName(nameof(ACHPlugInParameter))]
	public class ACHPlugInParameter : IBqlTable, IACHPlugInParameter
	{
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true, InputMask = "", IsKey = true)]
		[PXDefault(typeof(PaymentMethod.paymentMethodID))]
		[PXUIField(DisplayName = " Payment Method ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXParent(typeof(Select<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<paymentMethodID>>>>))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region PlugInTypeName
		public abstract class plugInTypeName : PX.Data.BQL.BqlGuid.Field<plugInTypeName> { }
		[PXDBString(255, IsKey = true)]
		[PXUIField(DisplayName = "Export Plug-In (Type)", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(PaymentMethod.aPBatchExportPlugInTypeName))]
		[PXUIVisible(typeof(Where<PaymentMethod.useForAP, Equal<True>,
			And<PaymentMethod.aPCreateBatchPayment, Equal<True>,
			And<PaymentMethod.aPBatchExportMethod, Equal<ACHExportMethod.plugIn>>>>))]
		[PXUIRequired(typeof(Where<PaymentMethod.useForAP, Equal<True>,
			And<PaymentMethod.aPCreateBatchPayment, Equal<True>>>))]
		public virtual string PlugInTypeName
		{
			get;
			set;
		}
		#endregion
		#region ParameterID
		public abstract class parameterID : PX.Data.BQL.BqlString.Field<parameterID> { }

		[PXDBString(30, IsKey = true)]
		[PXUIField(DisplayName = "Parameter ID", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		public virtual string ParameterID { get; set; }
		#endregion
		#region ParameterCode
		public abstract class parameterCode : PX.Data.BQL.BqlString.Field<parameterCode> { }

		[PXDBString(60)]
		[PXUIField(DisplayName = "Setting", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false)]
		public virtual string ParameterCode { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlGuid.Field<description> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual string Description { get; set; }
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }

		[PXUIField(DisplayName = "Value", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
		[PlugInSettingsList(4000, typeof(CR.CRTaskMaint), typeof(PX.SM.TaskTemplateSetting.fieldName), ExclusiveValues = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Value { get; set; }
		#endregion
		#region Required
		public abstract class required : PX.Data.BQL.BqlBool.Field<required> { }

		[PXDBBool]
		public bool? Required { get; set; }
		#endregion
		#region Visible
		public abstract class visible : PX.Data.BQL.BqlBool.Field<visible> { }

		[PXDBBool]
		public virtual bool? Visible { get; set; }
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }

		[PXDBInt]
		public virtual int? Type { get; set; }
		#endregion
		#region Order
		public abstract class order : PX.Data.BQL.BqlInt.Field<order> { }

		[PXDBInt]
		public int? Order { get; set; }
		#endregion
		#region UsedIn
		public abstract class usedIn : PX.Data.BQL.BqlInt.Field<usedIn> { }

		[PXDBInt]
		public int? UsedIn { get; set; }
		#endregion
		#region DetailMapping
		public abstract class detailMapping : PX.Data.BQL.BqlInt.Field<detailMapping> { }

		[PXInt]
		public int? DetailMapping { get; set; }
		#endregion
		#region ExportScenarioMapping
		public abstract class exportScenarioMapping : PX.Data.BQL.BqlInt.Field<exportScenarioMapping> { }

		[PXInt]
		public int? ExportScenarioMapping { get; set; }
		#endregion
		#region IsGroupHeader
		public abstract class isGroupHeader : PX.Data.BQL.BqlInt.Field<isGroupHeader> { }

		[PXDBBool]
		public virtual bool? IsGroupHeader { get; set; }
		#endregion
		#region IsAvailableInShortForm
		public abstract class isAvailableInShortForm : PX.Data.BQL.BqlInt.Field<isAvailableInShortForm> { }

		[PXDBBool]
		public virtual bool? IsAvailableInShortForm { get; set; }
		#endregion
		#region IsFormula
		public abstract class isFormula : PX.Data.BQL.BqlBool.Field<isFormula> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Is Formula", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		public virtual bool? IsFormula { get; set; }
		#endregion
	}

	[Serializable]
	[PXCacheName(nameof(ACHPlugInParameter))]
	public class ACHPlugInParameter2 : ACHPlugInParameter
	{
		#region PaymentMethodID
		public abstract new class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true, InputMask = "", IsKey = true)]
		[PXDefault(typeof(PaymentMethod.paymentMethodID))]
		[PXParent(typeof(Select<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<paymentMethodID>>>>))]
		public override string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region PlugInTypeName
		public abstract new class plugInTypeName : PX.Data.BQL.BqlGuid.Field<plugInTypeName> { }
		[PXDBString(255, IsKey = true)]
		[PXDefault(typeof(PaymentMethod.aPBatchExportPlugInTypeName))]
		public override string PlugInTypeName
		{
			get;
			set;
		}
		#endregion
		#region ParameterID
		public abstract new class parameterID : PX.Data.BQL.BqlString.Field<parameterID> { }

		[PXDBString(30, IsKey = true)]
		public override string ParameterID { get; set; }
		#endregion
		#region ParameterCode
		public abstract new class parameterCode : PX.Data.BQL.BqlString.Field<parameterCode> { }

		[PXDBString(60)]
		public override string ParameterCode { get; set; }
		#endregion
		#region Value
		public abstract new class value : PX.Data.BQL.BqlString.Field<value> { }

		[PXDBString(4000)]
		public override string Value { get; set; }
		#endregion
	}
}
