import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ClientsApiService } from '../../clients-api.service';
import { CreateClientRequest } from '../../clients.models';

@Component({
  selector: 'app-client-agent-create-page',
  imports: [ReactiveFormsModule],
  templateUrl: './client-agent-create-page.component.html',
  styleUrl: './client-agent-create-page.component.scss'
})
export class ClientAgentCreatePageComponent {
  private readonly clientsApi = inject(ClientsApiService);
  private readonly fb = inject(FormBuilder);

  protected isSaving = false;
  protected errorMessage = '';
  protected successMessage = '';

  protected readonly clientForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    taxId: ['', [Validators.required, Validators.maxLength(40)]],
    email: ['', [Validators.email, Validators.maxLength(160)]]
  });

  protected createClient(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (this.clientForm.invalid) {
      this.clientForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.clientForm.getRawValue();
    const payload: CreateClientRequest = {
      name: formValue.name,
      taxId: formValue.taxId,
      email: formValue.email,
      phone: null,
      address: null
    };

    this.clientsApi
      .createClient(payload)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (client) => {
          this.successMessage = `Client "${client.name}" created.`;
          this.clientForm.reset({
            name: '',
            taxId: '',
            email: ''
          });
        },
        error: () => {
          this.errorMessage =
            'Unable to create the client. Review the fields and try again.';
        }
      });
  }

  protected fieldHasError(fieldName: string): boolean {
    const field = this.clientForm.get(fieldName);
    return !!field && field.invalid && (field.dirty || field.touched);
  }
}
