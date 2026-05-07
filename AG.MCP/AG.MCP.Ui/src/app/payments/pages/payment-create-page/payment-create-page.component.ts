import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { InvoicesApiService } from '../../../invoices/invoices-api.service';
import { InvoiceDto } from '../../../invoices/invoices.models';
import { PaymentsApiService } from '../../payments-api.service';
import { CreatePaymentRequest } from '../../payments.models';

function dateInputToIso(value: string): string {
  return new Date(`${value}T12:00:00`).toISOString();
}

@Component({
  selector: 'app-payment-create-page',
  imports: [FormsModule, RouterLink, DecimalPipe],
  templateUrl: './payment-create-page.component.html',
  styleUrl: './payment-create-page.component.scss'
})
export class PaymentCreatePageComponent implements OnInit {
  private readonly paymentsApi = inject(PaymentsApiService);
  private readonly invoicesApi = inject(InvoicesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected invoices: InvoiceDto[] = [];
  protected invoiceId = '';
  protected amount: number | null = null;
  protected paymentDate = '';
  protected reference = '';
  protected notes = '';

  protected isLoadingInvoices = false;
  protected isSaving = false;
  protected errorMessage = '';

  ngOnInit(): void {
    const qpInvoiceId = this.route.snapshot.queryParamMap.get('invoiceId');
    this.isLoadingInvoices = true;

    forkJoin({
      pending: this.invoicesApi.getPending({ page: 1, pageSize: 200 }),
      paged: this.invoicesApi.getPaged({ page: 1, pageSize: 200, sortBy: 'issueDate', sortDescending: true })
    })
      .pipe(finalize(() => (this.isLoadingInvoices = false)))
      .subscribe({
        next: ({ pending, paged }) => {
          const byId = new Map<string, InvoiceDto>();
          for (const inv of pending.items) {
            byId.set(inv.id, inv);
          }
          for (const inv of paged.items) {
            if (inv.pendingAmount > 0) {
              byId.set(inv.id, inv);
            }
          }
          this.invoices = [...byId.values()].sort((a, b) =>
            a.invoiceNumber.localeCompare(b.invoiceNumber)
          );

          if (qpInvoiceId && byId.has(qpInvoiceId)) {
            this.invoiceId = qpInvoiceId;
            this.prefillAmount(byId.get(qpInvoiceId)!);
          }
        },
        error: () => {
          this.errorMessage = 'Unable to load invoices for selection.';
          this.invoices = [];
        }
      });
  }

  protected onInvoiceChange(): void {
    const inv = this.invoices.find((i) => i.id === this.invoiceId);
    if (inv) {
      this.prefillAmount(inv);
    }
  }

  private prefillAmount(inv: InvoiceDto): void {
    if (this.amount === null || this.amount === 0) {
      this.amount = inv.pendingAmount;
    }
  }

  protected submit(): void {
    this.errorMessage = '';
    if (!this.invoiceId) {
      this.errorMessage = 'Select an invoice.';
      return;
    }
    if (this.paymentDate.length === 0) {
      this.errorMessage = 'Payment date is required.';
      return;
    }
    const amt = this.amount ?? 0;
    if (amt <= 0) {
      this.errorMessage = 'Amount must be greater than zero.';
      return;
    }

    const payload: CreatePaymentRequest = {
      invoiceId: this.invoiceId,
      amount: amt,
      paymentDate: dateInputToIso(this.paymentDate),
      reference: this.reference.trim() || null,
      notes: this.notes.trim() || null
    };

    this.isSaving = true;
    this.paymentsApi
      .create(payload)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (p) => {
          void this.router.navigate(['/payments', p.id]);
        },
        error: () => {
          this.errorMessage = 'Unable to register payment. Check amount and API availability.';
        }
      });
  }
}
