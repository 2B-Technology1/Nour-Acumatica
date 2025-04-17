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

using PX.Api.ContractBased.Models;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce
{
	public class BCPriceClassEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Group;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedGroup Group;
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.CustomerPriceClass, BCCaptions.CustomerPriceClass, 10,
		IsInternal = false,
		Direction = SyncDirection.Bidirect,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.AR.ARPriceClassMaint),
		ExternTypes = new Type[] { typeof(CustomerGroupData) },
		LocalTypes = new Type[] { typeof(CustomerPriceClass) },
		AcumaticaPrimaryType = typeof(PX.Objects.AR.ARPriceClass),
		AcumaticaPrimarySelect = typeof(PX.Objects.AR.ARPriceClass.priceClassID),
		AcumaticaFeaturesSet = typeof(FeaturesSet.commerceB2B),
		URL = "customers/groups/{0}/edit",
		Requires = new string[] { }
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false, Hidden = true,
		PushSources = new String[] { "BC-PUSH-PriceClass" }, PushDestination = BCConstants.PushNotificationDestination)]
	public class BCPriceClassProcessor : BCProcessorSingleBase<BCPriceClassProcessor, BCPriceClassEntityBucket, MappedGroup>
	{
		[InjectDependency]
		protected IBCRestDataProviderFactory<IParentRestDataProvider<CustomerGroupData>> customerPriceClassDataProviderFactory { get; set; }
		protected IParentRestDataProvider<CustomerGroupData> customerPriceClassDataProvider { get; set; }

		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);

			customerPriceClassDataProvider = customerPriceClassDataProviderFactory.CreateInstance(
				BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>()));
		}
		#endregion

		#region Common
		public override async Task<MappedGroup> PullEntity(Guid? localID, Dictionary<string, object> externalInfo, CancellationToken cancellationToken = default)
		{
			CustomerPriceClass impl = cbapi.GetByID<CustomerPriceClass>(localID);
			if (impl == null) return null;

			MappedGroup obj = new MappedGroup(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}

		public override async Task<MappedGroup> PullEntity(string externID, String externalInfo, CancellationToken cancellationToken = default)
		{
			CustomerGroupData data = customerPriceClassDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedGroup obj = new MappedGroup(data, data.Id?.ToString(), data.Name, data.CalculateHash());

			return obj;
		}

		public override async Task<PullSimilarResult<MappedGroup>> PullSimilar(IExternEntity entity, CancellationToken cancellationToken = default)
		{
			var uniqueFieldValue = ((CustomerGroupData)entity)?.Name;
			if (uniqueFieldValue == null) return null;
			CustomerPriceClass[] impls = cbapi.GetAll(new CustomerPriceClass() { PriceClassID = uniqueFieldValue.SearchField() },
				filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>().ToArray(), cancellationToken: cancellationToken).ToArray();
			if (impls == null) return null;

			return new PullSimilarResult<MappedGroup>() { UniqueField = uniqueFieldValue, Entities = impls.Select(impl => new MappedGroup(impl, impl.SyncID, impl.SyncTime)) };
		}

		public override async Task<PullSimilarResult<MappedGroup>> PullSimilar(ILocalEntity entity, CancellationToken cancellationToken = default)
		{
			var uniqueFieldValue = ((CustomerPriceClass)entity)?.PriceClassID?.Value;
			if (uniqueFieldValue == null) return null;
			IEnumerable<CustomerGroupData> datas = customerPriceClassDataProvider.GetAll(new FilterGroups() { Name = uniqueFieldValue }, cancellationToken);
			if (datas == null) return null;

			return new PullSimilarResult<MappedGroup>() { UniqueField = uniqueFieldValue, Entities = datas.Select(data => new MappedGroup(data, data.Id.ToString(), data.Name, data.CalculateHash())) };
		}


		#endregion

		#region Import
		public override async Task<EntityStatus> GetBucketForImport(BCPriceClassEntityBucket bucket, BCSyncStatus bcstatus, CancellationToken cancellationToken = default)
		{
			CustomerGroupData data = customerPriceClassDataProvider.GetByID(bcstatus.ExternID);
			if (data == null) return EntityStatus.None;

			MappedGroup obj = bucket.Group = bucket.Group.Set(data, data.Id?.ToString(), data.Name, data.CalculateHash());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			return status;
		}
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			FilterGroups filter = null;

			IEnumerable<CustomerGroupData> datas = customerPriceClassDataProvider.GetAll(filter, cancellationToken);

			foreach (CustomerGroupData data in datas)
			{
				var bucket = CreateBucket();
				MappedGroup obj = bucket.Group = bucket.Group.Set(data, data.Id?.ToString(), data.Name, data.CalculateHash());
				EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
			}
		}

		public override async Task MapBucketImport(BCPriceClassEntityBucket entity, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedGroup obj = entity.Group;
			CustomerPriceClass impl = obj.Local = new CustomerPriceClass();
			CustomerGroupData data = obj.Extern;

			impl.PriceClassID = data.Name.ValueField();
		}

		public override async Task SaveBucketImport(BCPriceClassEntityBucket bucket, IMappedEntity existing, string operation, CancellationToken cancellationToken = default)
		{
			MappedGroup obj = bucket.Group;

			CustomerPriceClass impl = cbapi.Put<CustomerPriceClass>(obj.Local, obj.LocalID);

			if (obj.LocalID != impl.SyncID) obj.LocalID = null;
			bucket.Group.AddLocal(impl, impl.SyncID, impl.SyncTime);
			UpdateStatus(obj, operation);
		}
		#endregion

		#region Export
		public override async Task FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			IEnumerable<CustomerPriceClass> impls = cbapi.GetAll<CustomerPriceClass>(new CustomerPriceClass() { PriceClassID = new StringReturn() }, minDateTime, maxDateTime, filters, cancellationToken: cancellationToken);

			foreach (CustomerPriceClass impl in impls)
			{
				if (impl.SyncID == null) continue; //We need to skip the root node, which does not have a ID

				MappedGroup obj = new MappedGroup(impl, impl.SyncID, impl.SyncTime);
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
			}
		}
		public override async Task<EntityStatus> GetBucketForExport(BCPriceClassEntityBucket bucket, BCSyncStatus bcstatus, CancellationToken cancellationToken = default)
		{
			CustomerPriceClass impl = cbapi.GetByID<CustomerPriceClass>(bcstatus.LocalID);
			if (impl == null) return EntityStatus.None;

			MappedGroup obj = bucket.Group = bucket.Group.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			return status;
		}

		public override async Task MapBucketExport(BCPriceClassEntityBucket entity, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedGroup obj = entity.Group;
			CustomerGroupData data = obj.Extern = new CustomerGroupData();
			CustomerPriceClass impl = obj.Local;
			data.Name = impl.PriceClassID?.Value;
		}

		public override async Task SaveBucketExport(BCPriceClassEntityBucket bucket, IMappedEntity existing, string operation, CancellationToken cancellationToken = default)
		{
			MappedGroup obj = bucket.Group;
			CustomerGroupData data;
			if (obj.ExternID == null)
				data = customerPriceClassDataProvider.Create(obj.Extern);
			else
				data = customerPriceClassDataProvider.Update(obj.Extern, obj.ExternID);

			obj.AddExtern(data, data.Id?.ToString(), data.Name, data.CalculateHash());
			UpdateStatus(obj, operation);
		}
		#endregion
	}
}
