import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ClientsApiService } from '../../../clients/clients-api.service';
import { ClientDto } from '../../../clients/clients.models';
import { ReportsApiService } from '../../reports-api.service';
import { ClientStatementDto } from '../../reports.models';

function dateToParam(d: string): string | null {
  return d?.length ? new Date(`${d}T12:00:00`).toISOString() : null;
}

@Component({
  selector: 'app-client-statement-page',
  imports: [FormsModule, DatePipe, DecimalPipe],
  templateUrl: './client-statement-page.component.html',
  styleUrl: './client-statement-page.component.scss'
})
export class ClientStatementPageComponent implements OnInit {
  private readonly reportsApi = inject(ReportsApiService);
  private readonly clientsApi = inject(ClientsApiService);

  protected clients: ClientDto[] = [];
  protected clientId = '';
  protected fromDate = '';
  protected toDate = '';
  protected data: ClientStatementDto | null = null;
  protected isLoadingClients = false;
  protected isLoading = false;
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

  protected load(): void {
    this.errorMessage = '';
    if (!this.clientId) {
      this.errorMessage = 'Select a client.';
      return;
    }
    this.isLoading = true;
    this.reportsApi
      .getClientStatement(this.clientId, dateToParam(this.fromDate), dateToParam(this.toDate))
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (d) => {
          this.data = d;
        },
        error: () => {
          this.errorMessage = 'Unable to load statement. The client may not exist.';
          this.data = null;
        }
      });
  }
}
