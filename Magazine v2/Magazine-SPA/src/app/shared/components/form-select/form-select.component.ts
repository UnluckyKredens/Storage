import { booleanAttribute, Component, forwardRef, Input } from '@angular/core';
import {
  ControlValueAccessor,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  ValidationErrors,
  Validator,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

export interface FormSelectOption {
  value: string | number | null;
  label: string;
}

@Component({
  selector: 'app-form-select',
  imports: [MatFormFieldModule, MatSelectModule],
  templateUrl: './form-select.component.html',
  styleUrl: './form-select.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FormSelectComponent),
      multi: true,
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => FormSelectComponent),
      multi: true,
    },
  ],
})
export class FormSelectComponent implements ControlValueAccessor, Validator {
  @Input() label = '';
  @Input() placeholder = 'Wybierz';
  @Input() emptyValue: string | number | null = '';
  @Input() options: FormSelectOption[] = [];
  @Input({ transform: booleanAttribute }) multiple = false;
  @Input({ transform: booleanAttribute }) required = false;
  @Input({ transform: booleanAttribute }) disabled = false;

  value: string | number | null | (string | number | null)[] = '';

  private onChange: (value: string | number | null | (string | number | null)[]) => void = () =>
    undefined;
  private onTouched: () => void = () => undefined;

  writeValue(value: string | number | null | (string | number | null)[]): void {
    this.value = value ?? (this.multiple ? [] : this.emptyValue);
  }

  registerOnChange(fn: (value: string | number | null | (string | number | null)[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.disabled = disabled;
  }

  changeValue(value: string | number | null | (string | number | null)[]): void {
    this.value = value;
    this.onChange(value);
  }

  touch(): void {
    this.onTouched();
  }

  validate(): ValidationErrors | null {
    if (this.disabled || !this.required) return null;
    if (Array.isArray(this.value)) return this.value.length ? null : { required: true };
    return this.value === null || this.value === '' ? { required: true } : null;
  }
}
