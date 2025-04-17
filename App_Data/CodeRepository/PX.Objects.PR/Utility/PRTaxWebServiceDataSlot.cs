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
using PX.Payroll.Data;
using PX.Payroll.Data.Vertex;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRTaxWebServiceDataSlot : IPrefetchable<string>
	{
		public enum DataType
		{
			WageTypes,
			DeductionTypes,
			ReportingTypes,
			QuebecReportingTypes
		}

		public class DeductionTypeKey : IEquatable<DeductionTypeKey>
		{
			public int DeductionTypeID;
			public string TaxID;

			public DeductionTypeKey(int deductionTypeID, string taxID) => (DeductionTypeID, TaxID) = (deductionTypeID, taxID);

			public bool Equals(DeductionTypeKey other)
			{
				return other.DeductionTypeID == DeductionTypeID && other.TaxID == TaxID;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = 17;
					hash = hash * 23 + DeductionTypeID.GetHashCode();
					hash = hash * 23 + (TaxID?.GetHashCode() ?? 0);
					return hash;
				}
			}
		}

		private CachedData _Data;

		public static CachedData GetData(string countryID) => 
			PXDatabase.GetSlot<PRTaxWebServiceDataSlot, string>(nameof(PRTaxWebServiceDataSlot), countryID, typeof(PRTaxWebServiceData))._Data;

		public static IEnumerable<IDynamicType> GetDynamicTypeData(string countryID, DataType dataType)
		{
			CachedData fullData = GetData(countryID);
			if (fullData == null)
			{
				return new List<IDynamicType>();
			}

			switch (dataType)
			{
				case DataType.WageTypes:
					return fullData.WageTypes.Values;
				case DataType.DeductionTypes:
					return fullData.DeductionTypes.Values;
				case DataType.ReportingTypes:
					return fullData.ReportingTypes.Values;
				case DataType.QuebecReportingTypes:
					return fullData.QuebecReportingTypes.Values;
				default:
					throw new PXException(Messages.WebServiceDataTypeNotRecognized, dataType);
			}
		}

		public void Prefetch(string countryID)
		{
			PXDataRecord rec = PXDatabase.SelectSingle<PRTaxWebServiceData>(
				new PXDataField<PRTaxWebServiceData.wageTypes>(),
				new PXDataField<PRTaxWebServiceData.deductionTypes>(),
				new PXDataField<PRTaxWebServiceData.reportingTypes>(),
				new PXDataField<PRTaxWebServiceData.quebecReportingTypes>(),
				new PXDataFieldValue<PRTaxWebServiceData.countryID>(countryID));

			if (rec == null)
			{
				_Data = null;
			}
			else
			{
				string wageTypes = rec.GetString(0);
				string deductionTypes = rec.GetString(1);
				string reportingTypes = rec.GetString(2);
				string quebecReportingTypes = rec.GetString(3);

				_Data = new CachedData()
				{
					WageTypes = JsonConvert.DeserializeObject<IEnumerable<WageType>>(wageTypes).ToDictionary(k => k.TypeID, v => v),
					DeductionTypes = JsonConvert.DeserializeObject<IEnumerable<DeductionType>>(deductionTypes).ToDictionary(k => new DeductionTypeKey(k.TypeID, k.TaxID), v => v),
					ReportingTypes = JsonConvert.DeserializeObject<IEnumerable<ReportingType>>(reportingTypes).ToDictionary(k => k.TypeID, v => v),
					QuebecReportingTypes = quebecReportingTypes != null ? JsonConvert.DeserializeObject<IEnumerable<ReportingType>>(quebecReportingTypes)
						.ToDictionary(k => k.TypeID, v => v) : new Dictionary<int, ReportingType>(),
				};
			}
		}

		public class CachedData
		{
			public Dictionary<int, WageType> WageTypes { get; set; }
			public Dictionary<DeductionTypeKey, DeductionType> DeductionTypes { get; set; }
			public Dictionary<int, ReportingType> ReportingTypes { get; set; }
			public Dictionary<int, ReportingType> QuebecReportingTypes { get; set; }
		}
	}
}
