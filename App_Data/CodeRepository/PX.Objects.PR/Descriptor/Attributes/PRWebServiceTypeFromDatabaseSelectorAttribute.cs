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

using Newtonsoft.Json;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Payroll.Data;
using PX.Payroll.Data.Vertex;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRWebServiceTypeFromDatabaseSelectorAttribute : PXCustomSelectorAttribute, IPXFieldDefaultingSubscriber
	{
		private PRTaxWebServiceDataSlot.DataType _WebServiceType;
		private string _CountryID;
		private bool _UseDefault;
		private bool _IsDeduction;

		public PRWebServiceTypeFromDatabaseSelectorAttribute(PRTaxWebServiceDataSlot.DataType webServiceType, string countryID, bool useDefault)
			: base(typeof(PRDynType.id))
		{
			_WebServiceType = webServiceType;
			_CountryID = countryID;
			_UseDefault = useDefault;
			SubstituteKey = typeof(PRDynType.name);

			if (PRTaxWebServiceDataSlot.GetDynamicTypeData(_CountryID, _WebServiceType).Any(x => !string.IsNullOrEmpty(x.Description)))
			{
				DescriptionField = typeof(PRDynType.description);
				_FieldList = new[] { nameof(PRDynType.Name), nameof(PRDynType.Description) };
			}
			else
			{
				DescriptionField = null;
				_FieldList = new[] { nameof(PRDynType.Name) };
			}
		}

		public PRWebServiceTypeFromDatabaseSelectorAttribute(PRTaxWebServiceDataSlot.DataType webServiceType, string countryID, bool useDefault, string reportingType)
			: this(webServiceType, countryID, useDefault)
		{
			_IsDeduction = reportingType == ContributionType.EmployeeDeduction;
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!_UseDefault)
			{
				return;
			}

			IEnumerable<IDynamicType> cachedData = PRTaxWebServiceDataSlot.GetDynamicTypeData(_CountryID, _WebServiceType);
			if (!cachedData.Any())
			{
				return;
			}

			e.NewValue = cachedData.Min(x => x.TypeID);
		}

		protected virtual IEnumerable GetRecords()
		{
			IEnumerable<IDynamicType> cachedData = PRTaxWebServiceDataSlot.GetDynamicTypeData(_CountryID, _WebServiceType);
			if (!cachedData.Any())
			{
				return new object[] { };
			}

			return cachedData.Where(x => _WebServiceType != PRTaxWebServiceDataSlot.DataType.ReportingTypes
				|| ((ReportingType)x).IsDeduction == _IsDeduction
				|| ((ReportingType)x).TypeName == ReportingType.PensionAdjustment).GroupBy(x => x.TypeID).Select(x => new PRDynType()
			{
				ID = x.Key,
				Name = x.First().TypeName,
				Description = x.First().Description
			});
		}

		public override void SubstituteKeyCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e) { }
	}
}
