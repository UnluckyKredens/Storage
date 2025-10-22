import {
  ChangeDetectorRef,
  Component,
  DestroyRef,
  ElementRef,
  inject,
  OnInit,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { administratorRoleId } from '../../auth/roles';
import { AccountService } from '../../auth/services/account.service';
import { SearchSelectComponent } from '../../../shared/components/search-select/search-select.component';

@Component({
  selector: 'app-navigation-bar',
  standalone: true,
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    RouterLink,
    RouterLinkActive,
    SearchSelectComponent,
  ],
  templateUrl: './navigation-bar.component.html',
  styleUrl: './navigation-bar.component.scss',
})
export class NavigationBarComponent implements OnInit {
  readonly account = inject(AccountService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly links = viewChild<ElementRef<HTMLElement>>('links');

  nameError = false;
  ngOnInit(): void {
    // Na wąskim ekranie menu jest poziomym paskiem: po zmianie strony przewijamy go do aktywnej zakładki.
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => setTimeout(() => this.revealActiveLink()));
    // Pierwsza nawigacja kończy się, zanim pasek powstanie, więc przy starcie przewijamy osobno.
    setTimeout(() => this.revealActiveLink());
    this.account.loadPermissions().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    this.account
      .load()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          if (user.roleId === administratorRoleId) {
            this.loadWarehouses();
          }
        },
        error: () => {
          this.nameError = true;
          this.cdr.markForCheck();
        },
      });
  }

  get isAdministrator(): boolean {
    return this.account.user()?.roleId === administratorRoleId;
  }

  get warehouseOptions(): { value: string; label: string }[] {
    return this.account.warehouses().map((warehouse) => ({
      value: warehouse.id,
      label: warehouse.name,
    }));
  }

  selectWarehouse(warehouseId: string): void {
    if (warehouseId !== '__search__' && warehouseId !== this.account.activeWarehouseId()) {
      this.dialog.closeAll();
      this.account.setActiveWarehouse(warehouseId);
    }
  }

  logout(): void {
    this.account.logout();
    void this.router.navigateByUrl('/auth');
  }

  private revealActiveLink(): void {
    const nav = this.links()?.nativeElement;
    const active = nav?.querySelector<HTMLElement>('a.active');
    if (!nav || !active || nav.scrollWidth <= nav.clientWidth) return;
    nav.scrollTo({ left: active.offsetLeft - nav.offsetLeft - 12, behavior: 'smooth' });
  }

  private loadWarehouses(): void {
    this.account
      .loadWarehouses()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: () => this.cdr.markForCheck(),
      });
  }
}
