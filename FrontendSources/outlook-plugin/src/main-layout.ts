import { autoinject, Disposable } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';

@autoinject
export class MainLayout
{
	subscriptions: Disposable[] = new Array();
	initialized = false;
	callbackRunning = 0;
	private ready = false;

	constructor(private eventAggregator: EventAggregator) {	}

	attached()
	{
		let ss = this.eventAggregator.subscribe("screen-initialized", (params) => {
			this.ready = true;
			if (!params.pendingUpdate) this.initialized = true
		});
		this.subscriptions.push(ss);

		ss = this.eventAggregator.subscribe("callback-started", () => this.callbackRunning ++ );
		this.subscriptions.push(ss);

		ss = this.eventAggregator.subscribe("callback-completed", () => {
			this.callbackRunning -- ;
			if (this.callbackRunning === 0 && this.ready) this.initialized = true;
		});
		this.subscriptions.push(ss);
	}

	detached()
	{
		this.subscriptions.forEach(s => s.dispose());
		this.subscriptions = [];
	}
}
