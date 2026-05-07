import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ReportsApiService } from '../../reports-api.service';
import { AccountsReceivableSummaryDto } from '../../reports.models';

@Component({
  selector: 'app-accounts-receivable-page',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './accounts-receivable-page.component.html',
  styleUrl: './accounts-receivable-page.component.scss'
})
export class AccountsReceivablePageComponent implements OnInit {
  private readonly reportsApi = inject(ReportsApiService);

  protected data: AccountsReceivableSummaryDto | null = null;
  protected isLoading = false;
  protected errorMessage = '';

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.reportsApi
      .getAccountsReceivable()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (d) => {
          this.data = d;
        },
        error: () => {
          this.errorMessage = 'Unable to load accounts receivable report.';
          this.data = null;
        }
      });
  }
}
