import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ClientsApiService } from '../../clients-api.service';
import { ClientSummaryDto } from '../../clients.models';

@Component({
  selector: 'app-client-summary-page',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './client-summary-page.component.html',
  styleUrl: './client-summary-page.component.scss'
})
export class ClientSummaryPageComponent implements OnInit {
  private readonly clientsApi = inject(ClientsApiService);
  private readonly route = inject(ActivatedRoute);

  protected summary: ClientSummaryDto | null = null;
  protected clientId = '';
  protected isLoading = false;
  protected errorMessage = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage = 'Client id is required.';
      return;
    }
    this.clientId = id;
    this.loadSummary(id);
  }

  protected loadSummary(id: string): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.clientsApi
      .getClientSummary(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (s) => {
          this.summary = s;
        },
        error: () => {
          this.errorMessage = 'Unable to load client summary. The client may not exist.';
          this.summary = null;
        }
      });
  }
}
