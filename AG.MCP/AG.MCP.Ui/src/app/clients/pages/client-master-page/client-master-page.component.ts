import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-client-master-page',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './client-master-page.component.html',
  styleUrl: './client-master-page.component.scss'
})
export class ClientMasterPageComponent {}
