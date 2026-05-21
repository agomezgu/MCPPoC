import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ClientsApiService } from '../../../clients/clients-api.service';
import { ClientDto } from '../../../clients/clients.models';
import { InvoicesApiService } from '../../invoices-api.service';
import { CreateInvoiceItemRequest, CreateInvoiceRequest } from '../../invoices.models';

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

@Component({
  selector: 'app-invoice-create-page',
  imports: [FormsModule, RouterLink],
  templateUrl: './invoice-create-page.component.html',
  styleUrl: './invoice-create-page.component.scss'
})
export class InvoiceCreatePageComponent implements OnInit {
  private readonly invoicesApi = inject(InvoicesApiService);
  private readonly clientsApi = inject(ClientsApiService);
  private readonly router = inject(Router);

  protected clients: ClientDto[] = [];
  protected clientId = '';
  protected issueDate = '';
  protected dueDate = '';
  protected notes = '';
  protected lines: LineDraft[] = [
    { description: '', productCode: '', quantity: 1, unit: 'ea', unitPrice: 0 }
  ];

  protected isLoadingClients = false;
  protected isSaving = false;
  protected errorMessage = '';

  ngOnInit(): void {
    this.isLoadingClients = true;
    this.clientsApi.getClients({ page: 1, pageSize: 500, sortBy: 'name' }).subscribe({
      next: (r) => {
        this.clients = r.items;
        this.isLoadingClients = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load clients.';
        this.isLoadingClients = false;
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
    if (!this.clientId) {
      this.errorMessage = 'Select a client.';
      return;
    }
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

    const payload: CreateInvoiceRequest = {
      clientId: this.clientId,
      issueDate: dateInputToIso(this.issueDate),
      dueDate: dateInputToIso(this.dueDate),
      notes: this.notes.trim() || null,
      items
    };

    this.isSaving = true;
    this.invoicesApi
      .create(payload)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (inv) => {
          void this.router.navigate(['/invoices', inv.id]);
        },
        error: () => {
          this.errorMessage = 'Unable to create invoice. Check values and API availability.';
        }
      });
  }
}
