import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { PaymentsApiService } from '../../payments-api.service';
import { PaymentDto } from '../../payments.models';

@Component({
  selector: 'app-payment-detail-page',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './payment-detail-page.component.html',
  styleUrl: './payment-detail-page.component.scss'
})
export class PaymentDetailPageComponent implements OnInit {
  private readonly paymentsApi = inject(PaymentsApiService);
  private readonly route = inject(ActivatedRoute);

  protected payment: PaymentDto | null = null;
  protected paymentId = '';
  protected isLoading = false;
  protected errorMessage = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage = 'Payment id is required.';
      return;
    }
    this.paymentId = id;
    this.load(id);
  }

  private load(id: string): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.paymentsApi
      .getById(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (p) => {
          this.payment = p;
        },
        error: () => {
          this.errorMessage = 'Payment not found or unable to load.';
          this.payment = null;
        }
      });
  }
}
