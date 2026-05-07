import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ClientsApiService } from '../../clients-api.service';
import { ClientDto } from '../../clients.models';

@Component({
  selector: 'app-client-list-page',
  imports: [FormsModule, RouterLink],
  templateUrl: './client-list-page.component.html',
  styleUrl: './client-list-page.component.scss'
})
export class ClientListPageComponent implements OnInit {
  private readonly clientsApi = inject(ClientsApiService);

  protected clients: ClientDto[] = [];
  protected isLoading = false;
  protected errorMessage = '';

  protected page = 1;
  protected pageSize = 20;
  protected totalCount = 0;
  protected search = '';
  protected sortBy = 'name';
  protected sortDescending = false;

  ngOnInit(): void {
    this.loadClients();
  }

  protected loadClients(): void {
    this.errorMessage = '';
    this.isLoading = true;

    this.clientsApi
      .getClients({
        page: this.page,
        pageSize: this.pageSize,
        search: this.search.trim() || null,
        sortBy: this.sortBy.trim() || null,
        sortDescending: this.sortDescending
      })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (response) => {
          this.clients = response.items;
          this.totalCount = response.totalCount;
        },
        error: () => {
          this.errorMessage =
            'Unable to load clients. Check if AG.MCP.Api is running.';
        }
      });
  }

  protected applyFilters(): void {
    this.page = 1;
    this.loadClients();
  }

  protected goPrev(): void {
    if (this.page > 1) {
      this.page -= 1;
      this.loadClients();
    }
  }

  protected goNext(): void {
    const maxPage = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
    if (this.page < maxPage) {
      this.page += 1;
      this.loadClients();
    }
  }

  protected get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }
}
