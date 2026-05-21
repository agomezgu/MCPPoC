import { Routes } from '@angular/router';
import { ReportMasterPageComponent } from './pages/report-master-page/report-master-page.component';

export const REPORTS_ROUTES: Routes = [
  {
    path: '',
    component: ReportMasterPageComponent,
    children: [
      { path: '', redirectTo: 'accounts-receivable', pathMatch: 'full' },
      {
        path: 'accounts-receivable',
        loadComponent: () =>
          import('./pages/accounts-receivable-page/accounts-receivable-page.component').then(
            (m) => m.AccountsReceivablePageComponent
          )
      },
      {
        path: 'sales-summary',
        loadComponent: () =>
          import('./pages/sales-summary-page/sales-summary-page.component').then(
            (m) => m.SalesSummaryPageComponent
          )
      },
      {
        path: 'client-statement',
        loadComponent: () =>
          import('./pages/client-statement-page/client-statement-page.component').then(
            (m) => m.ClientStatementPageComponent
          )
      }
    ]
  }
];
