import { Routes } from '@angular/router';
import { InvoiceMasterPageComponent } from './pages/invoice-master-page/invoice-master-page.component';

export const INVOICES_ROUTES: Routes = [
  {
    path: '',
    component: InvoiceMasterPageComponent,
    children: [
      { path: '', redirectTo: 'list', pathMatch: 'full' },
      {
        path: 'list',
        data: { invoiceMode: 'all' },
        loadComponent: () =>
          import('./pages/invoice-list-page/invoice-list-page.component').then(
            (m) => m.InvoiceListPageComponent
          )
      },
      {
        path: 'pending',
        data: { invoiceMode: 'pending' },
        loadComponent: () =>
          import('./pages/invoice-list-page/invoice-list-page.component').then(
            (m) => m.InvoiceListPageComponent
          )
      },
      {
        path: 'overdue',
        data: { invoiceMode: 'overdue' },
        loadComponent: () =>
          import('./pages/invoice-list-page/invoice-list-page.component').then(
            (m) => m.InvoiceListPageComponent
          )
      },
      {
        path: 'create',
        loadComponent: () =>
          import('./pages/invoice-create-page/invoice-create-page.component').then(
            (m) => m.InvoiceCreatePageComponent
          )
      },
      {
        path: ':id/edit',
        loadComponent: () =>
          import('./pages/invoice-edit-page/invoice-edit-page.component').then(
            (m) => m.InvoiceEditPageComponent
          )
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./pages/invoice-detail-page/invoice-detail-page.component').then(
            (m) => m.InvoiceDetailPageComponent
          )
      }
    ]
  }
];
