import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize } from 'rxjs';
import { ClientsApiService } from './clients-api.service';
import { ClientDto } from './clients.models';

@Component({
  selector: 'app-clients-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './clients-page.component.html',
  styleUrl: './clients-page.component.scss'
})
export class ClientsPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly clientsApi = inject(ClientsApiService);

  protected clients: ClientDto[] = [];
  protected isLoading = false;
  protected isSaving = false;
  protected errorMessage = '';
  protected successMessage = '';

  protected readonly createClientForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    taxId: ['', [Validators.required, Validators.maxLength(40)]],
    email: ['', [Validators.email, Validators.maxLength(160)]],
    phone: ['', [Validators.maxLength(60)]],
    address: ['', [Validators.maxLength(240)]]
  });

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

  protected submitCreateClient(): void {
    this.successMessage = '';
    this.errorMessage = '';

    if (this.createClientForm.invalid) {
      this.createClientForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    this.clientsApi
      .createClient(this.createClientForm.getRawValue())
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (client) => {
          this.successMessage = `Client "${client.name}" created successfully.`;
          this.createClientForm.reset({
            name: '',
            taxId: '',
            email: '',
            phone: '',
            address: ''
          });
          this.loadClients();
        },
        error: () => {
          this.errorMessage =
            'Unable to create the client. Verify the form values and API availability.';
        }
      });
  }

  protected fieldHasError(fieldName: string): boolean {
    const field = this.createClientForm.get(fieldName);
    return !!field && field.invalid && (field.dirty || field.touched);
  }
}
