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

using PX.Common;
using PX.Common.Extensions;
using PX.Common.Mail;
using PX.Data;
using PX.Objects.CR.Extensions.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Compilation;
using PX.Objects.CR.MassProcess;

namespace PX.Objects.CR.Extensions.CRDuplicateEntities
{
	[PXInternalUseOnly]
	public class CRGramProcessor
	{
		private readonly PXGraph.GetDefaultDelegate _defaultDelegate;
		private long processedItems;
		private DateTime? track;

		protected readonly PXGraph graph;
		protected CRSetup Setup => _defaultDelegate() as CRSetup;

		public CRGramProcessor()
			: this(PXGraph.CreateInstance<PXGraph>())
		{
		}
		public CRGramProcessor(PXGraph graph)
		{
			this.graph = graph;
			processedItems = 0;
			track = null;
			if(!graph.Defaults.TryGetValue(typeof(CRSetup), out _defaultDelegate))
			{
				// adds default delegate to graph
				new IN.PXSetupOptional<CRSetup>(graph);
				_defaultDelegate = graph.Defaults[typeof(CRSetup)];
			}
		}

		public Contact GetContact(PXResult<Contact> entities) => entities?.GetItem<Contact>();

		public void CreateGrams(PXResult<Contact> entities)
		{
			PersistGrams(entities, true);
		}

		public IEnumerable<(bool IsBlocked, string BlockType)> CheckIsBlocked(PXResult<Contact> entities, IEnumerable<CRDuplicateResult> duplicates)
		{
			var currentGramsResult = DoCreateGrams(entities).Where(result => result.Gram?.CreateOnEntry != CreateOnEntryAttribute.Allow).ToList();
			if (currentGramsResult.Count == 0)
				return null;

			var duplicateRecords = duplicates.ToList();

			if (duplicateRecords.Count == 0)
				return null;

			return duplicateRecords
				.Select(duplicate =>
				{
					var duplicateRecord = duplicate.GetItem<CRDuplicateRecord>();

					var isBlocked = false;
					var blockType = CreateOnEntryAttribute.Warn;

					foreach (var (blockingGram, rule) in currentGramsResult.Where(_ => _.Gram?.ValidationType == duplicateRecord?.ValidationType))
					{
						if (blockingGram?.EntityName == null)
							continue;

						var entityType = PXBuildManager.GetType(typeName: blockingGram.EntityName, throwOnError: false);

						if (entityType == null)
							continue;

						object duplicateEntity = null;

						foreach (var table in duplicate.Tables)
						{
							if (entityType.IsAssignableFrom(table))
							{
								duplicateEntity = duplicate[table];

								break;
							}
						}

						if (duplicateEntity == null)
							continue;

						PXCache cache = graph.Caches[entityType];

						var originalValue = cache.GetValue(duplicateEntity, blockingGram.FieldName);

						if (String.IsNullOrWhiteSpace(originalValue?.ToString()))
							continue;

						var deduplicationAttribute = cache.GetFieldAttribute<PXDeduplicationSearchFieldAttribute>(blockingGram.FieldName);

						var values = deduplicationAttribute?.ConvertValue(originalValue, rule?.TransformationRule);

						if (values == null)
							continue;

						foreach (var value in values)
						{
							if (string.Equals(value.ToString(), blockingGram.FieldValue, StringComparison.InvariantCultureIgnoreCase))
							{
								isBlocked = true;

								blockType = blockType == CreateOnEntryAttribute.Warn
									? blockingGram.CreateOnEntry    // try to increase from Warn to Deny
									: blockType;                    // leave Deny
							}
						}
					}

					return (IsBlocked: isBlocked, BlockType: blockType);
				});
		}

		public (bool IsGramsCreated, string NewDuplicateStatus, DateTime? GramValidationDate) PersistGrams(PXResult<Contact> entities, bool requireRecreate = false)
		{
			Contact contact = GetContact(entities);

			if (contact == null)
				return (false, null, null);

			try
			{
				if (track == null)
					track = DateTime.Now;

				if (graph.Caches[contact.GetType()].GetStatus(contact) == PXEntryStatus.Deleted)
				{
					PXDatabase.Delete<CRGrams>(new PXDataFieldRestrict(nameof(CRGrams.EntityID), PXDbType.Int, 4, contact.ContactID, PXComp.EQ));

					return (false, contact.DuplicateStatus, contact.GrammValidationDateTime);
				}

				if (!requireRecreate && GramSourceUpdated(entities))
					return (false, contact.DuplicateStatus, contact.GrammValidationDateTime);

				PXDatabase.Delete<CRGrams>(new PXDataFieldRestrict(nameof(CRGrams.EntityID), PXDbType.Int, 4, contact.ContactID, PXComp.EQ));

				foreach ((CRGrams gram, CRValidationRules _) in DoCreateGrams(entities))
				{
					if (gram == null) continue;

					PXDatabase.Insert<CRGrams>(
						new PXDataFieldAssign(nameof(CRGrams.entityID), PXDbType.Int, 4, contact.ContactID),
						new PXDataFieldAssign(nameof(CRGrams.entityName), PXDbType.NVarChar, 40, gram.EntityName),
						new PXDataFieldAssign(nameof(CRGrams.fieldName), PXDbType.NVarChar, 60, gram.FieldName),
						new PXDataFieldAssign(nameof(CRGrams.fieldValue), PXDbType.NVarChar, 60, gram.FieldValue),
						new PXDataFieldAssign(nameof(CRGrams.score), PXDbType.Decimal, 8, gram.Score),
						new PXDataFieldAssign(nameof(CRGrams.validationType), PXDbType.NVarChar, 2, gram.ValidationType)
						);
				}

				string dupStatus = DuplicateStatusAttribute.NotValidated;
				DateTime? gramValidationDate = PXTimeZoneInfo.Now;

				PXDatabase.Update<Contact>
					(
						new PXDataFieldAssign(nameof(Contact.duplicateStatus), dupStatus),
						new PXDataFieldAssign(nameof(Contact.grammValidationDateTime), PXTimeZoneInfo.ConvertTimeToUtc(gramValidationDate.Value, LocaleInfo.GetTimeZone())),
						new PXDataFieldRestrict(nameof(Contact.contactID), contact.ContactID)
					);

				processedItems += 1;

				return (true, dupStatus, gramValidationDate);
			}
			finally
			{
				if (processedItems % 100 == 0)
				{
					TimeSpan taken = DateTime.Now - (DateTime)track;
					System.Diagnostics.Debug.WriteLine("Items count:{0}, increment taken {1}", processedItems, taken);
					track = DateTime.Now;
				}
			}
		}

		public bool GramSourceUpdated(PXResult<Contact> entities)
		{
			Contact contact = GetContact(entities);

			if (contact == null)
				return false;

			if (graph.Caches[contact.GetType()].GetStatus(contact).IsIn(PXEntryStatus.Inserted, PXEntryStatus.Notchanged))
				return false;

			var isGrammSourceUpdated = true;

			foreach (var entityType in entities.Tables)
			{
				PXCache cache = graph.Caches[entityType];
				var entity = entities[entityType];

				if (Definition.ValidationRules(contact.ContactType)
					.Any(rule => !String.Equals(
						cache.GetValue(entity, rule.MatchingField)?.ToString(),
						cache.GetValueOriginal(entity, rule.MatchingField)?.ToString(),
						StringComparison.InvariantCultureIgnoreCase)))
				{
					isGrammSourceUpdated = false;
					break;
				}
			}

			return isGrammSourceUpdated;
		}

		public bool IsRulesDefined
		{
			get { return Definition.IsRulesDefined; }
		}

		public bool IsAnyBlockingRulesConfigured(string contactType)
		{
			return Definition.IsAnyBlockingRulesConfigured(contactType);
		}

		public virtual bool IsValidationOnEntryActive(string contactType)
		{
			return Definition.IsValidationOnEntryActive(contactType);
		}

		protected class ValidationDefinition : IPrefetchable
		{
			public List<CRValidationRules> Rules;
			public Dictionary<string, List<CRValidationRules>> TypeRules;

			private List<CRValidationRules> Leads;
			private List<CRValidationRules> Contacts;
			private List<CRValidationRules> Accounts;

			private List<CRValidation> Validations;

			public void Prefetch()
			{
				var graph = PXGraph.CreateInstance<PXGraph>();

				Rules = new List<CRValidationRules>();
				Leads = new List<CRValidationRules>();
				Contacts = new List<CRValidationRules>();
				Accounts = new List<CRValidationRules>();
				TypeRules = new Dictionary<string, List<CRValidationRules>>();

				// Acuminator disable once PX1072 PXGraphCreationForBqlQueries [prefetch]
				foreach (CRValidationRules rule in PXSelect<CRValidationRules>.Select(graph))
				{
					Rules.Add(rule);

					if (!TypeRules.ContainsKey(rule.ValidationType))
					{
						TypeRules[rule.ValidationType] = new List<CRValidationRules>();
					}

					TypeRules[rule.ValidationType].Add(rule);

					switch (rule.ValidationType)
					{
						case ValidationTypesAttribute.LeadToLead:
							Leads.Add(rule);
							break;

						case ValidationTypesAttribute.LeadToContact:
						case ValidationTypesAttribute.ContactToLead:
							Leads.Add(rule);
							Contacts.Add(rule);
							break;

						case ValidationTypesAttribute.ContactToContact:
							Contacts.Add(rule);
							break;

						case ValidationTypesAttribute.LeadToAccount:
							Leads.Add(rule);
							Accounts.Add(rule);
							break;
						case ValidationTypesAttribute.ContactToAccount:
							Contacts.Add(rule);
							Accounts.Add(rule);
							break;

						case ValidationTypesAttribute.AccountToAccount:
							Accounts.Add(rule);
							break;
					}
				}

				Validations = new List<CRValidation>();

				// Acuminator disable once PX1072 PXGraphCreationForBqlQueries [prefetch]
				foreach (CRValidation validation in PXSelect<CRValidation>.Select(graph))
				{
					Validations.Add(validation);
				}
			}

			public List<CRValidationRules> ValidationRules(string contactType)
			{
				switch (contactType)
				{
					case ContactTypesAttribute.Lead:
						return Leads;

					case ContactTypesAttribute.Person:
						return Contacts;

					case ContactTypesAttribute.BAccountProperty:
						return Accounts;

					default:
						return new List<CRValidationRules>();
				}
			}

			public string[] ValidationTypes(string contactType)
			{
				switch (contactType)
				{
					case ContactTypesAttribute.Lead:
						return new[] { ValidationTypesAttribute.LeadToLead, ValidationTypesAttribute.LeadToContact, ValidationTypesAttribute.LeadToAccount, ValidationTypesAttribute.ContactToLead };

					case ContactTypesAttribute.Person:
						return new[] { ValidationTypesAttribute.ContactToLead, ValidationTypesAttribute.ContactToContact, ValidationTypesAttribute.ContactToAccount, ValidationTypesAttribute.LeadToContact };

					case ContactTypesAttribute.BAccountProperty:
						return new[] { ValidationTypesAttribute.LeadToAccount, ValidationTypesAttribute.ContactToAccount, ValidationTypesAttribute.AccountToAccount };

					default:
						return new string[0];
				}
			}

			public bool IsRulesDefined
			{
				get { return Contacts.Count > 0 && Accounts.Count > 0; }
			}

			public bool IsAnyBlockingRulesConfigured(string contactType)
			{
				switch (contactType)
				{
					case ContactTypesAttribute.Lead:
						return
							TypeRules[ValidationTypesAttribute.LeadToLead].Count > 0 && TypeRules[ValidationTypesAttribute.LeadToLead].Any(_ => _.CreateOnEntry != CreateOnEntryAttribute.Allow)
							|| TypeRules[ValidationTypesAttribute.LeadToContact].Count > 0 && TypeRules[ValidationTypesAttribute.LeadToContact].Any(_ => _.CreateOnEntry != CreateOnEntryAttribute.Allow)
							|| TypeRules[ValidationTypesAttribute.LeadToAccount].Count > 0 && TypeRules[ValidationTypesAttribute.LeadToAccount].Any(_ => _.CreateOnEntry != CreateOnEntryAttribute.Allow);

					case ContactTypesAttribute.Person:
						return
							TypeRules[ValidationTypesAttribute.ContactToLead].Count > 0 && TypeRules[ValidationTypesAttribute.ContactToLead].Any(_ => _.CreateOnEntry != CreateOnEntryAttribute.Allow)
							|| TypeRules[ValidationTypesAttribute.ContactToContact].Count > 0 && TypeRules[ValidationTypesAttribute.ContactToContact].Any(_ => _.CreateOnEntry != CreateOnEntryAttribute.Allow)
							|| TypeRules[ValidationTypesAttribute.ContactToAccount].Count > 0 && TypeRules[ValidationTypesAttribute.ContactToAccount].Any(_ => _.CreateOnEntry != CreateOnEntryAttribute.Allow);

					case ContactTypesAttribute.BAccountProperty:
						return
							TypeRules[ValidationTypesAttribute.AccountToAccount].Count > 0 && TypeRules[ValidationTypesAttribute.AccountToAccount].Any(_ => _.CreateOnEntry != CreateOnEntryAttribute.Allow);

					default:
						return false;
				}
			}

			public virtual bool IsValidationOnEntryActive(string contactType)
			{
				switch (contactType)
				{
					case ContactTypesAttribute.Lead:
						return
							Validations
								.Where(_ => _.Type.IsIn(ValidationTypesAttribute.LeadToLead, ValidationTypesAttribute.LeadToContact, ValidationTypesAttribute.LeadToAccount))
								.Any(_ => _.ValidateOnEntry == true);

					case ContactTypesAttribute.Person:
						return
							Validations
								.Where(_ => _.Type.IsIn(ValidationTypesAttribute.ContactToLead, ValidationTypesAttribute.ContactToContact, ValidationTypesAttribute.ContactToAccount))
								.Any(_ => _.ValidateOnEntry == true);

					case ContactTypesAttribute.BAccountProperty:
						return
							Validations
								.Where(_ => _.Type == ValidationTypesAttribute.AccountToAccount)
								.Any(_ => _.ValidateOnEntry == true);

					default:
						return false;
				}
			}
		}

		protected ValidationDefinition Definition
		{
			get
			{
				return PXDatabase.GetSlot<ValidationDefinition>(nameof(CRDuplicateValidationSetupMaint.ValidationRules), typeof(CRValidation), typeof(CRValidationRules));
			}
		}

		protected virtual (decimal total, decimal totalZero) GetTotals(PXCache entityCache, object entity, string validationType)
		{
			decimal total = 0;
			decimal totalZero = 0;

			foreach (var rule in Definition.TypeRules[validationType])
			{
				if (entityCache.GetValue(entity, rule.MatchingField) == null)
				{
					totalZero += rule.ScoreWeight.GetValueOrDefault();
				}
				else
				{
					total += rule.ScoreWeight.GetValueOrDefault();
				}
			}

			if (Setup?.DuplicateScoresNormalization is false)
			{
				return (total, 0m);
			}

			return (total, totalZero);
		}

		protected virtual IEnumerable<(CRGrams Gram, CRValidationRules Rule)> DoCreateGrams(PXResult<Contact> entities)
		{
			Contact contact = entities?.GetItem<Contact>();

			if (contact == null)
				yield break;

			var types = GetValidationTypes(contact);

			foreach (Type entityType in entities.Tables)
			{
				var entity = entities[entityType];
				PXCache entityCache = graph.Caches[entityType];

				foreach (string validationType in types)
				{
					if (!Definition.TypeRules.ContainsKey(validationType)) continue;

					foreach (var result in CreateGramsForType(contact.ContactID, entityCache, entity, validationType))
					{
						yield return result;
					}
				}
			}
		}

		protected virtual IEnumerable<(CRGrams Gram, CRValidationRules Rule)> CreateGramsForType(int? mainEntityID, PXCache entityCache, object entity, string validationType)
		{
			var (total, totalZero) = GetTotals(entityCache, entity, validationType);

			if (total == 0) 
				yield break;

			foreach (CRValidationRules rule in Definition.TypeRules[validationType])
			{
				string entityName = rule.MatchingEntity;
				string fieldName = rule.MatchingField;
				string transformRule = rule.TransformationRule;
				Decimal scoreWeight = rule.ScoreWeight ?? 0;
				if (scoreWeight == 0) continue;

				if (scoreWeight > 0 && totalZero > 0)
					scoreWeight += totalZero * (scoreWeight / total);

				var originalValue = entityCache.GetValue(entity, fieldName);

				if (String.IsNullOrWhiteSpace(originalValue?.ToString()))
					continue;

				var deduplicationAttribute = entityCache.GetFieldAttribute<PXDeduplicationSearchFieldAttribute>(fieldName);

				var values = deduplicationAttribute?.ConvertValue(originalValue, rule.TransformationRule);

				if (values == null)
					continue;

				foreach (var value in values)
				{
					string stringValue = value.ToString().ToLower();
					
					if (transformRule.Equals(TransformationRulesAttribute.SplitWords))
					{
						foreach (var result in GetSplitWordGrams(mainEntityID, rule, entityName, stringValue, scoreWeight, fieldName))
						{
							yield return result;
						}
					}
					else if (transformRule.Equals(TransformationRulesAttribute.DomainName))
					{
						foreach (var result in GetDomainNameGrams(mainEntityID, rule, entityName, stringValue, scoreWeight, fieldName))
						{
							yield return result;
						}
					}
					else
					{
						yield return GetGrams(mainEntityID, rule, entityName, fieldName, stringValue, Decimal.Round(scoreWeight, 4));
					}
				}
			}
		}

		protected virtual string[] GetValidationTypes(object document)
		{
			if (document is Contact contact)
				return Definition.ValidationTypes(contact.ContactType);
			throw new NotSupportedException();
		}

		protected virtual (CRGrams Gram, CRValidationRules Rule) GetGrams(int? mainEntityID, CRValidationRules rule, string entityName, string fieldName, string fieldValue, decimal? score)
		{
			if (mainEntityID == null)
				throw new NotSupportedException();

			return (
				new CRGrams
				{
					EntityID = mainEntityID,
					ValidationType = rule.ValidationType,
					EntityName = entityName,
					FieldName = fieldName,
					FieldValue = fieldValue,
					Score = score,
					CreateOnEntry = rule.CreateOnEntry,
				},
				rule
			);
		}

		protected virtual IEnumerable<(CRGrams Gram, CRValidationRules Rule)> GetSplitWordGrams(int? mainEntityID, CRValidationRules rule, string entityName, string stringValue, decimal scoreWeight, string fieldName)
		{
			var charsDelimiters = Setup?.DuplicateCharsDelimiters?.ToCharArray();

			string[] words = stringValue.Split(charsDelimiters, StringSplitOptions.RemoveEmptyEntries);

			foreach (string word in words)
			{
				Decimal score = Decimal.Round(scoreWeight / words.Length, 4);

				if (score <= 0)
					continue;

				yield return GetGrams(mainEntityID, rule, entityName, fieldName, word, score);
			}
		}

		protected virtual IEnumerable<(CRGrams Gram, CRValidationRules Rule)> GetDomainNameGrams(int? mainEntityID, CRValidationRules rule, string entityName, string stringValue, decimal scoreWeight, string fieldName)
		{
			if (stringValue.Contains('@'))
			{
				stringValue = stringValue.Segment('@', 1);
			}
			else
			{
				try
				{
					stringValue = new UriBuilder(stringValue).Host;
					int index = stringValue.IndexOf('.');
					string firstSegment = index < 0 ? stringValue : stringValue.Substring(0, index);
					if (firstSegment.Equals("www"))
					{
						stringValue = stringValue.Substring(index + 1);
					}
				}
				catch (UriFormatException)
				{
					//Do nothing
				}
			}

			yield return GetGrams(mainEntityID, rule, entityName, fieldName, stringValue, Decimal.Round(scoreWeight, 4));
		}
	}
}
