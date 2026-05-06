import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'clients', pathMatch: 'full' },
  {
    path: 'clients',
    loadChildren: () =>
      import('./clients/clients.routes').then((routes) => routes.CLIENTS_ROUTES)
  },
  { path: '**', redirectTo: 'clients' }
];
