import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-report-master-page',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './report-master-page.component.html',
  styleUrl: './report-master-page.component.scss'
})
export class ReportMasterPageComponent {}
