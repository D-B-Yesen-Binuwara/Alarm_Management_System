import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard';
import { PlaceholderPageComponent } from './shared/placeholder-page/placeholder-page';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'dashboard' },
	{ path: 'dashboard', component: DashboardComponent },
	{
		path: 'impact-analysis',
		component: PlaceholderPageComponent,
		data: { title: 'Impact Analysis' }
	},
	{
		path: 'network-map',
		component: PlaceholderPageComponent,
		data: { title: 'Network Map' }
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
