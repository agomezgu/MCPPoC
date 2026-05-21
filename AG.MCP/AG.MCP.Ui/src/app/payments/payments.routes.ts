import { Routes } from '@angular/router';
import { PaymentMasterPageComponent } from './pages/payment-master-page/payment-master-page.component';

export const PAYMENTS_ROUTES: Routes = [
  {
    path: '',
    component: PaymentMasterPageComponent,
    children: [
      { path: '', redirectTo: 'list', pathMatch: 'full' },
      {
        path: 'list',
        loadComponent: () =>
          import('./pages/payment-list-page/payment-list-page.component').then(
            (m) => m.PaymentListPageComponent
          )
      },
      {
        path: 'create',
        loadComponent: () =>
          import('./pages/payment-create-page/payment-create-page.component').then(
            (m) => m.PaymentCreatePageComponent
          )
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./pages/payment-detail-page/payment-detail-page.component').then(
            (m) => m.PaymentDetailPageComponent
          )
      }
    ]
  }
];
