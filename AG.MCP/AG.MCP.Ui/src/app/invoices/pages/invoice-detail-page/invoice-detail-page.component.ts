import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { InvoicesApiService } from '../../invoices-api.service';
import { InvoiceDto } from '../../invoices.models';

@Component({
  selector: 'app-invoice-detail-page',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './invoice-detail-page.component.html',
  styleUrl: './invoice-detail-page.component.scss'
})
export class InvoiceDetailPageComponent implements OnInit {
  private readonly invoicesApi = inject(InvoicesApiService);
  private readonly route = inject(ActivatedRoute);

  protected invoice: InvoiceDto | null = null;
  protected invoiceId = '';
  protected isLoading = false;
  protected errorMessage = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage = 'Invoice id is required.';
      return;
    }
    this.invoiceId = id;
    this.load(id);
  }

  protected load(id: string): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.invoicesApi
      .getById(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (inv) => {
          this.invoice = inv;
        },
        error: () => {
          this.errorMessage = 'Invoice not found or unable to load.';
          this.invoice = null;
        }
      });
  }
}
