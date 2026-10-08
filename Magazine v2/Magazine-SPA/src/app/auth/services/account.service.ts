import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { finalize, map, Observable, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AccountUser {
  id: string;
  login: string;
  firstName: string;
  lastName: string;
  email: string;
  roleId: string;
  roleName: string;
  warehouseId: string | null;
  warehouseName: string | null;
}

export interface WarehouseOption {
  id: string;
  name: string;
}

export interface AccountChanges {
  login: string;
  firstName: string;
  lastName: string;
  email: string;
  currentPassword: string | null;
  newPassword: string | null;
}

export interface UpdatedAccount {
  token: string;
  user: AccountUser;
}

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private userRequest: Observable<AccountUser> | null = null;
  private permissionsRequest: Observable<string[]> | null = null;
  private permissionsLoaded = false;
  readonly user = signal<AccountUser | null>(null);
  readonly permissionCodes = signal<string[]>([]);
  readonly warehouses = signal<WarehouseOption[]>([]);
  readonly activeWarehouseId = signal<string | null>(localStorage.getItem('activeWarehouseId'));
  readonly activeWarehouse = computed(
    () => this.warehouses().find((warehouse) => warehouse.id === this.activeWarehouseId()) ?? null,
  );

  load(): Observable<AccountUser> {
    const currentUser = this.user();
    if (currentUser) return of(currentUser);
    if (this.userRequest) return this.userRequest;

    this.userRequest = this.http.get<AccountUser>(`${environment.apiUrl}/Auth/me`).pipe(
      tap((user) => {
        this.user.set(user);
        if (user.warehouseId) this.setActiveWarehouse(user.warehouseId);
      }),
      finalize(() => (this.userRequest = null)),
      shareReplay(1),
    );
    return this.userRequest;
  }

  loadWarehouses(): Observable<WarehouseOption[]> {
    return this.http.get<WarehouseOption[]>(`${environment.apiUrl}/Warehouses`).pipe(
      tap((warehouses) => {
        this.warehouses.set(warehouses);
        const selectedExists = warehouses.some(({ id }) => id === this.activeWarehouseId());
        if (!selectedExists) this.setActiveWarehouse(warehouses[0]?.id ?? null);
      }),
    );
  }

  setActiveWarehouse(warehouseId: string | null): void {
    this.activeWarehouseId.set(warehouseId);
    if (warehouseId) localStorage.setItem('activeWarehouseId', warehouseId);
    else localStorage.removeItem('activeWarehouseId');
  }

  loadPermissions(): Observable<string[]> {
    if (this.permissionsLoaded) return of(this.permissionCodes());
    if (this.permissionsRequest) return this.permissionsRequest;

    this.permissionsRequest = this.http
      .get<{ permissionCodes: string[] }>(`${environment.apiUrl}/Auth/me/permissions`)
      .pipe(
        map((response) => response.permissionCodes),
        tap((codes) => {
          this.permissionCodes.set(codes);
          this.permissionsLoaded = true;
        }),
        finalize(() => (this.permissionsRequest = null)),
        shareReplay(1),
      );
    return this.permissionsRequest;
  }

  update(changes: AccountChanges): Observable<UpdatedAccount> {
    return this.http.put<UpdatedAccount>(`${environment.apiUrl}/Auth/me`, changes).pipe(
      tap((result) => {
        localStorage.setItem('token', result.token);
        this.user.set(result.user);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    this.user.set(null);
    this.permissionCodes.set([]);
    this.permissionsLoaded = false;
    this.userRequest = null;
    this.permissionsRequest = null;
    this.warehouses.set([]);
    this.setActiveWarehouse(null);
  }
}
