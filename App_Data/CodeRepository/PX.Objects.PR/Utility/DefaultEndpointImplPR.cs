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

using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Adapters;
using PX.Api.ContractBased.Models;
using PX.Api.ContractBased.UI.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.EP;
using PX.Objects.PR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PX.Objects.EndpointAdapters
{
	[PXVersion("22.200.001", "Default")]
	[PXVersion("23.200.001", "Default")]
	public class DefaultEndpointImplPR : IAdapterWithMetadata
	{
		public IEntityMetadataProvider MetadataProvider { protected get; set; }

		private const string EmployeeID = "EmployeeID";
		private const string ClassID = "ClassID";
		private const string PaymentMethod = "PaymentMethod";
		private const string CashAccountString = "CashAccount";
		private const string EmployeeTypeClassDefault = "EmployeeTypeClassDefault";
		private const string EmployeeType = "EmployeeType";
		private const string PayGroupClassDefault = "PayGroupClassDefault";
		private const string PayGroup = "PayGroup";
		private const string CalendarClassDefault = "CalendarClassDefault";
		private const string Calendar = "Calendar";
		private const string WeeksPerYearClassDefault = "WeeksPerYearClassDefault";
		private const string WorkingWeeksPerYear = "WorkingWeeksPerYear";
		private const string ExemptFromOvertimeRulesClassDefault = "ExemptFromOvertimeRulesClassDefault";
		private const string ExemptFromOvertimeRules = "ExemptFromOvertimeRules";
		private const string UnionClassDefault = "UnionClassDefault";
		private const string DefaultUnion = "DefaultUnion";
		private const string WCCCodeClassDefault = "WCCCodeClassDefault";
		private const string DefaultWCCCode = "DefaultWCCCode";
		private const string ExemptFromCertReportingClassDefault = "ExemptFromCertReportingClassDefault";
		private const string ExemptFromCertReporting = "ExemptFromCertReporting";
		private const string NetPayMinClassDefault = "NetPayMinClassDefault";
		private const string NetPayMinimum = "NetPayMinimum";
		private const string DeductionAndBenefitUseClassDefaults = "DeductionAndBenefitUseClassDefaults";
		private const string MaxPercOfNetPayForAllGarnishm = "MaxPercOfNetPayForAllGarnishm";
		private const string UseClassDefaultValueUsePayrollProjectWorkLocationUseDflt = "UseClassDefaultValueUsePayrollProjectWorkLocationUseDflt";
		private const string UsePayrollWorkLocationfromProject = "UsePayrollWorkLocationfromProject";

		private const string SettingValue = "Value";

		[FieldsProcessed(new[] {
			EmployeeID,
			ClassID,
			PaymentMethod,
			CashAccountString
		})]
		protected virtual void EmployeePayrollSettings_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EntityValueField employeeIDField = targetEntity.Fields.SingleOrDefault(f => f.Name == EmployeeID) as EntityValueField;
			EntityValueField classIDField = targetEntity.Fields.SingleOrDefault(f => f.Name == ClassID) as EntityValueField;
			EntityValueField paymentMethodField = targetEntity.Fields.SingleOrDefault(f => f.Name == PaymentMethod) as EntityValueField;
			EntityValueField cashAccountField = targetEntity.Fields.SingleOrDefault(f => f.Name == CashAccountString) as EntityValueField;

			PREmployeePayrollSettingsMaint employeePayrollSettingsMaint = graph as PREmployeePayrollSettingsMaint;

			var epGraph = PXGraph.CreateInstance<EPEmployeeSelectGraph>();

			EPEmployee employee = EPEmployee.UK.Find(epGraph, employeeIDField.Value);
			if (employee != null)
			{
				employeePayrollSettingsMaint.Caches[typeof(EPEmployee)] = epGraph.Caches[typeof(EPEmployee)];
				PREmployee prEmployee = employeePayrollSettingsMaint.PayrollEmployee.Extend(employee);
				prEmployee.EmployeeClassID = classIDField.Value;
				prEmployee.PaymentMethodID = paymentMethodField.Value;
				prEmployee.CashAccountID = ((CashAccount)SelectFrom<CashAccount>
					.Where<CashAccount.cashAccountCD.IsEqual<P.AsString>>
					.View.SelectSingleBound(epGraph, null, cashAccountField.Value)).CashAccountID;
				employeePayrollSettingsMaint.PayrollEmployee.Update(prEmployee);
				employeePayrollSettingsMaint.Actions.PressSave();
			}
		}

		[FieldsProcessed(new[] {
			EmployeeTypeClassDefault,
			EmployeeType
		})]
		protected virtual void EmployeePayrollSettings_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			PREmployeePayrollSettingsMaint employeePayrollSettingsMaint = graph as PREmployeePayrollSettingsMaint;
			PREmployee employee = employeePayrollSettingsMaint.CurrentPayrollEmployee.SelectSingle();

			if (employee != null)
			{
				IList<EntityMappingProjection> mappedFields = MetadataProvider.GetMappedFields();
				IEnumerable<UseDefaultValueAttribute> useDefaultAttributeList =
					typeof(PREmployee).GetMembers().SelectMany(m => Attribute.GetCustomAttributes(m, typeof(UseDefaultValueAttribute), false).Cast<UseDefaultValueAttribute>());
				foreach (UseDefaultValueAttribute attr in useDefaultAttributeList)
				{
					string useDefaultFieldName = attr.UseDefaultField.Name;
					string defaultFieldName = attr.DefaultField.Name;

					string useDefaultFieldEndpointName = mappedFields.Where(e => String.Equals(e.MappedField, useDefaultFieldName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.FieldName;
					string defaultFieldEndpointName = mappedFields.Where(e => String.Equals(e.MappedField, defaultFieldName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.FieldName;

					EntityValueField useDefaultFieldEndpoint = null;
					EntityValueField defaultFieldEndpoint = null;

					if (useDefaultFieldEndpointName != null)
					{
						useDefaultFieldEndpoint = GetUseDefaultFieldEndpoints(targetEntity, useDefaultFieldEndpointName);
					}
					if (defaultFieldEndpointName != null)
					{
						defaultFieldEndpoint = GetUseDefaultFieldEndpoints(targetEntity, defaultFieldEndpointName);
					}

					if (useDefaultFieldEndpoint != null && Boolean.TryParse(useDefaultFieldEndpoint.Value, out bool useDefault))
					{
						employeePayrollSettingsMaint.CurrentPayrollEmployee.Cache.SetValueExt(employee, useDefaultFieldName, useDefault);
					}
					if (defaultFieldEndpoint != null && !(bool)employeePayrollSettingsMaint.CurrentPayrollEmployee.Cache.GetValue(employee, useDefaultFieldName))
					{
						employeePayrollSettingsMaint.CurrentPayrollEmployee.Cache.SetValueExt(employee, defaultFieldName, defaultFieldEndpoint.Value);
					}
				}
				employeePayrollSettingsMaint.PayrollEmployee.Update(employee);
				employeePayrollSettingsMaint.Actions.PressSave();
			}
		}

		[FieldsProcessed(new[] {
			PayGroupClassDefault,
			PayGroup,
			CalendarClassDefault,
			Calendar,
			WeeksPerYearClassDefault,
			WorkingWeeksPerYear,
			ExemptFromOvertimeRulesClassDefault,
			ExemptFromOvertimeRules,
			UnionClassDefault,
			DefaultUnion,
			WCCCodeClassDefault,
			DefaultWCCCode,
			ExemptFromCertReportingClassDefault,
			ExemptFromCertReporting,
			NetPayMinClassDefault,
			NetPayMinimum
		})]
		protected virtual void EmployeeGeneralInfo_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{

		}

		[FieldsProcessed(new[] {
			UseClassDefaultValueUsePayrollProjectWorkLocationUseDflt,
			UsePayrollWorkLocationfromProject
		})]
		protected virtual void EmployeeWorkLocations_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{

		}

		[FieldsProcessed(new[] {
			DeductionAndBenefitUseClassDefaults,
			MaxPercOfNetPayForAllGarnishm
		})]
		protected virtual void DeductionsAndBenefits_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{

		}

		[FieldsProcessed(new[] {
			SettingValue
		})]
		protected virtual void TaxSettingDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			InsertUpdateTaxSettingDetail(graph, entity, targetEntity);
		}

		[FieldsProcessed(new[] {
			SettingValue
		})]
		protected virtual void TaxSettingDetail_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			InsertUpdateTaxSettingDetail(graph, entity, targetEntity);
		}

		protected virtual void InsertUpdateTaxSettingDetail(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			EntityValueField stringSettingValueField = targetEntity.Fields.SingleOrDefault(f => f.Name == SettingValue) as EntityValueField;

			PREmployeePayrollSettingsMaint employeePayrollSettingsMaint = graph as PREmployeePayrollSettingsMaint;

			PREmployeeAttribute employeeAttribute =
				employeePayrollSettingsMaint.EmployeeAttributes.Select().FirstTableItems.FirstOrDefault(x => x.NoteID == targetEntity.ID);

			employeeAttribute.Value = stringSettingValueField.Value;
			employeePayrollSettingsMaint.EmployeeAttributes.Update(employeeAttribute);
			employeePayrollSettingsMaint.Actions.PressSave();
		}

		protected virtual EntityValueField GetUseDefaultFieldEndpoints(EntityImpl entity, string endpointName)
		{
			EntityValueField fieldEndpoint = entity.Fields.FirstOrDefault(f => f.Name == endpointName) as EntityValueField;
			if (fieldEndpoint == null)
			{
				EntityObjectField parentEndpoint = entity.Fields
					.OfType<EntityObjectField>()
					.Where(e => ((EntityImpl)e.Value).Fields
					.Any(f => f.Name == endpointName))
					.FirstOrDefault();
				if (parentEndpoint != null)
				{
					fieldEndpoint = ((EntityImpl)parentEndpoint.Value).Fields.FirstOrDefault(f => f.Name == endpointName) as EntityValueField;
				}
			}
			return fieldEndpoint;
		}
	}
}
