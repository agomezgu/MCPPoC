import { Routes } from '@angular/router';
import { ClientMasterPageComponent } from './pages/client-master-page/client-master-page.component';

export const CLIENTS_ROUTES: Routes = [
  {
    path: 'agent-create',
    loadComponent: () =>
      import('./pages/client-agent-create-page/client-agent-create-page.component').then(
        (component) => component.ClientAgentCreatePageComponent
      )
  },
  {
    path: '',
    component: ClientMasterPageComponent,
    children: [
      { path: '', redirectTo: 'list', pathMatch: 'full' },
      {
        path: 'list',
        loadComponent: () =>
          import('./pages/client-list-page/client-list-page.component').then(
            (component) => component.ClientListPageComponent
          )
      },
      {
        path: 'create',
        loadComponent: () =>
          import('./pages/client-create-page/client-create-page.component').then(
            (component) => component.ClientCreatePageComponent
          )
      },
      {
        path: ':id/edit',
        loadComponent: () =>
          import('./pages/client-update-page/client-update-page.component').then(
            (component) => component.ClientUpdatePageComponent
          )
      },
      {
        path: ':id/summary',
        loadComponent: () =>
          import('./pages/client-summary-page/client-summary-page.component').then(
            (component) => component.ClientSummaryPageComponent
          )
      }
    ]
  }
];
