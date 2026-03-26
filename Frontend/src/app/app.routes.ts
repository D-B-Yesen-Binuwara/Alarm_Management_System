import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard';
import { NetworkMapComponent } from './features/network-map/network-map';
import { PlaceholderPageComponent } from './shared/placeholder-page/placeholder-page';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'dashboard' },
	{ path: 'dashboard', component: DashboardComponent },
	{
		path: 'network-map',
		component: NetworkMapComponent
	},
	{
		path: 'impact-analysis',
		component: PlaceholderPageComponent,
		data: { title: 'Impact Analysis' }
	},
	{
		path: 'events',
		component: PlaceholderPageComponent,
		data: { title: 'Events' }
	},
	{
		path: 'home',
		component: PlaceholderPageComponent,
		data: { title: 'Home' }
	},
	{ path: '**', redirectTo: 'dashboard' }
];
