import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ClientsApiService } from '../../clients-api.service';
import { ClientFormComponent } from '../../components/client-form/client-form.component';
import { ClientDto, ClientFormValue, UpdateClientRequest } from '../../clients.models';

@Component({
  selector: 'app-client-update-page',
  imports: [ClientFormComponent, RouterLink],
  templateUrl: './client-update-page.component.html',
  styleUrl: './client-update-page.component.scss'
})
export class ClientUpdatePageComponent implements OnInit {
  private readonly clientsApi = inject(ClientsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected client: ClientDto | null = null;
  protected clientId = '';
  protected isLoading = false;
  protected isSaving = false;
  protected errorMessage = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.errorMessage = 'Client id is required.';
      return;
    }

    this.clientId = id;
    this.loadClient(id);
  }

  protected updateClient(formValue: ClientFormValue): void {
    this.errorMessage = '';
    this.isSaving = true;

    const payload: UpdateClientRequest = {
      name: formValue.name,
      taxId: formValue.taxId,
      email: formValue.email,
      phone: formValue.phone,
      address: formValue.address,
      isActive: formValue.isActive
    };

    this.clientsApi
      .updateClient(this.clientId, payload)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (client) => {
          this.client = client;
          void this.router.navigate(['/clients', 'list']);
        },
        error: () => {
          this.errorMessage =
            'Unable to update the client. Verify the form values and API availability.';
        }
      });
  }

  private loadClient(id: string): void {
    this.errorMessage = '';
    this.isLoading = true;

    this.clientsApi
      .getClient(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (client) => {
          this.client = client;
        },
        error: () => {
          this.errorMessage = 'Unable to load the client for editing.';
        }
      });
  }
}
