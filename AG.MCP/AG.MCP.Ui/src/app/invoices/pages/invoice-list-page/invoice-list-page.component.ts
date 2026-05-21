import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ClientsApiService } from '../../../clients/clients-api.service';
import { ClientDto } from '../../../clients/clients.models';
import { InvoicesApiService } from '../../invoices-api.service';
import { InvoiceDto } from '../../invoices.models';

@Component({
  selector: 'app-invoice-list-page',
  imports: [FormsModule, RouterLink, DatePipe, DecimalPipe],
  templateUrl: './invoice-list-page.component.html',
  styleUrl: './invoice-list-page.component.scss'
})
export class InvoiceListPageComponent implements OnInit {
  private readonly invoicesApi = inject(InvoicesApiService);
  private readonly clientsApi = inject(ClientsApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly mode =
    (this.route.snapshot.data['invoiceMode'] as 'all' | 'pending' | 'overdue') ?? 'all';

  protected invoices: InvoiceDto[] = [];
  protected clients: ClientDto[] = [];
  protected isLoading = false;
  protected errorMessage = '';

  protected page = 1;
  protected pageSize = 20;
  protected totalCount = 0;
  protected search = '';
  protected sortBy = 'issueDate';
  protected sortDescending = true;
  protected clientFilter: string | null = null;

  ngOnInit(): void {
    this.loadClientsForFilter();
    this.loadInvoices();
  }

  private loadClientsForFilter(): void {
    this.clientsApi.getClients({ page: 1, pageSize: 500, sortBy: 'name' }).subscribe({
      next: (r) => {
        this.clients = r.items;
      },
      error: () => {
        this.clients = [];
      }
    });
  }

  protected loadInvoices(): void {
    this.errorMessage = '';
    this.isLoading = true;

    const common = {
      page: this.page,
      pageSize: this.pageSize,
      search: this.search.trim() || null,
      sortBy: this.sortBy.trim() || null,
      sortDescending: this.sortDescending
    };

    const request$ =
      this.mode === 'pending'
        ? this.invoicesApi.getPending(common)
        : this.mode === 'overdue'
          ? this.invoicesApi.getOverdue(common)
          : this.invoicesApi.getPaged({
              ...common,
              clientId: this.clientFilter
            });

    request$.pipe(finalize(() => (this.isLoading = false))).subscribe({
      next: (response) => {
        this.invoices = response.items;
        this.totalCount = response.totalCount;
      },
      error: () => {
        this.errorMessage = 'Unable to load invoices. Check if AG.MCP.Api is running.';
      }
    });
  }

  protected applyFilters(): void {
    this.page = 1;
    this.loadInvoices();
  }

  protected goPrev(): void {
    if (this.page > 1) {
      this.page -= 1;
      this.loadInvoices();
    }
  }

  protected goNext(): void {
    const maxPage = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
    if (this.page < maxPage) {
      this.page += 1;
      this.loadInvoices();
    }
  }

  protected get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  protected get title(): string {
    if (this.mode === 'pending') {
      return 'Pending invoices';
    }
    if (this.mode === 'overdue') {
      return 'Overdue invoices';
    }
    return 'All invoices';
  }
}
