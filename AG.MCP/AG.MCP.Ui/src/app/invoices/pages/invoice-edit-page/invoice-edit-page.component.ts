import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { InvoicesApiService } from '../../invoices-api.service';
import {
  CreateInvoiceItemRequest,
  InvoiceDto,
  UpdateInvoiceRequest
} from '../../invoices.models';

interface LineDraft {
  description: string;
  productCode: string;
  quantity: number;
  unit: string;
  unitPrice: number;
}

function dateInputToIso(value: string): string {
  return new Date(`${value}T12:00:00`).toISOString();
}

function isoToDateInput(iso: string): string {
  return iso?.length >= 10 ? iso.slice(0, 10) : '';
}

@Component({
  selector: 'app-invoice-edit-page',
  imports: [FormsModule, RouterLink],
  templateUrl: './invoice-edit-page.component.html',
  styleUrl: './invoice-edit-page.component.scss'
})
export class InvoiceEditPageComponent implements OnInit {
  private readonly invoicesApi = inject(InvoicesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected invoice: InvoiceDto | null = null;
  protected invoiceId = '';
  protected issueDate = '';
  protected dueDate = '';
  protected notes = '';
  protected lines: LineDraft[] = [];

  protected isLoading = false;
  protected isSaving = false;
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

  private load(id: string): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.invoicesApi
      .getById(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (inv) => {
          this.invoice = inv;
          this.issueDate = isoToDateInput(inv.issueDate);
          this.dueDate = isoToDateInput(inv.dueDate);
          this.notes = inv.notes ?? '';
          this.lines =
            inv.items.length > 0
              ? inv.items.map((i) => ({
                  description: i.description,
                  productCode: i.productCode ?? '',
                  quantity: i.quantity,
                  unit: i.unit,
                  unitPrice: i.unitPrice
                }))
              : [{ description: '', productCode: '', quantity: 1, unit: 'ea', unitPrice: 0 }];
        },
        error: () => {
          this.errorMessage = 'Unable to load invoice for editing.';
          this.invoice = null;
        }
      });
  }

  protected addLine(): void {
    this.lines.push({
      description: '',
      productCode: '',
      quantity: 1,
      unit: 'ea',
      unitPrice: 0
    });
  }

  protected removeLine(index: number): void {
    if (this.lines.length > 1) {
      this.lines.splice(index, 1);
    }
  }

  protected submit(): void {
    this.errorMessage = '';
    if (!this.issueDate || !this.dueDate) {
      this.errorMessage = 'Issue and due dates are required.';
      return;
    }

    const items: CreateInvoiceItemRequest[] = this.lines
      .filter((l) => l.description.trim().length > 0)
      .map((l) => ({
        description: l.description.trim(),
        productCode: l.productCode.trim() || null,
        quantity: l.quantity,
        unit: l.unit.trim() || 'ea',
        unitPrice: l.unitPrice
      }));

    if (items.length === 0) {
      this.errorMessage = 'Add at least one line item with a description.';
      return;
    }

    const payload: UpdateInvoiceRequest = {
      issueDate: dateInputToIso(this.issueDate),
      dueDate: dateInputToIso(this.dueDate),
      notes: this.notes.trim() || null,
      items
    };

    this.isSaving = true;
    this.invoicesApi
      .update(this.invoiceId, payload)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (inv) => {
          void this.router.navigate(['/invoices', inv.id]);
        },
        error: () => {
          this.errorMessage =
            'Unable to update invoice. It may have payments recorded or the API rejected the payload.';
        }
      });
  }
}
