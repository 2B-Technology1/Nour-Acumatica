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
using PX.Data;

namespace PX.Objects.CR
{
	public class CRTargetEntityType
	{
		public const string BAccount = "PX.Objects.CR.BAccount";
		public const string Customer = "PX.Objects.AR.Customer";
		public const string Employee = "PX.Objects.CR.CREmployee";
		public const string Vendor = "PX.Objects.AP.Vendor";
		public const string Lead = "PX.Objects.CR.CRLead";
		public const string Contact = "PX.Objects.CR.Contact";
		public const string CROpportunity = "PX.Objects.CR.CROpportunity";
		public const string CRCase = "PX.Objects.CR.CRCase";
		public const string SOOrder = "PX.Objects.SO.SOOrder";
		public const string POOrder = "PX.Objects.PO.POOrder";
		public const string APInvoice = "PX.Objects.AP.APInvoice";
		public const string ARInvoice = "PX.Objects.AR.ARInvoice";
		public const string EPExpenseClaimDetails = "PX.Objects.EP.EPExpenseClaimDetails";
		public const string CRCampaign = "PX.Objects.CR.CRCampaign";
		public const string CRQuote = "PX.Objects.CR.CRQuote";
	}

	public class CRRelationTypeListAttribure : PXStringListAttribute, IPXFieldDefaultingSubscriber
	{
		public enum TypeEntityList
		{
			None = 0,
			Contact = 1,
			BAccount = 2,
			All = 3
		}

		public class Item
		{
			public string[] FieldNames;
			public string[] NeutralFieldDisplayNames;
			public string[] FieldDisplayNames;

			public Item(string[] fieldNames, string[] fieldDisplayNames)
			{
				FieldNames = fieldNames;
				FieldDisplayNames = fieldDisplayNames;

				NeutralFieldDisplayNames = new string[fieldDisplayNames.Length];
				Array.Copy(fieldDisplayNames, NeutralFieldDisplayNames, fieldDisplayNames.Length);
			}
		}

		private readonly Type RoleType;
		private readonly Dictionary<TypeEntityList, Item> TypeArray;

		public CRRelationTypeListAttribure(Type roleType, string[] overrideAllowedValues = null, string[] overrideAllowedLabels = null) : base(new string[] { }, new string[] { })
		{
			RoleType = roleType;

			TypeArray = new Dictionary<TypeEntityList, Item>
			{
				{ TypeEntityList.Contact, new Item(new [] { CRTargetEntityType.Contact }, new [] { Messages.Contact }) },
				{ TypeEntityList.BAccount, new Item(new [] { CRTargetEntityType.BAccount }, new [] { Messages.BAccount }) },
				{
					TypeEntityList.All, new Item(
						overrideAllowedValues ?? new []
						{
							CRTargetEntityType.APInvoice,
							CRTargetEntityType.ARInvoice,
							CRTargetEntityType.BAccount,
							CRTargetEntityType.CRCampaign,
							CRTargetEntityType.CRCase,
							CRTargetEntityType.Contact,
							CRTargetEntityType.Customer,
							CRTargetEntityType.Employee,
							CRTargetEntityType.EPExpenseClaimDetails,
							CRTargetEntityType.Lead,
							CRTargetEntityType.CROpportunity,
							CRTargetEntityType.POOrder,
							CRTargetEntityType.SOOrder,
							CRTargetEntityType.CRQuote,
							CRTargetEntityType.Vendor,
						},
						overrideAllowedLabels ?? new []
						{
							Messages.APInvoice,
							Messages.ARInvoice,
							Messages.BusinessAccount,
							Messages.Campaign,
							Messages.Case,
							Messages.Contact,
							Messages.CRCustomer,
							Messages.Employee,
							Messages.ExpReceipt,
							Messages.Lead,
							Messages.Opportunity,
							Messages.POrder,
							Messages.SOrder,
							Messages.CRQuote,
							Messages.Vendor,
						})
				}
			};

			_AllowedValues = TypeArray[TypeEntityList.All].FieldNames.ToArray();
			_AllowedLabels = TypeArray[TypeEntityList.All].FieldDisplayNames.ToArray();
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
		}

		public static int GetTargetID<Field>(PXCache cache, object data, string role)
			where Field : IBqlField
		{
			foreach (var attr in cache.GetAttributesOfType<CRRelationTypeListAttribure>(data, typeof(Field).Name))
			{
				if (attr is CRRelationTypeListAttribure attribure)
				{
					return attribure.GetTargetID(role);
				}
			}

			return -1;
		}

		public virtual int GetTargetID(string role)
		{
			TypeEntityList res = TypeEntityList.All;
			if (role == null)
				return (int)res;

			switch (role)
			{
				case CRRoleTypeList.BusinessUser:
				case CRRoleTypeList.DecisionMaker:
				case CRRoleTypeList.Evaluator:
				case CRRoleTypeList.Referrer:
				case CRRoleTypeList.Supervisor:
				case CRRoleTypeList.TechnicalExpert:
				case CRRoleTypeList.SupportEngineer:
					res = TypeEntityList.Contact;
					break;

				case CRRoleTypeList.Licensee:
					res = TypeEntityList.BAccount;
					break;

				default:
					res = TypeEntityList.All;
					break;
			}

			return (int)res;
		}

		protected virtual Item GetTargetType(string role)
		{
			return TypeArray[(TypeEntityList)GetTargetID(role)];
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			var role = sender.GetValue(e.Row, RoleType.Name) as string;
			Item item = GetTargetType(role);

			e.ReturnState = (PXStringState)PXStringState.CreateInstance(e.ReturnState, null,
				isUnicode: null,
				fieldName: _FieldName,
				isKey: null,
				required: -1,
				inputMask: null,
				allowedValues: item.FieldNames,
				allowedLabels: Messages.GetLocal(item.FieldDisplayNames),
				exclusiveValues: true,
				defaultValue: null,
				neutralLabels: item.NeutralFieldDisplayNames);

			if (e.Row == null)
				return;

			((PXFieldState)e.ReturnState).Enabled = ((PXFieldState)e.ReturnState).Enabled && (role != null);
		}


		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Item item = GetTargetType(sender.GetValue(e.Row, RoleType.Name) as string);

			if (item.FieldNames.Length == 1)
			{
				e.NewValue = item.FieldNames[0];
			}
		}

	}
}
