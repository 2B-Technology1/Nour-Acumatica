import { autoinject, PLATFORM  } from 'aurelia-framework';
import { PXView, createCollection, graphInfo, createSingle, GenericInquiryApiClient, ScreenApiClient, GridPagerMode, DynamicNewInstanceResolver, PXScreen } from 'client-controls';
import { Container } from 'aurelia-dependency-injection';
import { GridApiClient, GridGenericInquiryApiClient } from 'client-controls/controls/compound/grid/grid-api-client';

@graphInfo({ graphType: 'PX.Data.PXGenericInqGrph', primaryView: 'Results' })
@autoinject
export class GI000000 extends PXScreen {
	Filter = createSingle(Filter);

	Results = createCollection(Results, {
		adjustPageSize: true, syncPosition: true, allowUpdate: false, allowInsert: false,
		mergeToolbarWith: "ScreenToolbar", pagerMode: GridPagerMode.Numeric
	});

	constructor(protected container: Container) {
		super();

		const gridApiClient = container.invoke(GridGenericInquiryApiClient, [this.genericInquiryId]);
		container.registerInstance(GridApiClient, gridApiClient);
		container.registerInstance(DynamicNewInstanceResolver, new GINewInstanceResolver(this.genericInquiryId));
	}

	get genericInquiryId(): string | undefined {
		const urlParams = new URLSearchParams(PLATFORM.global.location.search);
		return urlParams.get('id');
	}
}

export class Filter extends PXView {
}

export class Results extends PXView {
}

class GINewInstanceResolver extends DynamicNewInstanceResolver {

	constructor(private genericInquiryId: string) {
		super();
	}

	resolveType(type: any, dynamicParams: any[])	{
		if (type?.name === ScreenApiClient.name) {
			dynamicParams?.push(this.genericInquiryId);
			return GenericInquiryApiClient;
		}
		else if (type?.name === GridApiClient.name) {
			dynamicParams?.push(this.genericInquiryId);
			return GridGenericInquiryApiClient;
		}
		return super.resolveType(type, dynamicParams);
	}
}
