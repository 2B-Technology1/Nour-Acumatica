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
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.CR
{
	public class ValidateDocumentAddressGraph<TGraph> : PXGraph<TGraph>
		where TGraph : PXGraph, new()
	{
		#region Constants
		protected readonly static Type[] AddressFieldsToValidate = new Type[6]
		{
			typeof(UnvalidatedAddress.addressLine1), typeof(UnvalidatedAddress.addressLine2), typeof(UnvalidatedAddress.city),
			typeof(UnvalidatedAddress.state), typeof(UnvalidatedAddress.countryID), typeof(UnvalidatedAddress.postalCode)
		};
		#endregion

		#region Views
		[PXCacheName(Messages.Filter)]
		public PXFilter<ValidateDocumentAddressFilter> Filter;

		[PXCacheName(Messages.Address)]
		[PXFilterable]
		public PXFilteredProcessing<UnvalidatedAddress, ValidateDocumentAddressFilter> DocumentAddresses;
		#endregion

		#region Data View Delegate
		public virtual IEnumerable documentAddresses()
		{
			IEnumerable addresses = new List<UnvalidatedAddress>();

			if (!string.IsNullOrEmpty(Filter.Current?.DocumentType))
			{
				addresses = DocumentAddresses.Cache.Inserted.Count() > 0
					? GetCachedAddressRecords(Filter.Current)
					: GetAddressRecords(Filter.Current);
			}

			return addresses;
		}
		#endregion

		#region Actions
		public PXCancel<ValidateDocumentAddressFilter> Cancel;

		public PXAction<UnvalidatedAddress> viewDocument;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			UnvalidatedAddress documentAddress = DocumentAddresses.Current;
			PXGraph graph = null;
			if (documentAddress != null && !string.IsNullOrEmpty(documentAddress.DocumentNbr))
			{
				graph = GetDocGraph(documentAddress.Document);

				if (graph != null)
				{
					graph.Views[graph.PrimaryView].Cache.Current = documentAddress.Document;
					if (graph.Views[graph.PrimaryView]?.Cache?.Current != null)
					{
						throw new PXRedirectRequiredException(graph, true, string.Empty) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
			}

			return adapter.Get();
		}
		#endregion

		#region Constructor
		public ValidateDocumentAddressGraph()
		{
			DocumentAddresses.SetSelected<UnvalidatedAddress.selected>();
			DocumentAddresses.SetProcessCaption(PXMessages.LocalizeNoPrefix(Messages.Validate));
			DocumentAddresses.SetProcessAllCaption(PXMessages.LocalizeNoPrefix(Messages.ValidateAll));
			Actions.Move(Messages.Process, Messages.Cancel);
		}
		#endregion

		#region Event Handlers
		protected virtual void _(Events.RowSelected<ValidateDocumentAddressFilter> e)
		{
			ValidateDocumentAddressFilter filter = e.Row;
			if (filter == null) return;

			bool isCountryWarning = PXSelectReadonly<Country, Where<Country.addressValidatorPluginID, IsNotNull>>.Select(this).Count == 0;
			PXUIFieldAttribute.SetWarning(Filter.Cache, null, nameof(filter.Country), isCountryWarning ? Messages.CountryValidationWarning : null);

			DocumentAddresses.SetProcessDelegate(
				delegate (List<UnvalidatedAddress> documentAddresses)
				{
					ProcessAddress(filter, documentAddresses);
				});
		}

		protected virtual void _(Events.RowUpdated<ValidateDocumentAddressFilter> e)
		{
			ValidateDocumentAddressFilter row = e.Row;
			ValidateDocumentAddressFilter oldRow = e.OldRow;
			var actionChanged = row.Country != oldRow.Country || row.DocumentType != oldRow.DocumentType;

			if (actionChanged)
			{
				DocumentAddresses.Cache.Clear();
			}
		}
		#endregion

		#region Public methods
		public static void ProcessAddress(ValidateDocumentAddressFilter filter, List<UnvalidatedAddress> addresses)
		{
			bool isOverride = filter?.IsOverride ?? false;
			PXGraph docGraph = GetDocGraph(addresses[0].Document);

			foreach (UnvalidatedAddress address in addresses)
			{
				PXGraph graph = CreateInstance<ValidateDocumentAddressGraph<TGraph>>();
				List<string> warnings = new List<string>();
				foreach (Type field in AddressFieldsToValidate)
				{
					graph.ExceptionHandling.AddHandler(typeof(UnvalidatedAddress), field.Name,
						new PXExceptionHandling((sender, e) => PXAddressValidator.OnFieldException(sender, e, field, ref warnings)));
				}
				try
				{
					PXProcessing<UnvalidatedAddress>.SetCurrentItem(address);
					if (ValidateDocumentAddress(processingScreenGraph: graph, address, isOverride, documentGraph: docGraph))
					{
						PXProcessing<UnvalidatedAddress>.SetProcessed();
					}
					else
					{
						PXProcessing<UnvalidatedAddress>.SetWarning(PXAddressValidator.FormatWarningMessage(warnings));
					}
				}
				catch (PXException unknownException)
				{
					PXProcessing<UnvalidatedAddress>.SetError(unknownException);
				}
			}
		}
		#endregion

		#region Protected Methods
		protected virtual IEnumerable GetCachedAddressRecords(ValidateDocumentAddressFilter filter)
		{
			foreach (UnvalidatedAddress item in DocumentAddresses.Cache.Inserted)
			{
				yield return item;
			}
		}

		protected virtual IEnumerable GetAddressRecords(ValidateDocumentAddressFilter filter)
		{
			// Override this method in other project's graph implemention and include the addresses specific to documents applicable to that screen
			yield return null;
		}

		protected static bool ValidateDocumentAddress(PXGraph processingScreenGraph, UnvalidatedAddress originalAddress, bool isOverride, PXGraph documentGraph)
		{
			if (processingScreenGraph == null || originalAddress == null)
				return false;

			if (originalAddress.Address.IsValidated == true)
				return true;

			PXCache cache = processingScreenGraph.Caches[(originalAddress.Address).GetType()];
			IAddress addressToValidate = (IAddress)(!isOverride ? originalAddress.Address : cache.Insert(originalAddress.Address));

			if (PXAddressValidator.Validate(processingScreenGraph, addressToValidate, aSynchronous: true,
				updateToValidAddress: isOverride, forceOverride: isOverride,
				out IList<(string fieldName, string fieldValue, string warningMessage)> warningMessages))
			{
				// if some address fields are modified, then use document graph to persist. This would invoke events related to the field update.
				addressToValidate.IsValidated = true;
				if (isOverride && warningMessages.Count > 0)
				{
					SaveDocumentAddress(documentGraph, originalAddress.Document, addressToValidate);
				}
				else
				{
					// CacheAttached event is defined in the specific screen graphs.
					// Therefore create graph instance of the specific graph to persist
					PXGraph docGraph = CreateInstance<TGraph>();
					PXCache docCache = docGraph.Caches[(originalAddress.Address).GetType()];
					docCache.Update(addressToValidate);
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						docCache.Persist(addressToValidate, PXDBOperation.Update);
						ts.Complete(docGraph);
					}
				}
				return true;
			}
			else
			{
				PXCache docCache = processingScreenGraph.Caches[typeof(UnvalidatedAddress)];
				foreach (var item in warningMessages)
				{
					string error = item.warningMessage;
					docCache.RaiseExceptionHandling(item.fieldName, originalAddress, item.fieldValue, new PXSetPropertyException(error, PXErrorLevel.Warning));
				}
				return false;
			}
		}

		protected static PXGraph GetDocGraph(IBqlTable document)
		{
			PXGraph graph = PXGraph.CreateInstance<ValidateDocumentAddressGraph<TGraph>>();
			EntityHelper helper = new EntityHelper(graph);
			Type cacheType = document.GetType();
			Type graphType = helper.GetPrimaryGraphType(cacheType, document, false);
			return graphType != null ? CreateInstance(graphType) : null;
		}

		protected static void SaveDocumentAddress(PXGraph docGraph, IBqlTable row, IAddress address)
		{
			docGraph.Views[docGraph.PrimaryView].Cache.Current = row;
			PXCache addrCache = docGraph.Caches[address.GetType()];
			addrCache.Update(address);
			docGraph.Actions.PressSave();

			// clear the graph state as same instance is used for processing all the addresses
			docGraph.Clear();
		}

		protected virtual UnvalidatedAddress ConvertToUnvalidatedAddress<TAddress>(TAddress address, IBqlTable document, string documentNbr, string documentType, string status)
			where TAddress : IAddress
		{
			UnvalidatedAddress unvalidatedAddress = new UnvalidatedAddress();
			unvalidatedAddress.DocumentNbr = documentNbr;
			unvalidatedAddress.DocumentType = documentType;
			unvalidatedAddress.AddressID = address.AddressID;
			unvalidatedAddress.Status = status;
			unvalidatedAddress.AddressLine1 = address.AddressLine1;
			unvalidatedAddress.AddressLine2 = address.AddressLine2;
			unvalidatedAddress.City = address.City;
			unvalidatedAddress.State = address.State;
			unvalidatedAddress.PostalCode = address.PostalCode;
			unvalidatedAddress.CountryID = address.CountryID;
			unvalidatedAddress.Address = address;
			unvalidatedAddress.Document = document;

			return unvalidatedAddress;
		}
		#endregion
	}
}
