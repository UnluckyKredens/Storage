import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { Product } from '../../models/management.models';
import { ProductModalComponent } from '../../modals/product/product-modal.component';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-products-page',
  imports: [
    CurrencyPipe,
    MatButtonModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './products-page.component.html',
  styleUrl: '../management-page.scss',
})
export class ProductsPageComponent implements OnInit {
  private readonly service = inject(ProductService);
  private readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  readonly displayedColumns = [
    'name',
    'sku',
    'category',
    'unitOfMeasure',
    'salePrice',
    'isActive',
    'actions',
  ];
  products = signal<Product[]>([]);
  searchValue = '';
  sortBy = 'name';
  order: 'asc' | 'desc' = 'asc';
  pageIndex = 0;
  pageSize = 10;
  total = 0;
  loading = false;

  ngOnInit(): void {
    this.account.load().subscribe();
    this.account.loadPermissions().subscribe();
    this.load();
  }

  get canManage(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('products.manage')
    );
  }

  load(): void {
    this.loading = true;
    this.service
      .getAll(this.pageIndex + 1, this.pageSize, this.searchValue, this.sortBy, this.order)
      .subscribe({
        next: (page) => {
          this.products.set(page.list);
          this.total = page.total;
          this.loading = false;
        },
        error: (error: HttpErrorResponse) => this.showError(error),
      });
  }

  search(value: string): void {
    this.searchValue = value;
    this.pageIndex = 0;
    this.load();
  }

  sortChanged(sort: Sort): void {
    this.sortBy = sort.active;
    this.order = sort.direction || 'asc';
    this.pageIndex = 0;
    this.load();
  }

  pageChanged(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  showDetails(product: Product): void {
    this.service.getById(product.productId).subscribe({
      next: (details) => this.openModal('details', details),
      error: (error: HttpErrorResponse) => this.showError(error),
    });
  }

  add(): void {
    if (!this.canManage) return;
    this.openModal('form', null)
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.notification.success('Produkt dodany.');
        this.load();
      });
  }

  edit(product: Product): void {
    if (!this.canManage) return;
    this.openModal('form', product)
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.notification.success('Zmiany zapisane.');
        this.load();
      });
  }

  remove(product: Product): void {
    if (!this.canManage) return;
    this.openModal('delete', product)
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(product.productId).subscribe({
          next: () => {
            this.notification.success('Produkt usunięty.');
            this.load();
          },
          error: (error: HttpErrorResponse) => this.showError(error),
        });
      });
  }

  private openModal(mode: 'details' | 'form' | 'delete', row: Product | null) {
    let width = '720px';
    if (mode === 'details') width = '640px';
    if (mode === 'delete') width = '480px';

    return this.dialog.open(ProductModalComponent, {
      width,
      maxWidth: '95vw',
      data: { mode, row },
    });
  }

  private showError(error: HttpErrorResponse): void {
    this.notification.error(error.error?.message ?? 'Nie udało się pobrać produktów.');
    this.loading = false;
  }
}
