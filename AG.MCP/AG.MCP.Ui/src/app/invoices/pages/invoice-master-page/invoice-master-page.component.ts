import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-invoice-master-page',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './invoice-master-page.component.html',
  styleUrl: './invoice-master-page.component.scss'
})
export class InvoiceMasterPageComponent {}
