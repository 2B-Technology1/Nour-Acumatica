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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PX.Data;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR
{
	public abstract class MarketingMemberImport<TGraph, TMain, TDetail> : PXGraphExtension<TGraph>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
		where TDetail : class, IBqlTable, new()
	{
		#region ctor

		public override void Initialize()
		{
			ImportEntity.Cache
				.AdjustUI(null)
				.For<CRMarketingMemberForImport.memberName>(_ =>
				{
					_.Visible = true;
					_.Enabled = true;
				})
				.SameFor<CRMarketingMemberForImport.fullName>()
				;

			ImportEntity.Cache
				.AdjustUI(null)
				.For<CRMarketingMemberForImport.contactID>(_ =>
				{
					_.Visible = false;
					_.Enabled = false;
				})
				.SameFor<CRMarketingMemberForImport.linkCampaignID>()
				.SameFor<CRMarketingMemberForImport.linkMarketingListID>()

				.SameFor<CRMarketingMemberForImport.displayName>()
				.SameFor<CRMarketingMemberForImport.assignDate>()
				.SameFor<CRMarketingMemberForImport.attention>()
				.SameFor<CRMarketingMemberForImport.title>()
				.SameFor<CRMarketingMemberForImport.duplicateFound>()
				.SameFor<CRMarketingMemberForImport.duplicateStatus>()
				.SameFor<CRMarketingMemberForImport.resolution>()
				.SameFor<CRMarketingMemberForImport.searchSuggestion>()
				.SameFor<CRMarketingMemberForImport.selected>()
				.SameFor<CRMarketingMemberForImport.synchronize>()
				.SameFor<CRMarketingMemberForImport.anniversary>()
				.SameFor<CRMarketingMemberForImport.ownerID>()
				.SameFor<CRMarketingMemberForImport.status>()

				.SameFor<CRMarketingMemberForImport.defAddressID>()
				.SameFor<CRMarketingMemberForImport.addressType>()
				;
		}

		#endregion

		#region Views

		[PXHidden]
		public PXSelect<PXImportAttribute.PXImportSettings> ImportSettings;

		[PXHidden]
		public PXSelect<CRMarketingMemberForImport, Where<True, Equal<False>>> ImportEntity;

		[PXHidden]
		public PXInnerProcessing<TMain, TDetail> ImportMembers;

		#endregion

		#region Events

		protected virtual void _(Events.RowInserting<PXImportAttribute.PXImportSettings> e)
		{
			e.Row.Mode = PXImportAttribute.ImportMode.BYPASS_EXISTING;
		}

		protected virtual void _(Events.RowSelected<PXImportAttribute.PXImportSettings> e)
		{
			e.Cache
				.AdjustUI(e.Row)
				.For<PXImportAttribute.PXImportSettings.mode>(_ => _.Enabled = false);
		}

		protected virtual void _(Events.RowSelected<TMain> e)
		{
			ImportMembers.SetParametersDelegate(ParametersDelegate);
		}

		#endregion

		#region Methods

		public virtual bool ParametersDelegate(List<TMain> list)
		{
			list.Clear();

			var membersToCreate = Base
				.Caches[typeof(CRMarketingMemberForImport)]
				.Inserted
				.RowCast<CRMarketingMemberForImport>()
				.ToList();

			GenerateDummyRowsForProcessingResultsGrid(list, membersToCreate);

			ImportMembers.SetProcessDelegate(delegate(List<TMain> _, CancellationToken cancellationToken)
			{
				ProcessingHandler(membersToCreate, cancellationToken);
			});

			return true;
		}

		public abstract void GenerateDummyRowsForProcessingResultsGrid(List<TMain> list, List<CRMarketingMemberForImport> membersToCreate);

		public virtual Contact FindContactByContactID(int? contactID)
		{
			return PXSelect<
					Contact,
					Where<
						Contact.contactID, Equal<Required<Contact.contactID>>,
						And<Where<
							Contact.contactType, Equal<ContactTypesAttribute.lead>,
							Or<Contact.contactType, Equal<ContactTypesAttribute.person>,
							Or<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>>>>,
					OrderBy<
						Asc<Contact.contactPriority>>>
				.Select(Base, contactID);
		}

		public virtual Contact FindContactByMemberName(string contactDisplayName)
		{
			return PXSelect<
					Contact,
					Where<
						Contact.memberName, Equal<Required<Contact.memberName>>,
						And<Where<
							Contact.contactType, Equal<ContactTypesAttribute.lead>,
							Or<Contact.contactType, Equal<ContactTypesAttribute.person>,
							Or<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>>>>,
					OrderBy<
						Asc<Contact.contactPriority>>>
				.Select(Base, contactDisplayName);
		}

		public static void ProcessingHandler(List<CRMarketingMemberForImport> newMembers, CancellationToken cancellationToken)
		{
			var processingGraph = PXGraph.CreateInstance<TGraph>();
			var processingExt = processingGraph.GetProcessingExtension<MarketingMemberImport<TGraph, TMain, TDetail>>();

			processingExt.CreateEntityFromImport(newMembers, cancellationToken);
		}

		public virtual void CreateEntityFromImport(List<CRMarketingMemberForImport> newMembers, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var contactCounter = 0;
			var leadCounter = 0;
			var baCounter = 0;

			ContactMaint contactMaint = null;
			LeadMaint leadMaint = null;
			BusinessAccountMaint baMaint = null;

			for (int i = 0; i < newMembers.Count; i++)
			{
				var newMember = newMembers[i];

				Contact result = newMember;

				try
				{
					if (newMember.ContactType == ContactTypesAttribute.Lead)
					{
						if (leadCounter++ % 100 == 0 || leadMaint == null)
						{
							leadMaint = PXGraph.CreateInstance<LeadMaint>();

							var dedupExt = leadMaint
								.GetExtension<LeadMaint.CRDuplicateEntitiesForLeadGraphExt>();

							if (dedupExt != null)
								dedupExt.HardBlockOnly = true;
						}

						result = CreateLeadFromImport(leadMaint, newMember);

						leadMaint.Save.PressImpl(false, true);
					}
					else if (newMember.ContactType == ContactTypesAttribute.Person)
					{
						if (contactCounter++ % 100 == 0 || contactMaint == null)
						{
							contactMaint = PXGraph.CreateInstance<ContactMaint>();

							var dedupExt = contactMaint
								.GetExtension<ContactMaint.CRDuplicateEntitiesForContactGraphExt>();

							if (dedupExt != null)
								dedupExt.HardBlockOnly = true;
						}

						result = CreateContactFromImport(contactMaint, newMember);

						contactMaint.Save.PressImpl(false, true);
					}
					else if (newMember.ContactType == ContactTypesAttribute.BAccountProperty)
					{
						if (newMember.ExistingContactID == null)
							continue;

						if (baCounter++ % 100 == 0 || baMaint == null)
						{
							baMaint = PXGraph.CreateInstance<BusinessAccountMaint>();

							var dedupExt = baMaint
								.GetExtension<BusinessAccountMaint.CRDuplicateEntitiesForBAccountGraphExt>();

							if (dedupExt != null)
								dedupExt.HardBlockOnly = true;
						}

						result = Contact.PK.Find(Base, newMember.ExistingContactID);

						InsertBAMarketingInfoIfNotExists(baMaint, newMember.ExistingContactID, newMember);

						baMaint.Save.PressImpl(false, true);
					}

					var output = (!string.IsNullOrWhiteSpace(result?.DisplayName)
									? result?.DisplayName
									: result?.FullName)
								?? result.MemberName;

					PXProcessing<TMain>.SetInfo(i, PXMessages.LocalizeFormatNoPrefixNLA(MessagesNoPrefix.MarketingMemberImportResult_Successfull, output));
				}
				catch (PXException ex)
				{
					var output = (!string.IsNullOrWhiteSpace(result?.DisplayName)
						? result?.DisplayName
						: result?.FullName);

					if (output == null && result.MemberName != null)
					{
						PXProcessing<TMain>.SetWarning(i, PXMessages.LocalizeFormatNoPrefixNLA(MessagesNoPrefix.MarketingMemberImportResult_Warning, result.MemberName));
					}
					else
					{
						PXProcessing<TMain>.SetError(i, PXMessages.LocalizeFormatNoPrefixNLA(MessagesNoPrefix.MarketingMemberImportResult_Error, output, ex.Message));
					}

				}
				finally
				{
					contactMaint?.Clear();
					leadMaint?.Clear();
					baMaint?.Clear();
				}
			}
		}

		#region Create Contact

		public virtual Contact CreateContactFromImport(ContactMaint graph, CRMarketingMemberForImport newMember)
		{
			Contact contact;

			if (newMember.ExistingContactID == null)
			{
				contact = CreateNewContactFromImport(graph, newMember);
			}
			else
			{
				graph.Contact.Current
					= contact
						= Contact.PK.Find(Base, newMember.ExistingContactID);
			}

			InsertContactMarketingInfoIfNotExists(graph, contact, newMember);

			return contact;
		}

		public virtual Contact CreateNewContactFromImport(ContactMaint graph, CRMarketingMemberForImport newMember)
		{
			Contact contact = graph.Contact.Insert(newMember as Contact);

			contact.OverrideAddress = contact.BAccountID != null;

			contact = graph.Contact.Update(contact);

			Address address = graph.AddressCurrent.View.SelectSingle() as Address;
			if (address != null)
			{
				address.CountryID = newMember.CountryID;
				address.City = newMember.City;
				address.State = newMember.State;
				address.PostalCode = newMember.PostalCode;
				address.AddressLine1 = newMember.AddressLine1;
				address.AddressLine2 = newMember.AddressLine2;
				address.AddressLine3 = newMember.AddressLine3;

				graph.AddressCurrent.Cache.Update(address);
			}

			return contact;
		}

		public abstract void InsertContactMarketingInfoIfNotExists(ContactMaint graph, Contact contact, CRMarketingMemberForImport newMember);

		#endregion

		#region Create Lead

		public virtual CRLead CreateLeadFromImport(LeadMaint graph, CRMarketingMemberForImport newMember)
		{
			CRLead lead;

			if (newMember.ExistingContactID == null)
			{
				lead = CreateNewLeadFromImport(graph, newMember);
			}
			else
			{
				graph.Lead.Current
					= lead
						= CRLead.PK.Find(Base, newMember.ExistingContactID);
			}

			InsertLeadMarketingInfoIfNotExtsis(graph, lead, newMember);

			return lead;
		}

		public virtual CRLead CreateNewLeadFromImport(LeadMaint graph, CRMarketingMemberForImport newMember)
		{
			CRLead lead = graph.Lead.Extend(newMember as Contact);

			graph.Lead.Cache.SetStatus(lead, PXEntryStatus.Inserted);

			graph.Lead.Cache.SetDefaultExt<CRLead.resolution>(lead);

			lead.OverrideRefContact = lead.BAccountID != null;

			lead.Description = newMember.Description;

			lead = graph.Lead.Update(lead);

			Address address = graph.AddressCurrent.Insert() as Address;

			lead.DefAddressID = address.AddressID;

			lead = graph.Lead.Update(lead);

			address.CountryID = newMember.CountryID;
			address.City = newMember.City;
			address.State = newMember.State;
			address.PostalCode = newMember.PostalCode;
			address.AddressLine1 = newMember.AddressLine1;
			address.AddressLine2 = newMember.AddressLine2;
			address.AddressLine3 = newMember.AddressLine3;

			graph.AddressCurrent.Cache.Update(address);

			return lead;
		}

		public abstract void InsertLeadMarketingInfoIfNotExtsis(LeadMaint graph, CRLead contact, CRMarketingMemberForImport newMember);

		#endregion

		public abstract void InsertBAMarketingInfoIfNotExists(BusinessAccountMaint graph, int? contactID, CRMarketingMemberForImport newMember);

		#endregion

		#region IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (!values.Contains(nameof(CRMarketingMemberForImport.MemberName)) || values[nameof(CRMarketingMemberForImport.MemberName)] == null)
				return true;

			Contact contact;

			if (int.TryParse(values[nameof(CRMarketingMemberForImport.MemberName)].ToString(), out var contactID))
			{
				contact = FindContactByContactID(contactID);
			}
			else
			{
				string contactMemberName = values[nameof(CRMarketingMemberForImport.MemberName)].ToString();

				contact = FindContactByMemberName(contactMemberName);
			}

			if (contact == null)
				return true;

			values[nameof(CRMarketingMemberForImport.ExistingContactID)] = contact.ContactID;
			values[nameof(CRMarketingMemberForImport.ContactType)] = contact.ContactType;

			return true;
		}

		public virtual bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public virtual bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
		}

		#endregion

		#region IPXProcess

		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			Base.Actions["ProcessAll"].Press();
		}

		#endregion
	}
}
