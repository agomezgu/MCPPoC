import { Component, OnInit, inject } from '@angular/core';
import { finalize } from 'rxjs';
import { ClientsApiService } from '../../clients-api.service';
import { ClientDto } from '../../clients.models';

@Component({
  selector: 'app-client-list-page',
  templateUrl: './client-list-page.component.html',
  styleUrl: './client-list-page.component.scss'
})
export class ClientListPageComponent implements OnInit {
  private readonly clientsApi = inject(ClientsApiService);

  protected clients: ClientDto[] = [];
  protected isLoading = false;
  protected errorMessage = '';

  ngOnInit(): void {
    this.loadClients();
  }

  protected loadClients(): void {
    this.errorMessage = '';
    this.isLoading = true;

    this.clientsApi
      .getClients()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (response) => {
          this.clients = response.items;
        },
        error: () => {
          this.errorMessage =
            'Unable to load clients. Check if AG.MCP.Api is running.';
        }
      });
  }
}
