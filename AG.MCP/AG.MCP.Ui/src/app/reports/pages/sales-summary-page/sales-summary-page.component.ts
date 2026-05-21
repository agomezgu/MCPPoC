import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ReportsApiService } from '../../reports-api.service';
import { SalesSummaryDto } from '../../reports.models';

function dateToParam(d: string): string | null {
  return d?.length ? new Date(`${d}T12:00:00`).toISOString() : null;
}

@Component({
  selector: 'app-sales-summary-page',
  imports: [FormsModule, DatePipe, DecimalPipe],
  templateUrl: './sales-summary-page.component.html',
  styleUrl: './sales-summary-page.component.scss'
})
export class SalesSummaryPageComponent implements OnInit {
  private readonly reportsApi = inject(ReportsApiService);

  protected fromDate = '';
  protected toDate = '';
  protected data: SalesSummaryDto | null = null;
  protected isLoading = false;
  protected errorMessage = '';

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.reportsApi
      .getSalesSummary(dateToParam(this.fromDate), dateToParam(this.toDate))
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (d) => {
          this.data = d;
        },
        error: () => {
          this.errorMessage = 'Unable to load sales summary.';
          this.data = null;
        }
      });
  }
}
