import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { PaymentsApiService } from '../../payments-api.service';
import { PaymentDto } from '../../payments.models';

@Component({
  selector: 'app-payment-list-page',
  imports: [FormsModule, RouterLink, DatePipe, DecimalPipe],
  templateUrl: './payment-list-page.component.html',
  styleUrl: './payment-list-page.component.scss'
})
export class PaymentListPageComponent implements OnInit {
  private readonly paymentsApi = inject(PaymentsApiService);

  protected payments: PaymentDto[] = [];
  protected isLoading = false;
  protected errorMessage = '';

  protected page = 1;
  protected pageSize = 20;
  protected totalCount = 0;
  protected search = '';
  protected sortBy = 'paymentDate';
  protected sortDescending = true;

  ngOnInit(): void {
    this.loadPayments();
  }

  protected loadPayments(): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.paymentsApi
      .getPaged({
        page: this.page,
        pageSize: this.pageSize,
        search: this.search.trim() || null,
        sortBy: this.sortBy.trim() || null,
        sortDescending: this.sortDescending
      })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (r) => {
          this.payments = r.items;
          this.totalCount = r.totalCount;
        },
        error: () => {
          this.errorMessage = 'Unable to load payments. Check if AG.MCP.Api is running.';
        }
      });
  }

  protected applyFilters(): void {
    this.page = 1;
    this.loadPayments();
  }

  protected goPrev(): void {
    if (this.page > 1) {
      this.page -= 1;
      this.loadPayments();
    }
  }

  protected goNext(): void {
    const maxPage = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
    if (this.page < maxPage) {
      this.page += 1;
      this.loadPayments();
    }
  }

  protected get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }
}
