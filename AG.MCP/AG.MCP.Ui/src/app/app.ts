import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly router = inject(Router);

  protected readonly showAppShell = signal(true);

  constructor() {
    this.updateShellVisibility();

    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd), takeUntilDestroyed())
      .subscribe(() => this.updateShellVisibility());
  }

  private updateShellVisibility(): void {
    let route = this.router.routerState.snapshot.root;
    let hideAppShell = false;

    while (route) {
      hideAppShell = hideAppShell || route.data['hideAppShell'] === true;
      route = route.firstChild!;
    }

    this.showAppShell.set(!hideAppShell);
  }
}
