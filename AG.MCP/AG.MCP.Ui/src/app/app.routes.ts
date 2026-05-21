import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'clients', pathMatch: 'full' },
  {
    path: 'clients',
    loadChildren: () =>
      import('./clients/clients.routes').then((r) => r.CLIENTS_ROUTES)
  },
  {
    path: 'invoices',
    loadChildren: () =>
      import('./invoices/invoices.routes').then((r) => r.INVOICES_ROUTES)
  },
  {
    path: 'payments',
    loadChildren: () =>
      import('./payments/payments.routes').then((r) => r.PAYMENTS_ROUTES)
  },
  {
    path: 'reports',
    loadChildren: () =>
      import('./reports/reports.routes').then((r) => r.REPORTS_ROUTES)
  },
  { path: '**', redirectTo: 'clients' }
];
