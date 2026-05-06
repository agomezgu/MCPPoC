import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ClientDto, ClientFormValue } from '../../clients.models';

@Component({
  selector: 'app-client-form',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './client-form.component.html',
  styleUrl: './client-form.component.scss'
})
export class ClientFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);

  @Input() client: ClientDto | null = null;
  @Input() isSubmitting = false;
  @Input() submitLabel = 'Save client';
  @Input() submittingLabel = 'Saving...';
  @Input() showStatus = false;

  @Output() formSubmitted = new EventEmitter<ClientFormValue>();

  protected readonly clientForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    taxId: ['', [Validators.required, Validators.maxLength(40)]],
    email: ['', [Validators.email, Validators.maxLength(160)]],
    phone: ['', [Validators.maxLength(60)]],
    address: ['', [Validators.maxLength(240)]],
    isActive: [true]
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['client']) {
      this.clientForm.reset({
        name: this.client?.name ?? '',
        taxId: this.client?.taxId ?? '',
        email: this.client?.email ?? '',
        phone: this.client?.phone ?? '',
        address: this.client?.address ?? '',
        isActive: this.client?.isActive ?? true
      });
    }
  }

  protected submit(): void {
    if (this.clientForm.invalid) {
      this.clientForm.markAllAsTouched();
      return;
    }

    this.formSubmitted.emit(this.clientForm.getRawValue());
  }

  protected fieldHasError(fieldName: string): boolean {
    const field = this.clientForm.get(fieldName);
    return !!field && field.invalid && (field.dirty || field.touched);
  }
}
