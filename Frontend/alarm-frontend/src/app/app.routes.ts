import { Routes } from '@angular/router';

import { LayoutComponent } from './layout/layout';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { AlarmComponent } from './pages/alarm/alarm';
import { CorrelationComponent } from './pages/correlation/correlation';
import { ImpactComponent } from './pages/impact/impact';
import { GraphComponent } from './pages/impact/graph/graph';
import { TraversalComponent } from './pages/traversal/traversal';

export const routes: Routes = [

  {
    path: '',
    component: LayoutComponent,
    children: [

      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      { path: 'dashboard', component: DashboardComponent },

      { path: 'alarm', component: AlarmComponent },

      { path: 'correlation', component: CorrelationComponent },

      { path: 'impact', component: ImpactComponent },

      { path: 'graph', component: GraphComponent },

      { path: 'traversal', component: TraversalComponent }

    ]
  }

];
