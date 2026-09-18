import { ChangeDetectorRef, Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
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

  nameError = false;
  ngOnInit(): void {
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

  get canViewShipments(): boolean {
    return (
      this.isAdministrator ||
      this.account.permissionCodes().includes('stock-documents.read') ||
      this.account.permissionCodes().includes('stock-documents.receive') ||
      this.account.permissionCodes().includes('stock-shipments.create')
    );
  }

  get warehouseOptions(): { value: string; label: string }[] {
    return this.account.warehouses().map((warehouse) => ({
      value: warehouse.id,
      label: warehouse.name,
    }));
  }

  selectWarehouse(warehouseId: string): void {
    if (warehouseId !== '__search__') this.account.setActiveWarehouse(warehouseId);
  }

  logout(): void {
    this.account.logout();
    void this.router.navigateByUrl('/auth');
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
