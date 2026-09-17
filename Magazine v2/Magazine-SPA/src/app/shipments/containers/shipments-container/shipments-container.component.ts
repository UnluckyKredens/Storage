import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';

@Component({
  selector: 'app-shipments-container',
  imports: [RouterLink],
  templateUrl: './shipments-container.component.html',
  styleUrl: './shipments-container.component.scss',
})
export class ShipmentsContainerComponent implements OnInit {
  private readonly account = inject(AccountService);

  readonly sections = [
    {
      path: 'receive',
      title: 'Odbierz przesyłkę',
      description: 'Zeskanuj kod i sprawdź zawartość paczki.',
      permission: 'stock-documents.receive',
    },
    {
      path: 'create-shipment',
      title: 'Utwórz wysyłkę',
      description: 'Przygotuj wysyłkę WZ do innego oddziału.',
      permission: 'stock-shipments.create',
    },
    {
      path: 'receipts',
      title: 'Przyjęcia PZ',
      description: 'Przyjmowanie produktów od dostawców.',
      permission: 'stock-documents.read',
    },
    {
      path: 'issues',
      title: 'Wydania WZ',
      description: 'Wysyłanie produktów do odbiorców.',
      permission: 'stock-documents.read',
    },
    {
      path: 'history',
      title: 'Historia',
      description: 'Zatwierdzone dokumenty PZ i WZ.',
      permission: 'stock-documents.read',
    },
  ];

  ngOnInit(): void {
    this.account.load().subscribe();
    this.account.loadPermissions().subscribe();
  }

  get visibleSections() {
    if (this.account.user()?.roleId === administratorRoleId) return this.sections;
    return this.sections.filter((section) =>
      this.account.permissionCodes().includes(section.permission),
    );
  }
}
