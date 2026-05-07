import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-payment-master-page',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './payment-master-page.component.html',
  styleUrl: './payment-master-page.component.scss'
})
export class PaymentMasterPageComponent {}
