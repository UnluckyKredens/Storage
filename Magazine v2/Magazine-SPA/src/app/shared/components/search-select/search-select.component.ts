import { booleanAttribute, Component, forwardRef, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ControlValueAccessor,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  ValidationErrors,
  Validator,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FormSelectOption } from '../form-select/form-select.component';

@Component({
  selector: 'app-search-select',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchSelectComponent),
      multi: true,
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => SearchSelectComponent),
      multi: true,
    },
  ],
  templateUrl: './search-select.component.html',
  styleUrl: './search-select.component.scss',
})
export class SearchSelectComponent implements ControlValueAccessor, Validator {
  @Input() label = '';
  @Input() placeholder = 'Wybierz';
  @Input() searchLabel = 'Szukaj';
  @Input() options: FormSelectOption[] = [];
  @Input({ transform: booleanAttribute }) required = false;
  @Input({ transform: booleanAttribute }) disabled = false;

  value: string | number | null = '';
  search = '';

  private onChange: (value: string | number | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  get filteredOptions(): FormSelectOption[] {
    const phrase = this.search.trim().toLocaleLowerCase('pl');
    if (!phrase) return this.options;
    return this.options.filter((option) => option.label.toLocaleLowerCase('pl').includes(phrase));
  }

  writeValue(value: string | number | null): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string | number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.disabled = disabled;
  }

  changeValue(value: string | number | null): void {
    this.value = value;
    this.onChange(value);
  }

  openedChanged(opened: boolean): void {
    if (!opened) this.search = '';
  }

  touch(): void {
    this.onTouched();
  }

  validate(): ValidationErrors | null {
    if (this.disabled || !this.required) return null;
    return this.value === null || this.value === '' ? { required: true } : null;
  }
}
