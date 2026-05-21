import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ClientFormComponent } from '../../components/client-form/client-form.component';
import { ClientsApiService } from '../../clients-api.service';
import { ClientFormValue, CreateClientRequest } from '../../clients.models';

@Component({
  selector: 'app-client-create-page',
  imports: [ClientFormComponent, RouterLink],
  templateUrl: './client-create-page.component.html',
  styleUrl: './client-create-page.component.scss'
})
export class ClientCreatePageComponent {
  private readonly clientsApi = inject(ClientsApiService);
  private readonly router = inject(Router);

  protected isSaving = false;
  protected errorMessage = '';

  protected createClient(formValue: ClientFormValue): void {
    this.errorMessage = '';
    this.isSaving = true;

    const payload: CreateClientRequest = {
      name: formValue.name,
      taxId: formValue.taxId,
      email: formValue.email,
      phone: formValue.phone,
      address: formValue.address
    };

    this.clientsApi
      .createClient(payload)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: () => {
          void this.router.navigate(['/clients', 'list']);
        },
        error: () => {
          this.errorMessage =
            'Unable to create the client. Verify the form values and API availability.';
        }
      });
  }
}
