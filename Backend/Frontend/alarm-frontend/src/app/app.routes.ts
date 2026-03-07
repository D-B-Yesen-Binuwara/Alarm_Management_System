import { Routes } from '@angular/router';
import { CorrelationComponent } from './correlation/correlation.component';
import { ImpactComponent } from './impact/impact.component';

export const routes: Routes = [

  { path: '', component: CorrelationComponent },

  { path: 'impact', component: ImpactComponent }

];
