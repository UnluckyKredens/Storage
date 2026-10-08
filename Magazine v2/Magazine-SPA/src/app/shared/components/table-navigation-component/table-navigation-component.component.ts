import { Location } from '@angular/common';
import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Router } from '@angular/router';

@Component({
  selector: 'app-table-navigation-component',
  imports: [MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule],
  templateUrl: './table-navigation-component.component.html',
  styleUrl: './table-navigation-component.component.scss',
})
export class TableNavigationComponent {
  @Input({ required: true }) title = '';
  @Input() searchLabel = 'Szukaj';
  @Input() searchPlaceholder = '';
  @Input() searchValue = '';
  @Input() addLabel = 'Dodaj';
  @Input() backLink: string | null = null;
  @Input() showAddButton = true;

  @Output() readonly searchSubmit = new EventEmitter<string>();
  @Output() readonly add = new EventEmitter<void>();

  private readonly location = inject(Location);
  private readonly router = inject(Router, { optional: true });

  goBack() {
    if (this.backLink && this.router) {
      void this.router.navigateByUrl(this.backLink);
      return;
    }

    this.location.back();
  }

  submitSearch(value: string) {
    this.searchSubmit.emit(value.trim());
  }
}
